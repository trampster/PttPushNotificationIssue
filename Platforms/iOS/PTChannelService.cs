using AVFoundation;
using Foundation;
using PushToTalk;
using UIKit;

namespace PttPushNotificationIssue;

// Supress NSObject memory leak warnings as this is a 
// singleton was a scope that lasts the whole lifecycle
// of the app
#pragma warning disable MEM0002
#pragma warning disable MEM0003

public enum AudioSessionState
{
   Activated,
   Deactivated,
   Deactivating
};

public class PTChannelService :
   NSObject,
   IPTChannelManagerDelegate,
   IPTChannelRestorationDelegate,
   IPTChannelService
{
   readonly NSUuid _channelGuid;
   string? _apnsToken;

   const string LEAVE_CHANNEL_MESSAGE_ID = "leave_ptt_channel_message";

   bool _inChannel = false;

   public PTChannelService()
   {
      // this uuid uniquely identifies the iOS PTT channel used
      // it needs to be hardcoded so we can rejoin the existing channel
      // after app uninstall/reinstall, otherwise it creates a new one
      // and the user collects channels.
      _channelGuid = new NSUuid("6c46a0ff-24f3-49cf-b373-0048c3e794df");
   }

   PTChannelManager? ChannelManager
   {
      get;
      set;
   }

   Task ClearRemoteParticipant()
   {
      return ChannelManager!.SetActiveRemoteParticipantAsync(null, _channelGuid);
   }

   async Task OnStarting() => await UpdateServiceStatus();

   Task UpdateServiceStatus()
   {
      return ChannelManager!.SetServiceStatusAsync(
         PTServiceStatus.Ready,
         _channelGuid);
   }

   Task SetChannelDescriptor(string channelName)
   {
      return ChannelManager!.SetChannelDescriptorAsync(CreateChannelDescriptor(channelName), _channelGuid);
   }

   async Task SetRemoteParticipant(string name)
   {
      await ChannelManager!.SetActiveRemoteParticipantAsync(new PTParticipant(name, null), _channelGuid);
   }

   public async Task SetupChannel()
   {
      try
      {
         ChannelManager = await PTChannelManager.CreateAsync(this, this);

         if(!_inChannel)
         {
            ChannelManager!.RequestJoinChannel(_channelGuid, CreateChannelDescriptor("Test Channel"));
         }
      }
      catch(NSErrorException exception)
      {
         Console.WriteLine($"Exception occured setuping up channel {exception.Code}");
         throw;
      }

   }


   PTChannelDescriptor CreateChannelDescriptor(string title)
   {
      return new PTChannelDescriptor(title, null);
   }

   public void DidActivateAudioSession(PTChannelManager channelManager, AVAudioSession audioSession)
   {
      // Warning: This gets called from a thread pool thread
      Console.WriteLine("DidActivateAudioSession");
   }

   public void DidBeginTransmitting(PTChannelManager channelManager, NSUuid channelUuid, PTChannelTransmitRequestSource source)
   {
      Console.WriteLine("DidBeginTransmitting");
   }

   enum PTChannelError
   {
      Unknown = 0,
      ChannelNotFound = 1,
      ChannelLimitReached = 2,
      CallActive = 3,
      TransmissionInProgress = 4,
      TransmissionNotFound = 5,
      AppNotForeground = 6,
      DeviceManagementRestriction = 7,
      ScreenTimeRestriction = 8,
      TransmissionNotAllowed = 9
   }

   [Export("channelManager:failedToJoinChannelWithUUID:error:")]
   public void FailedToJoinChannel(PTChannelManager channelManager, NSUuid channelUuid, NSError error)
   {
      Console.Error.WriteLine($"Failed to join channel with error {error.Code}");
   }

   [Export("channelManager:failedToLeaveChannelWithUUID:error:")]
   public void FailedToLeaveChannel(PTChannelManager channelManager, NSUuid channelUuid, NSError error)
   {
      Console.Error.WriteLine("Failed to leave channel with error {ErrorCode}", error.Code);
   }

   [Export("channelManager:failedToBeginTransmittingInChannelWithUUID:error:")]
   public void FailedToBeginTransmittingInChannel(PTChannelManager channelManager, NSUuid channelUuid, NSError error)
   {
      Console.Error.WriteLine($"Failed to begin transmitting in channel with error {error.Code}");
   }

   [Export("channelManager:failedToStopTransmittingInChannelWithUUID:error:")]
   public void FailedToStopTransmittingInChannel(PTChannelManager channelManager, NSUuid channelUuid, NSError error)
   {
      Console.Error.WriteLine($"Failed to stop transmitting in channel with error {error.Code}");
   }

   public void DidDeactivateAudioSession(PTChannelManager channelManager, AVAudioSession audioSession)
   {
      Console.WriteLine("DidDeactivateAudioSession");
   }

   public void DidEndTransmitting(PTChannelManager channelManager, NSUuid channelUuid, PTChannelTransmitRequestSource source)
   {
      Console.WriteLine("DidEndTransmitting");
   }

   public async void DidJoinChannel(PTChannelManager channelManager, NSUuid channelUuid, PTChannelJoinReason reason)
   {
      // This is called by the OS when we restore from a reboot, but not from swiping the app away
      _inChannel = true;
      await channelManager.SetTransmissionModeAsync(PTTransmissionMode.FullDuplex, _channelGuid);
   }

   public void DidLeaveChannel(PTChannelManager channelManager, NSUuid channelUuid, PTChannelLeaveReason reason)
   {
      Console.WriteLine("DidLeaveChannel");
   }

   string GetPayloadEntry(NSDictionary<NSString, NSObject> payload, string entry)
   {
      NSObject entryOut;
      return payload.TryGetValue(new NSString(entry), out entryOut) ? entryOut.ToString() : "";
   }

   public PTPushResult IncomingPushResult(PTChannelManager channelManager, NSUuid channelUuid, NSDictionary<NSString, NSObject> pushPayload)
   {
      Console.WriteLine("Received Push");
      MainThread.BeginInvokeOnMainThread(async () =>
      {
         ReceivedPush?.Invoke(this, EventArgs.Empty);
         await Task.Delay(3000); // simulate receiving for 3 seconds
         ClearRemoteParticipant(); 
      });
      return PTPushResult.Create(new PTParticipant("Bob", null));
   }

   public void ReceivedEphemeralPushToken(PTChannelManager channelManager, NSData pushToken)
   {
      string apnsToken = BitConverter.ToString(pushToken.ToArray()).Replace("-", "").ToLower();
      Console.WriteLine($"StartupLogging ReceivedEphemeralPushToken {apnsToken}");
      MainThread.BeginInvokeOnMainThread(() =>
      {
         _apnsToken = apnsToken;
         ApnsTokenChanged?.Invoke(this, _apnsToken);
      });
   }

   public event EventHandler<string> ApnsTokenChanged;
   public event EventHandler ReceivedPush;

   public PTChannelDescriptor Create(NSUuid channelUuid)
   {
      var channelUuidString = channelUuid.AsString();
      if (_channelGuid.AsString() != channelUuidString)
      {
         Console.Error.WriteLine("Attempted to resume channel we don't recognize");
      }

      Console.WriteLine("Restoring existing channel");

      return CreateChannelDescriptor("Created");
   }
}

#pragma warning restore MEM0002
#pragma warning restore MEM0003
