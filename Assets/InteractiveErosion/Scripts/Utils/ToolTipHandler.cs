using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System;
/// <summary>
/// Newest version of the class. 
/// </summary>
public class ToolTipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField]
    private Func<string> dynamicText;
    [SerializeField]
    private string text;
    [SerializeField]
    private bool isDynamic;
    //[SerializeField]
    protected MainTooltip tooltipHolder;
    private bool inside;
    [SerializeField]
    private bool useDynamicUpdate;

   
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
