using UnityEngine;
using Microsoft.CognitiveServices.Speech;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class SpeechToText : MonoBehaviour
{
    private const string _clearCommand = "__CLEAR__";

    [Header("Authentication")]
    [SerializeField] private string _subscriptionKey = "";
    [SerializeField] private string _region = "";
    private SpeechConfig _config;
    private SpeechRecognizer _recognizer;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI _subtitlesText;
    [SerializeField] private int _subtitleLength = 25;
    [SerializeField] private TextMeshProUGUI _annotationText;

    private bool _displaySubtitles = false;
    private bool _isRecognizing = false;
    private bool _isAnnotateMode = false;

    private Queue<string> _recognizedTextQueue = new Queue<string>();
    private Queue<string> _annotationTextQueue = new Queue<string>();

    private readonly System.Object _threadLocker = new System.Object();

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
        _config = SpeechConfig.FromSubscription(_subscriptionKey, _region);
        _recognizer = new SpeechRecognizer(_config);

        if (_recognizer == null)
        {
            Debug.LogError("Failed to create recognizer. Please check your subscription details.");
            return;
        }

        _recognizer.Recognizing += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizingSpeech && _displaySubtitles)
            {
                lock (_threadLocker)
                {
                    _recognizedTextQueue.Enqueue(e.Result.Text);
                }
            }
        };

        _recognizer.Recognized += (s, e) =>
        {
            if (e.Result.Reason == ResultReason.RecognizedSpeech)
            {
                lock (_threadLocker)
                {
                    if (_isAnnotateMode)
                    {
                        _annotationTextQueue.Enqueue(e.Result.Text);
                        _isAnnotateMode = false;
                        _recognizedTextQueue.Enqueue(_clearCommand);
                    }
                    else
                    {
                        _recognizedTextQueue.Enqueue(_clearCommand);
                    }
                }
            }
        };

        _recognizer.Canceled += (s, e) =>
        {
            Debug.Log($"CANCELED: Reason={e.Reason}");

            if (e.Reason == CancellationReason.Error)
            {
                Debug.LogError($"CANCELED: ErrorCode={e.ErrorCode}");
                Debug.LogError($"CANCELED: ErrorDetails={e.ErrorDetails}");
            }
        };
    }

    private string _currentRecognitionContent = "";

    private void UpdateSubtitles()
    {
        lock (_threadLocker)
        {
            if (_recognizedTextQueue.Count > 0)
            {
                string text = _recognizedTextQueue.Dequeue();
                _recognizedTextQueue.Clear();

                if (text == _clearCommand)
                {
                    StartCoroutine(ClearSubtitlesAfterDelay(1f));
                    _currentRecognitionContent = "";
                }
                else
                {
                    _currentRecognitionContent = text;

                    int startIdx = Mathf.Max(0, _currentRecognitionContent.Length - _subtitleLength);
                    _subtitlesText.text = _currentRecognitionContent.Substring(startIdx);
                }
            }
        }
    }

    private void UpdateAnnotation()
    {
        lock (_threadLocker)
        {
            if (_annotationTextQueue.Count > 0)
            {
                _annotationText.text = _annotationTextQueue.Dequeue();
            }
        }
    }

    /// <summary>
    /// Enables the display of subtitles.
    /// </summary>
    public void SubtitlesOn()
    {
        _displaySubtitles = true;
        StartRecognizerIfNotRunning();
    }

    /// <summary>
    /// Disables the display of subtitles.
    /// </summary>
    public void SubtitlesOff()
    {
        _displaySubtitles = false;
        StartCoroutine(ClearSubtitlesAfterDelay(1f));
    }

    /// <summary>
    /// Activates annotation mode for speech.
    /// </summary>
    public void AnnotateSpeech()
    {
        _isAnnotateMode = true;
        _recognizedTextQueue.Clear();
        StartRecognizerIfNotRunning();
    }

    private async void StartRecognizerIfNotRunning()
    {
        if (!_isRecognizing)
        {
            _isRecognizing = true;
            await _recognizer.StartContinuousRecognitionAsync();
        }
    }

    IEnumerator ClearSubtitlesAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _subtitlesText.text = "";
    }

    void OnDestroy()
    {
        _recognizer.Dispose();
    }
}
