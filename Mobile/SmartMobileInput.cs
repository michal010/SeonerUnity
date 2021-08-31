using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;


/*
 TODO:
 Rotatial swipe force to calculte the power of spin for gameobject if needed.
 OnDrag, like the OnPinch/OnExpand during the update function, not during ending touch phase so stuff can
 be smoothly calulated by other scirpts. Everytime when there is a diffrence in deltaTouch position and Touch during
 double touch DuringPinch (or somewhat like that) should be called. Same goes for OnSwipe event, calculations should be
 also done during swipe itself not after picking up the finger from the screen, that should be called OnDrag.

     */


public enum Direction { Up, UpperRight, Right, LowerRight, Down, LowerLeft, Left, UpperLeft };

[System.Serializable]
public class InputEvents
{
    public UnityEvent OnTouch;
    public UnityEvent OnDoubleTouch;
    [Header("All the ohers OnSwipe events will be overprioritized by this call.")]
    public UnityEvent OnSwipe;
    [Header("All the ohers OnRotate events will be overprioritized by this call.")]
    public UnityEvent OnRotate;
    public UnityEvent OnPinch;
    public UnityEvent OnExpand;
    [Header("OnSwipeLeft and OnSwipeRight will be overprioritized by this call.")]
    public UnityEvent OnSwipeHorizontal;
    [Header("OnSwipeUp and OnSwipeDown will be overprioritized by this call.")]
    public UnityEvent OnSwipeVertical;
    public UnityEvent OnSwipeUp;
    public UnityEvent OnSwipeLeft;
    public UnityEvent OnSwipeDown;
    public UnityEvent OnSwipeRight;
    public UnityEvent OnRotateRight;
    public UnityEvent OnRotateLeft;
}

public class SmartMobileInput : MonoBehaviour
{
    private Vector2[] touchBeganPos;
    private Vector2[] touchEndPos;
    private bool doubleTouched = false;
    [Header("Minimum swipe distance in screen % size.")]
    [Range(0, 80)]
    public float minSweepDistance = 10f;
    [Header("Magnitude diffrence treshhold of pinching/expanding.")]
    [Range(0.1f, 30f)]
    public float doubleTouchMagnitudeTreshhold = 6.5f;
    [Header("Diameter diffrence treshhold of first and last touch.")]
    [Range(100f, 500f)]
    public float rotationDiameterTreshhold = 150f;

    public float doubleTouchPosDiffrence = 30f;

    [Header("Check to make script trigger not only on curret Game Object.")]
    public bool globalTouchInput = false;

    [SerializeField]
    public InputEvents events;
    /// <summary>
    /// Gets the Magnitude Diffrence between two last double touches,
    /// could be used to calculate shrinking/expanding speed of an Game Object or other calculations.
    /// </summary>
    public float DeltaMagnitudeDiff { get; private set; }

    /// <summary>
    /// Gets the Swipe Force, calculated as distance between initial and ending touch
    /// could be used to calculate rotation speed of an Game Object or other calculations.
    /// </summary>
    public float SwipeForce { get; private set; }

    /// <summary>
    /// Gets the Swipe Force, calculated as distance between initial and ending touch
    /// could be used to calculate rotation of an Game Object or other calculations.
    /// </summary>
    public Vector2 SwipeDirectionVector { get; private set; }

    /// <summary>
    /// Gets the rotation direction, can be used as alternative to OnRotateLeft / OnRotateRight
    ///
    /// </summary>
    public Direction RotationDirection { get; private set; }

    private float elapsedTime = 0f;
    float zeroOneDeltaDistance;
    float zeroOneDistance;


    private void Awake()
    {
        if (events.OnSwipe.GetPersistentEventCount() != 0)
            events.OnSwipeHorizontal = events.OnSwipeVertical = events.OnSwipe;
        if (events.OnSwipeHorizontal.GetPersistentEventCount() != 0)
            events.OnSwipeLeft = events.OnSwipeRight = events.OnSwipeHorizontal;
        if (events.OnSwipeVertical.GetPersistentEventCount() != 0)
            events.OnSwipeUp = events.OnSwipeDown = events.OnSwipeVertical;
        if (events.OnRotate.GetPersistentEventCount() != 0)
            events.OnRotateLeft = events.OnRotateRight = events.OnRotate;
        touchBeganPos = new Vector2[5];
        touchEndPos = new Vector2[5];
    }

