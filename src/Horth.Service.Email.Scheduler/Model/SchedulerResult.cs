using System;

namespace Horth.Service.Email.Scheduler.Model
{
    public class SchedulerResult
    {
        public SchedulerResult()
        {
        }
        public int Id { get; set; }
        public string Job { get; set; }

        //serialize the results
        public string ResultJoson { get; set; }

        public bool Success { get; set; }

        public DateTime LastRun { get; set; }
        public int LastRunEnd { get; set; }
    }
}
