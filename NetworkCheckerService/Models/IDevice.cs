using System;
using System.Threading.Tasks;

namespace NetworkChecker.Models
{
    public interface IDevice
    {
        string Name { get; set; }
        string DeviceIP { get; set; }
        TimeSpan TimeInterval { get; set; }
        Task<string[]> CheckDevice();
        bool IsParametersCorrect(string config);
        object CreateCounterForPrometheus();
        void PushDataToPrometheus(string[] args);
    }
}
