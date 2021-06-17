using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Horth.Service.Email.Queue.Model;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Email;
using Horth.Service.Email.Shared.MsgQueue;
using Horth.Service.Email.Shared.Service;
using Horth.Shared.Infrastructure.Configuration;
using Horth.Shared.Infrastructure.Console;
using Horth.Shared.Infrastructure.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Horth.Service.Email.TestClient
{
    class Program
    {
        public static readonly string AppName = "Horth.Service.Email.TestClient";

        /// <summary>
        /// Setup and configurate your enviornment then call MainAction 
        /// to actually run the code.  Provides a nice seperation 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static async Task<int> Main(string[] args)
        {

            var config = ConsoleHelper.InitializeConsole(AppName);

            //setup our DI, the base class will add our configuration, service registry etc
            var ret = await ConsoleHelper.ConfigureServices<EmailTestClient>(config, (hostContext, service) =>
            {
                service.AddEmailService();
            });
            return ret;
        }

    }
    public class EmailTestClient : ConsoleApplication
    {

        IEmailService _emailService;
        public EmailTestClient(IEmailService emailService)
        {
            _emailService=emailService;
        }


        /// <summary>
        /// The business logic you want you Main to actually complete
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public override async Task<int> Run()
        {

            var email = _emailService.NewMessage();
            email.AddHeader("Email Test Client", "Email Request");

            email.AddSectionHeader($"Section Header");
            email.AddColumn("Row 1", 1);
            email.AddColumn("Row 2", 2);
            email.AddColumn("Row 3", 3);

            email.AddTable(new List<EmailColumn> { new EmailColumn("Col 1"), new EmailColumn("Col 2"), new EmailColumn("Col 3") });
            for (int i = 0; i < 10; i++)
            {
                email.AddRow(new List<object> { i, i * 10, i * 100 });
            }
            email.EndTable();
            email.AddSignature("Test Team");
            await email.Send("dhorth@horth.com", "", "Email Test Client");


            Log.Logger.Information("All done!  Press any key to exit");
            Console.ReadKey();
            return 0;
        }
    }
}
