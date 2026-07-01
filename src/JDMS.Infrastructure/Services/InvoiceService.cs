using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace JDMS.Infrastructure.Services;

public class InvoiceService : IInvoiceService
{
    private const float ThermalWidthPt = 215f;
    private const float ThermalLogoMaxHeightPt = 28f;

    private readonly IUnitOfWork _unitOfWork;
    private readonly ICompanySettingsService _companySettingsService;
    private readonly IWebHostEnvironment _environment;

    public InvoiceService(
        IUnitOfWork unitOfWork,
        ICompanySettingsService companySettingsService,
        IWebHostEnvironment environment)
    {
        _unitOfWork = unitOfWork;
        _companySettingsService = companySettingsService;
        _environment = environment;
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<int> CreateInvoiceForOrderAsync(int orderId, CancellationToken cancellationToken = default)
    {
        var existing = await _unitOfWork.Invoices.Query().FirstOrDefaultAsync(i => i.OrderId == orderId, cancellationToken);
        if (existing != null) return existing.Id;

        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken)
            ?? throw new InvalidOperationException("الطلب غير موجود");

        var invoice = new Invoice
        {
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{orderId:D6}",
            OrderId = orderId,
            InvoiceDate = DateTime.UtcNow,
            Subtotal = order.Subtotal,
            DeliveryFee = order.DeliveryFee,
            Discount = order.Discount,
            Tax = order.Tax,
            GrandTotal = order.GrandTotal
        };
        await _unitOfWork.Invoices.AddAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return invoice.Id;
    }

    public async Task<byte[]> GeneratePdfAsync(int orderId, bool thermal = false, CancellationToken cancellationToken = default)
    {
        await CreateInvoiceForOrderAsync(orderId, cancellationToken);

        var order = await _unitOfWork.Orders.Query()
            .Include(o => o.Customer)
            .Include(o => o.Address).ThenInclude(a => a.Governorate)
            .Include(o => o.Address).ThenInclude(a => a.Area)
            .Include(o => o.OrderDetails).ThenInclude(d => d.Product)
            .Include(o => o.Invoice)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new InvalidOperationException("الطلب غير موجود");

        var company = await _unitOfWork.CompanySettings.Query().FirstOrDefaultAsync(cancellationToken);
        var logoPath = _companySettingsService.ResolvePhysicalLogoPath(company?.LogoPath, _environment.WebRootPath);
        var hasLogo = File.Exists(logoPath);

        var qrPayload = $"JDMS|{order.OrderNumber}|{order.GrandTotal:N2}";
        var qrBytes = InvoiceQrHelper.GeneratePng(qrPayload);

        var document = thermal
            ? BuildThermalDocument(order, company, logoPath, hasLogo, qrBytes)
            : BuildA4Document(order, company, logoPath, hasLogo, qrBytes);

        using var stream = new MemoryStream();
        document.GeneratePdf(stream);
        return stream.ToArray();
    }

    private static Document BuildThermalDocument(Order order, CompanySettings? company, string logoPath, bool hasLogo, byte[] qrBytes)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.ContinuousSize(ThermalWidthPt);
                page.MarginVertical(8);
                page.MarginHorizontal(6);
                page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Spacing(2);

                    if (hasLogo)
                    {
                        col.Item().AlignCenter().MaxHeight(ThermalLogoMaxHeightPt)
                            .Image(logoPath).FitArea();
                    }

                    col.Item().AlignCenter().Text(company?.CompanyName ?? "نظام إدارة التوصيل الأردني")
                        .Bold().FontSize(9);

                    if (!string.IsNullOrWhiteSpace(company?.Address))
                        col.Item().AlignCenter().Text(company.Address).FontSize(7);
                    if (!string.IsNullOrWhiteSpace(company?.Phone))
                        col.Item().AlignCenter().Text($"هاتف: {company.Phone}").FontSize(7);

                    col.Item().PaddingVertical(3).LineHorizontal(0.5f);

                    col.Item().Text($"فاتورة: {order.Invoice?.InvoiceNumber}").FontSize(7);
                    col.Item().Text($"تاريخ: {order.Invoice?.InvoiceDate:yyyy-MM-dd}").FontSize(7);
                    col.Item().Text($"طلب: {order.OrderNumber}").FontSize(7);

                    col.Item().PaddingTop(3).Text("بيانات العميل").Bold().FontSize(8);
                    col.Item().Text($"الاسم: {order.Customer.FullName}");
                    col.Item().Text($"الهاتف: {order.Customer.MobileNumber}");
                    col.Item().Text($"العنوان: {order.Address.Governorate.NameAr} - {order.Address.Area.NameAr}");
                    if (!string.IsNullOrWhiteSpace(order.Address.Street))
                        col.Item().Text(order.Address.Street);

                    if (!string.IsNullOrWhiteSpace(order.Address.DeliveryNotes))
                        col.Item().Text($"توصيل: {order.Address.DeliveryNotes.Trim()}").FontSize(7);

                    if (!string.IsNullOrWhiteSpace(order.Notes))
                    {
                        col.Item().PaddingTop(3).Text("ملاحظات الطلب").Bold();
                        col.Item().Text(order.Notes.Trim());
                    }

                    col.Item().PaddingVertical(3).LineHorizontal(0.5f);

                    foreach (var d in order.OrderDetails)
                    {
                        col.Item().Text(text =>
                        {
                            text.Span($"{d.Product.ProductName} ");
                            text.Span($"x{d.Quantity} = {d.LineTotal:N2} د.أ");
                        });
                    }

