#!/usr/bin/env bash
#
# Setup-LinuxTestVm.sh
#
# Provisions a Linux VM with a matrix of SSH test accounts and key combinations
# for exercising the Posh-SSH PowerShell module's New-SSHSession / New-SFTPSession
# authentication paths.
#
# Run as root on a disposable test VM. Idempotent: re-running deletes prior test
# users and re-provisions cleanly.
#
# Output: /root/posh-ssh-test/
#   keys/                  private + public keys for every account
#   credentials.txt        manifest with usernames, passwords, key paths, auth notes
#   teardown.sh            companion cleanup script
#   posh-ssh-test-bundle.tar.gz   tarball of keys+manifest for easy scp

set -euo pipefail

if [[ $EUID -ne 0 ]]; then
    echo "ERROR: must run as root" >&2
    exit 1
fi

for tool in useradd userdel chpasswd ssh-keygen sshd openssl; do
    command -v "$tool" >/dev/null || { echo "ERROR: missing $tool" >&2; exit 1; }
done

SSHD_SVC="sshd"
if systemctl list-unit-files 2>/dev/null | grep -q '^ssh\.service'; then
    SSHD_SVC="ssh"
fi

OUT=/root/posh-ssh-test
KEYS=$OUT/keys
MANIFEST=$OUT/credentials.txt
TEARDOWN=$OUT/teardown.sh
SSHD_DROPIN=/etc/ssh/sshd_config.d/99-posh-ssh-test.conf

mkdir -p "$KEYS"
: > "$MANIFEST"

echo "# Posh-SSH test account manifest"          >> "$MANIFEST"
echo "# generated $(date -Iseconds) on $(hostname)" >> "$MANIFEST"
echo                                              >> "$MANIFEST"

# Each row: USER  KEY_TYPE  KEY_BITS  PASSPHRASE  PEM_FORMAT  AUTH_NOTE
ACCOUNTS=(
    "pshpass        none      -     -                       -        Password authentication only"
    "pshrsa2048     rsa       2048  -                       openssh  RSA 2048 key, no passphrase"
    "pshrsa4096     rsa       4096  -                       openssh  RSA 4096 key, no passphrase"
    "pshrsaenc      rsa       2048  TestPassPhrase-RSA-1234 openssh  RSA 2048 key, passphrase-protected"
    "pshrsapem      rsa       2048  -                       pem      RSA 2048 key in classic PKCS#1 PEM format"
    "pshed25519     ed25519   -     -                       openssh  Ed25519 key, no passphrase"
    "pshed25519e    ed25519   -     TestPassPhrase-Ed-5678  openssh  Ed25519 key, passphrase-protected"
    "pshecdsa256    ecdsa     256   -                       openssh  ECDSA P-256 key, no passphrase"
    "pshecdsa384    ecdsa     384   -                       openssh  ECDSA P-384 key, no passphrase"
    "pshecdsa521    ecdsa     521   -                       openssh  ECDSA P-521 key, no passphrase"
    "pshmulti       ed25519   -     -                       openssh  Multi-factor: requires BOTH publickey AND password"
    "pshkbi         none      -     -                       -        Forces keyboard-interactive authentication"
)

random_password() {
    openssl rand -base64 24 | tr -dc 'A-Za-z0-9' | head -c 16
    echo
}

create_user() {
    local user=$1
    if id -u "$user" >/dev/null 2>&1; then
        userdel -rf "$user" 2>/dev/null || true
    fi
    useradd -m -s /bin/bash "$user"
}

set_password() {
    local user=$1 pw=$2
    echo "${user}:${pw}" | chpasswd
}

provision_authorized_keys() {
    local user=$1 pubkey_path=$2
    local home
    home=$(getent passwd "$user" | cut -d: -f6)
    install -d -m 700 -o "$user" -g "$user" "$home/.ssh"
    install -m 600 -o "$user" -g "$user" "$pubkey_path" "$home/.ssh/authorized_keys"
}

