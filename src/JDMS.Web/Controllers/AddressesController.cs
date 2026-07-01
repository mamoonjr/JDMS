using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager},{Roles.Employee}")]
public class AddressesController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public AddressesController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IActionResult> Index(int? customerId, CancellationToken cancellationToken)
    {
        var q = _unitOfWork.Addresses.Query()
            .Include(a => a.Governorate).Include(a => a.Area).Include(a => a.Customer)
            .AsNoTracking();
        if (customerId.HasValue) q = q.Where(a => a.CustomerId == customerId);

        var list = await q.Select(a => new AddressDto
        {
            Id = a.Id, CustomerId = a.CustomerId, GovernorateId = a.GovernorateId, AreaId = a.AreaId,
            GovernorateName = a.Governorate.NameAr, AreaName = a.Area.NameAr,
            Street = a.Street, Building = a.Building, Apartment = a.Apartment,
            Latitude = a.Latitude, Longitude = a.Longitude, GoogleMapsLink = a.GoogleMapsLink, IsDefault = a.IsDefault
        }).ToListAsync(cancellationToken);

        ViewBag.Customers = await _unitOfWork.Customers.Query().Select(c => new { c.Id, c.FullName }).ToListAsync(cancellationToken);
        ViewBag.Governorates = await _unitOfWork.Governorates.Query().Select(g => new { g.Id, g.NameAr }).ToListAsync(cancellationToken);
        ViewBag.CustomerId = customerId;
        return View(list);
    }

    public async Task<IActionResult> Create(int? customerId, CancellationToken cancellationToken)
    {
        await LoadLookups(cancellationToken);
        return View(new AddressCreateDto { CustomerId = customerId ?? 0 });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AddressCreateDto model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) { await LoadLookups(cancellationToken); return View(model); }
        await _unitOfWork.Addresses.AddAsync(new Address
        {
            CustomerId = model.CustomerId, GovernorateId = model.GovernorateId, AreaId = model.AreaId,
            Neighborhood = model.Neighborhood, Street = model.Street, Building = model.Building, Apartment = model.Apartment,
            Latitude = model.Latitude, Longitude = model.Longitude, GoogleMapsLink = model.GoogleMapsLink, IsDefault = model.IsDefault
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index), new { customerId = model.CustomerId });
    }

    private async Task LoadLookups(CancellationToken cancellationToken)
    {
        ViewBag.Customers = await _unitOfWork.Customers.Query().Select(c => new { c.Id, c.FullName }).ToListAsync(cancellationToken);
        ViewBag.Governorates = await _unitOfWork.Governorates.Query().Select(g => new { g.Id, g.NameAr }).ToListAsync(cancellationToken);
    }
}
