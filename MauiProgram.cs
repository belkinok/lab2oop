using Microsoft.Extensions.Logging;

namespace Lab2_DayOfFaculty;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();
        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<Services.XmlDocumentService>();
        builder.Services.AddSingleton<Services.XmlSearchService>();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}
