using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using JDMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager}")]
public class DriversController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public DriversController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    private static readonly Dictionary<VehicleType, string> VehicleNames = new()
    {
        { VehicleType.Motorcycle, "دراجة نارية" },
        { VehicleType.Car, "سيارة" },
        { VehicleType.Van, "فان" },
        { VehicleType.Truck, "شاحنة" }
    };

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var rows = await _unitOfWork.Drivers.Query()
            .Include(d => d.AssignedArea)
            .Include(d => d.DeliveryTrackings)
            .AsNoTracking()
            .Select(d => new
            {
                d.Id,
                d.DriverName,
                d.PhoneNumber,
                VehicleType = (int)d.VehicleType,
                d.AssignedAreaId,
                AssignedAreaName = d.AssignedArea != null ? d.AssignedArea.NameAr : null,
                d.IsActive,
                TotalDeliveries = d.DeliveryTrackings.Count,
                CompletedDeliveries = d.DeliveryTrackings.Count(t => t.Status == DeliveryStatus.Delivered)
            }).ToListAsync(cancellationToken);

        var list = rows.Select(d => new DriverDto
        {
            Id = d.Id,
            DriverName = d.DriverName,
            PhoneNumber = d.PhoneNumber,
            VehicleType = d.VehicleType,
            VehicleTypeName = VehicleNames[(VehicleType)d.VehicleType],
            AssignedAreaId = d.AssignedAreaId,
            AssignedAreaName = d.AssignedAreaName,
            IsActive = d.IsActive,
            TotalDeliveries = d.TotalDeliveries,
            CompletedDeliveries = d.CompletedDeliveries
        }).ToList();
        ViewBag.Areas = await _unitOfWork.Areas.Query().Select(a => new { a.Id, a.NameAr }).ToListAsync(cancellationToken);
        return View(list);
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        ViewBag.Areas = await _unitOfWork.Areas.Query().Select(a => new { a.Id, a.NameAr }).ToListAsync(cancellationToken);
        return View(new DriverCreateDto());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DriverCreateDto model, CancellationToken cancellationToken)
    {
        await _unitOfWork.Drivers.AddAsync(new Driver
        {
            DriverName = model.DriverName, PhoneNumber = model.PhoneNumber,
            VehicleType = (VehicleType)model.VehicleType, AssignedAreaId = model.AssignedAreaId, IsActive = model.IsActive
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Performance(int id, CancellationToken cancellationToken)
    {
        var driver = await _unitOfWork.Drivers.Query()
            .Include(d => d.DeliveryTrackings).ThenInclude(t => t.Order)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);
        return driver == null ? NotFound() : View(driver);
    }
}
