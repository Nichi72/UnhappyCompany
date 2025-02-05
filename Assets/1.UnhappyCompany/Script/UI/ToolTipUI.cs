using System;
using UnityEngine;
using UnityEngine.UI;

public class ToolTipUI : MonoBehaviour
{
    public static ToolTipUI instance { get; private set; }
    public TMPro.TextMeshProUGUI[] toolTipTexts = new TMPro.TextMeshProUGUI[3];
    [SerializeField] [ReadOnly] private IToolTip currentToolTips;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetToolTip(IToolTip toolTip)
    {
        currentToolTips = toolTip;
        if (currentToolTips == null)
        {
            toolTipTexts[0].text = "";
            toolTipTexts[1].text = "";
            toolTipTexts[2].text = "";
            return;
        }
        
        toolTipTexts[0].text = toolTip.ToolTipText;
        toolTipTexts[1].text = toolTip.ToolTipText2;
        toolTipTexts[2].text = toolTip.ToolTipText3;
    }
} 