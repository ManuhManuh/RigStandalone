using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UITest : MonoBehaviour
{
    private int pushCount = 0;
    [SerializeField] private TMP_Text textDisplay;
    public void OnButtonPushed()
    {
        pushCount++;
        textDisplay.text = $"This button has been pushed {pushCount} times";
    }
}
