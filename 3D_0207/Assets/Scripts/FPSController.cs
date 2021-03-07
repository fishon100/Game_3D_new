
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class FPSController : MonoBehaviour
{
    #region 基本腳色欄位
    [Header("移動速度"), Range(0, 2000)]
    public float Speed;
    [Header("旋轉速度"), Range(0, 2000)]
    public float Turn;
    [Header("跳躍高度"), Range(0, 1000)]
    public float jump;
    [Header("地板偵測位移")]
    public Vector3 FloorOffset;
    [Header("地板偵測半徑"), Range(0, 20)]
    public float FloorRadius = 1;
    #endregion

    #region 開槍調整欄位
    [Header("子彈生成的位置")]
    public Transform PointFire;
    [Header("子彈")]
    public GameObject Bullet;
    [Header("子彈速度")]
    public int BulletSpeed = 1000;

    [Header("子彈目前數量")]
    public int BulletCurrent = 30;
    [Header("子彈總數量")]
    public int BulletTotal = 150;
    [Header("補充子彈彈夾數量")]
    public int BulletClip = 30;
   
    [Header("文字:子彈目前數量")]
    public Text TextBulletCurrent;
    [Header("文字:子彈總數量")]
    public Text TextBulletTotal;
    [Header("子彈補充時間"), Range(0, 5)]
    public float AddBulletTime = 1;

    [Header("開槍音效")]
    public AudioClip SoundFire;
    [Header("裝彈夾音效")]
    public AudioClip SoundAddBullet;
    [Header("射擊間隔時間"), Range(0f, 1f)]
    public float FireInternal = 0.1f;

    [Header("攻擊力"), Range(10, 500)]
    public float Attack = 5;

    private AudioSource aud;
    private float timer;

    //是否再補充子彈
    private bool isAddBullet;
    #endregion


    private Animator ani;
    private Rigidbody rig;


    private void Awake()
    {
        Cursor.visible = false;         //隱藏鼠標
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();
    }

    //在遊戲場景畫出偵測圖形
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.position + FloorOffset, FloorRadius);//(自身中心位置 + 偵測位移 ，偵測半徑)
    }

    private void Update()
    {
        Move();
        Jump();
        Fire();
        AddFire();
    }

    /// <summary>
    /// 移動
    /// </summary>
    public void Move()
    {
        //輸入.軸向("垂直")
        float V = Input.GetAxis("Vertical"); //自動偵測 W S 上 下    //前進 = 1    後退 = -1  不動 = 0
        float H = Input.GetAxis("Horizontal"); //自動偵測 A D 左 右    //前進 = 1    後退 = -1  不動 = 0

        //鋼體.移動位置(自身 + 藍色軸向 * 垂直V * 移動速度 * 60秒影格 + 紅色軸向 * 水平H * 移動速度 * 60秒影格)
        rig.MovePosition(transform.position + transform.forward * V * Speed * Time.deltaTime + transform.right * H * Speed * Time.deltaTime);

        float x = Input.GetAxis("Mouse X");                 //滑鼠左右轉
        transform.Rotate(0, x * Time.deltaTime * Turn, 0);

       

    }

    public void Jump()
    {
        //3D 物理碰撞偵測                                                                                 //2D 物理碰撞偵測   
        //物理.覆蓋球體(自身中心位置 + 偵測位移 ，偵測半徑 , 1<<可跳躍圖層)                               //物理.覆蓋圓形(自身中心位置 + 偵測位移 ，偵測半徑)  
        Collider[] hits =  Physics.OverlapSphere(transform.position + FloorOffset, FloorRadius , 1 << 8); //Collider[] hits =  Physics2D.OverlapCircle(transform.position + FloorOffset, FloorRadius);

        //如果 碰撞物件.數量 > 0一件以上   並且   碰撞物件[存在]   並且   按下空白建
        if (hits.Length > 0 && hits[0] && Input.GetKeyDown(KeyCode.Space))
        {
            //重力.添加推力(X，跳躍高度 ，Z)
            rig.AddForce(0, jump, 0);
        }
    }

    /// <summary>
    /// d開槍
    /// </summary>
    public void Fire()
    {
        //按下左鍵 並且 目前子彈數量>0 並且 不是在補充子彈
       if(Input.GetKeyDown(KeyCode.Mouse0) && BulletCurrent > 0 && !isAddBullet)
        {
            //時間 >= 射擊間隔時間 可再射擊
            if (timer >= FireInternal)
            {
                ani.SetTrigger("開槍觸發");
                //時間歸0
                timer = 0;
                //播放射擊聲音
                aud.PlayOneShot(SoundFire, Random.Range(0.5f, 1.2f));
                
                //扣除子彈 更新介面
                BulletCurrent--;
                TextBulletCurrent.text = BulletCurrent.ToString();  //文字:子彈目前數量.文字 = 子彈目前數量.轉成文字()

                //子彈暫存 = 生成物件(子彈物件，子彈生成座標，子彈生成角度)
                GameObject temp = Instantiate(Bullet, PointFire.position, PointFire.rotation);

                //暫存子彈.取得元件<鋼體>().添加推力(子彈生成位置.前方 * 子彈速度)
                temp.GetComponent<Rigidbody>().AddForce(PointFire.up * -BulletSpeed);
                //暫存子彈.取得子彈腳本<子彈腳本>().傷害 = 玩家傷害
                temp.GetComponent<Bullet>().Attack = Attack;
            }
            //時間累加
            else timer += Time.deltaTime;
        }
    }

    /// <summary>
    /// 補充子彈
    /// </summary>
    public void AddFire()
    {
        if (Input.GetKeyDown(KeyCode.R))
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
        ani.SetTrigger("裝彈夾觸發");
        //播放換夾聲音
        aud.PlayOneShot(SoundAddBullet, Random.Range(0.5f, 1.2f));

        isAddBullet = true;
        yield return new WaitForSeconds(AddBulletTime);
        isAddBullet = false;

        if (BulletCurrent < BulletClip)
        {
            //計算補充幾顆子彈
            int add = BulletClip - BulletCurrent;  //增加子彈數量 = 彈夾數量 - 目前子彈數量

            if (BulletTotal >= add)  //總子彈數量 >= 增加子彈數量 
            {
                BulletCurrent += add;//目前子彈數量 += 增加子彈數量
                BulletTotal -= add;  //總子彈數量 -= 增加子彈數量
            }
            else
            {
                BulletCurrent += BulletTotal;  //目前子彈數量 += 總子彈數量
                BulletTotal = 0;               //總子彈數量 = 0
            }
            TextBulletCurrent.text = BulletCurrent.ToString();     //文字:子彈目前數量.文字 = 子彈目前數量.轉成文字()
            TextBulletTotal.text = BulletTotal.ToString();         //文字:總子彈數量.文字 = 總子彈數量量.轉成文字()
        }
    }
}
