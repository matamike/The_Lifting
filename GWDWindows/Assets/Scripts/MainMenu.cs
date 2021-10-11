using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour{
    public float resetTimeInSeconds = 26f;
    public float yAxisVelocityModifier = 0.1f;
    private float transitionTimeStep = 10f / 100f;
    private float transitionTime;

    GameObject scrollRectGO;
    ScrollRect scrollRect;
    float timedElapsed;
    Vector3 initialTextPos;

    void Start(){
        Time.timeScale = 1.0f; //resets timescale to normal after returning from pause menu.
        transitionTime = transitionTimeStep;
        timedElapsed = 0.0f;
        scrollRectGO = GameObject.FindGameObjectWithTag("AutoScroll");
        scrollRect=scrollRectGO.GetComponent<ScrollRect>();
        initialTextPos = scrollRect.GetComponentInChildren<Text>().gameObject.GetComponent<RectTransform>().anchoredPosition3D;
    }

    void Update(){
        Debug.Log("Delta Time: " +timedElapsed);
        //Fade In/Out between texts.
        if (GameObject.Find("Credits") is null){
            timedElapsed += Time.deltaTime;
            scrollRect.velocity += new Vector2(0f, yAxisVelocityModifier);
            scrollRect.content.Translate(Vector3.up * scrollRect.velocity.y, Space.Self);
            scrollRect.GetComponentInChildren<Text>().color = new Color(scrollRect.GetComponentInChildren<Text>().color.r,
                 scrollRect.GetComponentInChildren<Text>().color.g, scrollRect.GetComponentInChildren<Text>().color.b, Mathf.Lerp(scrollRect.GetComponentInChildren<Text>().color.a, 1.0f, transitionTime));
            transitionTime += transitionTimeStep;
        }
        else{
            scrollRect.GetComponentInChildren<Text>().color = new Color(scrollRect.GetComponentInChildren<Text>().color.r,
                 scrollRect.GetComponentInChildren<Text>().color.g, scrollRect.GetComponentInChildren<Text>().color.b, Mathf.Lerp(scrollRect.GetComponentInChildren<Text>().color.a, 0.0f, transitionTime));
            transitionTime += transitionTimeStep;
        }

        //Reset Text Position.
        if (timedElapsed > (resetTimeInSeconds/(yAxisVelocityModifier + 0.01f))){
            scrollRect.GetComponentInChildren<Text>().gameObject.GetComponent<RectTransform>().anchoredPosition3D = new Vector3(initialTextPos.x, initialTextPos.y, initialTextPos.z);
            timedElapsed = 0.0f;
        }
        transitionTime = transitionTimeStep;
    }

    public void PlayGame(){
        SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
    }

    public void QuitGame(){
        Application.Quit();
    }
}
