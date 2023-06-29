using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class CheckAudioClip : MonoBehaviour
{
    public double bpm = 140.0F;
    public float gain = 0.5F;
    public int signatureHi = 4;
    public int signatureLo = 4;

    private double nextTick = 0.0F;
    private float amp = 0.0F;
    private float phase = 0.0F;
    private double sampleRate = 0.0F;
    private int accent;
    private bool running = false;
    public AudioSource audioSource;

    void Start()
    {
        accent = signatureHi;
        double startTick = AudioSettings.dspTime;
        sampleRate = AudioSettings.outputSampleRate;
        nextTick = startTick * sampleRate;
        running = true;
    }

    //void OnAudioFilterRead(float[] data, int channels)
    //{
        
    //    if (!running)
    //        return;

    //    double samplesPerTick = sampleRate * 60.0F / bpm * 4.0F / signatureLo;
    //    double sample = AudioSettings.dspTime * sampleRate;
    //    int dataLen = data.Length / channels;

    //    int n = 0;
    //    while (n < dataLen)
    //    {
    //        float x = gain * amp * Mathf.Sin(phase);
    //        int i = 0;
    //        while (i < channels)
    //        {
    //            data[n * channels + i] += x;
    //            i++;
    //        }
    //        while (sample + n >= nextTick)
    //        {
    //            nextTick += samplesPerTick;
    //            amp = 1.0F;
    //            if (++accent > signatureHi)
    //            {
    //                accent = 1;
    //                amp *= 2.0F;
    //            }
    //            Debug.Log("Tick: " + accent + "/" + signatureHi);
    //        }
    //        phase += amp * 0.3F;
    //        amp *= 0.993F;
    //        n++;
    //    }
    //}
//    //async Task RecognizeSpeechAsync()
//    {
//        // Create a new speech recognizer object with your Azure service credentials
//        var config = SpeechConfig.FromSubscription("your_subscription_key", "your_service_region");
//    var recognizer = new SpeechRecognizer(config);

//    // Convert the `float` array to a `byte` array
//    byte[] audioBytes = new byte[audioData.Length * 2];
//    int rescaleFactor = 32767; //to convert float to Int16, scale to Int16 range
//        for (int i = 0; i<audioData.Length; i++)
//        {
//            short value = (short)(audioData[i] * rescaleFactor);
//    byte[] byteArr = BitConverter.GetBytes(value);
//    byteArr.CopyTo(audioBytes, i* 2);
//        }

//// Create a new MemoryStream object and pass the `audioBytes` array as the argument
//MemoryStream stream = new MemoryStream(audioBytes);

//// Start the speech recognition process
//var result = await recognizer.RecognizeOnceAsync(new AudioDataStream(stream));

//// Process the speech recognition result
//if (result.Reason == ResultReason.RecognizedSpeech)
//{
//    Debug.Log($"Recognized: {result.Text}");
//}
//else if (result.Reason == ResultReason.NoMatch)
//{
//    Debug.Log("NOMATCH: Speech not recognized");
//}

//recognizer.Dispose();
//    }

//    async Task RecognizeSpeechAsync()
//{
//    // Start the speech recognition process
//    var result = await recognizer.RecognizeAsync(audioData);

//    // Process the speech recognition result...
//}


void Update()
    {
        if (audioSource == null || audioSource.isPlaying == false)
        {
            Debug.Log("ses yok");
            return;
        }
        float[] samples = new float[audioSource.timeSamples *2];
        audioSource.GetOutputData(samples, 0);

        for (int i = 0; i < samples.Length; ++i)
        {
            samples[i] = samples[i] * 0.5f;
            Debug.Log(samples);
            ConvertFloatArrayToInt16ByteArray(samples);
        }
        //Debug.Log(audioSource.isVirtual);
        Debug.Log(audioSource.timeSamples);
        audioSource.clip.SetData(samples, 0);
        Debug.Log(samples);

    }
    private byte[] ConvertFloatArrayToInt16ByteArray(float[] data)
    {
        MemoryStream dataStream = new MemoryStream();
        int x = sizeof(Int16);
        Int16 maxValue = Int16.MaxValue;
        int i = 0;
        while (i < data.Length)
        {
            dataStream.Write(BitConverter.GetBytes(Convert.ToInt16(data[i] * maxValue)), 0, x);
            ++i;
        }
        byte[] bytes = dataStream.ToArray();
        Debug.Log(bytes);
        dataStream.Dispose();
        return bytes;
    }

}






