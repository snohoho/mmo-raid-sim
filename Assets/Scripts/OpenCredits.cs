using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OpenCredits : MonoBehaviour
{
    public RectTransform creditsPanel;

    public void OpenCreditsPanel() {
        creditsPanel.gameObject.SetActive(true);
    }
    
    public void CloseCreditsPanel() {
        creditsPanel.gameObject.SetActive(false);
    }
}
