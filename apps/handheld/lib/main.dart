import 'dart:async';
import 'package:firebase_core/firebase_core.dart';
import 'package:flutter/material.dart';
import 'api.dart';
import 'branding.dart';
import 'firebase_options.dart';
import 'push.dart';
import 'store.dart';
import 'screens/pairing_screen.dart';
import 'screens/login_screen.dart';
import 'screens/home_screen.dart';

// RIT HMS brand
const ritGreen = Color(0xFF15803D);
const ritGreenDark = Color(0xFF0F5C2E);

Future<void> main() async {
  WidgetsFlutterBinding.ensureInitialized();
  try {
    await Firebase.initializeApp(options: DefaultFirebaseOptions.currentPlatform);
  } catch (_) {
    // Placeholder firebase_options.dart (flutterfire configure not run yet) or offline —
    // the app works fine without push, PushService.init() below just quietly no-ops.
  }
  runApp(const HandheldApp());
}

class HandheldApp extends StatelessWidget {
  const HandheldApp({super.key});
  @override
  Widget build(BuildContext context) {
    final scheme = ColorScheme.fromSeed(seedColor: ritGreen, primary: ritGreen, brightness: Brightness.light);
    return MaterialApp(
      title: 'RIT HMS Handheld',
      debugShowCheckedModeBanner: false,
      scaffoldMessengerKey: PushService.messengerKey,
      theme: ThemeData(
        useMaterial3: true,
        colorScheme: scheme,
        scaffoldBackgroundColor: const Color(0xFFF1F5F9),
        appBarTheme: const AppBarTheme(backgroundColor: ritGreen, foregroundColor: Colors.white, elevation: 0),
        filledButtonTheme: FilledButtonThemeData(
          style: FilledButton.styleFrom(backgroundColor: ritGreen, minimumSize: const Size.fromHeight(52),
            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12))),
        ),
        inputDecorationTheme: InputDecorationTheme(
          filled: true, fillColor: Colors.white,
          border: OutlineInputBorder(borderRadius: BorderRadius.circular(12)),
        ),
      ),
      home: const RootGate(),
    );
  }
}

/// Decides the screen: not paired → Pairing; paired but no waiter → Login; else → Home.
class RootGate extends StatefulWidget {
  const RootGate({super.key});
  @override
  State<RootGate> createState() => _RootGateState();
}

class _RootGateState extends State<RootGate> {
  bool _loading = true;
  Map<String, dynamic>? _pairing;
  Map<String, dynamic>? _waiter;

  @override
  void initState() { super.initState(); _reload(); }

  Future<void> _reload() async {
    var p = await Store.pairing();
    // Refresh the device session on launch so the tenant logo + outlet stay current
    // (best-effort; keep the stored pairing if the network/device check fails).
    final token = p?['deviceToken'] as String?;
    if (p != null && token != null && token.isNotEmpty) {
      try {
        final r = await Api.pair(p['slug'] as String? ?? '', token);
        final tenant = (r['tenant'] as Map?) ?? {};
        p = {
          ...p,
          'displayName': tenant['displayName'] ?? p['displayName'],
          'locationId': r['locationId'] ?? p['locationId'],
          'logoUrl': tenant['logoUrl'],
          'deviceName': r['deviceName'] ?? p['deviceName'],
        };
        await Store.savePairing(p);
      } catch (_) { /* offline or revoked — keep the stored pairing */ }
    }
    final w = await Store.waiter();
    Api.token = w?['accessToken'] as String?;
    if (mounted) setState(() { _pairing = p; _waiter = w; _loading = false; });
    if (w != null) unawaited(PushService.init());   // #floor-push — best-effort, never blocks sign-in
  }

  @override
  Widget build(BuildContext context) {
    if (_loading) return const RitSplash();
    if (_pairing == null) return PairingScreen(onPaired: _reload);
    if (_waiter == null) return LoginScreen(pairing: _pairing!, onSignedIn: _reload);
    return HomeScreen(pairing: _pairing!, waiter: _waiter!, onSignedOut: _reload, onUnpair: _reload);
  }
}
