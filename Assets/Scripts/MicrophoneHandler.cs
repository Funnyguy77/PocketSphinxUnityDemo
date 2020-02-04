using System.ComponentModel;
using UnityEngine;

/* This is just a small class I wrote to help implement microphone input.
 * How this works: A user can call BeginRecording to start the microphone. Once finished call EndRecording(). This trims your audio clip, and returns the trimmed clip.
 * 
 * Note: I won't go into to much detail with the documentation here. This was written just to keep our main example a little cleaner.
 */
public class MicrophoneHandler
{
    public enum SamplingRateEnum
    {
        [Description("8K")]
        EightK = 8000,
        [Description("11K")]
        ElevenK = 11025,
        [Description("16K")]
        SixteenK = 16000,
        [Description("22K")]
        TwentyTwoK = 22050,
        [Description("32K")]
        ThirtyTwoK = 32000,
        [Description("44K")]
        FortyFourK = 44100,
        [Description("48K")]
        FortyEightK = 48000,
    }

    // Publicly exposed properties. 
    public string Name { get; } // Name of your microphone input. Ex: Microphone.devices[0]
    public SamplingRateEnum SamplingRate { get; } // Sampling rate of your microphone.
    public int ClipTime { get; } // Max length of a recorded AudioClip.

    // Events
    public delegate void RecordingFinishedHandler(AudioClip audio);
    public event RecordingFinishedHandler RecordingFinished;

    // Note: This is the audio clip we use to store our Microphone data.
    private AudioClip recording;

    public MicrophoneHandler(string _name, SamplingRateEnum _samplingRate, int _clipTime)
    {
        Name = _name;
        SamplingRate = _samplingRate;
        ClipTime = _clipTime;
    }

    public void BeginRecording()
    {
        recording = Microphone.Start(Name, true, ClipTime, (int)SamplingRate);
#if UNITY_EDITOR
        Debug.Log("<b><color=yellow>Recording started...</color></b>");
#endif
    }

    public void EndRecording()
    {
        var position = Microphone.GetPosition(Name);
        var soundData = new float[recording.samples * recording.channels];
        recording.GetData(soundData, 0);

        var newData = new float[position * recording.channels];

        for (int i = 0; i < newData.Length; i++)
        {
            newData[i] = soundData[i];
        }

        // Create a new trimmed clip.
        AudioClip newClip = AudioClip.Create(recording.name, position, recording.channels, recording.frequency, false);
        newClip.SetData(newData, 0);
        Object.Destroy(recording);

        if (RecordingFinished != null)
            RecordingFinished.Invoke(newClip);

#if UNITY_EDITOR
        Debug.Log("<b>Recording stopped...</b>");
#endif
    }
}
