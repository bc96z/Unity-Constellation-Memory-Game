using UnityEngine;
using TMPro;

public class GameSettingsManager : MonoBehaviour
{
    /// <summary>
    /// 將UI的值儲存
    /// </summary>
    public TMP_Dropdown starDropdown; // 下拉選單：星座
    public TMP_Dropdown ballsDropdown;  // 球數
    public TMP_InputField playerNameInput;  // 輸入框：玩家名字
    public TMP_Dropdown diffDropdown; // 難度選擇


    // 儲存參數
    public void SaveSettings()
    {
        string starSign = starDropdown.options[starDropdown.value].text;
        PlayerPrefs.SetString("StarSign", starSign);

        int balls = int.Parse(ballsDropdown.options[ballsDropdown.value].text);
        PlayerPrefs.SetInt("InitBallsNum", balls);
 
        string playerName = playerNameInput.text;
        PlayerPrefs.SetString("PlayerName", playerName);

        string diff = diffDropdown.options[diffDropdown.value].text;
        PlayerPrefs.SetString("Difficulty", diff);

        PlayerPrefs.Save();
    }

    // 載入設定（可選，遊戲啟動時恢復設定）
    public void LoadSettings()
    {
        if (PlayerPrefs.HasKey("StarSign"))
        {
            string savedStarSign = PlayerPrefs.GetString("StarSign");

            // 設定 Dropdown 為儲存的選項
            int index = starDropdown.options.FindIndex(option => option.text == savedStarSign);
            if (index != -1)
            {
                starDropdown.value = index;
            }
        }
            
        if(PlayerPrefs.HasKey("InitBallsNum"))
        {
            int savedBalls = PlayerPrefs.GetInt("InitBallsNum");

            // 找到對應數值的索引
            int index = ballsDropdown.options.FindIndex(option => option.text == savedBalls.ToString());

            if (index != -1)
            {
                ballsDropdown.value = index; // 設定 UI 的 Dropdown
            }
        }

        if (PlayerPrefs.HasKey("PlayerName"))
        {
            playerNameInput.text = PlayerPrefs.GetString("PlayerName");
        }

        if (PlayerPrefs.HasKey("Difficulty"))
        {
            string savedDiff = PlayerPrefs.GetString("Difficulty");

            // 找到對應數值的索引
            int index = diffDropdown.options.FindIndex(option => option.text == savedDiff);

            if (index != -1)
            {
                diffDropdown.value = index; // 設定 UI 的 Dropdown
            }
        }
    }
}
