using UnityEngine;
using UnityEngine.SceneManagement;

public class StartButtonHandler : MonoBehaviour
{
    public GameSettingsManager settingsManager; // 引用 GameSettingsManager

    // 當按下「開始遊戲」按鈕時執行
    public void OnStartGameButtonClicked()
    {
        settingsManager.SaveSettings(); // 儲存遊戲設定
        SceneManager.LoadScene("3DClickInOrderGame"); // 載入遊戲場景
    }
}
