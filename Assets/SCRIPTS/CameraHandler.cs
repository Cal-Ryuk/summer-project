using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraHandler : MonoBehaviour
{
    [Header("references")]
    [SerializeField] private InputManager inputManager;
    [SerializeField] private Transform targetObject;
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Transform mainCameraObject;
    [SerializeField] private LayerMask collisionLayers;

    [Header("values")]
    [SerializeField] private float followSpeed = 0.1f;
    [SerializeField] private float cameraRotationSpeed = 5;
    [SerializeField] private float mouseSenstivity = 10;
    [SerializeField] private float maxPivot = 35;
    [SerializeField] private float minPivot = -35;
    [SerializeField] private float cameraCollisionRadius = 0.2f;
    [SerializeField] private float cameraCollisionOffset = 0.2f;
    [SerializeField] private float minimumCollisionOffset = 0.2f;
    [SerializeField] private float cameraRecoverySpeed = 100;

    [Header("variables")]
    private float defaultPosition;

    private float pivotAngle;
    private float lookAngle;

    private Vector3 cameraRefVelocity;
    private Vector3 cameraVectorPosition;

    [Header("Screen Shake")]
    [SerializeField] private Transform targetShakeObject;
    [SerializeField] private float shakeDuration = 0.1f;
    // [SerializeField] private float shakeMagnitude = 0.1f;
    [SerializeField] private AnimationCurve curve;
    private Vector3 originalPos;
    [SerializeField] private bool isShaking = false;

    private void Awake()
    {
        targetObject = GameObject.FindWithTag("Player").transform;
        inputManager = targetObject.GetComponent<InputManager>();
        cameraPivot = transform.GetChild(0);
        mainCameraObject = Camera.main.transform;
        defaultPosition = mainCameraObject.localPosition.z;
    }

    public void HandleCameraMovements()
    {
        FollowTarget();
        RotateCamera();
        HandleCameraCollisions();
    }

    private void FollowTarget()
    {
        Vector3 targetPos = targetObject.position;
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref cameraRefVelocity, followSpeed);
    }

    private void RotateCamera()
    {
        lookAngle += inputManager.mouseX * cameraRotationSpeed * mouseSenstivity * Time.deltaTime;
        pivotAngle -= inputManager.mouseY * cameraRotationSpeed * mouseSenstivity * Time.deltaTime;

        pivotAngle = Mathf.Clamp(pivotAngle, minPivot, maxPivot);

        Vector3 rotation = Vector3.zero;
        rotation.x = pivotAngle;
        Quaternion targetRotation = Quaternion.Euler(rotation);
        cameraPivot.localRotation = targetRotation;

        rotation = Vector3.zero;
        rotation.y = lookAngle;
        targetRotation = Quaternion.Euler(rotation);
        transform.rotation = targetRotation;
    }

    private void HandleCameraCollisions()
    {
        float targetPos = defaultPosition;
        RaycastHit hit;

        Vector3 direction = mainCameraObject.position - cameraPivot.position;
        direction.Normalize();

        if (Physics.SphereCast(cameraPivot.transform.position, cameraCollisionRadius, direction, out hit, Mathf.Abs(targetPos), collisionLayers))
        {
            float distance = Vector3.Distance(cameraPivot.position, hit.point);
            targetPos = -(distance - cameraCollisionOffset);
        }

        if (Mathf.Abs(targetPos) < minimumCollisionOffset)
        {
            targetPos = -minimumCollisionOffset;
        }

        cameraVectorPosition.z = Mathf.Lerp(mainCameraObject.localPosition.z, targetPos, cameraRecoverySpeed);
        mainCameraObject.localPosition = cameraVectorPosition;
    }

    public void TriggerShake()
    {
        if (!isShaking)
            StartCoroutine(Shake());
    }

    private IEnumerator Shake()
    {
        isShaking = true;
        originalPos = targetShakeObject.position;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float strength = curve.Evaluate(elapsedTime/shakeDuration);
            targetShakeObject.localPosition = originalPos + Random.insideUnitSphere * strength;
            yield return null;
        }

        targetShakeObject.position = originalPos;
        isShaking = false;
    }
}

