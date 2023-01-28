using UnityEngine;

public class Bullet : MonoBehaviour
{
    protected RaycastHit2D[] raycastHits;
    protected float bulletSpeed;
    protected Vector2 bulletHitSpawnPoint;
    protected bool hitObject { get; private set; } = false;

    public void Init(float bulletSpeed, RaycastHit2D raycastHit)
    {
        this.bulletSpeed = bulletSpeed;
        bulletHitSpawnPoint = raycastHit.point;

        hitObject = true;

        Destroy(gameObject, raycastHit.distance / bulletSpeed);
    }
    private void OnDestroy()
    {
        if (hitObject)
        {
            Destroy(Instantiate(BulletManager.Singleton.bulletHitEffects[Random.Range(0, BulletManager.Singleton.bulletHitEffects.Length)], bulletHitSpawnPoint, Quaternion.Euler(0, 0, Random.Range(-180, 180))), BulletManager.Singleton.bulletHitEffectLifetime);
        }
    }

    public void Init(float bulletSpeed)
    {
        this.bulletSpeed = bulletSpeed;

        Destroy(gameObject, BulletManager.Singleton.BulletMaxTravelDistance / bulletSpeed);
    }

    private void Update()
    {
        transform.Translate(Vector2.up * bulletSpeed * Time.deltaTime);
    }
}
