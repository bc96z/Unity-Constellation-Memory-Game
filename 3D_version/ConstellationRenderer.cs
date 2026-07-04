using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System;
using Unity.VisualScripting;
using System;
using System.IO;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

public class ConstellationRenderer : MonoBehaviour
{
    public List<Constellation> allConstellations; // 存放所有星座 ScriptableObject
    private Dictionary<string, Constellation> constellationMap = new Dictionary<string, Constellation>();

    private Constellation constellationData; //要顯示的ScriptableObject
    private string selectedStarSign; //被選擇的星座(一開始讀取menu、遊玩過程中選擇
    public GameObject starPrefab; // 星星的預製體
    public GameObject linePrefab; // 繪製連線用
    public Vector3 positionOffset = new Vector3(0, 0, 0); // 偏移量

    private List<GameObject> currentStars = new List<GameObject>(); // 記錄當前顯示的星星
    private List<LineRenderer> currentLines = new List<LineRenderer>(); // 記錄當前的星座連線
    public int currentRound = 0;
    public int starsToShow;
    public int maxStars = 0; // 最多顯示的星星數量

    public GameObject nextRoundButton; // 下一輪的按鈕
    public TextMeshProUGUI TMPnextBTN; //下一輪按鈕的文字，中途變換
    private bool canClickStars = false; // 是否允許點擊星星


    public string gameDifficulty; //遊戲難度
    public float starSpawnInterval = 1f; // 星星出現的間隔秒數
    public int roundsPerIncrease = 1; // 幾輪後增加星星數
    public int totalRound; // 系統後台設定的總遊玩局數
    private int countRound = 0; //協助算局數


    // 處理答題情況///////////////////////////////////////////////////////////////////////////////////
    private List<GameObject> spawnOrder = new List<GameObject>(); // 正確順序
    private int currentClickIndex = 0;
    private bool isRoundActive = false;

    private List<float> clickTimings = new List<float>();
    private float roundStartTime = 0f;

    //private List<string> rawData = new List<string>();
    private int correctRounds = 0;
    private int incorrectRounds = 0;

    public string userID = "Player01"; // 在 UI 給使用者輸入
    private List<List<string>> structuredRawData = new List<List<string>>();
    /// //////////////////////////////////////////////////////////////////////////////////////////////////

    // 最後才會顯示
    public GameObject restartButton;
    public GameObject endGameButton;

    //錯誤時變色用
    public Material spot_white;
    public Material spot_red;
    public Material star_normal;
    public Material star_red;


    /// <summary>
    /// ///////////////////////////////選關卡相關
    /// </summary>
    //處理相機和燈光角度
    public Camera mainCamera;
    public Light sceneLight;
    
    private bool isRotating = false; //換關卡的轉動
    public GameObject SelectButton; // 開啟"選關卡功能"
    public GameObject RightButton; // 往右選擇鍵
    public GameObject LeftButton; // 往左選擇鍵

    public GameObject allStarGroup; // 給選關卡用，所有星座的群組
    public GameObject chooseGroup; // 給選關卡用，選項的指示
    public TextMeshProUGUI chosedStar; //選項的指示 的文字


    public GameObject finalScore; // 顯示答對率的group
    public TextMeshProUGUI accuracyText; // 答對率字

