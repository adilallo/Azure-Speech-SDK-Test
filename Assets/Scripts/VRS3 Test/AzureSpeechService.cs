using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.CognitiveServices.Speech;
using UnityEngine;

namespace CCHMC.SpeechServices
{
    public class AzureSpeechService : MonoBehaviour, IAzureSpeechService
    {
        [Header("Authentication")]
        [SerializeField]
        private string _subscriptionKey = "";

        [SerializeField]
        private string _region = "";

        private SpeechConfig _config;
        private SpeechRecognizer _recognizer;
        private readonly object _threadLocker = new object();

        private Queue<string> _recognizedTextQueue = new Queue<string>();
        private Queue<string> _recognizingTextQueue = new Queue<string>();

        public event Action<string> OnSpeechRecognized;
        public event Action<string> OnSpeechRecognizing;

        private void Awake()
        {
            _subscriptionKey = File.ReadAllText("Assets/SpeechSDK/AzureConfig.txt");
            InitializeRecognizer();
        }

        private void Update()
        {
            DequeueAndInvokeText();
        }

        public void StartRecognition()
        {
            if (_recognizer != null)
            {
                _recognizer.StartContinuousRecognitionAsync();
            }
        }

        public void StopRecognition()
        {
            if (_recognizer != null)
            {
                _recognizer.StopContinuousRecognitionAsync();
            }
        }

        private void OnDestroy()
        {
            if (_recognizer != null)
            {
                _recognizer.Dispose();
            }
        }

        /// <summary>
        /// Initializes the speech recognizer.
        /// </summary>
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
                if (e.Result.Reason == ResultReason.RecognizingSpeech)
                {
                    lock (_threadLocker)
                    {
                        _recognizingTextQueue.Enqueue(e.Result.Text);
                    }
                }
            };

            _recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    lock (_threadLocker)
                    {
                        _recognizedTextQueue.Enqueue(e.Result.Text);
                    }
                }
            };
        }

        /// <summary>
        /// Dequeues recognized and recognizing text and invokes corresponding events.
        /// </summary>
        private void DequeueAndInvokeText()
        {
            lock (_threadLocker)
            {
                while (_recognizedTextQueue.Count > 0)
                {
                    OnSpeechRecognized?.Invoke(_recognizedTextQueue.Dequeue());
                }

                while (_recognizingTextQueue.Count > 0)
                {
                    OnSpeechRecognizing?.Invoke(_recognizingTextQueue.Dequeue());
                }
            }
        }
    }
}
