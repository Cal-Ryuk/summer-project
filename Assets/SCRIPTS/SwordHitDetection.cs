using UnityEngine;

public class SwordHitDetection : MonoBehaviour
{
    [Header("Settings")]
    private CameraHandler cameraHandler;
    private PlayerLocomotion playerLocomotion;
    public LayerMask enemyLayer;
    public float damageAmount = 10f;
    public bool isActive = false;
    // [SerializeField] private float hitStopDuration = 0.2f;

    private void Start()
    {
        cameraHandler = FindObjectOfType<CameraHandler>();
        playerLocomotion = FindObjectOfType<PlayerLocomotion>();
    }

    public void EnableHitBox()
    {
        isActive = true;
    }
    public void DisableHitBox()
    {
        isActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        if ((enemyLayer & (1 << other.gameObject.layer)) == 0) return;

        if (playerLocomotion.currentAttackHitStop)
        {
            HitStop.instance.Stop(playerLocomotion.currentAttackHitStopDuration);
        }
        cameraHandler.TriggerShake();
    }
}
