using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WinPanel : BasePanel
{
    //倒计时
    private int nowTime;

    /// <summary>
    /// 初始化面板显示的信息
    /// </summary>
    /// <param name="money"></param>
    /// <param name="gem"></param>
    /// <param name="num"></param>
    public void InitInfo(int money,int gem,int num)
    {
        //更新金币奖励显示
        GetControl<Text>("MoneyNum").text = money.ToString();
        //更新宝石奖励显示
        GetControl<Text>("GemNum").text = gem.ToString();
        //更新分数奖励显示
        GetControl<Text>("Num ").text = num.ToString();

        //初始化关卡信息是就重置
        nowTime = 5;
        //开启协同程序
        StartCoroutine(BeginTime());
    }

    protected override void OnClick(string btnName)
    {
        switch (btnName)
        {
            //确定后处理什么逻辑
            case "ButSure":
                //点击按钮后 就不用 协同程序计时了 可以关掉
                StopAllCoroutines();//由于该对象只有一个协程 直接关所有
                //隐藏面板
                UIManager.GetInstance().HidePanel("WinPanel");
                //切换关卡
                LevelMgr.GetInstance().ChangLevel();
                UIManager.GetInstance().ShowPanel<FightPanel>("FightPanel");
                break;
        }
    }

    IEnumerator BeginTime()
    {
        while (true)
        {
            GetControl<Text>("next").text =nowTime+"秒后切换下一关卡";
            
            yield return new WaitForSeconds(1f);
            --nowTime;
            if (nowTime == 0) 
                break;
        }
        //处理关卡相关逻辑
        //隐藏面板
        UIManager.GetInstance().HidePanel("WinPanel");
        //切换关卡
        LevelMgr.GetInstance().ChangLevel();
        UIManager.GetInstance().ShowPanel<FightPanel>("FightPanel");
    }
}

