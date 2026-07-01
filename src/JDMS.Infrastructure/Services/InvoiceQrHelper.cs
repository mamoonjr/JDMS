using QRCoder;

namespace JDMS.Infrastructure.Services;

internal static class InvoiceQrHelper
{
    public static byte[] GeneratePng(string payload, int pixelsPerModule = 4)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
        var qr = new PngByteQRCode(data);
        return qr.GetGraphic(pixelsPerModule);
    }
}
