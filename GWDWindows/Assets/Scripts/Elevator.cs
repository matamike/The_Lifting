using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Elevator : MonoBehaviour{
    public Transform[] elevatorLevels;
    public GameObject upButton, downButton;

    [Range(0.1f,1.0f)]
    public float elevatorSpeed = 1f;
    private float speed;

    [Range(0.001f,0.005f)]
    public float manualSpeed = 0.002f;

    public bool moveFloorToFloor = true;
    public bool manualMovement = false;

    bool isMoving = false; //flag for moving
    int currentLevel; //target level
    int directionLevel = 0; //0 - no movement, 1 - moving up, -1 -moving down
    int min, max; //setting minimum and maximum levels.
    float floorsDistanceBetween = 0f; //placeholder for distance between floors
    Vector3 initPos; //starting position
    bool calculateNext = false; //flag for iterating elevator levels
    float nextHeightTarget = 0f; //calc for next level distance
    GameObject playerFound; //player

    void Start(){
        speed = elevatorSpeed / 100f;
        initPos = transform.position; //gets the starting position that the user decides
        playerFound = GameObject.Find("Player");
        if (elevatorLevels.Length > 1){
            min = 0; //ground floor.
            max = elevatorLevels.Length - 1; //size -1 equals last floor.
            currentLevel = min; //set the starting level of the elevator as the ground level.
            floorsDistanceBetween = Vector3.Distance(elevatorLevels[0].transform.position, elevatorLevels[1].transform.position); //init default position.
            //Note every floor has to have the same distance with each other.                           
        }
        else print("Floor levels references are missing!");
    }

    void Update(){
        if (isMoving){
            //Move Floor to Floor
            if (moveFloorToFloor){

                //Moving Up or break
                if (directionLevel == 1){

                    if (!calculateNext){
                        nextHeightTarget = transform.position.y + floorsDistanceBetween;
                        calculateNext = true;
                    }

                    if (currentLevel <= max){
                        Vector3 tempPos = Vector3.LerpUnclamped(transform.position, new Vector3(transform.position.x, nextHeightTarget, transform.position.z), speed);
                        speed += (elevatorSpeed / 100f);
                        if (tempPos != transform.position) transform.position = tempPos;
                        else isMoving = false;
                    }
                    else isMoving = false;
                }

                //Moving Down or break
                if (directionLevel == -1){

                    if (!calculateNext){
                        nextHeightTarget = transform.position.y - floorsDistanceBetween;
                        calculateNext = true;
                    }

                    if (currentLevel >= min){
                        Vector3 tempPos = Vector3.LerpUnclamped(transform.position, new Vector3(transform.position.x, nextHeightTarget, transform.position.z), speed);
                        speed += (elevatorSpeed / 100f);
                        if (tempPos != transform.position) transform.position = tempPos;
                        else isMoving = false;
                    }
                    else isMoving = false;
                }
            }

            //Move Manually
            if (manualMovement){

                //Moving Up or break
                if (directionLevel == 1){
                    float dist = Vector3.Distance(initPos, elevatorLevels[min].transform.position); //max distance based on starting location.
                    if (transform.position.y <= elevatorLevels[max].transform.position.y+dist-floorsDistanceBetween) transform.position += new Vector3(0f, manualSpeed, 0f);
                }

                //Moving Down or break
                if (directionLevel == -1){
                    if (transform.position.y >= initPos.y) transform.position -= new Vector3(0f,manualSpeed, 0f);
                }
            }
        }
        else{
            playerFound.GetComponent<FPSController>().ResetOption(isMoving); //stop movement.
            nextHeightTarget = 0f; //used for floor to floor 
            calculateNext = false; //used for floor to floor
            speed = elevatorSpeed / 100f;
        }
    }

    //User Defined Functions

    public void SetStatus(Status stats){
        if (moveFloorToFloor){
            if ((currentLevel + stats.myNextFloor) >= min && (currentLevel + stats.myNextFloor) <= max){
                isMoving = stats.myStatus; //flag to move or not
                floorsDistanceBetween = Vector3.Distance(elevatorLevels[currentLevel].transform.position, elevatorLevels[currentLevel + stats.myNextFloor].transform.position);
                currentLevel += stats.myNextFloor;//target to move array index.
                directionLevel = stats.myNextFloor; //direction of movement.
            }
            Debug.Log("Current Level: " + currentLevel);
        }

        if (manualMovement){
            isMoving = stats.myStatus;
            //no need for level reference.
            directionLevel = stats.myNextFloor;
        }
    }

    public bool GetStatus(){
        return isMoving; //return the current status to the player.
    }

    public int GetCurrentLevel(){
        return currentLevel; //return the level elevator is moving to or is located currently.
    }

    public int GetElevatorMovementType(){
        int typeOfMovement = -1; //No movement
        if(moveFloorToFloor) typeOfMovement = 0; //Moving Floor to Floor
        if (manualMovement) typeOfMovement = 1; //Moving Manually;
        return typeOfMovement;
    }


    //Unity Functions
    void OnCollisionEnter(Collision collision){
        //if (collision.gameObject.name == "Player") Debug.Log(" Cleaner detected. ");
    }

    void OnCollisionStay(Collision collision){
        //if (collision.gameObject.name == "Player") Debug.Log(" Select UP/DOWN. ");
    }

    void OnCollisionExit(Collision collision){
        //if (collision.gameObject.name == "Player") Debug.Log(" Cleaner Exited. ");
    }
}
