using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NestedHorizontalScroll : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect horizontal;
    private ScrollRect vertical;
    private bool routeToParent;

    void Awake()
    {
        horizontal = GetComponent<ScrollRect>();
        vertical = GetComponentInParent<ScrollRect>();
    }

    public void OnInitializePotentialDrag(PointerEventData eventData)
    {
        vertical?.OnInitializePotentialDrag(eventData);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        routeToParent = Mathf.Abs(eventData.delta.y) > Mathf.Abs(eventData.delta.x);

        if (routeToParent)
            vertical?.OnBeginDrag(eventData);
        else
            horizontal.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (routeToParent)
            vertical?.OnDrag(eventData);
        else
            horizontal.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (routeToParent)
            vertical?.OnEndDrag(eventData);
        else
            horizontal.OnEndDrag(eventData);

        routeToParent = false;
    }
}