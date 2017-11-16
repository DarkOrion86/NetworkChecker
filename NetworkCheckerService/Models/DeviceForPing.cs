using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using Prometheus;

namespace NetworkChecker.Models
{
    public class DeviceForPing : IDevice
    {
        public string Name { get; set; }
        public string DeviceIP { get; set; }
        public TimeSpan TimeInterval { get; set; }
        private Provider provider;
        private Gauge gauge;

        //constructor
        public DeviceForPing(string config)
        {
            provider = Service.provider;
            try
            {
                provider = Service.provider;
                string[] deviceConfig = config.Split(',');
                for (int j = 0; j < 3; j++)
                {
                    if (deviceConfig[j].StartsWith("IP="))
                        if (Controller.IsIPCorrect(deviceConfig[j].Split('=')[1])) DeviceIP = deviceConfig[j].Split('=')[1];
                    if (deviceConfig[j].StartsWith("Name=")) Name = deviceConfig[j].Split('=')[1];
                    if (deviceConfig[j].StartsWith("PingInterval=")) TimeInterval = TimeSpan.FromSeconds(int.Parse(deviceConfig[j].Split('=')[1]));
                }
                Service.logger.Debug("Adding device Name=" + Name + ", IP = "
                                + DeviceIP + ", timeinterval = " + TimeInterval);
            }
            catch (Exception e)
            {
                Service.logger.Error($"Faulted while creating DeviceForPing: {config}" + e.Message);
            }
        }

        //method that checks device
        public async Task<string[]> CheckDevice()
        {
            try
            {
                Ping ping = new Ping();
                double replyTime = 0;
                string replyStatus = null;

                //uses 1 ping packet
                PingReply reply = await ping.SendPingAsync(this.DeviceIP);
                if (reply.Status.ToString() == "Success")
                    replyTime = reply.RoundtripTime;
                else replyTime = -1;
                replyStatus = reply.Status.ToString();
                Service.logger.Debug($"{this.Name}:Ping {this.DeviceIP} {replyStatus}, Latency = {(replyTime).ToString()}");
                return new string[] { (replyTime).ToString() };
            }
            catch (Exception e)
            {
                Service.logger.Error($"Faulted while checking DeviceForPing: {this.Name}, {this.DeviceIP}, {this.TimeInterval}" + e.Message);
                return null;
            }
        }

        //Creating counter for Prometheus. It can be gauge,histogramm and others.
        public object CreateCounterForPrometheus()
        {
            return gauge = Metrics.CreateGauge(provider.NameOfMetrics, "help text", labelNames: new[] { "devname" });
        }

        //pushing data to Prometheus
        public void PushDataToPrometheus(string[] args)
        {
            try
            {
                gauge.Labels(this.Name).Set(double.Parse(args[0]));
            }
            catch (Exception e)
            {
                Service.logger.Error("Faulting to push data: " + e.Message);
            }
        }

        //this method controls, that input config string from ini file is correct
        public bool IsParametersCorrect(string config)
        {
            string ip = null, name = null;
            TimeSpan interval = TimeSpan.Zero;
            try
            {
                if (config.Length != 0)
                {
                    string[] deviceConfig = config.Split(',');
                    for (int j = 0; j < 3; j++)
                    {
                        if (deviceConfig[j].StartsWith("IP="))
                            if (Controller.IsIPCorrect(deviceConfig[j].Split('=')[1])) ip = deviceConfig[j].Split('=')[1];
                        if (deviceConfig[j].StartsWith("Name=")) name = deviceConfig[j].Split('=')[1];
                        if (deviceConfig[j].StartsWith("PingInterval=")) interval = TimeSpan.FromSeconds(int.Parse(deviceConfig[j].Split('=')[1]));
                    }
                }
                if ((ip != null) && (name != null) && (interval != TimeSpan.Zero))
                    return true;
                else return false;
            }
            catch (Exception e)
            {
                Service.logger.Error("Incorrect parameters: " + e.Message);
                return false;
            }
        }

    }
}

