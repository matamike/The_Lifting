using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using TMPro;

public class StoryController : MonoBehaviour
{

    [SerializeField]
    private TextAsset[] inkJSONAsset;
    private Story story;
    //private Elevator elevator;
    private int currentFloor;
    int previousFLoor = -1;

    // UI Prefabs
    [SerializeField]
    private TextMeshProUGUI text0, text1, text2;
    [SerializeField]
    private Image bubble0, bubble1, bubble2;

    private int readingTime = 4; //the time in seconds until we read next line
    private const float waitTime = 0.05f; //the time in seconds until we show another character

    private String character0 = "Ferdinand";
    private String character1 = "Vanessa";
    private String character2 = "Claudia";
    private String sound0 = "GunSound";
    private String sound1 = "DoorSlam";

    private bool storyStarted = false;

    void Awake()
    {
        // Remove the default message
        //clearText();
        //elevator = GameObject.FindGameObjectWithTag("Elevator").GetComponent<Elevator>();
    }

    void Update()
    {
        //if (Input.GetKeyUp(KeyCode.E))
        //{
        //    if (!storyStarted)
        //    {
        //        Invoke("StartStory", 3.0f);
        //        storyStarted = true;
        //    }
        //}
    }

    public void PlayVignette(int floor)
    {
        currentFloor = floor;
        print(currentFloor + " : " + previousFLoor);
        if (storyStarted && (previousFLoor == currentFloor)) return;
        
        //print("Max vignettes" + inkJSONAsset.Length + " : " + "floor number" + currentFloor);

        //if (currentFloor >= inkJSONAsset.Length)
        //{
        //    currentFloor = inkJSONAsset.Length - 1;
        //}

        TextMeshProUGUI[] texts = GameObject.Find("Vignette" + currentFloor).GetComponentsInChildren<TextMeshProUGUI>();
        text0 = texts[0];
        text1 = texts[1];

        Image[] images = GameObject.Find("Vignette" + currentFloor).GetComponentsInChildren<Image>();
        List<Image> bubbles = new List<Image>();
        foreach (Image image in images)
        {
            if (image.gameObject.name == "TextBG")
            {
                bubbles.Add(image);
            }
        }
        bubble0 = bubbles[0];
        bubble1 = bubbles[1];

        story = new Story(inkJSONAsset[currentFloor].text);

        List<String> characters = story.globalTags;
        if (characters == null)
        {
            print("No global tags found");
            text0.text = "Error";
            text1.text = "Error indeed.";
            return;
        }
        character0 = characters[0];
        character1 = characters[1];

        if (characters.Count > 2)
        {
            text2 = texts[2];
            bubble2 = bubbles[2];
        }

        clearText();
        if (!storyStarted && previousFLoor != currentFloor)
        {
            print("STORY STARTED");
            clearText();
            Invoke("StartStory", 3.0f);
            storyStarted = true;
        }
    }

    public void ExitVignette(int floor)
    {
        currentFloor = floor;
        previousFLoor = currentFloor;
        //storyStarted = false;
    }

    // Creates a new Story object with the compiled story which we can then play!
    private void StartStory()
    {
        RefreshView();
    }

    // This is the main function called every time the story changes. It does a few things:
    // Destroys all the old content and choices.
    // Continues over all the lines of text, then displays all the choices. If there are no choices, the story is finished!
    public void RefreshView()
    {
        // Remove all the UI on screen
        clearText();

        //read next content and put it on the screen
        if (story.canContinue)
        {
            // Continue moves to next line and reads it
            string text = story.Continue();
            // This removes any white space from the text.
            //text = text.Trim();
            readingTime = (int)(text.Length * 0.1) + 1;
            //Find out which character is speaking

            List<string> currentTags = story.currentTags;

            // Check for which character
            int canvID = 99;
            if (currentTags.Contains(character0))
            { //Ferdinand is talking
                canvID = 0;
            }
            else if (currentTags.Contains(character1))
            { //Vanessa is talking
                canvID = 1;
            }
            else if (currentTags.Contains(character2))
            {
                canvID = 2;
            }

            // Check for sounds to play
            if (currentTags.Contains(sound0))
            {
                gameObject.GetComponent<SoundEmitter>().PlayClickSound(0);
            }
            else if (currentTags.Contains(sound1))
            {
                gameObject.GetComponent<SoundEmitter>().PlayClickSound(1);
            }

            // Check for Laura entering room
            // IF TIME MAKE CHARACTERS LEAVE/ENTER ROOM

            // Display the text on screen!
            CreateContentView(text, canvID);
            //Debug.Log("CanvID is: " + canvID);
            // If there are any choices, scream
            if (story.currentChoices.Count > 0)
            {
                Debug.Log("Error, please do not include choices in the game");
            }

            // Check if there is a pause
            string pause = currentTags.FirstOrDefault(tag => tag.Contains("Pause"));

            if (pause != default)
            {
                string[] sub = pause.Split(':');
                float pauseTime = float.Parse(sub[1]);
                Invoke("clearText", readingTime);
                Invoke("RefreshView", readingTime + pauseTime);
            }
            else
            {
                Invoke("RefreshView", readingTime);
            }
        }
        else
        {
            previousFLoor = currentFloor;
            storyStarted = false;
        }
    }

    // Creates a text showing the current line
    // CanvID is the canvas we want it displayed on
    void CreateContentView(string text, int canvID)
    {
        switch (canvID)
        {
            case 0:
                text0.text = text;
                bubble0.enabled = true;
                StartCoroutine(revealText(text0));
                break;
            case 1:
                text1.text = text;
                bubble1.enabled = true;
                StartCoroutine(revealText(text1));
                break;
            case 2:
                text2.text = text;
                bubble2.enabled = true;
                StartCoroutine(revealText(text2));
                break;
        }
    }


    //reveal the text little at a time, if we are showing all text then wait for readingTime and start a new line
    IEnumerator revealText(TextMeshProUGUI textField)
    {
        int shownAmount = 0;
        textField.maxVisibleCharacters = shownAmount;
        //if we are not showing all the text show another character
        while (textField.maxVisibleCharacters < textField.text.Length)
        {
            shownAmount++;
            textField.maxVisibleCharacters = shownAmount;

            yield return new WaitForSeconds(waitTime);
        }

        //should (?) stop the coroutine
        yield break;
    }

    void clearText()
    {
        text0.text = "";
        bubble0.enabled = false;
        text1.text = "";
        bubble1.enabled = false;
        if (text2 != null) text2.text = "";
        if (bubble2 != null) bubble2.enabled = false;
    }
}
