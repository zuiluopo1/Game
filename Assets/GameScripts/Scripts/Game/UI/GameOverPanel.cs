using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPanel : BasePanel
{
    //允许按任意键复活
    private bool canDownAnyKey=true;
    public override void ShowMe()
    {
        base.ShowMe();
        //做倒计时有n种方法：1.update里自己计时，2.延迟函数，3.协同函数
        //开始倒计时 并且启动协程程序
        StartCoroutine(CountDown());
    }

    private void Update()
    {
        //是否按任意键复活
        if(canDownAnyKey && Input.anyKeyDown)
        {
            //允许按任意键复活 并且按下了任意键 就隐藏面板 让玩家复活
            LevelMgr.GetInstance().PlayerRelife();
        }
    }

    IEnumerator CountDown(int Time=9)
    {
        while (true)
        {
            GetControl<Text>("txtTime").text = Time.ToString();
            if (Time <= 0)
            {
                break;
            }
            yield return new WaitForSeconds(1f);
            --Time;
        }
        canDownAnyKey=false;
        //显示GameOver相关
        GetControl<Image>("imgGameOver").GetComponent<Animator>().Play("show");
        GetControl<Text>("txtGameover").GetComponent<Animator>().Play("show");

        //等待几秒 让玩家感受到失败 并且使动画播完
        yield return new WaitForSeconds(2f);

        //失败后的逻辑重新回到开始界面
        LevelMgr.GetInstance().PlayerDeadGameOver();
    }
}