    void Start()
    {

        foreach (var constellation in allConstellations)
        {
            constellationMap[constellation.constellationName] = constellation;
        }
        //Debug.Log(PlayerPrefs.GetString("StarSign"));
        if (nextRoundButton != null)
        {
            nextRoundButton.SetActive(false); // **開始時隱藏按鈕**
        }

        
        LoadGameSettings(); // 載入玩家設定
        StartNewRound();
    }
    void LoadGameSettings()
    {
        selectedStarSign = PlayerPrefs.GetString("StarSign", ""); // 讀取星座
        int savedStarCount = PlayerPrefs.GetInt("InitBallsNum", 2); // 讀取星星數量
        gameDifficulty = PlayerPrefs.GetString("Difficulty", ""); // 讀取難度設定
        Debug.Log(gameDifficulty);

        // 調整相機位置
        if(selectedStarSign == "小熊座")
        {
            mainCamera.transform.rotation = Quaternion.Euler(-15, 0, 0);
            sceneLight.transform.rotation = Quaternion.Euler(50, 0, 0);
        }
        else if (selectedStarSign == "巨蟹座")
        {
            mainCamera.transform.rotation = Quaternion.Euler(-15, 90, 0);
            sceneLight.transform.rotation = Quaternion.Euler(50, 90, 0);
        }
        else if (selectedStarSign == "鹿豹座")
        {
            mainCamera.transform.rotation = Quaternion.Euler(-15, 180, 0);
            sceneLight.transform.rotation = Quaternion.Euler(50, 180, 0);
        }
        else if (selectedStarSign == "鳳凰座")
        {
            mainCamera.transform.rotation = Quaternion.Euler(-15, 270, 0);
            sceneLight.transform.rotation = Quaternion.Euler(50, 270, 0);
        }

        // !!!!!!!!!!!!調整難度參數位置!!!!!!!!!!!!!
        if (gameDifficulty == "低")
        {
            totalRound = 30;
            roundsPerIncrease = 3;
            starSpawnInterval = 1f;
        }
        else if(gameDifficulty == "中")
        {
            totalRound = 40;
            roundsPerIncrease = 5;
            starSpawnInterval = 0.8f;
        }
        else if(gameDifficulty == "高")
        {
            totalRound = 50;
            roundsPerIncrease = 7;
            starSpawnInterval = 0.6f;
        }
        else if (gameDifficulty == "demo")
        {
            totalRound = 10;
            roundsPerIncrease = 1;
            starSpawnInterval = 1f;
        }
        // !!!!!!!!!!!!調整難度參數位置!!!!!!!!!!!!!


        if (constellationMap.ContainsKey(selectedStarSign))
        {
            constellationData = constellationMap[selectedStarSign];
        }
        else
        {
            Debug.LogError("找不到對應的星座：" + selectedStarSign);
            return;
        }
        maxStars = constellationData.stars.Count;
        starsToShow = savedStarCount;
        //Debug.Log("InitBallsNum: " + PlayerPrefs.GetString("StarSign", ""));
    }

    void StartNewRound()
    {
        
        currentRound++;
        ChangeStarsColor(Color.white);
        ClearStars(); // 清除上一輪的星星
        ClearLines(); // 清除上一輪的星座線
        //Debug.Log("A"+currentRound + "、" + countRound + "star:" + starsToShow);
        //if (currentRound % (roundsPerIncrease+1) == 0 && starsToShow < maxStars)
        if (countRound > roundsPerIncrease-1  && starsToShow < maxStars)
        {
            starsToShow++; // 每過 `roundsPerIncrease` 輪增加一顆星星
            countRound = 1;
        }
        else { countRound++; }
        //Debug.Log("B" + currentRound + "、" + countRound + "star:" + starsToShow);

        List<Star> shuffledStars = new List<Star>(constellationData.stars);
        ShuffleList(shuffledStars); // 隨機打亂星星

        StartCoroutine(SpawnStarsWithInterval(shuffledStars));
        //Debug.Log(starsToShow);
        //for (int i = 0; i < starsToShow && i < shuffledStars.Count; i++)
        //{
        //    Vector3 adjustedPosition = shuffledStars[i].position + positionOffset;
        //    GameObject newStar = Instantiate(starPrefab, adjustedPosition, Quaternion.identity);
        //    newStar.name = shuffledStars[i].name;
        //    currentStars.Add(newStar);
        //    Debug.Log(currentStars);
        //}

        if (nextRoundButton != null)
        {
            nextRoundButton.SetActive(false); // **新一輪開始時隱藏 Next Round 按鈕**
        }
        if (SelectButton != null)
        {
            SelectButton.SetActive(false); // **新一輪開始時隱藏 Select 按鈕**
        }
        RightButton.SetActive(false);
        LeftButton.SetActive(false);
    }

    IEnumerator SpawnStarsWithInterval(List<Star> shuffledStars)
    {
        canClickStars = false;
        for (int i = 0; i < starsToShow && i < shuffledStars.Count; i++)
        {
            Vector3 adjustedPosition = shuffledStars[i].position + positionOffset;

            // 根據星座調整旋轉角度
            Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);
            if (selectedStarSign == "小熊座")
            {
                rotation = Quaternion.Euler(-15f, 0f, 0f);
            }
            else if (selectedStarSign == "巨蟹座")
            {
                rotation = Quaternion.Euler(-15f, 90f, 0f);
            }
            else if (selectedStarSign == "鹿豹座")
            {
                rotation = Quaternion.Euler(-15f, 180f, 0f);
            }
            else if (selectedStarSign == "鳳凰座")
            {
                rotation = Quaternion.Euler(-15f, 270f, 0f);
            }

            GameObject newStar = Instantiate(starPrefab, adjustedPosition, rotation);
            newStar.name = shuffledStars[i].name;
            currentStars.Add(newStar);

            yield return new WaitForSeconds(starSpawnInterval); // **間隔出現**
        }
        canClickStars = true;

