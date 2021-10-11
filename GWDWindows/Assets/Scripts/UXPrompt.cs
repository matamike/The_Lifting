using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UXPrompt : MonoBehaviour{
    public Image[] imagesOfInterest; //Placeholder for UX Prompt images of the UI.
    private float fadeSpeed= 1f; //Calculator Holder
    [Range(1f, 100f)]
    public float step = 1f; //percentage for step (1%-100%)
    GameObject objOfInterest; //Raycasted Object of Interest.
    bool actionInProgress = false; //flag for ongoing action.
    public static UXPrompt _instance;
    public static UXPrompt Instance { get { return _instance; } }

     void Awake(){
        if(_instance !=null && _instance != this) Destroy(this.gameObject);
        else _instance = this;        
    }

    void Start(){
        objOfInterest = null;
        //disable text ui upon beginning.
        foreach(Image img in imagesOfInterest) img.GetComponent<Image>().color = new Color32(255, 255, 255, 0);
    }

    void Update(){
        ActivateButtonPrompt(); //Pop up Button Prompt when needed.
    }

    //User defined functions

    private void ActivateButtonPrompt(){
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.rotation * Vector3.forward, out RaycastHit hit, 10f) && !actionInProgress){
            objOfInterest = hit.collider.gameObject;

            foreach (Image img in imagesOfInterest){
                if (objOfInterest.name == img.gameObject.transform .parent.parent.parent.name){
                    fadeSpeed = Mathf.Lerp(fadeSpeed, 255, step / 100f);
                    img.GetComponent<Image>().color = new Color32(255, 255, 255, (byte)fadeSpeed);
                }
                else{
                    if (img.GetComponent<Image>().color.a > 0){
                        fadeSpeed = img.GetComponent<Image>().color.a;
                        img.GetComponent<Image>().color = new Color32(255, 255, 255, (byte)Mathf.Lerp(fadeSpeed, 0, step / 100f));    
                    }
                }
            }
        }
        else{
            foreach (Image img in imagesOfInterest){
                if (img.GetComponent<Image>().color.a > 0){
                    fadeSpeed = Mathf.Lerp(fadeSpeed, 0, step / 100f);
                    img.GetComponent<Image>().color = new Color32(255, 255, 255, (byte)fadeSpeed);  
                }
            }  
        }
    }

    public void ActionEnabled(bool state){
        actionInProgress = state;
    }
}
