using JDMS.Application.Constants;
using JDMS.Application.DTOs;
using JDMS.Application.Interfaces;
using JDMS.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JDMS.Web.Controllers;

[Authorize(Roles = $"{Roles.Administrator},{Roles.Manager},{Roles.Employee}")]
public class ProductsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public ProductsController(IUnitOfWork unitOfWork) => _unitOfWork = unitOfWork;

    public async Task<IActionResult> Index(string? search, CancellationToken cancellationToken)
    {
        var q = _unitOfWork.Products.Query().AsNoTracking();
        if (!string.IsNullOrWhiteSpace(search))
            q = q.Where(p => p.ProductName.Contains(search) || p.SKU.Contains(search)
                || (p.Barcode != null && p.Barcode.Contains(search)));
        var list = await q.OrderBy(p => p.ProductName).Select(p => new ProductDto
        {
            Id = p.Id, ProductName = p.ProductName, SKU = p.SKU, Barcode = p.Barcode, ImagePath = p.ImagePath,
            Description = p.Description, UnitPrice = p.UnitPrice, IsActive = p.IsActive
        }).ToListAsync(cancellationToken);
        ViewBag.Search = search;
        return View(list);
    }

    public IActionResult Create() => View(new ProductCreateDto());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductCreateDto model, CancellationToken cancellationToken)
    {
        await _unitOfWork.Products.AddAsync(new Product
        {
            ProductName = model.ProductName, SKU = model.SKU, Barcode = model.Barcode, ImagePath = model.ImagePath,
            Description = model.Description, UnitPrice = model.UnitPrice, IsActive = model.IsActive
        }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var p = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (p == null) return NotFound();
        return View(new ProductCreateDto
        {
            ProductName = p.ProductName, SKU = p.SKU, Barcode = p.Barcode, ImagePath = p.ImagePath,
            Description = p.Description, UnitPrice = p.UnitPrice, IsActive = p.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductCreateDto model, CancellationToken cancellationToken)
    {
        var p = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (p == null) return NotFound();
        p.ProductName = model.ProductName; p.SKU = model.SKU; p.Barcode = model.Barcode; p.ImagePath = model.ImagePath;
        p.Description = model.Description; p.UnitPrice = model.UnitPrice; p.IsActive = model.IsActive;
        await _unitOfWork.Products.UpdateAsync(p, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{Roles.Administrator},{Roles.Manager}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var p = await _unitOfWork.Products.GetByIdAsync(id, cancellationToken);
        if (p != null) { await _unitOfWork.Products.DeleteAsync(p, cancellationToken); await _unitOfWork.SaveChangesAsync(cancellationToken); }
        return RedirectToAction(nameof(Index));
    }
}
