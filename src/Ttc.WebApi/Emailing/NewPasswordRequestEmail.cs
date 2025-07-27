namespace Ttc.WebApi.Emailing;

public class NewPasswordRequestEmail
{
    private readonly EmailService _emailService;

    private const string NewPasswordRequestTemplate = @"
Reset je paswoord hier:<br>
<a href='{0}'>{0}</a>
";

    public NewPasswordRequestEmail(EmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Email(string email, Guid guid)
    {
        const string subject = "Paswoord reset TTC Aalst";
        string fullUrlLink = "https://ttc-aalst.be/login/nieuw-paswoord/" + guid;
        string content = string.Format(NewPasswordRequestTemplate, fullUrlLink);
        await _emailService.SendEmail(email, subject, content);
    }
}
