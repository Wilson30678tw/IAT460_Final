using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

public class TrumpQuotesDatabase : MonoBehaviour
{
    private List<Dictionary<string, string>> quotesList;

    void Start()
    {
        LoadTrumpQuotes();
    }

    private void LoadTrumpQuotes()
    {
        string path = Path.Combine(Application.streamingAssetsPath, "trump_quotes.json");

        if (File.Exists(path))
        {
            string jsonContent = File.ReadAllText(path);
            Debug.Log("📂 JSON 文件內容: " + jsonContent);

            JArray quotesArray = JArray.Parse(jsonContent);
            quotesList = new List<Dictionary<string, string>>();

            foreach (JObject quote in quotesArray)
            {
                quotesList.Add(new Dictionary<string, string>
                {
                    { "topic", quote["topic"].ToString() },
                    { "quote", quote["quote"].ToString() }
                });
            }

            Debug.Log("✅ Trump Quotes Loaded Successfully! 共有 " + quotesList.Count + " 條語錄。");
        }
        else
        {
            Debug.LogError("❌ JSON 文件不存在，請確保 `trump_quotes.json` 放在 `StreamingAssets` 資料夾內！");
        }
    }

    public string GetRelevantQuote(string userInput)
    {
        if (quotesList == null || quotesList.Count == 0)
        {
            Debug.LogError("❌ quotesList 是空的，確保 `trump_quotes.json` 已正確載入！");
            return "";
        }

        foreach (var quote in quotesList)
        {
            if (userInput.ToLower().Contains(quote["topic"].ToLower()))
            {
                Debug.Log("🔍 找到相關語錄: " + quote["quote"]);
                return quote["quote"];
            }
        }

        Debug.Log("❌ 沒有找到匹配的語錄！");
        return "";
    }
}