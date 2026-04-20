using UnityEngine;

public class Health : MonoBehaviour
{
    public float maxHealth = 30f;
    private float currentHealth;

    // 之后加入爆炸特效预制体
    // public GameObject explosionPrefab;

    void OnEnable()
    {
        currentHealth = maxHealth; // 每次激活时回满血
    }

    public void TakeDamage(float damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " 受到了伤害！剩余血量：" + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // 之后实现爆炸特效生成
        Debug.Log(gameObject.name + " 被摧毁了！");

        // 回收当前坦克
        // 注意：如果是玩家死亡，之后要由 GameManager 弹出失败界面，先留个坑
        if (gameObject.CompareTag("Player"))
        {
            gameObject.SetActive(false); // 玩家直接隐藏
        }
        else
        {
            PoolManager.Instance.Despawn(gameObject); // 敌人用对象池回收
        }
    }
}