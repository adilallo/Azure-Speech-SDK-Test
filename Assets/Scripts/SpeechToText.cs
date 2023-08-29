using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using TMPro;
using System.Collections.Generic;

public class SpeechToText : MonoBehaviour
{
    [Header(" Authentication ")]
    [SerializeField] private string subscriptionKey = "";
    [SerializeField] private string region = "";
    private SpeechConfig config;
    private SpeechRecognizer recognizer;

    [Header(" UI ")]
    [SerializeField] private TextMeshProUGUI subtitlesText;

    private Queue<string> recognizedTextQueue = new Queue<string>();

    void Start()
    {
        config = SpeechConfig.FromSubscription(subscriptionKey, region);
        recognizer = new SpeechRecognizer(config);

        recognizer.Recognizing += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizingSpeech)
            {
                Debug.Log($"Partially Recognized: {e.Result.Text}");
                recognizedTextQueue.Enqueue(e.Result.Text);
            }
        };

        recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                Debug.Log($"Recognized: {e.Result.Text}");
            }
        };

        recognizer.Canceled += (s, e) =>
        {
            Debug.Log($"CANCELED: Reason={e.Reason}");

            if (e.Reason == CancellationReason.Error)
            {
                Debug.LogError($"CANCELED: ErrorCode={e.ErrorCode}");
                Debug.LogError($"CANCELED: ErrorDetails={e.ErrorDetails}");
            }
        };
    }

    void Update()
    {
        while (recognizedTextQueue.Count > 0)
        {
            subtitlesText.text = recognizedTextQueue.Dequeue();
        }
    }

    public async void SubtitlesOn()
    {
        await recognizer.StartContinuousRecognitionAsync();
    }

    public async void SubtitlesOff()
    {
        await recognizer.StopContinuousRecognitionAsync();
    }

    void OnDestroy()
    {
        recognizer.Dispose();
    }
}

