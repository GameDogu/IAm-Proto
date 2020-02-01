using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody))]
public class Player : Agent
{
    [SerializeField] Camera playerCamera = null;
    public Camera Camera => playerCamera;
    [SerializeField] Collider playerCollider = null;
    public Collider Collider => playerCollider;
    [SerializeField] Rigidbody body = null;
    public Rigidbody Body => body;

    [SerializeField] PlayerCollisionHandler collisionHandler = null;
    public PlayerCollisionHandler CollisionHandler => collisionHandler;
    [SerializeField] MovementStateMachine movementStateMachine = null;
    public MovementStateMachine MovementStateMachine => movementStateMachine;

    TransformLocalInfo defaultCameraTransformInfo;
    float playerCameraDistance = 0f;

    private void Awake()
    {
        if (body == null)
            body = GetComponent<Rigidbody>();
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
