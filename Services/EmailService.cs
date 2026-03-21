using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace CosmeticStoreManagement.Services;

public class EmailSettings
{
    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string SenderName { get; set; } = "Cosmetic Store";

    public string SenderEmail { get; set; } = string.Empty;

    public string SenderPassword { get; set; } = string.Empty;
}

public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IConfiguration configuration)
    {
        var section = configuration.GetSection("EmailSettings");
        _settings = new EmailSettings
        {
            Host = section["Host"] ?? string.Empty,
            Port = int.TryParse(section["Port"], out var port) ? port : 587,
            EnableSsl = bool.TryParse(section["EnableSsl"], out var enableSsl) ? enableSsl : true,
            SenderName = section["SenderName"] ?? "Cosmetic Store",
            SenderEmail = section["SenderEmail"] ?? string.Empty,
            SenderPassword = section["SenderPassword"] ?? string.Empty
        };
    }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(_settings.Host) &&
        _settings.Port > 0 &&
        !string.IsNullOrWhiteSpace(_settings.SenderEmail) &&
        !string.IsNullOrWhiteSpace(_settings.SenderPassword);

    public async Task SendThankYouEmailAsync(string customerEmail, string customerName, int orderId, int rating, string comment)
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("Email settings are not configured.");
        }

        using var message = new MailMessage();
        message.From = new MailAddress(_settings.SenderEmail, _settings.SenderName);
        message.To.Add(customerEmail);
        message.Subject = $"Thank you for your feedback on order #{orderId}";
        message.Body =
$@"Dear {customerName},

Thank you for shopping with Cosmetic Store.

We sincerely appreciate your feedback for order #{orderId}.
Rating: {rating}/5
Comment: {comment}

Your feedback helps us improve our products and service.

Best regards,
Cosmetic Store";

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.SenderEmail, _settings.SenderPassword)
        };

        await client.SendMailAsync(message);
    }
}
