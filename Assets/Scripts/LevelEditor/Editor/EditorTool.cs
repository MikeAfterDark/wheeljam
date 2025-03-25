using System;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

[Serializable]
public class VoxelData
{
    public string[][][] layers;
}

public class EditorTool : EditorWindow
{
    private LevelEditor levelEditor;

    private string saveFile = "";
    private string loadFile = "";

    [MenuItem("Tools/Save Load Tool")]
    public static void ShowWindow()
    {
        GetWindow<EditorTool>("Save/Load Levels");
    }

    private void OnGUI()
    {
        GUILayout.Label("Save/Load Tool", EditorStyles.boldLabel);

        // Save Section
        saveFile = EditorGUILayout.TextField("Save File:", saveFile);
        if (GUILayout.Button("Save (overwrites)"))
        {
            SaveData(saveFile);
        }

        // Load Section
        loadFile = EditorGUILayout.TextField("Load File:", loadFile);
        if (GUILayout.Button("Load"))
        {
            LoadData(loadFile);
        }

        if (GUILayout.Button("Clear Board"))
        {
            ClearBoard();
        }
    }

    private void SaveData(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("Filename is empty.");
            return;
        }

        var voxels = GetLevelEditor().GetLevelFromTilemaps();
        VoxelData data = new VoxelData { layers = voxels };
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        string path = $"{Application.dataPath}/Levels/{fileName}.json";
        File.WriteAllText(path, json);
        Debug.Log($"Saved to: {path}");
    }

    public static string[][][] LoadLevelData(string fileName)
    {
        if (string.IsNullOrEmpty(fileName))
        {
            Debug.LogError("Filename is empty.");
            return null;
        }

        // string path = $"{Application.dataPath}/Levels/{fileName}.json";
        //
        // if (!File.Exists(path))
        // {
        //     Debug.LogError("Level file not found.");
        //     return null;
        // }

        string json = Resources.Load<TextAsset>("Levels/fileName").text;
        VoxelData data = JsonConvert.DeserializeObject<VoxelData>(json);
        Debug.Log("Loaded Voxel Data");
        return data.layers;
    }

    public void LoadData(string fileName)
    {
        GetLevelEditor().SetLevelFromVoxelTypes(LoadLevelData(fileName));
    }

    private void ClearBoard()
    {
        GetLevelEditor().ClearBoard();
    }

    private LevelEditor GetLevelEditor()
    {
        if (levelEditor == null)
        {
            levelEditor = FindFirstObjectByType<LevelEditor>();

            if (levelEditor == null)
            {
                Debug.LogError("No LevelEditor found in the scene.");
            }
        }

        return levelEditor;
    }

    public static void PrintVoxels(string[][][] voxels)
    {
        Debug.Log($"Dimensions: ({voxels.Length}, {voxels[0].Length}, {voxels[0][0].Length})");
        for (int z = 0; z < voxels.Length; z++)
        {
            Debug.Log($"Layer {z}:");

            for (int y = voxels[0][0].Length - 1; y >= 0; y--)
            {
                string row = "";

                for (int x = 0; x < voxels[0].Length; x++)
                {
                    string voxel = voxels[z][x][y] != "Void" ? voxels[z][x][y] : "o";
                    row += voxel.ToString().PadRight(12);
                }

                Debug.Log(row);
            }
        }
    }

    public static void PrintVoxels(Voxel[][][] voxels)
    {
        Debug.Log($"Dimensions: ({voxels.Length}, {voxels[0].Length}, {voxels[0][0].Length})");
        for (int z = 0; z < voxels.Length; z++)
        {
            Debug.Log($"Layer {z}:");

            for (int y = voxels[0][0].Length - 1; y >= 0; y--)
            {
                string row = "";

                for (int x = 0; x < voxels[0].Length; x++)
                {
                    if (voxels[z][x][y] != null)
                    {
                        VoxelTypes voxel = voxels[z][x][y].type;
                        row += voxel.ToString().PadRight(16); // Align columns
                    }
                    else
                    {
                        row += "o".PadRight(16);
                    }
                }

                Debug.Log(row);
            }
        }
    }
}
