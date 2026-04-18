using UnityEngine;

public class BulletBase : MonoBehaviour
{
    public float speed = 15f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // 子弹生成时，立刻给予向前的速度
        rb.velocity = transform.forward * speed;

        // 临时做法：3秒后自动销毁。
        Destroy(gameObject, 3f);
    }
}