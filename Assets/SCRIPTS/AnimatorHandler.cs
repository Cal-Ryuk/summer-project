using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorHandler : MonoBehaviour
{
    private Animator anim;
    private int horizontal;
    private int vertical;
    private PlayerLocomotion playerLocomotion;
    private GameManager gameManager;

    [Header("Root Motion Settings")]
    [Tooltip("If true, blend root motion with gravity. If false, ignore Y from root motion completely.")]
    public bool blendRootMotionWithGravity = false;

    private void Awake()
    {
        playerLocomotion = GetComponent<PlayerLocomotion>();
        gameManager = FindObjectOfType<GameManager>();
        anim = GetComponent<Animator>();
        horizontal = Animator.StringToHash("horizontal");
        vertical = Animator.StringToHash("vertical");
    }

    public void PlayTargetAnimation(string targetAnimation, bool isGroundInteracting, bool isAirInteracting, bool isUsingRootMotion)
    {
        anim.SetBool("isGroundInteracting", isGroundInteracting);
        anim.SetBool("isAirInteracting", isAirInteracting);
        anim.SetBool("isUsingRootMotion", isUsingRootMotion);
        anim.CrossFade(targetAnimation, 0.2f);
    }

    public void UpdateAnimatorValues(float horizontalMovement, float verticalMovement)
    {
        anim.SetFloat(horizontal, horizontalMovement, 0.1f, Time.deltaTime);
        anim.SetFloat(vertical, verticalMovement, 0.1f, Time.deltaTime);
    }

    private void OnAnimatorMove()
    {
        if (gameManager.isUsingRootMotion)
        {
            playerLocomotion.playerRb.drag = 0;

            Vector3 deltaPos = anim.deltaPosition;
            Vector3 velocity;

            if (blendRootMotionWithGravity)
            {
                // ✅ Option 1: Blend root motion with gravity
                velocity = deltaPos / Time.deltaTime;
                velocity.y = playerLocomotion.playerRb.velocity.y; // keep gravity from physics
            }
            else
            {
                // ✅ Option 2: Ignore root motion Y entirely
                deltaPos.y = 0f;
                velocity = deltaPos / Time.deltaTime;
                velocity.y = playerLocomotion.playerRb.velocity.y; // physics controls Y
            }

            playerLocomotion.playerRb.velocity = velocity;
        }
    }
}
