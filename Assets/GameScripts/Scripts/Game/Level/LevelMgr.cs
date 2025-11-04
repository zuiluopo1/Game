using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_Pass_Type
{
    /// <summary>
    /// 所有怪物死亡
    /// </summary>
    All_Monster_Dead=1,
    /// <summary>
    /// boss全部死亡
    /// </summary>
    Boss_Dead,
}

/// <summary>
/// 关卡管理器
/// </summary>
public class LevelMgr : BaseManager<LevelMgr>
{
    //用于记录分段ID
    private string[] sections;
    //当前所在分段索引
    private int nowSectionsIndex=0;

    //分段边界范围
    public SectionRect sectionRect;
    //当前分段有多少只怪物
    public int monsterNum;
    //是否处于推图，切分段的状态
    public bool isPush;

    /// <summary>
    /// 摄像机可视范围最左侧 世界范围x坐标
    /// </summary>
    public float cameraLeftX;

    //通关判断条件类型
    //1代表 分段怪物死亡 2 代表boss所有死亡
    public E_Pass_Type passType;

    //当前分段有多少boss
    public int bossNum;
    //下一关的ID
    private int nexrlevel;

    private int moneynum;//金币
    private int gemnum;//宝石
    private int num;//分数

    /// <summary>
    /// 切换关卡
    /// </summary>
    public void ChangLevel()
    {
        ScenesMgr.GetInstance().LoadScene("FightScene", () =>
        {
            InitLevel(nexrlevel);
            //清理相关内容
            Clearlevel();
        });
    }
    /// <summary>
    /// 关卡id 初始化关卡信息
    /// </summary>
    /// <param name="id"></param>
    public void InitLevel(int id)
    {
        //获取关卡表的数据
        T_level_Total_Info Info= BinaryDataMgr.Instance.GetTable<T_level_Total_InfoContainer>().dataDic[id];
        //记录通关条件
        passType = (E_Pass_Type)Info.f_pass;
        //下一关的ID
        nexrlevel= Info.f_next_level;

        //奖励初始化
        string[] strs = Info.f_reward.Split(";");
        string[] money = strs[0].Split(",");
        string[] gem = strs[1].Split(",");
        moneynum = Random.Range(int.Parse(money[0]),int.Parse(money[1])+1);
        gemnum = Random.Range(int.Parse(gem[0]), int.Parse(gem[1]) + 1);
        num=int.Parse(strs[2]);

        //动态创建关卡地图
        ResMgr.GetInstance().LoadAsync<GameObject>(Info.f_map_res, (obj) =>
        {
            //GameObject.Instantiate(obj);
            //地图加载结束 在去创建角色
            ResMgr.GetInstance().LoadAsync<GameObject>("Role/Player1", (player) =>
            {
                //GameObject Player= GameObject.Instantiate(player);
                //解析角色中数据
                string[] strs=Info.f_player_bornPos.Split(',');
                //设置玩家出生点
                player.transform.position = new Vector2(float.Parse(strs[0]), float.Parse(strs[1]));
                //处理分段数据
                sections = Info.f_sections.Split(",");//1 2 3
                //从第一个分段开始
                nowSectionsIndex = 0;
                //初始化当前第一个分段
                InitSectinInfo(int.Parse(sections[nowSectionsIndex]));
            });
        });

        //去显示战斗场景UI
        UIManager.GetInstance().ShowPanel<FightPanel>("FightPanel");

        //监听怪物死亡事件，当怪物死亡时会分发事件，我们处理逻辑即可
        //对于事件中心来说 不要直接写成那不达表达式，因为事件有加就有减
        EventCenter.GetInstance().AddEventListener<int>("MonsterDead", CheckMonsterDead);
        //玩家死亡监听
        EventCenter.GetInstance().AddEventListener("GameOver", CheckPlayerDead);
    }

