using System.Collections;
using UnityEngine;

public class CircleToStar : MonoBehaviour
{
    //public Sprite starSprite; // 拖入 星星 圖片
    //private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;
    private bool isTransforming = false;
    private bool isClicked = false; // 紀錄是否已經被點擊
    private ConstellationRenderer constellationRenderer; // 用來通知主控制腳本

    public Material starMaterial; // 點擊後要換上的材質
    public Material ballMaterial; // 原先球的材質
    private Renderer rend; //球的屬性


    void Start()
    {
        //spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale; // 記錄原始大小
        rend = GetComponent<Renderer>();
        constellationRenderer = FindObjectOfType<ConstellationRenderer>(); // 自動尋找遊戲內的 ConstellationRenderer
    }

    void OnMouseDown()
    {
        if (constellationRenderer != null && !constellationRenderer.CanClickStars())
        {
            return;
        }
        if (!isTransforming)
        {
            StartCoroutine(TransformToStar());
        }
    }

    IEnumerator TransformToStar()
    {
        isTransforming = true;
        isClicked = true; // 標記為已點擊
        float duration = 0.4f; // 縮小時間
        float elapsedTime = 0f;
        Vector3 targetScale = originalScale * 0.2f; // 變小 80%

        // 逐漸縮小
        while (elapsedTime < duration)
        {
            transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale; // 確保最後尺寸正確

        yield return new WaitForSeconds(0.1f); // 小延遲（0.1 秒）

        // 變成 星星
        if (starMaterial != null)
        {
            //spriteRenderer.sprite = starSprite;
            rend.material = starMaterial;
            float randomScale = Random.Range(2f, 2.3f);
            transform.localScale *= randomScale;
        }

        yield return new WaitForSeconds(0.2f); // 讓玩家有點時間看到變形

        // **通知 ConstellationRenderer 這顆星已經被點擊**
        if (constellationRenderer != null)
        {
            constellationRenderer.OnStarClicked(gameObject);
        }
        gameObject.tag = "Star";
        isTransforming = false;
    }
}
