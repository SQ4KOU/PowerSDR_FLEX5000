from pathlib import Path

path = Path('Project Files/Source/Console/database.cs')
text = path.read_text(encoding='utf-8-sig')

old = '''        private static void VerifyTXProfileColumns()
        {
            foreach (string tableName in new[] { "TXProfile", "TXProfileDef" })
            {
                if (!ds.Tables.Contains(tableName)) continue;
                DataTable t = ds.Tables[tableName];
                if (!t.Columns.Contains("CFCPhaseRotatorAuto"))
                    t.Columns.Add("CFCPhaseRotatorAuto", typeof(bool));
            }
        }
'''

new = '''        private static void VerifyTXProfileColumns()
        {
            // EU2AV introduced CFCPhaseRotatorAuto after many existing databases were created.
            // Adding a bool DataColumn to a populated DataTable leaves existing rows as DBNull.
            // IsDatabaseCompatible() correctly rejects DBNull values, but that would force a full
            // database migration for a setting whose defined legacy/default value is simply false.
            // Normalize this one additive field in-place; all other compatibility checks stay strict.
            foreach (string tableName in new[] { "TXProfile", "TXProfileDef" })
            {
                if (!ds.Tables.Contains(tableName)) continue;
                DataTable t = ds.Tables[tableName];
                DataColumn c;
                if (!t.Columns.Contains("CFCPhaseRotatorAuto"))
                {
                    c = t.Columns.Add("CFCPhaseRotatorAuto", typeof(bool));
                    c.DefaultValue = false;
                }
                else
                {
                    c = t.Columns["CFCPhaseRotatorAuto"];
                    if (c.DataType == typeof(bool)) c.DefaultValue = false;
                }

                if (c.DataType == typeof(bool))
                {
                    foreach (DataRow row in t.Rows)
                    {
                        if (row.RowState == DataRowState.Deleted) continue;
                        if (row.IsNull(c)) row[c] = false;
                    }
                }
            }
        }
'''

if old not in text:
    if new in text:
        print('DB compatibility fix already present.')
        raise SystemExit(0)
    raise SystemExit('VerifyTXProfileColumns anchor not found.')

path.write_text(text.replace(old, new, 1), encoding='utf-8-sig')
print('Applied EU2AV CFCPhaseRotatorAuto legacy DB compatibility fix.')
