import 'package:flutter/material.dart';
import '../api.dart';
import '../branding.dart';
import '../money.dart';
import '../push.dart';
import '../store.dart';
import 'floors_screen.dart';
import 'order_screen.dart';

/// Open tabs for the device's outlet. New tab, open a tab, or merge two tabs.
/// Everything here syncs straight to the POS open-tabs board + the kitchen (KOT).
class HomeScreen extends StatefulWidget {
  final Map<String, dynamic> pairing;
  final Map<String, dynamic> waiter;
  final Future<void> Function() onSignedOut;
  final Future<void> Function() onUnpair;
  const HomeScreen({super.key, required this.pairing, required this.waiter, required this.onSignedOut, required this.onUnpair});
  @override
  State<HomeScreen> createState() => _HomeScreenState();
}

class _HomeScreenState extends State<HomeScreen> {
  List<dynamic> _open = [];
  List<dynamic> _tables = [];
  String? _locationName;          // the outlet this device/waiter is working at
  bool _loading = true;
  String? _err;

  String get _locationId => (widget.pairing['locationId'] ?? widget.waiter['homeLocationId'] ?? '') as String;
  String get _waiterName => widget.waiter['name'] as String? ?? 'Waiter';

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    if (_locationId.isEmpty) { setState(() { _loading = false; _err = 'This device has no outlet assigned. Ask your manager to pin it to an outlet.'; }); return; }
    setState(() { _loading = true; _err = null; });
    try {
      final open = await Api.openOrders(_locationId);
      List<dynamic> tables = [];
      try { tables = await Api.tables(_locationId); } catch (_) {}
      if (_locationName == null) {
        try {
          final locs = await Api.locations();
          for (final l in locs) { if (l['id'] == _locationId) { _locationName = (l['name'] ?? l['code']) as String?; break; } }
        } catch (_) {}
      }
      if (mounted) setState(() { _open = open; _tables = tables; _loading = false; });
    } on ApiException catch (e) {
      if (e.status == 401) { await PushService.unregister(); await Store.clearWaiter(); await widget.onSignedOut(); return; }
      if (mounted) setState(() { _err = e.message; _loading = false; });
    } catch (_) { if (mounted) setState(() { _err = 'Could not load orders.'; _loading = false; }); }
  }

  Future<void> _newTab() async {
    final picked = await showModalBottomSheet<Map<String, String?>>(
      context: context, showDragHandle: true,
      builder: (_) => _NewTabSheet(tables: _tables),
    );
    if (picked == null) return;
    try {
      final o = await Api.createOrder(
        locationId: _locationId,
        orderType: picked['tableId'] != null ? 'dine_in' : 'takeaway',
        tableId: picked['tableId'], tableLabel: picked['label'],
      );
      await _openOrder(o['id'] as String);
    } on ApiException catch (e) { _snack(e.message); }
  }

  Future<void> _openOrder(String id) async {
    await Navigator.push(context, MaterialPageRoute(builder: (_) => OrderScreen(orderId: id, locationId: _locationId)));
    _load();
  }

  Future<void> _merge(Map order) async {
    final others = _open.where((o) => o['id'] != order['id']).toList();
    if (others.isEmpty) { _snack('No other open order to merge into.'); return; }
    final target = await showModalBottomSheet<Map>(
      context: context, showDragHandle: true,
      builder: (_) => SafeArea(child: Column(mainAxisSize: MainAxisSize.min, children: [
        const Padding(padding: EdgeInsets.all(16), child: Text('Merge this order into…', style: TextStyle(fontWeight: FontWeight.bold))),
        ...others.map((o) => ListTile(
          title: Text(_label(o)), trailing: Text('LKR ${_money(o['totalAmount'])}'),
          onTap: () => Navigator.pop(context, o as Map))),
      ])),
    );
    if (target == null) return;
    try {
      await Api.merge(target['id'] as String, order['id'] as String);   // source merged into target
      _snack('Merged into ${_label(target)}.');
      _load();
    } on ApiException catch (e) { _snack(e.message); }
  }

  void _snack(String m) { if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(m))); }
  static String _label(Map o) => (o['tableLabel'] as String?)?.isNotEmpty == true ? 'Table ${o['tableLabel']}' : (o['orderNumber'] as String? ?? 'Order');
  static String _money(dynamic v) => money(v);

  /// Colour-coded kitchen readiness for the tab (driven by the order's KOT tickets).
  Widget _statusChip(Map o) {
    final k = o['kitchenStatus'] as String? ?? 'building';
    final (Color fg, Color bg, String t) = switch (k) {
      'ready'     => (const Color(0xFF15803D), const Color(0xFFE7F3EC), 'Ready'),
      'preparing' => (const Color(0xFFB45309), const Color(0xFFFBF0E0), 'In kitchen'),
      'served'    => (const Color(0xFF1D4ED8), const Color(0xFFE6ECFB), 'Served'),
      _           => (const Color(0xFF64748B), const Color(0xFFEEF2F6), 'New'),
    };
    return Container(
      padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
      decoration: BoxDecoration(color: bg, borderRadius: BorderRadius.circular(20), border: Border.all(color: fg)),
      child: Row(mainAxisSize: MainAxisSize.min, children: [
        Container(width: 7, height: 7, decoration: BoxDecoration(color: fg, shape: BoxShape.circle)),
        const SizedBox(width: 5),
        Text(t, style: TextStyle(color: fg, fontSize: 11, fontWeight: FontWeight.w700)),
      ]),
    );
  }

  /// Close the bill — pick a tender and settle the full amount.
  Future<void> _closeBill(Map order) async {
    final total = (order['totalAmount'] ?? 0) as num;
    final pay = await showModalBottomSheet<String>(
      context: context, showDragHandle: true,
      builder: (_) => SafeArea(child: Column(mainAxisSize: MainAxisSize.min, children: [
        Padding(padding: const EdgeInsets.fromLTRB(16, 4, 16, 8), child: Column(children: [
          Text('Close ${_label(order)}', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16)),
          const SizedBox(height: 4),
          Text('LKR ${_money(total)}', style: const TextStyle(fontSize: 24, fontWeight: FontWeight.w900, color: Color(0xFF15803D))),
        ])),
        ListTile(leading: const Icon(Icons.payments_outlined), title: const Text('Cash'), onTap: () => Navigator.pop(context, 'cash')),
        ListTile(leading: const Icon(Icons.credit_card), title: const Text('Card'), onTap: () => Navigator.pop(context, 'card')),
        const SizedBox(height: 8),
      ])),
    );
    if (pay == null) return;
    try {
      await Api.settle(order['id'] as String, pay, total);
      _snack('Bill closed · ${pay == 'cash' ? 'Cash' : 'Card'} · LKR ${_money(total)}');
      _load();
    } on ApiException catch (e) { _snack(e.message); }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      bottomNavigationBar: const PoweredByFooter(),
      appBar: AppBar(
        leadingWidth: 56,
        leading: Padding(
          padding: const EdgeInsets.fromLTRB(12, 8, 0, 8),
          child: Container(
            decoration: BoxDecoration(color: Colors.white, borderRadius: BorderRadius.circular(9)),
            padding: const EdgeInsets.all(4),
            child: TenantLogo(url: widget.pairing['logoUrl'] as String?, size: 28)),
        ),
        title: Column(crossAxisAlignment: CrossAxisAlignment.start, mainAxisSize: MainAxisSize.min, children: [
          Text(widget.pairing['displayName'] as String? ?? 'RIT HMS', style: const TextStyle(fontSize: 17, fontWeight: FontWeight.w600)),
          Row(mainAxisSize: MainAxisSize.min, children: [
            const Icon(Icons.storefront, size: 12, color: Colors.white70),
            const SizedBox(width: 4),
            Text(_locationName ?? 'Loading outlet…', style: const TextStyle(fontSize: 12, color: Colors.white70, fontWeight: FontWeight.w400)),
          ]),
        ]),
        actions: [
          PopupMenuButton<String>(
            onSelected: (v) async {
              if (v == 'floors') {
                Navigator.of(context).push(MaterialPageRoute(builder: (_) => FloorsScreen(locationId: _locationId)));
              }
              if (v == 'waiter') { await PushService.unregister(); await Store.clearWaiter(); await widget.onSignedOut(); }
              if (v == 'device') { await Store.clearPairing(); await widget.onUnpair(); }
            },
            itemBuilder: (_) => [
              PopupMenuItem(enabled: false, child: Text('Signed in: $_waiterName')),
              if (widget.waiter['isServer'] == true && _locationId.isNotEmpty)
                const PopupMenuItem(value: 'floors', child: Text('My Floor Coverage')),
              const PopupMenuItem(value: 'waiter', child: Text('Switch waiter')),
              const PopupMenuItem(value: 'device', child: Text('Switch device')),
            ],
            child: Padding(padding: const EdgeInsets.symmetric(horizontal: 14),
              child: Row(children: [const Icon(Icons.person, size: 18), const SizedBox(width: 6),
                Text(_waiterName), const Icon(Icons.arrow_drop_down)])),
          ),
        ],
      ),
      floatingActionButton: _locationId.isEmpty ? null : FloatingActionButton.extended(
        onPressed: _newTab, backgroundColor: const Color(0xFF15803D), foregroundColor: Colors.white,
        icon: const Icon(Icons.add), label: const Text('New order')),
      body: RefreshIndicator(
        onRefresh: _load,
        child: _loading
            ? const Center(child: CircularProgressIndicator(color: Color(0xFF15803D)))
            : _err != null
                ? ListView(children: [Padding(padding: const EdgeInsets.all(32), child: Text(_err!, textAlign: TextAlign.center))])
                : _open.isEmpty
                    ? ListView(children: const [Padding(padding: EdgeInsets.all(48), child: Text('No open orders yet. Tap “New order” to start one.', textAlign: TextAlign.center))])
                    : ListView.separated(
                        padding: const EdgeInsets.all(12),
                        itemCount: _open.length,
                        separatorBuilder: (_, __) => const SizedBox(height: 8),
                        itemBuilder: (_, i) {
                          final o = _open[i] as Map;
                          final n = ((o['itemCount'] ?? 0) as num).round();
                          return Card(
                            margin: EdgeInsets.zero,
                            child: ListTile(
                              title: Row(children: [
                                Expanded(child: Text(_label(o), style: const TextStyle(fontWeight: FontWeight.bold))),
                                _statusChip(o),
                              ]),
                              // Order number disambiguates two bills sharing one table (e.g. after a split).
                              subtitle: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                                const SizedBox(height: 2),
                                Text.rich(TextSpan(children: [
                                  TextSpan(text: '${o['orderNumber'] ?? ''}', style: const TextStyle(fontWeight: FontWeight.w700, color: Color(0xFF475569))),
                                  TextSpan(text: ' · $n item(s)', style: const TextStyle(color: Color(0xFF64748B))),
                                ]), style: const TextStyle(fontSize: 12.5)),
                                const SizedBox(height: 4),
                                Text('LKR ${_money(o['totalAmount'])}', style: const TextStyle(fontWeight: FontWeight.w800, fontSize: 17, color: Color(0xFF15803D))),
                              ]),
                              trailing: PopupMenuButton<String>(
                                icon: const Icon(Icons.more_vert),
                                onSelected: (v) { if (v == 'merge') _merge(o); if (v == 'close') _closeBill(o); },
                                itemBuilder: (_) => const [
                                  PopupMenuItem(value: 'merge', child: Row(children: [Icon(Icons.merge_type, size: 18), SizedBox(width: 10), Text('Merge into…')])),
                                  PopupMenuItem(value: 'close', child: Row(children: [Icon(Icons.point_of_sale, size: 18), SizedBox(width: 10), Text('Close bill')])),
                                ],
                              ),
                              onTap: () => _openOrder(o['id'] as String),
                            ),
                          );
                        },
                      ),
      ),
    );
  }
}

class _NewTabSheet extends StatelessWidget {
  final List<dynamic> tables;
  const _NewTabSheet({required this.tables});
  @override
  Widget build(BuildContext context) {
    return SafeArea(
      child: ListView(shrinkWrap: true, children: [
        const Padding(padding: EdgeInsets.all(16), child: Text('Start an order', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 16))),
        ListTile(leading: const Icon(Icons.takeout_dining), title: const Text('Takeaway (no table)'),
            onTap: () => Navigator.pop(context, <String, String?>{'tableId': null, 'label': null})),
        const Divider(height: 1),
        ...tables.map((t) => ListTile(
          leading: const Icon(Icons.table_restaurant),
          title: Text('Table ${t['code']}'),
          onTap: () => Navigator.pop(context, <String, String?>{'tableId': t['id'] as String?, 'label': t['code'] as String?}),
        )),
      ]),
    );
  }
}
