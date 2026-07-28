import 'package:flutter/material.dart';
import '../api.dart';
import '../branding.dart';

/// Self-service floor coverage (#floor-push): the waiter picks which floor(s) of their
/// outlet they're covering right now, so guest-order pushes reach them for the right
/// tables. Same underlying data as the web Team screen's per-user picker, but this one
/// operates on "me" — any signed-in steward can set their own, no manager needed.
class FloorsScreen extends StatefulWidget {
  final String locationId;
  const FloorsScreen({super.key, required this.locationId});
  @override
  State<FloorsScreen> createState() => _FloorsScreenState();
}

class _FloorsScreenState extends State<FloorsScreen> {
  bool _loading = true;
  String? _err;
  List<Map<String, dynamic>> _floors = [];
  Set<String> _selected = {};
  bool _saving = false;

  @override
  void initState() { super.initState(); _load(); }

  Future<void> _load() async {
    setState(() { _loading = true; _err = null; });
    try {
      final opts = await Api.floors(widget.locationId);
      final mine = await Api.myFloors();
      setState(() {
        _floors = opts.cast<Map<String, dynamic>>();
        _selected = mine.cast<String>().toSet();
        _loading = false;
      });
    } catch (_) {
      setState(() { _err = 'Could not load floors.'; _loading = false; });
    }
  }

  Future<void> _save() async {
    setState(() => _saving = true);
    try {
      await Api.setMyFloors(_selected.toList());
      if (mounted) Navigator.of(context).pop();
    } catch (_) {
      if (mounted) setState(() => _err = 'Could not save your floor coverage.');
    } finally {
      if (mounted) setState(() => _saving = false);
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('My Floor Coverage')),
      bottomNavigationBar: const PoweredByFooter(),
      body: _loading
          ? const Center(child: CircularProgressIndicator())
          : _err != null
              ? Center(child: Padding(padding: const EdgeInsets.all(24), child: Text(_err!, style: const TextStyle(color: Colors.red))))
              : Padding(
                  padding: const EdgeInsets.all(16),
                  child: Column(crossAxisAlignment: CrossAxisAlignment.stretch, children: [
                    const Text('Which floors are you covering? Leave all unchecked to be notified for every floor.'),
                    const SizedBox(height: 16),
                    Expanded(
                      child: _floors.isEmpty
                          ? const Center(child: Text('No floors set up for your outlet yet — ask your manager to add some.'))
                          : ListView(children: _floors.map((f) {
                              final id = f['id'] as String;
                              final name = f['name'] as String? ?? '';
                              return CheckboxListTile(
                                title: Text(name),
                                value: _selected.contains(id),
                                onChanged: (v) => setState(() { if (v == true) { _selected.add(id); } else { _selected.remove(id); } }),
                              );
                            }).toList()),
                    ),
                    FilledButton(onPressed: _saving ? null : _save,
                        child: _saving ? const SizedBox(height: 22, width: 22, child: CircularProgressIndicator(strokeWidth: 2, color: Colors.white)) : const Text('Save')),
                  ]),
                ),
    );
  }
}
