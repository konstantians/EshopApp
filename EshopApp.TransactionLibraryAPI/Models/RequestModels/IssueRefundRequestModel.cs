using System.ComponentModel.DataAnnotations;

namespace EshopApp.TransactionLibraryAPI.Models.RequestModels;

public class IssueRefundRequestModel
{
    [Required]
    public string? PaymentIntentId { get; set; }
}
