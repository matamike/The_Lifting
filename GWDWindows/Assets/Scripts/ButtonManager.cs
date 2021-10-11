using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonManager : MonoBehaviour{
    public GameObject[] buttonList;
    //public bool colState = true; // test simulation flag- can be removed.
    List<Collider> buttonListCol;
    
    void Start(){
        //List for storing 
        buttonListCol = new List<Collider>();
        
        //pass each GO collider into the list of collider.
        foreach(GameObject button in buttonList){
            buttonListCol.Add(button.GetComponent<Collider>());
        }
    }

 //update test simulation only- can be removed.
 //void LateUpdate(){
 //       SetButtonsActive(colState);
 //   }
  

    //handles activity of buttons. (External Calls Only)
    public void SetButtonsActive(bool state) {
        // disable/enable buttons
        for (int i = 0; i < buttonListCol.Count; i++) buttonListCol[i].enabled = state;
    }
}
