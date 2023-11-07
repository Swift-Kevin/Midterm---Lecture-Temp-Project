using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class cameraControls : MonoBehaviour
{
    [SerializeField] int sensitivity;

    [Header("--- Vertical Min / Max ---")]
    [SerializeField] int lockVertMin;
    [SerializeField] int lockVertMax;
    bool invertY;

    float xRotation;

    // Start is called before the first frame update
    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }
    
    void Movement()
    {
        // Get input
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * sensitivity;
        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * sensitivity;

        if (invertY)
            xRotation += mouseY;
        else
            xRotation -= mouseY;

        // clamp camera rotation on the x-axis
        xRotation = Mathf.Clamp(xRotation, lockVertMin, lockVertMax);

        // rotate the camera on the x-axis
        transform.localRotation = Quaternion.Euler(xRotation, 0, 0);

        // rotate the player on the y-axis
        transform.parent.Rotate(Vector3.up * mouseX);
    }
}
