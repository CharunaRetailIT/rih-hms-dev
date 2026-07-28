import 'dart:convert';
import 'package:http/http.dart' as http;

/// Thin REST client for the RIT HMS API. The handheld reuses the SAME endpoints as the
/// web app: device pairing (/tab/session), staff sign-in (/auth/pin, /auth/magic-link),
/// and the shared orders/menu/availability APIs — so orders sync to POS + KOT automatically.
class Api {
  /// Point this at your server. Defaults to the live test deploy.
  static String baseUrl = const String.fromEnvironment('HMS_BASE_URL',
      defaultValue: 'http://localhost:5000');

  /// Staff JWT for the signed-in waiter (set after PIN / magic-link sign-in).
  static String? token;

  static Map<String, String> _headers({bool auth = true}) => {
        'Content-Type': 'application/json',
        if (auth && token != null) 'Authorization': 'Bearer $token',
      };

  static Uri _u(String path) => Uri.parse('$baseUrl$path');

  static dynamic _body(http.Response r) {
    if (r.statusCode >= 200 && r.statusCode < 300) {
      return r.body.isEmpty ? null : jsonDecode(r.body);
    }
    String msg = 'Error ${r.statusCode}';
    try {
      final j = jsonDecode(r.body);
      if (j is Map && j['error'] is String) msg = j['error'];
    } catch (_) {}
    throw ApiException(msg, r.statusCode);
  }

  // ── Device pairing (licensing) ──
  static Future<Map<String, dynamic>> pair(
      String tenantSlug, String deviceToken) async {
    final r = await http.post(_u('/api/v1/tab/session'),
        headers: _headers(auth: false),
        body: jsonEncode(
            {'tenantSlug': tenantSlug.trim(), 'token': deviceToken.trim()}));
    return _body(r) as Map<String, dynamic>;
  }

  // ── Waiter sign-in ──
  static Future<Map<String, dynamic>> pinLogin(
      String tenantSlug, String username, String pin) async {
    final r = await http.post(_u('/api/v1/auth/pin'),
        headers: _headers(auth: false),
        body: jsonEncode({
          'tenantSlug': tenantSlug.trim(),
          'username': username.trim(),
          'pin': pin.trim()
        }));
    return _body(r) as Map<String, dynamic>;
  }

  static Future<void> requestMagicLink(String tenantSlug, String email) async {
    final r = await http.post(_u('/api/v1/auth/magic-link'),
        headers: _headers(auth: false),
        body: jsonEncode(
            {'tenantSlug': tenantSlug.trim(), 'email': email.trim()}));
    _body(r);
  }

  static Future<Map<String, dynamic>> exchangeMagic(String magicToken) async {
    final r = await http.post(_u('/api/v1/auth/exchange'),
        headers: _headers(auth: false),
        body: jsonEncode({'token': magicToken.trim()}));
    return _body(r) as Map<String, dynamic>;
  }

  // ── Push (#floor-push): FCM device token, registered on sign-in / refresh ──
  static Future<void> registerDeviceToken(String token, String platform) async {
    await http.post(_u('/api/v1/push/device-token'),
        headers: _headers(),
        body: jsonEncode({'token': token, 'platform': platform}));
  }

  static Future<void> unregisterDeviceToken(String token) async {
    await http.post(_u('/api/v1/push/device-token/unregister'),
        headers: _headers(), body: jsonEncode({'token': token}));
  }

  // ── Floor coverage (#floor-push): which floor(s) THIS waiter covers ──
  static Future<List<dynamic>> floors(String locationId) async =>
      _body(await http.get(_u('/api/v1/floors?locationId=$locationId'),
          headers: _headers())) as List<dynamic>;

  static Future<List<dynamic>> myFloors() async =>
      _body(await http.get(_u('/api/v1/me/floors'), headers: _headers()))
          as List<dynamic>;

  static Future<void> setMyFloors(List<String> floorIds) async {
    final r = await http.put(_u('/api/v1/me/floors'),
        headers: _headers(), body: jsonEncode({'floorIds': floorIds}));
    _body(r);
  }

  // ── Menu + availability (location-scoped) ──
  static Future<List<dynamic>> products() async =>
      _body(await http.get(_u('/api/v1/products?activeOnly=true'),
          headers: _headers())) as List<dynamic>;

