namespace Ttc.Model.Core;

public class EmailConfig
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string EmailFromName { get; set; } = "";
    public string EmailFrom { get; set; } = "";
    

    public override string ToString() => $"From={EmailFrom}";
}
