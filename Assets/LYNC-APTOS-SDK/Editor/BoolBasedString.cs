using UnityEditor;
using UnityEngine;
using LYNC;

[CustomEditor(typeof(LyncManager))]
public class BoolBasedStringEditor : Editor
{
    public override void OnInspectorGUI()
    {
        LyncManager lyncManager = (LyncManager)target;

        lyncManager.LyncAPIKey = EditorGUILayout.TextField("Lync API Key", lyncManager.LyncAPIKey);
        lyncManager.Network = (NETWORK)EditorGUILayout.EnumPopup("Network", lyncManager.Network);
        lyncManager.SponsorTransaction = EditorGUILayout.Toggle("Sponsor Transaction", lyncManager.SponsorTransaction);

        EditorGUILayout.LabelField("Login options", EditorStyles.boldLabel);
        lyncManager.LoginOptionFirebase = EditorGUILayout.Toggle("Login Option Firebase", lyncManager.LoginOptionFirebase);
        lyncManager.LoginOptionStarKey = EditorGUILayout.Toggle("Login Option Pontem", lyncManager.LoginOptionStarKey);


        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}
