using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeginPanel : BasePanel
{
    private void Update()
    {
        if (Input.anyKeyDown)
        {
            ScenesMgr.GetInstance().LoadScene("FightScene", () =>
            {
                Debug.Log("切换场景成功");
                //隐藏场景UI
                UIManager.GetInstance().HidePanel("BeginPanel");
                //第一关关卡初始化
                LevelMgr.GetInstance().InitLevel(1001);
            });
        }
    }
}
