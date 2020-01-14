using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCursorControls : MonoBehaviour
{
    [SerializeField] KeyCode key = KeyCode.Escape;
    [SerializeField] Camera cam;

    int clicked = 0;
    float clickTime = 0;
    [SerializeField] float clickDelay = 0.5f;

    private void Start()
    {
        LockCursor();
        if (cam == null)
            cam = Camera.main;


    }

    private void HandleMouseClick()
    {
        if (Cursor.lockState == CursorLockMode.None && Input.GetKeyDown(KeyCode.Mouse0))
        {
            clicked++;
            if (clicked == 1) clickTime = Time.time;
            if (clicked > 1 && Time.time - clickTime <= clickDelay)
            {
                //double
                clicked = 0;
                clickTime = 0;
                if (cam.rect.Contains(cam.ScreenToViewportPoint(Input.mousePosition)))
                {
                    LockCursor();
                }
            }
            else if (clicked > 2 || Time.time - clickTime > clickDelay) clicked = 0;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(key))
            UnlockCursor();
        HandleMouseClick();
    }


    void LockCursor()
    {
        ChangeCursorState(CursorLockMode.Locked, false);
    }

    void UnlockCursor()
    {
        ChangeCursorState(CursorLockMode.None, true);
    }

    void ChangeCursorState(CursorLockMode mode, bool visibility)
    {
        Cursor.lockState = mode;
        Cursor.visible = visibility;
    }
}
