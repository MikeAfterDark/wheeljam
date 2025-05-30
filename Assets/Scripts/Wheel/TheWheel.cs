using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;

// CODE (C) Colin McInerney 2025

public enum WheelState
{
    AwaitingSelection,
    Rotating,
    NoInput,
}

public class TheWheel : MonoBehaviour
{
    /*
        Hello and welcome to the Persuasion Wheel Game Jam AKA WHEELJAM!
    
        This code is free to use and distribute for the purposes of WHEELJAM and any games you choose to make with it besides. Please provide attribution if you use it outside the jam. :)
        This code is also not required for WHEELJAM -- you are free to make your own implementation of the wheel, use your own assets, etc.
        Everything to make the wheel function is in this single script -- this is a deliberate choice to keep it plug-n-play for the jam. I don't recommend coding like this!
    
        Have fun and enjoy WHEELJAM!
        Love, Colin
    */

    #region Events
    public UnityEvent<WheelPayload> newDirChosen;
    public UnityEvent<Vector2Int> newDirSelected;
    public UnityEvent<WheelPayload> rotationStarted;
    public UnityEvent rotationFinished;
    public UnityEvent puzzleFinished;
    public UnityEvent resetWheel;
    #endregion

    #region Private Fields
    [SerializeField]
    private Transform[] slices; // these are the triangles of varying sizes that rotate and multiply by x1, x2, x3 or x4

    [SerializeField]
    private Transform[] covers; // once a quadrant is selected, it becomes disabled for selection, and these appear overlaid on the quadrant to communicate that

    [SerializeField]
    private Transform selector; // players input to move this -- controlled by sending a Vector into the ProcessInput function

    [SerializeField]
    private Transform sliceGimbal; // this is the thing that actually gets rotated

    [SerializeField]
    private int[] baseNumbers = new int[] { -2, -1, 1, 2 }; // order these from lowest value to greatest

    [SerializeField]
    private int[] sliceValues = new int[] { 1, 2, 3, 4 }; // these should also be lowest to greatest

    [SerializeField]
    private AnimationCurve curve; // used to evaluate how the turn animates

    private Dictionary<Vector3, int> _valueMappings; // this gets set in Awake -- ties a direction to a baseNumber value randomly
    private WheelState _state = WheelState.AwaitingSelection; // don't touch this unless you know what you're doing lol
    private int _numSelections = 0; // how many selections total have been made
    private WheelPayload _currentValue; // this is the actual current value selected

    // declaring directions as rotations here
    private readonly Vector3 _dirUp = new Vector3(0f, 0f, 0f);
    private readonly Vector3 _dirRight = new Vector3(0f, 0f, 270f);
    private readonly Vector3 _dirDown = new Vector3(0f, 0f, 180f);
    private readonly Vector3 _dirLeft = new Vector3(0f, 0f, 90f);

    private readonly int _targetSelections = 4; // how many selections _should_ be made
    #endregion

    #region Unity Methods

    private void Awake()
    {
        if (newDirSelected == null)
            newDirSelected = new UnityEvent<Vector2Int>();

        if (resetWheel == null)
            resetWheel = new UnityEvent();
    }

    private void Start()
    {
        Reset(); // all the setup is contained in reset
    }

    // throw this out and use unity's new input system or whatever plugin you decide, get your input, and pass it off through the ProcessDirectionInput() and ProcessConfirmInput() methods; this is only here for demonstration purposes and is inefficient and inflexible
    private void Update()
    {
        if (_state != WheelState.AwaitingSelection) // this is used in some other places too but as a failsafe i put it here
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ProcessConfirmInput();
            return;
        }

        if (Input.GetKeyDown(KeyCode.R) && _numSelections == 0)
        {
            Rotate();
            return;
        }

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;
        camForward.Normalize();
        camRight.Normalize();

        Vector2 inputDirection = Vector2.zero;

        if (Input.GetKeyDown(KeyCode.W))
            inputDirection = new Vector2(camForward.x, camForward.z);

        if (Input.GetKeyDown(KeyCode.S))
            inputDirection = new Vector2(-camForward.x, -camForward.z);

        if (Input.GetKeyDown(KeyCode.A))
            inputDirection = new Vector2(-camRight.x, -camRight.z);

        if (Input.GetKeyDown(KeyCode.D))
            inputDirection = new Vector2(camRight.x, camRight.z);

