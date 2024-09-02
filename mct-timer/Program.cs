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
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddUserSecrets<Program>();

builder.Services.AddDbContext<WebSettingsContext>(options =>
    options.UseCosmos(builder.Configuration.GetConnectionString("WebSettingsContext") ?? throw new InvalidOperationException("Connection string 'WebSettingsContext' not found."), "webapp"));

builder.Services.AddDbContext<UsersContext>(options =>
    options.UseCosmos(builder.Configuration.GetConnectionString("UsersContext") ?? throw new InvalidOperationException("Connection string 'UsersContext' not found."), "webapp"));


builder.Services.AddHttpContextAccessor();
//builder.Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());


var config = builder.Configuration.GetSection("ConfigMng");

builder.Services.Configure<ConfigMng>(config);
builder.Services.AddSingleton<AuthService>();





    builder.Services.AddDbContext<WebSettingsContext>(options =>
        options.UseCosmos(builder.Configuration.GetConnectionString("WebSettingsContext") ?? throw new InvalidOperationException("Connection string 'WebSettingsContext' not found."), "webapp"));

    // Blob generator
    BlobRepo blob = new BlobRepo(config["StorageAccountString"], config["Container"]);
    builder.Services.AddSingleton<IBlobRepo>(blob);

    // Dalle generator
    DalleGenerator dalleGen = new DalleGenerator(config["OpenAIEndpoint"], config["OpenAIKey"], config["OpenAIModel"]);
    builder.Services.AddSingleton<IDalleGenerator>(dalleGen);




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
     options.LoginPath = "Account/Login";
     options.LogoutPath = "Account/Logout";
 });



// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthorization();

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
    name: "inprogress",
    pattern: "/sets/",
    defaults: new { controller = "Home", action = "Inprogress" });

app.MapControllerRoute(
    name: "Info",
    pattern: "/info/",
    defaults: new { controller = "Home", action = "Info" });

app.MapControllerRoute(
    name: "Timer",
    pattern: "/timer/{m?}/{z?}/{t?}",
    defaults: new { controller = "Home", action = "Timer" });

//app.MapControllerRoute(
//    name: "Settings",
//    pattern: "/settings",
//    defaults: new { controller = "Web", action = "Index" });




app.MapGet("/test", () => "OK!")
    .RequireAuthorization();

app.Run();
