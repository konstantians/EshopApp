namespace EshopApp.MVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            IConfiguration configuration = builder.Configuration;

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(10);
                options.Cookie.IsEssential = true;
                options.Cookie.HttpOnly = true;
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Add HTTP client for API calls.
            builder.Services.AddHttpClient("GatewayApiClient", client =>
            {
                client.BaseAddress = new Uri(configuration["GatewayApiBaseUrl"]!);
                client.DefaultRequestHeaders.Add("X-API-KEY", configuration["GatewayApiKey"]!);
                client.DefaultRequestHeaders.Add("X-Bypass-Rate-Limiting", configuration["GatewayApiRateLimitingBypassCode"]!);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession(); //Maybe add an idle timeout?

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
