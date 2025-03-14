using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Wheel : MonoBehaviour
{
    public int sliceCount = 6;
    private float[] sliceFill;
    private Color[] sliceColors;

    public float rotationSpeed = 200f;
    private float inputHoldTimer = 0f;
    private bool isHoldingInput = false;

    private int currentSliceIndex = 0;
    private AudioSource audioSource;
    public AudioClip selectSound;

    private Mesh mesh;

    void Start()
    {
        float[] validFillValues = new float[sliceCount];
        for (int i = 0; i < sliceCount; i++)
        {
            validFillValues[i] = (i + 1f) / (sliceCount + 1f);
        }
        sliceFill = validFillValues.OrderBy(_ => Random.value).ToArray();

        sliceColors = new Color[sliceCount];
        for (int i = 0; i < sliceCount; i++)
        {
            sliceColors[i] = Color.Lerp(Color.gray, Color.yellow, sliceFill[i]);
        }

        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        audioSource = gameObject.AddComponent<AudioSource>();
        GenerateWheelMesh();
    }

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput != 0)
        {
            if (!isHoldingInput)
            {
                RotateSlice(horizontalInput);
                isHoldingInput = true;
                inputHoldTimer = 0f;
            }
            else
            {
                inputHoldTimer += Time.deltaTime * 1000f;

                if (inputHoldTimer >= rotationSpeed)
                {
                    RotateSlice(horizontalInput);
                    inputHoldTimer = 0f;
                }
            }
        }
        else
        {
            isHoldingInput = false;
            inputHoldTimer = 0f;
        }

        float anglePerSlice = 360f / sliceCount;
        transform.rotation = Quaternion.Euler(0, 0, -currentSliceIndex * anglePerSlice);
    }

    void RotateSlice(float direction)
    {
        if (direction > 0)
        {
            currentSliceIndex = (currentSliceIndex + 1) % sliceCount;
        }
        else if (direction < 0)
        {
            currentSliceIndex = (currentSliceIndex - 1 + sliceCount) % sliceCount;
        }

        if (selectSound != null)
        {
            audioSource.PlayOneShot(selectSound);
        }
    }

    void GenerateWheelMesh()
    {
        int numVertices = 60;
        Vector3[] vertices = new Vector3[numVertices + 1];
        int[] triangles = new int[numVertices * 3];

        vertices[0] = Vector3.zero;
        float angleStep = 360f / numVertices;

        float radius = 1f;
        for (int i = 0; i < numVertices; i++)
        {
            radius += angleStep;
            float angle = i * angleStep * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            int next = (i + 1) % numVertices + 1;
            triangles[i * 3] = 0;
            triangles[i * 3 + 1] = next;
            triangles[i * 3 + 2] = i + 1;
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void OnDrawGizmos()
    {
        float angleStep = 360f / sliceCount;

        for (int i = 0; i < sliceCount; i++)
        {
            float angle = Mathf.Deg2Rad * (i * angleStep);
            Vector3 dir = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
            Gizmos.DrawLine(Vector3.zero, dir);
        }
    }
}
