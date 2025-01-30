namespace PttPushNotificationIssue;

public interface IPTChannelService
{
    Task SetupChannel();
    event EventHandler<string> ApnsTokenChanged;
    event EventHandler ReceivedPush;
}
