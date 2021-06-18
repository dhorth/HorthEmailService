using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Horth.Service.Email.Shared.Configuration;
using Horth.Service.Email.Shared.Service;
using Irc.Infrastructure.IRC.Code;

namespace Horth.Service.Email.Shared.Email
{
    public interface IEmailMessage
    {
        string Text { get; }

        void AddHeader(string title, string subtitle, string caption);
        void AddHeader(string subtitle, string caption);
        void AddLine();
        void AddBlank(int times = 1);
        void AddNewLine(int times = 1);
        void AddText(string html, bool bold = false, bool newLine = false, bool highlight = false, bool italic = false, bool indent = false);
        void AddTable(List<EmailColumn> colHeaders);
        void AddSubTable(List<EmailColumn> colHeaders);
        void AddRow(List<object> colData, bool highlight = false);
        void AddHighlightRow(List<object> colData);
        void EndSubTable();
        void EndTable();
        void AddChangeTable();
        void AddChangeRow<T>(object old, object newObj, Expression<Func<T>> propertyExpression);
        void AddChangeRow<T>(object old, object newObj, Expression<Func<T>> propertyExpressionOld, Expression<Func<T>> propertyExpressionNew);
        void AddChangeRowFixed<T>(object old, object newValue, Expression<Func<T>> propertyExpression);
        void EndChangeTable();
        void AddSectionHeader(string title);
        void AddHighlightBlock(string text);
        void AddColumn(string title, string data);
        void AddColumn(string title, decimal? data);
        void AddColumn(string title, DateTime? data);
        void AddColumn(string title, bool? data);
        void AddLinkButton(string title, string key, string href);
        void AddLinkButton(string title, int key, string href);
        void AddSignature(string team);

        Task Send(string to, object mailMonitor, string v);
        Task<bool> Send(string to, string cc, string subject, string base64Data, string fileName);
        Task<bool> Send(List<string> to, List<string> cc, string subject, string attachment = "");
    }

    [DebuggerDisplay("Text = {Text}")]
    public partial class EmailMessage
    {

        private readonly IEmailService _emailService;
        private readonly StringBuilder _sb;

        #region styles
        private static string style_body = "style='font-family:sans-serif;font-size:14px;padding:10px 5px;word-break:normal;'";
        private static string style_table = "style='border-collapse:collapse;border-spacing:0;border-color:#999;'";

        private static string style_td = "style='padding:10px 5px;border-style:solid;border-width:0px;overflow:hidden;word-break:normal;border-color:#999;color:#444;background-color:#F7FDFA;border-top-width:1px;border-bottom-width:1px;'";
        private static string style_special_td = "style='padding:10px 5px;border-style:solid;border-width:0px;overflow:hidden;word-break:normal;border-color:#999;color:#444;background-color:#ffffd1;border-top-width:1px;border-bottom-width:1px;font-weight: bold'";

        private static string style_th = " style='{0} font-weight:normal;padding:10px 5px;border-style:solid;border-width:0px;overflow:hidden;word-break:normal;border-color:#999;color:#fff;background-color:#26ADE4;border-top-width:1px;border-bottom-width:1px;'";

        private static string style_sub_table = "style='border-collapse:collapse;border-spacing:0;border-color:#999;margin-left:50px;font-size: smaller'";
        private static string style_sub_th = " style='{0} font-weight:normal;padding:5px 2px;border-style:solid;border-width:0px;overflow:hidden;word-break:normal;border-color:#999;color:#fff;background-color:#16A085;border-top-width:1px;border-bottom-width:1px;'";
        public const string columnHeaderStyle = "style = 'display: inline-block;padding: 2px 2px 2px 20px;color: #1478b1;text-align: right;width: 150px;font-weight: bold;'";
        public const string style = "style = 'display: inline-block;padding: 2px 2px 2px 20px;color: #1478b1;text-align: right;width: 150px;'";
        public const string styleBold = "style = 'font-weight: bold;text-align: left;width: 150px;margin-left: 20px'";
        public const string styleTable = "style = 'width:100%; font-family: Calibri, sans-serif; font-size: 14px; font-style: normal; font-variant: normal; font-weight: normal; margin: 5px; padding: 0px; border-collapse: collapse; border-spacing: 0px; color:" + "rgb(0, 0, 0); letter-spacing: -0.1px; text-align: left; text-decoration: none; vertical-align: baseline; background-color: rgba(0, 0, 0, 0);'";
        //private string server => _appSettings.EmailImageServer;
        public const string styleWarning = "style = 'font-weight: bold;color: #cc5500;'";
        public const string styleDanger = "style = 'font-weight: bold;color: red;'";
        #endregion

