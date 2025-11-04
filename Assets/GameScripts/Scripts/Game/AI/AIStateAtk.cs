using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIStateAtk : AIStateBase
{
    //第一次攻击前的等待时间
    private float atkWaitTime ;
    //每次攻击前的的一个等待时间
    private float atkCDTimemin ;
    private float atkCDTimemax ;

    //攻击的范围 x与y
    private float atkRangeX ;
    private float atkRangeY ;

    //是否能攻击
    private bool canAtk=false;
    public AIStateAtk(AIlogic logic) : base(logic)
    {
        //初始化 攻击AI状态的数据
        atkWaitTime = ailogic.aiInfo.f_atk_fistWaitTime;
        string[] strs = ailogic.aiInfo.f_atk_cdTime.Split(',');
        atkCDTimemin=float.Parse(strs[0]);
        atkCDTimemax=float.Parse(strs[1]);

        strs = ailogic.aiInfo.f_atk_Range.Split(",");
        atkRangeX=float.Parse(strs[0]);
        atkRangeY=float.Parse(strs[1]);
    }
    public override void EnterAIState()
    {
        Debug.Log("进入攻击状态，可以攻击了");
        canAtk = false;
        //小框架里写好的mono管理器
        MonoMgr.GetInstance().StartCoroutine(AtkWaitTime());
    }

    public override void ExitAIState()
    {
    }

    public override void UpdateAIState()
    {
        if(PlayerObject.player==null)
            return;

        //在攻击状态 时要不停的检测玩家是否在攻击的范围内 如果不在就切换移动状态
        //判断y的范围内就切换
        if (Mathf.Abs(ailogic.monster.transform.position.y - PlayerObject.player.transform.position.y) > atkRangeY ||
            (ailogic.monster.BodyISRight && //当身体朝右且玩家在前方位置较远 或者在后面 所以就无法打中
            (PlayerObject.player.transform.position.x - ailogic.monster.transform.position.x > atkRangeX ||
            PlayerObject.player.transform.position.x < ailogic.monster.transform.position.x) ||
            (!ailogic.monster.BodyISRight && //当身体朝左 且玩家在前方位置较远 或者在后面 所以就无法打中
            (ailogic.monster.transform.position.x - PlayerObject.player.transform.position.x > atkRangeX ||
            PlayerObject.player.transform.position.x > ailogic.monster.transform.position.x)
            )))
        {
            ailogic.ChangAIState(E_AI_STATE.MOVE_STATE);
        }

        //不停 的让怪物攻击
        if (canAtk)
        {
            ailogic.monster.Atk();
            canAtk=false;

            MonoMgr.GetInstance().StartCoroutine(AtkCDTime());
        }
    }
    /// <summary>
    /// 第一次进攻的计时
    /// </summary>
    /// <returns></returns>
    private IEnumerator AtkWaitTime()
    {
        yield return new WaitForSeconds(atkWaitTime);
        canAtk = true;
    }
    /// <summary>
    /// 每一次攻击的计时
    /// </summary>
    /// <returns></returns>
    private IEnumerator AtkCDTime()
    {
        //随机攻击时间
        yield return new WaitForSeconds(Random.Range(atkCDTimemin,atkCDTimemax));
        canAtk = true;
    }
}
