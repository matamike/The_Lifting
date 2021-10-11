using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Squeegee : MonoBehaviour
{
    public Paint painter;
    public Transform restingPoint;
    public AnimationCurve stateTransition;
    [Range(0.03f, 1.0f)]
    public float animationDuration = 0.25f;

    private BoxCollider _collider;
    private AudioSource _audio;

    private Vector3 restingPosition = Vector3.zero;
    private Vector3 cleaningPosition = Vector3.zero;
    private Vector3 cleaningOffset = new Vector3(0, 0, 5.25f); // offset from player to window
    private Coroutine transitionRoutine;

    private void Awake(){
        if (restingPoint != null) {
            restingPosition = restingPoint.position;
            transform.position = restingPosition;
        } 
        else Debug.Log("Squeegee missing resting point");
        _collider = GetComponent<BoxCollider>();
        _audio = GetComponent<AudioSource>();
    }

    // Called by player to set squeegee to cleaning, resting or following the elevator
    // Only needs the vector3 for cleaning, should pass vector3.zero in all other cases. to be fixed
    public void ChangeState(int state, Vector3 endPosition)
    {
        if (transitionRoutine != null) StopCoroutine(transitionRoutine);
        // Cleaning
        if (state == 0)
        {
            cleaningPosition = endPosition + cleaningOffset;
            transitionRoutine = StartCoroutine(MoveSqueegee(transform.position, cleaningPosition));
        }
        // Resting
        else if (state == 1)
        {
            restingPosition = restingPoint.position;
            transitionRoutine = StartCoroutine(MoveSqueegee(transform.position, restingPosition));
        }
        // Following Elevator
        else if (state == 2)
        {
            transitionRoutine = StartCoroutine(FollowElevator());
        }
        else Debug.Log("Squeegee does not have a " + state + " state");
    }

    // Move the squeegee to clean
    public void MovePosition(Vector2 dir)
    {
        //if (magnitude < 0.05)
        //{
        //    _audio.volume = 0;
        //}
        //else
        //{
        //    _audio.volume = 1;
        //}


        if (!_audio.isPlaying)
        {
            float magnitude = dir.sqrMagnitude;
            if (magnitude != 0)
            {
                magnitude = Mathf.Clamp(magnitude * 4.4f + 0.7f, 0.75f, 1.25f);
                _audio.pitch = magnitude;
                _audio.Play();
            }
        }
        transform.position = new Vector3(transform.position.x + dir.x, transform.position.y + dir.y, transform.position.z);
        Vector3 size = _collider.size;
    }

    // Follows the elevator for the duration of elevator movement
    private IEnumerator FollowElevator()
    {
        WaitForEndOfFrame w = new WaitForEndOfFrame();
        yield return w;

        float endTime = Time.time + 7.5f; // should be relative to elevator animation time

        while (endTime > Time.time)
        {
            restingPosition = restingPoint.position;
            transform.position = restingPosition;
            yield return w;
        }
    }

    // Move squeegee between resting and cleaning points
    private IEnumerator MoveSqueegee(Vector3 start, Vector3 end)
    {
        WaitForFixedUpdate w = new WaitForFixedUpdate();

        float startTime = Time.time;
        float endTime = Time.time + animationDuration;

        while (endTime > Time.time)
        {
            float t = (Time.time - startTime) / animationDuration;
            float value = stateTransition.Evaluate(t);

            Vector3 tempPosition = Vector3.Lerp(start, end, value);
            transform.position = tempPosition;
            yield return w;
        }

        transform.position = end;
    }
}