        private AppSettings _appSettings;

        public EmailMessage(IEmailService emailService, AppSettings appSettings)
        {
            _sb = new StringBuilder();
            _emailService = emailService;
            _appSettings = appSettings;
        }

        public void AddHeader(string title, string subtitle, string caption)
        {
            var msg =
                $"<html><body {style_body}><table><tr><td><img src='{_appSettings.EmailLogo}' /></td><td><h1>{_appSettings.EmailTitle.ToUpper()}</h1>" +
                $"<h3>{title}</h3><h4>{subtitle}</h4></td></tr></table>" +
                $"<hr /><br /><h3>{caption}</h3><br/>";
            _sb.Append(msg);
        }

        public void AddHeader(string subtitle, string caption)
        {
            AddHeader(_appSettings.EmailTitle, subtitle, caption);
        }
        public void AddSectionHeader(string title)
        {
            _sb.Append(SectionHeader(title));
        }

        public void AddLine()
        {
            _sb.Append("<hr/>");
        }
        public void AddBlank(int times = 1)
        {
            for (int i = 0; i < times; i++)
                _sb.Append("<p>&nbsp;</p>");
        }
        public void AddNewLine(int times = 1)
        {
            for (int i = 0; i < times; i++)
                _sb.Append("<br>");
        }
        public void AddText(string html, bool bold = false, bool newLine = false, bool highlight = false, bool italic = false, bool indent = false)
        {
            _sb.Append("<span style='");

            if (bold || highlight)
                _sb.Append("font-weight: bold;");

            if (highlight)
                _sb.Append("color: red;");

            if (italic)
                _sb.Append("font-style: italic;");

            if (indent)
                _sb.Append("margin-left:15px;");

            _sb.Append("'>");
            _sb.Append(html);
            _sb.Append("</span>");

            if (newLine)
                _sb.Append("<br>");

        }
        public void AddHighlightBlock(string text)
        {
            var html = $"<br>" +
                $"<span style = 'background-color:#1478b1;border:1px solid #444;border-radius:5px;color:#fff;padding:15px; display:inline-block;font-family:sans-serif;font-size:16px;font-weight: bold; text-align:center;text-decoration:none;width:600px;-webkit-text-size-adjust:none;mso-hide:all;' >" +
                   $"{text}" +
                   $"</span> <br>";
            _sb.Append(html);

        }

        public void AddTable(List<EmailColumn> colHeaders)
        {
            _sb.Append($"<table {style_table}><tr>");
            foreach (var col in colHeaders)
            {
                var s = string.Format(style_th, col.Width.HasValue ? $"width: {col.Width.Value}px;" : "");
                _sb.Append($"<td {s}>{col.Title}</td>");
            }
            _sb.Append("</tr>");

        }
        public void AddSubTable(List<EmailColumn> colHeaders)
        {
            _sb.Append($"<tr><td colspan='99'><table {style_sub_table}><tr>");
            foreach (var col in colHeaders)
            {
                var s = string.Format(style_sub_th, col.Width.HasValue ? $"width: {col.Width.Value}px;" : "");
                _sb.Append($"<td {s}>{col.Title}</td>");
            }
            _sb.Append("</tr>");

        }
        public void AddRow(List<object> colData, bool highlight = false)
        {
            if (highlight)
            {
                AddHighlightRow(colData);
            }
            else
            {
                _sb.Append($"<tr>");
                foreach (var col in colData)
                    _sb.Append($"<td {style_td}>{col}</td>");
                _sb.Append($"</tr>");
            }
        }
        public void AddHighlightRow(List<object> colData)
        {
            _sb.Append($"<tr>");
            foreach (var col in colData)
                _sb.Append($"<td {style_special_td}>{col}</td>");
            _sb.Append($"</tr>");
        }
        public void EndSubTable()
        {
            _sb.Append("</table></td></tr>");
        }
        public void EndTable()
        {
            _sb.Append("</table>");
        }

