using QRCoder;
using System;
using System.Text;
using System.Web;

public static class TwoFactorHelper
{
    public static TwoFactorSetupResult Generate2FASetup(string issuer, string userEmail, string secret)
    {
        // URL encode parts
        string encodedIssuer = Uri.EscapeDataString(issuer);
        string encodedEmail = Uri.EscapeDataString(userEmail);

        // Create otpauth URI
        string otpauthUri = $"otpauth://totp/{encodedIssuer}:{encodedEmail}" +
                            $"?secret={secret}&issuer={encodedIssuer}&digits=6";

        // Generate QR Code (Base64 PNG)
        using var qrGenerator = new QRCodeGenerator();
        var qrCodeData = qrGenerator.CreateQrCode(otpauthUri, QRCodeGenerator.ECCLevel.Q);
        var qrCode = new PngByteQRCode(qrCodeData);
        var qrCodeBytes = qrCode.GetGraphic(20);
        var qrCodeBase64 = Convert.ToBase64String(qrCodeBytes);

        return new TwoFactorSetupResult
        {
            SharedKey = secret,
            AuthenticatorUri = otpauthUri,
            QrCodeImageBase64 = $"data:image/png;base64,{qrCodeBase64}"
        };
    }
}

public class TwoFactorSetupResult
{
    public string SharedKey { get; set; }
    public string AuthenticatorUri { get; set; }
    public string QrCodeImageBase64 { get; set; }
}
