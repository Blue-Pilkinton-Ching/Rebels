using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHitEffect : MonoBehaviour
{
    public void Start()
    {
        Destroy(gameObject, GameManager.Singleton.bulletHitEffectLifetime);
    }
}
