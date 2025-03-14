using UnityEngine;

public class WheelCameraFacing : MonoBehaviour
{
    public Camera camera;

    void Update()
    {
        if (camera == null)
        {
            camera = Camera.main;
        }

        transform.LookAt(transform.position + camera.transform.forward);
    }
}
