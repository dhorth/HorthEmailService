using System;
using System.Collections.Generic;

namespace Horth.Service.Email.Model
{
    public partial class EmailStat
    {
        public int Id { get; set; }
        public string Key { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string Subject { get; set; }
        public int Result { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
