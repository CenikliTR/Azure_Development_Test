using System;
using Furioos.SDK;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using Unity.RenderStreaming;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Random = System.Random;

public class RenderStremingManager : MonoBehaviour
{
    // Start is called before the first frame update
    public SignalingManager renderStreaming;
    public SingleConnection singleConnection;
    public VideoStreamReceiver videoReceiver;
    public AudioStreamReceiver audioReceiver;
    //public WebBrowserInputChannelReceiver inputReceiver;
    public RawImage remoteVideoImage;
    public AudioSource remoteAudioSource;
    [SerializeField] string connectionId;
    public TMP_Text connectionIdBox;
    private Random _randomConnectionId = new Random();
    private SpeechRecognizer recognizer;
    private SampleToWave sampleToWave;

    private string recognizedString = "";
    private string errorString = "";
    private System.Object threadLocker = new System.Object();
    private bool started = false;
    private int m_sampleRate;
    private void Awake()
    {
        videoReceiver.OnUpdateReceiveTexture = UpdateReceiveTexture;
        audioReceiver.targetAudioSource = remoteAudioSource;
        
        audioReceiver.OnUpdateReceiveAudioSource = UpdateReceiveAudioSource;
        //audioReceiver.

        
        
    }

    private void StartedChannel(string connectionId)
    {
        Debug.Log("input channel connected " + connectionId);
    }


    private void UpdateReceiveAudioSource(AudioSource source)
    {
        source.loop = true;
        source.Play();
    }

    private void UpdateReceiveTexture(Texture receivetexture)
    {
        remoteVideoImage.texture = receivetexture;
    }

    private void Start()
    {
        if (renderStreaming.runOnAwake)
            return;


        renderStreaming.Run();
          
        byte channels = 2;
        byte bitsPerSample = 32;
        uint samplesPerSecond = (uint) m_sampleRate; // or 8000
        sampleToWave = new SampleToWave();
        //AudioProcessingOptions aop = AudioProcessingOptions.Create(AudioProcessingConstants.);
        //C:\Users\cenik\cc_Hakan_Speech_To_Text\Assets\test48779885.wav

        var audioConfig = AudioConfig.FromStreamInput(sampleToWave.GetStream(samplesPerSecond, bitsPerSample, channels));
        //var audioConfig = AudioConfig.FromWavFileInput("C:\\Users\\cenik\\cc_Hakan_Speech_To_Text\\Assets\\test48779885.wav");
        var speechConfig = SpeechConfig.FromSubscription("fb612a2f97f948519d5da804f4af5730", "westeurope");
        speechConfig.SpeechRecognitionLanguage = "tr-tr";

        recognizer = new SpeechRecognizer(speechConfig, audioConfig);

        if (recognizer != null)
        {
            // Subscribes to speech events.
            recognizer.Recognizing += RecognizingHandler;
            recognizer.Recognized += RecognizedHandler;
            recognizer.SpeechStartDetected += SpeechStartDetectedHandler;
            recognizer.SpeechEndDetected += SpeechEndDetectedHandler;
            recognizer.Canceled += CanceledHandler;
            recognizer.SessionStarted += SessionStartedHandler;
            recognizer.SessionStopped += SessionStoppedHandler;
        }

       

    }

    public string filePath;
    public void StartSavingIntoFile()
    {
        Random rnd = new Random();
        filePath = SaveWav.CreateFileStreamSamples("test" + rnd.Next(1,100000000) + ".wav");
        started = true;
    }

    public void StopAndSaveFile()
    {
        started = false;
        SaveWav.StopSaveSamples(2048, m_sampleRate, 2);
    }



    public async void StartContinuousRecognition()
    {
        Debug.Log("Starting Continuous Speech Recognition.");
        if (recognizer != null)
        {
            Debug.Log("Starting Speech Recognizer.");
            await recognizer.StartContinuousRecognitionAsync().ConfigureAwait(false);
            Debug.Log("Speech Recognizer is now running.");
            
            
        }
        Debug.Log("Start Continuous Speech Recognition exit");
    }


    public void TriggerRenderStream()
    {
        if (!string.IsNullOrEmpty(connectionId) && singleConnection.IsConnected(connectionId))
        {
            StopRenderStream();
        }
        else
        {
            StartRenderStream();
        }
    }
    private void StartRenderStream()
    {

        if (!string.IsNullOrEmpty(connectionId))
        {
            StopRenderStream();
        }
        //connectionId = System.Guid.NewGuid().ToString("N");
        connectionId = _randomConnectionId.Next(10000, 100000).ToString();
        connectionIdBox.text = connectionId;
        singleConnection.CreateConnection(connectionId);
        SendConnectionIdToClient();
    }

    private void StopRenderStream()
    {
        singleConnection.DeleteConnection(connectionId);
        remoteVideoImage.texture = null;
        remoteAudioSource.Stop();
        connectionIdBox.text = String.Empty;
    }

    /*
    public void Update()
    {
        if (!started)
            return;
        float[] samples = new float[2048];
        remoteAudioSource.GetOutputData(samples, 1);
        sampleToWave.Write(samples);
    }
    */

    private void OnAudioFilterRead(float[] data, int channels)
    {
        if (!started)
            return;
        SaveWav.SaveSamples(data);
        sampleToWave.Write(SaveWav.Converter(data));

    }

    
    //public void StartRecon() 
    //{
    //    StartContinuousRecognition();
    //}

