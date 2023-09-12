using UnityEngine;
using TMPro;
using CCHMC.SpeechServices;

public class SpeechUITest : MonoBehaviour
{
    [SerializeField]
    private AzureSpeechService _azureSpeechServiceComponent; // This will show up in the inspector

    private IAzureSpeechService _azureSpeechService;

    [SerializeField]
    private TextMeshProUGUI _subtitlesText;

    [SerializeField]
    private TextMeshProUGUI _annotationText;

    private void Start()
    {
        _azureSpeechService = _azureSpeechServiceComponent as IAzureSpeechService;

        // Subscribe to the events
        _azureSpeechService.OnSpeechRecognizing += UpdateSubtitles;
        _azureSpeechService.OnSpeechRecognized += UpdateAnnotation;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the events
        _azureSpeechService.OnSpeechRecognizing -= UpdateSubtitles;
        _azureSpeechService.OnSpeechRecognized -= UpdateAnnotation;
    }

    private void UpdateSubtitles(string text)
    {
        _subtitlesText.text = text;
    }

    private void UpdateAnnotation(string text)
    {
        _annotationText.text = text;
    }
}