    /// <summary>
    /// 用来初始化分段信息
    /// 创建怪物
    /// 创建物件
    /// </summary>
    /// <param name="id"></param>
    private void InitSectinInfo(int id)
    { 
        //获取当前分段的信息
        T_level_Section_Info info = BinaryDataMgr.Instance.GetTable<T_level_Section_InfoContainer>().dataDic[id];
        //创建怪物和物体代码类似 而且经常使用 可以封装，提共API外部使用
        #region 创建怪物
        //创建怪物
        string[] monsterInfos = info.T_monster_info.Split(";");
        //记录当前分段有多少只怪物
        monsterNum=monsterInfos.Length;

        #region 有闭包问题
        //for (int i = 0; i < monsterInfos.Length; i++)
        //{
        //    //0索引：怪物ID
        //    //1索引：x坐标
        //    //2索引：y坐标
        //    string[] monsterInfo = monsterInfos[i].Split(",");
        //    //创建怪物
        //    T_MonsterLev_Info monster = BinaryDataMgr.Instance.GetTable<T_MonsterLev_InfoContainer>().dataDic[id];
        //    //根据怪物模型数据 动态的异步创建怪物对象
        //    PoolMgr.GetInstance().GetObj(monster.f_modeel_path, (monsterobj) =>
        //    {
        //        //设置怪物位置
        //        monsterobj.transform.position = new Vector2(float.Parse(monsterInfo[1]), float.Parse(monsterInfo[2]));
        //        //获取对象上的怪物脚本 把怪物ID传入 进行下一个属性的初始化
        //        MonsterObject monsterobject = monsterobj.GetComponent<MonsterObject>();
        //        monsterobject.InitProperty(int.Parse(monsterInfo[0]));
        //        Debug.Log($"怪物ID={monsterInfo[0]}, 类型={monster.f_type}");
        //        //得到有多少boss
        //        if (monster.f_type == 2 )
        //            ++bossNum;
        //    });
        //}
        #endregion

        for (int i = 0; i < monsterInfos.Length; i++)
        {
            // 1. 解析怪物信息
            string[] currentMonsterInfo = monsterInfos[i].Split(',');
            int monsterId = int.Parse(currentMonsterInfo[0]);
            Vector2 spawnPos = new Vector2(
                float.Parse(currentMonsterInfo[1]),
                float.Parse(currentMonsterInfo[2])
            );

            // 2. 从配置表获取怪物数据（替换原来的 GetMonsterData）
            T_MonsterLev_Info monsterData = BinaryDataMgr.Instance
                .GetTable<T_MonsterLev_InfoContainer>()
                .dataDic[monsterId]; // 直接通过ID读取配置表

            // 3. 异步加载预制体
            PoolMgr.GetInstance().GetObj(monsterData.f_modeel_path, (obj) =>
            {
                // 4. 设置位置和属性
                obj.transform.position = spawnPos;
                MonsterObject monsterObj = obj.GetComponent<MonsterObject>();
                monsterObj.InitProperty(monsterId); // 初始化怪物属性

                // 5. 调试日志
                Debug.Log($"生成怪物: ID={monsterId}, 类型={monsterData.f_type}, 位置={spawnPos}");

                // 6. 如果是Boss，计数
                if (monsterData.f_type == 2)
                {
                    ++bossNum;
                    Debug.Log($"Boss生成: 当前Boss数量={bossNum}");
                }
            });
        }
        #endregion

        # region 创建物件
        //如果没有配置物件 就不处理
        if (info.f_thing_info != "")
        {
            //创建物体
            string[] things = info.f_thing_info.Split(";");
            //遍历获取物的物件信息（物件ID，x坐标，y坐标）
            for (int i = 0; i < things.Length; i++)
            {
                //0 id
                //1 x坐标
                //2 y坐标
                string[] thing = things[i].Split(",");
                //一个根据ID 来读取物件表中的数据
                T_thing_Info tingInfo = BinaryDataMgr.Instance.GetTable<T_thing_InfoContainer>().dataDic[int.Parse(thing[0])];
                //根据表中数据 动态创建箱子的预设体
                PoolMgr.GetInstance().GetObj(tingInfo.f_model_res, (tingobj) =>
                {
                    //设置怪物位置
                    tingobj.transform.position = new Vector2(float.Parse(thing[1]), float.Parse(thing[2]));
                    //初始化
                    ThingObject thingObject = tingobj.GetComponent<ThingObject>();
                    thingObject.Init(tingInfo);
                });
            }
        }
        #endregion

        #region 获取边界
        string[] rects=info.f_range.Split(",");
        sectionRect = new SectionRect(float.Parse(rects[0]),
                                      float.Parse(rects[1]),
                                      float.Parse(rects[2]),
                                      float.Parse(rects[3]));
        #endregion

        #region 播放背景音乐
        MusicMgr.GetInstance().PlayBkMusic(info.f_bk_music);
        #endregion


        ++nowSectionsIndex;
    }
    /// <summary>
    /// 判断x坐标是否超出了 左右边界
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public bool CheckOutSectionRectX(float x)
    {
        //如果是推图状态左边界变成了 动态左边界
        if (isPush)
        {
            if(x < cameraLeftX || x > sectionRect.right)
            {
                return true;
            }
        }
        else
        {
            //小于左边界，大于右边界 就是越界
            if (x < sectionRect.left || x > sectionRect.right)
                return true;
        }

        return false;
    }
    /// <summary>
    /// 判断y坐标是否超出了 上下边界
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    public bool CheckOutSectionRectY(float y)
    {
        //小于下边界，大于上边界 就是越界
        if (y<sectionRect.bottom || y > sectionRect.top)
            return true;
        return false;
    }

