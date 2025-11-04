using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 巡逻目标编号
/// </summary>
public enum E_PATROL_KINE1
{
    //左
    LEFT,
    //右
    RIGHT,
    //上
    UP,
    //下
    DOWN, 
}

//巡逻Ai状态处理类
public class AIStatePatrol : AIStateBase
{
    //巡逻方式 0为按点 1为按边
    private int patrol_type = 1;

    //巡逻范围 写死 之后在改
    private int rangew ;
    private int rangeh;
    
    //当前巡逻目标边的编号 按边巡逻
    private E_PATROL_KINE1 kind1index = E_PATROL_KINE1.LEFT;
    //第二钟巡逻方式 按点巡逻
    private List<Vector3> targetpoes = new List<Vector3>();
    //当前选择第几个点
    private int nowindex = 0;
    //是加还是减
    private bool isadd=true;

    private float wiatTime;

    private int atkRange;
    //目标点
    private Vector3  targetPos;
    //是否到达
    private bool isArrive=true;
    //是否在等待
    private bool isWait=false;
    //记录玩家对象
    //private PlayerObject player;
    public AIStatePatrol(AIlogic logic) :base(logic)
    {
        //初始化巡逻的3个点
        //targetpoes.Add(new Vector3(6.53f, -0.84f, 0));
        //targetpoes.Add(new Vector3(4.18f, -2.45f, 0));
        //targetpoes.Add(new Vector3(7.41f, -1.86f, 0));

        //巡逻状态赋值
        patrol_type= logic.aiInfo.f_patrol_type;
        //根据巡逻状态不同赋值不同的变量
        if(patrol_type == 0)
        {
            //第一次分割字符串 得到n个带点
            string[] points= ailogic.aiInfo.f_patrol_point.Split(';');
            for(int i = 0; i < points.Length; i++)
            {
                //分割每个点的坐标 进行赋值
                string[] point = points[i].Split(",");
                //得到一个点的赋值 xyz 
                targetpoes.Add(new Vector3(float.Parse(point[0]),float.Parse(point[1]),float.Parse(point[2])));
            }
        }
        else
        {
            string[] rangWH=ailogic.aiInfo.f_patrol_rangew.Split(",");
            rangew = int.Parse(rangWH[0]);
            rangeh = int.Parse(rangWH[1]);
        }
        //等待时间
        wiatTime=ailogic.aiInfo.f_patrol_waitTime;
        //脱离巡逻状态 距离玩家多少
        atkRange=ailogic.aiInfo.f_patrol_moveRange;
    }
    public override void EnterAIState()
    {
        //每次切换状态时 刚切换时会执行的逻辑
        //player = GameObject.Find("Player").GetComponent<PlayerObject>();
    }

    public override void ExitAIState()
    {
        //每次从该状态切换到别的状态时 需要切换的逻辑
        //player=null;
    }

    public override void UpdateAIState()
    {
        //去处理 巡逻状态的逻辑
        //每帧都会进入

        //是否在等待
        if (!isWait)
        {
            //已经到达目标点状态 就重新获取目标点
            if (isArrive)
            {
                GetTargetPos();
                isArrive = false;
            }
            //如果没有到就一直朝目标点移动
            else
            {
                //只需要不停的告诉怪物朝能够方向移动
                ailogic.monster.Move(targetPos - ailogic.monster.transform.position);
                //判断当前位置和目标点位置 小于一定范围后 就认为他到达了
                //而表示直接判断==
                if (Vector2.Distance(ailogic.monster.transform.position, targetPos) < 0.5f)
                {
                    //让玩家停下来
                    ailogic.monster.Move(Vector3.zero);

                    isArrive = true;//到达目标点
                    isWait = true;
                    //利用小框架中的Mono管理器 处理 没有延时的功能
                    MonoMgr.GetInstance().StartCoroutine(WaitMove());
                }
            }
        }

        //在巡逻状态时 应该不停的判断和玩家之间距离 如果玩家进入怪物的地盘，就进行攻击
        //1.能够得到玩家对象
        //1-1.从场景中找到Player对象
        //if(Vector3.Distance(ailogic.monster.transform.position, player.transform.position) <= atkRange)
        //{
        //    //切换Ai到move状态
        //    ailogic.ChangAIState(E_AI_STATE.MOVE_STATE);
        //}
        //1-2.Playobject 中用应该变量去找
        if (PlayerObject.player == null)
            return;

        if (Vector3.Distance(ailogic.monster.transform.position, PlayerObject.player.transform.position) <= atkRange)
        {
            //停止移动
            ailogic.monster.Move(Vector2.zero);
            //切换Ai到move状态
            ailogic.ChangAIState(E_AI_STATE.MOVE_STATE);
        }
        //1-3.通过管理器 管理玩家对象 从管理器中获取


    }

    private IEnumerator WaitMove()
    {
        yield return new WaitForSeconds(wiatTime);
        isWait = false;
    }

    //获取目标点
    private void GetTargetPos()
    {
        switch (patrol_type)
        {
            case 0:
                gettargetype0();
                break;
            case 1:
                gettargetype1();
                break;
        }

        //随机选择点
        //targetPos.x = Random.Range(ailogic.monster.bornPos.x - rangeh, ailogic.monster.bornPos.x + rangeh);
        //targetPos.y = Random.Range(ailogic.monster.bornPos.y - rangew, ailogic.monster.bornPos.y + rangew);
        //targetPos.z = ailogic.monster.transform.position.z;
    }
    //按点获取巡逻点
    private void gettargetype0()
    {
        //按点巡逻
        targetPos = targetpoes[nowindex];
        nowindex = isadd ? nowindex + 1 : nowindex - 1;
        if (nowindex == targetpoes.Count)
        {
            isadd = false;
            nowindex = targetpoes.Count - 1;
        }
        else if (nowindex == -1)
        {
            isadd = true;
            nowindex = 0;
        }
    }
    //按边获取巡逻点
    private void gettargetype1()
    {
        //按边巡逻
        switch (kind1index)
        {
            case E_PATROL_KINE1.LEFT:
                //左
                targetPos.x = ailogic.monster.bornPos.x;
                targetPos.y = Random.Range(ailogic.monster.bornPos.y - rangew, ailogic.monster.bornPos.y + rangew);
                break;
            case E_PATROL_KINE1.RIGHT:
                //右
                targetPos.x = ailogic.monster.bornPos.x + rangeh;
                targetPos.y = Random.Range(ailogic.monster.bornPos.y - rangew, ailogic.monster.bornPos.y + rangew);
                break;
            case E_PATROL_KINE1.UP:
                //上
                targetPos.x = Random.Range(ailogic.monster.bornPos.x - rangeh, ailogic.monster.bornPos.x + rangeh);
                targetPos.y = ailogic.monster.bornPos.y + rangew;
                break;
            case E_PATROL_KINE1.DOWN:
                //下
                targetPos.x = Random.Range(ailogic.monster.bornPos.x - rangeh, ailogic.monster.bornPos.x + rangeh);
                targetPos.y = ailogic.monster.bornPos.y - rangeh;
                break;
            default:
                break;
        }

        kind1index++;
        if ((int)kind1index > 3)
        {
            kind1index = E_PATROL_KINE1.LEFT;
        }
    }
}

