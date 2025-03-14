using System.Linq;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
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
}
