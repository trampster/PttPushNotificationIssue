# PttPushNotificationIssue
This repository is a sample app which uses apples push to talk framework to reproduce an issue where the push-to-talk notifications stop arriving after switching networks.

PLATFORM AND VERSION: iOS
Development environment: Other: .net MAUI with vscode
Run-time configuration: iOS 18.1.1

DESCRIPTION OF PROBLEM
APNS notifications of apns-push-type pushtotalk sometimes stop arriving after switching networks.

STEPS TO REPRODUCE
On an iPhone SE (we havn't been able to reproduce on our iPhone 11)
1. Start the APP to register for the APNS push notifications
2. Turn off the WiFi wait for 5 seconds
3. Attempt a push to the app manually using the Push Notifications Console (this should fail, which is fine)
4. Turn on Cellular and wait for it to connect
5. Attempt to push to the app manually using the Push Notifications Console
-> This fails, and all attempts to send an pushtotalk push notifications fail until the we switch network again.

Send a push while offline before connecting to the new network seems to make it happen more often but hard to tell for sure.

The results of the failed push in the console look like this:

```
Delivery LogLast updated: 30/01/25, 16:45:06 GMT+13 Refresh
30 Jan 2025, 16:45:03.661 GMT+13
received by APNS Server

30 Jan 2025, 16:45:03.662 GMT+13
discarded as device was offline
```

The device is actually very much online.

Switching networks again oftern makes things come right. But it doesn't seem to come right by itself.

We can't respond to network changes and do anything as the whole point of using push-to-talk push notifications is to wake up the app when in the background to answer a call, this means we are not running and therefore cannot respond to network changes to try to work arround this issue.

