using UnityEditor;
using UnityEngine;
using System.IO;
using System;

public class AudioClipCutterWindow : EditorWindow
{
    private AudioClip audioClip;
    private float startTime = 0f;
    private float endTime = 1f;
    private Vector2 scrollPos;
    private bool isPlayingPreview = false;
    private AudioSource audioSource;

    [MenuItem("Tools/Audio Clip Cutter")]
    public static void ShowWindow()
    {
        GetWindow<AudioClipCutterWindow>("Audio Clip Cutter");
    }

    private void OnEnable()
    {
        audioSource = new GameObject("AudioSourcePreview").AddComponent<AudioSource>();
        audioSource.hideFlags = HideFlags.HideAndDontSave;
    }

    private void OnDisable()
    {
        if (audioSource != null)
        {
            DestroyImmediate(audioSource.gameObject);
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Audio Clip Cutter", EditorStyles.boldLabel);
        audioClip = (AudioClip)EditorGUILayout.ObjectField("Audio Clip", audioClip, typeof(AudioClip), false);

        if (audioClip == null)
        {
            EditorGUILayout.HelpBox("Please assign an audio clip to start editing.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Cut Range", EditorStyles.boldLabel);
        EditorGUILayout.MinMaxSlider(ref startTime, ref endTime, 0f, audioClip.length);
        EditorGUILayout.LabelField("Start Time: " + startTime.ToString("F2") + " sec", "End Time: " + endTime.ToString("F2") + " sec");

        EditorGUILayout.Space();
        if (GUILayout.Button(isPlayingPreview ? "Stop Preview" : "Preview Cut"))
        {
            if (isPlayingPreview)
            {
                audioSource.Stop();
                isPlayingPreview = false;
            }
            else
            {
                PlayPreview();
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Cut and Save as New Clip"))
        {
            CutAudioClip();
        }

        Repaint();
    }

    private void PlayPreview()
    {
        if (audioClip == null) return;

        audioSource.clip = audioClip;
        audioSource.time = startTime;
        audioSource.Play();
        isPlayingPreview = true;

        EditorApplication.update += StopPreviewWhenNeeded;
    }

    private void StopPreviewWhenNeeded()
    {
        if (audioSource.time >= endTime || !audioSource.isPlaying)
        {
            audioSource.Stop();
            isPlayingPreview = false;
            EditorApplication.update -= StopPreviewWhenNeeded;
        }
    }

    private void CutAudioClip()
    {
        if (audioClip == null) return;

        string path = AssetDatabase.GetAssetPath(audioClip);
        float[] samples = new float[(int)((endTime - startTime) * audioClip.frequency * audioClip.channels)];
        float[] allSamples = new float[audioClip.samples * audioClip.channels];

        audioClip.GetData(allSamples, 0);
        int startSample = (int)(startTime * audioClip.frequency) * audioClip.channels;
        int endSample = (int)(endTime * audioClip.frequency) * audioClip.channels;

        for (int i = 0; i < samples.Length; i++)
        {
            samples[i] = allSamples[startSample + i];
        }

        AudioClip newClip = AudioClip.Create(audioClip.name + "_Cut", samples.Length / audioClip.channels, audioClip.channels, audioClip.frequency, false);
        newClip.SetData(samples, 0);

        string newPath = Path.Combine(Path.GetDirectoryName(path), audioClip.name + "_Cut.wav");
        SaveWavUtility.Save(newPath, newClip);
        AssetDatabase.ImportAsset(newPath);
        AssetDatabase.Refresh();

        EditorUtility.DisplayDialog("Audio Clip Cutter", "New audio clip saved as " + newPath, "OK");
    }
}

public static class SaveWavUtility
{
    public static void Save(string filepath, AudioClip clip)
    {
        if (!filepath.ToLower().EndsWith(".wav"))
        {
            filepath += ".wav";
        }

        Directory.CreateDirectory(Path.GetDirectoryName(filepath));

        using (var fileStream = CreateEmpty(filepath))
        {
            ConvertAndWrite(fileStream, clip);
            WriteHeader(fileStream, clip);
        }

        Debug.Log("Wav file saved at: " + filepath);
    }

    private static FileStream CreateEmpty(string filepath)
    {
        var fileStream = new FileStream(filepath, FileMode.Create);
        byte emptyByte = new byte();

        for (int i = 0; i < 44; i++) // Prepare the header with empty bytes
        {
            fileStream.WriteByte(emptyByte);
        }

        return fileStream;
    }

    private static void ConvertAndWrite(FileStream fileStream, AudioClip clip)
    {
        float[] samples = new float[clip.samples * clip.channels];
        clip.GetData(samples, 0);

        Int16[] intData = new Int16[samples.Length];
        byte[] bytesData = new byte[samples.Length * 2];

        const float rescaleFactor = 32767; // to convert float to Int16

        for (int i = 0; i < samples.Length; i++)
        {
            intData[i] = (short)(samples[i] * rescaleFactor);
            byte[] byteArr = System.BitConverter.GetBytes(intData[i]);
            byteArr.CopyTo(bytesData, i * 2);
        }

        fileStream.Write(bytesData, 0, bytesData.Length);
    }

    private static void WriteHeader(FileStream fileStream, AudioClip clip)
    {
        int hz = clip.frequency;
        int channels = clip.channels;
        int samples = clip.samples;

        fileStream.Seek(0, SeekOrigin.Begin);

        byte[] riff = System.Text.Encoding.UTF8.GetBytes("RIFF");
        fileStream.Write(riff, 0, 4);

        byte[] chunkSize = System.BitConverter.GetBytes(fileStream.Length - 8);
        fileStream.Write(chunkSize, 0, 4);

        byte[] wave = System.Text.Encoding.UTF8.GetBytes("WAVE");
        fileStream.Write(wave, 0, 4);

        byte[] fmt = System.Text.Encoding.UTF8.GetBytes("fmt ");
        fileStream.Write(fmt, 0, 4);

        byte[] subChunk1 = System.BitConverter.GetBytes(16);
        fileStream.Write(subChunk1, 0, 4);

        UInt16 two = 2;
        UInt16 one = 1;

        byte[] audioFormat = System.BitConverter.GetBytes(one);
        fileStream.Write(audioFormat, 0, 2);

        byte[] numChannels = System.BitConverter.GetBytes(channels);
        fileStream.Write(numChannels, 0, 2);

        byte[] sampleRate = System.BitConverter.GetBytes(hz);
        fileStream.Write(sampleRate, 0, 4);

        byte[] byteRate = System.BitConverter.GetBytes(hz * channels * 2);
        fileStream.Write(byteRate, 0, 4);

        UInt16 blockAlign = (ushort)(channels * 2);
        fileStream.Write(System.BitConverter.GetBytes(blockAlign), 0, 2);

        UInt16 bps = 16;
        byte[] bitsPerSample = System.BitConverter.GetBytes(bps);
        fileStream.Write(bitsPerSample, 0, 2);

        byte[] datastring = System.Text.Encoding.UTF8.GetBytes("data");
        fileStream.Write(datastring, 0, 4);

        byte[] subChunk2 = System.BitConverter.GetBytes(samples * channels * 2);
        fileStream.Write(subChunk2, 0, 4);
    }
}
