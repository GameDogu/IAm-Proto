using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Agent
{
    [SerializeField] Camera playerCamera = null;

    TransformLocalInfo defaultCameraTransformInfo;
    float playerCameraDistance = 0f;

    private void Awake()
    {
        defaultCameraTransformInfo = playerCamera.transform.GetLocalInfo();
        playerCameraDistance = (transform.localPosition - defaultCameraTransformInfo.localPosition).magnitude;
    }

    public void MovePlayerCameraPosition(Vector3 directionFromPlayer)
    {
        var dirLocal =  transform.InverseTransformDirection(directionFromPlayer) * playerCameraDistance; ;

        playerCamera.transform.localPosition = new Vector3(dirLocal.x, defaultCameraTransformInfo.localPosition.y, dirLocal.z);
        playerCamera.transform.LookAt(transform);
    }

    public void ResetCamera()
    {
        playerCamera.transform.SetToTransformInfo(defaultCameraTransformInfo);
    }

    internal override void TakeEnergyDamage(float v)
    {
        Energy -= v;
    }
}
