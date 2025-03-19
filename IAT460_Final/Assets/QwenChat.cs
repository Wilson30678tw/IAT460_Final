using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq; // 確保 Unity 安裝了 Newtonsoft.Json 套件

public class QwenChat : MonoBehaviour
{
    [Header("API 設定")]
    public string apiURL = "https://api.your-qwen-endpoint.com"; // 替換為你的 Qwen2.5 API URL
    public string apiKey = "your-api-key"; // 替換為你的 API 金鑰

    [Header("UI 元件")]
    public TMP_InputField inputField; // 用戶輸入框
    public Button sendButton; // 傳送按鈕
    public TextMeshProUGUI chatLog; // 聊天紀錄顯示框

    private List<Dictionary<string, string>> _messages = new List<Dictionary<string, string>>(); // 儲存對話歷史
    private Coroutine thinkingCoroutine;

    private void Start()
    {
        // 確保 UI 事件綁定正確
        sendButton.onClick.AddListener(SendMessageToAI);
        inputField.onSubmit.AddListener(delegate { SendMessageToAI(); });
    }

    public void SendMessageToAI()
    {
        string userInput = inputField.text.Trim(); // 去除空白
        if (string.IsNullOrEmpty(userInput)) return;

        AddMessage("User", userInput);
        AddMessage("Trump", "Thinking..."); // 顯示暫時回應

        StartCoroutine(SendRequest(userInput, 500));

        inputField.text = ""; // 清空輸入框
        inputField.ActivateInputField(); // 讓輸入框保持可輸入狀態
    }


    private void AddMessage(string role, string content, bool isTemporary = false)
    {
        if (isTemporary) // 如果是 "Thinking..." 訊息，啟動計時器讓它 3 秒後消失
        {
            chatLog.text = role + ": " + content;

            // 確保舊的 "Thinking..." 訊息計時被取消，避免重疊
            if (thinkingCoroutine != null)
            {
                StopCoroutine(thinkingCoroutine);
            }
            thinkingCoroutine = StartCoroutine(ClearThinkingMessage(3f)); // 3 秒後清除
        }
        else // 正式 AI 回應，應該一直顯示，直到玩家下一次輸入
        {
            if (thinkingCoroutine != null)
            {
                StopCoroutine(thinkingCoroutine); // 確保 "Thinking..." 被清除
                thinkingCoroutine = null;
            }
            chatLog.text = role + ": " + content; // 直接覆蓋，確保 AI 回應長時間顯示
        }
    }

    private IEnumerator SendRequest(string prompt, int maxTokens)
    {
        string json = "{ " +
                      "\"model\": \"Qwen/Qwen2.5-Coder-32B-Instruct\", " +
                      "\"messages\": [" +
                      "{ \"role\": \"system\", \"content\": \"You are an AI version of Donald Trump. Reply in how he speak style and a little bit unpredictable. Close with a memorable phrase, such as “We’re gonna win big, believe me. Make it entertaining, confident, and absolutely full of personality.\" }, " +
                      "{ \"role\": \"user\", \"content\": \"" + prompt + "\" }" +
                      "], " +
                      "\"max_tokens\": " + maxTokens + ", " +
                      "\"temperature\": 0.8, " +
                      "\"top_p\": 0.9 " +
                      "}";

        using UnityWebRequest webRequest = new UnityWebRequest(apiURL, "POST", new DownloadHandlerBuffer(), new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json)));
        webRequest.SetRequestHeader("Content-Type", "application/json");
        webRequest.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.Success)
        {
            string response = webRequest.downloadHandler.text;

            // **解析 JSON 只提取 "content" 部分**
            string cleanResponse = ExtractContent(response);

            AddMessage("Trump", cleanResponse);
        }
        else
        {
            Debug.LogError("Error: " + webRequest.error);
            AddMessage("Trump", "Sorry, something went wrong.");
        }
        
    }
    private string ExtractContent(string jsonResponse)
    {
        try
        {
            JObject parsedJson = JObject.Parse(jsonResponse);
            return parsedJson["choices"][0]["message"]["content"].ToString();
        }
        catch
        {
            return "Error: Unable to parse AI response.";
        }
    }
    
    private IEnumerator ClearThinkingMessage(float delay)
    {
        yield return new WaitForSeconds(delay);
    
        // 確保清除的訊息仍然是 "Thinking..."，避免覆蓋真正的 AI 回應
        if (chatLog.text.Contains("Thinking..."))
        {
            chatLog.text = "";
        }
    }
}
