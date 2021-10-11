using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    public GameObject[] pauseObjects;
    public GameObject[] notPauseObjects;
    public AudioManager audioM;

    // Use this for initialization
    void Start()
    {
        Time.timeScale = 1f;
        pauseObjects = GameObject.FindGameObjectsWithTag("ShowOnPause");
        //notPauseObjects = GameObject.FindGameObjectsWithTag("DontShowOnPause"); //if not needed please remove
        notPauseObjects = null;
        hidePaused();
    }

    void Update(){
        // Pause / Resume
        if (Input.GetButtonDown("Cancel")) PauseControl();
    }

    //Pause Controller
    public void PauseControl()
    {
        if (Time.timeScale == 1)
        {
            audioM.ChangeSnapshot("Pause");
            Time.timeScale = 0;
            if (pauseObjects != null) showPaused();
            if (notPauseObjects != null) hideUnpaused();
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
        else if (Time.timeScale == 0)
        {
            audioM.ChangeSnapshot("Normal");
            Time.timeScale = 1;
            if (pauseObjects != null) hidePaused();
            if (notPauseObjects != null) showUnpaused();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void TogglePause() {
        if(Time.timeScale == 1) {
            Time.timeScale = 0;
        } else if(Time.timeScale == 0) {
            Time.timeScale = 1;
        }
    }

    //shows objects with ShowOnPause tag
    public void showPaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(true);
        }
    }

    //hides objects with ShowOnPause tag
    public void hidePaused()
    {
        foreach (GameObject g in pauseObjects)
        {
            g.SetActive(false);
        }
    }

    public void showUnpaused()
    {
        foreach (GameObject g in notPauseObjects)
        {
            g.SetActive(true);
        }
    }

    public void hideUnpaused()
    {
        foreach (GameObject g in notPauseObjects)
        {
            g.SetActive(false);
        }
    }

    public void Pause() {
        Time.timeScale = 0;
    }

    public void UnPause() {
        Time.timeScale = 1;
    }
}
