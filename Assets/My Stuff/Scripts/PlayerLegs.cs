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
        PlayerController.Players[OwnerClientId].OnTakeDamage += OnTakeDamage;

        if (!IsOwner)
        {
            footstepAudioSource.spatialBlend = GameManager.Singleton.soundBlend;
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
        footstepAudioSource.PlayOneShot(footstepClips[Random.Range(0, footstepClips.Length)]);
    }

    void OnTakeDamage(float damage, bool dead)
    {
        if (!dead)
        {
            sr.DOColor(GameManager.Singleton.PlayerDamageColor, GameManager.Singleton.PlayerDamageFlashTime * 0.25f).SetEase(Ease.OutSine).OnComplete(() =>
                sr.DOColor(Color.white, GameManager.Singleton.PlayerDamageFlashTime * 0.25f).SetEase(Ease.OutSine).SetDelay(GameManager.Singleton.PlayerDamageFlashTime * 0.5f)
                );
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
