using Unity.Cinemachine;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    private CinemachineInputAxisController rotationController;

    void Start()
    {
        rotationController = GetComponent<CinemachineInputAxisController>();
    }

    void Update()
    {
        rotationController.enabled = Input.GetKey(KeyCode.Mouse2);
    }
}
