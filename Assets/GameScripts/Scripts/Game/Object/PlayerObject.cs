using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerObject : RoleObject
{
    //玩家跳跃初速度和重力加速度 配表后 无需他
    //public float jumpSpead = 10;
    //public float gSpead = 9.8f;
    ////当前跳跃速度
    //private float nowYSpead;
    ////击退速度
    //private float nowXSpead;

    //攻击连招计数
    private int atkCount=0;

    //腿部连招计数
    private int KickCount=0;
    //静态玩家对象
    public static PlayerObject player;

    public int needexp;
    public int nowexp;

    //拾取物体是用于检测的 矩形框
    private Vector3 pickupCheckBoxffset = new Vector3(0.1f,0.5f,0);


    protected override void Awake()
    {
        //父类相关的Awake一定要保留
        base.Awake();
        //开启输入控制
        InputMgr.GetInstance().StartOrEndCheck(true);
        //获取输入权限
        GetController();

        player = this;

        roleType = E_Role_Type.Player;

        //测试初始化属性方法
        InitProperty(1);
    }

    public override void InitProperty(int id)
    {
        //怪物初始化数据
        property = new PlayerProperty(id);
    }

    #region 以前用的跳跃逻辑
    //protected override void Update()
    //{
    //之前用
    //    //一定要保持base.Update的存在因为 移动逻辑是写在父类中的
    //    //除非要重新写 否则才不需要他 
    //    base.Update();

    //    ////处理跳跃逻辑
    //    ////不是在地面就是在跳跃
    //    //if (!roleAnimator.GetBool("isGround"))
    //    //{
    //    //    //跳跃身体对象
    //    //    bodyTransfrom.Translate(Vector2.up * nowYSpead * Time.deltaTime);
    //    //    //数值上抛 下落逻辑 速度变化 v=v0-gt
    //    //    nowYSpead -= gSpead * Time.deltaTime;
    //    //    //我们注意判断高度是否<=0 即可判断是否落地
    //    //    //注意:一定不是判断==因为我们是- 帧间隔*速度 大部分不会刚刚好等于0
    //    //    if(bodyTransfrom.localPosition.y <= 0)
    //    //    {
    //    //        //放置到地面
    //    //        bodyTransfrom.localPosition = Vector2.zero;
    //    //        //改变地面表示
    //    //        ChangeRoleIsGround(true);
    //    //        //roleAnimator.SetBool("isGround",true);

    //    //        //落地后 不管你击退多少，也要停下来
    //    //        nowXSpead = 0;
    //    //        //让他站延迟起来
    //    //        Invoke("DelayClearHitFly", 0.5f);
    //    //    }
    //    //}
    //    //if(nowXSpead != 0)
    //    //{
    //    //    //this.transfrom指的整个父类，影子跟着一起动
    //    //    this.transform.Translate(Vector2.right*nowXSpead * Time.deltaTime);
    //    //    }
    //}

    //延迟起身
    //private void DelayClearHitFly()
    //{
    //    roleAnimator.SetBool("isHitFly", false);
    //}
    #endregion


    protected override void CheckMove()
    {
        //父类一定要保留
        base.CheckMove();

        //记录移动后的当前位置
        Vector3 nowPos=this.transform.position;
        //移动边界判断
        //读取当前的位置 来和边界进行判断
        //为了让斜着走时 某一个方向不受影响，我们应该分情况判断 ，将x,y分别处理

        if (LevelMgr.GetInstance().CheckOutSectionRectX(nowPos.x))
        {
            //单独把x坐标拉回去
            nowPos.x = frontPos.x;
        }

        

        if (LevelMgr.GetInstance().CheckOutSectionRectY(nowPos.y))
        {
            //单独把y坐标拉回去
            nowPos.y= frontPos.y;
        }
        //位置拉回去后的赋值 既使没有拉回去，也要赋值
        this.transform.position = nowPos;
    }


    //拾取
    /// <summary>
    /// 拾取动作
    /// </summary>
    public bool Pickup()
    {
        //判断物体相关内容 //由于没有记入物品掉落 则需要想办法先获取到物体
        //通过范围检测 和角色对象有重合 再进行下一步
        //1.先判断是否和地上物体重合 //要使用OverlapBoxAll
        Collider2D[] collider2D= Physics2D.OverlapBoxAll
            (this.transform.position + pickupCheckBoxffset, Vector2.one, 
            0, 1 << LayerMask.NameToLayer("PickItem"));
        
        //2.再判断Z轴
        for (int i=0;i<collider2D.Length;i++)
        {
            //判断Z轴的的距离是否合法
            if (Mathf.Abs(collider2D[i].transform.position.y - this.transform.position.y) < 0.3f)
            {
                //挂载
                PickItemObject obj=collider2D [i].GetComponent<PickItemObject>();
                switch (obj.type)
                {
                    case E_PickItem_type.Meat:
                        property.nowhp += obj.num;
                        if(property.nowhp > property.maxhp)
                            property.nowhp = property.maxhp; 
                            
                        Debug.Log("当前血量"+property.nowhp);
                        break;
                    case E_PickItem_type.Knife: 
                        break;
                }

                //切换拾取动作
                roleAnimator.SetTrigger("pickupTrigger");

                //拾取音效
                MusicMgr.GetInstance().PlaySound("ItemPickup", false);

                //把拾取的物体放进缓存池中
                PoolMgr.GetInstance().PushObj("Plckup/MeatPickup", collider2D[i].gameObject);

                //一次只能拾取一个物体
                return true;
            }
        }

        //在这里判断是否能不能拾取 如果能则 返回True
        return false;
    }
    
    //投掷
    public void Throw()
    {
        roleAnimator.SetTrigger("throwTrigger");
    }

    #region 以前用的代码
    //受伤方法
    //public void Hit(float hitTime)
    //{
    //    //  如果受伤时处于击飞状态就没有必要处理受伤逻辑
    //    if (roleAnimator.GetBool("isHitFly"))
    //        return;

    //    //清零延时 受伤又受伤则清零上一次延时
    //    CancelInvoke("DelayClealhit");
    //    //延时函数 来处理 过一段时间再结束受伤状态
    //    //切换受伤动作
    //    ChangAction(E_Action_type.Hit);
    //    //roleAnimator.SetBool("isHit", true);
    //    Invoke("DelayClealhit", hitTime);
    //}
    //private void DelayClealhit()
    //{
    //    roleAnimator.SetBool("isHit",false);
    //}

    //受伤击飞
    //public void HitFly(float xSpeed,float ySpeed)
    //{
    //    //  如果处于击飞状态则不能再次击飞
    //    if(roleAnimator.GetBool("isHitFly"))
    //        return;
    //    //如果是受伤状态 击飞的优先级高于他
    //    if (roleAnimator.GetBool("isHit"))
    //    {
    //        //取消延迟清除受伤状态
    //        CancelInvoke("DelayClealhit");
    //        //直接清除
    //        roleAnimator.SetBool("isHit", false);
    //    }
    //    //切换击飞动作
    //    ChangAction(E_Action_type.KnockDown);
    //    //roleAnimator.SetBool("isHitFly", true);
    //    //改变玩家不在地面
    //    ChangeRoleIsGround(false);
    //    //roleAnimator.SetBool("isGround",false );
    //    //初始竖直上抛的速度 应该是传进来的速度
    //    nowXSpead = xSpeed;
    //    nowYSpead = ySpeed;

    //}


    //跳跃函数
    #endregion y
    
    //跳跃
    protected void Jump()
    {
        AnimatorStateInfo layerinfo1 = roleAnimator.GetCurrentAnimatorStateInfo(1);
        //在地面是false 才进行跳跃
        //之后如果有不能跳跃的条件在加
        if (roleAnimator.GetBool("isGround") &&
            !roleAnimator.GetBool("isDefend") &&
            !roleAnimator.GetBool("isHit") &&
            !roleAnimator.GetBool("isHitFly") &&
            !layerinfo1.IsName("PIckup")&&
            !layerinfo1.IsName("Throw") &&
            !IsAtkState)
        {
            //初始跳跃速度 可以封装成属性或者函数
            nowYSpead = (property as PlayerProperty).jumpSpeed;
            //切换动作
            ChangAction(E_Action_type.Jump);
            //roleAnimator.SetTrigger("jumpTrigger");
            //切换在地面的状态
            ChangeRoleIsGround (false);
            //roleAnimator.SetBool("isGround", false);
        }
    }

    //跳跃攻击
    private void jumpatk()
    {
        //如果当前处于 跳跃攻击状态 就不能在触发跳跃攻击
        //isname:是找到1这个层的要处理名字  GetCurrentAnimatorStateInfo：指找到这个层
        if (!roleAnimator.GetCurrentAnimatorStateInfo(1).IsName("JumpKick"))
        {
            ChangAction(E_Action_type.JumpAtk);
            //roleAnimator.SetTrigger("JumpAtkTrigger");
        }
    }

    //手部攻击
    public override void Atk()
    {
        //或者使用If(roleAnimator.GetBool("isDefend")){return;}//不满足直接返回
        //需要判断是否可以攻击
        if (!roleAnimator.GetBool("isDefend"))
        {
            //第一个方式
            ChangAction(E_Action_type.Atk1);
            //roleAnimator.SetTrigger("atk1Trigger");
            //第二个方式 通过int累加处理
            //首先要停止延迟
            CancelInvoke("DelayClearAtkCount");
            AnimatorStateInfo stateInfo = roleAnimator.GetCurrentAnimatorStateInfo(1);
            if (stateInfo.IsName("Null"))
                atkCount = 1;
            else if (stateInfo.IsName("Atk 1"))
                atkCount = 2;
            else if (stateInfo.IsName("Atk 2"))
                atkCount = 3;
            else
                atkCount = 0;
            roleAnimator.SetInteger("atkCount", atkCount);
            //1.长时间后清零 2.写计时逻辑 3.通过协同程序去计时
            Invoke("DelayClearAtkCount", 0.4f);
        }
    }
    private void DelayClearAtkCount()
    {
        atkCount = 0;
        roleAnimator.SetInteger("atkCount", atkCount);
    }

    //腿部攻击
    private void kickAtk()
    {
        if (!roleAnimator.GetBool("isDefend"))
        {
            ChangAction(E_Action_type.Kick1);
            //roleAnimator.SetTrigger("KickTrigger");
            //清零
            CancelInvoke("DelayClearKickCount");
            //腿部计数
            ++KickCount;
            roleAnimator.SetInteger("KickCount", KickCount);
            //1.长时间后清零 2.写计时逻辑 3.通过协同程序去计时
            Invoke("DelayClearKickCount", 0.3f);
        }
        
        
    }
    private void DelayClearKickCount()
    {
        KickCount = 0;
        roleAnimator.SetInteger("KickCount",KickCount);
    }

    private void Defend(bool isDefend)
    {
        roleAnimator.SetBool("isDefend", isDefend);
    }

    //玩家死亡行为
    public override void Dead()
    {
        isDead = true;

        //处理失败的逻辑交给关卡管理处理更好
        EventCenter.GetInstance().EventTrigger("GameOver");
        //关闭玩家控制权
        InputMgr.GetInstance().StartOrEndCheck(false);
    }

    /// <summary>
    /// 复活
    /// </summary>
    public void ReLife()
    {
        //血量回归
        this.property.nowhp =this.property.maxhp;
        UIManager.GetInstance().GetPanel<FightPanel>("FightPanel").
            UpdatePlayerHP(this.property.nowhp,this.property.maxhp);
        
        //死亡状态修改
        isDead = false;
        roleAnimator.SetBool("isDead",false);
        //设计玩家动作变化

        //开启玩家控制权
        InputMgr.GetInstance().StartOrEndCheck(true);
    }

    //给予控制权
    public void GetController()
    {
        //事件有加就有减，一定不要传输nbd表达式 一定在下方申明函数
        EventCenter.GetInstance().AddEventListener<float>("Horizontal", CheckX);
        EventCenter.GetInstance().AddEventListener<float>("Vertical", CheckY);

        EventCenter.GetInstance().AddEventListener<KeyCode>("someKeyDown", CheckKeyDown);
        //监听按键抬起内容
        EventCenter.GetInstance().AddEventListener<KeyCode>("SomeKeyUp", CheckKeyUp);
    }
    //剥夺控制权
    public void RemoveController()
    {
        EventCenter.GetInstance().RemoveEventListener<float>("Horizontal", CheckX);
        EventCenter.GetInstance().RemoveEventListener<float>("Vertical", CheckY);

        EventCenter.GetInstance().RemoveEventListener<KeyCode>("someKeyDown", CheckKeyDown);
        EventCenter.GetInstance().RemoveEventListener<KeyCode>("SomeKeyUp", CheckKeyUp);

    }

    private void CheckX(float x)
    {
        //X有内容 是-1 0 1三个值，A是-1 不按是0 按D为1
        //Debug.Log("ad键"+x);
        //获取横向输入
        move.x = x;
    }
    private void CheckY(float y)
    {
        //y有内容 是-1 0 1三个值，W是-1 不按是0 按S为1
        //Debug.Log("ws键"+y);
        //获取纵向输入
        move.y = y; 
    }
    //检测玩家 初移动以外的输入内容
    private void CheckKeyDown(KeyCode key)
    {
        switch(key)
        {
            case KeyCode.J:
                Debug.Log("j攻击1");
                //手部攻击，如果不在地面 处理 跳跃攻击逻辑
                if (!roleAnimator.GetBool("isGround"))
                {
                    jumpatk();
                }
                else  //在地面上就能够攻击 还有限制条件就再加
                {
                    //当前是否站在 拾取物体上
                    if(!Pickup())
                        Atk();
                }
                break;
            case KeyCode.K:
                Debug.Log("k攻击2");
                //跳跃腿部攻击
                if (!roleAnimator.GetBool("isGround"))
                {
                    jumpatk();
                }
                else  //腿部攻击
                {
                    //当前是否站在 拾取物体上
                    if (!Pickup())
                        kickAtk();
                }
                break;
            case KeyCode.L:
                Debug.Log("l格挡");
                Defend(true);
                break;
            case KeyCode.Space:
                Debug.Log("Space跳跃");
                //roleAnimator.SetBool("isGround", true);
                Jump();
                //roleAnimator.SetBool("isGround", true);
                break;
                //测试受伤
            case KeyCode.B:
                //Hit(0.2f);
                //HitFly(-10,15);
                Pickup();
                //Throw();
                break;
            case KeyCode.V:
                HitFly(-10,15);
                break;
        }
    }

    private void CheckKeyUp(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.L:
                Defend(false);
                break;
        }
    }
    private void OnDestroy()
    {
        //有加就有减 一定要移除事件
        RemoveController();
    }

    /// <summary>
    /// 当对象死亡动画结束时 调用的内容
    /// </summary>
    protected override void CheckDead()
    {
        //throw new System.NotImplementedException();
    }
}



