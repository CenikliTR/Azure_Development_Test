using Microsoft.CognitiveServices.Speech.Audio;
using System.IO;
using System;
using UnityEngine.Networking;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO.Pipes;

public class SampleToWave 
{
    private AudioStreamFormat audioFormat;
    private PushAudioInputStream stream;
    public PushAudioInputStream GetStream(uint samplesPerSecond = 16000, byte bitsPerSample = 16, byte channels = 2) {
        audioFormat = AudioStreamFormat.GetWaveFormatPCM(samplesPerSecond, bitsPerSample, channels);
        //stream = new PushAudioInputStream(audioFormat);
        stream = AudioInputStream.CreatePushStream(audioFormat);
        return stream;
    }

    public void Write(float[] chunk)
    {
        Byte[] byteChunk = ConvertToByteArray(chunk);
        stream.Write(byteChunk, byteChunk.Length);
    }

    public void Write(Byte[] byteChunk)
    {
        stream.Write(byteChunk, byteChunk.Length);
    }

    private Byte[] ConvertAudioClipDataToInt16ByteArray(float[] data)
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
        Byte[] bytes = dataStream.ToArray();

        //  Validate converted bytes
        if(data.Length * x == bytes.Length)
        {
            Debug.Log("Unexpected float[] to Int16 to byte[] size: "+ data.Length * x + " == "+ bytes.Length);
        }

        dataStream.Dispose();

        return bytes;
    }

    private Byte[] ConvertToByteArray(float[] dataSource)
    {

        var intData = new Int16[dataSource.Length];
        //converting in 2 steps : float[] to Int16[], //then Int16[] to Byte[]

        var bytesData = new Byte[dataSource.Length * 2];
        //bytesData array is twice the size of
        //dataSource array because a float converted in Int16 is 2 bytes.

        var rescaleFactor = 32767; //to convert float to Int16

        for (var i = 0; i < dataSource.Length; i++)
        {
            intData[i] = (Int16)(dataSource[i] * rescaleFactor);
            var byteArr = new Byte[2];
            byteArr = BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        return bytesData;


    }


}