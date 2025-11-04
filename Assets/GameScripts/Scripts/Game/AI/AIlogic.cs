using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_AI_STATE
{
    NULL,
    PATROL_STATE,//巡逻状态
    MOVE_STATE,//移动状态
    ATK_STATE,//关机状态
    BACK_STATE,//脱离状态（回到出生点）
}

/// <summary>
/// 专门处理Ai逻辑类
/// </summary>
public class AIlogic 
{
    //Ai逻辑控制的怪物对象
    public MonsterObject monster;

    ////AI状态巡逻对象
    //private AIStatePatrol patrolState;
    //private AIStateMove moveState;
    //private AIStateAtk atkState;
    //private AIStateBack backState;

    //用一个字典记录AI逻辑中的所有AI状态 也可以用上面那种写法
    //方便获取到每一个状态的对象
    private Dictionary<E_AI_STATE,AIStateBase> stateDic=new Dictionary<E_AI_STATE, AIStateBase>();

    //当前Ai状态 并初始化
    public E_AI_STATE nowAIstate=E_AI_STATE.NULL;

    private AIStateBase nowState;
    //我们的AI数据对象
    public T_AI_Info aiInfo;

    /// <summary>
    /// 初始化 Ai逻辑对象时 就应该告诉他 你控制的是谁
    /// </summary>
    /// <param name="monster"></param>
    public AIlogic(MonsterObject monster,int id)
    {
        this.monster = monster;
        //读取Ai表中的 对应的ID的数据
        aiInfo = BinaryDataMgr.Instance.GetTable<T_AI_InfoContainer>().dataDic[id];

        //添加一个巡逻状态
        stateDic.Add(E_AI_STATE.PATROL_STATE, new AIStatePatrol(this));
        //移动
        stateDic.Add(E_AI_STATE.MOVE_STATE, new AIStateMove(this));
        //攻击
        stateDic.Add(E_AI_STATE.ATK_STATE, new AIStateAtk(this));
        //退出
        stateDic.Add(E_AI_STATE.BACK_STATE, new AIStateBack(this));

        ChangAIState(E_AI_STATE.PATROL_STATE);
    }

    public void UpdateAI()
    {
        nowState.UpdateAIState();
    }

    //切换Ai状态
    public void ChangAIState(E_AI_STATE state)
    {
        //1.在记录当前状态之前 应该 执行一些上一个状态的退出方法
        if(nowAIstate != E_AI_STATE.NULL)
            stateDic[nowAIstate].ExitAIState();

        //2.进入一个新状态 应该执行新状态 进入的方法 
        this.nowAIstate = state;

        //3.进入要切换到的状态
        stateDic[nowAIstate].EnterAIState();

        //记录当前AI状态的对象
        nowState=stateDic[nowAIstate];
    }
}

