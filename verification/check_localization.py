"""Validate shipped translations and C# key references without game dependencies."""
import csv
import re
import string
from pathlib import Path

root = Path(__file__).resolve().parents[1]
tables = {}
for path in sorted((root / "assets/Localization").glob("*.csv")):
    table = {}
    for row in csv.reader(path.read_text(encoding="utf-8").splitlines()):
        if not row or row[0].startswith("#"):
            continue
        assert len(row) == 2, (path.name, row)
        key, value = row
        assert key not in table and value.strip(), (path.name, key)
        table[key] = value
    tables[path.stem] = table
english = tables["en"]
formatter = string.Formatter()
for locale, table in tables.items():
    assert table.keys() == english.keys(), (locale, table.keys() ^ english.keys())
    for key, value in table.items():
        fields = {field for _, field, _, _ in formatter.parse(value) if field is not None}
        expected = {field for _, field, _, _ in formatter.parse(english[key]) if field is not None}
        assert fields == expected, (locale, key, fields, expected)
        assert all(field.isdigit() for field in fields), (locale, key)
        value.format("12.5", "100", "30.000")
source = "\n".join(path.read_text() for path in (root / "src").glob("*.cs"))
keys = set(re.findall(r'(?:L10n\.Text|Status|Button|Panel)\("([a-z_]+)"', source))
assert keys <= english.keys(), keys - english.keys()
assert not re.search(r'[\u4e00-\u9fff]', source), "Hardcoded Chinese remains in C#"
print(f"Localization passed: {len(tables)} locales, {len(english)} matching keys, {len(keys)} source references; placeholders valid.")
