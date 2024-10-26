using EshopApp.DataLibrary.Models;
using EshopApp.DataLibrary.Models.ResponseModels;
using EshopApp.DataLibrary.Models.ResponseModels.AttributeModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace EshopApp.DataLibrary.DataAccess;

//TODO just add logging codes at some point
public class AttributeDataAccess : IAttributeDataAccess
{
    private readonly AppDataDbContext _appDataDbContext;
    private readonly ILogger<AttributeDataAccess> _logger;

    AttributeDataAccess(AppDataDbContext appDataDbContext, ILogger<AttributeDataAccess> logger = null!)
    {
        _appDataDbContext = appDataDbContext;
        _logger = logger ?? NullLogger<AttributeDataAccess>.Instance;
    }

    public async Task<ReturnAttributesAndCodeResponseModel> GetAttributesAsync(int amount)
    {
        try
        {
            List<AppAttribute> attributes = await _appDataDbContext.Attributes
                .Include(a => a.Variants)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p!.Categories)
                .Include(a => a.Variants)
                    .ThenInclude(v => v.VariantImages)
                .Take(amount)
                .ToListAsync();

            return new ReturnAttributesAndCodeResponseModel(attributes, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetAttributesFailure"), ex, "An error occurred while retrieving the attributes. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnAttributeAndCodeResponseModel> GetAttributeByIdAsync(string id)
    {
        try
        {
            AppAttribute? foundAttribute = await _appDataDbContext.Attributes
                .Include(a => a.Variants)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p!.Categories)
                .Include(a => a.Variants)
                    .ThenInclude(v => v.VariantImages)
                .FirstOrDefaultAsync(attribute => attribute.Id == id);
            return new ReturnAttributeAndCodeResponseModel(foundAttribute!, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "GetAttributeByIdFailure"), ex, "An error occurred while retrieving the attribute with Id={id}. " +
                "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<ReturnAttributeAndCodeResponseModel> CreateAttributeAsync(AppAttribute attribute)
    {
        try
        {
            if (await _appDataDbContext.Attributes.AnyAsync(existingAttributes => existingAttributes.Name == attribute.Name))
                return new ReturnAttributeAndCodeResponseModel(null!, DataLibraryReturnedCodes.DuplicateEntityName);

            attribute.Id = Guid.NewGuid().ToString();
            while (await _appDataDbContext.Attributes.FirstOrDefaultAsync(otherAttribute => otherAttribute.Id == attribute.Id) is not null)
                attribute.Id = Guid.NewGuid().ToString();

            DateTime dateTimeNow = DateTime.Now;
            attribute.CreatedAt = dateTimeNow;
            attribute.ModifiedAt = dateTimeNow;
            await _appDataDbContext.Attributes.AddAsync(attribute);

            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "CreateAttributeSuccess"), "The attribute was successfully created.");
            return new ReturnAttributeAndCodeResponseModel(attribute, DataLibraryReturnedCodes.NoError);
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "CreateAttributeFailure"), ex, "An error occurred while creating attribute." +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> UpdateAttributeAsync(AppAttribute updatedAttribute)
    {
        try
        {
            if (updatedAttribute.Id is null)
                return DataLibraryReturnedCodes.TheIdOfTheEntityCanNotBeNull;

            AppAttribute? foundAttribute = await _appDataDbContext.Attributes.Include(a => a.Variants).FirstOrDefaultAsync(attribute => attribute.Id == updatedAttribute.Id);
            if (foundAttribute is null)
            {
                _logger.LogWarning(new EventId(9999, "UpdateAttributeFailureDueToNullAttribute"), "The attribute with Id={id} was not found and thus the update could not proceed.", updatedAttribute.Id);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            if (updatedAttribute.Name is not null)
            {
                if (await _appDataDbContext.Attributes.AnyAsync(existingAttributes => existingAttributes.Name == updatedAttribute.Name))
                    return DataLibraryReturnedCodes.DuplicateEntityName;

                foundAttribute.Name = updatedAttribute.Name;
            }

            if (updatedAttribute.Variants != null && !updatedAttribute.Variants.Any())
            {
                foundAttribute.Variants.Clear();
            }
            else if (updatedAttribute.Variants != null)
            {
                List<string> updatedAttributeVariants = updatedAttribute.Variants.Select(attributeVariant => attributeVariant.Id!).ToList(); // just add them here, for filtering below
                List<Variant> filteredAttributeVariants = await _appDataDbContext.Variants
                    .Where(databaseVariant => updatedAttributeVariants.Contains(databaseVariant.Id!))
                    .ToListAsync();

                foundAttribute.Variants.Clear();
                foundAttribute.Variants.AddRange(filteredAttributeVariants);
            }

            foundAttribute.ModifiedAt = DateTime.Now;
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "UpdateAttributeSuccess"), "The attribute with Id={id} was successfully updated.", updatedAttribute.Id);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "UpdateAttributeFailure"), ex, "An error occurred while updateding the attribute with Id={id}." +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", updatedAttribute.Id, ex.Message, ex.StackTrace);
            throw;
        }
    }

    public async Task<DataLibraryReturnedCodes> DeleteAttributeAsync(string attributeId)
    {
        try
        {
            AppAttribute? foundAttribute = await _appDataDbContext.Attributes.FirstOrDefaultAsync(attribute => attribute.Id == attributeId);
            if (foundAttribute is null)
            {
                _logger.LogWarning(new EventId(9999, "DeleteAttributeFailureDueToNullAttribute"), "The attribute with Id={id} was not found and thus the deletion could not proceed.", attributeId);
                return DataLibraryReturnedCodes.EntityNotFoundWithGivenId;
            }

            _appDataDbContext.Attributes.Remove(foundAttribute);
            await _appDataDbContext.SaveChangesAsync();

            _logger.LogInformation(new EventId(9999, "DeleteAttributeSuccess"), "The attribute with Id={id} was successfully deleted.", attributeId);
            return DataLibraryReturnedCodes.NoError;
        }
        catch (Exception ex)
        {
            _logger.LogError(new EventId(9999, "DeleteAttributeFailure"), ex, "An error occurred while deleting the attribute with Id={id}." +
            "ExceptionMessage={ExceptionMessage}. StackTrace={StackTrace}.", attributeId, ex.Message, ex.StackTrace);
            throw;
        }
    }
}
