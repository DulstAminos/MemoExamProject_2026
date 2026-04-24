using UnityEngine;

public class ItemVisual : MonoBehaviour
{
    void Update()
    {
        // 道具旋转
        transform.Rotate(Vector3.up * 50 * Time.deltaTime);
        // 道具上下移动
        float newY = transform.localPosition.y + Mathf.Sin(Time.time * 2f) * 0.001f * Time.timeScale;
        transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
    }
}