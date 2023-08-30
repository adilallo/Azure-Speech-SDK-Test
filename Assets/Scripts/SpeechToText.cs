using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using TMPro;
using System;
using System.Collections.Generic;
using System.Collections;

public class SpeechToText : MonoBehaviour
{
    private const string CLEAR_COMMAND = "__CLEAR__";

    [Header("Authentication")]
    [SerializeField] private string subscriptionKey = "";
    [SerializeField] private string region = "";
    private SpeechConfig config;
    private SpeechRecognizer recognizer;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI subtitlesText;
    [SerializeField] private int subtitleLength = 25; 
    [SerializeField] private TextMeshProUGUI annotationText;

    private bool displaySubtitles = false;
    private bool isRecognizing = false;
    private bool isAnnotateMode = false;

    private Queue<string> recognizedTextQueue = new Queue<string>();
    private Queue<string> annotationTextQueue = new Queue<string>();

    private System.Object threadLocker = new System.Object();

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

        // Ensure you handle errors here, e.g. invalid subscription details
        if (recognizer == null)
        {
            Debug.LogError("Failed to create recognizer. Please check your subscription details.");
            return;
        }

        recognizer.Recognizing += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizingSpeech && displaySubtitles)
            {
                lock (threadLocker)
                {
                    recognizedTextQueue.Enqueue(e.Result.Text);
                }
            }
        };

        recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                lock (threadLocker)
                {
                    if (isAnnotateMode)
                    {
                        annotationTextQueue.Enqueue(e.Result.Text);
                        isAnnotateMode = false;
                        recognizedTextQueue.Enqueue(CLEAR_COMMAND);
                    }
                    else
                    {
                        recognizedTextQueue.Enqueue(CLEAR_COMMAND);
                    }
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
                // Consider adding more robust error handling here
            }
        };
    }

    private string currentRecognitionContent = "";

    private void UpdateSubtitles()
    {
        lock (threadLocker)
        {
            if (recognizedTextQueue.Count > 0)
            {
                // Only take the most recent item from the queue
                string text = recognizedTextQueue.Dequeue();
                recognizedTextQueue.Clear(); // Clear out any other items

                if (text == CLEAR_COMMAND)
                {
                    StartCoroutine(ClearSubtitlesAfterDelay(1f));
                    currentRecognitionContent = ""; // Clear the current content
                }
                else
                {
                    currentRecognitionContent = text;  // Reset the current content

                    // Display only the last segment (up to the subtitle length) of the current content
                    int startIdx = Mathf.Max(0, currentRecognitionContent.Length - subtitleLength);
                    subtitlesText.text = currentRecognitionContent.Substring(startIdx);
                }
            }
        }
    }


    private void UpdateAnnotation()
    {
        lock (threadLocker)
        {
            if (annotationTextQueue.Count > 0)
            {
                annotationText.text = annotationTextQueue.Dequeue();
            }
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
        StartCoroutine(ClearSubtitlesAfterDelay(1f));
    }

    public void AnnotateSpeech()
    {
        isAnnotateMode = true;
        recognizedTextQueue.Clear(); // Clear any pending recognized phrases
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


