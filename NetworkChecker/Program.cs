using System;
using NLog;
using NetworkChecker.Models;



namespace NetworkChecker
{
    class Program
    {
        public static Logger logger;
        public static Provider provider;
        static Controller controller;


        static void Main(string[] args)
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

            string consoleInput = "default";

            //wainting start, stop or quit from console
            while (consoleInput.ToUpper() != "QUIT")
            {
                consoleInput = Console.ReadLine();
                if (consoleInput.ToUpper() == "STOP")
                    controller.StopCancellationToken();
                if (consoleInput.ToUpper() == "START")
                {
                    if (Controller.token.IsCancellationRequested)
                    {
                        controller.StartCancellationToken();
                        controller.CheckDevices();
                    }
                }
            }
            //quiting
            controller.StopCancellationToken();
            provider.StopServer();
            logger.Info("Quiting...");
            Console.ReadKey();
        }
    }
}
