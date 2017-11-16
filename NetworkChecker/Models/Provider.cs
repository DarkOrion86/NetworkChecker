using System;
using System.IO;
using System.Net;
using System.Text;
using Prometheus;
using NLog;

namespace NetworkChecker.Models
{
    public class Provider
    {
        MetricServer mSrv;
        MetricPusher pusher;
        public Logger logger;
        public int ServerMetricsPort { get; set; } = 1234;
        public string NameOfMetrics { get; set; } = "idea_ping_device";
        public string JobNameOfGatewayPusher { get; set; } = "pinger";
        public string PushGatewayURL { get; set; } = "http://localhost:9091";
        public bool ServerMetricsIsON { get; set; } = true;
        public bool PushGatewayIsON { get; set; } = true;

        //constructor
        public Provider()
        {
            logger = Program.logger;
        }

        //starting server
        public void StartServer()
        {
            try
            {
                //if ServerMetricsIsON=1 in config file metric server starts
                if (ServerMetricsIsON)
                {
                mSrv = new MetricServer(port: ServerMetricsPort);
                logger.Debug("Start exporter");
                mSrv.Start();
                }
                //if PushGatewayIsON=1 in config file metric pusher starts
                if (PushGatewayIsON)
                {
                    pusher = new MetricPusher(endpoint: PushGatewayURL, job: JobNameOfGatewayPusher);
                    pusher.Start();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message);
            }

        }

        //stopping server
        public void StopServer()
        {

            if (ServerMetricsIsON) mSrv.Stop();
            if (PushGatewayIsON) pusher.Stop();
        }
    }
}



