using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using JDMS.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager},{Roles.Employee}")]
public class DeliveriesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOrderService _orderService;

    public DeliveriesController(IUnitOfWork unitOfWork, IOrderService orderService)
    {
        _unitOfWork = unitOfWork;
        _orderService = orderService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var list = await _unitOfWork.DeliveryTrackings.Query()
            .Include(t => t.Order).Include(t => t.Driver)
            .AsNoTracking()
            .OrderByDescending(t => t.AssignDate)
            .Select(t => new DeliveryTrackingDto
            {
                Id = t.Id, OrderId = t.OrderId, OrderNumber = t.Order.OrderNumber,
                DriverId = t.DriverId, DriverName = t.Driver.DriverName,
                AssignDate = t.AssignDate, DeliveryDate = t.DeliveryDate,
                Status = (int)t.Status, StatusName = t.Status.ToString(), DeliveryNotes = t.DeliveryNotes
            }).ToListAsync(cancellationToken);

        ViewBag.Orders = await _unitOfWork.Orders.Query()
            .Where(o => o.Status == OrderStatus.ReadyForDelivery || o.Status == OrderStatus.Processing)
            .Select(o => new { o.Id, o.OrderNumber }).ToListAsync(cancellationToken);
        ViewBag.Drivers = await _unitOfWork.Drivers.Query().Where(d => d.IsActive)
            .Select(d => new { d.Id, d.DriverName }).ToListAsync(cancellationToken);
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(DeliveryAssignDto model, CancellationToken cancellationToken)
    {
        var existing = await _unitOfWork.DeliveryTrackings.Query().FirstOrDefaultAsync(t => t.OrderId == model.OrderId, cancellationToken);
        if (existing != null)
        {
            existing.DriverId = model.DriverId;
            existing.DeliveryNotes = model.DeliveryNotes;
            existing.AssignDate = DateTime.UtcNow;
            await _unitOfWork.DeliveryTrackings.UpdateAsync(existing, cancellationToken);
        }
        else
        {
            await _unitOfWork.DeliveryTrackings.AddAsync(new DeliveryTracking
            {
                OrderId = model.OrderId, DriverId = model.DriverId, DeliveryNotes = model.DeliveryNotes,
                Status = DeliveryStatus.Assigned
            }, cancellationToken);
        }
        await _orderService.UpdateStatusAsync(model.OrderId, (int)OrderStatus.OutForDelivery, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, int status, CancellationToken cancellationToken)
    {
        var t = await _unitOfWork.DeliveryTrackings.GetByIdAsync(id, cancellationToken);
        if (t != null)
        {
            t.Status = (DeliveryStatus)status;
            if (status == (int)DeliveryStatus.Delivered)
            {
                t.DeliveryDate = DateTime.UtcNow;
                await _orderService.UpdateStatusAsync(t.OrderId, (int)OrderStatus.Delivered, cancellationToken);
            }
            await _unitOfWork.DeliveryTrackings.UpdateAsync(t, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        return RedirectToAction(nameof(Index));
    }
}
