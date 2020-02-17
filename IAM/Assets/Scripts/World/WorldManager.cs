using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public event Action OnWorldDifficultyChange; //TODO figure out event data

    public static WorldManager Instance;

    [SerializeField] Player player = default;

    [SerializeField] BucketedEventProviderEven eventProvider = default;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        eventProvider.StartUp();

        player.OnEnergyUpdate += OnPlayerEnergyChanged;

        eventProvider.OnNewBucketHit += OnNewBucketHit;

    }

    private void OnValidate()
    {
        if (eventProvider.DesiredBucketCount != eventProvider.BucketCount)
        {
            eventProvider.BuildEvenBuckets();
        }
    }

    private void OnNewBucketHit(BucketedEventProvider.BucketInfo obj)
    {
        OnWorldDifficultyChange?.Invoke();
    }

    private void OnPlayerEnergyChanged(Agent obj)
    {
        eventProvider.InformOfValueUpdate(obj.Energy);
    }
}
