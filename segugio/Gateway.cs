using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace segugio
{

    public class Gateway
    {

        public Thread threadProcessMonitor, threadProcessLaunch, threadProcessScan;

        public ManualResetEvent startThreadProcessLaunch = new ManualResetEvent(false);
        public AutoResetEvent startThreadProcessScan = new AutoResetEvent(false);
        public ManualResetEventSlim pauseEvent = new ManualResetEventSlim(true);

        public MainForm mainForm;

        //ProcessMonitor handle for starting process monitoring
        public ProcessMonitor pm;
        //ProcessListManager handle for updating views
        public ProcessListManager plm;
        //ProcessScan handle
        public ProcessScan ps;
        //process launcher handle
        public ProcessLaunch pl;
        //utility cross Class
        public Utils ut;

        //Settings
        public SettingsManager settings;
        public YaraRules yaras;
        public ConfigExtraction mlwConfigEx;
        //Get Segugio process PID
        public uint thisCurrentProcessPid; 
        public string applicationName;

        //current program directory
        public string directoryPath;
        public string desktopPath;
        


        public Gateway()
        {
            thisCurrentProcessPid = (uint) Process.GetCurrentProcess().Id;
            string executablePath = Assembly.GetExecutingAssembly().Location;
            //desktop path for dumps
            desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            directoryPath = Path.GetDirectoryName(executablePath);
            applicationName = System.IO.Path.GetFileName(executablePath);



            this.ut = new Utils(this);

            this.settings = new SettingsManager(this);

            this.mainForm = new MainForm(this);

            this.yaras = new YaraRules(this);

            this.mlwConfigEx = new ConfigExtraction(this);

            this.ps = new ProcessScan(this);

            this.plm = new ProcessListManager(this);

            this.pm = new ProcessMonitor(this);

            this.pl = new ProcessLaunch(this);


            //Thread for monitoring processes
            threadProcessMonitor = new Thread(new ThreadStart(pm.ProcessMonitoringLoop));
            threadProcessMonitor.Name = "ProcessMonitor thread";

            //Thread for launching new process
            threadProcessLaunch = new Thread(new ThreadStart(pl.LaunchProcess));
            threadProcessLaunch.Name = "ProcessLaunch thread";

            //THread for Scan Processes
            threadProcessScan = new Thread(new ThreadStart(ps.ScanProcess));
            threadProcessScan.Name = "ProcessScan thread";

            threadProcessMonitor.Start();
            threadProcessLaunch.Start();
            threadProcessScan.Start();

        }



        public void TriggerProcessLaunchThread(string processToOpen, string additionalArguments)
        {
            //passing process to open path and arguments
            pl.AddStartupInfo(processToOpen, additionalArguments);

           

            //sveglia il thread
            startThreadProcessLaunch.Set();
        }

        public void Pause()
        {
            pauseEvent.Reset();
        }

        public void Resume()
        {
            pauseEvent.Set();
        }

        public void Stop()
        {
        
            // set isRunning to stop thread loop
            pm.isRunning = false;
            ps.isRunning = false;
           


            //awake all thread waiting...
            pauseEvent.Set();

            startThreadProcessLaunch.Set();
            startThreadProcessScan.Set();
            if (threadProcessLaunch != null && threadProcessLaunch.IsAlive)
            {
                threadProcessLaunch.Join();
            }
            if (threadProcessScan != null && threadProcessScan.IsAlive)
            {
                threadProcessScan.Join();
            }

            //join all threads to stop them
            if (threadProcessMonitor != null && threadProcessMonitor.IsAlive)
            {
                threadProcessMonitor.Join();
            }
           

        }
    }
}