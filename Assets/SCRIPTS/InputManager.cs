using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("REFERENCES")]
    private PlayerControls playercontrols;
    private PlayerLocomotion playerLocomotion;
    private GameManager gameManager;

    [Header("INPUTS")]
    [SerializeField] private Vector2 moveInput;
    [SerializeField] private Vector2 cameraInput;
    public bool b_Or_ShiftInput;
    public bool keyboardWalkAlt;
    public bool jumpInput;
    public bool attack_R1;
    public float horizontalInput;
    public float verticalInput;

    public float mouseX;
    public float mouseY;
    public float moveAmount;

    [Header("TIMERS")]
    [SerializeField] private float b_Or_ShiftInputTimer;
    [SerializeField] private float dodgeValidTimer = 0.15f;

    private void Awake()
    {
        playerLocomotion = GetComponent<PlayerLocomotion>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void OnEnable()
    {
        if (playercontrols == null)
        {
            playercontrols = new PlayerControls();

            playercontrols.PlayerMovement.movement.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
            playercontrols.PlayerMovement.movement.canceled += ctx => moveInput = Vector2.zero;

            playercontrols.CameraControls.camera.performed += ctx => cameraInput = ctx.ReadValue<Vector2>();
            playercontrols.CameraControls.camera.canceled += ctx => cameraInput = Vector2.zero;

            playercontrols.PlayerActionButtons.KeyboardWalk.performed += ctx => keyboardWalkAlt = true;
            playercontrols.PlayerActionButtons.KeyboardWalk.canceled += ctx => keyboardWalkAlt = false;

            playercontrols.PlayerActionButtons.JumpAXbutton.performed += ctx => jumpInput = true;
            playercontrols.PlayerActionButtons.JumpAXbutton.canceled += ctx => jumpInput = false;

            playercontrols.PlayerActionButtons.AttackR1.performed += ctx => attack_R1 = true;

            // Sprint/Dodge/ Duck input handling
            playercontrols.PlayerActionButtons.SprintDodge.started += ctx =>
            {
                b_Or_ShiftInput = true;
                b_Or_ShiftInputTimer = 0f; // reset timer at press start
            };

            playercontrols.PlayerActionButtons.SprintDodge.canceled += ctx =>
            {
                b_Or_ShiftInput = false;

                // Check if it was a tap (Dodge/Duck)
                if (b_Or_ShiftInputTimer > 0 && b_Or_ShiftInputTimer < dodgeValidTimer)
                {
                    if (!gameManager.isGroundInteracting && !gameManager.isAirInteracting)
                    {
                        playerLocomotion.dodgeCheck = true;  // Dodge or Duck handled in locomotion
                    }
                }
                else
                {
                    playerLocomotion.dodgeCheck = false;
                }

                playerLocomotion.sprintCheck = false;
                b_Or_ShiftInputTimer = 0;
            };
        }

        playercontrols.Enable();
    }

    private void OnDisable()
    {
        playercontrols.Disable();
    }

    public void HandleAllInputs()
    {
        HandleMoveInput();
        HandleCameraInputs();
        HandleDodgeAndSprintInput();
        HandleJumpInput();
    }

    private void HandleMoveInput()
    {
        horizontalInput = moveInput.x;
        verticalInput = moveInput.y;
        moveAmount = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        #region moveAmount Clamp
        if (moveAmount > 0 && moveAmount < 0.6f || keyboardWalkAlt && moveAmount > 0)
        {
            moveAmount = 0.6f;
        }
        else if (moveAmount > 0.6f && moveAmount < 1)
        {
            moveAmount = 1;
        }
        #endregion
    }

    private void HandleCameraInputs()
    {
        mouseX = cameraInput.x;
        mouseY = cameraInput.y;
    }

    private void HandleDodgeAndSprintInput()
    {
        if (b_Or_ShiftInput)
        {
            b_Or_ShiftInputTimer += Time.deltaTime;

            // Holding long enough = Sprint
            if (b_Or_ShiftInputTimer >= dodgeValidTimer)
            {
                playerLocomotion.sprintCheck = true;
                playerLocomotion.dodgeCheck = false;
            }
        }
    }

    private void HandleJumpInput()
    {
        if (jumpInput)
        {
            jumpInput = false;
            playerLocomotion.HandleJumping();
        }
    }
}
