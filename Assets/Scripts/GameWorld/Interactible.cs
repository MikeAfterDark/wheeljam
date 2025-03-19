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
    private static GameObject clicked = null;
    private bool hovered = false;
    public Color outlineColor = Color.white;
    private bool prevFrameClickable = false;
    private Outline outline;

    public string[] queuedEvents = { "Attack", "Move", "Heal" };

    void Awake()
    {
        outline = GetComponent<Outline>();
        outline.enabled = false;
        prevFrameClickable = !clickable;
    }

    void Update()
    {
        if (prevFrameClickable != clickable)
        {
            // outline.OutlineColor = clickable ? Color.green : Color.white;
            prevFrameClickable = clickable;
        }

        if (clickable && clicked != gameObject && !hovered)
        {
            outline.enabled = false;
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
}
