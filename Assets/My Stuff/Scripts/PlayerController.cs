using System;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerController : NetworkBehaviour
{
    public static PlayerController[] Players;

    public Canvas Canvas;
    public Image bloodyImage;
    public AudioListener AudioListener;
    public Weapon Weapon;
    public GameObject PlayerDeath;
    public AudioSource BulletTakeAudioSource;
    private PlayerInput playerInput;
    private Rigidbody2D rb;
    private Collider2D thisCollider;
    private CinemachineImpulseSource takeDamageImpulseSource;

    [SerializeField]
    private float defaultMovementForce = 2;

    [SerializeField]
    private float defaultHealth = 10;

    [SerializeField]
    private float camOrthogrphicChangeSpeed = 1;
    public bool IsAlive { get; private set; } = true;
    public float AngleToMouse { get; private set; }
    public float Health { get; private set; }
    public Action OnWeaponChange;
    public Action<float, bool> OnTakeDamage;
    public Action OnRespawn;

    private Vector2 inputMovement;
    private Camera main;
    private PauseMenu pauseMenu;
    public Color PlayerDamageColor;
    public float PlayerDamageFlashTime;


    Vector3 mousePos;
    Vector2 dir;
    IReadOnlyList<ulong> connectedClients;
    List<ulong> ids;

    Light2D lightCover;

    int healthID;

    public override void OnNetworkSpawn()
    {
        Players[OwnerClientId] = this;
        playerInput = GetComponent<PlayerInput>();
        thisCollider = GetComponent<Collider2D>();
        takeDamageImpulseSource = GetComponent<CinemachineImpulseSource>();
        lightCover = GetComponent<Light2D>();

        OnWeaponChange += OnThisWeaponChange;
        OnRespawn += OnThisRespawn;

        Health = defaultHealth;

        if (!IsOwner)
        {
            Destroy(playerInput);
            Destroy(AudioListener);
            Destroy(lightCover);
            Destroy(takeDamageImpulseSource);
            Destroy(bloodyImage);
            return;
        }

        healthID = Shader.PropertyToID("_Health");

        main = Camera.main;
        Canvas.worldCamera = main;
        Canvas.sortingLayerName = "Bloody UI";

        bloodyImage.material.SetFloat(healthID, Health / defaultHealth);

        rb = GetComponent<Rigidbody2D>();

        pauseMenu = GameManager.Singleton.PauseMenu;
    }

    private void Start()
    {
        OnWeaponChange.Invoke();
    }

    private void OnThisWeaponChange()
    {
        DOTween
            .To(
                () => GameManager.Singleton.Camera.m_Lens.OrthographicSize,
                x => GameManager.Singleton.Camera.m_Lens.OrthographicSize = x,
                Weapon.CameraOrthographicSize,
                camOrthogrphicChangeSpeed
            )
            .SetEase(Ease.OutSine);
    }

    private void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        dir = mousePos - transform.position;
        AngleToMouse = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        mousePos = main.ScreenToWorldPoint(
            new Vector3(
                Mathf.Clamp(Input.mousePosition.x, 0, main.pixelWidth),
                Mathf.Clamp(Input.mousePosition.y, 0, main.pixelHeight),
                0
            )
        );

        if (IsAlive && !pauseMenu.gameObject.activeInHierarchy)
        {
            GameManager.Singleton.Focus.position =
                mousePos - ((mousePos - transform.position) * Weapon.ViewFactor);
        }
        float intensity = Mathf.Pow(Mathf.Cos(Mathf.Pow(Mathf.Sin((NetworkManager.Singleton.ServerTime.TimeAsFloat / WeatherController.Singleton.DayLength) + 1), 10)), 1419);
        lightCover.falloffIntensity = intensity;
    }

    private void FixedUpdate()
    {
        if (!IsOwner)
        {
            return;
        }

        if (Health > 0 && IsAlive && !pauseMenu.gameObject.activeInHierarchy)
        {
            rb.AddForce(
                new Vector2(inputMovement.x, inputMovement.y)
                    * defaultMovementForce
                    * Weapon.MoveForce,
                ForceMode2D.Force
            );
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        inputMovement = context.ReadValue<Vector2>();
    }

    public void OnPauseButton(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            pauseMenu.ChangePause();
        }
    }

    private void Die()
    {
        IsAlive = false;
        thisCollider.enabled = false;
        Instantiate(PlayerDeath, transform.position, Quaternion.identity);

        if (IsOwner)
        {
            GameManager.Singleton.PlayerDeadCanvas.gameObject.SetActive(true);
        }
    }

    public void TakeDamage(float damage)
    {
        TakeDamageResponse(damage);
        TakeDamageServerRPC(damage);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TakeDamageServerRPC(float damage, ServerRpcParams serverRpcParams = default)
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
        TakeDamageClientRPC(
            damage,
            new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = ids } }
        );
    }

    [ClientRpc]
    private void TakeDamageClientRPC(float damage, ClientRpcParams clientRpcParams)
    {
        TakeDamageResponse(damage);
    }

    private void TakeDamageResponse(float damage)
    {
        Health -= damage;
        OnTakeDamage.Invoke(damage, Health <= 0);

        if (IsOwner)
        {
            AudioManager.PlayVariedAudio(BulletTakeAudioSource, Weapon.weaponTakeClips);
            Vector3 impulse = new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f), 0).normalized * 0.1f;
            takeDamageImpulseSource.GenerateImpulse(impulse);
            bloodyImage.material.SetFloat(healthID, Health / defaultHealth);
        }
        if (Health <= 0)
        {
            Die();
        }
    }

    public void Respawn()
    {
        RespawnServerRPC();
    }

    [ServerRpc]
    private void RespawnServerRPC()
    {
        transform.position = SpawnController.Singleton.GetSpawnLocation();
        RespawnClientRPC();
    }

    [ClientRpc]
    private void RespawnClientRPC()
    {
        IsAlive = true;
        thisCollider.enabled = true;
        OnRespawn.Invoke();
    }

    private void OnThisRespawn()
    {
        Health = defaultHealth;
        bloodyImage.material.SetFloat(healthID, Health / defaultHealth);
    }
}
