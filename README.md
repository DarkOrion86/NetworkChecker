# NetworkChecker
This is a tool, that checks network devices and works with Prometheus-net
Right now it checks devices only by ping. 

Configuration of app or service located in file config.ini
This is exammple of it:

"PushGatewayURL=http://localhost:9091/metrics
PushGatewayIsON=1
ServerMetricsIsON=1
ServerMetricsPort=12345
NameOfMetrics=idea_ping_device
JobNameOfGatewayPusher=MyPinger


class:DeviceForPing:IP=8.8.8.8,Name=google,PingInterval=5
class:DeviceForPing:IP=172.16.50.1,Name=cisco_Sluck50,PingInterval=3
class:DeviceForPing:IP=172.16.44.1,Name=cisco_Vitebsk44,PingInterval=3
class:DeviceForPing:IP=172.16.40.1,Name=cisco_Minsk40,PingInterval=3"

PushGatewayURL - to this URL app or service pushes the data;
PushGatewayIsON - ON(1) or OFF(0) using Push Gateway;
ServerMetricsIsON - ON(1) or OFF(0) server metrics;
ServerMetricsPort - local port of Metrics Server;
NameOfMetrics - name of metrics on server;
JobNameOfGatewayPusher  - name of job using Push Gateway;

class:DeviceForPing:IP=8.8.8.8,Name=google,PingInterval=5 means,
that, using class DeviceForPing, app or service will check device with ip 8.8.8.8
named as google every 5 seconds.

You can add your own class of device and realize methods of checking and pushing data to Prometheus. 
