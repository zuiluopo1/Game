using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 属性基类
/// </summary>
public class BaseProperty 
{
    /// <summary>
    /// 攻击
    /// </summary>
    public int atk;
    /// <summary>
    /// 防御
    /// </summary>
    public int def;
    /// <summary>
    /// 最大血量
    /// </summary>
    public int maxhp;
    /// <summary>
    /// 当前血量
    /// </summary>
    public int nowhp;
    /// <summary>
    /// 闪避率
    /// </summary>
    public int miss;
    /// <summary>
    /// 暴击率
    /// </summary>
    public int crit;
    /// <summary>
    /// 移动速度
    /// </summary>
    public int speed;
    /// <summary>
    /// 暴击伤害加成
    /// </summary>
    public int critNum;
    public BaseProperty(int infoid)
    {

    }
}
