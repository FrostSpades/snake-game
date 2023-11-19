// Authors: Ethan Andrews and Mary Garfield
// Creation of the maui program.
// University of Utah

namespace SnakeGame;

/// <summary>
/// Class representing the maui program
/// </summary>
public static class MauiProgram
{
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <returns></returns>
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        return builder.Build();
    }
}

