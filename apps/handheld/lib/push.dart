import 'package:firebase_messaging/firebase_messaging.dart';
import 'package:flutter/foundation.dart' show defaultTargetPlatform, TargetPlatform;
import 'package:flutter/material.dart';
import 'api.dart';

/// Must be a top-level function (FCM invokes it in a separate background isolate).
/// Nothing to do here — a notification-payload message shows an OS notification
/// automatically when the app isn't in the foreground; this just lets FCM confirm
/// the app can wake for data-only messages too.
@pragma('vm:entry-point')
Future<void> firebaseMessagingBackgroundHandler(RemoteMessage message) async {}

/// Push notifications (#floor-push) — a guest order landing on the waiter's floor
/// alerts them here even with the app backgrounded or fully closed. Foreground
/// messages don't auto-show an OS notification (iOS/Android both suppress that),
/// so those are surfaced as an in-app banner instead via [messengerKey].
class PushService {
  static final GlobalKey<ScaffoldMessengerState> messengerKey = GlobalKey<ScaffoldMessengerState>();
  static bool _listening = false;

  /// Requests permission, registers this device's FCM token with the API, and starts
  /// listening for messages. Safe to call repeatedly (e.g. on every sign-in) — best-effort:
  /// any failure (permission denied, Firebase not configured yet, offline) is swallowed so
  /// it never blocks sign-in.
  static Future<void> init() async {
    try {
      final settings = await FirebaseMessaging.instance.requestPermission(
        alert: true, badge: true, sound: true,
      );
      if (settings.authorizationStatus == AuthorizationStatus.denied) return;

      final platform = defaultTargetPlatform == TargetPlatform.iOS ? 'ios' : 'android';
      final token = await FirebaseMessaging.instance.getToken();
      if (token != null) {
        await Api.registerDeviceToken(token, platform);
      }
      FirebaseMessaging.instance.onTokenRefresh.listen((t) {
        Api.registerDeviceToken(t, platform).catchError((_) {});
      });

      if (!_listening) {
        _listening = true;
        FirebaseMessaging.onMessage.listen(_showForegroundBanner);
        FirebaseMessaging.onBackgroundMessage(firebaseMessagingBackgroundHandler);
      }
    } catch (_) {
      // Firebase not configured yet (placeholder firebase_options.dart), permission
      // denied, or offline — the app works fine without push, just quieter.
    }
  }

  /// Best-effort — called on sign-out so a shared/handed-off device stops getting
  /// pushes meant for the waiter who just signed out.
  static Future<void> unregister() async {
    try {
      final token = await FirebaseMessaging.instance.getToken();
      if (token != null) await Api.unregisterDeviceToken(token);
    } catch (_) {}
  }

  static void _showForegroundBanner(RemoteMessage message) {
    final title = message.notification?.title ?? message.data['title'] ?? 'RIT HMS';
    final body = message.notification?.body ?? message.data['body'] ?? '';
    messengerKey.currentState?.showSnackBar(SnackBar(
      content: Text(body.isEmpty ? title : '$title\n$body'),
      duration: const Duration(seconds: 5),
    ));
  }
}
