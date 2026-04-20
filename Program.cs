using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PortalAcademico.Data;
using PortalAcademico.Models; // 1. AGREGADO: Para reconocer Curso y Matricula

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString)); // Usamos la variable connectionString que ya tienes arriba

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => {
    options.SignIn.RequireConfirmedAccount = false; // 2. CAMBIADO: A false para que te deje loguear sin confirmar email en el examen
    options.Password.RequireDigit = false;          // Opcional: hace las contraseñas más fáciles para pruebas
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- 3. SECCIÓN DE DATOS DE PRUEBA (SEED DATA) ---
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    
    // Esto asegura que la base de datos exista
    context.Database.EnsureCreated();

    // Si no hay cursos, insertamos 3 de prueba
    if (!context.Cursos.Any())
    {
        context.Cursos.AddRange(
            new Curso { Codigo = "MAT101", Nombre = "Cálculo I", Creditos = 5, CupoMaximo = 30, HorarioInicio = new TimeSpan(8, 0, 0), HorarioFin = new TimeSpan(10, 0, 0), Activo = true },
            new Curso { Codigo = "PROG202", Nombre = "Programación II", Creditos = 4, CupoMaximo = 20, HorarioInicio = new TimeSpan(10, 0, 0), HorarioFin = new TimeSpan(12, 0, 0), Activo = true },
            new Curso { Codigo = "FIS303", Nombre = "Física III", Creditos = 4, CupoMaximo = 25, HorarioInicio = new TimeSpan(14, 0, 0), HorarioFin = new TimeSpan(16, 0, 0), Activo = true }
        );
        context.SaveChanges();
    }
}
// -------------------------------------------------

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
app.UseStaticFiles(); // Importante para cargar CSS/JS

app.UseRouting();

app.UseAuthentication(); // 4. ASEGÚRATE de que UseAuthentication esté ANTES de UseAuthorization
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();