        public void AddChangeTable()
        {
            _sb.Append(
                $"<table><tr><td {styleBold}>&nbsp;</td><td {styleBold}>Old</td><td {styleBold}>New</td></tr>");
        }
        public void AddChangeRow<T>(object old, object newObj, Expression<Func<T>> propertyExpression)
        {
            var log = "";
            var property = ReflectionHelper.GetPropertyName(propertyExpression);
            var o = ReflectionHelper.GetPropertyValue(old, property);
            var n = ReflectionHelper.GetPropertyValue(newObj, property);
            var style =
                "style = 'display: inline-block;padding: 2px 2px 2px 20px;color: #1478b1;text-align: right;width: 150px;'";
            var styleBold = "style = 'font-weight: bold;text-align: left;width: 150px;margin-left: 20px'";
            var s = (o != null && o.Equals(n)) ? "" : styleBold;
            log += $"<tr><td {style}>{property}</td><td {s}>{o}</td><td {s}>{n}</td></tr>";
            ReflectionHelper.SetPropertyValue(old, property, n);
            _sb.Append(log);
        }
        public void AddChangeRow<T>(object old, object newObj, Expression<Func<T>> propertyExpressionOld, Expression<Func<T>> propertyExpressionNew)
        {
            var log = "";
            var property = ReflectionHelper.GetPropertyName(propertyExpressionOld);
            var o = ReflectionHelper.GetPropertyValue(old, property);
            var n = ReflectionHelper.GetPropertyValue(newObj, ReflectionHelper.GetPropertyName(propertyExpressionNew));
            var style =
                "style = 'display: inline-block;padding: 2px 2px 2px 20px;color: #1478b1;text-align: right;width: 150px;'";
            var styleBold = "style = 'font-weight: bold;text-align: left;width: 150px;margin-left: 20px'";
            var s = (o != null && o.Equals(n)) ? "" : styleBold;
            log += $"<tr><td {style}>{property}</td><td {s}>{o}</td><td {s}>{n}</td></tr>";
            ReflectionHelper.SetPropertyValue(old, property, n);
            _sb.Append(log);
        }
        public void AddChangeRowFixed<T>(object old, object newValue, Expression<Func<T>> propertyExpression)
        {
            var log = "";
            var property = ReflectionHelper.GetPropertyName(propertyExpression);
            var o = ReflectionHelper.GetPropertyValue(old, property);
            var style =
                "style = 'display: inline-block;padding: 2px 2px 2px 20px;color: #1478b1;text-align: right;width: 150px;'";
            var styleBold = "style = 'font-weight: bold;text-align: left;width: 150px;margin-left: 20px'";
            var s = (o != null && o.Equals(newValue)) ? "" : styleBold;
            log += $"<tr><td {style}>{property}</td><td {s}>{o}</td><td {s}>{newValue}</td></tr>";
            ReflectionHelper.SetPropertyValue(old, property, newValue);
            _sb.Append(log);
        }
        public void EndChangeTable()
        {
            _sb.Append($"</table>");
        }


        public void AddColumn(string title, string data)
        {
            _sb.Append(Column(title, data));
        }
        public void AddColumn(string title, decimal? data)
        {
            _sb.Append(Column(title, data));
        }
        public void AddColumn(string title, DateTime? data)
        {
            _sb.Append(Column(title, data));
        }
        public void AddColumn(string title, bool? data)
        {
            _sb.Append(Column(title, data));
        }

        public void AddLinkButton(string title, string key, string href)
        {
            _sb.Append(LinkButton(title, key, href));
        }
        public void AddLinkButton(string title, int key, string href)
        {
            _sb.Append(LinkButton(title, key, href));
        }

        //public void AddSurveyHeader(string url, string keyField)
        //{
        //    var href = $"{url}&{keyField}=";

        //    AddSectionHeader("How did we do?");

        //    _sb.Append($"<table align='center' border='0' cellpadding='0' cellspacing='0' dir='ltr'>" +
        //               $"<tr>" +
        //               $"<td><a href='{href}{5}' target='_blank'><img src='{server}happy.png' alt='happy' border='0'  style='display:block;height:auto' width='40'/></a></td>" +
        //               "<td bgcolor='#ffffff' width='30'>&nbsp;</td>" +
        //               $"<td><a href='{href}{3}' target='_blank'><img src='{server}neutral.png' alt='neutral' border='0' style='display:block;height:auto' width='40'/></a></td>" +
        //               "<td bgcolor='#ffffff' width='30'>&nbsp;</td>" +
        //               $"<td><a href='{href}{1}' target='_blank'><img src='{server}sad.png' alt='disappointed' border='0' style='display:block;height:auto' width='40'/></a></td>" +
        //               $"</tr></table>");
        //}


