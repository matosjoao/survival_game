using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private InputReader InputReader;

    [Header("Look")]
    [SerializeField] private Transform cameraContainer;
    [SerializeField] private float minXLook;
    [SerializeField] private float maxXLook;
    private float camCurXRot;
    [SerializeField] private float lookSensitivity;

    [HideInInspector]
    public bool canLook = true;

    private void Start()
    {
        // Lock the cursor at the start of the game
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate() 
    {
        if(canLook == true)
            CameraLook();

    }

    private void CameraLook ()
    {
        // rotate the camera container up and down
        camCurXRot += InputReader.MouseDelta.y * lookSensitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);

        // rotate the player left and right
        transform.eulerAngles += new Vector3(0, InputReader.MouseDelta.x * lookSensitivity, 0);
    }

    public void ToggleCursor(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }
}
