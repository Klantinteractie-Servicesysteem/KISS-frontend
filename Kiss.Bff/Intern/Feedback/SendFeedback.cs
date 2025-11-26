using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using Duende.IdentityModel;
using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kiss.Bff.Feedback
{
    [ApiController]
    public class SendFeedback : ControllerBase
    {
        private static readonly HtmlSanitizer s_sanitizer = new();
        private readonly SmtpClient _smtpClient;
        private readonly IConfiguration _configuration;

        public SendFeedback(SmtpClient smtpClient, IConfiguration configuration)
        {
            _smtpClient = smtpClient;
            _configuration = configuration;
        }

        [HttpPost("api/feedback")]
        [Authorize(Policy = Policies.KcmOrRedactieOrKennisbankPolicy)]
        public async Task<IActionResult> Post(FeedbackModel model, CancellationToken token)
        {
            var from = _configuration["FEEDBACK_EMAIL_FROM"];
            var to = _configuration["FEEDBACK_EMAIL_TO"];

            var userEmail = User.FindFirstValue(JwtClaimTypes.Email) ?? User.FindFirstValue(JwtClaimTypes.PreferredUserName);
            var userName = User?.Identity?.Name ?? userEmail;
            var name = s_sanitizer.Sanitize(model.Name);
            var subject = $"KISS feedback: {name}";

            var body = BuildEmailBody(userName, model);
            using var message = new MailMessage(from, to, subject, body)
            {
                IsBodyHtml = true
            };

            await _smtpClient.SendMailAsync(message, token);
            return Ok();
        }

        public static string BuildEmailBody(string userName, FeedbackModel model)
        {
            var stringBuilder = new StringBuilder(@"
                <!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""https://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
                <html xmlns=""https://www.w3.org/1999/xhtml"">
                <head>
                <title>Test Email Sample</title>
                <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"" />
                <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
                <meta name=""viewport"" content=""width=device-width, initial-scale=1.0 "" />
                <style>

                </style>
                </head>
                <body>
                <p>
                ");

            stringBuilder
                .Append(s_sanitizer.Sanitize(userName))
                .Append(" heeft feedback gegeven op: <a href=\"")
                .Append(s_sanitizer.Sanitize(model.Topic))
                .Append("\">")
                .Append(s_sanitizer.Sanitize(model.Name))
                .Append("</a></p>");

            var sectionCount = 0;
            foreach (var section in model.Sections)
            {
                sectionCount++;
                if (sectionCount >= 10) break;
                stringBuilder.Append("<dl>");
                var itemCount = 0;
                foreach (var item in section)
                {
                    itemCount++;
                    if (itemCount >= 10) break;
                    stringBuilder.Append(itemCount == 1 ? "<dt>" : "<dd>");
                    stringBuilder.Append(s_sanitizer.Sanitize(item));
                    stringBuilder.Append(itemCount == 1 ? "</dt>" : "</dd>");
                }
                stringBuilder.Append("</dl>");
            }
            stringBuilder.Append("<p>Met vriendelijke groet,</p><p>KISS</p></body></html>");

            return stringBuilder.ToString();
        }
    }

    public record FeedbackModel([Required] string Topic, [Required] string Name, [Required] string[][] Sections);
}
