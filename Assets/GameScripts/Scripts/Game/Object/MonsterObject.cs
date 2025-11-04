using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MonsterObject : RoleObject
{
    //专门用于处理Ai逻辑的对象
    private AIlogic ailogic;
    //怪物出生点
    public Vector3 bornPos;

    public int deadexp;

    protected override void Awake()
    {
        base.Awake(); 
        roleType=E_Role_Type.Monster;

        //测试初始化属性方法
        //InitProperty(1);
    }
    protected override void Update()
    {
        base.Update();
        ailogic.UpdateAI();
    }


    public override void InitProperty(int id)
    {
        //读取怪物目的是 初始化ID
        T_MonsterLev_Info Info = BinaryDataMgr.Instance.GetTable<T_MonsterLev_InfoContainer>().dataDic[id];
        
        //初始化AI逻辑
        ailogic = new AIlogic(this, Info.f_ai);

        //怪物初始化数据
        property = new MonsterProperty(id);
        //记录怪物刚出现的位置
        bornPos = this.transform.position;
    }

    //受伤和击飞在基类
    //攻击
    public override void Atk()
    {
        //切换攻击状态即可，不需要考虑其他互斥    
        ChangAction(E_Action_type.Atk1);
    }

    public override void Hit(float hitTime)
    {
        //  如果受伤时处于击飞状态就没有必要处理受伤逻辑
        if (roleAnimator.GetBool("isHitFly"))
            return;

        //处理对应的随机伤害动作逻辑
        roleAnimator.SetInteger("hitStart", Random.Range(1, 4));

        //清零延时 受伤又受伤则清零上一次延时
        CancelInvoke("DelayClealhit");
        //延时函数 来处理 过一段时间再结束受伤状态
        //切换受伤动作
        ChangAction(E_Action_type.Hit);
        //roleAnimator.SetBool("isHit", true);
        Invoke("DelayClealhit", hitTime);

    }
    //移动
    public void Move(Vector2 dir)
    {
        //改变移动方向 在父类的update逻辑中 就会去处理移动相关的逻辑
        move=dir;
    }
    //死亡
    public override void Dead()
    {
        isDead=true;
        ChangAction(E_Action_type.Dead);

        //在怪物死亡中播放音效
        MusicMgr.GetInstance().PlaySound((property as MonsterProperty).deadSound, false);

        //分发怪物死亡事件
        EventCenter.GetInstance().EventTrigger("MonsterDead",(property as MonsterProperty).type);
    }

    /// <summary>
    /// 当对象死亡动画结束时 调用的内容 怪物消失
    /// </summary>
    protected override void CheckDead()
    {
        //throw new System.NotImplementedException();
        PoolMgr.GetInstance().PushObj(this.gameObject.name,this.gameObject);
        //为了避免在缓存池中取出后攻击不到的问题
        isDead=false;
    }
    protected override void ChackBodyDir()
    {
        //怪物的身体朝向 始终应该朝向玩家
        //base.ChackBodyDir();

        //如果死亡则不在转向
        if (isDead) 
            return;

        //巡逻和回归状态时 用的面朝移动方向逻辑
        if(ailogic.nowAIstate == E_AI_STATE.PATROL_STATE || 
           ailogic.nowAIstate == E_AI_STATE.BACK_STATE)
        {
            base.ChackBodyDir();
        }
        //攻击状态和和移动状态时 就始终面朝玩家
        else
        {
            //让怪物一直朝向玩家
            if (PlayerObject.player != null)
            {
                if (this.transform.position.x - PlayerObject.player.transform.position.x >= 0)
                {
                    roleSprite.flipX = true;
                }
                else if (this.transform.position.x - PlayerObject.player.transform.position.x <= 0)
                {
                    roleSprite.flipX = false;
                }
            }
        }
    }
}



