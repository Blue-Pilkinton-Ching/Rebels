using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class PlayerBody : NetworkBehaviour, IDamageable
{
    public AudioSource BulletShootAudioSource;
    public AudioSource BulletHitAudioSource;
    public Transform ShootPosition;
    public LayerMask BulletLayerMask;
    public LayerMask BulletChanceLayerMask;
    private Animator anm;
    private SpriteRenderer sr;
    private PlayerController player;
    private CinemachineImpulseSource shootImpulse;

    [Tooltip(
        "This array should contain the colliders of the player that shot, and any vehicals they are in"
    )]
    private Collider2D[] IgnoreColliders;
    private Weapon weapon;

    private bool shooting = false;
    private bool inputFire;
    private int oldBodyState;

    int bodyState;
    float aimAngle;
    IReadOnlyList<ulong> connectedClients;
    List<ulong> ids;

    GameObject bullet;
    RaycastHit2D[] bulletRaycast;
    RaycastHit2D raycast;

    int fire = Animator.StringToHash("BodyFire");
    int pistel = Animator.StringToHash("BodyPistel");

    public override void OnNetworkSpawn()
    {
        IgnoreColliders = PlayerController
            .Players[OwnerClientId]
            .GetComponentsInChildren<Collider2D>();

        sr = GetComponent<SpriteRenderer>();
        shootImpulse = GetComponent<CinemachineImpulseSource>();

        player = PlayerController.Players[OwnerClientId];

        player.OnTakeDamage += OnTakeDamage;
        player.OnWeaponChange += OnWeaponChange;
        player.OnRespawn += OnRespawn;

        weapon = PlayerController.Players[OwnerClientId].Weapon;

        if (!IsOwner)
        {
            Destroy(shootImpulse);
            return;
        }

        anm = GetComponent<Animator>();
    }

    private void OnWeaponChange()
    {
        if (IsOwner)
        {
            anm.speed = 1 * weapon.MoveForce;
        }
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        bodyState = GetBodyState();
        if (bodyState != oldBodyState)
        {
            anm.CrossFade(bodyState, 0);
            oldBodyState = bodyState;
        }

        transform.rotation = Quaternion.AngleAxis(
            PlayerController.Players[OwnerClientId].AngleToMouse + 90,
            Vector3.forward
        );
    }

    public void ShootFinished()
    {
        shooting = false;

        if (inputFire)
        {
            FireShot();
        }
    }

    private int GetBodyState()
    {
        if (shooting)
            return fire;

        return pistel;
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        inputFire = context.action.IsPressed();
        FireShot();
    }

    private void FireShot()
    {
        if (shooting == false && player.IsAlive)
        {
            aimAngle =
                -1
                * (
                    Mathf.Atan2(
                        GameManager.Singleton.Focus.position.y - transform.position.y,
                        GameManager.Singleton.Focus.position.x - transform.position.x
                    )
                    - (
                        (Random.Range(-weapon.weaponInacurracy, weapon.weaponInacurracy) + 90)
                        * Mathf.Deg2Rad
                    )
                );

            ShootPosition.localPosition = weapon.bulletFireLocation;

            int seed = System.Environment.TickCount;

            float x = Mathf.Cos(aimAngle + 90);
            float y = Mathf.Sin(aimAngle + 90);

            shootImpulse.GenerateImpulse(new Vector3(x, y).normalized * 0.05f);

            FireShotServerRPC(ShootPosition.position, aimAngle, seed);
            ShootVisuals(ShootPosition.position, aimAngle, seed);

            shooting = true;
        }
    }

    private void ShootVisuals(Vector3 position, float angle, int seed)
    {
        Random.InitState(seed);

        bulletRaycast = Physics2D.RaycastAll(
            transform.position,
            new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)),
            BulletManager.Singleton.BulletMaxTravelDistance,
            BulletLayerMask
        );
        bullet = Instantiate(
            weapon.bullet,
            position,
            Quaternion.AngleAxis(-1 * (angle * Mathf.Rad2Deg), Vector3.forward)
        );

        for (int i = 0; i < bulletRaycast.Length; i++)
        {
            if (IgnoreColliders.Contains(bulletRaycast[i].collider))
            {
                if (i == bulletRaycast.Length - 1)
                {
                    bullet.GetComponent<Bullet>().Init(weapon.BulletSpeed);
                    break;
                }
            }
            else
            {
                raycast = bulletRaycast[i];

                if (bulletRaycast[i].collider.CompareTag("Player") && IsOwner)
                {
                    StartCoroutine(
                        DelayDamage(
                            bulletRaycast[i].collider.GetComponent<PlayerController>(),
                            weapon.damageAmount,
                            bulletRaycast[i].distance / weapon.BulletSpeed
                        )
                    );
                }
                else if (BulletChanceLayerMask.Includes(bulletRaycast[i].collider.gameObject.layer))
                {
                    float chance = weapon.bulletChanceCollide.Evaluate(bulletRaycast[i].distance);
                    float perc = Random.Range(0, 100);

                    if (perc >= chance)
                    {
                        if (i == bulletRaycast.Length - 1)
                        {
                            bullet.GetComponent<Bullet>().Init(weapon.BulletSpeed);
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                bullet.GetComponent<Bullet>().Init(weapon.BulletSpeed, raycast);
                break;
            }
        }

        AudioManager.PlayVariedAudio(BulletShootAudioSource, weapon.weaponShootClips);
    }

    [ServerRpc]
    private void FireShotServerRPC(
        Vector3 position,
        float aimAngle,
        int seed,
        ServerRpcParams serverRpcParams = default
    )
    {
        connectedClients = NetworkManager.ConnectedClientsIds;
        ids = new List<ulong>();

        for (int i = 0; i < connectedClients.Count; i++)
        {
            if (connectedClients[i] != serverRpcParams.Receive.SenderClientId)
            {
                ids.Add(connectedClients[i]);
            }
        }
        FireShotClientRPC(
            position,
            aimAngle,
            seed,
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = ids } }
        );
    }

    [ClientRpc]
    private void FireShotClientRPC(
        Vector3 position,
        float aimAngle,
        int seed,
        ClientRpcParams clientRpcParams
    )
    {
        ShootVisuals(position, aimAngle, seed);
    }

    IEnumerator DelayDamage(PlayerController target, float damageAmount, float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.PlayVariedAudio(BulletHitAudioSource, weapon.weaponHitClips);
        target.TakeDamage(damageAmount);
    }

    private void OnRespawn()
    {
        sr.color = new Color(1, 1, 1, 1);
    }

    void OnTakeDamage(float damage, bool dead)
    {
        if (dead)
        {
            sr.color = new Color(0, 0, 0, 0);
        }
        else
        {
            sr.DOColor(player.PlayerDamageColor, player.PlayerDamageFlashTime / 2)
                .SetEase(Ease.OutSine)
                .OnComplete(
                    () =>
                        sr.DOColor(Color.white, player.PlayerDamageFlashTime / 2)
                            .SetEase(Ease.OutSine)
                );
        }
    }
}
