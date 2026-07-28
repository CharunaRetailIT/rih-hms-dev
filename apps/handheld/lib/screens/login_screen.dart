import 'package:flutter/material.dart';
import '../api.dart';
import '../branding.dart';
import '../store.dart';

/// Waiter sign-in on a paired device — PIN (fast) or magic link. Identifies who's serving;
/// their name rides on every order to POS + KOT.
class LoginScreen extends StatefulWidget {
  final Map<String, dynamic> pairing;
  final Future<void> Function() onSignedIn;
  const LoginScreen({super.key, required this.pairing, required this.onSignedIn});
  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> with SingleTickerProviderStateMixin {
  late final TabController _tab = TabController(length: 2, vsync: this);
  // Optional test-only pre-fill (e.g. --dart-define=LOGIN_USER=cashier). Empty in production.
  final _username = TextEditingController(text: const String.fromEnvironment('LOGIN_USER'));
  final _pin = TextEditingController(text: const String.fromEnvironment('LOGIN_PIN'));
  final _email = TextEditingController();
  final _code = TextEditingController();
  bool _busy = false;
  bool _linkSent = false;
  String? _err;

  String get _slug => widget.pairing['slug'] as String;

  Future<void> _save(Map<String, dynamic> r) async {
    final user = (r['user'] as Map?) ?? {};
    await Store.saveWaiter({
      'accessToken': r['accessToken'],
      'name': user['displayName'] ?? 'Waiter',
      'role': user['role'] ?? 2,
      'homeLocationId': user['homeLocationId'],
      'isServer': user['isServer'] ?? false,
    });
    Api.token = r['accessToken'] as String?;
    await widget.onSignedIn();
  }

  Future<void> _run(Future<void> Function() f) async {
    setState(() { _busy = true; _err = null; });
    try { await f(); }
    on ApiException catch (e) { setState(() => _err = e.message); }
    catch (_) { setState(() => _err = 'Could not reach the server.'); }
    finally { if (mounted) setState(() => _busy = false); }
  }

  Future<void> _pinLogin() => _run(() async => _save(await Api.pinLogin(_slug, _username.text, _pin.text)));
  Future<void> _sendLink() => _run(() async { await Api.requestMagicLink(_slug, _email.text); setState(() => _linkSent = true); });
  Future<void> _exchange() => _run(() async => _save(await Api.exchangeMagic(_code.text)));

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      bottomNavigationBar: const PoweredByFooter(),
      appBar: AppBar(
        title: Text(widget.pairing['displayName'] as String? ?? 'Sign in'),
        actions: [
          TextButton(
            onPressed: () async { await Store.clearPairing(); await widget.onSignedIn(); },
            child: const Text('Switch device', style: TextStyle(color: Colors.white70)),
          ),
        ],
        bottom: TabBar(controller: _tab, indicatorColor: Colors.white, labelColor: Colors.white,
          tabs: const [Tab(text: 'Staff PIN'), Tab(text: 'Magic link')]),
      ),
      body: SafeArea(
        child: Center(
          child: ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 440),
            child: TabBarView(controller: _tab, children: [
              // PIN
              ListView(padding: const EdgeInsets.all(24), children: [
                const Text('Sign in to take orders', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                const SizedBox(height: 16),
                TextField(controller: _username, autocorrect: false, decoration: const InputDecoration(labelText: 'Username')),
                const SizedBox(height: 12),
                TextField(controller: _pin, obscureText: true, keyboardType: TextInputType.number,
                    decoration: const InputDecoration(labelText: 'PIN')),
                if (_err != null) Padding(padding: const EdgeInsets.only(top: 12), child: Text(_err!, style: const TextStyle(color: Colors.red))),
                const SizedBox(height: 20),
                FilledButton(onPressed: _busy ? null : _pinLogin, child: const Text('Sign in')),
              ]),
              // Magic link
              ListView(padding: const EdgeInsets.all(24), children: [
                const Text('Email me a sign-in link', style: TextStyle(fontSize: 18, fontWeight: FontWeight.bold)),
                const SizedBox(height: 16),
                TextField(controller: _email, keyboardType: TextInputType.emailAddress, autocorrect: false,
                    decoration: const InputDecoration(labelText: 'Email')),
                const SizedBox(height: 12),
                FilledButton(onPressed: _busy ? null : _sendLink, child: Text(_linkSent ? 'Resend link' : 'Send link')),
                if (_linkSent) ...[
                  const SizedBox(height: 20),
                  Text('Open the email and paste the code from the link (the part after token=).',
                      style: TextStyle(color: Colors.grey[600], fontSize: 13)),
                  const SizedBox(height: 8),
                  TextField(controller: _code, autocorrect: false, decoration: const InputDecoration(labelText: 'Sign-in code')),
                  const SizedBox(height: 12),
                  OutlinedButton(onPressed: _busy ? null : _exchange, child: const Text('Sign in with code')),
                ],
                if (_err != null) Padding(padding: const EdgeInsets.only(top: 12), child: Text(_err!, style: const TextStyle(color: Colors.red))),
              ]),
            ]),
          ),
        ),
      ),
    );
  }
}