    void Update()
    {
        // Debug.Log(CalculateVector2Direction(new Vector2(0, 0), new Vector2(2, 2)));
        if (Input.touchCount > 0)
        {
            if (!globalTouchInput)
            {
                if (!isParentTouched(Input.touches[0].position))
                    return;
            }
            if (Input.touchCount == 2)
            {
                doubleTouched = true;

                if (Input.touches[0].phase == TouchPhase.Began || Input.touches[1].phase == TouchPhase.Began)
                {
                    touchBeganPos[0] = Input.touches[0].position;
                    touchBeganPos[1] = Input.touches[1].position;
                    touchBeganPos[0].x = touchBeganPos[0].x * Screen.height / Screen.width;
                    touchBeganPos[1].x = touchBeganPos[1].x * Screen.height / Screen.width;
                    touchBeganPos[0].y = touchBeganPos[0].y * Screen.width / Screen.height;
                    touchBeganPos[1].y = touchBeganPos[1].y * Screen.width / Screen.height;
                    zeroOneDeltaDistance = (touchBeganPos[0] - touchBeganPos[1]).magnitude;

                }
                else if (Input.touches[0].phase == TouchPhase.Ended || Input.touches[1].phase == TouchPhase.Ended)
                {
                    touchEndPos[0] = Input.touches[0].position;
                    touchEndPos[1] = Input.touches[1].position;

                    touchEndPos[0].x = touchEndPos[0].x * Screen.height / Screen.width;
                    touchEndPos[1].x = touchEndPos[1].x * Screen.height / Screen.width;
                    touchEndPos[0].y = touchEndPos[0].y * Screen.width / Screen.height;
                    touchEndPos[1].y = touchEndPos[1].y * Screen.width / Screen.height;

                    zeroOneDistance = (touchEndPos[0] - touchEndPos[1]).magnitude;
                    float DeltaMagnitudeDiff = zeroOneDeltaDistance - zeroOneDistance;
                    float zeroOneDiameterDiff = Mathf.Abs(DeltaMagnitudeDiff);

                    float posDiffrence = (touchBeganPos[0] - touchEndPos[0]).magnitude + (touchBeganPos[1] - touchEndPos[1]).magnitude;

                    //Check for just double touch by comparing  sum of diffrences between initial and ending touches and double touch treshhold.
                    if (posDiffrence < doubleTouchPosDiffrence)
                    {
                        if (events.OnDoubleTouch.GetPersistentEventCount() != 0)
                            events.OnDoubleTouch.Invoke();
                        else
                            Debug.Log("OnDoubleTouch not assigned!");
                    }
                    else
                    {
                        //Check if it is a rotational movement or pinching / expanding
                        //by comparing absolute value of substraction of first and second diameter of double touch.
                        if (zeroOneDiameterDiff < rotationDiameterTreshhold)
                        {
                            CalculateRotation(touchBeganPos[0], touchEndPos[0], touchBeganPos[1], touchEndPos[1]);
                        }
                        else
                        {
                            if (Mathf.Abs(DeltaMagnitudeDiff) > doubleTouchMagnitudeTreshhold)
                            {
                                if (DeltaMagnitudeDiff > 0)
                                {
                                    if (events.OnPinch.GetPersistentEventCount() != 0)
                                        events.OnPinch.Invoke();
                                    else
                                        Debug.Log("OnPinch not assigned!");
                                }
                                else
                                {
                                    if (events.OnExpand.GetPersistentEventCount() != 0)
                                        events.OnExpand.Invoke();
                                    else
                                        Debug.Log("OnExpand not assigned!");
                                }
                            }
                            else
                            {
                                Debug.Log("Double touchj");
                            }
                        }
                    }
                }

            }
            else if (Input.touchCount == 1 && !doubleTouched)
            {
                if (Input.touches[0].phase == TouchPhase.Began)
                {
                    touchBeganPos[0] = Input.touches[0].position;
                    elapsedTime += Time.deltaTime;
                }
                else if (Input.touches[0].phase == TouchPhase.Ended)
                {
                    touchEndPos[0] = Input.touches[0].position;
                    CalculateSweep(touchBeganPos[0], touchEndPos[0]);
                    elapsedTime = 0f;

                }
                else if (Input.touches[0].phase == TouchPhase.Moved || Input.touches[0].phase == TouchPhase.Stationary)
                {
                    elapsedTime += Time.deltaTime;
                }
            }
        }
        else
            doubleTouched = false;
    }

