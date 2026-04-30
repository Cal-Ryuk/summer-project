using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private PlayerLocomotion playerLocomotion;
    [SerializeField] private CameraHandler cameraHandler;
    [SerializeField] private Animator animator; 
    public bool isGroundInteracting;
    public bool isAirInteracting;
    public bool isUsingRootMotion;


    private void Awake()
    {
        if (instance == null) instance = this;

        else Destroy(gameObject);

        inputManager = FindObjectOfType<InputManager>();
        playerLocomotion = FindObjectOfType<PlayerLocomotion>();
        cameraHandler = FindObjectOfType<CameraHandler>();
    }

    private void Update()
    {
        inputManager.HandleAllInputs();
    }

    private void FixedUpdate()
    {
        playerLocomotion.HandlePlayerLocomotion();
    }
    private void LateUpdate()
    {
        cameraHandler.HandleCameraMovements();
        isGroundInteracting = animator.GetBool("isGroundInteracting");
        isAirInteracting = animator.GetBool("isAirInteracting");
        isUsingRootMotion = animator.GetBool("isUsingRootMotion");
        animator.SetBool("isGrounded", playerLocomotion.isGrounded);
        playerLocomotion.isJumping = animator.GetBool("isJumping");
    }
}
