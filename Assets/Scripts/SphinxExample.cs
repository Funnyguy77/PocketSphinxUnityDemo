using Pocketsphinx;
using System.Collections;
using System.IO;
using TarCs;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/* Thanks for checking out my example script for PocketSphinx in Unity! :)
 * Hopefully this resource helps others to understand using this tool within Unity. 
 * This example script should walk you through creating a decoder and having it recognize a custom keyphrase.
 * 
 * Note: when using the standard 'en-us' language model, it doesn't seem to pickup complicated sentences well. 
 * If you wish to recognize longer strings, please look at creating your own acoustic model / language model. 
 * 
 * IMPORTANT NOTE: It is *HIGHLY RECCOMENDED* to set your microphones sampling rate to 16000 for improved accuracy. (Higher sampling rates reduce accuracy dramatically)
 * 
 * More information on CMUSphinx / PocketSphinx can be found at -> https://cmusphinx.github.io/wiki/tutorial/
 */
public class SphinxExample : MonoBehaviour
{
    private Decoder d; // Decoder that's actually interpreting our speech.
    private MicrophoneHandler mic; // Handler that manages the recording of our microphone.
    private bool keyphraseDetected = false; // Simple bool for checking if we have recognized a keyphrase.
    [Header("Configuration:")]
    public string lang = "en-us"; // The language model you wish to use. (Name of your .tar file within StreamingAssets)
    public string keyphrase = "hello world"; // The actual keyphrase our decoder is looking to recognize. Note that this is case-sensitive.
    public KeyCode toggleRecordingKey = KeyCode.Space; // The key used to start recording our microphone.
    public int maxRecordingTime = 20; // Maximum length of the AudioClip recorded for your microphone.
    [Header("UI")]
    public Text sampleText; // Text that displays if a keyword was recognized. 
    public Text micText; // Text that displays the current microphone in use.

    private IEnumerator Start()
    {
        yield return WaitForMicrophoneInput();
        yield return Decompress();
        SetupDecoder();
        SetupMicrophone();
        SetupUI();
    }

    private void Update()
    {
        // If we press our key, we toggle our microphone.
        if(Input.GetKeyDown(toggleRecordingKey))
        {
            if(!Microphone.IsRecording(mic.Name))
            {
                mic.BeginRecording();
            }
            else
            {
                mic.EndRecording();
            }
        }

        // If we detect a keyphrase, update our UI to reflect this.
        if (keyphraseDetected)
        {
            sampleText.text = "Keyphrase detected!";
            StartCoroutine(WaitAndResetText());
            keyphraseDetected = false;
        }
    }

    // Desc: Pretty self-explanatory, wait until you actually have a microphone plugged in.
    private IEnumerator WaitForMicrophoneInput()
    {
        while (Microphone.devices.Length <= 0)
            yield return null;
    }

    // Desc: Setup our UI. (Normally you would do this outside of this script, but for the example it should be ok)
    private void SetupUI()
    {
        micText.text = Microphone.devices[0];
        sampleText.text = $"Say - {keyphrase}";
    }

    // Desc: Create a new MicrophoneHandler and setup our RecordingFinished event.
    private void SetupMicrophone()
    {
        mic = new MicrophoneHandler(Microphone.devices[0], MicrophoneHandler.SamplingRateEnum.SixteenK, maxRecordingTime);
        mic.RecordingFinished += ProcessAudio;
    }

