using System;
using System.Threading.Tasks;
using Horth.Service.Email.Scheduler.Model;
using Horth.Service.Email.Shared.Configuration;
using Newtonsoft.Json;
using OneOffice.Scheduler.Job.Shared;
using Quartz;

namespace Horth.Service.Email.Scheduler.Quartz.Jobs
{
    public abstract class OneOfficeBaseJob<T> : IJob
    {
        public enum ReportResult { Success, Failed, Exception };
        protected ISchedulerResultUnitOfWork Db;
        private T _results;
        protected AppSettings AppSettings;
        protected OneOfficeBaseJob(AppSettings appSettings, ISchedulerResultUnitOfWork db)
        {
            AppSettings = appSettings;
            this.Db = db;
            _results = (T)Activator.CreateInstance(typeof(T));
#if RELEASE
            SendEmail = true;
#endif
        }
        public abstract Task Execute(IJobExecutionContext context);
        public T Results => _results;
        public bool SendEmail { get; set; }
        public bool LastRun { get; set; }
        public int LastRunEnd { get; set; }
        public DateTime LastRunOn { get; set; }
        public string Job => GetType().Name;

        public async Task Load()
        {
            var json = await Db.SchedulerResult.GetAsync(a => a.Job == Job);
            if (json != null)
            {
                var history = JsonConvert.DeserializeObject<T>(json.ResultJoson);
                if (history != null)
                {
                    _results = history;
                }
                LastRun = json.Success;
                LastRunOn = json.LastRun.ToLocalTime();
                LastRunEnd = json.LastRunEnd;
            }

            if (_results == null)
                _results = (T)Activator.CreateInstance(typeof(T));
        }
        public async Task UpdateResult()
        {
            var obj = await Db.SchedulerResult.GetAsync(a=>a.Job==Job);
            if (obj == null)
            {
                obj = new SchedulerResult
                {
                    Job = Job,
                    Success = LastRun,
                    LastRun = DateTime.Now,
                    ResultJoson = JsonConvert.SerializeObject(_results),
                    LastRunEnd = LastRunEnd
                };
                if (obj.ResultJoson.Length > 15000000)
                    obj.ResultJoson = "";

                Db.SchedulerResult.Add(obj);
            }
            else
            {
                obj.Success = LastRun;
                obj.LastRun = DateTime.Now;
                obj.LastRunEnd = LastRunEnd;
                obj.ResultJoson = JsonConvert.SerializeObject(_results);

                if (obj.ResultJoson.Length > 15000000)
                    obj.ResultJoson = "";
            }
            Db.Save();

            //_db.AddTask(GetType(), LastRun ? 1 : 0);
        }
    }

}
