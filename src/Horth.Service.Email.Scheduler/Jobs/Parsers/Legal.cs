using System.Linq;
using Serilog;
using System.Threading.Tasks;
using Horth.Service.Email.Model;
using Horth.Service.Email.Shared.MsgQueue;

namespace OneOffice.Scheduler.Job.Email.Jobs.Parsers
{
    public class LegalAssistance : BaseMailMonitor
    {
        public override string Key => "Legal Assistance Request Form-";
        public string EndKey => "";


        public LegalAssistance(IIrcMessageQueueService messageQueueService) : base(messageQueueService)
        {

        }

        public override async Task<int> ProcessMessage(OneOfficeMailMessage message)
        {
            Log.Logger.Information($"Processing message - {message.Subject} From: {message.From}");
            if (message.Attachments.Count <= 0)
            {
                Log.Logger.Error("Missing attachment");
                return -1;
            }
            int id;

            //parse the subject line for the id
            var idStr = message.Subject.Substring(Key.Length).Trim();
            if (!int.TryParse(idStr, out id))
            {
                Log.Logger.Error("Unable to parse subject line {0}", message.Subject);
                return -1;
            }


            //var obj = new CollectionRequest {Id=id,FormData=message.Attachments.FirstOrDefault()?.FileData, FileName=message.FileName };
            ////place on queue 
            //await Publish(obj);
            return -1;
        }

    }
}
