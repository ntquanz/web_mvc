using BTL_WEB.Helpers;
using BTL_WEB.Models;
using BTL_WEB.ViewModels.Dashboard;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BTL_WEB.Controllers;

[Authorize(Policy = RoleNames.StaffOrAdmin)]
public class DashboardController : Controller
{
    private readonly PetCareHubContext _context;

    public DashboardController(PetCareHubContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            TotalPets = await _context.Pets.CountAsync(),
            AvailablePets = await _context.Pets.CountAsync(x => x.AdoptionStatus == "Available"),
            AdoptedPets = await _context.Pets.CountAsync(x => x.AdoptionStatus == "Adopted"),
            PendingAppointments = await _context.Appointments.CountAsync(x => x.Status == "Pending"),
            PendingAdoptionRequests = await _context.AdoptionRequests.CountAsync(x => x.Status == "Pending"),
            PaidPaymentsTotal = await _context.Payments
                .Where(x => x.PaymentStatus == "Paid")
                .Select(x => (decimal?)x.Amount)
                .SumAsync() ?? 0m
        };

        return View(model);
    }
}
