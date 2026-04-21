using UnityEngine;

public class ItemVisual : MonoBehaviour
{
    void Update()
    {
        // 自转
        transform.Rotate(Vector3.up * 50 * Time.deltaTime);
        // 上下漂浮 (波浪效果)
        float newY = transform.localPosition.y + Mathf.Sin(Time.time * 2f) * 0.001f;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }
}
