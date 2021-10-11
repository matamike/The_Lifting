using System;
using System.Collections;
using UnityEngine;

// LYHNE EDIT
//Status Struct to alert Elevator.
//public struct Status
//{
//    public bool myStatus;
//    public int myNextFloor;

//    public Status(bool sStatus, int nNextFoor)
//    {
//        this.myStatus = sStatus;
//        this.myNextFloor = nNextFoor;
//    }
//}

public class FPSController : MonoBehaviour
{
    [Range(1, 10)]
    public float sensitivityX;
    [Range(1, 10)]
    public float sensitivityY;
    public float maxCamAngle;
    [Range(1, 40)]
    public float movementSpeed;
    [Range(1, 10)]
    public float squeegeeSensitivityX;
    [Range(1, 10)]
    public float squeegeeSensitivityY;
    public Transform[] windowTarget;
    public AnimationCurve stateTransition;

    [System.Serializable]
    public enum PlayerState { Looking, Cleaning }

    private PlayerState state = PlayerState.Looking;
    private Squeegee _squeegee;
    private Paint _window;
    private Rigidbody _rigidbody;
    [SerializeField]
    private float rotationY;
    private bool transitionInProgress = false; // This is used to disable squeegee while player is in transition towards the window
    private Coroutine transitionRoutine;
    private Camera _playerCam;
    private Vector3 _playerCamDirection;

    // LYHNE EDIT - From elevator playercontrol
    Material buttonMat;
    GameObject buttonGO;
    public GameObject uxZoomInOutPrompt;
    bool readyToClean = false;
    bool isRayEnabled = false;
    bool isOptionSelected = false; //for floor to floor movement
    bool isManualMove = false; //for manual movement
    bool movementState = true; //is Able to move
    int currentFloor = 0; // SHOULD GET THIS FROM ELEVATOR
    Status stat = new Status(false, 0); //status data

