using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadmePopUp : MonoBehaviour
{
    public GameObject helpPanel;

    public GameObject[] uiToHide;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleHelp()
    {
        helpPanel.SetActive(!helpPanel.activeSelf);
        foreach (GameObject go in uiToHide)
        {
            if (go != null)
                go.SetActive(false);
        }
    }

    public void HideHelp()
    {
        helpPanel.SetActive(false);
        foreach (GameObject go in uiToHide)
        {
            if (go != null)
                go.SetActive(true);
        }
    }
}
