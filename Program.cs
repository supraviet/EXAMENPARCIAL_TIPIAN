using Microsoft.EntityFrameworkCore;
using EXAPARCIALALVARO.Data;
using Microsoft.AspNetCore.Identity;
using EXAPARCIALALVARO.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configurar MemoryCache
builder.Services.AddMemoryCache();

// Configurar Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = "EXAPARCIALALVARO.Session";
});

// Registrar servicios personalizados
builder.Services.AddScoped<IMemoryCacheService, MemoryCacheService>();
builder.Services.AddScoped<ICursosService, CursosService>();

// CONFIGURACIÓN MEJORADA DE IDENTITY
builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false;
    // Configuraciones más flexibles para desarrollo
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 3;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

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
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// SEED MEJORADO DE USUARIO ADMINISTRADOR
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        
        // Asegurar que la base de datos esté creada
        await context.Database.MigrateAsync();
        
        // Crear rol de Coordinador si no existe
        if (!await roleManager.RoleExistsAsync("Coordinador"))
        {
            await roleManager.CreateAsync(new IdentityRole("Coordinador"));
            Console.WriteLine("✅ Rol 'Coordinador' creado");
        }
        
        // Crear usuario coordinador si no existe
        var coordinadorEmail = "coordinador@universidad.edu";
        var coordinadorUser = await userManager.FindByEmailAsync(coordinadorEmail);
        if (coordinadorUser == null)
        {
            coordinadorUser = new IdentityUser
            {
                UserName = coordinadorEmail,
                Email = coordinadorEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(coordinadorUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(coordinadorUser, "Coordinador");
                Console.WriteLine($"✅ Usuario coordinador creado: {coordinadorEmail}");
            }
            else
            {
                Console.WriteLine($"❌ Error creando usuario coordinador: {string.Join(", ", result.Errors.Select(e => e.Description))}");
            }
        }
        else
        {
            // Asegurar que el usuario existente tenga el rol
            if (!await userManager.IsInRoleAsync(coordinadorUser, "Coordinador"))
            {
                await userManager.AddToRoleAsync(coordinadorUser, "Coordinador");
                Console.WriteLine($"✅ Rol coordinador asignado a usuario existente: {coordinadorEmail}");
            }
        }

        // Crear un usuario estudiante de ejemplo
        var estudianteEmail = "estudiante@universidad.edu";
        var estudianteUser = await userManager.FindByEmailAsync(estudianteEmail);
        if (estudianteUser == null)
        {
            estudianteUser = new IdentityUser
            {
                UserName = estudianteEmail,
                Email = estudianteEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(estudianteUser, "Estudiante123!");
            if (result.Succeeded)
            {
                Console.WriteLine($"✅ Usuario estudiante creado: {estudianteEmail}");
            }
        }

        Console.WriteLine("🎉 Base de datos inicializada correctamente");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error occurred seeding the database.");
    }
}

Console.WriteLine("🚀 Aplicación iniciada correctamente");
app.Run();