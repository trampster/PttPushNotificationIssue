using Foundation;

namespace PttPushNotificationIssue;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp()
	{
		IServiceCollection services = new ServiceCollection();
    	services.AddSingleton<IPTChannelService, PTChannelService>();
		return MauiProgram.CreateMauiApp(services);
	}
}
