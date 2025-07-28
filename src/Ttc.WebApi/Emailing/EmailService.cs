using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System.Globalization;
using Ttc.DataEntities.Core;
using Ttc.Model.Core;
using Ttc.Model.Players;

namespace Ttc.WebApi.Emailing;

public class EmailService
{
    private static readonly CultureInfo Culture = new("nl-BE");
    private readonly EmailConfig _config;
    private readonly ITtcDbContext _context;

    public EmailService(EmailConfig config, ITtcDbContext context)
    {
        _config = config;
        _context = context;
    }

    public async Task SendEmail(ICollection<Player> players, WeekCompetitionEmailModel email)
    {
        //var message = new MimeMessage();
        //message.From.Add(new MailboxAddress(_config.EmailFromName, _config.EmailFrom));
        //var toEmails = players
        //    .Where(ply => !string.IsNullOrWhiteSpace(ply.Contact?.Email))
        //    .Select(ply => new MailboxAddress(ply.FirstName + " " + ply.LastName, ply.Contact!.Email));
        //message.To.AddRange(toEmails);
        //message.Subject = email.Title;


        string body = email.Email;
        var tournaments = await _context.Tournaments
            .Where(x => x.Date >= DateTime.Today && x.Date <= DateTime.Today.AddDays(30))
            .OrderBy(x => x.Date)
            .ToArrayAsync();

        if (tournaments.Any())
        {
            string tournamentInfo = "";
            tournamentInfo += "<br><br>";
            tournamentInfo += "<b>Toernooitje doen?</b><br><ul>";
            foreach (var tournament in tournaments)
            {
                tournamentInfo += $"<li>{tournament.Date.ToString("ddd dd/MM/yyyy", Culture)}: {tournament.Name}</li>";
            }
            tournamentInfo += "</ul>";
            body = body.Replace("{{tournament-info}}", tournamentInfo);
        }
        else
        {
            body = body.Replace("{{tournament-info}}", "");
        }


        // using var client = new SmtpClient(new ProtocolLogger(Console.OpenStandardOutput()));
        using var client = new SmtpClient();
        await client.ConnectAsync(_config.Host, _config.Port, SecureSocketOptions.Auto);
        await client.AuthenticateAsync(_config.UserName, _config.Password);
        
        foreach (var player in players)
        {
            string customContent;
            if (email.Players.TryGetValue(player.Id, out string? team))
            {
                customContent = body.Replace("{{player-info}}", $"<br>Proficiat {player.FirstName}! Je bent opgesteld in {team}. Succes!<br>");
            }
            else
            {
                customContent = body.Replace("{{player-info}}", "");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_config.EmailFromName, _config.EmailFrom));
            var toEmails = players
                .Where(ply => !string.IsNullOrWhiteSpace(ply.Contact?.Email))
                .Select(ply => new MailboxAddress(ply.FirstName + " " + ply.LastName, ply.Contact!.Email));
            message.To.AddRange(toEmails);
            message.Subject = email.Title;
            message.Body = new TextPart("html")
            {
                Text = customContent
            };

            await client.SendAsync(message);
        }

        await client.DisconnectAsync(true);
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
        await client.ConnectAsync(_config.Host, _config.Port, SecureSocketOptions.SslOnConnect);
        await client.AuthenticateAsync(_config.UserName, _config.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
