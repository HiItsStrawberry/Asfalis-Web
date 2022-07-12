using asfalis.Client;
using asfalis.Client.ViewModels;
using Blazored.Toast;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var project = "Asfalis";
var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });



//builder.Services.AddScoped<ILoginViewModel, LoginViewModel>();
//builder.Services.AddScoped<IRegisterViewModel, RegisterViewModel>();
//builder.Services.AddScoped<ICustomUserValidator, CustomUserValidator>();

#region DI and Http Request handler
// Services for dependency injection and Http Request handler
builder.Services.AddTransient<CustomAuthHandler>();

builder.Services.AddHttpClient<ILoginViewModel, LoginViewModel>(
    project, client =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    }).AddHttpMessageHandler<CustomAuthHandler>();

builder.Services.AddHttpClient<IRegisterViewModel, RegisterViewModel>(
    project, client =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    });

builder.Services.AddHttpClient<ICustomUserValidator, CustomUserValidator>(
    project, client =>
    {
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    });
#endregion

#region Other services
// Custom authentication state provider
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(
    provider => provider.GetRequiredService<CustomAuthStateProvider>());

// Toast service for blazor
builder.Services.AddBlazoredToast();

// Authorization feature for blazor
builder.Services.AddAuthorizationCore();

// Local storage access for blazor
builder.Services.AddBlazoredLocalStorage();
#endregion

await builder.Build().RunAsync();
