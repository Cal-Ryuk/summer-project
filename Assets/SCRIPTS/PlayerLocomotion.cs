using System.Collections;
using System.Collections.Generic;
using UnityEditor.AnimatedValues;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR;

public class PlayerLocomotion : MonoBehaviour
{
    #region VARIABLES

    [Header("REFERENCES")]
    private InputManager inputManager;
    private GameManager gameManager;
    private AnimatorHandler animatorHandler;
    private Animator animator;
    [HideInInspector] public Rigidbody playerRb;
    private Vector3 moveDir = Vector3.zero;
    public Vector3 playerVelocity;


    [Header("GROUNDED SPEEDS")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 5;
    [SerializeField] private float sprintSpeed = 8;
    [SerializeField] private float rotationSpeed = 10;


    [Header("GROUNDED FLAGS")]
    public bool sprintCheck;
    public bool dodgeCheck;


    [Header("IN AIR SPEEDS")]
    [SerializeField] private float inAirMoveSpeed = 5;
    [SerializeField] private float inAirRotationSpeed = 2;


    [Header("GROUND CHECKS")]
    [SerializeField] private Transform groundCheckPos;
    [SerializeField] private Transform trueGroundCheckPos;
    [SerializeField] private float constantGravity = 5f;
    [SerializeField] private float trueGroundDetectionDistance = 0.2f;
    [SerializeField] private float groundCheckRadius = 0.21f;
    [SerializeField] private float groundCheckDistance = 0.35f;
    [SerializeField] private LayerMask groundDetectionLayers;
    public bool isGrounded;
    public bool trueGrounded;

    [Header("SLOPE DETECTION")]
    [SerializeField] private float maxSlopeAngle = 40;
    private RaycastHit slopeHit;
    [SerializeField] private float slopeCheckMaxDis = 0.2f;

    [Header("FALLING/LANDING SPEEDS")]
    [SerializeField] private float fallingVelocity = 150;
    [SerializeField] private float leepingVelocity = 5;
    [SerializeField] private float inAirTimer;
    [SerializeField] private float maxFallingVelocity = -15;
    [SerializeField] private float landAnimPlayTimer;


    [Header("JUMP SPEEDS / CHECKS")]
    [SerializeField] private float jumpHeight = 3;
    [SerializeField] private float gravityIntensity = -15;
    public bool isJumping;
    // REMINDER ---> groundCheckPos is at 0.43 at Y positive at player feet(slightly below knee)

    [Header("COMBAT")]
    public ComboDataSO currentCombo;
    [SerializeField] private int comboCounter;
    [SerializeField] private float comboWindowTimer = 0f;
    // [SerializeField] private float comboWindowDuration = 0.8f;
    [SerializeField] private bool canCombo = false;
    #endregion

    private void Awake()
    {
        playerRb = GetComponent<Rigidbody>();
        animatorHandler = GetComponent<AnimatorHandler>();
        animator = GetComponent<Animator>();
        gameManager = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        inputManager = GetComponent<InputManager>();
    }

    public void HandlePlayerLocomotion()
    {
        HandleFallingAndLanding();
        playerVelocity = playerRb.velocity;

        if (isGrounded)
        {
            if (gameManager.isGroundInteracting)
            {
                // sets player x and z velocity to zero while interacting 
                if (!gameManager.isUsingRootMotion)
                {
                    Vector3 frozenVel = new Vector3(0f, playerRb.velocity.y, 0f);
                    playerRb.velocity = frozenVel;
                }
                return;
            }

            if (!isJumping)
            {
                HandleGroundedMovement();
                HandleGroundedRotation();
                HandlePlayerAnimationValues();
                HandleDuckingAndDodging();
                HandleAttacksAndCombos();
            }
        }
        else // In Air
        {
            if (gameManager.isAirInteracting)
            {
                HandleInAirMovement();
                HandleInAirRotation();
            }
        }
    }

    private void HandleGroundedMovement()
    {
        moveDir = Camera.main.transform.forward * inputManager.verticalInput;
        moveDir += Camera.main.transform.right * inputManager.horizontalInput;

        moveDir.y = 0;
        moveDir.Normalize();

        if (sprintCheck && inputManager.moveAmount > 0.6f)
        {
            moveSpeed = sprintSpeed;
        }
        else
        {
            if (inputManager.moveAmount == 0.6f)
            {
                moveSpeed = walkSpeed;
            }
            else if (inputManager.moveAmount == 1)
            {
                moveSpeed = runSpeed;
            }
        }

        if (OnSlope())
        {
            moveDir = GetSlopeMoveDir() * moveSpeed;
            playerRb.velocity = moveDir;
            constantGravity = 0;
        }
        else
        {
            moveDir *= moveSpeed;
            playerRb.velocity = moveDir;
            constantGravity = 5f;
        }
    }

    private void HandleGroundedRotation()
    {
        Vector3 targetDir = Vector3.zero;

        targetDir = Camera.main.transform.forward * inputManager.verticalInput;
        targetDir += Camera.main.transform.right * inputManager.horizontalInput;

        targetDir.Normalize();
        targetDir.y = 0;

        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }

        Quaternion targetRotationDir = Quaternion.LookRotation(targetDir);
        playerRb.MoveRotation(Quaternion.Slerp(playerRb.rotation, targetRotationDir, rotationSpeed * Time.deltaTime));
    }

