using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Pushtype
{
    //把自己回收到池子中
    Self,
    //把父对象回收到池子中
    Father,
}
public class EffectPlayOver : MonoBehaviour
{
    public Pushtype type;
    public void OnParticleSystemStopped()
    {
        //ebug.Log("粒子特效播放完毕");
        //需要粒子特效播放完毕 放入缓存池中
        if (type == Pushtype.Self)
            PoolMgr.GetInstance().PushObj(this.gameObject.name,this.gameObject);
        //将父对象回收到池中
        else
            PoolMgr.GetInstance().PushObj(this.transform.parent.name,
                this.transform.parent.gameObject);
    }
}