        // 處理答題情況
        roundStartTime = Time.time; // 開始計時
        clickTimings = new List<float>();
        spawnOrder = new List<GameObject>(currentStars); // 記住順序
        currentClickIndex = 0;
        isRoundActive = true;
    }


    public void OnStarClicked(GameObject star)
    {
        if (!canClickStars || !isRoundActive) return; // **還沒開放點擊**

        float reactionTime = Time.time - roundStartTime;
        

        if (spawnOrder[currentClickIndex] == star) // 答對
        {
            clickTimings.Add(reactionTime);
            currentClickIndex++;
            star.GetComponent<MeshCollider>().enabled = false;

            if (currentClickIndex >= starsToShow) // 全部點完
            {
                if (starsToShow == maxStars)
                {
                    DrawConstellationLines();
                }

                RecordRoundData(true);
                if (currentRound < totalRound)
                {
                    nextRoundButton.SetActive(true);
                    SelectButton.SetActive(true);
                }
                    
                isRoundActive = false;
            }
        }
        else // 答錯
        {
            clickTimings.Add(reactionTime); // 點錯也記錄時間
            while (clickTimings.Count < maxStars)
            {
                clickTimings.Add(0f);
            }
            ChangeStarsColor(Color.red); // 讓星星變紅
            StartCoroutine(ShakeCamera()); // 震動畫面
            // TODO: 這裡可以加點錯動畫效果
            RecordRoundData(false);
            if (currentRound < totalRound)
            {
                nextRoundButton.SetActive(true);
                SelectButton.SetActive(true);
            }
            isRoundActive = false;
            canClickStars = false;
        }
        //if (currentStars.Contains(star))
        //{
        //    currentStars.Remove(star);
        //}

        //if (currentStars.Count == 0) // 當所有星星都點擊後
        //{
        //    if(starsToShow == maxStars)
        //    {
        //        DrawConstellationLines(); // 當滿足條件時，畫出連線
        //    }
        //    nextRoundButton.SetActive(true);
        //}
    }
    // 讓外部檢查能否點擊
    public bool CanClickStars()
    {
        return canClickStars;
    }


    public void NextRound()
    {
        if (nextRoundButton != null)
        {
            nextRoundButton.SetActive(false); // **點擊後立即隱藏 Next Round 按鈕**
        }
        if(TMPnextBTN.text == "Play")
        {
            TMPnextBTN.text = "Next";
            nextRoundButton.transform.localPosition = new Vector3(-237f, -465f, 0f);
        }
        if (SelectButton != null)
        {
            SelectButton.SetActive(false); // **點擊後立即隱藏 Select 按鈕**
        }
        RightButton.SetActive(false);
        LeftButton.SetActive(false);
        chooseGroup.SetActive(false);
        // 隱藏選關顯示的星座
        allStarGroup.SetActive(false);
        ChangeShowStar();
        StartNewRound();
    }


    void ClearStars()
    {
        GameObject[] stars = GameObject.FindGameObjectsWithTag("Star");
        foreach (GameObject star in stars)
        {
            Destroy(star);
        }
        currentStars.Clear();
    }





    void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }


    void DrawConstellationLines()
    {
        foreach (Edge edge in constellationData.edges)
        {
            //if (edge.starIndex1 >= constellationData.stars.Count || edge.starIndex2 >= constellationData.stars.Count)
            //{
            //    Debug.LogWarning("無效的星星索引：" + edge.starIndex1 + " <-> " + edge.starIndex2);
            //    continue;
            //}

            Vector3 startPos = constellationData.stars[edge.starIndex1].position + positionOffset;
            Vector3 endPos = constellationData.stars[edge.starIndex2].position + positionOffset;

            GameObject lineObj = Instantiate(linePrefab, Vector3.zero, Quaternion.identity);
            LineRenderer lineRenderer = lineObj.GetComponent<LineRenderer>();
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, endPos);
            lineRenderer.startWidth = 0.08f;
            lineRenderer.endWidth = 0.08f;

            currentLines.Add(lineRenderer);
        }

    }
    void ClearLines()
    {
        foreach (LineRenderer line in currentLines)
        {
            Destroy(line.gameObject);
        }
        currentLines.Clear();
    }



    //紀錄數據
    void RecordRoundData(bool isCorrect)
    {
        List<string> row = new List<string>();
        row.Add(currentRound.ToString());

        for (int i = 0; i < maxStars; i++)
        {
            float time = (i < clickTimings.Count) ? clickTimings[i] : 0f;
            row.Add(time.ToString("F6")); // 小數點後6位
        }

        row.Add(isCorrect ? "1" : "0"); // Score

        structuredRawData.Add(row);

        if (isCorrect) correctRounds++;
        else incorrectRounds++;

        SaveDataIfNeeded();
    }

    void SaveDataIfNeeded()
    {

        if (currentRound >= totalRound)
        {
            if (nextRoundButton != null)
                nextRoundButton.SetActive(false); // 結束時關閉 NextRound 按鈕
            if (SelectButton != null)
                SelectButton.SetActive(false); // 結束時關閉 NextRound 按鈕

            if (restartButton != null)
                restartButton.SetActive(true); // 顯示 Restart 按鈕

            if (endGameButton != null)
                endGameButton.SetActive(true); // 顯示 Back to Menu 按鈕

            accuracyText.text = "答對率: " + correctRounds + " / " + totalRound;
            if (finalScore != null)
                finalScore.SetActive(true); // 顯示 Back to Menu 按鈕


            string folderName = $"{userID}_{System.DateTime.Now:yyyyMMdd_HHmmss}";
            string basePath = Application.persistentDataPath + "/ciphgameClickInOrder/" + folderName;

#if UNITY_ANDROID && !UNITY_EDITOR
            if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
            {
                Permission.RequestUserPermission(Permission.ExternalStorageWrite);
                return;
            }
            string downloadPath = "/storage/emulated/0/Download" + "/ciphgameClickInOrder/" + folderName;
            Directory.CreateDirectory(downloadPath);
#endif


            System.IO.Directory.CreateDirectory(basePath);
            Debug.Log("資料儲存於: " + basePath);
            // 寫 RawData CSV
            string rawPath = basePath + "/RawData.csv";
            List<string> lines = new List<string>();
#if UNITY_ANDROID && !UNITY_EDITOR
            string downloadRawPath = downloadPath + "/RawData.csv";
#endif

            // 標題行
            List<string> header = new List<string> { "Round" };
            for (int i = 0; i < maxStars; i++)
            {
                header.Add($"Btn{i + 1}_ReactionTime");
            }
            header.Add("Score");
            lines.Add(string.Join(",", header));

            // 資料行
            foreach (var row in structuredRawData)
            {
                lines.Add(string.Join(",", row));
            }

            System.IO.File.WriteAllLines(rawPath, lines);
#if UNITY_ANDROID && !UNITY_EDITOR
            System.IO.File.WriteAllLines(downloadRawPath, lines);
#endif

            // 寫 ProcessedData
            string processedPath = basePath + "/ProcessedData.csv";
#if UNITY_ANDROID && !UNITY_EDITOR
            string downloadProcessedPath = downloadPath + "/ProcessedData.csv";
#endif

            float accuracy = (correctRounds + incorrectRounds) > 0 ? ((float)correctRounds / (correctRounds + incorrectRounds)) : 0f;
            List<string> summary = new List<string>
        {
            "CorrectRounds,IncorrectRounds,Accuracy",
            $"{correctRounds},{incorrectRounds},{accuracy:F2}"
        };
            System.IO.File.WriteAllLines(processedPath, summary);
#if UNITY_ANDROID && !UNITY_EDITOR
            System.IO.File.WriteAllLines(downloadProcessedPath, summary);
#endif
        }
    }

    // 點錯相關: 星星變色
    void ChangeStarsColor(Color color)
    {
        foreach (var star in currentStars)
        {
            var renderer = star.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material currentMat = renderer.sharedMaterial;

                if (currentMat == spot_white)
                {
                    renderer.material = spot_red;
                }
                else if (currentMat == star_normal)
                {
                    renderer.material = star_red;
                }
            }
        }
    }

    IEnumerator ShakeCamera()
    {
        float shakeDuration = 0.3f;
        float shakeMagnitude = 0.8f;
        float elapsed = 0f;

        // 記住原本的位置
        Dictionary<GameObject, Vector3> originalPositions = new Dictionary<GameObject, Vector3>();
        foreach (GameObject star in currentStars)
        {
            if (star != null)
                originalPositions[star] = star.transform.position;
        }

        while (elapsed < shakeDuration)
        {
            foreach (GameObject star in currentStars)
            {
                if (star != null)
                {
                    Vector3 randomOffset = new Vector3(
                        UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude),
                        UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude),
                        UnityEngine.Random.Range(-shakeMagnitude, shakeMagnitude)
                    );
                    star.transform.position = originalPositions[star] + randomOffset;
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 抖完回到原本的位置
        foreach (GameObject star in currentStars)
        {
            if (star != null && originalPositions.ContainsKey(star))
                star.transform.position = originalPositions[star];
        }
    }


    //關於轉換關卡
    public void RotateLeft()
    {
        if (!isRotating)
            StartCoroutine(RotateOverTime(-90));
    }

    public void RotateRight()
    {
        if (!isRotating)
            StartCoroutine(RotateOverTime(90));
    }

    private IEnumerator RotateOverTime(float angle)
    {
        isRotating = true;

        Quaternion startRotationCamera = mainCamera.transform.rotation;
        Quaternion startRotationLight = sceneLight.transform.rotation;

        // 計算目標旋轉
        Vector3 currentEulerCamera = mainCamera.transform.eulerAngles;
        Vector3 currentEulerLight = sceneLight.transform.eulerAngles;

        Vector3 targetEulerCamera = new Vector3(currentEulerCamera.x, currentEulerCamera.y + angle, currentEulerCamera.z);
        Vector3 targetEulerLight = new Vector3(currentEulerLight.x, currentEulerLight.y + angle, currentEulerLight.z);


        Quaternion endRotationCamera = Quaternion.Euler(targetEulerCamera);
        Quaternion endRotationLight = Quaternion.Euler(targetEulerLight);

        float elapsed = 0f;
        float rotationDuration = 1f; //旋轉秒數
        while (elapsed < rotationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / rotationDuration);

            mainCamera.transform.rotation = Quaternion.Slerp(startRotationCamera, endRotationCamera, t);
            sceneLight.transform.rotation = Quaternion.Slerp(startRotationLight, endRotationLight, t);

            yield return null;
        }

        // 確保結束時角度精確
        mainCamera.transform.rotation = endRotationCamera;
        sceneLight.transform.rotation = endRotationLight;

        isRotating = false;
        float yRotation = Mathf.Round(mainCamera.transform.rotation.eulerAngles.y);
        if (yRotation == 0f)
        {
            chosedStar.text = "小熊座";
        }
        else if (yRotation == 90f)
        {
            chosedStar.text = "巨蟹座";
        }
        else if (yRotation == 180f)
        {
            chosedStar.text = "鹿豹座";
        }
        else if (yRotation == 270f || yRotation == -90f)
        {
            chosedStar.text = "鳳凰座";
        }
    }

    // 選關按鍵
    public void ClickSelectBTN()
    {
        ClearStars(); // 清除上一輪的星星
        ClearLines(); // 清除上一輪的星座線
        allStarGroup.SetActive(true);
        RightButton.SetActive(true);
        LeftButton.SetActive(true);
        chooseGroup.SetActive(true);
        SelectButton.SetActive(false);
        TMPnextBTN.text = "Play";
        nextRoundButton.transform.localPosition = new Vector3(0f, -465f, 0f);

        float yRotation = Mathf.Round(mainCamera.transform.rotation.eulerAngles.y);
        if (yRotation == 0f)
        {
            chosedStar.text = "小熊座";
        }
        else if (yRotation == 90f)
        {
            chosedStar.text = "巨蟹座";
        }
        else if (yRotation == 180f)
        {
            chosedStar.text = "鹿豹座";
        }
        else if (yRotation == 270f || yRotation == -90f)
        {
            chosedStar.text = "鳳凰座";
        }

        constellationData = constellationMap["小熊座"];
        DrawConstellationLines();
        constellationData = constellationMap["巨蟹座"];
        DrawConstellationLines();
        constellationData = constellationMap["鹿豹座"];
        DrawConstellationLines();
        constellationData = constellationMap["鳳凰座"];
        DrawConstellationLines();
    }

    private void ChangeShowStar()
    {
        float yRotation = Mathf.Round(mainCamera.transform.rotation.eulerAngles.y);
        if (yRotation == 0f)
        {
            selectedStarSign = "小熊座";
            constellationData = constellationMap[selectedStarSign]; 
        }
        else if (yRotation == 90f)
        {
            selectedStarSign = "巨蟹座";
            constellationData = constellationMap[selectedStarSign];
        }
        else if (yRotation == 180f)
        {
            selectedStarSign = "鹿豹座";
            constellationData = constellationMap[selectedStarSign];
        }
        else if (yRotation == 270f || yRotation == -90f)
        {
            selectedStarSign = "鳳凰座";
            constellationData = constellationMap[selectedStarSign];
        }
        Debug.Log("更換成: " + selectedStarSign);
    }
}

