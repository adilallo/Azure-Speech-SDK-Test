using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CCHMC.SpeechServices
{
    public interface IAzureSpeechService
    {
        /// <summary>
        /// Starts the speech recognition process.
        /// </summary>
        void StartRecognition();

        /// <summary>
        /// Stops the speech recognition process.
        /// </summary>
        void StopRecognition();

        /// <summary>
        /// Event triggered for full transcription of spoken phrase.
        /// </summary>
        event Action<string> OnSpeechRecognized;

        /// <summary>
        /// Event triggered for partial or in progress transcription of spoken phrase.
        /// </summary>
        event Action<string> OnSpeechRecognizing;
    }
}
