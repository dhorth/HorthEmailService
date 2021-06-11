using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Horth.Service.Email.Model;
using Horth.Service.Email.Shared.Configuration;
using MailKit.Net.Pop3;
using MimeKit;
using Serilog;

namespace Horth.Service.Email.Service
{
    public interface IPop3MailClient
    {
        Task<List<OneOfficeMailMessage>> GetMessages();
    }
    public class Pop3MailClient : IDisposable, IPop3MailClient
    {
        private Pop3Client _pop3;
        private AppSettings AppSettings { get;set;}
        public Pop3MailClient(AppSettings appSettings)
        {
            AppSettings= appSettings;
            _pop3 = new Pop3Client();
        }
        public async Task<List<OneOfficeMailMessage>> GetMessages()
        {
            var ret = new List<OneOfficeMailMessage>();
            try
            {
                var textKey = "Content-Type: text/plain;";
                if (_pop3.IsConnected)
                    await _pop3.DisconnectAsync(true);

                await _pop3.ConnectAsync(AppSettings.EmailServer, 995, true);
                await _pop3.AuthenticateAsync(AppSettings.EmailUser, AppSettings.EmailPassword);
                if (_pop3.Count <= 0)
                    return ret;

                var list = await _pop3.GetMessagesAsync(0, _pop3.Count);
                foreach (var message in list)
                {
                    var body = message.HtmlBody;
                    var text = message.Body.ToString();
                    if (text.StartsWith(textKey))
                    {
                        body = text;
                        var idx = body.IndexOf("inline", StringComparison.Ordinal) + 6;
                        if (idx > 5)
                            body = $"<html><body>{body.Substring(idx)}</body></html>";
                        //Log.Logger.Warning($"Replacing body text with msg={body}");
                    }
                    //Log.Logger.Information($"Front Office Message #: {message.Subject}  Body: {body}");
                    if (string.IsNullOrWhiteSpace(body))
                    {
                        body = text;
                        Log.Logger.Warning($"Body text in empty, msg={text}, using {body} instead");
                    }

                    var m = new OneOfficeMailMessage
                    {
                        Subject = message.Subject,
                        Body = body,
                        From = message.From.First().Name,
                        Date = message.Date.UtcDateTime,
                        To = new List<string>()
                    };

                    foreach (var to in message.To)
                        m.To.Add(to.Name);

                    m.Cc = new List<string>();
                    foreach (var cc in message.Cc)
                        m.Cc.Add(cc.Name);
                    ret.Add(m);

                    m.Attachments = new List<OneOfficeMailAttachment>();
                    foreach (var a in message.Attachments)
                    {
                        string attachment;
                        await using (var ms = new MemoryStream())
                        {
                            if (a is MessagePart rfc822)
                            {
                                //var fileName = a.ContentDisposition?.FileName;
                                //if (string.IsNullOrEmpty(fileName))
                                //    fileName = "attached-message.eml";

                                await rfc822.WriteToAsync(ms);
                            }
                            else
                            {
                                var part = (MimePart)a;
                                await part.Content.DecodeToAsync(ms);
                            }
                            //await a.WriteToAsync(ms);
                            attachment = Convert.ToBase64String(ms.ToArray());
                        }
                        m.Attachments.Add(new OneOfficeMailAttachment
                        {
                            FileName = a.ContentId,
                            MimeContent = a.ContentType.ToString(),
                            FileData = attachment
                        });
                    }
                }

                await _pop3.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Check Email");
                ret = null;
            }
            return ret;
        }
        public void Dispose()
        {
            _pop3?.Dispose();
        }
    }
}
