using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.ImagesModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.DataLibrary.DataAccess;

//TODO update this class when orders entity is also added
public class ImageDataAccess : IImageDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<ImageDataAccess> _logger;

    ImageDataAccess(AppDataDbContext appDataDbContext, ILogger<ImageDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<ImageDataAccess>.Instance;
    }

    public async Task<ReturnImagesAndCodeResponseModel> GetImagesAsync(int amount, bool includeSoftDeletedImages)
    {
        try
        {
            List<AppImage> images;
            if (!includeSoftDeletedImages)
            {
                images = await _appDataDbContext.Images
                .Include(image => image.VariantImages)
                    .ThenInclude(variantImages => variantImages.Variant)
                        .ThenInclude(variant => variant!.Product)
                .Where(image => !image.ShouldNotShowInGallery)
                .Take(amount)
                .ToListAsync();
            }
            else
            {
                images = await _appDataDbContext.Images
                .Include(image => image.VariantImages)
                    .ThenInclude(variantImages => variantImages.Variant)
                        .ThenInclude(variant => variant!.Product)
                .Take(amount)
                .ToListAsync();
            }

            return new ReturnImagesAndCodeResponseModel(images, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetImagesFailure"), ex, "An error occurred while retrieving the images. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnImageAndCodeResponseModel> GetImageAsync(string id, bool includeSoftDeletedImages)
    {
        try
        {
            AppImage? foundImage;
            if (!includeSoftDeletedImages)
            {
                foundImage = await _appDataDbContext.Images
                .Include(image => image.VariantImages)
                    .ThenInclude(variantImages => variantImages.Variant)
                        .ThenInclude(variant => variant!.Product)
                .FirstOrDefaultAsync(image => image.Id == id && !image.ShouldNotShowInGallery);
            }
            else
            {
                foundImage = await _appDataDbContext.Images
                .Include(image => image.VariantImages)
                    .ThenInclude(variantImages => variantImages.Variant)
                        .ThenInclude(variant => variant!.Product)
                .FirstOrDefaultAsync(image => image.Id == id);
            }

            return new ReturnImageAndCodeResponseModel(foundImage!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetImageByIdFailure"), ex, "An error occurred while retrieving the image with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnImageAndCodeResponseModel> CreateImageAsync(AppImage image)
    {
        try
        {
            if (await _appDataDbContext.Images.AnyAsync(existingImage => existingImage.Name == image.Name))
                return new ReturnImageAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateEntityName);

            image.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Images.FirstOrDefaultAsync(otherImage => otherImage.Id == image.Id) is not null)
                image.Id = Guid.NewGuid().ToString();

            image.ImagePath = "image_" + Guid.NewGuid().ToString();
            while (await _appDataDbContext.Images.FirstOrDefaultAsync(otherImage => otherImage.ImagePath == image.ImagePath) is not null) //TODO maybe change the filepath name in the future
                image.ImagePath = "image_" + Guid.NewGuid().ToString();

            DateTime dateTimeNow = DateTime.Now;
            image.CreatedAt = dateTimeNow;
            image.ModifiedAt = dateTimeNow;
            await _appDataDbContext.Images.AddAsync(image);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreateImageSuccess"), "The image was successfully created.");
            return new ReturnImageAndCodeResponseModel(image, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateImageFailure"), ex, "An error occurred while creating the image. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdateImageAsync(AppImage updatedImage)
    {
        try
        {
            if (updatedImage.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            AppImage? foundImage = await _appDataDbContext.Images.FirstOrDefaultAsync(image => image.Id == updatedImage.Id);
            if (foundImage is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateImageDueToNullImage"), "The image with Id={id} was not found and thus the update could not proceed.", updatedImage.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (updatedImage.Name is not null)
            {
                if (await _appDataDbContext.Images.AnyAsync(existingDiscount => existingDiscount.Name == updatedImage.Name))
                    return DataLibraryReturnedCodes.DuplicateEntityName;
                foundImage.Name = updatedImage.Name;
            }

            foundImage.ShouldNotShowInGallery = updatedImage.ShouldNotShowInGallery;
            foundImage.ExistsInOrder = updatedImage.ExistsInOrder;
            foundImage.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateImageSuccess"), "The image was successfully updated.");
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateImageFailure"), ex, "An error occurred while updating the image with Id={id}. " +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedImage.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> RestoreSoftDeletedImagesAsync(List<string> imageIds)
    {
        try
        {
            List<AppImage> foundImages = await _appDataDbContext.Images
                .Where(image => imageIds.Contains(image.Id!))
                .ToListAsync();

            if (!foundImages.Any())
                return DataLibraryReturnedCodes.NoError;

            foreach (var image in foundImages)
                image.ShouldNotShowInGallery = false;

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "RestoreSoftDeletedSuccess"), "The images were successfully restored to the gallery.");
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateImageFailure"), ex, "An error occurred while restoring soft deleted images. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteImageAsync(string imageId)
    {
        try
        {
            AppImage? foundImage = await _appDataDbContext.Images.FirstOrDefaultAsync(image => image.Id == imageId);
            if (foundImage is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteImageFailureDueToNullImage"), "The image with Id={id} was not found and thus the delete could not proceed.", imageId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (foundImage.ExistsInOrder)
            {
                foundImage.ShouldNotShowInGallery = true;
                await _appDataDbContext.SaveChangesAsync();

                _logger.LogInformation(new EventId(9999, "DeleteImageSuccessButSetToNotVisible"), "The image with Id={id} exists in an order and thus can not be fully deleted until that order is deleted, " +
                    "but it will correctly not be shown in the images of the gallery.", imageId);
                return DataLibraryReturnedCodes.NoError;
            }

            _appDataDbContext.Images.Remove(foundImage);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteImageSuccess"), "The image with Id={id} was successfully deleted.", imageId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteImageFailure"), ex, "An error occurred while deleting the image with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", imageId, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
