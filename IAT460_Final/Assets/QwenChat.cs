using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq; // 確保 Unity 安裝了 Newtonsoft.Json 套件

public class QwenChat : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject qwenPanel;
    
    [Header("API 設定")]
    public string apiURL = "https://api.your-qwen-endpoint.com"; // 替換為你的 Qwen2.5 API URL
    public string apiKey = "your-api-key"; // 替換為你的 API 金鑰

    [Header("UI 元件")]
    public TMP_InputField inputField; // 用戶輸入框
    public Button sendButton; // 傳送按鈕
    public TextMeshProUGUI chatLog; // 聊天紀錄顯示框

    private List<Dictionary<string, string>> _messages = new List<Dictionary<string, string>>(); // 儲存對話歷史
    private Coroutine thinkingCoroutine;
    private TrumpQuotesDatabase quotesDatabase;
    private ElevenLabsTTS tts;

    private void Start()
    {
        startPanel.SetActive(true);
        qwenPanel.SetActive(false);
        
        quotesDatabase = FindAnyObjectByType<TrumpQuotesDatabase>();
        tts = FindAnyObjectByType<ElevenLabsTTS>();
        if (quotesDatabase == null)
        {
            Debug.LogError("Error: TrumpQuotesDatabase not found in the scene!");
        }
        else
        {
            Debug.Log("✅ TrumpQuotesDatabase 成功找到！");
        }

        // 確保 UI 事件綁定正確
        sendButton.onClick.AddListener(SendMessageToAI);
        inputField.onSubmit.AddListener(delegate { SendMessageToAI(); });
    }

    public void OnStartButtonClicked()
    {
        startPanel.SetActive(false);
        qwenPanel.SetActive(true);

        // AI 或其他初始化也可寫在這
        chatLog.text = "Welcome! Ask me anything.";
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
       
        if (isTemporary)
        {
            // chatLog.text = role + ": " + content;

            if (thinkingCoroutine != null)
            {
                StopCoroutine(thinkingCoroutine);
            }
            thinkingCoroutine = StartCoroutine(ClearThinkingMessage(3f));
        }
        else
        {
            if (thinkingCoroutine != null)
            {
                StopCoroutine(thinkingCoroutine);
                thinkingCoroutine = null;
            }

            chatLog.text = role + ": " + content;

            // ✅ 這裡加入 Trump 語音播放
            if (role == "Trump" && tts != null)
            {
                // tts.Speak(content); // 🔈 播放語音！
                StartCoroutine(PlayWithUI(content));
            }
        }
    }

    private IEnumerator SendRequest(string prompt, int maxTokens)
    {
        // 1️⃣ 先檢索特朗普語錄
        string retrievedInfo = GetComponent<TrumpQuotesDatabase>().GetRelevantQuote(prompt);
    
        // 2️⃣ **保留原始 Prompt，只在有語錄時增加背景知識**
        string finalPrompt;
    
        if (!string.IsNullOrEmpty(retrievedInfo))
        {
            finalPrompt = 
                $"Here is a quote from Donald Trump: \"{retrievedInfo}\".\n\n" + 
                $"Now, based on his speaking style, answer the following: {prompt}";
        }
        
        else if (string.IsNullOrEmpty(retrievedInfo))
        {
            finalPrompt = "I'm sorry, I cannot answer that question without factual information.";
        }
        else
        {
            finalPrompt = prompt; // 直接使用用戶輸入的 prompt
        }
        
        string json =  "{ " +
                       "\"model\": \"Qwen/Qwen2.5-Coder-32B-Instruct\", " +
                       "\"messages\": [" +
                       "{ \"role\": \"system\", \"content\": \"You are an AI version of Donald Trump. Reply in how he speak style and a little bit unpredictable. Close with a memorable phrase like Donald Trump should have,and no need to be long. Make it entertaining, confident, and absolutely full of personality.\" }, " +
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

            StartCoroutine(PlayWithUI(cleanResponse));
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
        
            // 檢查 JSON 結構，確保 `choices` 存在
            if (parsedJson["choices"] != null && parsedJson["choices"].HasValues)
            {
                return parsedJson["choices"][0]["message"]["content"].ToString();
            }
            else
            {
                Debug.LogError("❌ API 回應 JSON 格式錯誤：" + jsonResponse);
                return "Error: AI response format is invalid.";
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("❌ 解析 JSON 失敗：" + ex.Message);
            return "Error: Unable to parse AI response.";
        }
    }
    
    private IEnumerator ClearThinkingMessage(float delay)
    {
        yield return new WaitForSeconds(delay);
    
        // 確保清除的訊息仍然是 "Thinking..."，避免覆蓋真正的 AI 回應
        if (chatLog.text.Contains("")) //Thinking....
        {
            chatLog.text = "";
        }
    }
    private IEnumerator PlayWithUI(string text)
    {
        TrumpUIDialogue trumpUI = FindAnyObjectByType<TrumpUIDialogue>();
        if (trumpUI == null)
        {
            Debug.LogError("❌ 找不到 TrumpUIDialogue.cs");
            yield break;
        }

        // 用 ElevenLabs 播語音（取得 AudioClip）
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{tts.voiceId}/stream";

        var payload = new ElevenLabsTTS.TTSRequest
        {
            text = text,
            model_id = "eleven_monolingual_v1",
            voice_settings = new ElevenLabsTTS.VoiceSettings
            {
                stability = 0.7f,
                similarity_boost = 0.85f
            }
        };

        string json = JsonConvert.SerializeObject(payload);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest req = UnityWebRequest.PostWwwForm(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(bodyRaw);
        req.downloadHandler = new DownloadHandlerAudioClip(url, AudioType.MPEG);
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Accept", "audio/mpeg");
        req.SetRequestHeader("xi-api-key", tts.apiKey);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
            trumpUI.PlayTrumpSpeech(text, clip); // ✅ 傳到 UI 系統播放文字 + 聲音
        }
        else
        {
            Debug.LogError("❌ ElevenLabs 語音下載失敗：" + req.error);
        }
    }
    public void OnQuitButtonClicked()
    {
        Debug.Log("Quit button pressed.");
        Application.Quit();

        // 注意：在 Editor 模式不會真正退出，但在打包後有效
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
