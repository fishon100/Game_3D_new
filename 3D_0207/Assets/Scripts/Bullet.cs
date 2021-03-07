using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    /// <summary>
    /// 子彈的傷害
    /// </summary>
    public float Attack;

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(gameObject);
    }
}
