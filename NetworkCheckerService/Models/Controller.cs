using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using System.IO;
using System;
using System.Reflection;

namespace NetworkChecker.Models
{
    public class Controller
    {
        static List<IDevice> devices = new List<IDevice>();
        public static CancellationToken token;
        static CancellationTokenSource tokenSource;
        static Logger logger;
        Provider provider;

        //constructor
        public Controller()
        {
            logger = Service.logger;
            provider = Service.provider;
        }

        //method for checking devices
        public void CheckDevices()
        {
            //for each device run task to ckeck
            foreach (IDevice device in devices)
            {
                var task = new Task(() => RunCheck(device));
                task.Start();
            }
        }

        //Method creates counter for Prometheus, checks device and pushes results to Prometheus
        public async void RunCheck(IDevice device)
        {
            var counter = device.CreateCounterForPrometheus();
            try
            {
                while (!Controller.token.IsCancellationRequested)
                {
                    string[] result = await device.CheckDevice();
                    //sending reply data to provider
                    device.PushDataToPrometheus(result);
                    //waiting a time period
                    Thread.Sleep(device.TimeInterval);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Faulting checking device: {device.Name}, {e.Message}");
            }

        }
        //ini-file reading method
        public void ReadIniFile(string[] args)
        {
            string path = null;
            string[] iniFile = null;
            Assembly assembly = Assembly.GetExecutingAssembly();
            var types = assembly.ExportedTypes;
            logger.Debug("Reading INI file");
            try
            {
                if (args.Length != 0)
                    path = args[0];
                else path = AppDomain.CurrentDomain.BaseDirectory + @"config.ini";
                logger.Info("Reading INI file: " + path);
                iniFile = File.ReadAllLines(path);
                logger.Debug("Read " + iniFile.Length + " lines");

                //finding bindings of app parameters
                for (int i = 0; i < iniFile.Length; i++)
                {
                    iniFile[i].Replace(" ", "");

                    if (iniFile[i].StartsWith("PushGatewayURL"))
                    {
                        provider.PushGatewayURL = iniFile[i].Split('=')[1];
                    }
                    if (iniFile[i].StartsWith("ServerMetricsPort"))
                    {
                        provider.ServerMetricsPort = int.Parse(iniFile[i].Split('=')[1]);
                    }
                    if (iniFile[i].StartsWith("NameOfMetrics"))
                    {
                        provider.NameOfMetrics = iniFile[i].Split('=')[1];
                    }
                    if (iniFile[i].StartsWith("PushGatewayIsON"))
                    {
                        if ((iniFile[i].Split('=')[1]) == "1") provider.PushGatewayIsON = true;
                        else if ((iniFile[i].Split('=')[1]) == "0") provider.PushGatewayIsON = false;
                    }
                    if (iniFile[i].StartsWith("ServerMetricsIsON"))
                    {
                        if ((iniFile[i].Split('=')[1]) == "1") provider.ServerMetricsIsON = true;
                        else if ((iniFile[i].Split('=')[1]) == "0") provider.ServerMetricsIsON = false;
                    }
                    if (iniFile[i].StartsWith("JobNameOfGatewayPusher"))
                        provider.JobNameOfGatewayPusher = iniFile[i].Split('=')[1];

                    //finding in assembly classes of devices and adding it to list for check
                    if (iniFile[i].StartsWith("class:"))
                    {
                        try
                        {
                            string nameOfClass = iniFile[i].Split(':')[1];
                            foreach (var type in types)
                            {
                                if (type.FullName == "NetworkChecker.Models." + nameOfClass)
                                {
                                    var device = Activator.CreateInstance(type, iniFile[i].Split(':')[2]) as IDevice;
                                    if (device.IsParametersCorrect(iniFile[i].Split(':')[2]))
                                        devices.Add(device as IDevice);
                                    else logger.Error("Incorrect parameters" + iniFile[i].Split(':')[2]);
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            logger.Error("Can't create instance of class " + iniFile[i].Split(':')[1] + " " + iniFile[i].Split(':')[2] + " " + e.Message);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }
        }

        //control of correct ip address
        public static bool IsIPCorrect(string deviceIP)
        {
            try
            {
                string[] octets = deviceIP.Split('.');
                if (octets.Length == 4)
                    if ((int.Parse(octets[0]) >= 0) && (int.Parse(octets[0]) <= 255) &&
                        (int.Parse(octets[1]) >= 0) && (int.Parse(octets[1]) <= 255) &&
                        (int.Parse(octets[2]) >= 0) && (int.Parse(octets[2]) <= 255) &&
                        (int.Parse(octets[3]) >= 0) && (int.Parse(octets[3]) <= 255))
                        return true;
                    else
                    {
                        logger.Error("Incorrect IP address");
                        return false;
                    }
                else return false;
            }
            catch (Exception e)
            {
                logger.Error("Incorrect IP address" + e.Message);
                return false;
            }
        }

        //inicialization of Cancellation Token
        public void StartCancellationToken()
        {
            logger.Info("Starting");
            tokenSource = new CancellationTokenSource();
            token = tokenSource.Token;
        }

        //stopping by Cancellation Token
        public void StopCancellationToken()
        {
            logger.Info("Stopping");
            tokenSource.Cancel();
        }
    }
}
