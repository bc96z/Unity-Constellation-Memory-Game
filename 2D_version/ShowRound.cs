using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShowRound : MonoBehaviour
{

    public ConstellationRenderer constellationRenderer;
    public TextMeshProUGUI roundText; // 顯示 當前/總局數
    public TextMeshProUGUI starText; // 顯示 當前星星數
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        roundText.text = $"Round: {constellationRenderer.currentRound}/{constellationRenderer.totalRound}";
        starText.text = $": {constellationRenderer.starsToShow}";
    }
}
