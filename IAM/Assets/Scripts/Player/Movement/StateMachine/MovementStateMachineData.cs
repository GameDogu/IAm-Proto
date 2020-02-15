using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName ="Movement/State Machine Data")]
public class MovementStateMachineData : ScriptableObject
{
    [SerializeField] List<string> generalOptionsData =new List<string>();
    public IReadOnlyList<string> GeneralOptionsData => generalOptionsData;

    [SerializeField]List<MovementState.Data> states = new List<MovementState.Data>();
    public IReadOnlyList<MovementState.Data> States => states;

    public MovementState InitializeStateMachine(MovementStateMachine machine)
    {
        HandleGeneralMovementOptrions(machine);
        return HandleMovementStates(machine);
    }

    private void HandleGeneralMovementOptrions(MovementStateMachine machine)
    {
        for (int i = 0; i < generalOptionsData.Count; i++)
        {
            Type optionType = Type.GetType(generalOptionsData[i]);

            if (optionType == null)
                throw new Exception($"Type does not exist {generalOptionsData[i]}");

            EntityMovementOption movementOption;
            if (!GameObjectHasOptionComponent(machine.gameObject, optionType))
            {
                movementOption = machine.gameObject.AddComponent(optionType) as EntityMovementOption;
            }
            else
            {
                movementOption = machine.GetComponent(optionType) as EntityMovementOption;
            }
            machine.AddGeneralMovementOption(movementOption);
        }
    }

    private MovementState HandleMovementStates(MovementStateMachine machine)
    {
        MovementState initial = null;

        for (int i = 0; i < states.Count; i++)
        {
            var st = states[i].Create(machine);
            machine.AddNewState(st);

            if (st.IsInitialState)
                initial = st;
        }
        return initial;
    }

    private bool GameObjectHasOptionComponent(GameObject gameObject, Type option)
    {
        return gameObject.GetComponent(option) != null;
    }

    public void AddGeneralMovementOption(Type t)
    {
        if (t.IsSubclassOf(typeof(EntityMovementOption)))
        {
            generalOptionsData.Add(t.Name);
        }
    }

    public void AddGeneralMovementOption(EntityMovementOption option)
    {
        generalOptionsData.Add(option.GetType().Name);
    }

    public void AddState(MovementState state)
    {
        states.Add(new MovementState.Data(state));
    }

    public static MovementStateMachineData CreateAssetAndSave(MovementStateMachine machine, string assetStoragePath)
    {
        MovementStateMachineData data = CreateInstance<MovementStateMachineData>();

        machine.FillDataObject(data);

        AssetDatabase.CreateAsset(data, assetStoragePath);

        AssetDatabase.Refresh();

        return data;
    }

    public static bool Save(MovementStateMachine machine, out MovementStateMachineData data)
    {
        string path = EditorUtility.OpenFolderPanel("Save Data", "", "stateMachineMovementData");
        if (path.Length != 0)
        {
            //Success
            data = machine.Save(path);
            return true;
        }
        data = null;
        return false;
    }

    public static bool Save(MovementStateMachine machine)
    {
        string path = EditorUtility.OpenFolderPanel("Save Data", "", "stateMachineMovementData");
        if (path.Length != 0)
        {
            //Success
            machine.Save(path);
            return true;
        }
        return false;
    }

}
