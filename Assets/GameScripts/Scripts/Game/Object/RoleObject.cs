using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

//角色阵营
public enum E_Role_Type
{
    Player,//玩家类型
    Monster,//怪物类型
}

//角色对象基类之后的 怪物 玩家 都继承他
//因为都有共同点
public abstract class RoleObject : MonoBehaviour
{
    //角色的移动方向
    protected Vector2 move=Vector2.zero;
    //移动速度 配表后可以不要
    //public int speed = 3;
    //身体sprite
    protected SpriteRenderer roleSprite;
    //影子
    protected Transform Showdow;
    //角色 Animator
    protected Animator roleAnimator;
    //身体对象
    protected Transform bodyTransfrom;


    //当前跳跃速度
    protected float nowYSpead;
    //击退速度
    protected float nowXSpead;
    public float gSpead = 9.8f;

    protected DmgCheck dmgCheck;

    public E_Role_Type roleType;

    //对象的属性类 里面有相关内容
    public BaseProperty property;

    //移动之前 上一次所在的位置
    protected Vector3 frontPos;

    /// <summary>
    /// 是否死亡
    /// </summary>
    public bool isDead = false;

    protected virtual void Awake()
    {
        //上面创建后要在这里写挂载
        roleSprite = this.transform.Find("Role").GetComponent<SpriteRenderer>();
        roleAnimator =this.GetComponentInChildren<Animator>();
        bodyTransfrom = this.transform.Find("Role");
        Showdow = this.transform.Find("Square");

        //可以得到子对象伤害检测的事件监听的脚本 然后进行处理
        dmgCheck = this.GetComponentInChildren<DmgCheck>();
        dmgCheck.CheckCallback += CheckDmg;
        //死亡订阅 监听
        dmgCheck.checkDead += CheckDead;
    }

    protected virtual void Update()
    {
        //处理移动
        CheckMove();
        //处理击飞跳跃
        CheckJumpOrHitFly();
        //处理身体朝向
        ChackBodyDir();
    }

    /// <summary>
    /// 初始化属性信息
    /// </summary>
    /// <param name="id"></param>
    public abstract void InitProperty(int id);

    //获取对象身体是不是朝右
    public bool BodyISRight
    {
        get
        {
            return !roleSprite.flipX;
        }
    }

    //检测处理跳跃击飞相关的逻辑
    protected void CheckJumpOrHitFly()
    {
        //处理跳跃逻辑
        //不是在地面就是在跳跃
        if (!roleAnimator.GetBool("isGround"))
        {
            //跳跃身体对象
            bodyTransfrom.Translate(Vector2.up * nowYSpead * Time.deltaTime);
            //数值上抛 下落逻辑 速度变化 v=v0-gt
            nowYSpead -= gSpead * Time.deltaTime;
            //我们注意判断高度是否<=0 即可判断是否落地
            //注意:一定不是判断==因为我们是- 帧间隔*速度 大部分不会刚刚好等于0
            if (bodyTransfrom.localPosition.y <= 0)
            {
                //放置到地面
                bodyTransfrom.localPosition = Vector2.zero;
                //改变地面表示
                ChangeRoleIsGround(true);
                //roleAnimator.SetBool("isGround",true);

                //落地后 不管你击退多少，也要停下来
                nowXSpead = 0;
                //让他站延迟起来
                Invoke("DelayClearHitFly", 0.5f);

                //在落地时创建落地特效
                PoolMgr.GetInstance().GetObj("Effect/DownEff", (eff) =>
                {
                    //异步创建特效成功后 做的事情
                    //根据面朝向 决定灰尘特效的朝向
                    eff.transform.rotation = !roleSprite.flipX? Quaternion.identity:Quaternion.Euler(0, 180, 0);
                    //设置位置 还要减去vector3.forward是为了让特效距离摄像机更新，出现在任务和场景之前
                    eff.transform.position=this.transform.position - Vector3.forward;
                    //让粒子特效重新播放
                    ParticleSystem effSystem= eff.transform.Find("Eff").GetComponent<ParticleSystem>();
                    effSystem.Play();
                    //跳跃音效
                    MusicMgr.GetInstance().PlaySound("Drop",false);
                });
            } 
        }
        if (nowXSpead != 0)
        {
            //在移动之前记录读取位置 移动后 他就是上一次的移动位置
            frontPos = this.transform.position;

            //this.transfrom指的整个父类，影子跟着一起动
            this.transform.Translate(Vector2.right * nowXSpead * Time.deltaTime);
        }
    }
    //延迟起身
    private void DelayClearHitFly()
    {
        roleAnimator.SetBool("isHitFly", false);

        roleAnimator.ResetTrigger("atk1Trigger");
        if (roleType == E_Role_Type.Player)
        {
            roleAnimator.ResetTrigger("jumpTrigger");
            roleAnimator.ResetTrigger("JumpAtkTrigger");
        }
    }

