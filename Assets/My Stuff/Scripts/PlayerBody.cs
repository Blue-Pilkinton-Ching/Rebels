using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private Animator anm;
    private SpriteRenderer sr;
    private Collider2D[] PlayerColliders;
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

    int fire = Animator.StringToHash("BodyFire");
    int pistel = Animator.StringToHash("BodyPistel");

    public override void OnNetworkSpawn()
    {
        PlayerColliders = PlayerController.Players[OwnerClientId].GetComponentsInChildren<Collider2D>();

        sr = GetComponent<SpriteRenderer>();
        PlayerController.Players[OwnerClientId].OnTakeDamage += OnTakeDamage;
        PlayerController.Players[OwnerClientId].OnWeaponChange += OnWeaponChange;
        weapon = PlayerController.Players[OwnerClientId].Weapon;

        if (!IsOwner)
        {
            BulletShootAudioSource.spatialBlend = GameManager.Singleton.soundBlend;
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

        transform.rotation = Quaternion.AngleAxis(PlayerController.Players[OwnerClientId].AngleToMouse + 90, Vector3.forward);
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
        if (shooting) return fire;

        return pistel;
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        inputFire = context.action.IsPressed();
        FireShot();
    }

    private void FireShot()
    {
        if (shooting == false)
        {
            aimAngle = -1 * (Mathf.Atan2(GameManager.Singleton.Focus.position.y - transform.position.y, GameManager.Singleton.Focus.position.x - transform.position.x) - ((Random.Range(-weapon.weaponInacurracy, weapon.weaponInacurracy) + 90) * Mathf.Deg2Rad));

            ShootPosition.localPosition = weapon.bulletFireLocation;

            FireShotServerRPC(ShootPosition.position, aimAngle);
            ShootVisuals(ShootPosition.position, aimAngle);

            shooting = true;
        }
    }

    private void ShootVisuals(Vector3 position, float angle)
    {
        AudioManager.PlayVariedAudio(BulletShootAudioSource, weapon.weaponShootClips); 

        bullet = Instantiate(weapon.bullet, position, Quaternion.AngleAxis(-1 * (angle * Mathf.Rad2Deg), Vector3.forward));

        bulletRaycast = Physics2D.RaycastAll(transform.position, new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)), GameManager.Singleton.BulletMaxTravelDistance, BulletLayerMask);

        bullet.GetComponent<Bullet>().Init(weapon.BulletSpeed, bulletRaycast, PlayerColliders);

        if (IsOwner)
        {
            for (int i = 0; i < bulletRaycast.Length; i++)
            {
                if (!PlayerColliders.Contains(bulletRaycast[i].collider) && bulletRaycast[i].collider.CompareTag("Player"))
                {
                    StartCoroutine(DelayDamage(bulletRaycast[i].collider.GetComponent<PlayerController>(), weapon.damageAmount, bulletRaycast[i].distance / weapon.BulletSpeed));
                    break;
                }
            }
        }
    }
    
    [ServerRpc]
    private void FireShotServerRPC(Vector3 position, float aimAngle, ServerRpcParams serverRpcParams = default)
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
        FireShotClientRPC(position, aimAngle, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = ids }});
    }

    [ClientRpc]
    private void FireShotClientRPC(Vector3 position, float aimAngle, ClientRpcParams clientRpcParams)
    {
        ShootVisuals(position, aimAngle);
    }

    IEnumerator DelayDamage(PlayerController target, float damageAmount, float delay)
    {
        yield return new WaitForSeconds(delay);
        AudioManager.PlayVariedAudio(BulletHitAudioSource, weapon.weaponHitClips);
        target.TakeDamage(damageAmount);
    }

    void OnTakeDamage(float damage, bool dead)
    {
        if (!dead)
        {
            sr.DOColor(GameManager.Singleton.PlayerDamageColor, GameManager.Singleton.PlayerDamageFlashTime / 2).SetEase(Ease.OutSine).OnComplete(() =>
                sr.DOColor(Color.white, GameManager.Singleton.PlayerDamageFlashTime / 2).SetEase(Ease.OutSine)
                );
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
