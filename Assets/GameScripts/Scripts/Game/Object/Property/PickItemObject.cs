using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_PickItem_type
{
    //肉
    Meat,
    //刀
    Knife,
}

public class PickItemObject : MonoBehaviour
{
    //道具类型
    public E_PickItem_type type;
    //道具数值
    public int num;

    public void Init()
    {
        //改变Z轴的 让他能够在显示时 表现出正确的层级关系
        Vector3 pos =this.transform.position;
        pos.z=pos.y;
        this.transform.position = pos;
    }

}
