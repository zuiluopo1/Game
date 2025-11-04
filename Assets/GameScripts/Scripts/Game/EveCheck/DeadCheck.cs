using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeadCheck : MonoBehaviour
{
    //用于伤害检测时 真正调用的函数体的委托
    public event UnityAction CheckCallback;

    /// <summary>
    /// 用于给动画事件关联的函数
    /// </summary>
    public void CheckDead()
    {
        //调用委托
        CheckCallback?.Invoke();
    }
}
