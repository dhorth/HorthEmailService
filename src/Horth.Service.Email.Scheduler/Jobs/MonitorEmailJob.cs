using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Horth.Service.Email.Model;
using Horth.Service.Email.Scheduler.Model;
using Horth.Service.Email.Scheduler.Quartz.Jobs;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.MsgQueue;
using Horth.Service.Email.Shared.Service;
using OneOffice.Scheduler.Job.Email.Jobs.Parsers;
using Quartz;
using Serilog;

namespace Horth.Service.Email.Scheduler.Jobs
{
    [DisallowConcurrentExecution]
    public class MonitorEmailJob : OneOfficeBaseJob<List<OneOfficeMailMessage>>
    {
        private readonly List<BaseMailMonitor> _mailBoxes;
        private readonly IIrcMessageQueueService _messageQueueService;
        private readonly IEmailService _emailService;

        public MonitorEmailJob(
            IIrcMessageQueueService messageQueueService,
            ISchedulerResultUnitOfWork db,
            IEmailService email,
            AppSettings appSettings) :base(appSettings,db)
        {
            Log.Logger.Debug("Initializing Email Monitor Job");
            _messageQueueService = messageQueueService;
            _emailService = email;
            _mailBoxes = new List<BaseMailMonitor>
            {
                new LegalAssistance(_messageQueueService),
            };
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            bool rc = false;
            Log.Logger.Debug("Checking Email Monitor Started");
            try
            {
                Results.Clear();
                Results.AddRange(await _emailService.CheckMail());
                Log.Logger.Information($"Found {Results.Count} Messages");
                var i=1;
                foreach (var msg in Results)
                {
                    Log.Logger.Information($"Processing {i} of {Results.Count} Messages");
                    await ProcessMessage(msg);
                    i++;
                }
                rc = true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Execute Email Monitor Runner");
            }
            LastRun = rc;
            LastRunOn = DateTime.Now;
            await UpdateResult();
        }
        private async Task ProcessMessage(OneOfficeMailMessage msg)
        {
            try
            {
                foreach (var mb in _mailBoxes)
                {
                    Log.Logger.Debug("Checking Mailbox  {0}", mb.Key);
                    if (msg.Subject.Contains(mb.Key))
                    {
                        try
                        {
                            Log.Logger.Debug("Processing Message {0}", mb.Key);
                            await mb.ProcessMessage(msg);
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Error(ex, "");
                        }
                    }
                }
                Log.Logger.Debug("Done Processing Message");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "");
            }
        }
    }
}