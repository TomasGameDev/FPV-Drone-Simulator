#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;


public class MeshSaver : MonoBehaviour
{
    public string path = "Assets/";
    public GameObject origin; // Посилання на компонент MeshFilter об'єкта, який містить меш, який ви хочете зберегти
    public Material defaultMaterial;
    public int targetLODLevel = 20;
    public int minimumLODLevel = 20;
    public bool saveOnlyActive = true;
    public bool targetLODLevelEnable = true;
    public bool minimumLODLevelEnable = true;
    public void SaveMeshAsAsset()
    {
        ClearFiles();
        ClearMeshes();
        int counter = 0;

        for (int c = 0; c < origin.transform.childCount; c++)
        {
            if (targetLODLevelEnable)
            {
                if (!origin.transform.GetChild(c).transform.name.Contains("L" + targetLODLevel)) continue;
            }
            else if (minimumLODLevelEnable)
            {
                bool Contains = false;
                for (int l = minimumLODLevel; l < 25; l++)
                {
                    if (origin.transform.GetChild(c).transform.name.Contains("L" + minimumLODLevel))
                    {
                        Contains = true;
                        break;
                    }
                }
                if (!Contains) continue;
            }
            if (saveOnlyActive && !origin.transform.GetChild(c).gameObject.active) continue;
            GameObject mesh = origin.transform.GetChild(c).transform.GetChild(0).gameObject;

            Mesh _mesh = mesh.GetComponent<MeshFilter>().mesh;

            Mesh savedMesh = new Mesh();
            savedMesh.vertices = _mesh.vertices;
            savedMesh.triangles = _mesh.triangles;
            savedMesh.uv = _mesh.uv;
            savedMesh.normals = _mesh.normals;


            Material material = mesh.GetComponent<MeshRenderer>().material;

            Texture mainTexture = material.GetTexture("_overlay0Texture");

            AssetDatabase.CreateAsset(savedMesh, path + "/Meshes/Terr" + c + ".asset");
            AssetDatabase.CreateAsset(mainTexture, path + "/Textures/TerrText" + c + ".asset");
            GameObject newMesh = new GameObject();
            newMesh.name = "Terr" + c;
            newMesh.transform.position = mesh.transform.position;
            newMesh.transform.rotation = mesh.transform.rotation;
            newMesh.transform.localScale = mesh.transform.localScale;
            newMesh.AddComponent<MeshFilter>().mesh = savedMesh;

            newMesh.AddComponent<MeshCollider>().sharedMesh = savedMesh;

            newMesh.AddComponent<MeshRenderer>().material = defaultMaterial;
            newMesh.GetComponent<MeshRenderer>().material.SetTexture("_overlay0Texture", mainTexture);

            newMesh.transform.SetParent(transform);
            newMesh.SetActive(origin.transform.GetChild(c).gameObject.active);
            counter++;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        print("Exported " + counter + " meshes!");
    }
    public void ClearFiles()
    {
        List<string> paths = new List<string>();
        string[] meshesPaths = Directory.GetFiles(path + "/Meshes");
        string[] texturesPaths = Directory.GetFiles(path + "/Textures");
        paths.AddRange(meshesPaths);
        paths.AddRange(texturesPaths);
        foreach (string file in paths)
        {
            File.Delete(file);
        }
    }
    public void ClearMeshes()
    {
        int childCount = transform.childCount;
        for (int c = 0; c < childCount; c++)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }
}
[CustomEditor(typeof(MeshSaver))]
public class MeshSaverEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("SaveMeshAsAsset")) (target as MeshSaver).SaveMeshAsAsset();
        if (GUILayout.Button("ClearFiles")) (target as MeshSaver).ClearFiles();
        if (GUILayout.Button("ClearMeshes")) (target as MeshSaver).ClearMeshes();
    }
}
#endif