    // Desc: Creates a new decoder for speech processing.
    private void SetupDecoder()
    {
        Debug.Log("<color=red>Initializing decoder...</color>");
        // Create a new configuration for our decoder.
        Config c = Decoder.DefaultConfig();
        // Find our decompressed language model.
        string speechDataPath = Path.Combine(Application.persistentDataPath, lang);
        // Find our decompressed dictionary. Note: You may need to change the dictionary name if you create a custom dictionary.
        string dictPath = Path.Combine(speechDataPath, "dictionary");
        // Creates a path to store log files.
        string logPath = Path.Combine(Application.persistentDataPath, "ps.log");

        // Tell our decoder what language model we wish to use.
        c.SetString("-hmm", speechDataPath);
        // Tell our decoder what dictionary to use.
        c.SetString("-dict", dictPath);
        // Tell our decoder what phrase to look for.
        c.SetString("-keyphrase", keyphrase);

        /* How accurate our decoder will be. For shorter keyphrases you can use smaller thresholds like 1e-1, 
         * for longer keyphrases the threshold must be bigger, up to 1e-50. 
         * If your keyphrase is very long – larger than 10 syllables – it is recommended to split it 
         * and spot for parts separately. 
         */
        c.SetFloat("-kws_threshold", 1e-15);

        // These two lines enable and save raw data to a log for debugging. Feel free to comment these lines if you have no need for logs.
        c.SetString("-logfn", logPath);
        c.SetString("-rawlogdir", Application.persistentDataPath);

        // Create a new decoder with our configuration.
        d = new Decoder(c);

        // Starts the decoder.
        d.StartUtt();
        Debug.Log("<color=green><b>Decoder initialized!</b></color>");
    }

    // Desc: This is the "secret-sauce" of PocketSphinx. This method actually provides Sphinx with data and checks to see if we detect a keyphrase.
    private void ProcessAudio(AudioClip audio)
    {
        // Create a new array for our audio data.
        var newData = new float[audio.samples * audio.channels];
        // Get our data from our AudioClip.
        audio.GetData(newData, 0);

        // Convert audio data into byte data.
        byte[] byteData = ConvertToBytes(newData, audio.channels);
        // Process the raw byte data with our decoder.
        d.ProcessRaw(byteData, byteData.Length, false, false);

        // Checks if we recognize a keyphrase.
        if (d.Hyp() != null)
        {
            keyphraseDetected = true;
            // Stop the decoder.
            d.EndUtt();
            // Start the decoder again.
            d.StartUtt();
        }
    }

    /*  Locates and decompresses your language model.
    *   Note: This is useful if you're frequently making changes to your model, or you wish to reduce your build size. 
    *   However, I would suggest decompressing your model before building as decompressing will increase your load times. 
    */
    private IEnumerator Decompress()
    {
        Debug.Log("<color=red>Decompressing language model...</color>");
        // Locate your language model.
        string dataPath = Path.Combine(Application.streamingAssetsPath, lang + ".tar");
        Stream dataStream;

        // Download your language model if you wish to store it remotely.
        if (dataPath.Contains("://"))
        {
            UnityWebRequest www = UnityWebRequest.Get(dataPath);
            www.SendWebRequest();

            while (!www.isDone)
            {
                yield return null;
            }

            dataStream = new MemoryStream(www.downloadHandler.data);
        }
        // Otherwise look for the file locally.
        else
        {
            dataStream = File.OpenRead(dataPath);
        }

        TarReader reader = new TarReader(dataStream);
        // Unpack your .tar file into your persistentDataPath.
        reader.ReadToEnd(Application.persistentDataPath);
        yield return null;

        Debug.Log("<color=green><b>Decompress complete!</b></color>");
    }

    /*
    *  HELPER METHODS
    */

    // Converts your audio data into a byte array.
    private static byte[] ConvertToBytes(float[] data, int channels)
    {
        float tot = 0;
        byte[] byteData = new byte[data.Length / channels * 2];
        for (int i = 0; i < data.Length / channels; i++)
        {
            float sum = 0;
            for (int j = 0; j < channels; j++)
            {
                sum += data[i * channels + j];
            }
            tot += sum * sum;
            short val = (short)(sum / channels * 20000); // volume
            byteData[2 * i] = (byte)(val & 0xff);
            byteData[2 * i + 1] = (byte)(val >> 8);
        }
        return byteData;
    }

    // Resets our text to reflect the keyphrase we need to speak.
    private IEnumerator WaitAndResetText()
    {
        yield return new WaitForSeconds(2);
        sampleText.text = $"Say - {keyphrase}";
    }
}
