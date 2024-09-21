using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace segugio
{
    public class UpdateSignal
    {
        public enum UpdateAction
        {
            Idle,//only first time, after first process monitoring loop
            ProcessScanStarted, //when scan has started
            CreatedProcesses, //when new process is created
            TerminatedProcesses, //when process has terminated
            UpdatedProcess,  // when properties changes on yara or Config extraction
            NewLogMessage //notify a new log message to the GUI
        }

        public UpdateAction Action { get; set; }

        public string LogMessage { get; set; }

        public uint StartingPIDtoMonitor { get; set; } //ProcessToMonitorHasBeenStarted

        public ConcurrentDictionary<uint, ProcessInfoTracked> NewProcessesList { get; set; }
    }
}
