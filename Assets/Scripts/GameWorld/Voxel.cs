using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    public VoxelTypes type;
    public Vector3Int origin;
    public List<Vector3Int> offsets;
}
