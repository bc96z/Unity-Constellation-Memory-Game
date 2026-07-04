using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//±±¨î¦^¨ì menu

public class BackToMENU : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToMenu()
    {
        SceneManager.LoadScene("MenuUI");
    }
}

