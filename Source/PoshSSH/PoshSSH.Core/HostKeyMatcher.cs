using SSH.Stores;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SSH
{
    /// <summary>
    /// Decides whether a host key presented by a server is covered by the entries recorded for
    /// that host in a trusted host store.
    ///
    /// This is kept separate from the connection scaffolding so the rule can be exercised
    /// directly, without a server. It is the check that decides whether to trust a host, so it
    /// is worth being able to test in isolation.
    /// </summary>
    public static class HostKeyMatcher
    {
        /// <summary>
        /// Host key algorithm names that share the "ssh-rsa" key format.
        ///
        /// RFC 8332 section 3 defines rsa-sha2-256 and rsa-sha2-512 as public key *signature*
        /// algorithms over the existing "ssh-rsa" key format, explicitly keeping the encoded
        /// key, and therefore its fingerprint, unchanged. A known_hosts file records the key
        /// format rather than the signature algorithm, so an ssh-rsa entry is the correct and
        /// only entry for a host that negotiates rsa-sha2-256 or rsa-sha2-512.
        /// </summary>
        private static readonly string[] RsaHostKeyNames = { "ssh-rsa", "rsa-sha2-256", "rsa-sha2-512" };

        /// <summary>
        /// Whether a stored host key name covers the algorithm a server presented.
        /// An empty stored name means any key type. It does not relax the fingerprint check;
        /// see <see cref="IsTrusted"/>.
        /// </summary>
        public static bool NameMatches(string storedName, string presentedName)
        {
            if (string.IsNullOrEmpty(storedName))
            {
                return true;
            }
            if (string.Equals(storedName, presentedName, StringComparison.Ordinal))
            {
                return true;
            }
            return RsaHostKeyNames.Contains(storedName) && RsaHostKeyNames.Contains(presentedName);
        }

        /// <summary>
        /// Whether any stored entry trusts the presented host key.
        ///
        /// The fingerprint must always match. An entry only ever widens which key *types* are
        /// acceptable, never which keys.
        /// </summary>
        /// <param name="storedKeys">Entries recorded for the host.</param>
        /// <param name="presentedName">Host key algorithm the server negotiated.</param>
        /// <param name="sha256Fingerprint">SHA256 fingerprint of the presented key.</param>
        /// <param name="legacyFingerprint">Colon separated MD5 fingerprint, as written by older versions.</param>
        public static bool IsTrusted(IEnumerable<TrustedHostValue> storedKeys, string presentedName,
            string sha256Fingerprint, string legacyFingerprint)
        {
            if (storedKeys == null)
            {
                return false;
            }

            return storedKeys.Any(stored =>
                stored != null &&
                FingerprintMatches(stored.Fingerprint, sha256Fingerprint, legacyFingerprint) &&
                NameMatches(stored.HostKeyName, presentedName));
        }

        private static bool FingerprintMatches(string storedFingerprint, string sha256Fingerprint, string legacyFingerprint)
        {
            if (string.IsNullOrEmpty(storedFingerprint))
            {
                return false;
            }
            return string.Equals(storedFingerprint, sha256Fingerprint, StringComparison.Ordinal)
                || string.Equals(storedFingerprint, legacyFingerprint, StringComparison.Ordinal);
        }
    }
}
