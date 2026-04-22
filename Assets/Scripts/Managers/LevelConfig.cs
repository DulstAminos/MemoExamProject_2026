using UnityEngine;

[CreateAssetMenu(fileName = "LevelConfig", menuName = "Configs/LevelConfig")]
public class LevelConfig : ScriptableObject
{
    public string sceneName;
    public int enemyCount; // 初始敌人数量
    public float star3Time; // 3星所需时间（如 <30s）
    public float star2Time; // 2星所需时间（如 <60s）
}