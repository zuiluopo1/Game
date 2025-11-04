using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProperty : BaseProperty
{
    /// <summary>
    /// 当前等级
    /// </summary>
    public int lev;
    /// <summary>
    /// 升级需要的经验
    /// </summary>
    public int levexp;
    /// <summary>
    /// 当前经验
    /// </summary>
    public int nowexp;
    /// <summary>
    /// 跳跃速度
    /// </summary>
    public int jumpSpeed;
    public PlayerProperty(int infoid) : base(infoid)
    {
        //玩家怪物属性表
        T_PlayerLev_Info info = BinaryDataMgr.Instance.GetTable<T_PlayerLev_InfoContainer>().dataDic[infoid];
        if (info == null)
        {
            Debug.LogError("玩家等级表为空：" + infoid);
            return;
        }
        //初始化各个数据
        this.atk = info.f_atk;
        this.def = info.f_def;
        this.miss = info.f_miss;
        this.crit=info.f_crit;
        this.lev = infoid;
        this.levexp = info.f_lecup_exp;
        this.speed = info.f_speed;
        this.jumpSpeed = info.f_jnumpSpeed;
        this.critNum=info.f_critNum;
        this.nowhp=this.maxhp=info.f_hp;
        this.nowexp = 0;
    }
}