    public void CheckPushOver()
    {
        //当动态左边界与下一分段的左边界时 推图结束
        if(cameraLeftX >= sectionRect.left)
        {
            isPush = false;
            //隐藏推图标志
            UIManager.GetInstance().GetPanel<FightPanel>("FightPanel").showOrHideGOGOGO(false);
        }
    }

    /// <summary>
    /// 用于怪物死亡监听处理逻辑
    /// </summary>
    private void CheckMonsterDead(int monstertype)
    {
        --monsterNum;
        if (monstertype ==2 )
        {
            --bossNum;
            //当Boss死亡且通关条件是boss死亡 并且是最后一个分段 才算结束
            if(bossNum <= 0 && passType == E_Pass_Type.Boss_Dead &&
                nowSectionsIndex == sections.Length)
            {
                //boss死亡时进行结算处理
                UIManager.GetInstance().ShowPanel<WinPanel>("WinPanel",E_UI_Layer.Top ,(panel) =>
                {
                    panel.InitInfo(moneynum, gemnum, num);
                });
            }

        }
        //当前分段怪物死亡了
        if(monsterNum == 0)
        {
            //切换分段 或者相关逻辑

            //进入下一个分段的加载
            //判断是不是最后一关
            if (nowSectionsIndex == sections.Length)
            {
                //关卡最后一关的逻辑处理
                //如果当前通关的条件是所有怪物死亡 我们就可以在这里写逻辑
                if(passType == E_Pass_Type.All_Monster_Dead)
                {
                    //显示结算界面
                    UIManager.GetInstance().ShowPanel<WinPanel>("WinPanel", E_UI_Layer.Top, (panel) =>
                    {
                        panel.InitInfo(moneynum, gemnum, num);
                    });
                }
            }
            else
            {
                //显示gogogo 图标，只有还有下一个图标才会有这个
                //Debug.Log("怪物全部死亡了");
                UIManager.GetInstance().GetPanel<FightPanel>("FightPanel").showOrHideGOGOGO(true);
                InitSectinInfo(int.Parse(sections[nowSectionsIndex]));
                //进入下一个分段
                isPush = true;
                //记录当前动态左边界
                cameraLeftX = Camera.main.ScreenToWorldPoint(Vector3.zero).x;
            }

        }
    }
    /// <summary>
    /// 用于玩家死亡监听处理逻辑
    /// </summary>
    private void CheckPlayerDead()
    {
        //显示失败界面
        UIManager.GetInstance().ShowPanel<GameOverPanel>("GameOverPanel");
    }
    /// <summary>
    /// 玩家复活
    /// </summary>
    public void PlayerRelife()
    {
        //重生
        PlayerObject.player.ReLife();
        //开启输入
        EventCenter.GetInstance().EventTrigger("GameOver");
        //隐藏倒计时界面
        UIManager.GetInstance().HidePanel("GameOverPanel");
        
    }

    /// <summary>
    /// 玩家死亡失败退出的逻辑 回到开始界面
    /// 麻烦的点：清理各种对象引用 ，隐藏UI，
    /// 清理缓存池、资源管理、事件移除，
    /// 还有判空处理 
    /// </summary>
    public void PlayerDeadGameOver()
    {
        Debug.Log("返回界面");
        //清理相关内容
        Clearlevel();
        //加载开始场景
        ScenesMgr.GetInstance().LoadSceneAsyn("BeginScene", () =>
        {
            //切场景之后去隐藏 游戏结束的界面
            //因为目前的UI系统 过场景不会移除
            UIManager.GetInstance().HidePanel("GameOverPanel");//游戏结束隐藏场景UI
        });
    }

    //切换关卡时 会清理的逻辑
    private void Clearlevel()
    {
        #region 事件的移除
        //事件中心的监听 有加就有减
        EventCenter.GetInstance().RemoveEventListener<int>("MonsterDead", CheckMonsterDead);
        EventCenter.GetInstance().RemoveEventListener("GameOver", CheckPlayerDead);
        #endregion

        #region 缓存池的清空
        PoolMgr.GetInstance().Clear();
        #endregion

        #region 把一些静态的滞空
        PlayerObject.player = null;
        #endregion

        UIManager.GetInstance().HidePanel("FightPanel");//隐藏战斗场景UI

    }
}

//分段边界范围
public struct SectionRect
{
    public float right;
    public float left;
    public float top;
    public float bottom;

    public SectionRect(float right ,float left, float top, float bottom)
    {
        this.left = right;
        this.right = left;
        this.top = top;
        this.bottom = bottom;
    }
}

    