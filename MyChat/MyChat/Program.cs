using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyChat.Data;
using MyChat.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; options.Password.RequiredLength = 6; options.Password.RequireUppercase = true; 
    options.Password.RequireLowercase = true; options.Password.RequireDigit = true; options.Password.RequireNonAlphanumeric = false; 
}).AddRoles<IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Chat}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
    .WithStaticAssets();

await CreateRoles(app);

await CreateAdmin(app);

app.Run();

static async Task CreateRoles(WebApplication app)
{
    using IServiceScope scope = app.Services.CreateScope();

    RoleManager<IdentityRole> roleManager =
        scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "user", "admin" };

    foreach (string role in roles)
    {
        bool roleExists = await roleManager.RoleExistsAsync(role);

        if (!roleExists)
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

static async Task CreateAdmin(WebApplication app)
{
    using IServiceScope scope = app.Services.CreateScope();

    UserManager<User> userManager =
        scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    const string adminEmail = "admin@admin.com";
    const string adminUserName = "admin";
    const string adminPassword = "Admin123";

    User? admin = await userManager.FindByEmailAsync(adminEmail);

    if (admin == null)
    {
        admin = new User
        {
            UserName = adminUserName,
            Email = adminEmail,
            BirthDate = new DateTime(2000, 1, 1),
            MessagesCount = 0
        };

        IdentityResult result =
            await userManager.CreateAsync(admin, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(admin, "admin");
        }
    }
    else
    {
        bool isAdmin = await userManager.IsInRoleAsync(admin, "admin");

        if (!isAdmin)
        {
            await userManager.AddToRoleAsync(admin, "admin");
        }
    }
}