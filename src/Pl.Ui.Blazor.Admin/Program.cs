using Common.Clients.DependencyInjection;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor.Services;

using Pl.Ui.Blazor.Admin;
using Pl.Ui.Blazor.Services.DependencyInjection;
using Pl.Ui.Blazor.Validators.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.UseHttpClients(builder.Configuration);
builder.Services.AddMudServices();
builder.Services.UseServices();
builder.Services.AddValidators();

await builder.Build().RunAsync();
