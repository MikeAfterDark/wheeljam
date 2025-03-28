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
    public float rotationTime = 0.4f;
    public AnimationCurve rotationAnimationCurve;

    public Transform[] slices;
    public Transform[] covers;
    public Transform selector;
    public Transform sliceGimbal;

    public int sliceReference = 0;
    public int[] sliceValues = new int[] { 4, 3, 2, 1 }; //selectiondir is counterclockwise
    public int[] sliceSelectionMap = new int[] { 0, 3, 2, 1 };
    private Camera mainCamera;

    private int currentSelectionIndex = 0;
    public SelectionDir selectedDir = SelectionDir.Up;
    public Vector3 normalSliceScale = new Vector3(1, 1, 1);
    public Vector3 highlightedSliceScale = new Vector3(1.2f, 1.2f, 1.2f);
    public float highlightedScaleHeight = 0.5f;

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
            if (Select())
            {
                Rotate();
            }
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
        if (dir != Vector2.zero)
        {
            MoveSelector(dir);
        }
        UpdateSelectorIndicator();
    }

    bool Select()
    {
        if (covers[(int)selectedDir].gameObject.activeSelf)
            return false;
        covers[(int)selectedDir].gameObject.SetActive(true);

        float angle = (int)selectedDir * 90 * Mathf.Deg2Rad;

        Vector2Int direction = new Vector2Int(
            (int)Mathf.Round(Mathf.Sin(-angle)),
            (int)Mathf.Round(Mathf.Cos(angle))
        );
        int strength = sliceValues[((int)selectedDir + sliceReference) % 4];

        Debug.Log("Selected: " + (direction * strength).ToString());
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
        state = WheelStates.Idle;
        sliceReference = (sliceReference + 1) % 4;
        selectedDir = (SelectionDir)(((int)selectedDir + 3) % 4);
    }

    void MoveSelector(Vector2 dir)
    {
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
        // Debug.Log($"SelectionDir: {selectedDir}, index: {currentSelectionIndex}");

        selector.localEulerAngles = new Vector3(0, 0, currentSelectionIndex * 90);
    }

    public void UpdateSelectorIndicator()
    {
        int mapping = ((int)selectedDir + sliceReference + 1) % 4;
        int index = sliceSelectionMap[mapping];

        slices[index].localScale = highlightedSliceScale;
        slices[index].localPosition = new Vector3(0, 0, -highlightedScaleHeight);
        covers[(int)selectedDir].localScale = highlightedSliceScale;
        covers[(int)selectedDir].localPosition = new Vector3(0, 0, -highlightedScaleHeight);
        for (int i = 1; i < 4; i++)
        {
            index = (index + 1) % 4;
            slices[index].localScale = normalSliceScale;
            slices[index].localPosition = Vector3.zero;

            covers[((int)selectedDir + i) % 4].localScale = normalSliceScale;
            covers[((int)selectedDir + i) % 4].localPosition = Vector3.zero;
        }
    }

    public void Reset()
    {
        state = WheelStates.Idle;
        currentSelectionIndex = 0;
        selectedDir = SelectionDir.Up;
        sliceReference = 0;

        // Visual indicator resets
        sliceGimbal.localEulerAngles = Vector3.zero;

        Stack<float> angleStack = new Stack<float>(new float[] { 0f, 90f, 180f, 270f });
        foreach (Transform slice in slices)
        {
            float angle = angleStack.Pop(); // TODO: randomize
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
