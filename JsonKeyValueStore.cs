using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class JsonKeyValueStore
{
    // •Û‘¶
    public static void SaveKeyValue(string filePath, Dictionary<string, string> data)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");

        int i = 0;
        foreach (var kv in data)
        {
            sb.Append("  \"")
              .Append(Escape(kv.Key))
              .Append("\": \"")
              .Append(Escape(kv.Value))
              .Append("\"");

            if (i < data.Count - 1)
                sb.Append(",");

            sb.AppendLine();
            i++;
        }

        sb.AppendLine("}");
        File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
    }

    // “Ç‚Ýž‚Ý
    public static Dictionary<string, string> LoadKeyValue(string filePath)
    {
        var result = new Dictionary<string, string>();

        if (!File.Exists(filePath))
            return result;

        foreach (var line in File.ReadAllLines(filePath))
        {
            string trimmed = line.Trim();

            if (trimmed.StartsWith("{") || trimmed.StartsWith("}"))
                continue;

            // "key": "value",
            int colon = trimmed.IndexOf(':');
            if (colon < 0) continue;

            string key = trimmed.Substring(0, colon).Trim();
            string value = trimmed.Substring(colon + 1).Trim();

            key = Unquote(key);
            value = Unquote(value.TrimEnd(','));

            result[key] = value;
        }

        return result;
    }

    // --- •â•ŠÖ” ---

    private static string Escape(string s)
    {
        return s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    private static string Unquote(string s)
    {
        if (s.StartsWith("\"") && s.EndsWith("\""))
            s = s.Substring(1, s.Length - 2);

        return s.Replace("\\\"", "\"").Replace("\\\\", "\\");
    }
}
