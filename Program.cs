using Blazored.LocalStorage;
using BlazorWallet;
using BlazorWallet.Service;
using BlazorWallet.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<CategoriaService>();
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddMudServices();

builder.Services.AddScoped<MovimientoService>();

var host = builder.Build();

using (var scope = host.Services.CreateScope())
{
    var categoriaService = scope.ServiceProvider.GetRequiredService<CategoriaService>();
    await categoriaService.InitSeedDataAsync();
}

//await builder.Build().RunAsync();
await host.RunAsync();

