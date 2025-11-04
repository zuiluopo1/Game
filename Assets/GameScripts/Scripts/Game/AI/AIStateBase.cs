using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Ai状态处理基类
public abstract class AIStateBase 
{
    //记入AI逻辑对象
    protected  AIlogic ailogic;

    /// <summary>
    /// 构造函数 用于记录Ai逻辑对象 主要方便获取Monster 以及进行状态的切换
    /// </summary>
    /// <param name="logic"></param>
    public AIStateBase(AIlogic logic)
    {
        this.ailogic = logic;
    }

    //进入AI状态时 处理什么逻辑
    public abstract void EnterAIState();

    //更新AI状态
    public abstract void UpdateAIState();

    //离开AI状态时是什么逻辑
    public abstract void ExitAIState();
}

