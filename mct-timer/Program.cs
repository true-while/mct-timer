using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using mct_timer.Models;
using System;
using System.Configuration;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<WebSettingsContext>(options =>
    options.UseCosmos(builder.Configuration.GetConnectionString("WebSettingsContext") ?? throw new InvalidOperationException("Connection string 'WebSettingsContext' not found."), "webapp"));



var config = builder.Configuration.GetSection("ConfigMng");

builder.Services.Configure<ConfigMng>(config);

// Blob generator
BlobRepo blob = new BlobRepo(config["StorageAccountString"], config["Container"]);
builder.Services.AddSingleton<IBlobRepo>(blob);

// Dalle generator
DalleGenerator dalleGen = new DalleGenerator(config["OpenAIEndpoint"], config["OpenAIKey"], config["OpenAIModel"]);
builder.Services.AddSingleton<IDalleGenerator>(dalleGen);

// Add services to the container.
builder.Services.AddControllersWithViews();

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

app.Run();
