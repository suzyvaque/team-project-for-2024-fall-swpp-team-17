using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using OurGame;

// TODO: Do not let the Camera see outside of the hallway.

/// <summary>
/// This class manages the overall rotation and location of the camera.
/// Camera rotation due to mouse movement is managed by CameraMouseController.
/// </summary>
public class CameraManager : MonoBehaviour
{
    Transform gravityTransform, playerTransform;
    public Transform wormhole;
    public float moveSpeed = 10.0f;
    private GameState gameState;
    CameraMouseManager cameraMouseManager;
    GameManager gameManager;

    private float distance = 15.0f;
    public float rotationSpeed = 5f;
    private Vector3 sphericalCoordinates;
    private float mouseRotX, mouseRotY;
    private float sensitivity = 3f;

    void Start()
    {
        gravityTransform = GameObject.Find("GravityManager").transform;
        playerTransform = GameObject.FindWithTag("Player").transform;
        cameraMouseManager = GameObject.Find("Main Camera").GetComponent<CameraMouseManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        sphericalCoordinates = new Vector3(0, 0, distance);
        UpdateCameraPosition();

    }

    /// <summary>
    /// This function updates camera rotation and position according to the mode.
    /// 0: default mode, 1: wormhole animation mode
    /// </summary>
    void Update()
    {
        switch (gameState)
        {
            case GameState.Playing:
                UpdateCameraPosition();
                break;
            case GameState.WormholeEffect:
                SpiralTowardsWormhole();
                break;
        }
    }

    private Vector3 savedSphericalCoordinates;
    private bool isShiftPressedLastFrame = false;
    private float targetDistance;
    private void UpdateCameraPosition()
    {

        if (Input.GetMouseButtonDown(0)) ResetMouseControl();

        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (scrollInput != 0)
        {
            targetDistance = Mathf.Clamp(distance - scrollInput * 10f, 15f, 25f); // 범위 제한: 15~20
        }
        distance = Mathf.Lerp(distance, targetDistance, Time.deltaTime * 100f);


        bool isShiftPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (isShiftPressed && !isShiftPressedLastFrame)
        {
            savedSphericalCoordinates = sphericalCoordinates;
        }

        if (!isShiftPressed && isShiftPressedLastFrame)
        {
            sphericalCoordinates = savedSphericalCoordinates;
        }

        mouseRotX += -Input.GetAxisRaw("Mouse X") * sensitivity * Time.deltaTime;
        mouseRotY += Input.GetAxisRaw("Mouse Y") * sensitivity * Time.deltaTime;

        sphericalCoordinates.x += mouseRotX * rotationSpeed * Time.deltaTime;
        sphericalCoordinates.y = Mathf.Clamp(sphericalCoordinates.y + mouseRotY * rotationSpeed * Time.deltaTime, 0.5f, Mathf.PI - 1.5f);


        float x = distance * Mathf.Cos(sphericalCoordinates.x) * Mathf.Sin(sphericalCoordinates.y);
        float y = distance * Mathf.Cos(sphericalCoordinates.y);
        float z = distance * Mathf.Sin(sphericalCoordinates.x) * Mathf.Sin(sphericalCoordinates.y);

        Vector3 center = playerTransform.localPosition;
        Vector3 desiredLocalPosition = center + new Vector3(x, y, z);

        Vector3 centerWorld = transform.parent.TransformPoint(center);
        Vector3 desiredWorldPosition = transform.parent.TransformPoint(desiredLocalPosition);


        Ray ray = new Ray(centerWorld, desiredWorldPosition - centerWorld);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, distance))
        {
            Vector3 hitLocalPosition = playerTransform.parent.InverseTransformPoint(hit.point);
            transform.localPosition = hitLocalPosition - (desiredLocalPosition - center) * 0.5f;
        }
        else
        {
            transform.localPosition = desiredLocalPosition;
        }


        transform.LookAt(playerTransform.position + playerTransform.up * 5, playerTransform.up);

        if (!isShiftPressed)
            UpdatePlayerRotation();

        isShiftPressedLastFrame = isShiftPressed;

    }

    private void UpdatePlayerRotation()
    {
        Vector3 directionToCamera = transform.localPosition - playerTransform.localPosition;
        directionToCamera.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(-directionToCamera);

        playerTransform.localRotation = Quaternion.Slerp(playerTransform.localRotation, targetRotation, Time.deltaTime * 1000);


    }

    private void ResetMouseControl()
    {
        mouseRotX = 0;
        mouseRotY = 0;
    }



    float spiralAngle = 0.0f, spiralRadius, distanceToWormhole;
    public float spiralSpeed = 15.0f;
    public float spiralRadiusDenom = 25.0f;
    public float moveSpeedNum = 2.5f;
    public float minRad = 0.5f;

    /// <summary>
    /// Updates camera's position to spiral towards the wormhole.
    /// can be used in mode 1.
    /// </summary>
    // TODO: alter constants on main scene
    private void SpiralTowardsWormhole()
    {
        if (Vector3.Distance(transform.position, wormhole.position) > 1f)
        {
            spiralAngle += spiralSpeed * Time.deltaTime;
            distanceToWormhole = Vector3.Distance(transform.position, wormhole.position);
            spiralRadius = Mathf.Min(minRad, distanceToWormhole / spiralRadiusDenom);
            Vector3 spiralOffest = new Vector3(
                Mathf.Cos(spiralAngle) * spiralRadius,
                Mathf.Sin(spiralAngle) * spiralRadius,
                0);

            transform.position = Vector3.Lerp(transform.position, wormhole.position, moveSpeed * Time.deltaTime) + spiralOffest;
            moveSpeed = moveSpeedNum;

            transform.LookAt(wormhole);
        }
        else
        {
            exitWormholeMode();
            gameManager.exitWormhole();
        }
    }

    /// <summary>
    /// Accelerates towards wormhole. can be used in mode 1.
    /// </summary>
    private void MoveTowardsWormhole()
    {
        if (Vector3.Distance(transform.position, wormhole.position) > 2f)
        {
            transform.position = Vector3.Lerp(transform.position, wormhole.position, moveSpeed * Time.deltaTime);
            moveSpeed += 2.0f * Time.deltaTime;
            transform.LookAt(wormhole);
        }
        else
        {
            exitWormholeMode();
            gameManager.exitWormhole();
        }
    }

    public void enterWormholeMode(Transform wormhole)
    {
        gameState = GameState.WormholeEffect;
        cameraMouseManager.SetMouseControl(false);
        this.wormhole = wormhole;
    }

    public void exitWormholeMode()
    {
        gameState = GameState.Playing;
        cameraMouseManager.SetMouseControl(true);
    }
}