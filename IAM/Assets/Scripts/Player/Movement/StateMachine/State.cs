using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

[Serializable]
public abstract class State<T>
{
    public uint ID { get; protected set; }
    public string Name { get; set; }

    public State(uint id, string name)
    {
        ID = id;
        this.Name= name;
    }

    public abstract void Start(T prevState);
}

