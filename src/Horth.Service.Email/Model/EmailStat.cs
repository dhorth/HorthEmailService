using System;
using System.Collections.Generic;

namespace Horth.Service.Email.Model
{
    public partial class EmailStat
    {
        public int Id { get; set; }
        public DateTime StatDay { get; set; }
        public string Client { get; set; }
        public int SentExternal { get; set; }
        public int RetriesExternal { get; set; }
        public int FailedExternal { get; set; }
        public int SentInternal { get; set; }
        public int RetriesInternal { get; set; }
        public int FailedInternal { get; set; }
        public DateTime LastUpdate { get; set; }
    }
}