    private Vector3 pos;

    //检测玩家移动相关的逻辑
    protected virtual void CheckMove()
    {
        //在移动前加一个判断 满足后才能移动
        if (CanMoving)
        {
            
            //在移动之前记录读取位置 移动后 他就是上一次的移动位置
            frontPos = this.transform.position;

            //角色移动逻辑 //normalized为移动向量
            this.transform.Translate(move.normalized * property.speed * Time.deltaTime);

            //一种解决层级问题最简单的方法
            pos = this.transform.position;
            pos.z = pos.y;
            this.transform.position= pos;

            //控制玩家转向 不要考虑0
            if (move.x > 0)
            {
                Showdow.localPosition = Vector3.right * 0.14f;
            }
            else if (move.x < 0)
            {
                Showdow.localPosition = Vector3.right * -0.14f;
            }
            //是否移动
            if (move.sqrMagnitude < 0.01f)
                ChangAction(E_Action_type.Idle);
            else
                ChangAction(E_Action_type.Move);
        }
    }


    /// <summary>
    /// 检测身体朝向
    /// </summary>
    protected virtual void ChackBodyDir()
    {
        if (move.x > 0)
        {
            roleSprite.flipX = false;
        }
        else if (move.x < 0)
        {
            roleSprite.flipX = true;
        }
    }

    //攻击方法
    public abstract void Atk();

    //受伤
    public virtual void Hit(float hitTime)
    {
        //  如果受伤时处于击飞状态就没有必要处理受伤逻辑
        if (roleAnimator.GetBool("isHitFly"))
            return;

        //清零延时 受伤又受伤则清零上一次延时
        CancelInvoke("DelayClealhit");
        //延时函数 来处理 过一段时间再结束受伤状态
        //切换受伤动作
        ChangAction(E_Action_type.Hit);
        //roleAnimator.SetBool("isHit", true);
        Invoke("DelayClealhit", hitTime);
    }
    private  void DelayClealhit()
    {
        roleAnimator.SetBool("isHit", false);

        roleAnimator.ResetTrigger("atk1Trigger");
        if (roleType == E_Role_Type.Player)
        {
            roleAnimator.ResetTrigger("jumpTrigger");
            roleAnimator.ResetTrigger("JumpAtkTrigger");
        }
    }
    //击飞
    public virtual void HitFly(float xSpeed, float ySpeed)
    {
        //  如果处于击飞状态则不能再次击飞
        if (roleAnimator.GetBool("isHitFly"))
            return;
        //如果是受伤状态 击飞的优先级高于他 
        if (roleAnimator.GetBool("isHit"))
        {
            //取消延迟清除受伤状态
            CancelInvoke("DelayClealhit");
            //直接清除
            roleAnimator.SetBool("isHit", false);
        }
        //切换击飞动作
        
        ChangAction(E_Action_type.KnockDown);
        //roleAnimator.SetBool("isHitFly", true);
        //改变玩家不在地面
        ChangeRoleIsGround(false);
        //roleAnimator.SetBool("isGround",false );
        //初始竖直上抛的速度 应该是传进来的速度
        nowXSpead = xSpeed;
        nowYSpead = ySpeed;

    }

