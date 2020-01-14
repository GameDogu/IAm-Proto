using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnergyLeechTrigger : MonoBehaviour
{
    public event Action<Agent> OnAgentEnter;
    public event Action<Agent> OnAgentExit;

    [SerializeField] LayerMask triggerMask = -1;
    [SerializeField] float leechRadius = 1f;
    [SerializeField] SphereCollider col;

    private void Awake()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (col == null)
            col = GetComponent<SphereCollider>();
        col.radius = leechRadius;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (triggerMask == (triggerMask | (1 << other.gameObject.layer)))
            OnAgentEnter?.Invoke(other.gameObject.GetComponent<Agent>());
    }

    public void OnTriggerExit(Collider other)
    {
        if (triggerMask == (triggerMask | (1 << other.gameObject.layer)))
            OnAgentExit?.Invoke(other.gameObject.GetComponent<Agent>());
    }

}
