using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;

// TODO: Do not let the Camera see outside of the hallway.

/// <summary>
/// This class manages the overall rotation and location of the camera.
/// Camera rotation due to mouse movement is managed by CameraMouseController.
/// </summary>
public class CameraManager : MonoBehaviour
{
    Transform gravityTransform, playerTransform;
    public Transform wormhole;
    Quaternion targetRot;
    public Vector3 offset = new Vector3(0, -3, 6);
    public float followSpeed = 15f;
    public float moveSpeed = 10.0f;
    public int cameraMode = 0; // 0 is default mode
    CameraMouseManager cameraMouseManager;
    GameManager gameManager;
    public float angle = 0.0f;
    public float spiralRadius = 5.0f;
    public float spiralSpeed = 5.0f;
    public float wormholeTriggerDistance = 50.0f;

    void Start()
    {
        gravityTransform = GameObject.Find("GravityManager").transform;
        playerTransform = GameObject.Find("Player").transform;
        targetRot = gravityTransform.rotation;
        cameraMouseManager = GameObject.Find("Main Camera").GetComponent<CameraMouseManager>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        cameraMode = 0;
    }

    /// <summary>
    /// This function updates camera rotation
    /// </summary>
    void Update()
    {
        switch (cameraMode)
        {
            case 0:
                FollowPlayer();
                break;
            case 1:
                MoveTowardsWormhole();
                break;
        }

    }

    private void FollowPlayer()
    {
        if (targetRot != transform.rotation) transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10 * Time.deltaTime);
        transform.position = playerTransform.position + transform.rotation * offset;
    }


    private void MoveTowardsWormhole()
    {
        if (Vector3.Distance(transform.position, wormhole.position) > 2f)
        {
            // 가속하며 웜홀로 이동
            transform.position = Vector3.Lerp(transform.position, wormhole.position, moveSpeed * Time.deltaTime);
            moveSpeed += 30.0f * Time.deltaTime; // 이동 속도 점차 증가

            // 웜홀을 바라보도록 카메라 회전
            transform.LookAt(wormhole);
        }
        else
        {
            cameraMode = 0;
            gameManager.exitWormhole();
        }
    }

    private void SpiralTowardsWormhole()
    {
        if (Vector3.Distance(transform.position, wormhole.position) > 2f)
        {
            // 나선형 각도와 반경 계산
            angle += spiralSpeed * Time.deltaTime;

            // 나선형 반경 계산
            float radius = spiralRadius * Mathf.Lerp(1, 0, Vector3.Distance(transform.position, wormhole.position) / wormholeTriggerDistance);

            // 나선형 위치 계산
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 nextPosition = Vector3.Lerp(transform.position, wormhole.position, moveSpeed * Time.deltaTime) + offset;

            // 카메라 위치와 회전 적용
            transform.position = nextPosition;
            transform.LookAt(wormhole);
        }
        else
        {
            cameraMode = 0;
            gameManager.exitWormhole();
        }
    }

    /// <summary>
    /// This function is called by GravityManager when the gravity changes.
    /// It updates targetRot, so that the camera's rotation can smoothly change to the gravity's direction via the Slerp in the Update().
    /// </summary>
    public void CameraRot()
    {
        targetRot = gravityTransform.rotation;
    }

    public void SwitchMode(int mode)
    {
        cameraMouseManager.SetMouseControl(false);
        cameraMode = mode;
    }

    public void enterWormholeMode(Transform wormhole)
    {
        cameraMouseManager.SetMouseControl(false);
        cameraMode = 1;
        this.wormhole = wormhole;
    }
}
