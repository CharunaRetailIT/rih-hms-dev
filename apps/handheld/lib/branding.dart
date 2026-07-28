import 'dart:convert';
import 'package:flutter/material.dart';
import 'api.dart';

/// The tenant's own uploaded company logo (#82a). Accepts a data-URL or a hosted/relative
/// URL; falls back to the RIT mark when none is set or it fails to load.
class TenantLogo extends StatelessWidget {
  final String? url;
  final double size;
  const TenantLogo({super.key, this.url, this.size = 28});
  @override
  Widget build(BuildContext context) {
    final u = (url ?? '').trim();
    if (u.isEmpty) return RitLogo(size: size);
    Widget fallback(_, __, ___) => RitLogo(size: size);
    if (u.startsWith('data:')) {
      try {
        final bytes = base64Decode(u.substring(u.indexOf(',') + 1));
        return Image.memory(bytes, width: size, height: size, fit: BoxFit.contain, errorBuilder: fallback);
      } catch (_) { return RitLogo(size: size); }
    }
    final full = u.startsWith('http') ? u : '${Api.baseUrl}$u';
    return Image.network(full, width: size, height: size, fit: BoxFit.contain, errorBuilder: fallback);
  }
}

/// The RIT brand mark (green rounded square + yellow dot + white "R") — square uses.
class RitLogo extends StatelessWidget {
  final double size;
  const RitLogo({super.key, this.size = 72});
  @override
  Widget build(BuildContext context) =>
      Image.asset('assets/rit_logo.png', width: size, height: size, fit: BoxFit.contain);
}

/// The full RETAIL IT wordmark (green RETAIL + yellow IT + swoosh + tagline).
class RitWordmark extends StatelessWidget {
  final double width;
  const RitWordmark({super.key, this.width = 240});
  @override
  Widget build(BuildContext context) =>
      Image.asset('assets/rit_wordmark.png', width: width, fit: BoxFit.contain);
}

/// In-app splash shown while the app decides pair → login → home. Matches the native
/// launch screen (white bg + RIT mark) so there's no jarring hand-off.
class RitSplash extends StatelessWidget {
  const RitSplash({super.key});
  @override
  Widget build(BuildContext context) => Scaffold(
        backgroundColor: Colors.white,
        bottomNavigationBar: const PoweredByFooter(),
        body: Center(
          child: Column(mainAxisSize: MainAxisSize.min, children: [
            const RitWordmark(width: 280),
            const SizedBox(height: 10),
            Text('Waiter Handheld', style: TextStyle(fontSize: 13, color: Colors.grey[600], letterSpacing: 0.4, fontWeight: FontWeight.w600)),
            const SizedBox(height: 32),
            const SizedBox(width: 22, height: 22, child: CircularProgressIndicator(strokeWidth: 2.5, color: Color(0xFF15803D))),
          ]),
        ),
      );
}

/// App-wide footer: copyright + "powered by". Shown as the Scaffold bottomNavigationBar
/// on the main screens so it sits pinned at the bottom without overlapping content.
class PoweredByFooter extends StatelessWidget {
  final bool onDark;
  const PoweredByFooter({super.key, this.onDark = false});

  @override
  Widget build(BuildContext context) {
    final muted = onDark ? Colors.white70 : const Color(0xFF94A3B8);
    final strong = onDark ? Colors.white : const Color(0xFF64748B);
    return SafeArea(
      top: false,
      child: Padding(
        padding: const EdgeInsets.fromLTRB(16, 6, 16, 10),
        child: Column(mainAxisSize: MainAxisSize.min, children: [
          Text('Powered by Retail Information Technologies',
              textAlign: TextAlign.center,
              style: TextStyle(fontSize: 11.5, fontWeight: FontWeight.w700, color: strong)),
          const SizedBox(height: 1),
          Text('© 2026 Retail Information Technologies (Pvt) Ltd · All rights reserved',
              textAlign: TextAlign.center, style: TextStyle(fontSize: 10, color: muted)),
        ]),
      ),
    );
  }
}
