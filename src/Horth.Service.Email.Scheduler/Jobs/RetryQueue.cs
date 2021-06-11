using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Horth.Service.Email.Model;
using Horth.Service.Email.Scheduler.Model;
using Horth.Service.Email.Scheduler.Quartz.Jobs;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Email;
using Horth.Service.Email.Shared.MsgQueue;
using Horth.Service.Email.Shared.Service;
using Irc.Infrastructure.Services.Queue;
using Quartz;
using Serilog;

namespace Horth.Service.Email.Scheduler.Jobs
{
    [DisallowConcurrentExecution]
    public class RetryQueue : OneOfficeBaseJob<List<OneOfficeMailMessage>>
    {
        private readonly IIrcMessageQueueService _messageQueueService;
        private readonly IEmailService _emailService;

        public RetryQueue(
            AppSettings appSettings,
            ISchedulerResultUnitOfWork db,
            IIrcMessageQueueService messageQueueService,
            IEmailService email) : base(appSettings, db)
        {
            Log.Logger.Debug("Initializing Email Retry Queue");
            _messageQueueService = messageQueueService;
            _emailService = email;
        }

        public override async Task Execute(IJobExecutionContext context)
        {
            bool rc = false;
            Log.Logger.Information("Email Retry Queue Started");
            try
            {
                Results.Clear();
                //var messages = await _messageQueueService.GetList(IrcMessageQueueMessage.MsgService.Email.ToString());
                //foreach (var id in messages)
                //{
                //    var msg = await _messageQueueService.Get(IrcMessageQueueMessage.MsgService.Email.ToString(), id);
                //    if (msg.RetryCount <= 5)
                //        await _messageQueueService.Retry(msg);
                //}
                //    var retryMsgs = await _messageQueueService.GetAll(IrcMessageQueueMessage.MsgService.Email.ToString());
                //    var requests = await _emailService.GetQueue();
                //    Results.AddRange(requests);
                //    foreach (var msg in retryMsgs)
                //    {
                //        if (requests.All(a => a.Id != msg.Id))
                //        {
                //            //remove the request
                //            await _messageQueueService.Remove(IrcMessageQueueMessage.MsgService.Email.ToString(), msg.Id);
                //        }

                //        //leave are error messages in the queue, 
                //        //if (msg.Subject == "Email Retry Queue - Error")
                //        //{
                //        //    continue;
                //        //}

                //        if (msg.RetryCount == 5 && !string.IsNullOrWhiteSpace(AppSettings.MailMonitor))
                //        {
                //            //cancel it and let the author know
                //            //await _messageQueueService.Remove<OneOfficeMailMessage>(MsgService.Email, msg.Key);

                //            //send the report 
                //            var email = _emailService.NewMessage();
                //            email.AddHeader("Email Request Canceled", "Email Request");

                //            email.AddSectionHeader($"Unable to process email request ");
                //            var r = Results.FirstOrDefault(a => a.Id == msg.Id);
                //            email.AddColumn("ID", r.Id);
                //            email.AddColumn("Subject", r.Subject);
                //            email.AddColumn("To", r.To.First());
                //            email.AddColumn("CC", r.Cc.FirstOrDefault());
                //            email.AddColumn("Created", msg.Created);
                //            email.AddColumn("LastTry", msg.LastTry);
                //            var to = AppSettings.MailMonitor;

                //            if (SendEmail)
                //                await email.Send(to, "", "Email Retry Queue - Error");
                //        }
                //}
                //foreach (var request in Results)
                //{
                //    var i = retryMsgs.FirstOrDefault(a => a.Key == request.Key);
                //    request.RetryCount = i == null ? -1 : i.RetryCount;
                //}
                rc = true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Execute Email Retry Queue");
            }
            LastRun = rc;
            LastRunOn = DateTime.Now;
            await UpdateResult();
        }

    }
}