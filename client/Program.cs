using Azure.Identity;
using client.App_Code;
using client.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureKeyVault(
    new Uri("https://mehujuhlat.vault.azure.net/"),
    new DefaultAzureCredential());

AppSecrets.Initialize(builder.Configuration);
var dbConnectionString = builder.Configuration["dbconnection"];

builder.Services.AddControllersWithViews();
builder.Services.AddHostedService<CleanupUserService>();
builder.Services.AddDbContext<MehujuhlatContext>(options =>
options.UseSqlServer(dbConnectionString));
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<AzureBlobStorageService>();
builder.Services.AddHttpClient<RecaptchaService>();
builder.Services.AddSignalR();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(context =>
            context.User.HasClaim(c => c.Type == "IsAdmin" && c.Value == "True")));
});

var app = builder.Build();
Helper.Initialize(app.Environment);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.MapHub<MsgHub>("/msgHub");
//app.UseSession();
app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{

    endpoints.MapControllerRoute(
    name: "ticketPurchase",
    pattern: "MyTickets/Buy/{Id}/{ticketId}",
    defaults: new { controller = "MyTickets", action = "Buy" });


    endpoints.MapAreaControllerRoute(
    name: "pTicketsIndex",
    areaName: "Admin",
    pattern: "Admin/Ptickets/Index/{Id}/{Page}/{Count}",
    defaults: new { controller = "Ptickets", action = "Index" });

    endpoints.MapControllerRoute(
      name: "areas",
      pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
    );


});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
