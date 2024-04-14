using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    private new Camera camera;
    [SerializeField] float walkSpeed = 0.1f;
    [SerializeField] float rotSpeed = 100.0f;
    private void Awake()
    {
        camera = GetComponent<Camera>();
    }

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
        {
            camera.transform.Translate(camera.transform.forward * walkSpeed, Space.World);
        }

        if (Input.GetKey(KeyCode.S))
        {
            camera.transform.Translate(-camera.transform.forward * walkSpeed, Space.World);
        }

        if (Input.GetKey(KeyCode.A))
        {
            camera.transform.Translate(-camera.transform.right * walkSpeed, Space.World);
        }

        if (Input.GetKey(KeyCode.D))
        {
            camera.transform.Translate(camera.transform.right * walkSpeed, Space.World);
        }

        float pitch = 0.0f;
        float yaw = 0.0f;

        if (Input.GetKey(KeyCode.UpArrow))
        {
            pitch += rotSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            pitch -= rotSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
             yaw -= rotSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            yaw += rotSpeed * Time.deltaTime;
        }

        camera.transform.Rotate(0, yaw, 0, Space.World);
        camera.transform.Rotate(-pitch, 0, 0, Space.Self);
    }
}
