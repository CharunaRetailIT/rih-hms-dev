import 'package:flutter/material.dart';
import '../api.dart';
import '../money.dart';

const _green = Color(0xFF15803D);
const _border = Color(0xFFCBD5E1);

/// Take an order: pick items off the availability-aware, categorised menu, adjust
/// quantities, split the bill, then Send to Kitchen. 86'd items are greyed and locked.
class OrderScreen extends StatefulWidget {
  final String orderId;
  final String locationId;
  const OrderScreen({super.key, required this.orderId, required this.locationId});
  @override
  State<OrderScreen> createState() => _OrderScreenState();
}

class _OrderScreenState extends State<OrderScreen> {
  Map<String, dynamic>? _order;
  List<dynamic> _products = [];
  List<dynamic> _categories = [];
  Map<String, bool> _avail = {};       // productId → available
  String _cat = 'all';
  String _q = '';
  bool _loading = true;
  bool _sending = false;
  bool _cartOpen = false;              // phone: cart overlay visible

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    setState(() => _loading = true);
    try {
      final r = await Future.wait([
        Api.getOrder(widget.orderId), Api.products(), Api.availability(widget.locationId), Api.categories(),
      ]);
      final am = <String, bool>{};
      for (final a in (r[2] as List)) { am[a['productId'] as String] = a['available'] as bool; }
      if (!mounted) return;
      setState(() {
        _order = r[0] as Map<String, dynamic>;
        _products = r[1] as List<dynamic>;
        _avail = am;
        _categories = r[3] as List<dynamic>;
        _loading = false;
      });
    } catch (e) {
      if (mounted) { setState(() => _loading = false); _snack(e.toString()); }
    }
  }

  // ── cart helpers ──
  List<dynamic> get _items => (_order?['items'] as List?) ?? [];
  num get _total => (_order?['totalAmount'] ?? 0) as num;
  int get _count => _items.fold<int>(0, (s, it) => s + ((it['quantity'] ?? 0) as num).round());
  // Items not yet fired to the kitchen — only these go on the next "Send".
  int get _pendingCount => _items.where((it) => it['kotStatus'] != 'sent').fold<int>(0, (s, it) => s + ((it['quantity'] ?? 0) as num).round());
  Map<String, int> get _qtyByProduct {
    final m = <String, int>{};
    for (final it in _items) {
      final pid = it['productId'] as String?;
      if (pid != null) m[pid] = (m[pid] ?? 0) + ((it['quantity'] ?? 0) as num).round();
    }
    return m;
  }

  Future<void> _add(Map p) async {
    final id = p['id'] as String;
    if (_avail[id] == false) { _snack('${p['name']} is 86’d at this outlet.'); return; }
    try {
      // Merge ONLY into a line that hasn't fired yet — bumping an already-"sent" line
      // would never reach the kitchen (Confirm only fires pending lines). So if the
      // matching line is sent, add a fresh pending line that fires on the next send.
      Map? line;
      for (final it in _items) {
        if (it['productId'] == id && it['variantId'] == null && it['kotStatus'] != 'sent') { line = it as Map; break; }
      }
      final o = line != null
          ? await Api.updateQty(widget.orderId, line['id'] as String, ((line['quantity'] ?? 0) as num).round() + 1)
          : await Api.addItem(widget.orderId, id);
      setState(() => _order = o);
    } on ApiException catch (e) { _snack(e.message); }
  }

  Future<void> _setQty(Map item, int qty) async {
    try { final o = await Api.updateQty(widget.orderId, item['id'] as String, qty); setState(() => _order = o); }
    on ApiException catch (e) { _snack(e.message); }
  }

  Future<void> _send() async {
    if (_items.isEmpty) return;
    setState(() => _sending = true);
    try { await Api.confirm(widget.orderId); if (mounted) { _snack('Sent to kitchen'); Navigator.pop(context); } }
    on ApiException catch (e) { _snack(e.message); }
    finally { if (mounted) setState(() => _sending = false); }
  }

  void _snack(String m) { if (mounted) ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(m))); }
  String _money(dynamic v) => 'LKR ${money(v)}';

  // ── split bill ──
  Future<void> _openSplit() async {
    if (_items.isEmpty) { _snack('Nothing to split yet.'); return; }
    final moves = <String, int>{ for (final it in _items) it['id'] as String : 0 };
    final confirmed = await showModalBottomSheet<bool>(
      context: context, showDragHandle: true, isScrollControlled: true,
      builder: (_) => StatefulBuilder(builder: (ctx, setSheet) {
        final picked = moves.values.fold<int>(0, (s, q) => s + q);
        return SafeArea(child: Padding(padding: const EdgeInsets.all(16),
          child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.stretch, children: [
            const Text('Split the bill', style: TextStyle(fontWeight: FontWeight.bold, fontSize: 18)),
            const SizedBox(height: 2),
            Text('Choose how many of each item move onto a new bill at this table.', style: TextStyle(color: Colors.grey[600], fontSize: 13)),
            const SizedBox(height: 12),
            ConstrainedBox(constraints: const BoxConstraints(maxHeight: 340),
              child: ListView(shrinkWrap: true, children: _items.map((it) {
                final id = it['id'] as String; final max = ((it['quantity'] ?? 0) as num).round(); final v = moves[id]!;
                return Padding(padding: const EdgeInsets.symmetric(vertical: 4), child: Row(children: [
                  Expanded(child: Text(it['productName'] as String? ?? 'Item')),
                  _StepBtn(icon: Icons.remove, onTap: v > 0 ? () => setSheet(() => moves[id] = v - 1) : null),
                  SizedBox(width: 28, child: Text('$v', textAlign: TextAlign.center, style: const TextStyle(fontWeight: FontWeight.bold))),
                  _StepBtn(icon: Icons.add, filled: true, onTap: v < max ? () => setSheet(() => moves[id] = v + 1) : null),
                  SizedBox(width: 36, child: Text('/$max', style: TextStyle(color: Colors.grey[500]))),
                ]));
              }).toList()),
            ),
            const SizedBox(height: 12),
            FilledButton.icon(
              style: FilledButton.styleFrom(backgroundColor: _green),
              onPressed: picked == 0 ? null : () => Navigator.pop(ctx, true),
              icon: const Icon(Icons.call_split), label: Text(picked == 0 ? 'Pick items to move' : 'Move $picked item(s) to a new bill')),
          ])));
      }),
    );
    if (confirmed != true) return;
    final lines = moves.entries.where((e) => e.value > 0).map((e) => {'itemId': e.key, 'quantity': e.value}).toList();
    try {
      final newOrder = await Api.split(widget.orderId, lines);
      await _load();
      if (!mounted) return;
      ScaffoldMessenger.of(context).showSnackBar(SnackBar(
        content: const Text('Split onto a new bill at this table.'),
        action: SnackBarAction(label: 'OPEN', onPressed: () {
          Navigator.push(context, MaterialPageRoute(builder: (_) => OrderScreen(orderId: newOrder['id'] as String, locationId: widget.locationId)));
        }),
      ));
    } on ApiException catch (e) { _snack(e.message); }
  }

  @override
  Widget build(BuildContext context) {
    final label = (_order?['tableLabel'] as String?)?.isNotEmpty == true
        ? 'Table ${_order!['tableLabel']}' : (_order?['orderNumber'] as String? ?? 'Order');
    return Scaffold(
      backgroundColor: const Color(0xFFF1F5F9),
      appBar: AppBar(
        title: Text(label),
        actions: [
          if (_items.isNotEmpty)
            IconButton(tooltip: 'Split bill', icon: const Icon(Icons.call_split), onPressed: _openSplit),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator(color: _green))
          : LayoutBuilder(builder: (_, c) {
              final wide = c.maxWidth >= 720;
              if (wide) {
                return Row(children: [
                  Expanded(child: _menu()),
                  const VerticalDivider(width: 1),
                  SizedBox(width: 340, child: Container(color: Colors.white, child: _cart(sheet: false))),
                ]);
              }
              return Stack(children: [
                Positioned.fill(child: Padding(padding: const EdgeInsets.only(bottom: 64), child: _menu())),
                if (_cartOpen) Positioned.fill(child: GestureDetector(onTap: () => setState(() => _cartOpen = false),
                  child: Container(color: Colors.black38))),
                if (_cartOpen) Positioned(left: 0, right: 0, bottom: 0, top: 70,
                  child: Material(borderRadius: const BorderRadius.vertical(top: Radius.circular(18)), clipBehavior: Clip.antiAlias,
                    child: _cart(sheet: true))),
                if (!_cartOpen) Positioned(left: 0, right: 0, bottom: 0, child: _bottomBar()),
              ]);
            }),
    );
  }

  // ── menu (search + category chips + bordered grid) ──
  Widget _menu() {
    final cats = _categories.where((c) => _products.any((p) => p['categoryId'] == c['id'])).toList();
    final filtered = _products.where((p) =>
        (_cat == 'all' || p['categoryId'] == _cat) &&
        (_q.isEmpty || (p['name'] as String).toLowerCase().contains(_q.toLowerCase()))).toList();
    final byProd = _qtyByProduct;
    return Column(children: [
      Padding(padding: const EdgeInsets.fromLTRB(10, 10, 10, 6),
        child: TextField(onChanged: (v) => setState(() => _q = v),
          decoration: InputDecoration(prefixIcon: const Icon(Icons.search), hintText: 'Search menu…', isDense: true,
            filled: true, fillColor: Colors.white,
            border: OutlineInputBorder(borderRadius: BorderRadius.circular(10), borderSide: const BorderSide(color: _border))))),
      SizedBox(height: 42, child: ListView(scrollDirection: Axis.horizontal, padding: const EdgeInsets.symmetric(horizontal: 8), children: [
        _chip('All', _cat == 'all', () => setState(() => _cat = 'all')),
        ...cats.map((c) => _chip(c['name'] as String, _cat == c['id'], () => setState(() => _cat = c['id'] as String))),
      ])),
      Expanded(child: GridView.builder(
        padding: const EdgeInsets.all(8),
        gridDelegate: const SliverGridDelegateWithMaxCrossAxisExtent(maxCrossAxisExtent: 200, childAspectRatio: 1.35, crossAxisSpacing: 8, mainAxisSpacing: 8),
        itemCount: filtered.length,
        itemBuilder: (_, i) {
          final p = filtered[i] as Map;
          final off = _avail[p['id']] == false;
          final qty = byProd[p['id']] ?? 0;
          return InkWell(
            onTap: () => _add(p), borderRadius: BorderRadius.circular(12),
            child: Container(
              decoration: BoxDecoration(
                color: off ? const Color(0xFFF1F5F9) : Colors.white,
                borderRadius: BorderRadius.circular(12),
                border: Border.all(color: qty > 0 ? _green : _border, width: qty > 0 ? 1.5 : 1),
                boxShadow: off ? null : const [BoxShadow(color: Color(0x0F000000), blurRadius: 3, offset: Offset(0, 1))]),
              padding: const EdgeInsets.all(10),
              child: Column(crossAxisAlignment: CrossAxisAlignment.start, mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
                Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Expanded(child: Text(p['name'] as String, maxLines: 2, overflow: TextOverflow.ellipsis,
                    style: TextStyle(fontWeight: FontWeight.bold, height: 1.15,
                      decoration: off ? TextDecoration.lineThrough : null, color: off ? Colors.grey : const Color(0xFF0F172A)))),
                  if (off) const _Badge('86', Colors.red)
                  else if (qty > 0) _Badge('$qty', _green),
                ]),
                Text(_money(p['basePrice']), style: TextStyle(fontWeight: FontWeight.w800, color: off ? Colors.grey : _green)),
              ]),
            ),
          );
        },
      )),
    ]);
  }

  Widget _chip(String label, bool active, VoidCallback onTap) => Padding(
    padding: const EdgeInsets.only(right: 8),
    child: ChoiceChip(label: Text(label), selected: active, onSelected: (_) => onTap(),
      selectedColor: _green, labelStyle: TextStyle(color: active ? Colors.white : const Color(0xFF334155), fontWeight: FontWeight.w600),
      backgroundColor: Colors.white, shape: StadiumBorder(side: BorderSide(color: active ? _green : _border))));

  // ── cart panel (qty steppers + remove + split + send) ──
  Widget _cart({required bool sheet}) {
    return Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
      Container(color: Colors.white, padding: const EdgeInsets.fromLTRB(16, 14, 8, 10),
        child: Row(children: [
          Expanded(child: Text('Order · $_count item(s)', style: const TextStyle(fontWeight: FontWeight.bold, fontSize: 16))),
          if (sheet) IconButton(icon: const Icon(Icons.close), onPressed: () => setState(() => _cartOpen = false)),
        ])),
      const Divider(height: 1),
      Expanded(child: _items.isEmpty
        ? const Center(child: Padding(padding: EdgeInsets.all(24), child: Text('Tap menu items to add them.', textAlign: TextAlign.center)))
        : ListView.separated(
            padding: const EdgeInsets.symmetric(vertical: 4),
            itemCount: _items.length, separatorBuilder: (_, __) => const Divider(height: 1),
            itemBuilder: (_, i) {
              final it = _items[i] as Map; final qty = ((it['quantity'] ?? 0) as num).round();
              final sent = it['kotStatus'] == 'sent';
              return Padding(padding: const EdgeInsets.fromLTRB(12, 6, 12, 6), child: Row(children: [
                // Sent lines are locked (already fired) — show qty only, no steppers.
                if (sent)
                  SizedBox(width: 94, child: Text('$qty×', style: const TextStyle(fontWeight: FontWeight.bold, color: Color(0xFF64748B))))
                else ...[
                  _StepBtn(icon: qty <= 1 ? Icons.delete_outline : Icons.remove, onTap: () => _setQty(it, qty - 1)),
                  SizedBox(width: 30, child: Text('$qty', textAlign: TextAlign.center, style: const TextStyle(fontWeight: FontWeight.bold))),
                  _StepBtn(icon: Icons.add, filled: true, onTap: () => _setQty(it, qty + 1)),
                ],
                const SizedBox(width: 10),
                Expanded(child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
                  Row(children: [
                    Flexible(child: Text(it['productName'] as String? ?? 'Item',
                      style: TextStyle(fontWeight: FontWeight.w600, color: sent ? const Color(0xFF475569) : Colors.black87))),
                    if (sent) Container(margin: const EdgeInsets.only(left: 6), padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 1),
                      decoration: BoxDecoration(color: const Color(0xFFE7F3EC), borderRadius: BorderRadius.circular(5)),
                      child: const Text('✓ sent', style: TextStyle(fontSize: 10, fontWeight: FontWeight.w700, color: Color(0xFF15803D)))),
                  ]),
                  if ((it['variantName'] as String?)?.isNotEmpty == true) Text(it['variantName'] as String, style: TextStyle(fontSize: 12, color: Colors.grey[600])),
                ])),
                Text(_money(it['lineTotal'] ?? it['lineSubtotal']), style: const TextStyle(fontWeight: FontWeight.w700)),
              ]));
            })),
      const Divider(height: 1),
      Container(color: Colors.white, child: SafeArea(top: false, child: Column(mainAxisSize: MainAxisSize.min, children: [
        Padding(padding: const EdgeInsets.fromLTRB(16, 12, 16, 6),
          child: Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
            const Text('Total', style: TextStyle(fontWeight: FontWeight.bold)),
            Text(_money(_total), style: const TextStyle(fontWeight: FontWeight.w900, fontSize: 20, color: _green)),
          ])),
        Padding(padding: const EdgeInsets.fromLTRB(12, 0, 12, 12), child: Row(children: [
          if (_items.isNotEmpty) ...[
            OutlinedButton.icon(onPressed: _openSplit, icon: const Icon(Icons.call_split, size: 18), label: const Text('Split'),
              style: OutlinedButton.styleFrom(foregroundColor: _green, side: const BorderSide(color: _green), padding: const EdgeInsets.symmetric(horizontal: 14, vertical: 14))),
            const SizedBox(width: 8),
          ],
          Expanded(child: FilledButton.icon(
            style: FilledButton.styleFrom(backgroundColor: _green, padding: const EdgeInsets.symmetric(vertical: 14)),
            onPressed: (_pendingCount == 0 || _sending) ? null : _send,
            icon: const Icon(Icons.send),
            label: Text(_sending ? 'Sending…' : _pendingCount == 0 ? 'All sent ✓' : 'Send $_pendingCount to Kitchen'))),
        ])),
      ]))),
    ]);
  }

  // ── phone bottom bar ──
  Widget _bottomBar() => Material(
    color: _items.isEmpty ? Colors.white : _green, elevation: 8,
    child: InkWell(
      onTap: _items.isEmpty ? null : () => setState(() => _cartOpen = true),
      child: Padding(padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 16),
        child: _items.isEmpty
          ? const Text('Tap items to start the order', textAlign: TextAlign.center, style: TextStyle(color: Color(0xFF64748B)))
          : Row(mainAxisAlignment: MainAxisAlignment.spaceBetween, children: [
              Row(children: [
                Container(padding: const EdgeInsets.all(5), decoration: const BoxDecoration(color: Colors.white24, shape: BoxShape.circle),
                  child: Text('$_count', style: const TextStyle(color: Colors.white, fontWeight: FontWeight.bold, fontSize: 12))),
                const SizedBox(width: 10),
                const Text('Review & send', style: TextStyle(color: Colors.white, fontWeight: FontWeight.bold)),
              ]),
              Text(_money(_total), style: const TextStyle(color: Colors.white, fontWeight: FontWeight.w900, fontSize: 16)),
            ])),
    ),
  );
}

class _StepBtn extends StatelessWidget {
  final IconData icon; final VoidCallback? onTap; final bool filled;
  const _StepBtn({required this.icon, this.onTap, this.filled = false});
  @override
  Widget build(BuildContext context) => InkResponse(onTap: onTap, radius: 22,
    child: Container(height: 32, width: 32, alignment: Alignment.center,
      decoration: BoxDecoration(shape: BoxShape.circle,
        color: filled ? (onTap == null ? const Color(0xFFCBD5E1) : _green) : Colors.transparent,
        border: filled ? null : Border.all(color: onTap == null ? const Color(0xFFE2E8F0) : _border)),
      child: Icon(icon, size: 18, color: filled ? Colors.white : (onTap == null ? const Color(0xFFCBD5E1) : const Color(0xFF475569)))));
}

class _Badge extends StatelessWidget {
  final String text; final Color color;
  const _Badge(this.text, this.color);
  @override
  Widget build(BuildContext context) => Container(
    padding: const EdgeInsets.symmetric(horizontal: 6, vertical: 2),
    decoration: BoxDecoration(color: color, borderRadius: BorderRadius.circular(6)),
    child: Text(text, style: const TextStyle(color: Colors.white, fontSize: 11, fontWeight: FontWeight.w900)));
}
