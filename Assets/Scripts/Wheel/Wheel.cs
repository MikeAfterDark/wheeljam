using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum WheelStates
{
    Idle,
    Rotating,
    NoInput,
}

public enum SelectionDir
{
    Up,
    Left,
    Down,
    Right,
}

public class Wheel : MonoBehaviour
{
    public WheelStates state;
    public float rotationTime = 1.0f;
    public AnimationCurve rotationAnimationCurve;

    public Transform[] slices;
    public Transform[] covers;
    public Transform selector;
    public Transform sliceGimbal;

    public int sliceReference = 0;
    public int[] sliceValues = new int[] { 4, 1, 2, 3 };
    private Camera mainCamera;

    private int currentSelectionIndex = 0;
    public SelectionDir selectedDir = SelectionDir.Up;

    public UnityEvent<Vector2Int> newDirSelected;
    public UnityEvent resetWheel;

    void Start()
    {
        state = WheelStates.Idle;
        mainCamera = Camera.main;

        newDirSelected = new UnityEvent<Vector2Int>();
        resetWheel = new UnityEvent();

        Reset();
    }

    void Update()
    {
        if (state != WheelStates.Idle)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Select();
            if (Select())
                Rotate();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Rotate();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Reset();
        }

        Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        MoveSelector(dir);
    }

    bool Select()
    {
        if (covers[(int)selectedDir].gameObject.active)
            return false;
        covers[(int)selectedDir].gameObject.SetActive(true);

        float angle = (int)selectedDir * 90 * Mathf.Deg2Rad;

        Vector2Int direction = new Vector2Int(
            (int)Mathf.Round(Mathf.Sin(-angle)),
            (int)Mathf.Round(Mathf.Cos(angle))
        );
        int strength = sliceValues[((int)selectedDir + sliceReference) % 4];

        newDirSelected?.Invoke(direction * strength);
        return true;
    }

    void Rotate()
    {
        if (state == WheelStates.Rotating)
            return;
        StartCoroutine(RotateWheel());
    }

    IEnumerator RotateWheel()
    {
        state = WheelStates.Rotating;
        float elapsedTime = 0f;
        float startAngle = sliceGimbal.localEulerAngles.z;
        float endAngle = startAngle - 90f;

        while (elapsedTime < rotationTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotationTime;
            float curveValue = rotationAnimationCurve.Evaluate(t);
            float newAngle = Mathf.Lerp(startAngle, endAngle, curveValue);
            sliceGimbal.localRotation = Quaternion.Euler(0, 0, newAngle);
            yield return null;
        }

        sliceGimbal.localRotation = Quaternion.Euler(0, 0, endAngle);
        // currentSelectionIndex = (currentSelectionIndex + 1) % slices.Length;
        state = WheelStates.Idle;
        sliceReference = (sliceReference + 1) % 4;
    }

    void MoveSelector(Vector2 dir)
    {
        if (dir == Vector2.zero)
            return;

        Vector3 cameraForward = mainCamera.transform.forward;
        Vector3 cameraRight = mainCamera.transform.right;

        cameraForward.y = 0;
        cameraRight.y = 0;
        cameraForward.Normalize();
        cameraRight.Normalize();

        Vector3 worldDirection = (cameraForward * -dir.x + cameraRight * dir.y).normalized;
        float angle = Mathf.Atan2(worldDirection.z, worldDirection.x) * Mathf.Rad2Deg;

        if (angle < 0)
            angle += 360;

        if (angle % 45 == 0)
        {
            angle += 0.1f;
        }

        currentSelectionIndex = Mathf.FloorToInt((angle + 45) / 90) % 4;
        selectedDir = (SelectionDir)currentSelectionIndex;
        Debug.Log($"SelectionDir: {selectedDir}, index: {currentSelectionIndex}");

        selector.localEulerAngles = new Vector3(0, 0, currentSelectionIndex * 90);
        // Vector3 worldDirection = (cameraRight * dir.x + cameraForward * dir.y).normalized;
        // float angle = Mathf.Atan2(worldDirection.z, worldDirection.x) * Mathf.Rad2Deg;
        // if (angle < 0)
        //     angle += 360;
        //
        // if (angle >= 45 && angle < 135)
        //     currentSelectionIndex = 0; // Forward
        // else if (angle >= 135 && angle < 225)
        //     currentSelectionIndex = 1; // Left
        // else if (angle >= 225 && angle < 315)
        //     currentSelectionIndex = 2; // Backward
        // else
        //     currentSelectionIndex = 3; // Right
        // selector.localEulerAngles = the square angle for that selection index
        //
        // Debug.Log($"{Time.time}: Moved Selector to Index: {currentSelectionIndex}");
    }

    public void Reset()
    {
        state = WheelStates.Idle;
        currentSelectionIndex = 0;

        // Visual indicator resets
        sliceGimbal.localEulerAngles = Vector3.zero;

        Stack<float> angleStack = new Stack<float>(new float[] { 0f, 90f, 180f, 270f });
        foreach (Transform slice in slices)
        {
            float angle = angleStack.Pop();
            slice.localEulerAngles = new Vector3(0, 0, angle);
        }
        foreach (Transform cover in covers)
        {
            cover.gameObject.SetActive(false);
        }

        resetWheel?.Invoke();
        Debug.Log("Wheel Reset");
    }
}
