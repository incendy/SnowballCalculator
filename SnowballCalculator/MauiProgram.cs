using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;

namespace SnowballCalculator;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().UseMauiCommunityToolkitMediaElement().ConfigureFonts(fonts =>
        {
            fonts.AddFont("Inter-Black.ttf", "InterBlack");
            fonts.AddFont("Inter-Bold.ttf", "InterBold");
            fonts.AddFont("Inter-ExtraBold.ttf", "InterExtraBold");
            fonts.AddFont("Inter-ExtraLight.ttf", "InterExtraLight");
            fonts.AddFont("Inter-Light.ttf", "InterLight");
            fonts.AddFont("Inter-Medium.ttf", "InterMedium");
            fonts.AddFont("Inter-Regular.ttf", "InterRegular");
            fonts.AddFont("Inter-SemiBold.ttf", "InterSemiBold");
            fonts.AddFont("Inter-Thin.ttf", "InterThin");
        }).UseMauiCommunityToolkit();
#if DEBUG
        builder.Logging.AddDebug();
#endif
        return builder.Build();
    }
}