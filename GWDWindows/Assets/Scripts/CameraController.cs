using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour{
    public Transform[] targets; //make it one target for the purpose of this project instead of array
    public bool firstPerson = false;
    public float followSpeed = 8f;
    private Vector3 cameraDirection;
    private Vector3 cameraPosition;

    void Update(){
        //FP CAMERA CONTROLLER
        if (firstPerson){
           if (targets[0] != null) foreach (Transform tar in targets) FirstPersonCameraHandler(tar);
        }
    }

    void FirstPersonCameraHandler(Transform target){
        if (target.gameObject.GetComponent<PlayerController>().GetMoveStatus()){
            float rotY = Input.GetAxis("Mouse X");
            float rotX = Input.GetAxis("Mouse Y");

            //Mouse Rotation X,Y Axis (use rotX for inverted  mouse X Axis).
            Vector3 rotateVal = new Vector3(-rotX, rotY, 0);
            float smoothS = Mathf.SmoothStep(0.0f, 2.0f, 10f);
            transform.eulerAngles += rotateVal * smoothS;

            //Move smoothly to the target.
            Vector3 tempPosFinal = Vector3.Slerp(transform.position, target.transform.position, followSpeed * Time.deltaTime);
            transform.position = tempPosFinal;

            //Set Camera Direction and pass the direction to player for relative movement //lock direction
            cameraDirection = new Vector3(Mathf.Sin(transform.eulerAngles.y * Mathf.Deg2Rad), 0f, Mathf.Cos(transform.eulerAngles.y * Mathf.Deg2Rad));
            target.GetComponent<PlayerController>().SetDirection(cameraDirection);
            cameraPosition=this.gameObject.transform.position;
        }
        else transform.position = target.transform.position;
    }

    public Vector3 GetCameraDirection() {
        return cameraDirection;
    }

    public Vector3 GetCameraPosition(){
        return cameraPosition;
    }
}
