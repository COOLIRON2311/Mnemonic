using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    [SerializeField] float walkSpeed = 1.0f;
    [SerializeField] float rotSpeed = 100.0f;
    Vector3 currentEulerAngles;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        if (Input.GetKey(KeyCode.W))
            transform.Translate(Time.deltaTime * walkSpeed * transform.forward, Space.World);

        if (Input.GetKey(KeyCode.S))
            transform.Translate(Time.deltaTime * walkSpeed * -transform.forward, Space.World);

        if (Input.GetKey(KeyCode.A))
            transform.Translate(Time.deltaTime * walkSpeed * -transform.right, Space.World);

        if (Input.GetKey(KeyCode.D))
            transform.Translate(Time.deltaTime * walkSpeed * transform.right, Space.World);

        float pitch = 0.0f;
        float yaw = 0.0f;
        float roll = 0.0f;

        if (Input.GetKey(KeyCode.Keypad8))
            pitch = rotSpeed;

        if (Input.GetKey(KeyCode.Keypad5))
            pitch = -rotSpeed;

        if (Input.GetKey(KeyCode.Keypad4))
            yaw = -rotSpeed;

        if (Input.GetKey(KeyCode.Keypad6))
            yaw = rotSpeed;

        if (Input.GetKey(KeyCode.Keypad7))
            roll = rotSpeed;

        if (Input.GetKey(KeyCode.Keypad9))
            roll = -rotSpeed;

        currentEulerAngles += Time.deltaTime * new Vector3(-pitch, yaw, roll);
        transform.eulerAngles = currentEulerAngles;
    }
}
