namespace KTALearningHub.API.Models;

public class PlatformSettings
{
    public Guid Id { get; set; }
    public string? LogoUrl { get; set; }
    public string PlatformName { get; set; } = "KTA Learning Hub";
    public string PrimaryColor { get; set; } = "#4F46E5";
    public string? EmailSettingsJson { get; set; }       // JSON: SMTP host, port, credentials
    public string? PaymentGatewayJson { get; set; }      // JSON: Gateway config (Paystack/Flutterwave)
    public string? NotificationSettingsJson { get; set; } // JSON: Notification preferences
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
