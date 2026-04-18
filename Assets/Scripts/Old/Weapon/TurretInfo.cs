using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 炮台预制体信息类（挂载在每一个炮台预制体上）
/// 存储炮台类型、使用的炮弹类型、所有炮口位置
/// </summary>
public class TurretInfo : MonoBehaviour
{
    // 炮台种类
    public TurretType turretType;

    // 炮弹种类
    public BulletType bulletType;

    // 炮口位置列表（支持多管炮，可在面板拖拽多个炮口节点）
    public List<Transform> muzzlePoints;
}
