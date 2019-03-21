using UnityEngine;
using System;
using Random = UnityEngine.Random;
using KModkit;
using System.Linq;
using System.Collections;

public class subscribeToPewdiepieScript : MonoBehaviour {

    public KMBombModule Module;
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable submitSelectable;
    public KMSelectable[] topRowSelectables, bottomRowSelectables;
    public TextMesh pewdiepieText, tseriesText, gapText;

    private static int _moduleIdCounter = 1;
    private int _moduleId;
    private bool solved;
    private int startingPewdiepie, startingTSeries;
    private int pewdiepieSubs, tseriesSubs;
    private int startingSubGap;
    private int[] numbers = { 0, 0, 0, 0, 0 };
    private int taps = 0;

    // Use this for initialization
    void Start () {
        _moduleId = _moduleIdCounter++;
        Module.OnActivate += Activate;

        GenerateModule();
    }

    void Activate()
    {
        submitSelectable.OnInteract += delegate ()
        {
            if (!solved)
                Submit();
            submitSelectable.AddInteractionPunch();
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, Module.transform);
            return false;
        };

        for (int i = 0; i < topRowSelectables.Length; i++)
        {
            int h = i;
            topRowSelectables[i].OnInteract += delegate ()
            {
                if (!solved)
                    ChangeNumber(h, 1);
                topRowSelectables[h].AddInteractionPunch();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
                return false;
            };
        }

        for (int i = 0; i < bottomRowSelectables.Length; i++)
        {
            int j = i;
            bottomRowSelectables[i].OnInteract += delegate ()
            {
                if (!solved)
                    ChangeNumber(j, -1);
                bottomRowSelectables[j].AddInteractionPunch();
                Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Module.transform);
                return false;
            };
        }
    }

    void GenerateModule()
    {
        startingPewdiepie = Random.Range(10000000, 100000000);
        startingTSeries = Random.Range(10000000, 100000000);
        startingSubGap = Math.Abs(startingPewdiepie - startingTSeries);

        pewdiepieText.text = startingPewdiepie.ToString();
        int length = pewdiepieText.text.Length;
        for (int i = 0; i < 8 - length; i++)
        {
            pewdiepieText.text = "0" + pewdiepieText.text;
        }

        tseriesText.text = startingTSeries.ToString();
        length = tseriesText.text.Length;
        for (int i = 0; i < 8 - length; i++)
        {
            tseriesText.text = "0" + tseriesText.text;
        }

        gapText.text = "00000";

        DebugMsg("PewDiePie starts with " + startingPewdiepie + " subscribers and T-Series starts with " + startingTSeries + " subscribers.");

        if (startingTSeries > startingPewdiepie) // rule 1
        {
            pewdiepieSubs = startingTSeries;
            tseriesSubs = startingPewdiepie;
        }

        else
        {
            pewdiepieSubs = startingPewdiepie;
            tseriesSubs = startingTSeries;
        }

        foreach (var module in Bomb.GetModuleNames()) // rule 2
        {
            if (module == "T-Words")
                tseriesSubs += 500;

            if (module == "Pie")
                pewdiepieSubs += 500;
        }

        pewdiepieSubs += Bomb.GetModuleNames().Count * 10; // rule 3

        if (Bomb.GetModuleNames().Contains("101 Dalmatians") && Bomb.GetModuleNames().Contains("Cooking")) // rule 4
            tseriesSubs -= Math.Abs(pewdiepieSubs - tseriesSubs);

        for (int i = 0; i < Bomb.GetBatteryCount(); i++) // rule 5
        {
            int originalSubs = pewdiepieSubs;
            pewdiepieSubs = (int)(pewdiepieSubs * .95);
            DebugMsg("The number " + originalSubs + " multiplied by .95 is " + pewdiepieSubs);
        }

        if (Bomb.GetSerialNumberLetters().Contains('T') || Bomb.GetSerialNumberLetters().Contains('S') || Bomb.GetSerialNumberLetters().Contains('E') || Bomb.GetSerialNumberLetters().Contains('R') || Bomb.GetSerialNumberLetters().Contains('I'))
            tseriesSubs = (int)(tseriesSubs * 1.5);

        DebugMsg("PewDiePie actually has " + pewdiepieSubs + " subscribers and T-Series actually has " + tseriesSubs + " subscribers.");
        if (tseriesSubs >= pewdiepieSubs)
            DebugMsg("You should submit 00000.");

        else
            DebugMsg("You should submit " + ((pewdiepieSubs - tseriesSubs) % 100000) + ".");
    }

    void Submit()
    {
        if (tseriesSubs >= pewdiepieSubs)
        {
            if (numbers[0] == 0 && numbers[1] == 0 && numbers[2] == 0 && numbers[3] == 0 && numbers[4] == 0)
            {
                StartCoroutine(Solve());
            }

            else
            {
                taps = 0;
                Module.HandleStrike();
            }
        }
        
        else
        {
            int gap = (pewdiepieSubs - tseriesSubs) % 100000;
            if (numbers[4] == gap % 10 && numbers[3] == gap / 10 % 10 && numbers[2] == gap / 100 % 10 && numbers[1] == gap / 1000 % 10 && numbers[0] == gap / 10000)
            {
                StartCoroutine(Solve());
            }

            else
            {
                taps = 0;
                Module.HandleStrike();
            }
        }
    }

    void ChangeNumber(int btnNumber, int amount)
    {
        numbers[btnNumber] += amount;
        if (numbers[btnNumber] == -1)
            numbers[btnNumber] = 9;
        if (numbers[btnNumber] == 10)
            numbers[btnNumber] = 0;

        gapText.text = numbers[0].ToString() + numbers[1].ToString() + numbers[2].ToString() + numbers[3].ToString() + numbers[4].ToString();
    }
    
    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Subscribe to Pewdiepie #{0}] {1}", _moduleId, msg);
    }

    IEnumerator Solve()
    {
        if (taps == 0)
            DebugMsg("You submitted " + gapText.text + ".");
        taps++;

        for (int i = 0; i < 50; i++)
        {
            gapText.text = Random.Range(0, 100000).ToString();
            int length = gapText.text.Length;
            for (int x = 0; x < 5 - length; x++)
            {
                gapText.text = "0" + gapText.text;
            }

            pewdiepieText.text = Random.Range(10000000, 100000000).ToString();
            length = pewdiepieText.text.Length;
            for (int x = 0; x < 8 - length; x++)
            {
                pewdiepieText.text = "0" + pewdiepieText.text;
            }

            tseriesText.text = Random.Range(10000000, 100000000).ToString();
            length = tseriesText.text.Length;
            for (int x = 0; x < 8 - length; x++)
            {
                tseriesText.text = "0" + tseriesText.text;
            }

            yield return new WaitForSeconds(.01f);
        }

        gapText.text = "done!";
        pewdiepieText.text = "!!COOL!!";
        tseriesText.text = "!!!GG!!!";

        Module.HandlePass();
        solved = true;
        DebugMsg("That was right!");
    }

    public string TwitchHelpMessage = "Use !{0} submit 12345 to submit 12345. (Replace 12345 with your answer.)";
    IEnumerator ProcessTwitchCommand(string cmd)
    {
        if (cmd.ToLowerInvariant().StartsWith("submit "))
        {
            for (int i = 0; i < 5; i++)
            {
                while (gapText.text[i] != cmd.Substring(7)[i])
                {
                    yield return null;
                    yield return new KMSelectable[] { topRowSelectables[i] };
                }
            }

            yield return null;
            yield return new KMSelectable[] { submitSelectable };
        }

        else
            yield break;
    }
}