        if (inputDirection != Vector2.zero)
            ProcessDirectionInput(inputDirection);
    }

    #endregion

    #region Public Methods
    public void ProcessDirectionInput(Vector2 input) // once you get your directional input from whatever, pass it into this function to change the direction of the wheel
    {
        Vector3 cacheDir = selector.localEulerAngles;

        if (_state != WheelState.AwaitingSelection)
            return;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        {
            if (input.x > 0)
                selector.localEulerAngles = _dirRight;
            else
                selector.localEulerAngles = _dirLeft;
        }
        else
        {
            if (input.y > 0)
                selector.localEulerAngles = _dirUp;
            else
                selector.localEulerAngles = _dirDown;
        }

        if (selector.localEulerAngles != cacheDir)
        {
            _currentValue = GetCurrentWheelValue();
            newDirChosen?.Invoke(_currentValue);
        }
    }

    public void ProcessConfirmInput()
    {
        if (_state != WheelState.AwaitingSelection)
        {
            return;
        }

        foreach (Transform c in covers)
        {
            if (c.localEulerAngles == selector.localEulerAngles)
            {
                if (!c.gameObject.activeSelf)
                {
                    c.gameObject.SetActive(true);
                    Rotate();
                    _numSelections++;

                    Vector2Int direction = Vector2Int.zero;
                    Vector3 euler = selector.localEulerAngles;
                    float zRotation = Mathf.Round(euler.z) % 360;

                    if (zRotation == 0f)
                        direction = Vector2Int.up;
                    else if (zRotation == 90f)
                        direction = Vector2Int.left;
                    else if (zRotation == 180f)
                        direction = Vector2Int.down;
                    else if (zRotation == 270f)
                        direction = Vector2Int.right;

                    int strength = 1; // MIKEY TODO: get strength from the slices... map the rotation of the selector to the value somehow
                    newDirSelected?.Invoke(direction * strength);
                    return;
                }
            }
        }
    }

    // you can call this independently if you want feature parity with TES4
    public void Rotate()
    {
        if (_state != WheelState.AwaitingSelection) // in case we skipped ProcessConfirmInput()
            return;

        _state = WheelState.Rotating;
        rotationStarted?.Invoke(_currentValue);

        StartCoroutine(RotateSlices(-90));
    }

    [ContextMenu("Reset")]
    public void Reset()
    {
        selector.localEulerAngles = Vector3.zero; // remove this if you don't want the selector to reset up every time
        sliceGimbal.localEulerAngles = Vector3.zero;
        _numSelections = 0;

        List<Vector3> directions = GetDirectionsList();

        // hide the covers
        for (int i = 0; i < directions.Count; i++)
        {
            covers[i].localEulerAngles = directions[i];
            covers[i].gameObject.SetActive(false);
        }

        // repopulate list (could probably combine these but i added this later and that's how i'm living my life)
        directions = GetDirectionsList();
        _valueMappings = new Dictionary<Vector3, int>();

        for (int i = 0; i < slices.Length; i++)
        {
            Vector3 dir = directions[UnityEngine.Random.Range(0, directions.Count)];

            _valueMappings.Add(dir, baseNumbers[i]);
            directions.Remove(dir);
        }

        InitializeSlices();

        _currentValue = GetCurrentWheelValue();
        newDirChosen?.Invoke(_currentValue);
        resetWheel?.Invoke();

        _state = WheelState.AwaitingSelection;
    }
    #endregion

    #region Private Methods
    private void InitializeSlices()
    {
        // create list of possible directions
        List<Vector3> directions = GetDirectionsList();

        // assign one to each slice
        foreach (Transform slice in slices)
        {
            Vector3 dir = directions[UnityEngine.Random.Range(0, directions.Count)];
            slice.localEulerAngles = dir;
            directions.Remove(dir);
        }
    }

    private List<Vector3> GetDirectionsList()
    {
        List<Vector3> directions = new List<Vector3> { _dirUp, _dirRight, _dirDown, _dirLeft };

        return directions;
    }

    private IEnumerator RotateSlices(float angle)
    {
        Quaternion startRotation = sliceGimbal.localRotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle) * startRotation;

        float timer = 0f;
        float lastKeyTime = curve.keys[^1].time;

        while (timer < lastKeyTime)
        {
            float t = math.remap(0f, lastKeyTime, 0f, 1f, timer);
            sliceGimbal.localRotation = Quaternion.Lerp(
                startRotation,
                targetRotation,
                curve.Evaluate(t)
            );
            timer += Time.deltaTime;

            yield return null;
        }

        sliceGimbal.localRotation = targetRotation;

        _currentValue = GetCurrentWheelValue();
        newDirChosen?.Invoke(_currentValue);
        rotationFinished?.Invoke();
        EndCheck();
    }

    // this gets added as a listener in awake to rotationFinished
    private void EndCheck()
    {
        if (_numSelections >= _targetSelections)
        {
            _state = WheelState.NoInput;
            puzzleFinished?.Invoke();
        }
        else
        {
            _state = WheelState.AwaitingSelection;
        }
    }

    private WheelPayload GetCurrentWheelValue()
    {
        WheelPayload wp = new WheelPayload();

        foreach (KeyValuePair<Vector3, int> kvp in _valueMappings)
        {
            if ((int)selector.localEulerAngles.z == (int)kvp.Key.z)
            {
                for (int i = 0; i < slices.Length; i++)
                {
                    if ((int)slices[i].eulerAngles.z == (int)kvp.Key.z)
                    {
                        wp.BaseValue = kvp.Value;
                        wp.SliceValue = sliceValues[i];
                        wp.TotalValue = wp.BaseValue * wp.SliceValue;
                        return wp;
                    }
                }
            }
        }

        return null; // you fricked up
    }
    #endregion
}

public class WheelPayload
{
    public int BaseValue;
    public int SliceValue;
    public int TotalValue;
}
