using UnityEngine;
using UnityEngine.Tilemaps;

public class TileMover : MonoBehaviour
{
    public float tilesPerSecond = 5f;
    private bool isMoving;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            var tilemap = GetComponentInParent<Tilemap>();
            MoveTo(transform.position + Vector3.right * 4);
        }
    }

    public void MoveTo(Vector3 targetPosition)
    {
        if (!isMoving)
            StartCoroutine(SmoothMove(targetPosition));
    }

    private System.Collections.IEnumerator SmoothMove(Vector3 target)
    {
        isMoving = true;
        Vector3 startPos = transform.position;
        float distance = Vector3.Distance(startPos, target);
        float duration = distance / tilesPerSecond;

        float time = 0;
        while (time < 1f)
        {
            time += Time.deltaTime / duration;
            transform.position = Vector3.Lerp(startPos, target, time);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }
}
