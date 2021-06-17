using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Horth.Service.Email.Model;
using Horth.Service.Email.Service;
using Horth.Service.Email.Shared.MsgQueue;
using Irc.Infrastructure.Controller;
using Irc.Infrastructure.Services.Queue;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Serilog;

namespace Horth.Service.Email.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : IrcController
    {
        private readonly IEmailUnitOfWork _db;
        IPop3MailClient _pop3Client;
        IIrcMessageQueueService _messageQueueService;

        public EmailController(IEmailUnitOfWork db, IPop3MailClient pop3Client, IIrcMessageQueueService messageQueueService, IConfiguration config) : base(config)
        {
            _db = db;
            _pop3Client = pop3Client;
            _messageQueueService = messageQueueService;
        }

        [HttpGet("api/v1/[action]")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<EmailStat>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetStats()
        {
            Log.Logger.Debug($"Calling GetAll()");
            var ret = await _db.Email.GetAllAsync();
            Log.Logger.Information($"GetAll() => {ret.Count()}");
            return Ok(ret);
        }

        [HttpGet("api/v1/[action]")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<OneOfficeMailMessage>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> CheckMail()
        {
            Log.Logger.Debug($"Calling CheckMail()");
            var ret = await _pop3Client.GetMessages();
            if (ret == null)
                return Problem();
            Log.Logger.Information($"CheckMail() => {ret.Count()}");
            return Ok(ret);
        }


        [HttpGet("api/v1/[action]")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(IEnumerable<OneOfficeMailMessage>), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetFailedMessages()
        {
            Log.Logger.Debug($"Calling GetFailedMessages()");
            var msgs = await _messageQueueService.GetFailures(IrcMessageQueueMessage.MsgService.Email);
            if (msgs == null)
                return Problem();
            var ret = msgs.Select(a => new OneOfficeMailMessage(a));
            Log.Logger.Information($"GetFailedMessages() => {ret.Count()}");
            return Ok(ret);
        }

        [HttpPost("api/v1/[action]")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> TestSend()
        {
            Log.Logger.Debug($"Calling TestSend()");
            var ooMsg = new OneOfficeMailMessage(
                new List<string> { AppSettings.MailMonitor },
                "Email Receiver Startup",
                "<h1>Email Service Started</h1><p>Ready to Receive Messages</p>");

            var msg = new IrcMessageQueueMessage(IrcMessageQueueMessage.MsgService.Email,Request.Host.ToString(),JsonConvert.SerializeObject(ooMsg));
            await _messageQueueService.Publish(msg);
            Log.Logger.Information($"TestSend() => true");
            return Ok(true);
        }

    }
}
