using System;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class EntityEditedInfoWindow : EditorWindow
{
    static List<Type> _instantiableTypes;
    public static List<Type> InstantiableNodeTypes
    {
        get
        {
            if (_instantiableTypes == null)
                _instantiableTypes = Assembly.GetAssembly(typeof(EntityMovementOption)).GetTypes().Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(EntityMovementOption))).ToList();
            return _instantiableTypes;
        }
    }


    public event Action OnEditedEntityChanged;
    public event Action OnEntityReloaded;

    public static void ShowWindow()
    {
        var window = GetWindow<EntityEditedInfoWindow>();
        window.Show();
    }

    MovementStateMachineEditor stateMachineEditor;
    StateEditorWindow stateEditor;
    MovementStateMachine entityEdited;
    public MovementStateMachine EntityEdited
    {
        get { return entityEdited; }
        protected set
        {
            if (entityEdited != value)
            {
                entityEdited = value;
                OnEditedEntityChanged?.Invoke();
            }
        }
    }

    public bool HasEntity => EntityEdited != null;

    string prefabAssetPath = null;

    private void OnGUI()
    {
        Draw();
    }

    public void Initialize(MovementStateMachineEditor stateMachineEditor, StateEditorWindow stateEditor)
    {
        this.stateMachineEditor = stateMachineEditor;
        this.stateEditor = stateEditor;
    }

    private void Draw()
    {
        EntityEdited = EditorGUILayout.ObjectField("Editing", EntityEdited, typeof(MovementStateMachine), true) as MovementStateMachine;

        if (EntityEdited != null)
        {
            prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(EntityEdited);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Movement Options For Entity:");
            EditorGUI.indentLevel += 1;
            for (int i = EntityEdited.GeneralMovementOptions.Count-1; i >= 0 ; i--)
            {
                var opt = EntityEdited.GeneralMovementOptions[i];
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(opt.Name);
                if (GUILayout.Button("X"))
                {
                    EntityEdited.RemoveGeneralMovementOption(opt);
                    Save();
                }

                EditorGUILayout.EndHorizontal();
            }
            EditorGUI.indentLevel -= 1;

            if (GUILayout.Button("Add General Movement Option"))
            {
                AddGeneralMovementOption();
            }

            if (GUILayout.Button("Reload"))
            {
                EntityEdited.ReloadFromData();
                OnEntityReloaded?.Invoke();
            }

        }
    }

    public void Save()
    {
        EditorUtility.SetDirty(EntityEdited.gameObject);
        AssetDatabase.SaveAssets();
        Repaint();
    }

    private void AddGeneralMovementOption()
    {
        Debug.Log(InstantiableNodeTypes.Count);
        GenericMenu men = new GenericMenu();
        for (int i = 0; i < InstantiableNodeTypes.Count; i++)
        {
            var t = InstantiableNodeTypes[i];
            //if (!EntityEdited.GeneralMovementOptions.Exists(op => op.GetType() == t))
            //{
                men.AddItem(new GUIContent(t.Name), false, 
                () => {
                    var opt = EntityEdited.gameObject.AddComponent(t) as EntityMovementOption;
                    EntityEdited.AddGeneralMovementOption(opt);
                    Save();
                });
            //}
        }
        men.ShowAsContext();
    }

    internal void RemoveState(MovementState state)
    {
        EntityEdited.RemoveState(state);
    }
}


public class SaveHandler
{
    //TODO change saving depending on prefab or not
}