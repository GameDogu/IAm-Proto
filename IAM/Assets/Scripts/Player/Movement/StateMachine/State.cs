using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

[Serializable]
public abstract class State<T>
{
    public uint ID { get; protected set; }
    [SerializeField]string name = "";
    public string Name => name;

    public State(uint id, string name)
    {
        ID = id;
        this.name = name;
    }

    public abstract void Start(T prevState);
}

