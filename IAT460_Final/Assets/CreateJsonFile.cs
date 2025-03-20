using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CreateJsonFile : MonoBehaviour
{
    void Start()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "trump_quotes.json");

        // 如果文件已經存在，就不再創建
        if (File.Exists(path))
        {
            Debug.Log("trump_quotes.json 已存在！");
            return;
        }

        // 創建特朗普語錄列表
        List<Dictionary<string, string>> quotes = new List<Dictionary<string, string>>()
        {
            new Dictionary<string, string>() { { "topic", "economy" }, { "quote", "The economy is the strongest it's ever been. Believe me, folks!" } },
            new Dictionary<string, string>() { { "topic", "taxes" }, { "quote", "We have the biggest tax cuts in history. Huge, just huge!" } },
            new Dictionary<string, string>() { { "topic", "media" }, { "quote", "Fake news is the enemy of the people. You know it, I know it!" } }
        };

        // 將數據轉換為 JSON 格式
        string jsonContent = JsonHelper.ToJson(quotes, true);

        // 寫入 JSON 文件
        File.WriteAllText(path, jsonContent);

        Debug.Log("trump_quotes.json 創建成功！");
    }
}

// 幫助類：處理 JSON 轉換
public static class JsonHelper
{
    public static string ToJson<T>(List<T> list, bool prettyPrint)
    {
        return JsonUtility.ToJson(new Wrapper<T> { Items = list }, prettyPrint);
    }

    [System.Serializable]
    private class Wrapper<T>
    {
        public List<T> Items;
    }
}