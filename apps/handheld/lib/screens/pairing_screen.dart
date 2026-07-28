import 'package:flutter/material.dart';
import '../api.dart';
import '../branding.dart';
import '../store.dart';

/// One-time device activation (licensing). The manager registers this device in the
/// back-office and gives you the workspace + one-time token.
class PairingScreen extends StatefulWidget {
  final Future<void> Function() onPaired;
  const PairingScreen({super.key, required this.onPaired});
  @override
  State<PairingScreen> createState() => _PairingScreenState();
}

class _PairingScreenState extends State<PairingScreen> {
  // Optional test-only pre-fill (e.g. --dart-define=PAIR_SLUG=demo). Empty in production.
  final _slug = TextEditingController(text: const String.fromEnvironment('PAIR_SLUG'));
  final _token = TextEditingController(text: const String.fromEnvironment('PAIR_TOKEN'));
  bool _busy = false;
  String? _err;

  Future<void> _pair() async {
    setState(() { _busy = true; _err = null; });
    try {
      final r = await Api.pair(_slug.text, _token.text);
      final tenant = (r['tenant'] as Map?) ?? {};
      await Store.savePairing({
        'slug': tenant['slug'] ?? _slug.text.trim().toLowerCase(),
        'displayName': tenant['displayName'] ?? _slug.text.trim(),
        'locationId': r['locationId'],
        'deviceName': r['deviceName'] ?? 'Handheld',
        'logoUrl': tenant['logoUrl'],
        'deviceToken': _token.text.trim(),
      });
      await widget.onPaired();
    } on ApiException catch (e) {
      setState(() => _err = e.message);
    } catch (_) {
      setState(() => _err = 'Could not reach the server. Check your connection.');
    } finally {
      if (mounted) setState(() => _busy = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      bottomNavigationBar: const PoweredByFooter(),
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: ConstrainedBox(
              constraints: const BoxConstraints(maxWidth: 420),
              child: Column(mainAxisSize: MainAxisSize.min, crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                const Center(child: RitWordmark(width: 230)),
                const SizedBox(height: 22),
                const Text('Activate this handheld', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                const SizedBox(height: 4),
                Text('Enter the workspace and the one-time device code from your manager.',
                    style: TextStyle(color: Colors.grey[600])),
                const SizedBox(height: 24),
                TextField(controller: _slug, autocorrect: false,
                    decoration: const InputDecoration(labelText: 'Workspace', hintText: 'e.g. demo')),
                const SizedBox(height: 12),
                TextField(controller: _token, autocorrect: false,
                    decoration: const InputDecoration(labelText: 'Device code')),
                if (_err != null) Padding(padding: const EdgeInsets.only(top: 12),
                    child: Text(_err!, style: const TextStyle(color: Colors.red))),
                const SizedBox(height: 20),
                FilledButton(onPressed: _busy ? null : _pair,
                    child: _busy ? const SizedBox(height: 22, width: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white)) : const Text('Activate device')),
              ]),
            ),
          ),
        ),
      ),
    );
  }
}
