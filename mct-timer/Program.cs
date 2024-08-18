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
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WebSettingsContext>(options =>
    options.UseCosmos(builder.Configuration.GetConnectionString("WebSettingsContext") ?? throw new InvalidOperationException("Connection string 'WebSettingsContext' not found."), "webapp"));

builder.Services.AddHttpContextAccessor();
//builder.Services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor());


var config = builder.Configuration.GetSection("ConfigMng");

builder.Services.Configure<ConfigMng>(config);

// Blob generator
BlobRepo blob = new BlobRepo(config["StorageAccountString"], config["Container"]);
builder.Services.AddSingleton<IBlobRepo>(blob);

// Dalle generator
DalleGenerator dalleGen = new DalleGenerator(config["OpenAIEndpoint"], config["OpenAIKey"], config["OpenAIModel"]);
builder.Services.AddSingleton<IDalleGenerator>(dalleGen);
builder.Services.AddTransient<AuthService>();



//builder.Services.AddAuthentication();

builder.Services.AddAuthentication();

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
    pattern: "/timer/{m?}/{t?}",
    defaults: new { controller = "Home", action = "Timer" });

//app.MapControllerRoute(
//    name: "Settings",
//    pattern: "/settings",
//    defaults: new { controller = "Web", action = "Index" });




app.MapGet("/test", () => "OK!")
    .RequireAuthorization();

app.Run();
