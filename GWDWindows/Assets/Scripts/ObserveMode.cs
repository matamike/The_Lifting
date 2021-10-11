using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ObserveMode : MonoBehaviour{
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private float sensitivityX = 1.4f;
    private float sensitivityY = 1.4f;
    private GameObject[] toolTipLoader;
    private Image toolTipLoaderCycle;
    private Rigidbody _rigidbody;
    private Camera _playerCam;
    private bool hasMovedToLoc = false;
    [Range(1,10)]
    public float zoomThreshold = 3f;
    [Range(0, 90)]
    public float rotationXAngleLimits=20f;
    [Range(0, 90)]
    public float rotationYAngleLimits = 30f;
    [Range(0.01f, 1.0f)] //0.01% - 100% modifier for speed (smaller => slower)
    public float step = 0.10f;
    [Range(0.01f,1.0f)] //0.01% - 100% modifier for Fade In (which percentage during loading allows the text to appear)
    public float FadeInToolTipTextThreshold = 0.98f;
    [Range(0.01f,1.0f)] //0.01% - 100% modifier for Fade In (which percentage during loading allows the text to disappear)
    public float FadeOutToolTipTextThreshold = 0.25f;
    private GameObject toolTipFocusManager,toolTipFocusName,toolTipFocusDescription;
    private GameObject squegee;

    void Start(){
        squegee = GameObject.Find("Squegee");
        toolTipLoader = GameObject.FindGameObjectsWithTag("ZoomInOut");
        toolTipFocusManager = GameObject.FindGameObjectWithTag("ToolTipFocusManager");
        toolTipFocusName = GameObject.FindGameObjectWithTag("ToolTipFocusName");
        toolTipFocusDescription = GameObject.FindGameObjectWithTag("ToolTipFocusDescription");

        for (int i = 0; i < toolTipLoader.Length; i++){
            if (toolTipLoader[i].name == "ImageFill"){
                toolTipLoaderCycle = toolTipLoader[i].GetComponent<Image>();
                break;
            }
        }

        _rigidbody = this.GetComponent<Rigidbody>();
        _playerCam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update(){

       if(Input.GetMouseButton(1) && this.gameObject.GetComponent<FPSController>().GetPlayerState() == FPSController.PlayerState.Cleaning){

            if (this.gameObject.GetComponent<FPSController>().isActiveAndEnabled){
                this.gameObject.GetComponent<FPSController>().uxZoomInOutPrompt.SetActive(false);
                this.gameObject.GetComponent<FPSController>().enabled = false;
            }
            if (squegee.GetComponent<MeshRenderer>().enabled){
                squegee.GetComponent<MeshRenderer>().enabled = false;
                squegee.GetComponent<Collider>().enabled = false;

            }
            MoveToTargetLocation(true);
            Observing();
            TooltipLoader(true);
        }
        else{
            if (!this.gameObject.GetComponent<FPSController>().isActiveAndEnabled){
                GameObject[] windows = GameObject.FindGameObjectsWithTag("Window");
                for (int i = 0; i < windows.Length; i++) windows[i].GetComponent<Collider>().enabled = true;
                TooltipLoader(false);
                Tooltip(null); //disable tool tip text
                MoveToDefaultLocation(false);//Return Camera to previous position.
                this.gameObject.GetComponent<FPSController>().enabled = true; //restore initial script to run.
            }
            if (!squegee.GetComponent<MeshRenderer>().enabled){
                squegee.GetComponent<MeshRenderer>().enabled = true;
                squegee.GetComponent<Collider>().enabled = true;
            }
        }
    }


    //Zoom In/Out Functions - Michael Edit
    private void Observing(){
        float smoothS = Mathf.SmoothStep(0.0f, 2.0f, 10f);

        rotationX += transform.eulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX * smoothS;
        rotationY += transform.eulerAngles.y + Input.GetAxis("Mouse Y") * sensitivityY * smoothS;

        rotationX = Mathf.Clamp(rotationX, -rotationYAngleLimits, rotationYAngleLimits);
        rotationY = Mathf.Clamp(rotationY, -rotationXAngleLimits, rotationXAngleLimits);
        _playerCam.transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
    }

    private void MoveToTargetLocation(bool moveStatus){
        if (moveStatus != hasMovedToLoc) {
            Camera.main.transform.position += new Vector3(0f, 0f,zoomThreshold);
            hasMovedToLoc = moveStatus;
        }
    }

    private void MoveToDefaultLocation(bool moveStatus){
        if (moveStatus != hasMovedToLoc) {
            Camera.main.transform.position -= new Vector3(0f, 0f, zoomThreshold);
            Camera.main.transform.eulerAngles = Vector3.zero;
            hasMovedToLoc = moveStatus;
        }
    }

    private void Tooltip(GameObject obj){
        if (obj != null){
            //activate
            toolTipFocusManager.GetComponent<Canvas>().enabled = true;
            toolTipFocusName.GetComponent<Text>().text = obj.GetComponent<TooltipText>().GetObjName();
            toolTipFocusDescription.GetComponent<Text>().text = obj.GetComponent<TooltipText>().GetObjDescription();
        }
        else{
            //deactivate
            toolTipFocusName.GetComponent<Text>().text = "";
            toolTipFocusDescription.GetComponent<Text>().text = "";
            toolTipFocusManager.GetComponent<Canvas>().enabled = false;
        }
    }

    private void TooltipLoader(bool state){
        //Enable Marker and Loader UI
        for (int i = 0; i < toolTipLoader.Length; i++) toolTipLoader[i].GetComponent<Image>().enabled = state;
        
        //Disable Window Colliders
        GameObject[] windows = GameObject.FindGameObjectsWithTag("Window");
        for (int i = 0; i < windows.Length; i++) windows[i].GetComponent<Collider>().enabled = !state;
       

        // Activate Loader
        if (state){
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.rotation * transform.forward * 10f, out RaycastHit hitInfo)){
               
                if (hitInfo.collider.tag == "TooltipFocus" && toolTipLoaderCycle.fillAmount <= 1f){
                    toolTipLoaderCycle.fillAmount = Mathf.Lerp(toolTipLoaderCycle.fillAmount, 1, step);
                    if (toolTipLoaderCycle.fillAmount >= FadeInToolTipTextThreshold) Tooltip(hitInfo.collider.gameObject); //enable tooltip text
                }
                else if (toolTipLoaderCycle.fillAmount > 0f && toolTipLoaderCycle.fillAmount <= 1f){
                    toolTipLoaderCycle.fillAmount = Mathf.Lerp(toolTipLoaderCycle.fillAmount, 0, step);
                    if (toolTipLoaderCycle.fillAmount <= FadeOutToolTipTextThreshold) Tooltip(null); //disable tooltip text
                }
            }
            else{ //Scenario where raycast hits nothing.
                if (toolTipLoaderCycle.fillAmount > 0 && toolTipLoaderCycle.fillAmount <= 1f){
                    toolTipLoaderCycle.fillAmount = Mathf.Lerp(toolTipLoaderCycle.fillAmount, 0, step);
                    if (toolTipLoaderCycle.fillAmount <= FadeOutToolTipTextThreshold) Tooltip(null); //disable text
                }
            }
        }
    }
}
