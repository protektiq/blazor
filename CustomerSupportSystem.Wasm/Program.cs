using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CustomerSupportSystem.Wasm;
using CustomerSupportSystem.Wasm.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure HTTP client for API calls
builder.Services.AddHttpClient("ApiClient", client =>
{
    // For development, use localhost API
    // For production, this would be your deployed API URL
    client.BaseAddress = new Uri("https://localhost:7000/");
});

// Register services
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<TicketsService>();

await builder.Build().RunAsync();
