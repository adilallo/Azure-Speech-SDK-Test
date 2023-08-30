using UnityEngine;

public class VoiceCommand : MonoBehaviour
{
    [Header("Language Understanding")]
    [SerializeField] private string LANGUAGE_KEY;
    [SerializeField] private string LANGUAGE_ENDPOINT;

    [Header("Speech SDK")]
    [SerializeField] private string SPEECH_KEY;
    [SerializeField] private string SPEECH_REGION;

    // Create public getters for these variables if needed elsewhere in your code
    public string LanguageKey => LANGUAGE_KEY;
    public string LanguageEndpoint => LANGUAGE_ENDPOINT;
    public string SpeechKey => SPEECH_KEY;
    public string SpeechRegion => SPEECH_REGION;
}

