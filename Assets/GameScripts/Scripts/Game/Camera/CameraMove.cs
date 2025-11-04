using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : SingletonMono<CameraMove>
{
    //摄像机移动速度
    public float moveSpeed=5000;

    //摄像机移动的目标点
    private Vector3 targetPos;

    //上一次摄像机所在的坐标
    private Vector3 frontPos;
    /// <summary>
    /// 摄像机可视范围最左侧 世界范围x坐标
    /// </summary>
    private float CameraLeftX;

    //摄像机要使用LateUpdate，避免异样情况，在Update之后执行
    private void Start()
    {
        //因为摄像机的y和z都不会变化 所以我们的赋值一次
        targetPos = this.transform.position;
    }


    private void LateUpdate()
    {
        //如果没有玩家 避免报错
        if(PlayerObject.player==null)
            return;

        //在移动之前记录移动的位置，方便拉回来
        frontPos=this.transform.position;

        //获取目标位置的x 就是玩家的x
        targetPos.x = PlayerObject.player.transform.position.x;
        //利用差值函数Lerp，让摄像机向目标点靠近
        this.transform.position = Vector3.Lerp(this.transform.position, targetPos, moveSpeed * Time.deltaTime);

        //摄像机可视范围 边界判断的核心知识
        //屏幕坐标转成世界坐标 然后用屏幕左右对应的世界坐标来进行 分段边界判断
        //注意：由于是正交摄像机 所以不用考虑转换横截面问题

        //判断 可视范围 左右 是否超出了 分段左右
        //可视范围最左侧的x坐标轴的一点
        //可视范围最右侧的x坐标轴的一点

        //ScreenToWorldPoint是转世界坐标
        //推图过程中 如果摄像机的左边界大于动态左边界 在去修改动态左边界的值（重要）
        if (LevelMgr.GetInstance().isPush &&
            Camera.main.ScreenToWorldPoint(Vector3.zero).x>LevelMgr.GetInstance().cameraLeftX)
        {
            //动态左边界变化的地方
            LevelMgr.GetInstance().cameraLeftX = 
                Camera.main.ScreenToWorldPoint(Vector3.zero).x;
            //进行推图时要判断是否到了第二张地图
            LevelMgr.GetInstance().CheckPushOver();
        }

        if (LevelMgr.GetInstance().CheckOutSectionRectX(Camera.main.ScreenToWorldPoint(Vector3.zero).x) ||
         LevelMgr.GetInstance().CheckOutSectionRectX(Camera.main.ScreenToWorldPoint(Vector3.right * Screen.width).x))
        {
            this.transform.position = frontPos;
        }     
    }
}
