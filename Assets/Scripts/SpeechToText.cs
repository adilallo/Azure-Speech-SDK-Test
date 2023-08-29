using UnityEngine;
using Microsoft.CognitiveServices.Speech;

public class SpeechToText : MonoBehaviour
{
    [Header(" Authentication ")]
    [SerializeField] private string subscriptionKey = "";
    [SerializeField] private string region = "";
    private SpeechConfig config;
    private SpeechRecognizer recognizer;

    void Start()
    {
        config = SpeechConfig.FromSubscription(subscriptionKey, region);
        recognizer = new SpeechRecognizer(config);
        
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

        recognizer.SpeechEndDetected += (s, e) =>
        {
            Debug.Log("Speech end detected, stopping recognizer...");
            recognizer.StopContinuousRecognitionAsync();
        };
    }

    public async void OnButtonPress()
    {
        await recognizer.StartContinuousRecognitionAsync();
    }

    void OnDestroy()
    {
        recognizer.Dispose();
    }
}
