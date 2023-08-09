using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TwoStateElevator : MonoBehaviour
{
    [SerializeField] float BottomFloorElevation = 0.0f; // elevation of the top of the floor blocks, not the bottom
    [SerializeField] float TopFloorElevation = 0.0f;
    [SerializeField] AnimationCurve ElevatorTransitionCurve;
    [SerializeField] float RideDuration = 4.0f;
    [SerializeField] List<MovingEnvironment> ElevatorPlatforms;

    enum TwoStateElevatorState { GoingUp, GoingDown, AtTopFloor, AtBottomFloor};

    [SerializeField] TwoStateElevatorState CurrentState;
    float NormalizedProgress = 0.0f;



    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        switch (CurrentState)
        {
            case TwoStateElevatorState.GoingDown:
            case TwoStateElevatorState.GoingUp:
                RunElevator();
                break;
        }
    }

    public void Toggle()
    {
        switch (CurrentState)
        {
            case TwoStateElevatorState.AtBottomFloor:
                NormalizedProgress = 0.0f;
                CurrentState = TwoStateElevatorState.GoingUp;
                break;
            case TwoStateElevatorState.AtTopFloor:
                NormalizedProgress = 0.0f;
                CurrentState = TwoStateElevatorState.GoingDown;
                break;
        }
    }

    void RunElevator()
    {
        float PreviousFloor = CurrentState == TwoStateElevatorState.GoingDown ? TopFloorElevation : BottomFloorElevation;
        float NextFloor = CurrentState == TwoStateElevatorState.GoingDown ? BottomFloorElevation : TopFloorElevation;
        NormalizedProgress += Time.deltaTime / RideDuration;

        if (NormalizedProgress >= 1.0f)
        {
            NormalizedProgress = 1.0f;
            CurrentState = CurrentState == TwoStateElevatorState.GoingUp ? TwoStateElevatorState.AtTopFloor : TwoStateElevatorState.AtBottomFloor;
        }

        foreach (MovingEnvironment platform in ElevatorPlatforms)
        {
            platform.SetToElevation(Mathf.Lerp(PreviousFloor, NextFloor, ElevatorTransitionCurve.Evaluate(NormalizedProgress)), true);
        }
    }

    /*
    IEnumerator GoToFloor(bool IsGoToBottomFloor)
    {
        CurrentState = IsGoToBottomFloor ? TwoStateElevatorState.GoingDown : TwoStateElevatorState.GoingUp;
        float PreviousFloor = IsGoToBottomFloor ? TopFloorElevation : BottomFloorElevation;
        float NextFloor = IsGoToBottomFloor ? BottomFloorElevation : TopFloorElevation;
        NormalizedProgress = 0.0f;

        while (NormalizedProgress < 1.0f)
        {
            NormalizedProgress += Time.deltaTime / RideDuration;
            foreach (MovingEnvironment platform in ElevatorPlatforms)
            {
                platform.SetToElevation(Mathf.Lerp(PreviousFloor, NextFloor, ElevatorTransitionCurve.Evaluate(NormalizedProgress)), true);
            }
            yield return new WaitForEndOfFrame();
        }
        CurrentState = IsGoToBottomFloor ? TwoStateElevatorState.AtBottomFloor : TwoStateElevatorState.AtTopFloor;
    }*/

    // useful when player enters a level from an elevator in motion. use negative to indicate going down, or positive for going up
    public void SetStateImmediate(float NormalizedElevation)
    {
        float PreviousFloor, NextFloor;
        if (NormalizedElevation > 0)
        {
            PreviousFloor = BottomFloorElevation;
            NextFloor = TopFloorElevation;
            CurrentState = TwoStateElevatorState.GoingUp;
        }
        else
        {
            PreviousFloor = TopFloorElevation;
            NextFloor = BottomFloorElevation;
            CurrentState = TwoStateElevatorState.GoingDown;
        }

        NormalizedProgress = Mathf.Abs(NormalizedElevation);
        // since this may happen in scene initialization, need to do this so player is set to the right elevation.
        foreach (MovingEnvironment platform in ElevatorPlatforms)
        {
            platform.SetToElevation(Mathf.Lerp(PreviousFloor, NextFloor, ElevatorTransitionCurve.Evaluate(NormalizedProgress)), true);
        }
    }
}
