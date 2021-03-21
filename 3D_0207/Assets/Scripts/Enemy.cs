using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;


public class Enemy : MonoBehaviour
{
    //玩家位置資訊
    private Transform Player;

    //AI導覽代理器
    private NavMeshAgent Nav;

    //動畫管理器
    private Animator Ani;

    //找到gm腳本
    private GameManager gm;

    //時間管理器
    private float Timer;

    [Header("走路速度"),Range(0,100)]
    public float Speed = 20;
    [Header("攻擊範圍"), Range(0, 100)]
    public float RangeAttack = 100;

    [Header("子彈生成的位置")]
    public Transform PointFire;
    [Header("子彈")]
    public GameObject Bullet;
    [Header("子彈速度")]
    public int BulletSpeed = 1000;
    [Header("射擊間隔時間"), Range(0f, 1f)]
    public float FireInternal = 0.1f;
    [Header("面向玩家速度"), Range(0, 10)]
    public float FaceSpeed = 0.5f;

    [Header("子彈目前數量")]
    public int BulletCurrent = 30;
    [Header("子彈總數量")]
    public int BulletTotal = 150;
    [Header("補充子彈彈夾數量")]
    public int BulletClip = 30;
    [Header("子彈補充時間"), Range(0, 5)]
    public float AddBulletTime = 1;

    [Header("攻擊力"), Range(10, 500)]
    public float Attack = 5;

    private float hp = 100f;

    //是否再補充子彈
    private bool isAddBullet;

    private void Awake()
    {
        Nav = GetComponent<NavMeshAgent>();              //取得NAV元件
        Ani = GetComponent<Animator>();
        Player = GameObject.Find("玩家").transform;    //玩家資訊 = 物件.找到("名稱").位置
        Nav.speed = Speed;                             //Nav.速度 = 公開速度
        Nav.stoppingDistance = RangeAttack;            //Nav.停止距離 = 公開攻擊範圍
        
        //暫存 = 取得<gm腳本>
        gm = GetComponent<GameManager>();
    }

    //繪製圖形
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(2, 2, 2, 0.3f);
        Gizmos.DrawSphere(transform.position, RangeAttack);
    }

    private void Update()
    {
        if (isAddBullet) return;

        Track();
        
    }

    /// <summary>
    /// 追蹤玩家
    /// </summary>
    private void Track()
    {
        //AI NAV.設定目的定(玩家.位置)
        Nav.SetDestination(Player.position);

        //如果 Nav.剩下的距離 > 攻擊範圍
        if (Nav.remainingDistance > RangeAttack)
        {
            Ani.SetBool("跑步開關",true);
        }
        else
        {
            Ani.SetBool("跑步開關", false);
            Fire();
        }
    }

    

    /// <summary>
    /// 開槍
    /// </summary>
    private void Fire()
    {
        Ani.SetBool("跑步開關", false);
        Ani.SetTrigger("攻擊觸發");
        if (Timer >= FireInternal)
        {
            Timer = 0;                                                                      //時間歸0
            GameObject temp = Instantiate(Bullet, PointFire.position, PointFire.rotation);  //子彈暫存 = 生成物件(子彈物件，子彈生成座標，子彈生成角度)
            temp.GetComponent<Rigidbody>().AddForce(PointFire.right * -BulletSpeed);        //暫存子彈.取得元件<鋼體>().添加推力(子彈生成位置.前方 * 子彈速度)
                                                                                            
            temp.GetComponent<Bullet>().Attack = Attack;                                    //暫存子彈.取得子彈腳本<子彈腳本>().傷害 = 敵人傷害
            temp.name += name;
            ManageBulletCount();
        }
        else
        {
            Timer += Time.deltaTime;
            FaceToPlayer();
        }
    }

    /// <summary>
    /// 管理子彈
    /// </summary>
    private void ManageBulletCount()
    {
        BulletCurrent--;
        if (BulletCurrent < 0)
        {
            StartCoroutine(AddBulletDelay());
        }
    }

    /// <summary>
    /// 補充子彈協程
    /// </summary>
    /// <returns></returns>
    private IEnumerator AddBulletDelay()
    {
        Ani.SetTrigger("裝彈夾觸發");
        isAddBullet = true;
        yield return new WaitForSeconds(AddBulletTime);
        isAddBullet = false;
        BulletCurrent += BulletClip;
        
    }

    /// <summary>
    /// 面對玩家
    /// </summary>
    private void FaceToPlayer()
    {
        //角度資訊   暫存    = 角度資訊.看向量角度(玩家.位置 - 自身.位置)
        Quaternion faceAngle = Quaternion.LookRotation(Player.position - transform.position);
        //角度資訊.差值(自身.軸向，暫存 ，時間累加 * 轉向速度)
        Quaternion.Lerp(transform.rotation, faceAngle, Time.deltaTime * FaceSpeed);
    }

    /// <summary>
    /// 受到傷害
    /// </summary>
    /// <param name="getDamege"></param>
    private void Damege(float getDamege)
    {
        if (hp <= 0) return;
        hp -= getDamege;
        Ani.SetTrigger("受傷觸發");

        if (hp <= 0) Dead();

    }

    private void Dead()
    {
        
        Ani.SetTrigger("死亡觸發");
        GetComponent<CapsuleCollider>().enabled = false; //取得此物件的碰撞器.啟動 = 關閉
        GetComponent<SphereCollider>().enabled = false;
        this.enabled = false;                            //此腳本.啟動 = 關閉

        //要更新玩家殺敵數量
        //gm腳本.裡面的更新殺敵數(腳本.玩家殺敵，腳本.文字玩家殺敵數，"玩家"，腳本.玩家死亡數
        gm.UpdateDataKill(ref GameManager.KillPlayer, gm.TextDataPlayer, "玩家", GameManager.DeadPlayer);

        if (name == "敵人1") gm.UpdateDataDead(GameManager.KillNpc1, gm.TextDataNpc1, "敵方1", ref GameManager.DeadNpc1);
        else if (name == "敵人2") gm.UpdateDataDead(GameManager.KillNpc2, gm.TextDataNpc2, "敵方2", ref GameManager.DeadNpc2);
        else if (name == "敵人3") gm.UpdateDataDead(GameManager.KillNpc3, gm.TextDataNpc3, "敵方3", ref GameManager.DeadNpc3);
        else if (name == "敵人4") gm.UpdateDataDead(GameManager.KillNpc4, gm.TextDataNpc4, "敵方4", ref GameManager.DeadNpc4);

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "子彈")
        {
            float damege = collision.gameObject.GetComponent<Bullet>().Attack;

            //如果(碰撞.第一個碰撞器.這個碰撞器.種類.圓形
            if (collision.contacts[0].thisCollider.GetType().Equals(typeof(SphereCollider)))
            {
                print("暴頭");
                Damege(100); //受到100傷害
            }
            Damege(damege);
        }
    }

   
}
