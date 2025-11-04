using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterProperty : BaseProperty
{
    /// <summary>
    /// 怪物死亡掉落经验
    /// </summary>
    public int deadexp;
    //死亡音效
    public string deadSound;

    /// <summary>
    /// 怪物类型 1为普通怪物 2是boss
    /// </summary>
    public int type;

    public MonsterProperty(int infoid):base(infoid)
    {
        //读取怪物属性表
        T_MonsterLev_Info info = BinaryDataMgr.Instance.GetTable<T_MonsterLev_InfoContainer>().dataDic[infoid];
        if (info == null)
        {
            Debug.LogError("怪物登记表为空：" + infoid);
            return;
        }
        //初始化各个数据
        this.atk = info.f_atk;
        this.def = info.f_def;
        this.miss = info.f_miss;
        this.crit = info.f_crit;
        this.speed = info.f_speed;
        this.critNum = info.f_critNum;
        this.nowhp = this.maxhp = info.f_hp;
        this.deadexp = info.f_exp;
        this.deadSound=info.f_dead_sound;
        this.type = info.f_type;
    }
}


