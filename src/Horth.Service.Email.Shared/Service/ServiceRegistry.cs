using System.Collections.Generic;
using System.Linq;
using Horth.Service.Email.Shared.Configuration;
using Serilog;

namespace Horth.Service.Email.Shared.Service
{
    public enum RegisteredServiceName
    {
        Email,
    };

    public interface IServiceRegistry
    {
        string GetTarget(RegisteredServiceName serviceName);
    }
    public class ServiceRegistry: IServiceRegistry
    {
        public ServiceRegistry(AppSettings appSettings)
        {
            Targets = new List<RegisteredService>();
            Targets.AddRange(appSettings.ServiceTargets);
        }
        public List<RegisteredService> Targets { get; set; }
        

        public string GetTarget(RegisteredServiceName serviceName)
        {
            var url="";
            var target=Targets.FirstOrDefault(a=>a.Service==serviceName);
            if(target==null)
                Log.Logger.Error($"Unable to find registration for service {serviceName}");
            url=target?.Target;
            return url;
        }
    }

    public class RegisteredService
    {
        private readonly string _host;
        private readonly int _port;

        public RegisteredService(RegisteredServiceName service, string host,  int port)
        {
            Service = service;
            _host = host;
            _port = port;
        }
        public RegisteredServiceName Service { get; set; }

        public string Target => $"https://{_host}:{_port}";
        public string HealthTarget => $"https://{_host}:{_port}/hc";
    }
}
