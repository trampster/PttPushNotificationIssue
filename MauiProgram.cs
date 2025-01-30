using Microsoft.Extensions.Logging;

namespace PttPushNotificationIssue;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp(IServiceCollection serviceCollection)
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			});

		builder.Services.AddSingleton<PttPushViewModel>();

		foreach(var service in serviceCollection)
		{
			builder.Services.Add(service);
		}

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