    void Awake() {
        uxZoomInOutPrompt = GameObject.FindGameObjectWithTag("UXZoomPrompt");
        _squeegee = FindObjectOfType<Squeegee>();
        _window = FindObjectOfType<Paint>();
        _rigidbody = GetComponent<Rigidbody>();
        _playerCam = Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Application.targetFrameRate = 60;
        uxZoomInOutPrompt.SetActive(false);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.E) && readyToClean) {
            SwitchState();
        }
        if (Time.timeScale == 0) return;

        switch (state) {
            case PlayerState.Looking:
                _rigidbody.constraints &= ~RigidbodyConstraints.FreezePosition; //fix for player jumping and off the platform
                Looking();
                uxZoomInOutPrompt.SetActive(false);
                break;
            case PlayerState.Cleaning:
                if (!transitionInProgress) UpdateSquegee();
                if (_squeegee.GetComponent<MeshRenderer>().enabled) uxZoomInOutPrompt.SetActive(true);

                _rigidbody.constraints = RigidbodyConstraints.FreezePosition; //fix for player jumping and off the platform
                _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ; //fix for player jumping and off the platform
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Window")
        {
            readyToClean = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Window")
        {
            readyToClean = true;
        }
    }

    private void FixedUpdate() {
        switch (state) {
            case PlayerState.Looking:
                Movement();
                break;
            case PlayerState.Cleaning:
                break;
            default:
                break;
        }
    }

    private void SwitchState() {
        switch (state) {
            case PlayerState.Looking:
                state = PlayerState.Cleaning;
                transitionInProgress = true;
                UXPrompt.Instance.ActionEnabled(true);
                transitionRoutine = StartCoroutine(MovePlayerToWindow());
                _squeegee.ChangeState(0, windowTarget[currentFloor].transform.position);
                GameObject.FindGameObjectWithTag("StoryManager").GetComponent<StoryController>().PlayVignette(currentFloor);
                break;
            case PlayerState.Cleaning:
                GameObject.FindGameObjectWithTag("StoryManager").GetComponent<StoryController>().ExitVignette(currentFloor);
                StopCoroutine(transitionRoutine);
                transitionInProgress = false;
                _squeegee.ChangeState(1, Vector3.zero);
                ResetSquegee();
                UXPrompt.Instance.ActionEnabled(false);
                // why do we do this rotation???
                rotationY = -_playerCam.transform.rotation.eulerAngles.x < -180 ? -_playerCam.transform.rotation.eulerAngles.x + 360 : -_playerCam.transform.rotation.eulerAngles.x;
                state = PlayerState.Looking;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public PlayerState GetPlayerState()
    {
        return state;
    }

    private IEnumerator MovePlayerToWindow() {

        print("CURRENT FLOOR " + currentFloor);
        WaitForFixedUpdate w = new WaitForFixedUpdate();
        yield return w;
        Vector3 startPosition = _rigidbody.position;
        Vector3 endPosition = windowTarget[currentFloor].transform.position; // LYHNE EDIT
        Quaternion playerStartRotation = _rigidbody.rotation;
        Quaternion playerEndRotation = windowTarget[currentFloor].transform.rotation; // LYHNE EDIT
        Quaternion camStartRotation = _playerCam.transform.rotation;
        Quaternion camEndRotation = windowTarget[currentFloor].transform.rotation; // LYHNE EDIT
        float startTime = Time.time;
        float endTime = Time.time + stateTransition.keys[stateTransition.length - 1].time;

        while (endTime > Time.time) {
            float t = Time.time - startTime;
            float value = stateTransition.Evaluate(t);

            Vector3 position = Vector3.Lerp(startPosition, endPosition, value);

            _rigidbody.position = position;
            _rigidbody.rotation = Quaternion.Slerp(playerStartRotation, playerEndRotation, value);
            _playerCam.transform.position = position;
            _playerCam.transform.rotation = Quaternion.Slerp(camStartRotation, camEndRotation, value);

            yield return w;
        }

        transitionInProgress = false;
        _rigidbody.position = endPosition;
        _rigidbody.rotation = playerEndRotation;
        _playerCam.transform.position = endPosition;
        _playerCam.transform.rotation = camEndRotation;
        rotationY = _playerCam.transform.rotation.eulerAngles.y;
    }

    // Run in Update
    private void Looking() {
        float smoothS = Mathf.SmoothStep(0.0f, 2.0f, 10f);

        float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX * smoothS;
        rotationY += Input.GetAxis("Mouse Y") * sensitivityY * smoothS;
        rotationY = Mathf.Clamp(rotationY, -maxCamAngle, maxCamAngle);

        _rigidbody.MoveRotation(Quaternion.Euler(0, rotationX, 0));

        _playerCam.transform.position = this.transform.position;
        _playerCam.transform.localEulerAngles = Vector3.RotateTowards(this.transform.eulerAngles,new Vector3(-rotationY, rotationX, 0),Mathf.Deg2Rad * maxCamAngle,maxCamAngle); //new Vector3(-rotationY, rotationX, 0) -Previous


        // LYHNE EDIT
        // elevator ray
        if (isRayEnabled) {
            ElevatorAction(); //Checks elevator method movement and applies behavior.

            //Floor to Floor Activation
            if (isOptionSelected) ActivateFloorToFloorMove();

            //Manual Movement Activation
            if (isManualMove) {
                ActivateManualMove(true);
                SetMoveStatus(false);
            }
            else {
                ActivateManualMove(false);
                SetMoveStatus(true);
            }
        }
    }


    // Run in FixedUpdate
    private void Movement() {
        Vector3 moveDir = _rigidbody.rotation * new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        _rigidbody.MovePosition(_rigidbody.position + movementSpeed * Time.fixedDeltaTime * moveDir);
    }

    private void SetDirection(Vector3 dir) {
        _playerCamDirection = dir;
    }

    public Vector3 GetDirection(){
        return _playerCamDirection;
    }

    private void UpdateSquegee(){
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X") * squeegeeSensitivityX, Input.GetAxis("Mouse Y") * squeegeeSensitivityY);
        _squeegee.MovePosition(mouseDelta * Time.deltaTime);
        _window.UpdateCircle(_squeegee.transform.position);
    }

    private void ResetSquegee()
    {
        _window.UpdateCircle(new Vector3(-9000, -9000, -9000));
    }

    // LYHNE EDIT - BELOW IS ELEVATOR FUNCTIONALITY
    void ActivateFloorToFloorMove()
    {
        if (buttonGO != null && buttonGO.GetComponentInParent<Elevator>().GetElevatorMovementType() == 0)
        {
            _squeegee.ChangeState(2, Vector3.zero);
            currentFloor = GameObject.Find("ElevatorPlatform").GetComponent<Elevator>().GetCurrentLevel();
            if (currentFloor >= 6) currentFloor = 5; // sorry for this ugly ninja  // (found it) :P 
            if (GameObject.Find("Window" + currentFloor) != null)
            {
                _window = GameObject.Find("Window" + currentFloor).GetComponent<Paint>();
                if (buttonGO.name == "UpButton")
                {
                    stat = new Status(true, 1);
                    buttonGO.SendMessageUpwards("SetStatus", stat);
                }

                if (buttonGO.name == "DownButton")
                {
                    //stat = new Status(true, -1);
                    // Play error sound here or fix currentFloor bug
                    print("FEATURE CURRENTLY UNAVAILABLE");
                }

            }
            //buttonGO.SendMessageUpwards("SetStatus", stat); //signal elevator to move.
            //currentFloor = GameObject.Find("ElevatorPlatform").GetComponent<Elevator>().GetCurrentLevel(); //update current level from the elevator instead. MICHAEL EDIT
            // LYHNE NINJA since we now have a floor with no window, this approach needs rethinking
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
        if (GetPlayerState() != PlayerState.Cleaning)
        {
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.rotation * Vector3.forward, out RaycastHit hit, 5f))
            {
                if ((hit.collider.gameObject.name == "UpButton" || hit.collider.gameObject.name == "DownButton") && !isOptionSelected)
                {
                    buttonGO = hit.collider.gameObject;
                    buttonMat = hit.collider.gameObject.GetComponent<Renderer>().material;
                    buttonMat.EnableKeyword("_EMISSION");

                    //Single Touch for Floor to Floor
                    if (buttonGO.GetComponentInParent<Elevator>().GetElevatorMovementType() == 0)
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            buttonGO.GetComponent<SoundEmitter>().PlayClickSound(0);
                            isOptionSelected = !isOptionSelected;
                            UXPrompt.Instance.ActionEnabled(isOptionSelected);//Disable UI prompts
                        }
                    }

                    //Continuous Touch for Manual Movement
                    if (buttonGO.GetComponentInParent<Elevator>().GetElevatorMovementType() == 1)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            isManualMove = true;
                            UXPrompt.Instance.ActionEnabled(isManualMove);//Disable UI prompts
                        }
                        else
                        {
                            isManualMove = false;
                            UXPrompt.Instance.ActionEnabled(isManualMove);//Enable UI prompts
                        }
                    }
                }
                else if (buttonMat != null && !isOptionSelected)
                {
                    buttonMat.DisableKeyword("_EMISSION");
                    UXPrompt.Instance.ActionEnabled(isOptionSelected);//Disable UI prompts
                }
            }
            else if (buttonMat != null && !isOptionSelected)
            {
                buttonMat.DisableKeyword("_EMISSION");
                UXPrompt.Instance.ActionEnabled(isOptionSelected);//Disable UI prompts
            }
        }
    }

    //User Defined functions
    public void ResetOption(bool flag){
        if (isOptionSelected == true) isOptionSelected = flag;
        if (isManualMove == true) isManualMove = flag;
    }

    void SetMoveStatus(bool move){
        movementState = move;
    }

    //Unity Functions
    void OnCollisionEnter(Collision collision){
        _rigidbody.drag = 99999; //fixes the problem with player jitter and jump off platform
        if (collision.gameObject.tag == "Elevator") isRayEnabled = true;
    }

    void OnCollisionStay(Collision collision){
        _rigidbody.drag = 99999; //fixes the problem with player jitter and jump off platform
        if (collision.gameObject.tag == "Elevator") isRayEnabled = true;
    }

    void OnCollisionExit(Collision collision){
        _rigidbody.drag = 0; //fixes the problem with player bouncing off the platform
        if (collision.gameObject.tag == "Elevator") isRayEnabled = false;
    }
}
