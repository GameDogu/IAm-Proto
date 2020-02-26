using System;
using System.Collections;
using UnityEngine;
using UnityEditor;

/// <summary>
/// A State defined by ID and Name, that can be started?
/// relic of previous design lol
/// </summary>
/// <typeparam name="T">Type of previous state</typeparam>
[Serializable]
public abstract class State<T>
{
    /// <summary>
    /// ID of state
    /// </summary>
    public uint ID { get; protected set; }
    /// <summary>
    /// Name of State
    /// </summary>
    public string Name { get; set; }

    public State(uint id, string name)
    {
        ID = id;
        this.Name= name;
    }

    /// <summary>
    /// function to kick off state, with knowledge of prev state
    /// </summary>
    /// <param name="prevState"></param>
    public abstract void Start(T prevState);
}

