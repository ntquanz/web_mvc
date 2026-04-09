using BTL_WEB.Models;

namespace BTL_WEB.Services;

public class PetService : IPetService
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg",
        ".jpeg",
        ".png"
    };

    private readonly IWebHostEnvironment _environment;

    public PetService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<List<PetImage>> SaveImagesAsync(int petId, IEnumerable<IFormFile> imageFiles)
    {
        var savedImages = new List<PetImage>();
        var files = imageFiles.Where(x => x is not null && x.Length > 0).ToList();

        if (files.Count == 0)
        {
            return savedImages;
        }

        var uploadRoot = Path.Combine(_environment.WebRootPath, "images", "pets");
        Directory.CreateDirectory(uploadRoot);

        foreach (var file in files)
        {
            var extension = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(extension))
            {
                throw new InvalidOperationException("Chi cho phep anh jpg, jpeg hoac png.");
            }

            var uniqueName = $"{Guid.NewGuid():N}{extension}";
            var destination = Path.Combine(uploadRoot, uniqueName);

            await using var stream = new FileStream(destination, FileMode.Create);
            await file.CopyToAsync(stream);

            savedImages.Add(new PetImage
            {
                PetId = petId,
                ImageUrl = $"/images/pets/{uniqueName}",
                IsPrimary = savedImages.Count == 0
            });
        }

        return savedImages;
    }
}