    public async void Recognize()
    {
        var speechRecognitionResult = await recognizer.RecognizeOnceAsync();
       Debug.Log($"RECOGNIZED: Text={speechRecognitionResult.Text}");
    }

    private void OnDestroy()
    {
        StopRenderStream();
    }



    private FurioosSDK sdk;

    void OnEnable()
    {
        this.sdk = gameObject.AddComponent<FurioosSDK>();
        this.sdk.OnSDKMessage += this.OnSDKMessage;
        this.sdk.OnSDKSessionStart += this.OnSDKSessionStart;
        this.sdk.OnSDKSessionStop += this.OnSDKSessionStop;
        OnAudioConfigurationChanged(false);
        AudioSettings.OnAudioConfigurationChanged += OnAudioConfigurationChanged;
    }

    void OnAudioConfigurationChanged(bool deviceWasChanged)
    {
        m_sampleRate = AudioSettings.outputSampleRate;
    }

    void OnDisable()
    {

        this.sdk.OnSDKMessage -= this.OnSDKMessage;
        this.sdk.OnSDKSessionStart -= this.OnSDKSessionStart;
        this.sdk.OnSDKSessionStop -= this.OnSDKSessionStop;
        
            StopRecognition();
        recognizer.Dispose();
        recognizer = null;
        AudioSettings.OnAudioConfigurationChanged -= OnAudioConfigurationChanged;
        StopAndSaveFile();
        /// <summary>
        /// Stops the recognition on the speech recognizer or translator as applicable.
        /// Important: Unhook all events & clean-up resources.
        /// </summary>
        /// 
    }
    public async void StopRecognition()
    {
        if (recognizer != null)
        {
            await recognizer.StopContinuousRecognitionAsync().ConfigureAwait(false);
            recognizer.Recognizing -= RecognizingHandler;
            recognizer.Recognized -= RecognizedHandler;
            recognizer.SpeechStartDetected -= SpeechStartDetectedHandler;
            recognizer.SpeechEndDetected -= SpeechEndDetectedHandler;
            recognizer.Canceled -= CanceledHandler;
            recognizer.SessionStarted -= SessionStartedHandler;
            recognizer.SessionStopped -= SessionStoppedHandler;
            recognizedString = "Speech Recognizer is now stopped.";
            Debug.Log("Speech Recognizer is now stopped.");
            
        }
    }

            private void OnSDKSessionStart(string from)
    {
        Debug.Log("SDK session started : \"" + from);
    }

    private void OnSDKSessionStop(string from)
    {
        Debug.Log("SDK session stopped : \"" + from);
    }

    public void OnSDKMessage(JToken data, string from)
    {
        var value = JsonConvert.SerializeObject(data);
        Debug.Log("SDK data from \"" + from + "\" : \n" + value);
    }

    public void SendConnectionIdToClient()
    {
        string jsonMessage = "{connectionId:\"" + connectionId + "\"}";
        Debug.Log("Send Message to client: " + jsonMessage);
        this.sdk.send(JObject.Parse(jsonMessage));
    }


    #region Speech Recognition event handlers
    private void SessionStartedHandler(object sender, SessionEventArgs e)
    {
        Debug.Log($"\n    Session started event. Event: {e.ToString()}.");
        
    }

    private void SessionStoppedHandler(object sender, SessionEventArgs e)
    {
        Debug.Log($"\n    Session event. Event: {e.ToString()}.");
        Debug.Log($"Session Stop detected. Stop the recognition.");
        
       
    }

    private void SpeechStartDetectedHandler(object sender, RecognitionEventArgs e)
    {
        Debug.Log($"SpeechStartDetected received: offset: {e.Offset}.");
    }

    private void SpeechEndDetectedHandler(object sender, RecognitionEventArgs e)
    {
        Debug.Log($"SpeechEndDetected received: offset: {e.Offset}.");
        Debug.Log($"Speech end detected.");
    }

    // "Recognizing" events are fired every time we receive interim results during recognition (i.e. hypotheses)
    private void RecognizingHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizingSpeech)
        {
            Debug.Log($"HYPOTHESIS: Text={e.Result.Text}");
            lock (threadLocker)
            {
                recognizedString = $"HYPOTHESIS: {Environment.NewLine}{e.Result.Text}";
            }
        }
    }

    // "Recognized" events are fired when the utterance end was detected by the server
    private void RecognizedHandler(object sender, SpeechRecognitionEventArgs e)
    {
        if (e.Result.Reason == ResultReason.RecognizedSpeech)
        {
            Debug.Log($"RECOGNIZED: Text={e.Result.Text}");
            lock (threadLocker)
            {
                //**********Debug.Log("kesin metin");
                recognizedString = e.Result.Text;
                Debug.Log(e.Result.Text);
                //messageContainer.MessageSpeech = e.Result.Text;
                //StopRecognition();
                
            }
        }
        else if (e.Result.Reason == ResultReason.NoMatch)
        {
            Debug.Log($"NOMATCH: Speech could not be recognized.");
        }
    }

    // "Canceled" events are fired if the server encounters some kind of error.
    // This is often caused by invalid subscription credentials.
    private void CanceledHandler(object sender, SpeechRecognitionCanceledEventArgs e)
    {
        Debug.Log($"CANCELED: Reason={e.Reason}");

        errorString = e.ToString();
        if (e.Reason == CancellationReason.Error)
        {
            Debug.LogError($"CANCELED: ErrorDetails={e.ErrorDetails}");
            Debug.LogError("CANCELED: Did you update the subscription info?");
        }
    }
    #endregion
}