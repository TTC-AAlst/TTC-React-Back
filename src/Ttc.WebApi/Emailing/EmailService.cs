using SendGrid;
using SendGrid.Helpers.Mail;
using System.Text.RegularExpressions;
using Ttc.DataAccess.Utilities;
using Ttc.Model.Core;
using Ttc.Model.Players;

namespace Ttc.WebApi.Emailing;

// SendGrid API Example usage:
// https://github.com/sendgrid/sendgrid-csharp/blob/master/USE_CASES.md
public class EmailService
{
    private readonly TtcLogger _logger;

    public EmailService(TtcLogger logger)
    {
        _logger = logger;
    }

    public async Task SendEmail(IEnumerable<Player> players, WeekCompetitionEmailModel email, EmailConfig config)
    {
        var client = new SendGridClient(config.SendGridApiKey);
        var from = new EmailAddress(config.EmailFrom);

        foreach (var player in players)
        {
            var to = new EmailAddress(player.Contact.Email, player.FirstName + " " + player.LastName);

            string customContent;
            if (email.Players.TryGetValue(player.Id, out string team))
            {
                customContent = email.Email.Replace("{{player-info}}", $"<br>Yaye! Je bent opgesteld in {team}. Succes!<br>");
            }
            else
            {
                customContent = email.Email.Replace("{{player-info}}", "");
            }
            
            string plainContent = Regex.Replace(customContent.Replace("<br>", Environment.NewLine), "<.*?>", "");
            var msg = MailHelper.CreateSingleEmail(from, to, email.Title, plainContent, customContent);
            var response = await client.SendEmailAsync(msg);
            await CheckSendGridResponse(email.Title, response);
        }
    }

    public async Task SendEmail(string email, string subject, string content, EmailConfig config)
    {
        var client = new SendGridClient(config.SendGridApiKey);
        var from = new EmailAddress(config.EmailFrom);
        var to = new EmailAddress(email);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, content, content);
        var response = await client.SendEmailAsync(msg);
        await CheckSendGridResponse(subject, response);
    }

    private async Task CheckSendGridResponse(string subject, Response response)
    {
        string statusCode = response.StatusCode.ToString();
        if (statusCode != "Accepted")
        {
            string troubles = await response.Body.ReadAsStringAsync();
            _logger.Error($"SendEmail {subject} returned {statusCode}{Environment.NewLine}{troubles}");
            throw new Exception("Error sending email:" + Environment.NewLine + troubles);
        }
    }
}
