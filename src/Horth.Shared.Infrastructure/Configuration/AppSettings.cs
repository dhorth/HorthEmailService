using System;
using System.Collections.Generic;
using System.Linq;
using Horth.Service.Email.Shared.Service;
using Microsoft.Extensions.Configuration;
using NATS.Client;
using Serilog;

namespace Horth.Service.Email.Shared.Configuration
{
    public class AppSettings
    {
        private readonly IConfiguration _configuration;
        private static readonly object _lock = new object();

        public enum EmailServiceProvider
        {
            Smtp,Aws
        }

        public AppSettings(IConfiguration c)
        {
            _configuration = c;
            ServiceTargets = new List<RegisteredService>();

            var targets = c.GetSection("Targets");
            var siteCount = targets.GetChildren().Count();

            Log.Logger.Information($"Found {siteCount} sites to create/update");
            foreach (var target in targets.GetChildren())
            {
                var service= (RegisteredServiceName)Enum.Parse(typeof(RegisteredServiceName), target.GetValue<string>("service"));
                var host = target.GetValue<string>("host");
                var port = target.GetValue<int>("port");

                var site = new RegisteredService(service, host, port);
                ServiceTargets.Add(site);
            }
        }

        public string ConnectionString => GetValue("ConnectionStrings:SqliteDatabase", $"Data Source={DbDirectory}\\{Database}.sqlite");
        public string DbDirectory => GetValue("Database:Dir", "..\\data");
        public string Database => GetValue("Database:Database", "horth_email");

        //Message Queue
        public string RabbitMqServer => GetValue("MessageQueue:Rabbit:Host", "localhost");
        public string RabbitMqUserName => GetValue("MessageQueue:Rabbit:UserName", "guest");
        public string RabbitMqPassword => GetValue("MessageQueue:Rabbit:Password", "guest");

        //Nats Service Name
        public string NatsServerUrl => GetValue("MessageQueue:Nats:Url", "nats://localhost");
        public string NatsServerServiceName => GetValue("MessageQueue:Nats:ServiceName", "");

        //logging
        public string SeqServerUrl =>GetValue("Serilog:SeqServerUrl", "http://localhost:5341");
        public string Logstash => GetValue("Serilog:Logstash", "");

        //email
        public string MailMonitor => GetValue("Email:MailMonitor", "");

        public EmailServiceProvider EmailService => GetEnumValue<EmailServiceProvider>("Email:Service", "Smtp");

        public string SourceEmail => GetValue("Email:SourceEmail", "");

        public string SmtpServer => GetValue("Email:SmtpServer", "smtp.gmail.com");
        public int SmtpPort => GetValue<int>("Email:SmtpPort", 465);
        public string SmtpUsername => GetValue("Email:SmtpUsername", EmailUser);
        public string SmtpPassword => GetValue("Email:SmtpPassword", EmailPassword);

        public string AWSUsername => GetValue("Email:AWSUsername", "");
        public string AWSPassword => GetValue("Email:AWSPassword", "");

        public string EmailServer => GetValue("Email:PopServer", "pop.gmail.com");
        public string EmailUser => GetValue("Email:User", "");
        public string EmailPassword => GetValue("Email:Password", "");

        public string EmailTitle => GetValue("Email:MessageTemplate:Title", "");
        public string EmailLogo => GetValue("Email:MessageTemplate:Logo", "");



        public List<RegisteredService> ServiceTargets { get; set; }

        protected string GetValue(string name, string defaultValue = "")
        {
            return _configuration.GetValue(name, defaultValue);
        }
        protected T GetValue<T>(string name, T defaultValue)
        {
            return _configuration.GetValue(name, defaultValue);
        }
        protected T GetEnumValue<T>(string name, string defaultValue)
        {
            var strValue= _configuration.GetValue(name, defaultValue);
            var ret=(T)Enum.Parse(typeof(T), strValue);
            return ret;
        }

    }

}
