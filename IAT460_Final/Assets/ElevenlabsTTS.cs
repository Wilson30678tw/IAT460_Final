using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;

[RequireComponent(typeof(AudioSource))]
public class ElevenLabsTTS : MonoBehaviour
{
    [Header("ElevenLabs Settings")]
    public string apiKey = "你的_API_KEY";
    public string voiceId = "TxGEqnHWrfWFTf9P7U3b"; // 預設 Josh ID，可更換自定義聲音

    public void Speak(string text)
    {
        StartCoroutine(GenerateSpeech(text));
    }

    private IEnumerator GenerateSpeech(string text)
    {
        string url = $"https://api.elevenlabs.io/v1/text-to-speech/{voiceId}/stream";

        var payload = new TTSRequest
        {
            text = text,
            model_id = "eleven_monolingual_v1",
            voice_settings = new VoiceSettings
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
        req.SetRequestHeader("xi-api-key", apiKey);

        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
            GetComponent<AudioSource>().clip = clip;
            GetComponent<AudioSource>().Play();
            Debug.Log("✅ 播放成功");
        }
        else
        {
            Debug.LogError("❌ ElevenLabs 錯誤：" + req.responseCode + " - " + req.error);
            // 使用 Buffer 顯示錯誤 JSON（不能用 GetText）
            byte[] errorData = req.downloadHandler.data;
            if (errorData != null)
            {
                string errorMessage = Encoding.UTF8.GetString(errorData);
                Debug.LogError("📦 回傳錯誤內容：" + errorMessage);
            }
        }
    }

    [System.Serializable]
    public class TTSRequest
    {
        public string text;
        public string model_id;
        public VoiceSettings voice_settings;
    }

    [System.Serializable]
    public class VoiceSettings
    {
        public float stability;
        public float similarity_boost;
    }
}
