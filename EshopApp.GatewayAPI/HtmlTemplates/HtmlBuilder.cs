using EshopApp.GatewayAPI.DataMicroService.SharedModels;

namespace EshopApp.GatewayAPI.HtmlTemplates;

public class HtmlBuilder : IHtmlBuilder
{
    private readonly IConfiguration _configuration;

    public HtmlBuilder(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Dictionary<string, string> CreateOrderSummaryEmail(GatewayOrder order)
    {
        string orderItemsHtml = string.Join("", order!.OrderItems.Select(item =>
        {
            string productName = item.Variant?.Product?.Name ?? "Product";
            string variantInfo = item.Variant?.SKU ?? "";
            int quantity = item.Quantity ?? 0;
            decimal price = item.UnitPriceAtOrder ?? 0;
            string? imagePath = item.Image?.ImagePath;
            string imageUrl = !string.IsNullOrEmpty(imagePath) ? $"{_configuration["TrustedOrigins"]}/DynamicImages/{imagePath}" : $"{_configuration["TrustedOrigins"]}/Images/noProductImage.jpg";

            return $@"
            <tr>
              <td style='padding:12px 0; border-bottom:1px solid #f0f0f0;'>
                <table role='presentation' style='border-collapse:collapse;'>
                  <tr>
                    <td style='vertical-align:middle; padding-right:12px;'>
                      <img src='{imageUrl}' width='52' height='52' style='display:block; border-radius:8px; object-fit:cover;' />
                    </td>

                    <td style='vertical-align:middle;'>
                      <div style='font-size:14px; font-weight:600; color:#1a1a1a;'>
                        {productName}
                      </div>
                      <div style='font-size:12px; color:#aaa; margin-top:3px;'>
                        {variantInfo}
                      </div>
                    </td>
                  </tr>
                </table>
              </td>

              <td style='text-align:center; border-bottom:1px solid #f0f0f0;'>
                {quantity}
              </td>

              <td style='text-align:right; border-bottom:1px solid #f0f0f0;'>
                {price:0.00} €
              </td>
            </tr>";
        }));

        string templatePath = Path.Combine(
            AppContext.BaseDirectory,
            "HtmlTemplates",
            "OrderSuccessTemplate.html"
        );

        string html = File.ReadAllText(templatePath);

        html = html
            .Replace("{{OrderId}}", order.Id!.ToString())
            .Replace("{{OrderDate}}", order.CreatedAt.ToString("dd MMM yyyy"))
            .Replace("{{PaymentMethod}}", order.PaymentDetails!.PaymentOption!.Name!)
            .Replace("{{ShippingMethod}}", order.ShippingOption!.Name!)
            .Replace("{{OrderItems}}", orderItemsHtml)
            .Replace("{{SubTotal}}", $"{order.FinalPrice - order.ShippingCostAtOrder - order.PaymentDetails.PaymentOptionExtraCostAtOrder:0.00} €")
            .Replace("{{ShippingCost}}", $"{order.ShippingCostAtOrder:0.00} €")
            .Replace("{{ShouldBeShown}}", order.PaymentDetails!.PaymentOption!.NameAlias!.ToLower() != "cod" ? "display:none;" : "")
            .Replace("{{CODCost}}", $"{order.PaymentDetails.PaymentOptionExtraCostAtOrder:0.00} €")
            .Replace("{{Total}}", $"{order.FinalPrice:0.00} €")
            .Replace("{{ViewOrderLink}}", "#"); //TODO eventually add the link that will allow the user to check their order

        return new Dictionary<string, string>{
            { "receiver", order!.OrderAddress!.Email! },
            { "title", "Order Confirmed" },
            { "message", html}
        };
    }
}
