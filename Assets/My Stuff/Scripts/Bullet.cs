using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    protected Vector2 startPos;
    protected RaycastHit2D[] raycastHits;
    protected Collider2D[] ignoreColliders;
    protected float bulletShootDistance;
    protected float bulletSpeed;

    RaycastHit2D raycast;

    public void Start()
    {
        startPos = transform.position;

        bulletShootDistance = GameManager.Singleton.BulletMaxTravelDistance;

        for (int i = 0; i < raycastHits.Length; i++)
        {
            if (!ignoreColliders.Contains(raycastHits[i].collider))
            {
                raycast = raycastHits[i];

                bulletShootDistance = raycast.distance;

                break;
            }
        }

        if (bulletShootDistance != GameManager.Singleton.BulletMaxTravelDistance)
        {
            Invoke("CreateBulletHitEffect", bulletShootDistance / bulletSpeed);

        }
    }
    private void CreateBulletHitEffect()
    {
        Destroy(Instantiate(GameManager.Singleton.bulletHitEffects[Random.Range(0, GameManager.Singleton.bulletHitEffects.Length)], raycast.point, Quaternion.Euler(0, 0, Random.Range(-180, 180))), GameManager.Singleton.bulletHitEffectLifetime);
    }

    public void Init(float bulletSpeed, RaycastHit2D[] raycastHits, Collider2D[] ignoreColliders)
    {
        this.bulletSpeed = bulletSpeed;
        this.raycastHits = raycastHits;
        this.ignoreColliders = ignoreColliders;
    }
    private void Update()
    {
        transform.Translate(Vector2.up * bulletSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, startPos) >= bulletShootDistance)
        {
            Destroy(gameObject);
        }
    }
}
