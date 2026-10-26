using MailKit.Net.Smtp;
using MimeKit;

namespace MyChat.Services;

public class EmailService
{
    public async Task SendEmailAsync(string email, string subject, string message)
    {
        MimeMessage emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("MyChat", "example@gmail.com"));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;

        emailMessage.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = message
        };

        using SmtpClient client = new SmtpClient();

        await client.ConnectAsync("smtp.gmail.com", 465, true);

        await client.AuthenticateAsync(
            "example@gmail.com",
            "app_password"
        );

        await client.SendAsync(emailMessage);
        await client.DisconnectAsync(true);
    }
}