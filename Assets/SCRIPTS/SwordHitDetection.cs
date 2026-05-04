using UnityEngine;

public class SwordHitDetection : MonoBehaviour
{
    [Header("Settings")]
    private CameraHandler cameraHandler;
    public LayerMask enemyLayer;
    public float damageAmount = 10f;
    public bool isActive = false;
    [SerializeField] private float hitStopDuration = 0.08f;

    private void Start()
    {
        cameraHandler = FindObjectOfType<CameraHandler>();
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
        HitStop.instance.Stop(hitStopDuration);
        cameraHandler.TriggerShake();
    }
}
