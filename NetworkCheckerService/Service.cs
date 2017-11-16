using System;
using System.ServiceProcess;
using NLog;
using NetworkChecker.Models;

namespace NetworkChecker
{
    public partial class Service : ServiceBase
    {
        public static Logger logger;
        public static Provider provider;
        static Controller controller;

        public Service()
        {
            InitializeComponent();
        }
        //Starting
        protected override void OnStart(string[] args)
        {
            //inicialization of logger
            logger = LogManager.GetCurrentClassLogger();
            logger.Debug("Initialization");
            //inicialization of provider
            provider = new Provider();
            //inicialization of controller
            controller = new Controller();
            //reading ini-file
            controller.ReadIniFile(args);

            provider.StartServer();

            //inicialization of Cancellation Token
            controller.StartCancellationToken();

            //start for checking devices
            controller.CheckDevices();
        }
        
        //stopping
        protected override void OnStop()
        {
            logger.Info("Quiting...");
            controller.StopCancellationToken();
            provider.StopServer();
        }
    }
}
