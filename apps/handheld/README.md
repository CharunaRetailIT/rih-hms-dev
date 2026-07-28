# RIT HMS — Waiter Handheld (#107)

Flutter app (iOS + Android, phone + tablet) for waiters to take tab orders. It talks to the
**same live API** as the web app, so orders, items and table merges sync straight to the POS
open-tabs board and the kitchen (KOT).

## Model
1. **Pair the device (once)** — a manager registers this handheld in the back-office
   (Settings → tab devices) which uses one of your licensed device seats and returns a
   **one-time device code**. Enter the workspace + code on first launch (`/api/v1/tab/session`).
2. **Sign in (each shift)** — the waiter signs in with **PIN** (username + PIN) or **magic link**.
   Their name rides on every order. "Switch waiter" hands the device to the next person.

## Run it
Prereqs: [Flutter SDK](https://docs.flutter.dev/get-started/install) (3.3+), Xcode (iOS) / Android Studio (Android).

```bash
cd apps/handheld
flutter create .          # generates the android/ ios/ platform folders (keeps lib/ + pubspec)
flutter pub get
flutter run               # pick a device/simulator
```

Point at a different server (default is the live test deploy):

```bash
flutter run --dart-define=HMS_BASE_URL=https://hms.retailit.lk
```

## Build for release
```bash
flutter build apk         # Android
flutter build ios         # iOS (then archive in Xcode for the App Store)
```

## What's implemented
- Device pairing (licensing) + waiter PIN / magic-link sign-in (RIT-green theme).
- Open-tabs list for the device's outlet; new tab (takeaway or pick a table); **merge tabs**.
- Availability-aware menu (86'd items greyed + can't be added, from #112); add items; **Send to Kitchen**.
- Responsive: tablet shows menu + order side-by-side; phone stacks them.

## Follow-ups (not yet in this scaffold)
- QR-scan pairing (instead of typing the code); biometric unlock; push notifications.
- Item modifiers / serving-size variants on add (POS has these; handheld adds the base item for now).
- Magic-link deep-linking (currently: request link → paste the `token=` code from the email).
- Offline queue for spotty Wi-Fi.