                    col.Item().PaddingTop(4).LineHorizontal(0.5f);
                    col.Item().Text($"فرعي: {order.Subtotal:N2} د.أ");
                    col.Item().Text($"توصيل: {order.DeliveryFee:N2} د.أ");
                    col.Item().Text($"خصم: {order.Discount:N2} د.أ");
                    col.Item().Text($"ضريبة: {order.Tax:N2} د.أ");
                    col.Item().Text($"الإجمالي: {order.GrandTotal:N2} د.أ").Bold().FontSize(9);
                    col.Item().PaddingTop(4).AlignCenter().Width(56).Height(56).Image(qrBytes);
                    col.Item().AlignCenter().Text(order.OrderNumber).FontSize(6);
                });
            });
        });
    }

    private static Document BuildA4Document(Order order, CompanySettings? company, string logoPath, bool hasLogo, byte[] qrBytes)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    col.Item().Row(row =>
                    {
                        row.RelativeItem().AlignRight().Column(c =>
                        {
                            c.Item().Text(company?.CompanyName ?? "نظام إدارة التوصيل الأردني").Bold().FontSize(18);
                            if (!string.IsNullOrWhiteSpace(company?.Address))
                                c.Item().Text(company.Address);
                            if (!string.IsNullOrWhiteSpace(company?.Phone))
                                c.Item().Text($"هاتف: {company.Phone}");
                            if (!string.IsNullOrWhiteSpace(company?.Email))
                                c.Item().Text(company.Email);
                        });

                        if (hasLogo)
                        {
                            row.ConstantItem(72).MaxHeight(55)
                                .Image(logoPath).FitArea();
                        }
                    });

                    col.Item().PaddingVertical(15).LineHorizontal(1);
                    col.Item().Text($"فاتورة: {order.Invoice?.InvoiceNumber} | تاريخ: {order.Invoice?.InvoiceDate:yyyy-MM-dd} | طلب: {order.OrderNumber}").FontSize(10);
                    col.Item().PaddingTop(10).Text("بيانات العميل").Bold();
                    col.Item().Text($"الاسم: {order.Customer.FullName}");
                    col.Item().Text($"الهاتف: {order.Customer.MobileNumber}");
                    col.Item().Text($"العنوان: {order.Address.Governorate.NameAr} - {order.Address.Area.NameAr} - {order.Address.Street}");

                    if (!string.IsNullOrWhiteSpace(order.Address.DeliveryNotes))
                    {
                        col.Item().PaddingTop(6).Text("ملاحظات التوصيل").Bold();
                        col.Item().Text(order.Address.DeliveryNotes.Trim());
                    }

                    if (!string.IsNullOrWhiteSpace(order.Notes))
                    {
                        col.Item().PaddingTop(8).Text("ملاحظات الطلب").Bold();
                        col.Item().Text(order.Notes.Trim()).Italic();
                    }

                    col.Item().PaddingVertical(10);
                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(c =>
                        {
                            c.RelativeColumn(3);
                            c.ConstantColumn(50);
                            c.ConstantColumn(70);
                            c.ConstantColumn(70);
                        });
                        table.Header(h =>
                        {
                            h.Cell().Text("المنتج").Bold();
                            h.Cell().Text("الكمية").Bold();
                            h.Cell().Text("السعر").Bold();
                            h.Cell().Text("الإجمالي").Bold();
                        });
                        foreach (var d in order.OrderDetails)
                        {
                            table.Cell().Text(d.Product.ProductName);
                            table.Cell().Text(d.Quantity.ToString());
                            table.Cell().Text(d.UnitPrice.ToString("N2"));
                            table.Cell().Text(d.LineTotal.ToString("N2"));
                        }
                    });

                    col.Item().PaddingTop(15).Row(row =>
                    {
                        row.RelativeItem().AlignRight().Column(summary =>
                        {
                            summary.Item().Text($"المجموع الفرعي: {order.Subtotal:N2} د.أ");
                            summary.Item().Text($"رسوم التوصيل: {order.DeliveryFee:N2} د.أ");
                            summary.Item().Text($"الخصم: {order.Discount:N2} د.أ");
                            summary.Item().Text($"الضريبة: {order.Tax:N2} د.أ");
                            summary.Item().Text($"الإجمالي: {order.GrandTotal:N2} د.أ").Bold().FontSize(14);
                            summary.Item().Text($"الدفع: {PaymentMethodLabel(order.PaymentMethod)}");
                            if (order.AmountReceived > 0)
                                summary.Item().Text($"المستلم: {order.AmountReceived:N2} | الباقي: {order.ChangeDue:N2}");
                        });
                        row.ConstantItem(90).AlignCenter().Column(qr =>
                        {
                            qr.Item().Width(80).Height(80).Image(qrBytes);
                            qr.Item().AlignCenter().Text(order.OrderNumber).FontSize(8);
                        });
                    });
                });
            });
        });
    }

    private static string PaymentMethodLabel(Domain.Enums.PaymentMethod method) => method switch
    {
        Domain.Enums.PaymentMethod.CreditCard => "بطاقة ائتمان",
        Domain.Enums.PaymentMethod.BankTransfer => "تحويل بنكي",
        Domain.Enums.PaymentMethod.OnlinePayment => "دفع إلكتروني",
        _ => "نقدي"
    };
}
