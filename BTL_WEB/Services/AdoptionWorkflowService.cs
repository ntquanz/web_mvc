using BTL_WEB.Models;
using BTL_WEB.ViewModels.Adoption;
using Microsoft.EntityFrameworkCore;

namespace BTL_WEB.Services;

public class AdoptionWorkflowService : IAdoptionWorkflowService
{
    private readonly PetCareHubContext _context;

    public AdoptionWorkflowService(PetCareHubContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, string? ErrorMessage, int? RequestId)> CreateRequestAsync(int currentUserId, CreateAdoptionRequestViewModel model)
    {
        var pet = await _context.Pets.FirstOrDefaultAsync(x => x.PetId == model.PetId);
        if (pet is null)
        {
            return (false, "Thu cung khong ton tai.", null);
        }

        if (pet.AdoptionStatus != "Available")
        {
            return (false, "Pet hien khong o trang thai co the nhan nuoi.", null);
        }

        var duplicatePending = await _context.AdoptionRequests.AnyAsync(x =>
            x.PetId == model.PetId &&
            x.UserId == currentUserId &&
            x.Status == "Pending");

        if (duplicatePending)
        {
            return (false, "Ban da co yeu cau Pending cho pet nay.", null);
        }

        var request = new AdoptionRequest
        {
            UserId = currentUserId,
            PetId = model.PetId,
            RequestDate = DateTime.Now,
            Status = "Pending",
            Message = model.Message
        };

        _context.AdoptionRequests.Add(request);
        await _context.SaveChangesAsync();
        return (true, null, request.RequestId);
    }

    public async Task<(bool Success, string? ErrorMessage)> ReviewRequestAsync(int reviewerUserId, ReviewAdoptionRequestViewModel model)
    {
        var request = await _context.AdoptionRequests
            .Include(x => x.Pet)
            .FirstOrDefaultAsync(x => x.RequestId == model.RequestId);

        if (request is null)
        {
            return (false, "Yeu cau nhan nuoi khong ton tai.");
        }

        if (request.Status != "Pending")
        {
            return (false, "Chi co the duyet yeu cau dang Pending.");
        }

        var staffId = await _context.Staff
            .Where(x => x.UserId == reviewerUserId)
            .Select(x => (int?)x.StaffId)
            .FirstOrDefaultAsync();

        request.ReviewedByStaffId = staffId;
        request.ReviewedAt = DateTime.Now;

        if (string.Equals(model.Action, "Approve", StringComparison.OrdinalIgnoreCase))
        {
            if (request.Pet.AdoptionStatus != "Available")
            {
                return (false, "Pet khong con o trang thai Available.");
            }

            request.Status = "Approved";
            request.Pet.AdoptionStatus = "Adopted";
            request.Pet.OwnerId = request.UserId;

            _context.AdoptionContracts.Add(new AdoptionContract
            {
                RequestId = request.RequestId,
                PetId = request.PetId,
                UserId = request.UserId,
                SignedDate = DateTime.Now,
                AdoptionFee = model.AdoptionFee ?? 0,
                Terms = model.Terms,
                Status = "Active"
            });
        }
        else if (string.Equals(model.Action, "Reject", StringComparison.OrdinalIgnoreCase))
        {
            request.Status = "Rejected";
        }
        else
        {
            return (false, "Hanh dong review khong hop le.");
        }

        await _context.SaveChangesAsync();
        return (true, null);
    }
}
