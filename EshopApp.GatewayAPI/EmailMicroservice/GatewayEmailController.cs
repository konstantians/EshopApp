using EshopApp.GatewayAPI.EmailMicroservice.Models;
using EshopApp.GatewayAPI.HelperMethods;
using EshopApp.GatewayAPI.HtmlTemplates;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EshopApp.GatewayAPI.EmailMicroservice;

[ApiController]
[EnableRateLimiting("DefaultWindowLimiter")]
[Route("api/[controller]")]
public class GatewayEmailController : ControllerBase
{
    private readonly HttpClient emailHttpClient;
    private readonly IConfiguration _configuration;
    private readonly IUtilityMethods _utilityMethods;
    private readonly IHtmlBuilder _htmlBuilder;

    public GatewayEmailController(IConfiguration configuration, IHttpClientFactory httpClientFactory, IUtilityMethods utilityMethods, IHtmlBuilder htmlBuilder)
    {
        _configuration = configuration;
        _utilityMethods = utilityMethods;
        _htmlBuilder = htmlBuilder;
        emailHttpClient = httpClientFactory.CreateClient("EmailApiClient");
    }

    [HttpPost("ContactEmail")]
    public async Task<IActionResult> SendContactEmail(GatewaySendContactEmailRequestModel gatewaySendContactEmailRequestModel)
    {
        try
        {
            //fix it here
            var emailHtml = @"
            <!doctype html>
            <html lang=""el"">
            <head>
            <meta charset=""UTF-8"" />
            <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
            <title>Μήνυμα Επικοινωνίας</title>

            <style>
              body {
                margin: 0;
                padding: 32px 16px;
                background: #f2f2f2;
                font-family: Arial, sans-serif;
                color: #1a1a1a;
              }

              .wrapper {
                max-width: 640px;
                margin: 0 auto;
                background: #fff;
                border-radius: 16px;
                overflow: hidden;
                box-shadow: 0 8px 28px rgba(0,0,0,0.08);
              }

              .header {
                background: #ffffff;
                padding: 26px 24px 18px;
                text-align: center;
                border-bottom: 1px solid #f0f0f0;
              }

              .icon {
                width: 52px;
                height: 52px;
                margin: 6px auto 12px;
                border-radius: 14px;
                background: #fff5ef;
                display: flex;
                align-items: center;
                justify-content: center;
                border: 1px solid #ffd8c2;
              }

              .badge {
                display: inline-block;
                font-size: 11px;
                font-weight: 700;
                letter-spacing: 0.6px;
                padding: 6px 10px;
                border-radius: 999px;
                background: #fff0e8;
                color: #ff5e00;
                margin-bottom: 10px;
              }

              .title {
                font-size: 22px;
                font-weight: 800;
                margin: 0;
              }

              .subtitle {
                margin-top: 6px;
                font-size: 13px;
                color: #777;
              }

              .body {
                padding: 22px;
              }

              .card {
                border: 1px solid #eee;
                border-radius: 12px;
                padding: 16px;
                margin-bottom: 14px;
              }

              .card-title {
                font-size: 12px;
                font-weight: 800;
                color: #888;
                margin-bottom: 12px;
                text-transform: uppercase;
                letter-spacing: 0.8px;
              }

              .row {
                display: flex;
                justify-content: space-between;
                padding: 10px 0;
                border-bottom: 1px solid #f3f3f3;
                font-size: 14px;
              }

              .row:last-child {
                border-bottom: none;
              }

              .label {
                color: #777;
                font-weight: 600;
              }

              .value {
                font-weight: 700;
                max-width: 60%;
                text-align: right;
                word-break: break-word;
              }

              .message {
                background: #fafafa;
                border-left: 4px solid #ff5e00;
                padding: 14px;
                font-size: 14px;
                line-height: 1.6;
                border-radius: 6px;
              }

              .footer {
                text-align: center;
                padding: 16px;
                font-size: 12px;
                color: #888;
                background: #f7f7f7;
              }
            </style>
            </head>

            <body>

            <div class=""wrapper"">

              <div class=""header"">

                <div class=""icon"">
                  <svg width=""24"" height=""24"" viewBox=""0 0 24 24"" fill=""none"" stroke=""#ff5e00"" stroke-width=""2"">
                    <path d=""M21 15a4 4 0 0 1-4 4H7l-4 4V7a4 4 0 0 1 4-4h10a4 4 0 0 1 4 4z"" />
                  </svg>
                </div>

                <div class=""badge"">ΝΕΟ ΜΗΝΥΜΑ</div>

                <h1 class=""title"">Αίτημα Επικοινωνίας</h1>

                <div class=""subtitle"">
                  Λάβατε ένα νέο μήνυμα από τη φόρμα επικοινωνίας της ιστοσελίδας σας
                </div>

              </div>

              <div class=""body"">

                <div class=""card"">
                  <div class=""card-title"">Στοιχεία Αποστολέα</div>

                  <div class=""row"">
                    <div class=""label"">Όνομα</div>
                    <div class=""value"">" + gatewaySendContactEmailRequestModel.FirstName + @"</div>
                  </div>

                  <div class=""row"">
                    <div class=""label"">Επώνυμο</div>
                    <div class=""value"">" + gatewaySendContactEmailRequestModel.LastName + @"</div>
                  </div>

                  <div class=""row"">
                    <div class=""label"">Email</div>
                    <div class=""value"">" + gatewaySendContactEmailRequestModel.Email + @"</div>
                  </div>

                  <div class=""row"">
                    <div class=""label"">Θέμα</div>
                    <div class=""value"">" + gatewaySendContactEmailRequestModel.Subject + @"</div>
                  </div>

                </div>

                <div class=""card"">
                  <div class=""card-title"">Μήνυμα</div>

                  <div class=""message"">
                    " + gatewaySendContactEmailRequestModel.Message + @"
                  </div>
                </div>

              </div>

              <div class=""footer"">
                © " + DateTime.UtcNow.Year + @" Eshopapp — Ειδοποίηση φόρμας επικοινωνίας
              </div>

            </div>

            </body>
            </html>";

            //create the category
            _utilityMethods.SetDefaultHeadersForClient(false, emailHttpClient, _configuration["EmailApiKey"]!, _configuration["DataRateLimitingBypassCode"]!);
            HttpResponseMessage? response = await _utilityMethods.MakeRequestWithRetriesForServerErrorAsync(() => emailHttpClient.PostAsJsonAsync("Emails/ContactEmail", new
            {
                Receiver = gatewaySendContactEmailRequestModel.Email,
                Title = "Contact Form Email From: " + gatewaySendContactEmailRequestModel.LastName,
                Message = emailHtml
            }));

            if ((int)response.StatusCode >= 400)
                return await _utilityMethods.CommonHandlingForErrorCodesAsync(response);

            return NoContent();
        }
        catch (Exception)
        {
            return StatusCode(500);
        }
    }
}
