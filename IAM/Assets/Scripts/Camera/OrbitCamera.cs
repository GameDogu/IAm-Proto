using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class OrbitCamera : MonoBehaviour
{

    // Input
    //Orbit Mouse X
    //Orbit Mouse Y

    [SerializeField] Camera cam = default;
    [SerializeField] Transform focus = default;
    [SerializeField, Range(0.1f, 20f)] float distance=5f;
    [SerializeField, Min(0f)] float focusRadius = 1f;
    [SerializeField, Range(0f, 1f)] float focusCentering = 0.5f;

    [SerializeField]Vector2 orbitAngles = new Vector2(45f, 0f);
    [SerializeField,Range(1f,360f)] float rotationSpeed = 90f;
    [SerializeField, Range(-89f, 89f)] float minVerticalAngle = -30f, maxVerticalAngle = 60f;
    [SerializeField, Min(0f)] float alignDelay =5f;
    [SerializeField, Range(0f, 90f)] float alignSmoothRange = 45f;

    [SerializeField] LayerMask obstructionMask = -1;

    Vector3 focusPnt,prevFocusPnt;
    float lastManualRotationTime;

    Vector3 CameraHalfExtends
    {
        get
        {
            Vector3 halfExtends;

            halfExtends.y = cam.nearClipPlane * Mathf.Tan(0.5f * Mathf.Deg2Rad * cam.fieldOfView);
            halfExtends.x = halfExtends.y * cam.aspect;
            halfExtends.z = 0f;
            return halfExtends;
        }
    }

    private void Awake()
    {
        if (cam == null)
            cam = GetComponent<Camera>();
        focusPnt = focus.position;
        transform.localRotation = Quaternion.Euler(orbitAngles);
    }

    private void OnValidate()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (maxVerticalAngle < minVerticalAngle)
            maxVerticalAngle = minVerticalAngle;

        if (focus != null)
        {
            LateUpdate();
        }
    }

    private void LateUpdate()
    {
        UpdateFocusPoint();
        Quaternion lookRot;
        if (ManualRotation()||AutoRotation())
        {
            ConstrainAngles();
            lookRot = Quaternion.Euler(orbitAngles);
        }
        else
        {
            lookRot = transform.localRotation;
        }
        Vector3 lookDir = lookRot * Vector3.forward;
        Vector3 lookPosition = focusPnt - lookDir * distance;

        Vector3 rectOffset = lookDir * cam.nearClipPlane;
        Vector3 rectPos = lookPosition + rectOffset;
        Vector3 castFrom = focus.position;
        Vector3 castLine = rectPos - castFrom;
        float castDist = castLine.magnitude;
        Vector3 castDir = castLine.normalized;

        if (Physics.BoxCast(castFrom,CameraHalfExtends, castDir, out RaycastHit hit, lookRot,castDist,obstructionMask))
        {
            rectPos = castFrom + castDir * hit.distance;
            lookPosition = rectPos - rectOffset;
        }
               
        transform.SetPositionAndRotation(lookPosition, lookRot);
    }

    void UpdateFocusPoint()
    {
        prevFocusPnt = focusPnt;
        Vector3 targetPnt = focus.position;

        if (focusRadius > 0f)
        {
            float distance = Vector3.Distance(targetPnt, focusPnt);

            if (distance > focusRadius)
            {
                //pull camera outside of foc radius
                focusPnt = Vector3.Lerp(targetPnt, focusPnt, focusRadius / distance);
            }
            if (distance > 0.01f && focusCentering > 0f)
            {
                focusPnt = Vector3.Lerp(targetPnt, focusPnt, Mathf.Pow(1 - focusCentering, Time.unscaledDeltaTime));
            }
        }
        else
        {
            focusPnt = targetPnt;
        }
    }

    bool ManualRotation()
    {
        Vector2 rotationInput = new Vector2(Input.GetAxis("Orbit Mouse Y"), Input.GetAxis("Orbit Mouse X"));
        const float e = 0.001f;
        if (rotationInput.x < -e || rotationInput.x > e || rotationInput.y < -e || rotationInput.y > e)
        {
            orbitAngles += rotationInput * rotationSpeed * Time.unscaledDeltaTime;
            lastManualRotationTime = Time.unscaledTime;
            return true;
        }
        return false;
    }

    bool AutoRotation()
    {
        if (Time.unscaledTime - lastManualRotationTime < alignDelay)
        {
            return false;
        }

        Vector2 movement = new Vector2(focusPnt.x - prevFocusPnt.x, focusPnt.z - prevFocusPnt.z);
        float movementSqrMgnt = movement.sqrMagnitude;

        if (movementSqrMgnt < 0.0001f)
        {
            //next to no movement no adjustment needed
            return false;
        }

        float headingAngle = (movement.normalized).GetAngleDeg();

        float deltaAbs = Mathf.Abs(Mathf.DeltaAngle(orbitAngles.y, headingAngle));

        float rotChange = rotationSpeed * Mathf.Min(Time.unscaledDeltaTime,movementSqrMgnt);//dampen tiny motions

        if (deltaAbs < alignSmoothRange)
        {
            rotChange *= deltaAbs / alignSmoothRange;
        }
        else if (180f - deltaAbs < alignSmoothRange)
        {
            rotChange *= (180 - deltaAbs) / alignSmoothRange;
        }

        orbitAngles.y = Mathf.MoveTowardsAngle(orbitAngles.y,headingAngle,rotChange);
        return true;
    }

    void ConstrainAngles()
    {
        orbitAngles.x = Mathf.Clamp(orbitAngles.x, minVerticalAngle, maxVerticalAngle);

        if (orbitAngles.y < 0f)
        {
            orbitAngles.y += 360f;
        }
        else if (orbitAngles.y >= 360f)
        {
            orbitAngles.y -= 360f;
        }
    }
}
