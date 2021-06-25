using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Text;
using System.Threading.Tasks;

namespace SSH
{
    internal class OperationProgressHelper
    {
        public Action<ulong> Callback { get; }
        public Action Complete { get; }

        public OperationProgressHelper(PSCmdlet cmdlet, string Operation, string FileName, long FileLength, int ActivityId = 1, int ParentActivityId = -1)
        {
            var activity = Operation + "ing " + FileName;
            var operation = Operation.ToLower();
            int prev_percent = 0;
            DateTime start_time = DateTime.Now;
            var progressRecord = new ProgressRecord(ActivityId, activity, " ")
            {
                PercentComplete = 0,
                SecondsRemaining = -1,
                ParentActivityId = ParentActivityId,
            };
            if (cmdlet.SessionState.PSVariable.GetValue("ProgressPreference", "Continue").ToString().Equals("SilentlyContinue", StringComparison.OrdinalIgnoreCase))
            {
                Callback = null;
                Complete = () => {};
            }
            else
            {
                Callback = new Action<ulong>(bytes =>
                {
                    var percent = (int)((double)bytes / FileLength * 100);
                    var time = DateTime.Now;
                    var time_passed_total = (time - start_time).TotalSeconds;
                    // change progress only if it really changed and the process lasted more than 1 second
                    if (prev_percent != percent && time_passed_total > 1)
                    {
                        // average speed
                        var speed = bytes / time_passed_total / 1024;
                        // instant speed
                        //var speed = ((double)bytes - prev_bytes) / (time - prev_time).TotalSeconds / 1024;
                        var remain = ((double)FileLength - bytes) / bytes * time_passed_total;

                        progressRecord.PercentComplete = percent;
                        progressRecord.SecondsRemaining = (int)remain;
                        progressRecord.StatusDescription = string.Format("{0} Bytes {1}ed of {2}, speed {3:f3} Kb/sec", bytes, operation, FileLength, speed);
                        cmdlet.Host.UI.WriteProgress(1, progressRecord);
                    }
                    prev_percent = percent;
                });
                Complete = () =>
                {
                    progressRecord.PercentComplete = 100;
                    progressRecord.RecordType = ProgressRecordType.Completed;
                    cmdlet.Host.UI.WriteProgress(1, progressRecord);
                };
            }
        }        
    }
}
