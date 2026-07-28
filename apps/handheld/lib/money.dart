/// Format a number as a grouped amount with 2 decimals, e.g. 12804.9 → "12,804.90".
/// Dependency-free (no intl) thousands separators for prices/totals across the app.
String money(dynamic v) {
  final n = ((v ?? 0) as num).toDouble();
  final neg = n < 0;
  final s = n.abs().toStringAsFixed(2);
  final dot = s.indexOf('.');
  final intPart = s.substring(0, dot);
  final dec = s.substring(dot + 1);
  final b = StringBuffer();
  for (var i = 0; i < intPart.length; i++) {
    if (i > 0 && (intPart.length - i) % 3 == 0) b.write(',');
    b.write(intPart[i]);
  }
  return '${neg ? '-' : ''}$b.$dec';
}
