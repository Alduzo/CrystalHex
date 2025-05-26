using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WorldMapManager))]
public class WorldMapManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WorldMapManager manager = (WorldMapManager)target;

if (GUILayout.Button("🔄 Regenerar Mundo"))
{
    manager.InitializeWorld();
    Debug.Log("🛠 Mundo regenerado manualmente desde botón.");
}



    }
}