    private bool isParentTouched(Vector2 v)
    {
        Ray ray = Camera.main.ScreenPointToRay(v);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider != null)
            {
                GameObject touchedObject = hit.transform.gameObject;
                if (touchedObject.name == gameObject.name)
                    return true;
            }
        }
        return false;
    }

    private void CalculateSweep(Vector2 v1, Vector2 v2)
    {
        SwipeDirectionVector = (v2 - v1).normalized;
        if (isHorizontal(v1, v2))
        {
            float disInPerc = (Mathf.Abs(v2.x - v1.x) / Screen.width) * 100;
            if (disInPerc >= minSweepDistance)
            {
                if (v2.x > v1.x)
                {
                    if (events.OnSwipeRight.GetPersistentEventCount() != 0)
                        events.OnSwipeRight.Invoke();
                    else
                        Debug.Log("OnSwipeRight/OnSwipeHorizontal not assigned!");
                }
                else
                {
                    if (events.OnSwipeLeft.GetPersistentEventCount() != 0)
                        events.OnSwipeLeft.Invoke();
                    else
                        Debug.Log("OnSwipeLeft/OnSwipeHorizontal not assigned!");
                }
            }
            else
            {
                if (events.OnTouch.GetPersistentEventCount() != 0)
                    events.OnTouch.Invoke();
                else
                    Debug.Log("OnTouch not assigned!");
            } // not a swipe, too small swipe distance, threat like touch

        }
        else
        {
            float disInPerc = (Mathf.Abs(v2.y - v1.y) / Screen.height) * 100;
            if (disInPerc >= minSweepDistance)
            {
                if (v2.y > v1.y)
                {
                    if (events.OnSwipeUp.GetPersistentEventCount() != 0)
                        events.OnSwipeUp.Invoke();
                    else
                        Debug.Log("OnSwipeUp/OnSwipeVertical not assigned!");
                }
                else
                {
                    if (events.OnSwipeDown.GetPersistentEventCount() != 0)
                        events.OnSwipeDown.Invoke();
                    else
                        Debug.Log("OnSwipeDown/OnSwipeVertical not assigned!");
                }
            }
            else
            {
                if (events.OnTouch.GetPersistentEventCount() != 0)
                    events.OnTouch.Invoke();
                else
                    Debug.Log("OnTouch not assigned!");
            } // not a swipe, too small swipe distance, threat like touch
        }
    }

    private void CalculateRotation(Vector2 v1origin, Vector2 v1destination, Vector2 v2origin, Vector2 v2destination)
    {
        Direction dir;
        if (touchBeganPos[0].y > touchBeganPos[1].y)
            dir = CalculateVector2Direction(v1origin, v1destination);
        else
            dir = CalculateVector2Direction(v2origin, v2destination);

        if (dir == Direction.Left || dir == Direction.LowerLeft || dir == Direction.UpperLeft)
        {
            if (events.OnRotateLeft.GetPersistentEventCount() != 0)
                events.OnRotateLeft.Invoke();
            else
                Debug.Log("OnRotateLeft/OnRotate not assigend!");
        }
        else
        {
            if (events.OnRotateRight.GetPersistentEventCount() != 0)
                events.OnRotateRight.Invoke();
            else
                Debug.Log("OnRotateRight/OnRotate not assigned!");
        }
    }

    private Direction CalculateVector2Direction(Vector2 origin, Vector2 destination)
    {
        Direction dir = 0;

        Vector2 vDir = (destination - origin).normalized;

        if (vDir.x == 0)
        {
            if (vDir.y == 1)
                dir = Direction.Up;
            else if (vDir.y == -1)
                dir = Direction.Down;
        }
        else if (vDir.y == 0)
        {
            if (vDir.x == 1)
                dir = Direction.Right;
            else if (vDir.x == -1)
                dir = Direction.Left;
        }
        else
        {
            if (vDir.y > 0)
            {
                if (vDir.x > 0)
                    dir = Direction.UpperRight;
                else
                    dir = Direction.UpperLeft;
            }
            else
            {
                if (vDir.x > 0)
                    dir = Direction.LowerRight;
                else
                    dir = Direction.LowerLeft;
            }
        }
        return dir;
    }

    private Direction OppositeDirection(Direction dir)
    {
        return (Direction)((int)(dir + 4) % 8);
    }

    bool isHorizontal(Vector2 v1, Vector2 v2)
    {
        if (Mathf.Abs(v2.x - v1.x) > Mathf.Abs(v2.y - v1.y))
        {
            SwipeForce = Mathf.Round(Mathf.Abs(v2.x - v1.x) / (Screen.width * elapsedTime) * 100f) / 100f;
            return true;
        }
        else
        {
            SwipeForce = Mathf.Round(Mathf.Abs(v2.y - v1.y) / (Screen.height * elapsedTime) * 100f) / 100f;
            return false;
        }
    }
}