    //死亡
    public abstract void Dead();
    //切换动作
    public void ChangAction(E_Action_type type)
    {
        switch (type)
        {
            case E_Action_type.Idle:
                roleAnimator.SetBool("isMoveing", false);
                break;
            case E_Action_type.Move:
                roleAnimator.SetBool("isMoveing", true);
                break;
                //基类里只要共有的不要其他
            //case E_Action_type.Pickup:
            //    break;
            case E_Action_type.Hit:
                roleAnimator.SetBool("isHit", true);
                break;
            case E_Action_type.Atk1:
                roleAnimator.SetTrigger("atk1Trigger");
                break;
            //case E_Action_type.Atk2:
            //    break;
            //case E_Action_type.Atk3:
            //    break;
            case E_Action_type.Jump:
                roleAnimator.SetTrigger("jumpTrigger");
                break;
            case E_Action_type.JumpAtk:
                roleAnimator.SetTrigger("JumpAtkTrigger");
                break;
            case E_Action_type.Kick1:
                roleAnimator.SetTrigger("KickTrigger");
                break;
            //case E_Action_type.Kick2:
            //    break;
            //case E_Action_type.Throw:
            //    break;
            case E_Action_type.KnockDown:
                roleAnimator.SetBool("isHitFly", true);
                break;
            //case E_Action_type.Defend:
            //    break;
            case E_Action_type.Dead:
                roleAnimator.SetBool("isDead", true);
                break;
        }
    }

    //切换玩家是否在地面的状态
    protected void ChangeRoleIsGround(bool isGround)
    {
        roleAnimator.SetBool("isGround", isGround);
    }
    //是否可以移动
    protected bool CanMoving
    {
        get 
        {
            //去得到状态机中两层的状态 判断是否可以移动的状态
            AnimatorStateInfo layerinfo1 = roleAnimator.GetCurrentAnimatorStateInfo(0);
            AnimatorStateInfo layerinfo2 = roleAnimator.GetCurrentAnimatorStateInfo(1);
            if (//layerinfo1.IsName("Walk")||
                IsAtkState ||
                layerinfo2.IsName("Defend") ||
                layerinfo2.IsName("Hit") ||
                layerinfo2.IsName("KnockDown") ||
                layerinfo2.IsName("PIckup") ||
                layerinfo2.IsName("Throw")||
                layerinfo2.IsName("StandUp")||
                roleAnimator.GetBool("isDead")) 
                return false;

            //默认不能移动
            return true;
        }
    }

    protected bool IsAtkState
    {
        get
        {
            AnimatorStateInfo layerinfo = roleAnimator.GetCurrentAnimatorStateInfo(1);
           if (//layerinfo1.IsName("Walk")||
                layerinfo.IsName("Atk") ||//这个是怪物
                layerinfo.IsName("Atk 1") ||
                layerinfo.IsName("Atk 2") ||
                layerinfo.IsName("Atk 3") ||
                layerinfo.IsName("Kick 1") ||
                layerinfo.IsName("Kick 2"))
                return true;
            return false;
        }
    }


