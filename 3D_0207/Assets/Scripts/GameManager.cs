
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    private int WinPlayer;
    private int WinNpc;

    public int KillPlayer;
    public int KillNpc1;
    public int KillNpc2;
    public int KillNpc3;
    public int KillNpc4;

    public int DeadPlayer;
    public int DeadNpc1;
    public int DeadNpc2;
    public int DeadNpc3;
    public int DeadNpc4;
    [Header("勝利次數-玩家")]
    public Text TextPlayer;
    [Header("勝利次數-敵方")]
    public Text TextNpc;
    [Header("資料-玩家")]
    public Text TextDataPlayer;
    [Header("資料-敵人1")]
    public Text TextDataNpc1;
    [Header("資料-敵人2")]
    public Text TextDataNpc2;
    [Header("資料-敵人3")]
    public Text TextDataNpc3;
    [Header("資料-敵人4")]
    public Text TextDataNpc4;

    [Header("結束畫面")]
    public CanvasGroup group;

    private int EnemyCount;

    /// <summary>
    /// 更新殺敵數量
    /// </summary>
    ///<param name="kill">要更新殺敵數量</param>
    ///<param name="textKill">要更新的介面</param>
    ///<param name="content">要顯示的文字內容</param>
    public void UpdateDataKill(ref int kill ,Text textKill,string content, int dead)  //ref傳送參考的地址-呼叫時也要加
    {
        kill++;
        //殺敵文字.文字 = 名稱內容 + 空格 + 殺敵數 +   |   +死亡數
        textKill.text   = content  + " "  + kill   + " | " + dead;
    }

    /// <summary>
    /// 更新死亡數量
    /// </summary>
    ///<param name="Dead">要更新死亡數量</param>
    /// ///<param name="textdaed">要更新的介面</param>
    ///<param name="content">要顯示的文字內容</param>
    public void UpdateDataDead(int kill, Text textdead, string content,ref int dead)
    {
        dead++;
        //殺敵文字.文字 = 名稱內容 + 空格 + 殺敵數 +   |   +死亡數
        textdead.text = content + " " + kill + " | " + dead;

        if (content == "玩家0") StartCoroutine(showFinal());
        else if (content.Contains("敵方"))
        {
            EnemyCount++;
                if(EnemyCount == 4) StartCoroutine(showFinal());
        }
    }

    private IEnumerator showFinal()
    {
        float a = group.alpha;
        while(a < 1)
        {
            a += 0.1f;
            group.alpha = a;
            yield return new WaitForSeconds(0.1f);
        }
    }
}
