using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum VoxelTypes
{
    Void,

    // mobile
    Bulldozer,
    Rubble,
    Roller, // rolls on an axis

    // immobile
    Stone,
    Ice, // stuff slides on this without consuming movement
    Sand, //stuff that rolls will stop on this
    ConveyorBelt, //moves stuff on it in one turn
}

public class LevelManager : MonoBehaviour
{
    public GameObject wheel;
    public string levelName;
    public Transform grid;
    public Transform cameraTarget;

    // NOTE: Round stuffs
    private enum RoundState
    {
        Plan,
        Play,
    }

    private static readonly Dictionary<RoundState, Color> stateColors = new()
    {
        { RoundState.Plan, Color.yellow },
        { RoundState.Play, Color.green },
    };

    private static RoundState roundState;
    public TMP_Text roundStateText;
    public Renderer floorRenderer;

    // NOTE: Board Stuffs


    private Voxel[][][] board;

    void OnEnable()
    {
        Interactible.OnClicked += HandleClick;
    }

    void OnDisable()
    {
        Interactible.OnClicked -= HandleClick;
    }

    void Awake()
    {
        roundState = RoundState.Plan;
        UpdateRoundStateVisuals();
        //LoadBoard(); // somehow loads the board
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            var tiles = GetTilesOnLayer(1)
                .Select(tile => tile.GetComponent<TileMover>())
                .Where(mover => mover != null);

            foreach (var mover in tiles)
                mover.MoveTo(mover.transform.position + Vector3.forward * 2);
        }
        if (Input.GetKeyDown(KeyCode.C))
        { // cycle through the roundstates
            roundState = (RoundState)(
                ((int)roundState + 1) % Enum.GetValues(typeof(RoundState)).Length
            );
            UpdateRoundStateVisuals();
        }
        if (Input.GetKeyDown(KeyCode.V))
        { // Load the level
            StartCoroutine(LoadLevel(EditorTool.LoadLevelData(levelName)));
        }
    }

    public IEnumerator LoadLevel(string[][][] voxels)
    {
        if (voxels == null)
        {
            Debug.LogError($"Failed to load level {levelName}, voxels array is null");
            yield break;
        }
        EditorTool.PrintVoxels(voxels);

        int paddingPerSide = 2;
        int depth = voxels.Length;
        int width = voxels[0].Length + paddingPerSide * 2;
        int height = voxels[0][0].Length + paddingPerSide * 2;

        board = new Voxel[depth][][];
        for (int z = 0; z < depth; z++)
        {
            board[z] = new Voxel[width][];
            for (int x = 0; x < width; x++)
            {
                board[z][x] = new Voxel[height];
                for (int y = 0; y < height; y++)
                {
                    board[z][x][y] = null;
                }
            }
        }

        for (int z = 0; z < voxels.Length; z++)
        {
            Transform layer = grid.Find($"Layer{z}");

            foreach (Transform child in layer)
            {
                Destroy(child.gameObject);
            }

            yield return new WaitUntil(() => layer.childCount == 0);

            for (int x = 0; x < voxels[z].Length; x++)
            {
                for (int y = 0; y < voxels[z][x].Length; y++)
                {
                    string voxel = voxels[z][x][y];
                    string[] voxelData = voxel.Split(".");

                    Voxel graph = null;

                    // Visual Representation
                    if (voxelData[0] != "Void")
                    {
                        if (z == 1)
                        {
                            Debug.Log($"Found [{voxel}]");
                        }
                        GameObject prefab = Resources.Load<GameObject>(
                            $"Models/LevelEditor/{voxelData[0]}"
                        );
                        int angleRotation = int.Parse(voxelData[1]);

                        if (prefab != null)
                        {
                            Vector3 position = new Vector3(
                                x + paddingPerSide,
                                z,
                                y + paddingPerSide
                            ); // + new Vector3(0.5f,0.5f,0.5f);
                            GameObject instantiatedPrefab = Instantiate(
                                prefab,
                                position,
                                Quaternion.AngleAxis(angleRotation, Vector3.up)
                            );
                            instantiatedPrefab.transform.SetParent(layer);
                            instantiatedPrefab.name = voxel;
                            graph = instantiatedPrefab.GetComponent<Voxel>();
                            graph.origin = new Vector3Int(
                                x + paddingPerSide,
                                y + paddingPerSide,
                                z
                            );
                            board[z][x + paddingPerSide][y + paddingPerSide] = graph;
                        }
                    }

                    // Logical Representation:
                    if (graph != null)
                    {
                        foreach (Vector3Int offset in graph.offsets)
                        {
                            int newZ = graph.origin.z + offset.z;
                            int newX = graph.origin.x + offset.x;
                            int newY = graph.origin.y + offset.y;
                            if (
                                newZ < 0
                                || newX < 0
                                || newY < 0
                                || newZ > depth
                                || newX > width
                                || newY > height
                            )
                            {
                                Debug.LogError(
                                    $"Out of bounds access: {newZ},{newX},{newY} outside {depth},{width},{height}, offset: {offset.ToString()}, graph origin: {graph.origin.ToString()}"
                                );
                            }
                            else
                            {
                                Voxel offsetVoxel = board[newZ][newX][newY];

                                if (offsetVoxel != null && offsetVoxel.type != VoxelTypes.Void)
                                {
                                    Debug.LogError(
                                        $"Tried to set a {graph.type.ToString()} at pos [{z + offset.z}][{x + offset.x}][{y + offset.y}] where there's already a {offsetVoxel.type.ToString()}"
                                    );
                                }
                                else
                                {
                                    board[newZ][newX][newY] = graph;
                                    // Debug.Log(
                                    //     $"set graph {graph.type} at [{z + offset.z}][{x + offset.x}][{y + offset.y}]"
                                    // );
                                }
                            }
                        }
                    }
                }
            }
        }

        EditorTool.PrintVoxels(board);
        PositionGameElements(width, height);
    }

    private void PositionGameElements(int width, int height)
    {
        Vector3 center = new Vector3(width / 2.0f, 0, height / 2.0f);
        Vector3 cameraTargetOffset = new Vector3(0, cameraTarget.localPosition.y, 0);

        roundStateText.transform.localPosition = new Vector3(0, (height / 2.0f) + 1, 0.1f);
        cameraTarget.localPosition = cameraTargetOffset + center;
    }

    void HandleClick(Interactible clickedObject)
    {
        if (roundState == RoundState.Plan)
        {
            // Activate the Wheel and pass the clicked object
            wheel.SetActive(true);
            Debug.Log("Wheel activated on: " + clickedObject.gameObject.name);
        }
        else if (roundState == RoundState.Play)
        {
            // Get the queued events from the clicked object
            foreach (var action in clickedObject.queuedEvents)
            {
                Debug.Log("Executing Action: " + action);
            }
        }
    }

    GameObject[] GetTilesOnLayer(int layer)
    {
        string layerName = "Layer" + layer;
        var parent = GameObject.Find(layerName);
        if (parent == null)
            return new GameObject[0];

        return parent
            .GetComponentsInChildren<Transform>()
            .Where(t => t.gameObject != parent)
            .Select(t => t.gameObject)
            .ToArray();
    }

    private void UpdateRoundStateVisuals()
    {
        roundStateText.text = roundState.ToString();
        roundStateText.color = stateColors[roundState];
        floorRenderer.material.color = stateColors[roundState];
    }
}
