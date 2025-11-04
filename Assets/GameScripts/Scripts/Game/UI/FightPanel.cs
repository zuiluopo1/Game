using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI
/// </summary>
public class FightPanel : BasePanel
{
    private GameObject MonsterHp;

    private GameObject Goobj;
    //玩家血条多宽
    private Vector2 PlayerHpMaxWH;
    //怪物血条
    private Vector2 MonsterHpMaxWH;
    //玩家血条
    private Image imageHp;
    //怪物血条
    private Image imgMonHp;

    protected override void Awake()
    {
        base.Awake();
        //初始化
        imageHp = this.GetControl<Image>("ImageHp");
        //显示面板时得到血条 最慢时它的高度
        PlayerHpMaxWH = imageHp.rectTransform.sizeDelta;
        //怪物初始化
        imgMonHp = this.GetControl<Image>("ImgmonHp");
        //显示面板时得到血条 最慢时它的高度
        MonsterHpMaxWH = imgMonHp.rectTransform.sizeDelta;
    }
    public override void ShowMe()
    {
        base.ShowMe();
        //显示面板时 找到怪物血条和gogogo提示图标
        MonsterHp=this.transform.Find("MonsterHp").gameObject;
        Goobj = this.transform.Find("Go").gameObject;

        
        MonsterHp.SetActive(false);
        Goobj.SetActive(false);

        //显示面板等到血条最慢时他的宽度
        PlayerHpMaxWH = this.GetControl<Image>("ImageHp").rectTransform.sizeDelta;
    }

    /// <summary>
    /// 更新玩家面板
    /// </summary>
    /// <param name="nowHp"></param>
    /// <param name="maxHp"></param>
    public void UpdatePlayerHP(int nowHp,int maxHp)
    {
        //根据传入的血量 来决定血条的长度
        imageHp.rectTransform.sizeDelta = new Vector2
        (PlayerHpMaxWH.x * nowHp / maxHp, PlayerHpMaxWH.y);
    }

    /// <summary>
    /// 更新怪物面板
    /// </summary>
    /// <param name="nowHp"></param>
    /// <param name="maxHp"></param>
    public void UpdateMonsterHp(int nowHp,int maxHp)
    {
        MonsterHp.SetActive(true);
        //每次重置延迟时间
        CancelInvoke("HideMonsterHp");
        //延迟一秒隐藏Ui
        Invoke("HideMonsterHp", 1f);
        //根据传入的血量 来决定血条的长度
        imgMonHp.rectTransform.sizeDelta = new Vector2
        (MonsterHpMaxWH.x * nowHp / maxHp, MonsterHpMaxWH.y);
    }

    private void HideMonsterHp()
    {
        MonsterHp.SetActive(false);
    }

    /// <summary>
    /// 提供给外部调用显示的函数
    /// </summary>
    /// <param name="isShow"></param>
    public void showOrHideGOGOGO(bool isShow)
    {
        Goobj.SetActive(isShow);
    }
}
