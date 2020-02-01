using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class StateEditorWindow : EditorWindow
{
    public static void ShowWindow()
    {
        var window = GetWindow<StateEditorWindow>();
        window.Show();
    }



    private void OnGUI()
    {
        Draw();
    }

    private void Draw()
    {

    }
}