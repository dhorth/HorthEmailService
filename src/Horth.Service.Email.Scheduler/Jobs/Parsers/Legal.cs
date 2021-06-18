using System.Linq;
using Serilog;
using System.Threading.Tasks;
using Horth.Service.Email.Model;
using Horth.Service.Email.Shared.MsgQueue;
using Horth.Service.Email.Shared.Email;
using Irc.Infrastructure.Services.Queue;
using System;

namespace OneOffice.Scheduler.Job.Email.Jobs.Parsers
{
    public class LegalAssistance : BaseMailMonitor
    {
        protected override MonitorSubject Key => MonitorSubjects.Legal(-1);


        public LegalAssistance(IIrcMessageQueueService messageQueueService) : base(messageQueueService)
        {

        }

        protected override Task<int> ProcessMessageInternal(int id, OneOfficeMailMessage message)
        {
            Log.Logger.Information($"Processing message - {message.Subject} From: {message.From}");
            var ret = -1;
            try
            {
                if (message.Attachments.Count > 0)
                {
                    Log.Logger.Information($"Do any required processing of request {id} here");
                }
                else
                {
                    Log.Logger.Error("Missing attachment");
                    ret = -1;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "ProcessMessageInternal");
            }
            //var obj = new CollectionRequest {Id=id,FormData=message.Attachments.FirstOrDefault()?.FileData, FileName=message.FileName };
            ////place on queue 
            //await Publish(obj);
            return Task.FromResult(ret);
        }

    }
}