    //当动作播放时 触发伤害检测相应的函数
    protected void CheckDmg(int id)
    {
        //不同的动作
        //1.伤害判断不同
        //2.可以带给目标的伤害表现不同（受伤，击飞，僵直）
        //对于相同逻辑处理，但是表现数据不同的逻辑
        //我们就可以理想通过配置表到达多样性，提升开发效率
        print("角色伤害检测");
        //获取伤害表中的对应数据
        if (!BinaryDataMgr.Instance.GetTable<DmgInfoContainer>().dataDic.ContainsKey(id))
        {
            //如果没有对应数据就返回
            Debug.Log("对应ID的伤害信息没有找到" + id);
            return;
        }

        DmgInfo info = BinaryDataMgr.Instance.GetTable<DmgInfoContainer>().dataDic[id]; 

        //1.伤害范围检测
        string[] strs = info.f_Check_Range.Split(',');
        //我们需要判断玩家当前面朝向是左是右 来决定偏移位置
        float offsetx = float.Parse(strs[0])*(!roleSprite.flipX ? 1 : -1);
        float offsety = float.Parse(strs[1]);
        float offsetw = float.Parse(strs[2]);
        float offseth = float.Parse(strs[3]);
        //利用unity自带的范围检测 来进行 碰撞检测 来获取对象可以攻击
        Collider2D[] colliders= Physics2D.OverlapBoxAll(new Vector2(roleSprite.transform.position.x + offsetx, roleSprite.transform.position.y + offsety),
                                new Vector2(offsetw,offseth),0);

        //RoleObject roleObj;
        //*********出现攻击者创建打击特效bug 原因
        //第二次进入循环会改变roleObj////bug解决方案为将RoleObject roleObj;写在for里面
        //*********伤害检测矩形框 检测到自己和被攻击者都在矩形框范围时 必出bug
        for (int i=0; i < colliders.Length; i++)
        {
            #region 判断物件的 伤害的逻辑
            ThingObject thingObj = colliders[i].GetComponent<ThingObject>();
            //得到了才会认为是物件，而不是任务怪物
            if (thingObj != null)
            {
                //没有死亡的物件才进行处理
                if (thingObj.isDead)
                    continue;
                //判断物件和玩家伪Z轴的范围
                if(math.abs(this.transform.position.y - thingObj.transform.position.y)>info.f_Check_Zrange)
                    continue;

                //播放受伤的打击特效
                PoolMgr.GetInstance().GetObj("Effect/HitEff", (Eff) =>
                {
                    //异步创建特效成功后 做的事情
                    //设置位置 还要减去vector3.forward是为了让特效距离摄像机更新，出现在任务和场景之前
                    Eff.transform.position = thingObj.transform.position + Vector3.up * 1.5f;
                    //获取对象上挂载的粒子的特效组件
                    ParticleSystem effSystem = Eff.transform.GetComponent<ParticleSystem>();

                    //根据面朝向 决定打击特效的朝向
                    //effSystem.startRotation3D = !thingObj.roleSprite.flipX ? Vector3.up * 180 : Vector3.zero;
                    //effSystem.main.startRotationXMultiplier = 180;

                    //让粒子特效重新播放
                    effSystem.Play();
                });
                //让物件受伤
                thingObj.Wound();
                continue;
            }
            #endregion

            #region 判断玩家怪物之间的逻辑
            //如果改碰撞体压根没有父对象 那肯定不是角色对象
            if (colliders[i] == null)
            {
                continue;
            }
            //把roleObject写在里面可以解决闭包问题
            //bug解决方案，保证roleObj每一次for循环取出的内容
            RoleObject roleObj = colliders[i].transform.parent.GetComponent<RoleObject>();
            //父对象上压根没有角色相关的脚本 那就继续处理
            if (roleObj == null)
            {
                //Debug.LogWarning("No RoleObject component found on parent!");
                continue;
            }
            if (roleObj.isDead)
            {
                continue;
            }

            //找到受伤对象后，还应该判断z轴的平面的误差
            //如果z轴的误差大于表中数据 证明不能受到伤害 题目不在同一平面内
            if (math.abs(this.transform.position.y - roleObj.transform.position.y) > info.f_Check_Zrange)
                continue;

            //判断受伤对象 是否处于格挡状态
            //如果是处于格挡状态，则会创建格挡特效
            if (roleObj.roleAnimator.GetBool("isDefend"))
            {
                //格挡音效
                MusicMgr.GetInstance().PlaySound("Defend", false);
                //格挡特效
                PoolMgr.GetInstance().GetObj("Effect/DefendEff", (eff) =>
                {
                    //异步创建特效成功后 做的事情
                    //根据面朝向 决定格挡特效的朝向
                    eff.transform.rotation = !roleSprite.flipX ? Quaternion.identity : Quaternion.Euler(0, 180, 0);
                    //设置位置 还要减去vector3.forward是为了让特效距离摄像机更新，出现在任务和场景之前
                    Vector3 horizontalOffset = roleSprite.flipX ? Vector3.left : Vector3.right;
                    eff.transform.position = this.transform.position + Vector3.up * 1.7f + horizontalOffset * 0.9f;
                    //让粒子特效重新播放
                    ParticleSystem effSystem = eff.transform.GetComponent<ParticleSystem>();
                    effSystem.Play();
                });

                //格挡后，不需要处理后面逻辑
                continue;
            }

            //如果是同一正营，就不需要让他受伤
            //********bug：第一次攻击者和被攻击者不同正营 会往下走逻辑
            //********会“异步”！！！！加载资源
            //第一种 推荐
            if (roleObj.roleType == this.roleType)
                continue;
            //第二种
            //if(roleObj.gameObject.tag==this.gameObject.tag)
            //    continue ;

            //受伤 伤害的处理
            if (info.f_wound_Time != 0)
            {
                roleObj.Hit(info.f_wound_Time);
            }
            else
            {
                strs = info.f_Hitfly_Speed.Split(',');
                //如果攻击者在攻击的右侧就右飞否则就左飞
                roleObj.HitFly(float.Parse(strs[0]) * (roleObj.transform.position.x >= this.transform.position.x ? 1 : -1),
                    float.Parse(strs[1]));
            }

            //print("找到对象 有伤害");
            //受伤伤害处理
            //由于用的是属性基类，所以不用考虑攻击者是玩家还是怪物
            //攻击力，防御力，闪避率，暴击率 一般由策划提供程序
            //1.先计算闪避率
            if (UnityEngine.Random.Range(0, 100) <= roleObj.property.miss)
            {
                //闪避率成功
                //显示闪避效果
                continue;
            }
            //2.伤害

            //打击音效播放
            MusicMgr.GetInstance().PlaySound(info.f_hit_sound,false);

            //播放受伤的打击特效
            PoolMgr.GetInstance().GetObj("Effect/HitEff", (Eff) =>
            {
                //异步创建特效成功后 做的事情
                //设置位置 还要减去vector3.forward是为了让特效距离摄像机更新，出现在任务和场景之前
                Eff.transform.position = roleObj.transform.position + Vector3.up * 1.5f;
                //获取对象上挂载的粒子的特效组件
                ParticleSystem effSystem = Eff.transform.GetComponent<ParticleSystem>();

                //根据面朝向 决定打击特效的朝向
                effSystem.startRotation3D = !roleObj.roleSprite.flipX ? Vector3.up * 180 : Vector3.zero;
                //effSystem.main.startRotationXMultiplier = 180;

                //让粒子特效重新播放
                effSystem.Play();
            });

            //2.1基础伤害
            int dmg = this.property.atk = roleObj.property.def;
            if (dmg < 0)
            {
                //1.伤害至少为1
                dmg = 1;
                //2.处理成格挡
            }
            //2.2暴击
            if (UnityEngine.Random.Range(0, 100) <= roleObj.property.crit)
            {
                dmg += (int)(dmg * (this.property.critNum / 100f));
            }
            //3减少血量
            roleObj.property.nowhp -= dmg;

            //更新血条
            if(roleObj.roleType==E_Role_Type.Player)
            {
                UIManager.GetInstance().GetPanel<FightPanel>("FightPanel").UpdatePlayerHP(roleObj.property.nowhp, roleObj.property.maxhp);
            }
            else
            {
                UIManager.GetInstance().GetPanel<FightPanel>("FightPanel").UpdateMonsterHp(roleObj.property.nowhp, roleObj.property.maxhp);

            }
            //4判断死亡
            if (roleObj.property.nowhp <= 0)
            {
                roleObj.Dead();
            }
            //测试减血
            print(roleObj.property.nowhp + "/" + roleObj.property.maxhp);
            
        }
        #endregion
        }

    //当玩家死亡动画播放结束会调用的函数
    protected abstract void CheckDead();

}
 
public enum E_Action_type
{
    /// <summary>
    /// 待机
    /// </summary>
    Idle,
    /// <summary>
    /// 移动
    /// </summary>
    Move,
    /// <summary>
    /// 跳跃
    /// </summary>
    Jump,
    /// <summary>
    /// 跳跃攻击
    /// </summary>
    JumpAtk,
    /// <summary>
    /// 攻击1
    /// </summary>
    Atk1,
    //Atk2,
    //Atk3,
    /// <summary>
    /// 腿部攻击1
    /// </summary>
    Kick1,
    //Kick2,
    /// <summary>
    /// 受击
    /// </summary>
    Hit,
    /// <summary>
    /// 防御
    /// </summary>
    Defend,
    /// <summary>
    /// 扔东西
    /// </summary>
    Throw,
    /// <summary>
    /// 死亡倒地
    /// </summary>
    KnockDown,
    /// <summary>
    /// 下蹲
    /// </summary>
    Pickup,
    /// <summary>
    /// 死亡
    /// </summary>
    Dead,
}




