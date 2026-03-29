using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(string toEmail, string subject, string body)
    {
        try
        {
            Console.WriteLine("📧 Starting email send...");
            Console.WriteLine($"Host: {_settings.Host}");
            Console.WriteLine($"Port: {_settings.Port}");
            Console.WriteLine($"Username: {_settings.Username}");
            Console.WriteLine($"To: {toEmail}");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            Console.WriteLine("✅ Message object created");

            using var client = new SmtpClient();

            Console.WriteLine("🔌 Connecting to SMTP...");
            await client.ConnectAsync(_settings.Host, _settings.Port,
                MailKit.Security.SecureSocketOptions.StartTls);
            Console.WriteLine("✅ Connected!");

            Console.WriteLine("🔐 Authenticating...");
            await client.AuthenticateAsync(_settings.Username, _settings.Password);
            Console.WriteLine("✅ Authenticated!");

            Console.WriteLine("📤 Sending email...");
            await client.SendAsync(message);
            Console.WriteLine("✅ Email sent successfully to: " + toEmail);

            await client.DisconnectAsync(true);
            Console.WriteLine("🔒 Disconnected.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ EMAIL FAILED!");
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Details: {ex.InnerException?.Message}");
        }
    }
}