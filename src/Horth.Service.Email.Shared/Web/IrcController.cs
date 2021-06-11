using System;
using System.Collections.Generic;
using System.Text;
using Horth.Service.Email.Shared.Configuration;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Irc.Infrastructure.Controller
{
    public class IrcController : Microsoft.AspNetCore.Mvc.Controller
    {
        public virtual AppSettings AppSettings { get; set; }
        public IrcController(IConfiguration configuration)
        {
            Configuration = configuration;
            AppSettings = new AppSettings(configuration);
        }
        public IConfiguration Configuration { get; }
        public ILogger Logger => Log.Logger;

    }
    public class IrcControllerBase : ControllerBase
    {
        public virtual AppSettings AppSettings { get; set; }
        public IrcControllerBase(IConfiguration configuration)
        {
            Configuration = configuration;
            AppSettings = new AppSettings(configuration);
        }
        public IConfiguration Configuration { get; }
        public ILogger Logger => Log.Logger;
    }
}
