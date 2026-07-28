import 'dart:convert';
import 'package:shared_preferences/shared_preferences.dart';

/// Persisted state: the one-time DEVICE pairing (licensing + outlet) and the current
/// WAITER session (who's signed in). Pairing survives sign-out; only the waiter changes.
class Store {
  static const _kPairing = 'hms.pairing';
  static const _kWaiter = 'hms.waiter';

  // --- Device pairing (licensing) ---
  static Future<void> savePairing(Map<String, dynamic> p) async {
    final sp = await SharedPreferences.getInstance();
    await sp.setString(_kPairing, jsonEncode(p));
  }

  static Future<Map<String, dynamic>?> pairing() async {
    final sp = await SharedPreferences.getInstance();
    final s = sp.getString(_kPairing);
    return s == null ? null : jsonDecode(s) as Map<String, dynamic>;
  }

  static Future<void> clearPairing() async {
    final sp = await SharedPreferences.getInstance();
    await sp.remove(_kPairing);
    await sp.remove(_kWaiter);
  }

  // --- Waiter session (identity) ---
  static Future<void> saveWaiter(Map<String, dynamic> w) async {
    final sp = await SharedPreferences.getInstance();
    await sp.setString(_kWaiter, jsonEncode(w));
  }

  static Future<Map<String, dynamic>?> waiter() async {
    final sp = await SharedPreferences.getInstance();
    final s = sp.getString(_kWaiter);
    return s == null ? null : jsonDecode(s) as Map<String, dynamic>;
  }

  static Future<void> clearWaiter() async {
    final sp = await SharedPreferences.getInstance();
    await sp.remove(_kWaiter);
  }
}