        public void AddSignature(string team)
        {
            _sb.Append(EmailSignature(team));
        }

        public string Text => _sb.ToString();

        public async Task<bool> Send(string to, string cc, string subject, string attachment = "")
        {
            bool rc = false;
            rc = await _emailService.SendAsync(to, cc, subject, _sb.ToString(), attachment);
            return rc;
        }
        public async Task<bool> Send(string to, string cc, string subject, string base64Data, string fileName)
        {
            bool rc = false;
            rc = await _emailService.SendAsync(to, cc, subject, _sb.ToString(), base64Data, fileName);
            return rc;
        }
        public async Task<bool> Send(List<string> to, List<string> cc, string subject, string attachment = "")
        {
            bool rc = false;
            rc = await _emailService.SendAsync(to, cc, subject, _sb.ToString(), attachment);
            return rc;
        }

        public async Task<bool> SendMonitoredEmail(string to, string cc, MonitorSubject monitorSubject, string attachment = "")
        {
            bool rc = false;
            rc = await _emailService.SendAsync(to, cc, monitorSubject.ToString(), _sb.ToString(), attachment);
            return rc;
        }

        private static string GetEmailHeader(string title, string subtitle, string caption)
        {
            var msg =
                $"<html><body><table><tr><td><img src='http://www.bseco.com/images/logos/newlogo.png' /></td><td><h1>BAY&nbsp;STATE&nbsp;ELEVATOR</h1>" +
                $"<h3>{title}</h3><h4>{subtitle}</h4></td></tr></table>" +
                $"<hr /><br /><h3>{caption}</h3><br/>";
            msg += $"<table><tr><td {styleBold}>&nbsp;</td><td {styleBold}>Old</td><td {styleBold}>New</td></tr>";

            return msg;
        }
        private static string EmailHeader(string title, string subtitle = "")
        {
            return $"<html><body><table><tr><td><img src='http://www.bseco.com/images/logos/newlogo.png' /></td><td><h1>BAY&nbsp;STATE&nbsp;ELEVATOR</h1>" +
                    $"<h3>{title}</h3><h4>{subtitle}</h4></td></tr></table>" +
                    $"<hr /><br />";

        }
        private static string LinkButton(string title, int key, string href)
        {
            return LinkButton(title, key.ToString(), href);
        }
        private static string LinkButton(string title, string key, string href)
        {
            return $"<br><br><a href='{href}'><span style = 'background-color:#EB7035;border:1px solid #EB7035;border-radius:3px;color:#ffffff;display:inline-block;font-family:sans-serif;font-size:16px;font-weight: bold; line-height:44px;text-align:center;text-decoration:none;width:200px;-webkit-text-size-adjust:none;mso-hide:all;' >{title}: {key}<br>click here to view</span></a> ";
        }
        private static string Column(string title, string data)
        {
            return ColumnHeader(title) + ColumnValue(data) + "<br>";
        }
        private static string Column(string title, decimal? data)
        {
            return ColumnHeader(title) + ColumnValue(data?.ToString() ?? "") + "<br>";
        }
        private static string Column(string title, bool? data)
        {
            return ColumnHeader(title) + ColumnValue(data?.ToString() ?? "") + "<br>";
        }
        private static string Column(string title, DateTime? data)
        {
            return ColumnHeader(title) + ColumnValue(data?.ToString("g") ?? "") + "<br>";
        }
        private static string SectionHeader(string title)
        {
            return $"<p>&nbsp;</p><h3>{title}</h3>";
        }
        private static string ColumnHeader(string title)
        {
            return $"<span {columnHeaderStyle} >{(string.IsNullOrWhiteSpace(title) ? "&nbsp;" : title)}:</span>";
        }
        private static string ColumnValue(string data)
        {
            return $"<span>{data}</span>";
        }

