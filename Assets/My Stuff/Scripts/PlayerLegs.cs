using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerLegs : NetworkBehaviour, IDamageable
{
    public AudioSource footstepAudioSource;
    public AudioClip[] footstepClips;

    private Animator anm;
    private SpriteRenderer sr;
    private PlayerController player;

    public float rotateSpeed;


    int pantsState;
    int oldPantsState;

    int pantsIdle = Animator.StringToHash("PantsIdle");
    int pantsRun = Animator.StringToHash("PantsRun");

    Vector2 inputMovement;


    float targetAngle;
    float lastTargetAngle;

    public override void OnNetworkSpawn()
    {
        sr = GetComponent<SpriteRenderer>();
        player = PlayerController.Players[OwnerClientId];
        player.OnTakeDamage += OnTakeDamage;
        player.OnRespawn += OnRespawn;
        player.OnDie += OnDie;

        if (!IsOwner)
        {
            return;
        }

        anm = GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        pantsState = GetPantsState();

        if (pantsState != oldPantsState)
        {
            anm.CrossFade(pantsState, 0);
            oldPantsState = pantsState;
        }

        if (inputMovement != Vector2.zero)
        {
            targetAngle = Mathf.Atan2(inputMovement.y, inputMovement.x) * Mathf.Rad2Deg;

            if (Mathf.Abs(targetAngle - lastTargetAngle) == 180)
            {
                targetAngle = lastTargetAngle;
            }

            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, targetAngle + 90), rotateSpeed * Time.deltaTime);

            lastTargetAngle = targetAngle;
        }
    }
    private int GetPantsState()
    {
        if (inputMovement == Vector2.zero) return pantsIdle;

        return pantsRun;
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        inputMovement = context.ReadValue<Vector2>();
    }
    public void Footstep()
    {
        if (player.IsAlive)
        {
            footstepAudioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
        }
    }

    private void OnRespawn()
    {
        sr.color = new Color(1, 1, 1, 1);
    }

    void OnTakeDamage(float damage)
    {
        if (player.Health.Value - damage > 0)
        {
            sr.DOColor(player.PlayerDamageColor, player.PlayerDamageFlashTime * 0.25f).SetEase(Ease.OutSine).SetId("damage" + OwnerClientId).OnComplete(() =>
                sr.DOColor(Color.white, player.PlayerDamageFlashTime * 0.25f).SetEase(Ease.OutSine).SetDelay(player.PlayerDamageFlashTime * 0.5f)
                    ).SetId("damage" + OwnerClientId);
        }
    }

    void OnDie()
    {
        sr.color = new Color(0, 0, 0, 0);
    }
}
