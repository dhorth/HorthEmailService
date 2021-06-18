using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Horth.Service.Email.Shared.Email
{
    public  class MonitorSubject
    {
        internal MonitorSubject(int id, string subject)
        {
            Key = id.ToString();
            Subject = subject;
        }
        internal MonitorSubject(string key, string subject)
        {
            Key = key;
            Subject = subject;
        }
        public string Subject { get;set;}
        public string Key { get;set;}
        public override string ToString()
        {
            return $"{Subject}-{Key}";
        }
    }

    public partial class MonitorSubjects
    {
        public static MonitorSubject Legal(int key) => new MonitorSubject(key, "Legal Assistance Request Form");
    }
}
