using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public bool IsPaused { get; private set; } = false;
    private Canvas pauseMenu;

    // Update is called once per frame
    private void Start()
    {
        pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu").GetComponent<Canvas>();
    }

    public void OnPauseButton(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            IsPaused = !IsPaused;
            PauseChanged();
        }
    }

    private void PauseChanged()
    {
        pauseMenu.gameObject.SetActive(IsPaused);
    }
}
