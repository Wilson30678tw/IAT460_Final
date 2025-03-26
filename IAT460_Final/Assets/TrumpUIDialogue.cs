using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrumpUIDialogue : MonoBehaviour
{
    [Header("UI 元件")]
    public GameObject dialogueBox;              // 對話框 Panel (可控制顯示/隱藏)
    public TextMeshProUGUI dialogueText;        // 顯示用戶文字
    public Image trumpImage;                    // Trump 頭像用的 UI Image
    public Sprite trumpIdle;                    // 閉嘴圖
    public Sprite trumpTalking;                 // 講話中圖
    public AudioSource audioSource;             // 語音播放 AudioSource

    [Header("打字機設定")]
    public float typingSpeed = 0.05f;

    private Coroutine typingCoroutine;
    private Coroutine speakingLoopCoroutine;

    private void Start()
    {
        // 遊戲開始時預設隱藏對話框與文字
        dialogueBox.gameObject.SetActive(false);
        dialogueText.text = "";
        trumpImage.sprite = trumpIdle;
    }
    public void PlayTrumpSpeech(string text, AudioClip clip)
    {
        // if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        //
        // dialogueBox.SetActive(true);
        // dialogueText.text = "";
        //
        // trumpImage.sprite = trumpTalking;
        // audioSource.clip = clip;
        // audioSource.Play();
        //
        // typingCoroutine = StartCoroutine(TypeText(text));
        // StartCoroutine(EndSpeechAfterAudio());
        // if (speakingLoopCoroutine != null)
        //     StopCoroutine(speakingLoopCoroutine);
        // speakingLoopCoroutine = StartCoroutine(TrumpTalkLoop());
        // audioSource.clip = clip;
        // audioSource.Play();
        // StartCoroutine(TypeSentence(dialogueText.text, clip.length));
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (speakingLoopCoroutine != null) StopCoroutine(speakingLoopCoroutine);

        dialogueBox.SetActive(true);
        dialogueText.text = "";

        trumpImage.sprite = trumpTalking;
        audioSource.clip = clip;
        audioSource.Play();

        // ✅ 只啟用一種打字機
        typingCoroutine = StartCoroutine(TypeSentence(text, clip.length));

        StartCoroutine(EndSpeechAfterAudio());
        speakingLoopCoroutine = StartCoroutine(TrumpTalkLoop());
    }

    private IEnumerator TypeText(string fullText)
    {
        foreach (char c in fullText)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    private IEnumerator EndSpeechAfterAudio()
    {
        yield return new WaitWhile(() => audioSource.isPlaying);
        yield return new WaitUntil(() => dialogueText.text.Length >= 1); // 確保打完字
        yield return new WaitForSeconds(0.5f); // 留個 0.5 秒緩衝
        trumpImage.sprite = trumpIdle;
        // dialogueBox.SetActive(false);
    }
    private IEnumerator TrumpTalkLoop()
    {
        bool toggle = false;
        while (audioSource.isPlaying)
        {
            trumpImage.sprite = toggle ? trumpTalking : trumpIdle;
            toggle = !toggle;
            yield return new WaitForSeconds(0.25f); // 你可以調整頻率
        }

        trumpImage.sprite = trumpIdle;
    }
    private IEnumerator TypeSentence(string text, float audioDuration)
    {
        float delay = Mathf.Max(audioDuration / Mathf.Max(1, text.Length), 0.03f);
        foreach (char letter in text)
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(delay);
        }
    }
    
}