    private void HandleInAirMovement()
    {
        moveDir = Camera.main.transform.forward * inputManager.verticalInput;
        moveDir += Camera.main.transform.right * inputManager.horizontalInput;

        moveDir.y = 0;
        moveDir.Normalize();

        Vector3 horizontalVelocity = new Vector3(playerRb.velocity.x, 0f, playerRb.velocity.z);

        Vector3 airPush = moveDir * inAirMoveSpeed * Time.deltaTime;
        horizontalVelocity += airPush;

        float maxAirSpeed = moveSpeed;
        if (horizontalVelocity.magnitude > maxAirSpeed)
        {
            horizontalVelocity = horizontalVelocity.normalized * maxAirSpeed;
        }

        playerRb.velocity = new Vector3(horizontalVelocity.x, playerRb.velocity.y, horizontalVelocity.z);
    }

    private void HandleInAirRotation()
    {
        Vector3 targetDir = Vector3.zero;

        targetDir = Camera.main.transform.forward * inputManager.verticalInput;
        targetDir += Camera.main.transform.right * inputManager.horizontalInput;

        targetDir.Normalize();
        targetDir.y = 0;

        if (targetDir == Vector3.zero)
        {
            targetDir = transform.forward;
        }

        Quaternion targetRotationDir = Quaternion.LookRotation(targetDir);
        playerRb.MoveRotation(Quaternion.Slerp(playerRb.rotation, targetRotationDir, inAirRotationSpeed * Time.deltaTime));
    }

    private void HandleFallingAndLanding()
    {
        RaycastHit hit;
        if (!isGrounded && !isJumping)
        {
            if (!gameManager.isGroundInteracting && !gameManager.isAirInteracting)
            {
                animatorHandler.PlayTargetAnimation("falling", true, true, false); // both interactions are true because we dont want to play falling animation on every frame  
            }

            inAirTimer += Time.deltaTime;

            //Applies terminal velocity so player doesnt keep on accelerating while falling 
            if (playerVelocity.y <= maxFallingVelocity)
            {
                playerVelocity.y = maxFallingVelocity;
                playerRb.velocity = playerVelocity;


                // uncomment this if you want no accelartion even after reaching terminal velocity, right now it adds a bit of acceleration even after reaching terminal velocity
                // if (inAirTimer >= 1)
                // {
                //     inAirTimer = 1;
                // }
            }

            playerRb.AddForce(transform.forward * leepingVelocity);
            playerRb.AddForce(-Vector3.up * fallingVelocity * inAirTimer);
        }

        if (Physics.SphereCast(groundCheckPos.position, groundCheckRadius, Vector3.down, out hit, groundCheckDistance, groundDetectionLayers))
        {
            if (!isGrounded && inAirTimer > landAnimPlayTimer)
            {
                animatorHandler.PlayTargetAnimation("landing", true, false, false);
                Vector3 fallingSpeed = playerRb.velocity;
                fallingSpeed.x = 0;
                fallingSpeed.z = 0;
                playerRb.velocity = fallingSpeed;
            }

            inAirTimer = 0;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
            trueGrounded = false;
        }

        // GRAVITY FOR WHEN PLAYER IS GROUNDED BUT IS HOVERING IN AIR SLIGHTLY ABOEV THE GROUND
        if (isGrounded && !isJumping && !Physics.Raycast(trueGroundCheckPos.position, Vector3.down, out RaycastHit ray, trueGroundDetectionDistance, groundDetectionLayers))
        {
            playerRb.AddForce(-Vector3.up * fallingVelocity);
            trueGrounded = true;
        }
        else if (isGrounded && !isJumping)
        {
            playerRb.AddForce(-Vector3.up * constantGravity);
        }

    }

