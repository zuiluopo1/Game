using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateMove : AIStateBase
{
    //目标点
    private Vector3 targetPos;
    //第一次移动时进行等待
    private float moveWaitTime;
    //是否进入移动
    private bool isWait=true;
    //允许的最大活动距离 超过则返回 在巡逻
    private float maxDis;

    public AIStateMove(AIlogic logic) : base(logic)
    {
        //初始化时 获取表中内容
        moveWaitTime=logic.aiInfo.f_move_WaitTime;
        //最大的攻击活动范围
        maxDis = logic.aiInfo.f_back_maxDis;
    }
    public override void EnterAIState()
    {
        Debug.Log("已经进入啦");
        isWait = true;
        MonoMgr.GetInstance().StartCoroutine(MoveWaitTime());
    }

    public override void ExitAIState()
    {
    }

    public override void UpdateAIState()
    {
        //我们应该在朝玩家移动时 不停的检测 是否超过我们的 活动范围 超过了则回到出生点
        //怪物和出生点的距离 超过了一定范围则切换回归状态
        if (Vector2.Distance(ailogic.monster.transform.position, ailogic.monster.bornPos) > maxDis)
        {
            //让怪物停下来
            ailogic.monster.Move(Vector2.zero);
            //切换到出生点
            ailogic.ChangAIState(E_AI_STATE.BACK_STATE);
            return;
        } 

        if (isWait) 
            return;

        if(PlayerObject.player==null)
            return;
        //在移动时 怪物的表现
        //怪物 要移动到玩家的面前去攻击
        if(ailogic.monster.transform.position.x > PlayerObject.player.transform.position.x)
        {
            targetPos = PlayerObject.player.transform.position + Vector3.right*1.6f;
        }
        else
        {
            targetPos = PlayerObject.player.transform.position - Vector3.right * 1.6f;

        }

        //获取移动方向的向量 朝目标点移动即可
        ailogic.monster.Move(targetPos - ailogic.monster.transform.position);

        //到达目标一定范围内就可以攻击了
        if(Vector3.Distance(ailogic.monster.transform.position, targetPos) < 0.1f)
        {
            //停下来 在切换之前停
            ailogic.monster.Move(Vector2.zero);

            //为了让怪物不卡状态 一个让其强制让怪物面朝玩家
    
            //切换攻击状态
            ailogic.ChangAIState(E_AI_STATE.ATK_STATE);
        }
    }
    private IEnumerator MoveWaitTime()
    {
        yield return new WaitForSeconds(moveWaitTime);
        isWait = false;
    }
}
