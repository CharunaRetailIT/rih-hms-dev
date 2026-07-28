// PLACEHOLDER — generated file, but not yet by FlutterFire.
//
// Run this once (needs your own Firebase login — see README):
//   dart pub global activate flutterfire_cli
//   flutterfire configure
// It will OVERWRITE this file with your real project's API keys/app IDs. Until then, the
// values below are fake and push notifications will silently no-op (Api.registerDeviceToken
// fails quietly, same as the rest of the app's "offline" handling).
//
// ignore_for_file: type=lint
import 'package:firebase_core/firebase_core.dart' show FirebaseOptions;
import 'package:flutter/foundation.dart' show defaultTargetPlatform, kIsWeb, TargetPlatform;

class DefaultFirebaseOptions {
  static FirebaseOptions get currentPlatform {
    if (kIsWeb) {
      throw UnsupportedError('DefaultFirebaseOptions have not been configured for web — run `flutterfire configure`.');
    }
    switch (defaultTargetPlatform) {
      case TargetPlatform.android:
        return android;
      case TargetPlatform.iOS:
        return ios;
      default:
        throw UnsupportedError('DefaultFirebaseOptions are not supported for this platform.');
    }
  }

  static const FirebaseOptions android = FirebaseOptions(
    apiKey: 'AIzaSyBl53zUsIxl3TGnMDY5h-2uZvFGpcVJ_QQ',
    appId: '1:701155706679:android:d66a9872ee6edbcbcdfcb2',
    messagingSenderId: '701155706679',
    projectId: 'rit-hms-service',
    storageBucket: 'rit-hms-service.firebasestorage.app',
  );
  static const FirebaseOptions ios = FirebaseOptions(
    apiKey: 'AIzaSyC3ibxyu0TYpIdki-1Ah4ZCbvZlbUXGpeI',
    appId: '1:701155706679:ios:79b8fa2fcdbb18c3cdfcb2',
    messagingSenderId: '701155706679',
    projectId: 'rit-hms-service',
    storageBucket: 'rit-hms-service.firebasestorage.app',
    iosBundleId: 'lk.retailit.ritHandheld',
  );
}
