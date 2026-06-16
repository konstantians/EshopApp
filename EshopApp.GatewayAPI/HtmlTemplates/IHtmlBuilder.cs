using EshopApp.GatewayAPI.DataMicroService.SharedModels;

namespace EshopApp.GatewayAPI.HtmlTemplates;
public interface IHtmlBuilder
{
    Dictionary<string, string> CreateOrderSummaryEmail(GatewayOrder order);
}