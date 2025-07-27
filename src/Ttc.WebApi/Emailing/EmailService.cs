using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Ttc.Model.Core;
using Ttc.Model.Players;

namespace Ttc.WebApi.Emailing;

public class EmailService
{
    private readonly EmailConfig _config;

    public EmailService(EmailConfig config)
    {
        _config = config;
    }

    public async Task SendEmail(IEnumerable<Player> players, WeekCompetitionEmailModel email)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config.EmailFromName, _config.EmailFrom));
        var toEmails = players
            .Where(ply => !string.IsNullOrWhiteSpace(ply.Contact?.Email))
            .Select(ply => new MailboxAddress(ply.FirstName + " " + ply.LastName, ply.Contact!.Email));
        message.To.AddRange(toEmails);
        message.Subject = email.Title;
        message.Body = new TextPart("html")
        {
            Text = email.Email.Replace("{{player-info}}", "")
        };


        using var client = new SmtpClient();
        await client.ConnectAsync(_config.Host, _config.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_config.UserName, _config.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

        // TODO: Send individual emails? Per team? Or one global one?
        //foreach (var player in players)
        //{
        //    string customContent;
        //    if (email.Players.TryGetValue(player.Id, out string team))
        //    {
        //        customContent = email.Email.Replace("{{player-info}}", $"<br>Yaye! Je bent opgesteld in {team}. Succes!<br>");
        //    }
        //    else
        //    {
        //        customContent = email.Email.Replace("{{player-info}}", "");
        //    }
        //}
    }

    public async Task SendEmail(string email, string subject, string content)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_config.EmailFromName, _config.EmailFrom));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;
        message.Body = new TextPart("html")
        {
            Text = content
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_config.Host, _config.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_config.UserName, _config.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
