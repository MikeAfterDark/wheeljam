using System;
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
    private bool prevFrameClickable = false;
    private Outline outline;

    public string[] queuedEvents = { "Attack", "Move", "Heal" };

    void Awake()
    {
        outline = GetComponent<Outline>();
        prevFrameClickable = !clickable;
    }

    void Update()
    {
        if (prevFrameClickable != clickable)
        {
            outline.OutlineColor = clickable ? Color.green : Color.white;
            prevFrameClickable = clickable;
        }
    }

    public void OnPointerDown(PointerEventData eventData) { }

    public void OnPointerUp(PointerEventData eventData) { }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (clickable)
        {
            OnClicked?.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverable)
        {
            outline.enabled = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverable)
        {
            outline.enabled = false;
        }
    }
}