  static Future<List<dynamic>> categories() async =>
      _body(await http.get(_u('/api/v1/categories'), headers: _headers()))
          as List<dynamic>;

  static Future<List<dynamic>> availability(String locationId) async =>
      _body(await http.get(_u('/api/v1/availability?locationId=$locationId'),
          headers: _headers())) as List<dynamic>;

  static Future<List<dynamic>> locations() async =>
      _body(await http.get(_u('/api/v1/locations'), headers: _headers()))
          as List<dynamic>;

  // ── Tables + open tabs ──
  // Handheld tab strip: light rows + per-order kitchenStatus (building|preparing|ready|served).
  static Future<List<dynamic>> openOrders(String locationId) async => _body(
      await http.get(_u('/api/v1/orders/open-tabs?locationId=$locationId'),
          headers: _headers())) as List<dynamic>;

  static Future<List<dynamic>> tables(String locationId) async =>
      _body(await http.get(_u('/api/v1/tables?locationId=$locationId'),
          headers: _headers())) as List<dynamic>;

  // ── Order lifecycle (mirrors POS) ──
  static Future<Map<String, dynamic>> createOrder({
    required String locationId,
    String orderType = 'dine_in',
    String? tableLabel,
    String? tableId,
    int covers = 2,
  }) async {
    final r = await http.post(_u('/api/v1/orders'),
        headers: _headers(),
        body: jsonEncode({
          'locationId': locationId,
          'orderType': orderType,
          'covers': covers,
          if (tableLabel != null) 'tableLabel': tableLabel,
          if (tableId != null) 'tableId': tableId,
        }));
    return _body(r) as Map<String, dynamic>;
  }

  static Future<Map<String, dynamic>> getOrder(String id) async =>
      _body(await http.get(_u('/api/v1/orders/$id'), headers: _headers()))
          as Map<String, dynamic>;

  static Future<Map<String, dynamic>> addItem(String orderId, String productId,
      {int quantity = 1, String? variantId}) async {
    final r = await http.post(_u('/api/v1/orders/$orderId/items'),
        headers: _headers(),
        body: jsonEncode({
          'productId': productId,
          'quantity': quantity,
          if (variantId != null) 'variantId': variantId
        }));
    return _body(r) as Map<String, dynamic>;
  }

  static Future<Map<String, dynamic>> updateQty(
      String orderId, String itemId, int quantity) async {
    final r = await http.put(_u('/api/v1/orders/$orderId/items/$itemId'),
        headers: _headers(), body: jsonEncode({'quantity': quantity}));
    return _body(r) as Map<String, dynamic>; // qty 0 removes the line
  }

  static Future<Map<String, dynamic>> removeItem(
          String orderId, String itemId) async =>
      _body(await http.delete(_u('/api/v1/orders/$orderId/items/$itemId'),
          headers: _headers())) as Map<String, dynamic>;

  static Future<Map<String, dynamic>> confirm(String orderId) async =>
      _body(await http.post(_u('/api/v1/orders/$orderId/confirm'),
          headers: _headers())) as Map<String, dynamic>;

  /// Split chosen line quantities onto a new bill at the same table (#69). Returns the NEW bill.
  static Future<Map<String, dynamic>> split(
      String orderId, List<Map<String, dynamic>> lines) async {
    final r = await http.post(_u('/api/v1/orders/$orderId/split'),
        headers: _headers(), body: jsonEncode({'lines': lines}));
    return _body(r) as Map<String, dynamic>;
  }

  /// Close the bill — settle with a single tender for the full amount.
  static Future<Map<String, dynamic>> settle(
      String orderId, String payType, num amount) async {
    final r = await http.post(_u('/api/v1/orders/$orderId/settle'),
        headers: _headers(),
        body: jsonEncode({
          'payments': [
            {'payType': payType, 'amount': amount}
          ]
        }));
    return _body(r) as Map<String, dynamic>;
  }

  static Future<Map<String, dynamic>> merge(
      String orderId, String sourceOrderId) async {
    final r = await http.post(_u('/api/v1/orders/$orderId/merge'),
        headers: _headers(),
        body: jsonEncode({'sourceOrderId': sourceOrderId}));
    return _body(r) as Map<String, dynamic>;
  }
}

class ApiException implements Exception {
  final String message;
  final int status;
  ApiException(this.message, this.status);
  @override
  String toString() => message;
}
