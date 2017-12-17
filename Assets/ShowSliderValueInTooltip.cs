using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowSliderValueInTooltip : ToolTipHandler
{        
    // Use this for initialization
    private void Start()
    {
        base.Start();
        this.setDynamicString(()=>"Value: " + GetComponent<Slider>().value);
    }    
}