echo ">> provisioning ${#ACCOUNTS[@]} test accounts..."
for row in "${ACCOUNTS[@]}"; do
    # shellcheck disable=SC2086
    read -r USER KEY_TYPE KEY_BITS PASSPHRASE PEM AUTH_NOTE <<< "$row"
    AUTH_NOTE_FULL=$(echo "$row" | awk '{for(i=6;i<=NF;i++)printf "%s ",$i; print ""}')

    echo "  - $USER"
    create_user "$USER"

    PASSWORD=$(random_password)
    set_password "$USER" "$PASSWORD"

    PRIV=""
    PUB=""
    if [[ "$KEY_TYPE" != "none" ]]; then
        PRIV="$KEYS/${USER}.priv"
        PUB="${PRIV}.pub"
        rm -f "$PRIV" "$PUB"

        KEYGEN_ARGS=(-t "$KEY_TYPE" -f "$PRIV" -q -C "posh-ssh-test/${USER}")
        [[ "$KEY_BITS" != "-" ]] && KEYGEN_ARGS+=(-b "$KEY_BITS")
        if [[ "$PASSPHRASE" == "-" ]]; then
            KEYGEN_ARGS+=(-N "")
        else
            KEYGEN_ARGS+=(-N "$PASSPHRASE")
        fi
        [[ "$PEM" == "pem" ]] && KEYGEN_ARGS+=(-m PEM)

        ssh-keygen "${KEYGEN_ARGS[@]}"
        provision_authorized_keys "$USER" "$PUB"
    fi

    {
        echo "[$USER]"
        echo "  password:    $PASSWORD"
        if [[ -n "$PRIV" ]]; then
            echo "  private_key: keys/${USER}.priv"
            echo "  public_key:  keys/${USER}.priv.pub"
            [[ "$PASSPHRASE" != "-" ]] && echo "  passphrase:  $PASSPHRASE"
            echo "  key_format:  $PEM"
        fi
        echo "  notes:       $AUTH_NOTE_FULL"
        echo
    } >> "$MANIFEST"
done

echo ">> writing $SSHD_DROPIN"

if ! grep -qE '^\s*Include\s+/etc/ssh/sshd_config\.d/' /etc/ssh/sshd_config 2>/dev/null; then
    echo "  WARNING: /etc/ssh/sshd_config has no Include directive — appending one"
    echo "Include /etc/ssh/sshd_config.d/*.conf" >> /etc/ssh/sshd_config
fi

cat > "$SSHD_DROPIN" <<'EOF'
# Drop-in for Posh-SSH test accounts. Managed by tests/Setup-LinuxTestVm.sh.

PasswordAuthentication       yes
PubkeyAuthentication         yes
KbdInteractiveAuthentication yes
PermitEmptyPasswords         no

# Multi-factor: pshmulti must present BOTH a valid public key AND a password.
Match User pshmulti
    AuthenticationMethods publickey,password

# Force keyboard-interactive for pshkbi.
Match User pshkbi
    AuthenticationMethods keyboard-interactive
    PasswordAuthentication no
EOF

chmod 644 "$SSHD_DROPIN"

echo ">> validating sshd_config"
sshd -t

echo ">> reloading $SSHD_SVC"
systemctl reload "$SSHD_SVC" || systemctl restart "$SSHD_SVC"

echo ">> bundling artifacts"
chmod 600 "$KEYS"/*.priv 2>/dev/null || true
tar -C "$OUT" -czf "$OUT/posh-ssh-test-bundle.tar.gz" keys credentials.txt
chmod 600 "$OUT/posh-ssh-test-bundle.tar.gz"

cat > "$TEARDOWN" <<'EOF'
#!/usr/bin/env bash
set -euo pipefail
[[ $EUID -ne 0 ]] && { echo "ERROR: run as root" >&2; exit 1; }

USERS=(pshpass pshrsa2048 pshrsa4096 pshrsaenc pshrsapem pshed25519
       pshed25519e pshecdsa256 pshecdsa384 pshecdsa521 pshmulti pshkbi)

for u in "${USERS[@]}"; do
    id -u "$u" >/dev/null 2>&1 && userdel -rf "$u" 2>/dev/null && echo "removed user $u"
done

rm -f /etc/ssh/sshd_config.d/99-posh-ssh-test.conf
rm -rf /root/posh-ssh-test

SVC=sshd
systemctl list-unit-files 2>/dev/null | grep -q '^ssh\.service' && SVC=ssh
systemctl reload "$SVC" || systemctl restart "$SVC"

echo "teardown complete"
EOF
chmod 700 "$TEARDOWN"

cat <<EOF

============================================================
  Posh-SSH test VM provisioning complete
============================================================

  Manifest:    $MANIFEST
  Keys:        $KEYS/
  Bundle:      $OUT/posh-ssh-test-bundle.tar.gz
  Teardown:    $TEARDOWN

  Download the bundle:
      scp root@$(hostname -I 2>/dev/null | awk '{print $1}'):$OUT/posh-ssh-test-bundle.tar.gz .

  Then drive New-SSHSession against each account using the manifest.
  Companion: tests/Posh-SSH.Integration.Tests.ps1 (from PR #628)

  To wipe everything:  sudo $TEARDOWN

EOF
