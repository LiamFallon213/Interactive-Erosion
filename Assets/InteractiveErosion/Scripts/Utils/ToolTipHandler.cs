using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
/// <summary>
/// Provides tooltip to gameobject. Relies on MainTooltip class. V2
/// </summary>
public class ToolTipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Func<string> dynamicText;

    [SerializeField]
    private string text;

    [SerializeField]
    private bool isDynamic;

    [SerializeField]
    private bool useDynamicUpdate;

    protected MainTooltip tooltipHolder;
    private bool inside;
    protected void Start()
    {
        tooltipHolder = MainTooltip.get();
    }
    public void setDynamicString(Func<string> dynamicString)
    {
        this.dynamicText = dynamicString;
        isDynamic = true;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (text != "" || dynamicText != null)
        {
            if (dynamicText == null)
                tooltipHolder.SetTooltip(text);
            else
                tooltipHolder.SetTooltip(dynamicText());
            inside = true;
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipHolder != null)
            tooltipHolder.HideTooltip();
        inside = false;
    }
    public bool isInside()
    {
        return inside;
    }
    private void Update()
    {
        if (isInside() && useDynamicUpdate)
            OnPointerEnter(null);
    }
}