        private static string EmailSignature(string team)
        {
            var ret = "";
            switch (team)
            {
                default:
                    ret = EmailSignature(team.ToString(), "???", "???");
                    break;
            }
            return ret;
        }
        private static string EmailSignature(string teamMember, string phone, string email)
        {
            return
                "<table style = 'width:400px; font-family: Calibri, sans-serif; font-size: 11px; font-style: normal; font-variant: normal; font-weight: normal; margin: 5px; padding: 0px; border-collapse: collapse; border-spacing: 0px; color:" +
                "rgb(0, 0, 0); letter-spacing: -0.1px; text-align: left; text-decoration: none; vertical-align: baseline; background-color: rgba(0, 0, 0, 0);' > " +
                "<tbody>" + "<tr>" + "<td colspan='2' style='width: 100%'>" +
                "<span id='teammemberName' style='font-size: 11px; font-weight: bold; padding:5px'></span>" +
                "</td></tr>" +
                "<tr style='border-left: 1px solid rgb(13,64,106);border-right: 1px solid rgb(13,64,106);border-top: 1px solid rgb(13,64,106)'>" +
                "<td colspan='2' style='width: 100%; background: rgb(13,64,106); color: #fff; font-size: 11px ; padding:5px; margin: 0px;'>" +
                "Bay State Elevator Company" + "</td></tr>" +
                "<tr style='border-left: 1px solid rgb(13,64,106);border-right: 1px solid rgb(13,64,106)'>" +
                "<td colspan='2' style='width: 100%; font-size: 11px; font-weight:bold; color: rgb(13,64,106); padding:5px'>" +
                $"<span id='teammemberTitle'>{teamMember}</span>" + "</td>" + "</tr>" +
                "<tr style='border-left: 1px solid rgb(13,64,106);border-right: 1px solid rgb(13,64,106);'>" +
                "<!--logo-->" + "<td style='padding:11px'>" +
                "<img data-class='external' class='logo' id='LogoPlaceholder' nosend='1' src='http://www.bseco.com/images/logos/newlogo.png' alt='BSECO' title='Bay State Elevator Co.' style='width: 64px;" +
                "height: 60px;'>" + "</td>" + "<!--body-->" + "<td style='width: 90%'>" +
                "<table cellpadding='0' cellspacing='0' border='0' style='width: 100%; font-size: 11px;'>" + "<tbody>" +
                "<tr>" + "<td>" + "<b>p:&nbsp;</b>" + $"<span id='teammemberPhone'>{phone}</span>" + "</td>" + "</tr>" +
                "<tr>" + "<td>" + "<b>e:&nbsp;</b>" + $"<a id='teammemberEmail'>{email}</a>" + "</td>" + "</tr>" +
                "<tr>" + "<td>" + "<b>w:&nbsp;</b>" + "<a class='website' target='_blank' href='http://www.bseco.com'>" +
                "www.bseco.com" + "</a>" + "</td>" + "</tr>" + "<tr>" + "<td>" + "<b>f:&nbsp;</b>" +
                "<a class='website' target='_blank' title='Follow us on Facebook' href='https://www.facebook.com/baystateelevator'>" +
                "https://www.facebook.com/baystateelevator" + "</a>" + "</td>" + "</tr>" + "<tr>" + "<td>" +
                "<b>t:&nbsp;</b>" +
                "<a class='website' target='_blank' title='Follow us on Twitter' href='https://twitter.com/elevator_bse'>" +
                "https://twitter.com/elevator_bse" + "</a>" + "</td>" + "</tr>" + "<tr>" + "<td>" + "<b>l:&nbsp;</b>" +
                "<a class='website' target='_blank' title='Check us out LinkedIn' href='https://www.linkedin.com/company/bay-state-elevator-co'>" +
                "https://www.linkedin.com/company/bay-state-elevator-co" + "</a>" + "</td>" + "</tr>" + "</tbody>" +
                "</table>" + "</td>" + "</tr></tbody>" +
                "<tbody><tr style='border-left: 1px solid rgb(13,64,106);border-right: 1px solid rgb(13,64,106);border-bottom: 1px solid rgb(0,100,176)'>" +
                "<td colspan='2' style='width: 100%; font-size: 11px; font-weight:bold; color: rgb(0,100,176); padding:5px; text-align: right'>" +
                "Your Vertical Transportation Partner Since 1908" + "</td>" + "</tr>" + "</tbody></table>";
        }
    }

    public class EmailColumn
    {
        public EmailColumn(string title, int width)
        {
            Title = title;
            Width = width;
        }
        public EmailColumn(string title)
        {
            Title = title;
        }
        public int? Width { get; set; }
        public string Title { get; set; }
    }
}
