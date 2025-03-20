using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Interactible
    : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
{
    public static event Action<Interactible> OnClicked;

    public bool hoverable = true;
    public bool clickable = true;
    private static GameObject clicked = null;
    private bool hovered = false;
    public Color outlineColor = Color.white;
    private bool prevFrameClickable = false;
    public Outline outline;
    public bool moved = false;

    public string[] queuedEvents = { "Attack", "Move", "Heal" };
    public TheWheel theWheel;
    public Queue<Vector2Int> selections;

    public Transform globalWheel;

    void Start()
    {
        // globalWheel = GameObject.Find("Wheel").transform;
        outline = GetComponent<Outline>();
        outline.enabled = false;
        prevFrameClickable = !clickable;
        selections = new Queue<Vector2Int>();
        if (clickable)
        {
            theWheel?.newDirSelected?.AddListener(PushSelection);
            // theWheel?.resetWheel.AddListener(ClearSelection);
            theWheel?.gameObject.SetActive(false);
            globalWheel = GameObject.Find("Wheel").transform;
            if (globalWheel == null)
            {
                Debug.LogError("GLOBAL WHEEL IS NULL");
            }
        }
    }

    void Update()
    {
        if (prevFrameClickable != clickable)
        {
            // outline.OutlineColor = clickable ? Color.green : Color.white;
            prevFrameClickable = clickable;
        }

        if (clickable && clicked != gameObject && !hovered && outline.enabled)
        {
            Debug.Log("Got clicked off");
            outline.enabled = false;
            if (theWheel == null)
            {
                Debug.Log("wheel is null");
            }
            if (theWheel.newDirSelected == null)
            {
                Debug.Log("new dir selected is null");
            }
            // theWheel.newDirSelected.RemoveListener(PushSelection);
            // theWheel.resetWheel.RemoveListener(ClearSelection);
            theWheel.gameObject.SetActive(false);
        }
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData) { }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickable)
        {
            OnClicked?.Invoke(this);
            clicked = gameObject;
            outline.enabled = true;
            theWheel.transform.position = globalWheel.transform.position; //TODO: move this mess to levelmanager handleclick()
            theWheel.transform.rotation = globalWheel.transform.rotation;
            theWheel.transform.localScale = globalWheel.transform.localScale;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovered = true;
        if (hoverable)
        {
            if (outline.OutlineColor != outlineColor)
            {
                outline.OutlineColor = outlineColor;
            }
            outline.enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverable && clicked != gameObject)
        {
            outline.enabled = false;
        }

        hovered = false;
    }

    public void PhaseReset()
    {
        ClearSelection();
    }

    public void PushSelection(Vector2Int selection)
    {
        Debug.Log($"{gameObject.name}: Enqueued: {selection}");
        selections.Enqueue(selection);
    }

    public Vector2Int PopSelection()
    {
        Vector2Int pop = selections.Dequeue();
        Debug.Log($"{gameObject.name}: Dequeued: {pop}");
        return pop;
    }

    public void ClearSelection()
    {
        Debug.Log("{gameObject.name}: Clearing Selection");
        theWheel.Reset();
        selections.Clear();
    }
}
