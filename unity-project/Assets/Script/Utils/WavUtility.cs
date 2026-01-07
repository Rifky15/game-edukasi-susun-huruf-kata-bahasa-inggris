using UnityEngine;
using System.Collections;
using System.IO;

public static class WavUtility
{
    // Method to convert WAV bytes to AudioClip
    public static AudioClip ToAudioClip(byte[] fileBytes, int offsetSamples, string clipName)
    {
        // WAV header length (44 bytes)
        int headerSize = 44;

        // Check the file length to ensure it's valid WAV
        if (fileBytes.Length < headerSize)
        {
            Debug.LogError("Invalid WAV file.");
            return null;
        }

        // Read the sample rate from the WAV header (bytes 24-27)
        int sampleRate = System.BitConverter.ToInt32(fileBytes, 24);
        
        // The number of samples in the WAV file (2 bytes per sample, assuming 16-bit PCM)
        int sampleCount = (fileBytes.Length - headerSize) / 2;  // 2 bytes per sample

        // Read the number of channels from the WAV header (bytes 22-23)
        short numChannels = System.BitConverter.ToInt16(fileBytes, 22);

        // Create the AudioClip (using sample rate from WAV file)
        AudioClip audioClip = AudioClip.Create(clipName, sampleCount, numChannels, sampleRate, false);

        // Fill the AudioClip with data
        short[] audioData = new short[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            audioData[i] = System.BitConverter.ToInt16(fileBytes, headerSize + (i * 2));
        }

        // Convert the short[] to float[] (normalized to -1 to 1)
        float[] audioFloats = new float[sampleCount];
        for (int i = 0; i < audioData.Length; i++)
        {
            audioFloats[i] = audioData[i] / 32768.0f;  // Normalize to -1 to 1
        }

        // Set the samples to the AudioClip
        audioClip.SetData(audioFloats, offsetSamples);

        return audioClip;
    }
}
