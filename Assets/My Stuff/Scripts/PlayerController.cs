using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController ClientSingleton;

    public PlayerInput PlayerInput;
    public Transform Focus;

    public float MovementSpeed;
    public float ViewFactor;

    private Vector2 inputMovement;    

    private Camera main;
    private Vector3 mousePos;
    Vector2 dir;
    public float angleToMouse { get; private set; }

    private void Awake()
    {
        main = Camera.main;

        if (ClientSingleton != null)
        {
            Debug.LogError("basd");
        }
        ClientSingleton = this;
    }

    private void LateUpdate()
    {
        // Mouse Aiming

        mousePos = main.ScreenToWorldPoint(new Vector3(Mathf.Clamp(Input.mousePosition.x, 0, main.pixelWidth), Mathf.Clamp(Input.mousePosition.y, 0, main.pixelHeight), 0));
        Focus.position = mousePos - ((mousePos - transform.position) * ViewFactor);
    }

    private void Update()
    {
        dir = mousePos - transform.position;
        angleToMouse = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        transform.position += new Vector3(inputMovement.x, inputMovement.y) * MovementSpeed * Time.deltaTime;
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        inputMovement = context.ReadValue<Vector2>();
    }
}
