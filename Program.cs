using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Razor_RolesDemo.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    policy.RequireRole("Admin"));
});

builder.Services.AddRazorPages(options => 
{
    options.Conventions.AuthorizeFolder("/Admin", "AdminPolicy");
    options.Conventions.AuthorizeFolder("/Member");
});

var connectionString = builder.Configuration.GetConnectionString("RolesDbConnection");
builder.Services.AddDbContext<AuthDbContext>(options => options.UseSqlServer(connectionString));


builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AuthDbContext>();


builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Index";
    options.AccessDeniedPath = "/Index";

});


// Skapa en instans av serviceProvider s� att vi kan komma �t allt som ligger i DI container
using (ServiceProvider serviceProvider = builder.Services.BuildServiceProvider())
{

    // Skapa variabler f�r allt som ligger i DI container
    var context = serviceProvider.GetRequiredService<AuthDbContext>();
    var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // S�kerst�ll att Databasen �r skapad... Annars skapa den!
    context.Database.Migrate();

    if (!roleManager.RoleExistsAsync("Admin").GetAwaiter().GetResult())
    {
        // Skapa en instans av en klass f�r admin
        IdentityRole adminRole = new()
        {
            Name = "Admin",
        };

        // Skapa adminrollen i databasen synkront. Vi vill skapa rollen synkront, eftersom appen ska ej forts�tta byggas innan rollen �r med.
        var createRoleResult = roleManager.CreateAsync(adminRole).GetAwaiter().GetResult(); // GetAwaiter().GetResult(); G�r n�got som �r ett asynkront call till synkront.


        // Om vi kunde skapa adminrollen
        if (createRoleResult.Succeeded)
        {
            // H�mta en anv�ndare med ett username
            IdentityUser? user = userManager.FindByNameAsync("Besim123").GetAwaiter().GetResult();

            // Om vi hittade en user...
            if (user != null)
            {
                // L�gg till den nya adminrollen till anv�ndaren
                var addToRoleResult = userManager.AddToRoleAsync(user, "Admin").GetAwaiter().GetResult();
            }

        }
    }
}


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
