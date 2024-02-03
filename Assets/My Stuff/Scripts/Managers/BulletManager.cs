using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public static BulletManager Singleton;

    public GameObject[] bulletHitEffects;
    public float bulletHitEffectLifetime = 0.1f;
    public float BulletMaxTravelDistance = 100;
    private void Awake()
    {
        Singleton = this;
    }
}
