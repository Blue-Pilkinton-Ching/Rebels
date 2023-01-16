using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInput PlayerInput;
    public Transform Body;
    public Transform Legs;
    public Transform Focus;

    public float MovementSpeed;
    public float ViewFactor;

    public float rotateSpeed;

    private Animator LegsAnim;
    private Animator BodyAnim;

    private Vector2 inputMovement;    
    private bool inputFire;    

    private Camera main;
    private Vector3 mousePos;
    private int oldPantsState;
    private int oldBodyState;

    Vector2 dir;
    float angle;
    int pantsState;
    int bodyState;

    int fire = Animator.StringToHash("BodyFire");
    int pistel = Animator.StringToHash("BodyPistel");

    int pantsIdle = Animator.StringToHash("PantsIdle");
    int pantsRun = Animator.StringToHash("PantsRun");

    private void Awake()
    {
        main = Camera.main;

        LegsAnim = Legs.GetComponent<Animator>();
        BodyAnim = Body.GetComponent<Animator>();
    }

    private void LateUpdate()
    {
        // Mouse Aiming

        mousePos = main.ScreenToWorldPoint(new Vector3(Mathf.Clamp(Input.mousePosition.x, 0, main.pixelWidth), Mathf.Clamp(Input.mousePosition.y, 0, main.pixelHeight), 0));
        Focus.position = mousePos - ((mousePos - transform.position) * ViewFactor);
    }

    private void Update()
    {
        pantsState = GetPantsState();
        bodyState = GetBodyState();

        if (pantsState != oldPantsState)
        {
            LegsAnim.CrossFade(pantsState, 0);
            oldPantsState = pantsState;
        }
        if (bodyState != oldBodyState)
        {
            BodyAnim.CrossFade(bodyState, 0);
            oldBodyState = bodyState;
        }

        dir = mousePos - transform.position;
        angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Body.rotation = Quaternion.AngleAxis(angle + 90, Vector3.forward);

        Legs.rotation = Quaternion.RotateTowards(Legs.rotation, Quaternion.AngleAxis(angle + 90, Vector3.forward), rotateSpeed * Time.deltaTime);

        transform.position += new Vector3(inputMovement.x, inputMovement.y) * MovementSpeed * Time.deltaTime;
    }

    private int GetPantsState()
    {
        if (inputMovement == Vector2.zero) return pantsIdle;

        return pantsRun;
    }
    private int GetBodyState()
    {
        if (inputFire) return fire;

        return pistel;
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        inputMovement = context.ReadValue<Vector2>();
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        inputFire = context.action.IsPressed();
    }
}
