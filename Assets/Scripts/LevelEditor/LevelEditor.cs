using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelEditor : MonoBehaviour
{
    public string[][][] GetLevelFromTilemaps()
    {
        List<Transform> layers = new List<Transform>();
        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Layer"))
            {
                layers.Add(child);
            }
        }

        Vector3 min = Vector3Int.one * int.MaxValue;
        Vector3 max = Vector3Int.one * int.MinValue;

        foreach (var layer in layers)
        {
            foreach (Transform child in layer.transform)
            {
                Vector3 pos = child.transform.position;
                min = Vector3.Min(min, pos);
                max = Vector3.Max(max, pos);
            }
        }

        int width = (int)(max.x - min.x) + 1;
        int height = (int)(max.z - min.z) + 1;
        int depth = layers.Count;
        Debug.Log($"Dimensions Found: ({depth}, {width}, {height}), Min: {min}, Max: {max}");

        string[][][] voxels = new string[depth][][];
        for (int z = 0; z < depth; z++)
        {
            voxels[z] = new string[width][];
            for (int x = 0; x < width; x++)
            {
                voxels[z][x] = new string[height];
                for (int y = 0; y < height; y++)
                {
                    voxels[z][x][y] = "Void";
                }
            }
        }

        for (int z = 0; z < layers.Count; z++)
        {
            var layer = layers[z];

            foreach (Transform child in layer.transform)
            {
                Vector3 pos = child.position;

                int x = (int)(pos.x - min.x);
                int y = (int)(pos.z - min.z);

                Voxel voxel = child.GetComponent<Voxel>();
                voxels[z][x][y] = $"{voxel.type}.{child.transform.eulerAngles.y}";
            }
        }

        return voxels;
    }

    public void SetLevelFromVoxelTypes(string[][][] voxels)
    {
        for (int z = 0; z < voxels.Length; z++)
        {
            Transform layer = transform.Find($"Layer{z}");

            while (layer.childCount > 0)
            {
                DestroyImmediate(layer.GetChild(0).gameObject);
            }

            for (int x = 0; x < voxels[z].Length; x++)
            {
                for (int y = 0; y < voxels[z][x].Length; y++)
                {
                    string voxel = voxels[z][x][y];

                    if (voxel != "Void")
                    {
                        string[] voxelData = voxel.Split(".");

                        GameObject prefab = Resources.Load<GameObject>(
                            $"Models/LevelEditor/{voxelData[0]}"
                        );
                        int angleRotation = int.Parse(voxelData[1]);

                        if (prefab != null)
                        {
                            Vector3 position = new Vector3(x + 0.5f, z + 0.5f, y + 0.5f);
                            GameObject instantiatedPrefab = Instantiate(
                                prefab,
                                position,
                                Quaternion.AngleAxis(angleRotation, Vector3.up)
                            );
                            instantiatedPrefab.transform.SetParent(layer);
                            instantiatedPrefab.name = voxel;
                        }
                    }
                }
            }
        }
    }

    public void ClearBoard()
    {
        foreach (Transform child in transform)
        {
            while (child.childCount > 0)
            {
                DestroyImmediate(child.GetChild(0).gameObject);
            }
        }
    }
}
