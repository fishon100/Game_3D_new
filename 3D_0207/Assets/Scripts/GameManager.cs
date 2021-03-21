
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //一般欄位  載入後會恢復預設值
    //靜態欄位  載入後不會恢復預設值
    //靜態欄位  不會顯示在屬性面板
    private static int WinPlayer;
    private static int WinNpc;

    public static int KillPlayer;
    public static int KillNpc1;
    public static int KillNpc2;
    public static int KillNpc3;
    public static int KillNpc4;

    public static int DeadPlayer;
    public static int DeadNpc1;
    public static int DeadNpc2;
    public static int DeadNpc3;
    public static int DeadNpc4;
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
    private bool GameOver;

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

        if (content == "玩家")
        {
            WinNpc++;
            TextNpc.text = "勝利次數 : " + WinNpc;
            StartCoroutine(showFinal());
        }
        else if (content.Contains("敵方"))
        {
            EnemyCount++;
            if (EnemyCount == 4)
            {
                WinPlayer++;
                TextPlayer.text = "勝利次數 : " + WinPlayer;
                StartCoroutine(showFinal());
            }
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

        GameOver = true;
    }

    private void Replay()
    {
        if (Input.GetKeyDown(KeyCode.Space) && GameOver) SceneManager.LoadScene("遊戲場景");
    }

    private void Update()
    {
        Replay();
    }
}
