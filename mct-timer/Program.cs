using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using mct_timer.Models;
using System;
using System.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Azure.Cosmos.Core;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.CodeAnalysis.Options;
using System.Net;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Mvc.Filters;
using Azure.Core;
using Azure.Identity;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Azure.Cosmos;
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddApplicationInsightsTelemetry(new Microsoft.ApplicationInsights.AspNetCore.Extensions.ApplicationInsightsServiceOptions
{
    ConnectionString = builder.Configuration["ApplicationInsights"]
});

var config = builder.Configuration.GetSection("ConfigMng");

//to test locally you need implement steps provided in readme file.
TokenCredential ctoken = new DefaultAzureCredential(
                    new DefaultAzureCredentialOptions()
                    {
                        TenantId = config["TenantID"],
                        AdditionallyAllowedTenants = { "*" },
                    });


builder.Services.AddDbContext<WebSettingsContext>(options =>
    options.UseCosmos(config["CosmosDBEndpoint"], ctoken, "webapp"));
   
builder.Services.AddDbContext<UsersContext>(options =>
    options.UseCosmos(config["CosmosDBEndpoint"], ctoken, "webapp"));


builder.Services.AddHttpContextAccessor();
//builder.Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());




builder.Services.Configure<ConfigMng>(config);
builder.Services.AddSingleton<AuthService>();

TelemetryClient ai = new TelemetryClient(new Microsoft.ApplicationInsights.Extensibility.TelemetryConfiguration() { ConnectionString = builder.Configuration["ApplicationInsights"] });
builder.Services.AddSingleton<TelemetryClient>(ai);

UploadValidator upValid = new UploadValidator(ai);
builder.Services.AddSingleton<UploadValidator>(upValid);

builder.Services.AddDbContext<WebSettingsContext>(options =>
        options.UseCosmos(builder.Configuration.GetConnectionString("WebSettingsContext") ?? throw new InvalidOperationException("Connection string 'WebSettingsContext' not found."), "webapp"));

// Blob generator
BlobRepo blob = new BlobRepo(config["StorageAccountName"], config["ContainerName"], config["TenantID"]);
builder.Services.AddSingleton<IBlobRepo>(blob);

// Dalle generator
DalleGenerator dalleGen = new DalleGenerator(config["OpenAIEndpoint"], config["OpenAIKey"], config["OpenAIModel"], ai);
builder.Services.AddSingleton<IDalleGenerator>(dalleGen);

//KeyVault
KeyVaultMng keymng = new KeyVaultMng(config["KeyVault"], config["PssKey"],ai);
builder.Services.AddSingleton<IKeyVaultMng>(keymng);

builder.Services
    .AddAuthentication(cfg => {
    cfg.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    cfg.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(x => {
    x.RequireHttpsMetadata = false;
    x.SaveToken = false;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8
            .GetBytes(config["JWT"])
        ),
        ValidateIssuer = false,
        ValidateAudience = false,
        ClockSkew = TimeSpan.Zero
        

    };
}).AddCookie(options =>
 {
     options.LoginPath = "~/Login";
     options.LogoutPath = "~/Logout";
 });



// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization();

//builder.Services.AddRazorPages(options =>
////{
////    options.Conventions.ConfigureFilter(model =>
////    {
////        if (model.RelativePath.Contains("/Home/Settings"))
////        {
////            //return new AddHeaderAttribute(
////            //    "OtherPagesPage2Header",
////            //    new string[] { "OtherPages/Page2 Header Value" });
////        }
////        return new EmptyFilter();
////    });

//    //options.Conventions
//    //    .AddFolderApplicationModelConvention("/Home",
//    //        model =>
//    //        {
//    //            model.Filters.Add(
//    //                new GenerateAntiforgeryTokenCookieAttribute());
//    //            model.Filters.Add(
//    //                new DisableFormValueModelBindingAttribute());
//    //        });   
//});


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
app.UseAuthentication();
app.UseAuthorization();

app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;

    if (response.StatusCode == (int)HttpStatusCode.Unauthorized ||
            response.StatusCode == (int)HttpStatusCode.Forbidden)
        response.Redirect("/Account/Login");
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "Settings",
    pattern: "/sets/",
    defaults: new { controller = "Settings", action = "Index" });
app.MapControllerRoute(
    name: "DefBackground",
    pattern: "/dbg/",
    defaults: new { controller = "Settings", action = "Default" });
app.MapControllerRoute(
    name: "CustBackground",
    pattern: "/cbg/",
    defaults: new { controller = "Settings", action = "Custom" });

app.MapControllerRoute(
    name: "Info",
    pattern: "/info/",
    defaults: new { controller = "Home", action = "Info" });

app.MapControllerRoute(
    name: "Timer",
    pattern: "/timer/{m?}/{z?}/{t?}",
    defaults: new { controller = "Home", action = "Timer" });

app.MapControllerRoute(
    name: "DeleteBG",
    pattern: "/deletebg/{bgid?}",
    defaults: new { controller = "Home", action = "DeleteBG" });

app.MapControllerRoute(
    name: "Login",
    pattern: "/login",
    defaults: new { controller = "Account", action = "Login" });

app.MapControllerRoute(
    name: "Logout",
    pattern: "/logout",
    defaults: new { controller = "Account", action = "Logout" });

app.MapControllerRoute(
    name: "UploadPhysical",
    pattern: "/UploadPhysical",
    defaults: new { controller = "Home", action = "UploadPhysical" });

app.MapControllerRoute(
    name: "AvTest",
    pattern: "/avtest",
    defaults: new { controller = "Settings", action = "AvTest" });

app.MapGet("/test", () => "OK!")
    .RequireAuthorization();

app.Run();
