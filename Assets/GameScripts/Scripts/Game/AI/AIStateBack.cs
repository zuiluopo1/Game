using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateBack : AIStateBase
{
    public AIStateBack(AIlogic logic) : base(logic)
    {

    }
    public override void EnterAIState()
    {
        Debug.Log("切换到回归状态");
    }

    public override void ExitAIState()
    {
    }

    public override void UpdateAIState()
    {
        //就应该不停的回到出生点
        //回到出生点后 应该切换到巡逻状态

        //1.移动
        //得到回到出生点的移动方向
        //命令怪物朝这个方向移动
        ailogic.monster.Move(ailogic.monster.bornPos - ailogic.monster.transform.position);

        //2.停止移动
        if (Vector2.Distance(ailogic.monster.bornPos, ailogic.monster.transform.position) < 0.1f)
        {
            //停下来
            ailogic.monster.Move(Vector2.zero);
            //且换巡逻状态
            ailogic.ChangAIState(E_AI_STATE.PATROL_STATE);
        }
    }
}


