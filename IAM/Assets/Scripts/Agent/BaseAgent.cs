using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseAgent : Agent
{
    [SerializeField] EnergyLeechTrigger trigger = null;
    [SerializeField] LineRenderer leechLine = null;
    Agent agentLeechingFrom;
    private bool leeching;
    [SerializeField] float leechRate = 1f;
    float LeechRate => leechRate * Time.deltaTime;

    // Start is called before the first frame update
    void Start()
    {
        trigger.OnAgentEnter += OnEnter;
        trigger.OnAgentExit += OnExit;
    }

    private void OnEnter(Agent agent)
    {
        if (CheckPlayerEntered(agent))
        {
            StartLeeching(agent);
        }
    }

    private void StartLeeching(Agent agent)
    {
        agentLeechingFrom = agent;
        leeching = true;
        leechLine.enabled = true;
        StartCoroutine(Leech());
    }

    private void OnExit(Agent agent)
    {
        if (CheckPlayerEntered(agent))
        {
            StopLeeching(agent);
        }
    }

    private void StopLeeching(Agent agent)
    {
        agentLeechingFrom = null;
        leechLine.enabled = false;
        leeching = false;
    }

    bool CheckPlayerEntered(Agent agent)
    {
        return agent.gameObject.CompareTag("Player");
    }

    IEnumerator Leech()
    {
        while (leeching)
        {
            UpdateLeechLine();
            LeechFromAgent();
            yield return null;
        }
    }

    private void LeechFromAgent()
    {
        if (agentLeechingFrom.Energy > 0)
        {
            agentLeechingFrom.TakeEnergyDamage(LeechRate);
        }
        else
        {
            StopLeeching(agentLeechingFrom);
        }
    }

    private void UpdateLeechLine()
    {
        leechLine.SetPosition(leechLine.positionCount - 1, transform.InverseTransformPoint(agentLeechingFrom.transform.position));
    }

    internal override void TakeEnergyDamage(float v)
    {
        Energy -= v;
    }
}
