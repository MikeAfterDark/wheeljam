using Unity.Cinemachine;
using UnityEngine;

public class CameraSpinController : MonoBehaviour
{
    public CinemachineCamera virtualCamera; // Assign your Cinemachine camera
    public float spinSpeed = 30f; // Speed of rotation
    private static bool isSpinning = false;
    private static CinemachineOrbitalFollow orbitalFollow;
    private static CinemachineInputAxisController inputController;
    private static CameraControl cameraControl;

    void Start()
    {
        // Get the OrbitalFollow component to modify rotation
        orbitalFollow = virtualCamera.GetComponent<CinemachineOrbitalFollow>();
        if (orbitalFollow == null)
        {
            Debug.LogError("CinemachineOrbitalFollow not found on the camera!");
        }

        // Get the InputAxisController component to disable/enable input
        inputController = virtualCamera.GetComponent<CinemachineInputAxisController>();
        if (inputController == null)
        {
            Debug.LogError("CinemachineInputAxisController not found on the camera!");
        }
        cameraControl = virtualCamera.GetComponent<CameraControl>();
        if (cameraControl == null)
        {
            Debug.LogError("CameraControl not found on the camera!");
        }

        StartSpinning();
    }

    void Update()
    {
        if (isSpinning && orbitalFollow != null)
        {
            // Rotate the camera by modifying the horizontal axis value
            orbitalFollow.HorizontalAxis.Value += spinSpeed * Time.deltaTime;
        }
    }

    public static void StartSpinning()
    {
        isSpinning = true;
        DisableInput();
    }

    public static void StopSpinning()
    {
        isSpinning = false;
        EnableInput();

        orbitalFollow.HorizontalAxis.Value = 0;
    }

    static void DisableInput()
    {
        if (inputController != null)
        {
            inputController.enabled = false;
        }
        if (cameraControl != null)
        {
            cameraControl.enabled = false;
        }
    }

    static void EnableInput()
    {
        if (inputController != null)
        {
            inputController.enabled = true;
        }

        if (cameraControl != null)
        {
            cameraControl.enabled = true;
        }
    }
}
