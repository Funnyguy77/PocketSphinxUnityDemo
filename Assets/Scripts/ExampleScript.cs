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
 * Note: when using the standard 'en-us' language model, it doesn't seem to pickup complicated sentences well. 
 * If you wish to recognize longer strings, please look at creating your own acoustic model / language model. 
 * 
 * More information on CMUSphinx / PocketSphinx can be found at -> https://cmusphinx.github.io/wiki/tutorial/
 */
[RequireComponent(typeof(AudioSource))]
public class ExampleScript : MonoBehaviour
{
    public Text txt; // Text game-object that displays if a keyword was recognized. 
    public Text micTxt; // Text game-object that displays the current microphone in use.
    private Decoder d; // Decoder that's actually interpreting our speech.
    private string LANG = "en-us"; // The language model you wish to use. (Name of your .tar file within StreamingAssets)
    private string KEYPHRASE = "test one"; // The actual keyphrase our decoder is looking to recognize. Note that this is case-sensitive.

    private bool keyphraseDetected = false;

    // Use this for initialization. 
    private IEnumerator Start()
    {
        // Wait until we have a microphone connected before attempting to initialize. 
        while (Microphone.devices.Length <= 0)
            yield return null;

       StartCoroutine(BeginSetup());
    }

    // Only used to update our text.
    private void Update()
    {
        // If we detect a keyphrase, update our UI to reflect this.
        if (keyphraseDetected)
        {
            txt.text = "Keyphrase detected!";
            StartCoroutine(WaitAndResetText());
            keyphraseDetected = false;
        }
    }

    // Begins the setup process for PocketSphinx.
    private IEnumerator BeginSetup()
    {
        // Update our UI text.
        txt.text = $"Say - {KEYPHRASE}";
        micTxt.text = $"Microphone - {Microphone.devices[0]}";
        // Waits to decompress our language model before accessing any data from it.
        yield return Decompress();
        // Setup our Decoder.
        SetupDecoder();
        // Start listening to our microphone.
        SetupMicrophone();
    }

    // This is the actual method that is called when we speak into the microphone. 
    // Docs -> https://docs.unity3d.com/ScriptReference/MonoBehaviour.OnAudioFilterRead.html
    private void OnAudioFilterRead(float[] data, int channels)
    {
        // Insurance.
        if (d == null)
            return;

        // Convert audio data into byte data.
        byte[] byteData = ConvertToBytes(data, channels);
        // Process the raw data with our Decoder.
        d.ProcessRaw(byteData, byteData.Length, false, false);

        // Check if we recognize a keyword.
        if(d.Hyp() != null)
        {
            Debug.Log("<b><color=yellow>Keyphrase detected!</color></b>");
            keyphraseDetected = true;
            d.EndUtt();
            d.StartUtt();
        }
    }

    // Locates and decompresses your language model.
    private IEnumerator Decompress()
    {
        Debug.Log("<color=red>Decompressing language model...</color>");
        // Locate your language model.
        string dataPath = Path.Combine(Application.streamingAssetsPath, LANG + ".tar");
        Stream dataStream;

        // Download your language model if you wish to store it remotely.
        if (dataPath.Contains("://"))
        {
            UnityWebRequest www = new UnityWebRequest(dataPath);
            yield return www;
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

    private void SetupDecoder()
    {
        Debug.Log("<color=red>Initializing decoder...</color>");
        // Create a new configuration for our decoder.
        Config c = Decoder.DefaultConfig();
        // Find our newly decompressed language model.
        string speechDataPath = Path.Combine(Application.persistentDataPath, LANG);
        // Find our newly decompressed dictionary. Note: You may need to change the dictionary name if you create a custom dictionary.
        string dictPath = Path.Combine(speechDataPath, "dictionary");
        // Creates a path to store log files.
        string logPath = Path.Combine(Application.persistentDataPath, "ps.log");

        // Tell our decoder what language model we wish to use.
        c.SetString("-hmm", speechDataPath);
        // Tell our decoder what dictionary to use.
        c.SetString("-dict", dictPath);
        // Tell our decoder what phrase to look for.
        c.SetString("-keyphrase", KEYPHRASE);

        /* How accurate our decoder will be. For shorter keyphrases you can use smaller thresholds like 1e-1, 
         * for longer keyphrases the threshold must be bigger, up to 1e-50. 
         * If your keyphrase is very long – larger than 10 syllables – it is recommended to split it 
         * and spot for parts separately. 
         */
        c.SetFloat("-kws_threshold", 1e-50);

        // These two lines enable and save raw data to a log for debugging. Feel free to comment these lines if you have no need for logs.
        c.SetString("-logfn", logPath);
        c.SetString ("-rawlogdir", Application.persistentDataPath);

        // Create a new Decoder with our configuration.
        d = new Decoder(c);

        /* This section of code deals with adding a custom language model or .lm file to PocketSphinx. 
         * Without modification to this project, you will not be able to use a custom LM. Leave commented unless you create a custom language model.
         * For more information on how to create your own language model see -> https://cmusphinx.github.io/wiki/tutoriallm/
         */
        //string lmPath = Path.Combine();
        //d.SetLmFile("lm", lmPath);

        // IF YOU'RE USING A CUSTOM LM THIS -> Sets the actual keyphrase to look for.
        //d.SetKeyphrase("kws", KEYPHRASE);
        // IF YOU'RE USING A CUSTOM LM THIS -> Sets our decoders search mode. (kws = keyword search, lm = language model)
        //d.SetSearch("kws");

        // Starts the decoder.
        d.StartUtt();
        Debug.Log("<color=green><b>Decoder initialized!</b></color>");
    }

    // Enables listening to audio coming from a users microphone.
    private void SetupMicrophone()
    {
        Debug.Log("<color=red>Starting microphone...</color>");
        AudioSource source = GetComponent<AudioSource>();
        // Starts listening to our microphone.
        source.clip = Microphone.Start(null, true, 1, 44100);
        source.Play();
        Debug.Log("<color=green><b>Microphone started!</b></color>");
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
        txt.text = $"Say - {KEYPHRASE}";
    }
}
