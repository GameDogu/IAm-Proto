using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    public event Action<Agent> OnEnergyUpdate;
    [SerializeField] string agentName = "";
    public string AgentName =>  agentName;
    [SerializeField] float energy = 10f;
    public float Energy
    {
        get { return energy; }
        protected set
        {
            energy = value;
            OnEnergyUpdate?.Invoke(this);
        }
    }

    internal abstract void TakeEnergyDamage(float v);
}
