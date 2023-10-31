using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SaveMesh : MonoBehaviour
{
    //reference: https://unitycoder.com/blog/2013/01/26/save-mesh-created-by-script-in-editor-playmode/
    public KeyCode saveKey = KeyCode.F12;
    public string saveName = "SavedMesh";
    public Transform selectedGameObject;

    void Update()
    {
        if (Input.GetKeyDown(saveKey))
        {
            SaveAsset();
        }
    }

    void SaveAsset()
    {
        var mf = selectedGameObject.GetComponent<MeshFilter>();
        if (mf)
        {
            var savePath = "Assets/" + saveName + ".asset";
            Debug.Log("Saved Mesh to:" + savePath);
            AssetDatabase.CreateAsset(mf.mesh, savePath);
        }
    }
}
