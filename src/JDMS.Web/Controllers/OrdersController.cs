using JDMS.Application.Constants;

using JDMS.Application.DTOs;

using JDMS.Application.Interfaces;

using JDMS.Domain.Enums;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;



namespace JDMS.Web.Controllers;



[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager},{Roles.Employee}")]

public class OrdersController : Controller

{

    private readonly IOrderService _orderService;

    private readonly IUnitOfWork _unitOfWork;



    public OrdersController(IOrderService orderService, IUnitOfWork unitOfWork)

    {

        _orderService = orderService;

        _unitOfWork = unitOfWork;

    }



    public async Task<IActionResult> Index(OrderFilterDto filter, CancellationToken cancellationToken)

    {

        ViewBag.Filter = filter;

        ViewBag.StatusOptions = GetStatusOptions();

        var orders = await _orderService.GetFilteredAsync(filter, cancellationToken);

        return View(orders);

    }



    [HttpGet]

    public async Task<IActionResult> ExportExcel(OrderFilterDto filter, CancellationToken cancellationToken)

    {

        var bytes = await _orderService.ExportExcelAsync(filter, cancellationToken);

        var fileName = $"orders-{DateTime.Now:yyyyMMdd-HHmm}.xlsx";

        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

    }



    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)

    {

        var order = await _orderService.GetByIdAsync(id, cancellationToken);

        if (order == null) return NotFound();

        return View(order);

    }



    public async Task<IActionResult> Create(CancellationToken cancellationToken)

    {

        await LoadLookups(cancellationToken);

        return View(new OrderCreateDto { Status = 0, Details = new List<OrderDetailInputDto> { new() } });

    }



    [HttpPost]

    [ValidateAntiForgeryToken]

    public async Task<IActionResult> Create(OrderCreateDto model, CancellationToken cancellationToken)

    {

        if (model.Details == null || !model.Details.Any())

            ModelState.AddModelError(string.Empty, "يجب إضافة منتج واحد على الأقل");

        if (!ModelState.IsValid) { await LoadLookups(cancellationToken); return View(model); }

        var id = await _orderService.CreateAsync(model, cancellationToken);

        return RedirectToAction(nameof(Details), new { id });

    }



    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)

    {

        var order = await _orderService.GetByIdAsync(id, cancellationToken);

        if (order == null) return NotFound();

        await LoadLookups(cancellationToken);

        return View(new OrderCreateDto

        {

            CustomerId = order.CustomerId, AddressId = order.AddressId, DeliveryDate = order.DeliveryDate,

            Status = order.Status, Notes = order.Notes, OrderDiscount = order.Discount,

            Details = order.Details.Select(d => new OrderDetailInputDto

            {

                ProductId = d.ProductId, Quantity = d.Quantity, UnitPrice = d.UnitPrice, Discount = d.Discount

            }).ToList()

        });

    }



    [HttpPost]

    [ValidateAntiForgeryToken]

    public async Task<IActionResult> Edit(int id, OrderCreateDto model, CancellationToken cancellationToken)

    {

        if (model.Details == null || !model.Details.Any())

            ModelState.AddModelError(string.Empty, "يجب إضافة منتج واحد على الأقل");

        if (!ModelState.IsValid)

        {

            await LoadLookups(cancellationToken);

            return View(model);

        }

        await _orderService.UpdateAsync(id, model, cancellationToken);

        return RedirectToAction(nameof(Details), new { id });

    }



    [HttpPost]

    [ValidateAntiForgeryToken]

    public async Task<IActionResult> UpdateStatus(int id, int status, CancellationToken cancellationToken)

    {

        await _orderService.UpdateStatusAsync(id, status, cancellationToken);

        return RedirectToAction(nameof(Details), new { id });

    }



    private async Task LoadLookups(CancellationToken cancellationToken)

    {

        ViewBag.Customers = await _unitOfWork.Customers.Query().Select(c => new { c.Id, c.FullName }).ToListAsync(cancellationToken);

        ViewBag.Products = await _unitOfWork.Products.Query().Where(p => p.IsActive).Select(p => new { p.Id, p.ProductName, p.UnitPrice }).ToListAsync(cancellationToken);

        ViewBag.Addresses = await _unitOfWork.Addresses.Query()

            .Include(a => a.Area).Include(a => a.Governorate)

            .Select(a => new { a.Id, a.CustomerId, Label = a.Governorate.NameAr + " - " + a.Area.NameAr + " - " + a.Street, a.Area.DeliveryFee })

            .ToListAsync(cancellationToken);

    }



    private static List<(string Value, string Text)> GetStatusOptions() =>

    [

        ("", "كل الحالات"),

        (((int)OrderStatus.New).ToString(), "جديد"),

        (((int)OrderStatus.Processing).ToString(), "قيد المعالجة"),

        (((int)OrderStatus.ReadyForDelivery).ToString(), "جاهز للتوصيل"),

        (((int)OrderStatus.OutForDelivery).ToString(), "قيد التوصيل"),

        (((int)OrderStatus.Delivered).ToString(), "تم التسليم"),

        (((int)OrderStatus.Cancelled).ToString(), "ملغي"),

        (((int)OrderStatus.Returned).ToString(), "مرتجع")

    ];

}



