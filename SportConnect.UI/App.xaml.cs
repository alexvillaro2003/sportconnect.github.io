using System.IO;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SportConnect.Data;
using SportConnect.Services.Achievements;
using SportConnect.Services.Implementations;
using SportConnect.Services.Interfaces;
using SportConnect.UI.ViewModels;
using SportConnect.UI.Views;

namespace SportConnect.UI;

public partial class App : Application
{
    public static IServiceProvider Services { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        // Inicializar BD (crea esquema y siembra catálogos si están vacíos)
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            DatabaseInitializer.Initialize(db);
        }

        // Lanzar ventana de login
        var login = Services.GetRequiredService<LoginWindow>();
        login.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // ----- Datos -----
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SportConnect", "sportconnect.db");
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"),
            ServiceLifetime.Transient);

        // ----- Sesión: singleton para que toda la app comparta el usuario logueado -----
        services.AddSingleton<ICurrentSession, CurrentSession>();

        // ----- Servicios de negocio -----
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IUserService, UserService>();
        services.AddTransient<IActivityService, ActivityService>();
        services.AddTransient<IChatService, ChatService>();
        services.AddTransient<IStatsService, StatsService>();
        services.AddTransient<IAchievementService, AchievementService>();

        // ----- Reglas de achievements (Strategy pattern) -----
        services.AddTransient<IAchievementRule, MasterCreatorRule>();
        services.AddTransient<IAchievementRule, DedicatedPlayerRule>();
        services.AddTransient<IAchievementRule, FirstStepsRule>();
        services.AddTransient<IAchievementRule, SocialButterflyRule>();

        // ----- ViewModels -----
        services.AddTransient<LoginViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<ActivityFeedViewModel>();
        services.AddTransient<MyActivitiesViewModel>();
        services.AddTransient<ProfileViewModel>();
        services.AddTransient<AdminViewModel>();
        services.AddTransient<CreateActivityViewModel>();
        services.AddTransient<ActivityDetailsViewModel>();
        services.AddTransient<EditActivityViewModel>();

        // ----- Views (las que se construyen vía DI) -----
        services.AddTransient<LoginWindow>();
        services.AddTransient<MainWindow>();
        services.AddTransient<CreateActivityDialog>();
        services.AddTransient<ActivityDetailsDialog>();
        services.AddTransient<EditActivityDialog>();
    }
}
