using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BaseDataPersistanceManager), true)]
public class DataPersistanceEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        BaseDataPersistanceManager dataPersistanceManager = (BaseDataPersistanceManager)target;
        if (dataPersistanceManager != null)
        {
            // Save game button
            if (GUILayout.Button("Save to File"))
            {
                dataPersistanceManager.SaveFile();
            }

            // Load game button
            if (GUILayout.Button("Load from File"))
            {
                dataPersistanceManager.LoadFile();
            }
        }
    }
}

