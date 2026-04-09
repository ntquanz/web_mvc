using BTL_WEB.Helpers;
using BTL_WEB.Models;
using BTL_WEB.ViewModels.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BTL_WEB.Controllers;

[Authorize(Policy = RoleNames.StaffOrAdmin)]
public class ServicesController : Controller
{
    private readonly PetCareHubContext _context;

    public ServicesController(PetCareHubContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? keyword, int? categoryId, string? status, int page = 1, int pageSize = 10)
    {
        var query = _context.Services
            .Include(x => x.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x => x.ServiceName.Contains(keyword) || (x.Description ?? string.Empty).Contains(keyword));
        }

        if (categoryId.HasValue)
        {
            query = query.Where(x => x.CategoryId == categoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == status);
        }

        ViewBag.Keyword = keyword;
        ViewBag.CategoryId = categoryId;
        ViewBag.Status = status;
        ViewBag.PageSize = pageSize;
        ViewBag.Categories = await _context.ServiceCategories
            .OrderBy(x => x.CategoryName)
            .Select(x => new SelectListItem(x.CategoryName, x.CategoryId.ToString()))
            .ToListAsync();

        return View(await PaginatedList<Service>.CreateAsync(query.OrderBy(x => x.ServiceName), page, pageSize));
    }

    public async Task<IActionResult> Details(int id)
    {
        var service = await _context.Services
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.ServiceId == id);

        return service is null ? NotFound() : View(service);
    }

    public async Task<IActionResult> Create()
    {
        return View(await BuildModelAsync(new ServiceFormViewModel()));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(await BuildModelAsync(model));
        }

        _context.Services.Add(new Service
        {
            CategoryId = model.CategoryId,
            ServiceName = model.ServiceName,
            Description = model.Description,
            Price = model.Price,
            DurationMinutes = model.DurationMinutes,
            Status = model.Status
        });

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service is null)
        {
            return NotFound();
        }

        return View(await BuildModelAsync(new ServiceFormViewModel
        {
            ServiceId = service.ServiceId,
            CategoryId = service.CategoryId,
            ServiceName = service.ServiceName,
            Description = service.Description,
            Price = service.Price,
            DurationMinutes = service.DurationMinutes,
            Status = service.Status
        }));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ServiceFormViewModel model)
    {
        if (id != model.ServiceId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(await BuildModelAsync(model));
        }

        var service = await _context.Services.FindAsync(id);
        if (service is null)
        {
            return NotFound();
        }

        service.CategoryId = model.CategoryId;
        service.ServiceName = model.ServiceName;
        service.Description = model.Description;
        service.Price = model.Price;
        service.DurationMinutes = model.DurationMinutes;
        service.Status = model.Status;

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var service = await _context.Services
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.ServiceId == id);

        return service is null ? NotFound() : View(service);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var service = await _context.Services.FindAsync(id);
        if (service is null)
        {
            return NotFound();
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private async Task<ServiceFormViewModel> BuildModelAsync(ServiceFormViewModel model)
    {
        model.CategoryOptions = await _context.ServiceCategories
            .OrderBy(x => x.CategoryName)
            .Select(x => new SelectListItem(x.CategoryName, x.CategoryId.ToString()))
            .ToListAsync();

        return model;
    }
}