    public void HandleJumping()
    {
        if (gameManager.isGroundInteracting) return;
        if (isGrounded)
        {
            animator.SetBool("isJumping", true);
            animatorHandler.PlayTargetAnimation("Jump", false, true, false);

            float jumpingVelocity = Mathf.Sqrt(-2 * gravityIntensity * jumpHeight);
            Vector3 playerVelocity = playerRb.velocity;
            playerVelocity.y = jumpingVelocity;

            playerRb.velocity = playerVelocity;
            // playerRb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }
    }

    private void HandleDuckingAndDodging()
    {
        if (gameManager.isUsingRootMotion) return;
        if (dodgeCheck)
        {
            moveDir = Camera.main.transform.forward * inputManager.verticalInput;
            moveDir += Camera.main.transform.right * inputManager.horizontalInput;

            if (inputManager.moveAmount > 0)
            {
                animatorHandler.PlayTargetAnimation("dodge", true, false, true);
                moveDir.y = 0;
                Quaternion rollRotation = Quaternion.LookRotation(moveDir);
                transform.rotation = rollRotation;
            }
            else
            {
                animatorHandler.PlayTargetAnimation("duck", true, false, false);
            }
            dodgeCheck = false;
        }
    }

    private void HandleAttacksAndCombos()
    {
        if (comboWindowTimer > 0)
        {
            comboWindowTimer -= Time.deltaTime;
            if (comboWindowTimer <= 0)
            {
                comboCounter = 0;
                canCombo = false;
            }
        }
        if (gameManager.isGroundInteracting && !canCombo) return;
        if (inputManager.attack_R1)
        {
            if (currentCombo == null || currentCombo.attacks.Length == 0) return;

            canCombo = false;

            if (comboCounter >= currentCombo.attacks.Length) comboCounter = 0;

            ComboAttack attack = currentCombo.attacks[comboCounter];
            animatorHandler.PlayTargetAnimation(attack.animClip.name, true, false, attack.useRootMotion);

            comboWindowTimer = attack.comboWindowDuration;
            comboCounter++;

            if (comboCounter >= currentCombo.attacks.Length) comboCounter = 0;

            // if (comboCounter == 0)
            // {
            //     animatorHandler.PlayTargetAnimation("slash1", true, false, true);
            // }
            // else if (comboCounter == 1)
            // {
            //     animatorHandler.PlayTargetAnimation("slash2", true, false, true);
            // }
            // else if (comboCounter == 2)
            // {
            //     animatorHandler.PlayTargetAnimation("slash3", true, false, true);
            // }
            // comboCounter++;

            // if (comboCounter > 2) comboCounter = 0;

            // comboWindowTimer = comboWindowDuration;
        }
    }

    public void EnableComboWindow()
    {
        canCombo = true;
    }

    public void ResetCombo()
    {
        comboCounter = 0;
        canCombo = false;
        comboWindowTimer = 0f;
    }
    private void HandlePlayerAnimationValues()
    {
        if (sprintCheck && inputManager.moveAmount > 0.6f) //checks for sprint check an if the player is running if player is running then he can sprint  
        {
            inputManager.moveAmount = 2;
        }

        animatorHandler.UpdateAnimatorValues(0, inputManager.moveAmount);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPos == null)
            return;

        Gizmos.color = Color.red;

        // Visualize the cast origin sphere
        Gizmos.DrawWireSphere(groundCheckPos.position, groundCheckRadius);

        // Visualize the path of the SphereCast
        Vector3 castDirection = Vector3.down;
        Vector3 castEndPoint = groundCheckPos.position + castDirection * groundCheckDistance;

        // Bottom sphere at the end of the cast
        Gizmos.DrawWireSphere(castEndPoint, groundCheckRadius);

        // Line connecting top and bottom spheres
        Gizmos.DrawLine(groundCheckPos.position, castEndPoint);
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(trueGroundCheckPos.position, Vector3.down, out slopeHit, slopeCheckMaxDis, groundDetectionLayers))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }

    private Vector3 GetSlopeMoveDir()
    {
        return Vector3.ProjectOnPlane(moveDir, slopeHit.normal).normalized;
    }
}

