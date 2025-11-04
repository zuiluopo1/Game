using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 回调
/// </summary>
public class DmgCheck : MonoBehaviour
{
    //用于伤害检测时 真正调用的函数体的委托
    public event UnityAction<int> CheckCallback;

    //用于存储在死亡时真正调用的事件
    public event UnityAction checkDead;

    /// <summary>
    /// 用于给动画事件关联的函数
    /// </summary>
    public void CheckDemg(int id)
    {
        //调用委托
        CheckCallback?.Invoke(id);
    }

    /// <summary>
    /// 用于死亡动画关联 的函数
    /// </summary>
    public void CheckDead()
    {
        checkDead?.Invoke();
    }
}
