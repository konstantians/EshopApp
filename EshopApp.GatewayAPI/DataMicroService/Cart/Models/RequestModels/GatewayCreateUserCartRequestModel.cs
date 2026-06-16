namespace EshopApp.GatewayAPI.DataMicroService.Cart.Models.RequestModels;

public class GatewayCreateUserCartRequestModel
{
    public List<GatewayCreateCartItemRequestModel> CreateCartItemRequestModels { get; set; } = new List<GatewayCreateCartItemRequestModel>();
}
