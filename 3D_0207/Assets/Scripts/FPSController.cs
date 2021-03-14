
using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class FPSController : MonoBehaviour
{
    #region 基本腳色欄位
    [Header("移動速度"), Range(0, 500)]
    public float Speed;
    [Header("旋轉速度"), Range(0, 10)]
    public float Turn;
    [Header("上下旋轉速度"), Range(0, 10)]
    public float  UDSpeed;
    [Header("上下旋轉限制範圍")]
    public Vector2 UDRange = new Vector2(1, 10);
    [Header("跳躍高度"), Range(0, 500)]
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


    private Transform Target;

    private Animator ani;
    private Rigidbody rig;


    private void Awake()
    {
        Cursor.visible = false;         //隱藏鼠標
        ani = GetComponent<Animator>();
        rig = GetComponent<Rigidbody>();
        aud = GetComponent<AudioSource>();

        TraCam = transform.Find("攝相機物件").Find("Camera");
        TraMainCar = transform.Find("攝相機物件").Find("Main Camera");
        Target = transform.Find("上下看目標");
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

        float y = Input.GetAxis("Mouse Y");                 //滑鼠左右轉
        Vector3 posTarget = Target.localPosition;                //取得 = 目標.自身高度座標
        posTarget.y += y * Time.deltaTime * UDSpeed;        //目標座標.Y += Y軸累加 * 速度
        posTarget.y = Mathf.Clamp(posTarget.y, UDRange.x, UDRange.y);    //目標座標.Y = 限制範圍(目標座標.Y ，範圍.X, 範圍.Y)
        Target.localPosition = posTarget;                        //目標.位置 = 目標座標.Y



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


    /// <summary>
    /// 血條
    /// </summary>
    private float hp = 100f;
    private float hpMax = 100f;

    [Header("血量與血條")]
    public Image HPImg;
    public Text HPText;

    /// <summary>
    /// 受到傷害
    /// </summary>
    /// <param name="getDamege"></param>
    private void Damege(float getDamege)
    {
        if (hp <= 0) return;  //如果血條規0  則跳過以下程式運作
        hp -= getDamege;

        if (hp <= 0) Dead();

        HPText.text = hp.ToString();     //血條文字.文字 = 浮點數血量.轉呈字串
        HPImg.fillAmount = hp / hpMax;
    }

    private void Dead()
    {
        hp = 0;
        ani.SetTrigger("死亡觸發");
        
        this.enabled = false;                            //此腳本.啟動 = 關閉

        StartCoroutine(CameraMove());
    }

    [Header("攝相機照出死亡動畫")]
    private Transform TraCam;
    private Transform TraMainCar;

    private IEnumerator CameraMove()
    {
        TraMainCar.LookAt(transform);      //攝相機面向
        TraCam.LookAt(transform);

        Vector3 posCam = TraMainCar.position;    //取得 =  攝相機. 座標

        float yCam = posCam.y;                   //取得 = 攝相機.y
        float yUP = yCam + 5;                    //攝相機往上 =  攝相機.y + 5

        //慢慢往上移 每次移動0.05
        while(yCam < yUP)
        {
            //遞增
            yCam += 0.05f;                     
            posCam.y = yCam;                      //更新 三維向量

            TraMainCar.position = posCam;         //更新 攝相機 座標
            TraCam.position = posCam;

            yield return new WaitForSeconds(0.05f);  //等待
        }
    }



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "子彈")
        {
            float damege = collision.gameObject.GetComponent<Bullet>().Attack;
            Damege(damege);
        }
    }
}
