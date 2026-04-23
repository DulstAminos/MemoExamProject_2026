using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public Image fillImage;        // 绿/红色的填充图
    public float hideDelay = 3f;   // 几秒后隐藏

    public CanvasGroup canvasGroup;
    private float lastDamageTime;
    private Transform mainCamera;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        mainCamera = Camera.main.transform;
    }

    private void Start()
    {
        // 初始显示血条
        canvasGroup.alpha = 1;
        lastDamageTime = Time.time;
    }

    private void Update()
    {
        // 让血条永远朝向摄像机 (Billboard效果)
        transform.forward = mainCamera.forward;

        // 一段时间未受伤，隐藏血条
        if (Time.time - lastDamageTime > hideDelay && canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * 2f; // 渐隐
        }
    }

    // 在你的玩家/敌人受击逻辑中调用此方法
    public void UpdateHealth(float currentHealth, float maxHealth)
    {
        float ratio = Mathf.Clamp01(currentHealth / maxHealth);
        fillImage.fillAmount = ratio;

        // 颜色渐变：血多绿，血少红
        fillImage.color = Color.Lerp(Color.red, Color.green, ratio);

        // 重新显示血条
        canvasGroup.alpha = 1;
        lastDamageTime = Time.time;
    }
}