using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Awake()
    {
        //如果数据不多并且经常用则 可以一进就加载
        BinaryDataMgr.Instance.InitData();
        //显示开始面板
        UIManager.GetInstance().ShowPanel<BeginPanel>("BeginPanel");
        //播放开始场景的音乐
        MusicMgr.GetInstance().PlayBkMusic(BinaryDataMgr.Instance.GetTable<T_UtilContainer>().dataDic["f_beginSconene_music"].f_value);
        MusicMgr.GetInstance().ChangeBKValue(0.6f);
    }

    void Update()
    {
        
    }
}
