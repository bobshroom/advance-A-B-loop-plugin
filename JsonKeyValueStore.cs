using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;


public static class JsonKeyValueStore
{
    // キーバリュー辞書を JSON に保存
    public static void SaveKeyValue(string filePath, Dictionary<string, string> data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }

    public static Dictionary<string, string> LoadKeyValue(string filePath)
    {
        if (!File.Exists(filePath))
            return new Dictionary<string, string>();

        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json)
               ?? new Dictionary<string, string>();
    }

}
