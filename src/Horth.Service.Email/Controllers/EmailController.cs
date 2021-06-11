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
            Log.Logger.Information($"GetAll()=>{ret.Count()}");
            return Ok(ret);
        }

        //[HttpGet("api/v1/[action]")]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(IEnumerable<OneOfficeMailMessage>), (int)HttpStatusCode.OK)]
        //public async Task<ActionResult<IEnumerable<OneOfficeMailMessage>>> GetQueue()
        //{
        //    Log.Logger.Debug($"Calling GetQueue()");
        //    var ret=new List<OneOfficeMailMessage>();
        //    try
        //    {
        //        var msgs = await _messageQueueService.GetAll(IrcMessageQueueMessage.MsgService.Email.ToString());
        //        //convert the message payload to a OneOfficeMailMessage
        //        foreach (var msg in msgs)
        //        {
        //            var mailMessage = new OneOfficeMailMessage(msg);
        //            ret.Add(mailMessage);
        //        }
        //        Log.Logger.Information($"GetQueue()=>{ret.Count()}");
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Error(ex, "GetQueue");
        //    }
        //    return Ok(ret);
        //}

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
            Log.Logger.Debug($"CheckMail()=>{ret.Count()}");
            return Ok(ret);
        }

        [HttpPost("api/v1/[action]")]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> TestSend(string to)
        {
            Log.Logger.Debug($"Calling TestSend()");
            var ooMsg = new OneOfficeMailMessage(
                new List<string> { to },
                "Email Receiver Startup",
                "<h1>Email Service Started</h1><p>Ready to Receive Messages</p>");

            var msg = new IrcMessageQueueMessage(IrcMessageQueueMessage.MsgService.Email,Request.Host.ToString(),JsonConvert.SerializeObject(ooMsg));
            await _messageQueueService.Publish(msg);
            return Ok(true);
        }

        //[HttpPost("api/v1/[action]")]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
        //public async Task<IActionResult> ProcessQueue()
        //{
        //    Log.Logger.Debug($"Calling ProcessQueue()");
        //    await _messageQueueService.Process(IrcMessageQueueMessage.MsgService.Email.ToString());
        //    Log.Logger.Information($"ProcessQueue()=>true");
        //    return Ok(true);
        //}

        //[HttpPost("api/v1/[action]")]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(IEnumerable<OneOfficeMailMessage>), (int)HttpStatusCode.OK)]
        //public async Task<IActionResult> FlushQueue()
        //{
        //    Log.Logger.Debug($"Calling FlushQueue()");
        //    var ret = await _messageQueueService.GetAll(IrcMessageQueueMessage.MsgService.Email.ToString());
        //    await _messageQueueService.RemoveAll(IrcMessageQueueMessage.MsgService.Email.ToString());
        //    Log.Logger.Information($"FlushQueue()=>{ret.Count()}");
        //    return Ok(true);
        //}

        //[HttpPost("api/v1/[action]")]
        //[ProducesResponseType((int)HttpStatusCode.BadRequest)]
        //[ProducesResponseType((int)HttpStatusCode.NotFound)]
        //[ProducesResponseType(typeof(IEnumerable<OneOfficeMailMessage>), (int)HttpStatusCode.OK)]
        //public async Task<IActionResult> RemoveMessage(int msgId)
        //{
        //    Log.Logger.Debug($"Calling RemoveMessage({msgId})");
        //    await _messageQueueService.Remove(IrcMessageQueueMessage.MsgService.Email.ToString(), msgId);
        //    Log.Logger.Information($"RemoveMessage({msgId})=>true");
        //    return Ok(true);
        //}

    }
}
