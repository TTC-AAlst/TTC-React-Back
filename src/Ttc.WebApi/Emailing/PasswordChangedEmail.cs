namespace Ttc.WebApi.Emailing;

public class PasswordChangedEmail
{
    private readonly EmailService _emailService;

    private const string NewPasswordRequestTemplate = @"
Je paswoord is aangepast!<br>
Als je dit niet zelf gedaan hebt, dan is er iets mis!<br>
";

    public PasswordChangedEmail(EmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Email(string email)
    {
        const string subject = "Nieuw paswoord TTC Aalst";
        string content = string.Format(NewPasswordRequestTemplate);
        await _emailService.SendEmail(email, subject, content);
    }
}
