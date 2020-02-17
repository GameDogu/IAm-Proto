using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class BucketedEventProvider
{
    public event Action<BucketInfo> OnNewBucketHit;
    public event Action<BucketInfo> OnSameBucketHit;

    [SerializeField]protected Vector2 minMax;
    protected float max => minMax.y;
    protected float min => minMax.x;

    [SerializeField] float InitialValue = default;
    public float CurrentValue { get; protected set; }

    protected int currentBucketIdx;

    [SerializeField] protected List<Bucket> buckets;
    public int BucketCount => buckets.Count;

    [SerializeField]bool invokeSameBucketEvent= default;

    public virtual void StartUp()
    {
        CurrentValue = InitialValue;
    }

    protected abstract int GetBucketIndex();

    public void InformOfValueUpdate(float newVal)
    {
        CurrentValue = newVal;

        int bucketIdx = -1;
        var buck = GetBucketHit(out bucketIdx);
        HandleBucket(buck, bucketIdx);
    }

    private void HandleBucket(Bucket buck,int idx)
    {
        if (idx == currentBucketIdx)
        {
            if(invokeSameBucketEvent)
                OnSameBucketHit?.Invoke(new BucketInfo(buck, CurrentValue, idx, buckets.Count));
        }
        else
        {
            currentBucketIdx = idx;
            OnNewBucketHit?.Invoke(new BucketInfo(buck, CurrentValue, idx, buckets.Count));
        }
    }

    Bucket GetBucketHit(out int bucketIdx)
    {
        bucketIdx = GetBucketIndex();
        if (bucketIdx < 0)
            bucketIdx = 0;
        else if (bucketIdx >= buckets.Count)
            bucketIdx = buckets.Count - 1;
        return buckets[bucketIdx];
    }

    [Serializable]
    public struct Bucket
    {
        [SerializeField]float min;
        public float Min => min;
        [SerializeField]float max;
        public float Max => max;
        public float Center => (Max + Min) * .5f;

        public Bucket(float minimum, float maximum) : this()
        {
            min = minimum;
            max = maximum;
        }
        
        public bool Contains(float val)
        {
            return val >= Min && val <= Max;
        }
    }

    public struct BucketInfo
    {
        public Bucket BucketHit;
        public float ActualValue;
        public int BucketIdx;
        public int BucketCount;

        public BucketInfo(Bucket buck, float currentValue, int idx, int count) : this()
        {
            this.BucketHit = buck;
            this.ActualValue = currentValue;
            this.BucketIdx = idx;
            this.BucketCount = count;
        }
    }

}

[Serializable]
public class BucketedEventProviderUneven : BucketedEventProvider
{
    protected override int GetBucketIndex()
    {
        return buckets.FindIndex(buck => buck.Contains(CurrentValue));
    }
}

[Serializable]
public class BucketedEventProviderEven : BucketedEventProvider
{
    float bucketStepEven => (max - min) / (float)buckets.Count;
    [SerializeField,Range(1,100)] int desiredBucketCount;
    public int DesiredBucketCount => desiredBucketCount;

    public override void StartUp()
    {
        base.StartUp();
        BuildEvenBuckets();
    }

    public void BuildEvenBuckets()
    {
        if (desiredBucketCount == 0)
            desiredBucketCount = 1;
        buckets.Clear();
        float desiredBucketStep = (max - min) / (float)desiredBucketCount;

        for (int i = 0; i < desiredBucketCount; i++)
        {

            float bucketMin = min + (desiredBucketStep * i);
            float bucketMax = bucketMin + desiredBucketStep - float.Epsilon;

            buckets.Add(new Bucket(bucketMin, bucketMax));
        }
    }

    protected override int GetBucketIndex()
    {
        return Mathf.CeilToInt(CurrentValue / bucketStepEven) - 1;
    }
}