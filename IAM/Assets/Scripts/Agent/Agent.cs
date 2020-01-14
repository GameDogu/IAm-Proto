using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Agent : MonoBehaviour
{
    [SerializeField] string agentName = "";
    public string AgentName =>  agentName;
    [SerializeField] float energy = 10f;
    public float Energy
    {
        get { return energy; }
        protected set
        {
            energy = value;
        }
    }

    internal abstract void TakeEnergyDamage(float v);
}
