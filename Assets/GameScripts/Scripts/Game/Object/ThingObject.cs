using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThingObject : MonoBehaviour
{
    //当前血量
    private int nowHP;
    //掉落物体配置信息
    private string downThingInfo;
    //动画状态机主件
    private Animator animator;

    //是否死亡
    public bool isDead=false;
    public void Awake()
    {
        //获取子对象的动画状态机
        animator = this.GetComponentInChildren<Animator>();

        //去找到子对象中 死亡动画事件处理的脚本 来进行事件
        //当对象播放完死亡动画完毕后 就会执行我们希望他执行的逻辑
        DeadCheck deadCheck = this.GetComponentInChildren<DeadCheck>();
        deadCheck.CheckCallback += () =>
        {
            //为了性能考虑 使用可能会进程使用 所以我们将将其放入缓存池中
            PoolMgr.GetInstance().PushObj(this.gameObject.name,this.gameObject);
        };

        //测试代码 目前没有创建物体的地方
        //用于初始化相关内容
        Init(BinaryDataMgr.Instance.GetTable<T_thing_InfoContainer>().dataDic[1]);
    }
    /// <summary>
    /// 初始化物件信息
    /// </summary>
    /// <param name=""></param>
    public void Init(T_thing_Info info)
    {
        string[] strs=info.f_hit_nums.Split(',');
        //在n 到 m之间随机选择
        nowHP = Random.Range(int.Parse(strs[0]), int.Parse(strs[1])+1);
        //记录掉落的物品
        downThingInfo = info.f_down_thing;
        //初始化
        isDead = false;

        animator.SetBool("isDead", false);

        //改变Z轴的 让他能够在显示时 表现出正确的层级关系
        Vector3 pos = this.transform.position;
        pos.z = pos.y;
        this.transform.position = pos;
    }

    /// <summary>
    /// 受伤方法
    /// </summary>
    public void Wound()
    {
        //减血
        nowHP -= 1;
        //物件抖动

        //血量减完 就死亡
        if (nowHP <= 0)
        {
            Dead();
            return;
        }
    }

    public void Dead()
    {
        //切换死亡动画
        animator.SetBool("isDead", true);
        isDead = true;
        //随机掉落物体
        //第一次分割是为了获取多少给物品
        string[] strs=downThingInfo.Split(";");
        for (int i = 0; i < strs.Length; i++)
        {
            string[] itemInfo = strs[i].Split(",");
            //第二次分割是为了 获取爆率
            //通过随机数来取出0到100的输
            int ifCreate = Random.Range(0, 101);
            //判断随机数 是否小于等于 我们配置的数
            if (ifCreate <= int.Parse(itemInfo[1]))
            {
                //根据路径 在缓存池中来创建
                PoolMgr.GetInstance().GetObj(itemInfo[0], (obj) =>
                {
                    obj.transform.position = this.transform.position;
                });
            }
        }
    }
}




