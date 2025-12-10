using Ecommerce.DataAccess;
using Ecommerce.DataAccess.Implementation;
using Ecommerce.Entities.Repositories;
using Ecommerce.Utilities;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDbContextConnection")));

builder.Services.AddIdentity<IdentityUser,IdentityRole>(options=>options.Lockout.DefaultLockoutTimeSpan=TimeSpan.FromHours(1))
	.AddDefaultTokenProviders().AddDefaultUI().
	AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.Configure<StripeData>(builder.Configuration.GetSection("stripe"));
builder.Services.AddRazorPages().AddRazorRuntimeCompilation();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
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

StripeConfiguration.ApiKey = builder.Configuration.GetSection("stripe:Secretkey").Get<string>();

app.UseAuthorization();
app.UseSession();
app.MapRazorPages();
app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");
app.Run();
