using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class SpeechToText : MonoBehaviour
{
    private const string CLEAR_COMMAND = "__CLEAR__";

    [Header(" Authentication ")]
    [SerializeField] private string subscriptionKey = "";
    [SerializeField] private string region = "";
    private SpeechConfig config;
    private SpeechRecognizer recognizer;

    [Header(" UI ")]
    [SerializeField] private TextMeshProUGUI subtitlesText;
    [SerializeField] private TextMeshProUGUI annotationText;

    private bool displaySubtitles = false;
    private bool isRecognizing = false;
    private bool isAnnotateMode = false;

    private Queue<string> recognizedTextQueue = new Queue<string>();
    private Queue<string> annotationTextQueue = new Queue<string>();

    void Start()
    {
        InitializeRecognizer();
    }

    void Update()
    {
        UpdateSubtitles();
        UpdateAnnotation();
    }

    private void InitializeRecognizer()
    {
        config = SpeechConfig.FromSubscription(subscriptionKey, region);
        recognizer = new SpeechRecognizer(config);

        recognizer.Recognizing += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizingSpeech && displaySubtitles)
            {
                recognizedTextQueue.Enqueue(e.Result.Text);
            }
        };

        recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                if (isAnnotateMode)
                {
                    annotationTextQueue.Enqueue(e.Result.Text);
                    isAnnotateMode = false;
                }
                else
                {
                    recognizedTextQueue.Enqueue(CLEAR_COMMAND);
                }
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

    private void UpdateSubtitles()
    {
        while (recognizedTextQueue.Count > 0)
        {
            string text = recognizedTextQueue.Dequeue();

            if (text == CLEAR_COMMAND)
            {
                StartCoroutine(ClearSubtitlesAfterDelay(3f));
            }
            else
            {
                subtitlesText.text = text;
            }
        }
    }

    private void UpdateAnnotation()
    {
        if (annotationTextQueue.Count > 0)
        {
            annotationText.text = annotationTextQueue.Dequeue();
        }
    }

    public void SubtitlesOn()
    {
        displaySubtitles = true;
        StartRecognizerIfNotRunning();
    }

    public void SubtitlesOff()
    {
        displaySubtitles = false;
    }

    public void AnnotateSpeech()
    {
        isAnnotateMode = true;
        StartRecognizerIfNotRunning();
    }

    private async void StartRecognizerIfNotRunning()
    {
        if (!isRecognizing)
        {
            isRecognizing = true;
            await recognizer.StartContinuousRecognitionAsync();
        }
    }

    IEnumerator ClearSubtitlesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        subtitlesText.text = "";
    }

    void OnDestroy()
    {
        recognizer.Dispose();
    }
}

