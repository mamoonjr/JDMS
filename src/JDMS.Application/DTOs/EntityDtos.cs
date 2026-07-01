using System.ComponentModel.DataAnnotations;
using JDMS.Application.Constants;

namespace JDMS.Application.DTOs;

public class CustomerDto
{
    public int Id { get; set; }
    public string CustomerCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? SecondaryMobile { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CustomerCreateDto
{
    public string FullName { get; set; } = string.Empty;
    public string MobileNumber { get; set; } = string.Empty;
    public string? SecondaryMobile { get; set; }
    public string? Email { get; set; }
    public string? Notes { get; set; }
}

public class ProductDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? ImagePath { get; set; }
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; }
}

public class ProductCreateDto
{
    public string ProductName { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public string? Barcode { get; set; }
    public string? ImagePath { get; set; }
    public string? Description { get; set; }
    public decimal UnitPrice { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GovernorateDto
{
    public int Id { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
}

public class AreaDto
{
    public int Id { get; set; }
    public int GovernorateId { get; set; }
    public string NameAr { get; set; } = string.Empty;
    public decimal DeliveryFee { get; set; }
}

public class AddressDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int GovernorateId { get; set; }
    public int AreaId { get; set; }
    public string GovernorateName { get; set; } = string.Empty;
    public string AreaName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? Apartment { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? GoogleMapsLink { get; set; }
    public bool IsDefault { get; set; }
}

public class AddressCreateDto
{
    public int CustomerId { get; set; }
    public int GovernorateId { get; set; }
    public int AreaId { get; set; }
    public string Neighborhood { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string? Building { get; set; }
    public string? Apartment { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public string? GoogleMapsLink { get; set; }
    public bool IsDefault { get; set; }
}

public class DriverDto
{
    public int Id { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int VehicleType { get; set; }
    public string VehicleTypeName { get; set; } = string.Empty;
    public int? AssignedAreaId { get; set; }
    public string? AssignedAreaName { get; set; }
    public bool IsActive { get; set; }
    public int TotalDeliveries { get; set; }
    public int CompletedDeliveries { get; set; }
}

public class DriverCreateDto
{
    public string DriverName { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public int VehicleType { get; set; }
    public int? AssignedAreaId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class DeliveryTrackingDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int DriverId { get; set; }
    public string DriverName { get; set; } = string.Empty;
    public DateTime AssignDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? DeliveryNotes { get; set; }
}

public class DeliveryAssignDto
{
    public int OrderId { get; set; }
    public int DriverId { get; set; }
    public string? DeliveryNotes { get; set; }
}

public class OrderListDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? PaymentMethodName { get; set; }
    public string? AddressSummary { get; set; }
    public string? ProductsSummary { get; set; }
    public int ItemCount { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public string? Notes { get; set; }
}

public class OrderFilterDto
{
    public string? Search { get; set; }
    public int? Status { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class OrderDetailLineDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
    public decimal LineTotal { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public int AddressId { get; set; }
    public string AddressSummary { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public decimal Subtotal { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal GrandTotal { get; set; }
    public List<OrderDetailLineDto> Details { get; set; } = new();
}

public class OrderDetailInputDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Discount { get; set; }
}

public class OrderCreateDto
{
    public int CustomerId { get; set; }
    public int AddressId { get; set; }
    public DateTime? DeliveryDate { get; set; }
    public int Status { get; set; }

    [MaxLength(500, ErrorMessage = "ملاحظات الطلب يجب ألا تتجاوز 500 حرف")]
    [Display(Name = "ملاحظات الطلب")]
    public string? Notes { get; set; }

    public decimal OrderDiscount { get; set; }
    public decimal DeliveryFee { get; set; }
    public List<OrderDetailInputDto> Details { get; set; } = new();
}

public class ReportFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? GovernorateId { get; set; }
    public int? AreaId { get; set; }
    public int? DriverId { get; set; }
    public int? OrderStatus { get; set; }
    public string ReportType { get; set; } = "orders";
}

public class ReportDataDto
{
    public string Title { get; set; } = string.Empty;
    public List<Dictionary<string, object?>> Rows { get; set; } = new();
    public decimal TotalAmount { get; set; }
    public int TotalCount { get; set; }
}

public class UserViewModel
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string RoleDisplayAr { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class UserCreateViewModel
{
    [Required(ErrorMessage = "اسم الدخول مطلوب")]
    [MaxLength(50)]
    [Display(Name = "اسم الدخول")]
    public string UserName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "البريد غير صالح")]
    [Display(Name = "البريد الإلكتروني")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [MaxLength(200)]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(6, ErrorMessage = "كلمة المرور 6 أحرف على الأقل")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة المرور")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
    [Compare(nameof(Password), ErrorMessage = "كلمة المرور غير متطابقة")]
    [DataType(DataType.Password)]
    [Display(Name = "تأكيد كلمة المرور")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "يجب اختيار صلاحية المستخدم")]
    [Display(Name = "الصلاحية / الدور")]
    public string Role { get; set; } = Roles.Employee;
}

public class UserEditViewModel
{
    public string Id { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم الدخول مطلوب")]
    [MaxLength(50)]
    [Display(Name = "اسم الدخول")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "البريد غير صالح")]
    [Display(Name = "البريد الإلكتروني")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [MaxLength(200)]
    [Display(Name = "الاسم الكامل")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "يجب اختيار صلاحية المستخدم")]
    [Display(Name = "الصلاحية / الدور")]
    public string Role { get; set; } = string.Empty;

    [Display(Name = "نشط")]
    public bool IsActive { get; set; } = true;

    [MinLength(6, ErrorMessage = "كلمة المرور 6 أحرف على الأقل")]
    [DataType(DataType.Password)]
    [Display(Name = "كلمة مرور جديدة")]
    public string? NewPassword { get; set; }

    [Compare(nameof(NewPassword), ErrorMessage = "كلمة المرور غير متطابقة")]
    [DataType(DataType.Password)]
    [Display(Name = "تأكيد كلمة المرور الجديدة")]
    public string? ConfirmNewPassword { get; set; }
}
