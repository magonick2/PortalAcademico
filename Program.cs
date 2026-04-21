using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CONFIGURACIÓN DE SERVICIOS (builder.Services) ---

// Base de Datos SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Redis y Sesión (Pregunta 4)
builder.Services.AddStackExchangeRedisCache(options => {
    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
    options.InstanceName = "PortalAcademico_";
});

builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identidad / Usuarios
builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false;
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

// --- 2. CONSTRUCCIÓN DE LA APLICACIÓN ---
var app = builder.Build();

// --- 3. CONFIGURACIÓN DEL PIPELINE (Middlewares) ---

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

// El orden aquí es CRÍTICO
app.UseAuthentication();
app.UseAuthorization();
app.UseSession(); // Sesión siempre después de Auth

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    if (!context.Cursos.Any())
    {
        context.Cursos.AddRange(
            new Curso { Codigo = "MAT101", Nombre = "Cálculo I", Creditos = 5, CupoMaximo = 30, HorarioInicio = new TimeSpan(8, 0, 0), HorarioFin = new TimeSpan(10, 0, 0), Activo = true },
            new Curso { Codigo = "PROG202", Nombre = "Programación II", Creditos = 4, CupoMaximo = 20, HorarioInicio = new TimeSpan(10, 0, 0), HorarioFin = new TimeSpan(12, 0, 0), Activo = true }
        );
        context.SaveChanges();
    }
}
app.Run();