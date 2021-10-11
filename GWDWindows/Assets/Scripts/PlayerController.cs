using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Status Struct to alert Elevator.
public struct Status{
    public bool myStatus;
    public int myNextFloor;

    public Status(bool sStatus, int nNextFoor){
        this.myStatus = sStatus;
        this.myNextFloor = nNextFoor;
    }
}


public class PlayerController : MonoBehaviour{
    
    public float movementSpeed = 4f;
    public float jumpSpeed = 4f;

    
    Material buttonMat;
    GameObject buttonGO;
    bool isRayEnabled = false;
    bool isOptionSelected = false; //for floor to floor movement
    bool isManualMove = false; //for manual movement
    bool movementState = true; //is Able to move
    Status stat = new Status(false, 0); //status data
    Vector3 playerDir; //player direction

    void Update(){
        //Player Movement
        PlayerMovement(isManualMove);

        //Activated when elevator trigger gets activated.
        if (isRayEnabled){
            ElevatorAction(); //Checks elevator method movement and applies behavior.

            //Floor to Floor Activation
            if (isOptionSelected) ActivateFloorToFloorMove();

            //Manual Movement Activation
            if (isManualMove) {
                ActivateManualMove(true);
                SetMoveStatus(false);
            }
            else{
                ActivateManualMove(false);
                SetMoveStatus(true);
            }
        }        
    }

    //User Defined functions
    public void ResetOption(bool flag){
        if(isOptionSelected==true) isOptionSelected = flag;
        if (isManualMove == true) isManualMove = flag;
    }

    public void SetDirection(Vector3 dir){
        playerDir = dir;
    }

    public bool GetMoveStatus(){
        return movementState;
    }

    void SetMoveStatus(bool move){
        movementState = move;
    }

    void PlayerMovement(bool state){
        if (!state){
            //PLAYER CONTROLLER FOR FP CAMERA
            if (Input.GetKey(KeyCode.W)) transform.position += playerDir.normalized * movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.A)) transform.position += Vector3.Cross(playerDir, Vector3.up).normalized * movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.S)) transform.position += -playerDir.normalized * movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.D)) transform.position += Vector3.Cross(playerDir, Vector3.down).normalized * movementSpeed * Time.deltaTime;

            if (Input.GetKey(KeyCode.Space)) transform.position += Vector3.up * jumpSpeed * Time.deltaTime;
        }
    }

    void ActivateFloorToFloorMove(){
        if (buttonGO != null && buttonGO.GetComponentInParent<Elevator>().GetElevatorMovementType() == 0)
        {
            if (buttonGO.name == "UpButton") stat = new Status(true, 1);
            if (buttonGO.name == "DownButton") stat = new Status(true, -1);

            buttonGO.SendMessageUpwards("SetStatus", stat); //signal elevator to move.
            buttonGO = null; //delete reference to button
        }
    }

    void ActivateManualMove(bool manualMove){
        if (buttonGO != null && buttonGO.GetComponentInParent<Elevator>().GetElevatorMovementType() == 1){
            if (buttonGO.name == "UpButton") stat = new Status(manualMove, 1);
            if (buttonGO.name == "DownButton") stat = new Status(manualMove, -1);

            buttonGO.SendMessageUpwards("SetStatus", stat); //signal elevator to move.
            buttonGO = null; //delete reference to button
        }
    }

    void ElevatorAction(){
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.rotation * Vector3.forward, out RaycastHit hit, 5f)){
            if ((hit.collider.gameObject.name == "UpButton" || hit.collider.gameObject.name == "DownButton") && !isOptionSelected){
                buttonGO = hit.collider.gameObject;
                buttonMat = hit.collider.gameObject.GetComponent<Renderer>().material;
                buttonMat.EnableKeyword("_EMISSION");

                //Single Touch for Floor to Floor
                if (buttonGO.GetComponentInParent<Elevator>().GetElevatorMovementType() == 0){
                    if (Input.GetMouseButtonDown(0)) isOptionSelected = !isOptionSelected;
                }

                //Continuous Touch for Manual Movement
                if (buttonGO.GetComponentInParent<Elevator>().GetElevatorMovementType() == 1){
                    if (Input.GetMouseButton(0)) isManualMove = true;
                    else isManualMove = false;
                }
            }
            else if (buttonMat != null && !isOptionSelected) buttonMat.DisableKeyword("_EMISSION");
        }
        else if (buttonMat != null && !isOptionSelected) buttonMat.DisableKeyword("_EMISSION");
    }

    //Unity Functions
    void OnCollisionEnter(Collision collision){
        if (collision.gameObject.tag == "Elevator") isRayEnabled = true;
        print(isRayEnabled);
    }

    void OnCollisionStay(Collision collision){
        if (collision.gameObject.tag == "Elevator") isRayEnabled = true;
        print(isRayEnabled);
    }

    void OnCollisionExit(Collision collision){
        if (collision.gameObject.tag == "Elevator") isRayEnabled = false;
        print(isRayEnabled);
    }
}
