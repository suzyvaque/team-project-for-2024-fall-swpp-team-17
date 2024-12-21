using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using OurGame;
using UnityEngine;

/// <summary>
/// This class manages the gravity of the overall game.
/// When number keys 1, 2, 3 are pushed, it rotates the gravity of the scene appropriately.
/// Objects that need to change direction to align with gravity (player, camera, etc.) are notified via observer pattern.
/// </summary>
public class GravityManager : MonoBehaviour, GameStateObserver
{
    public Vector3 initGravity = new Vector3(0, -35f, 0);
    public float lastChangeTime = -100f;
    private GameState gameState;
    Subject<GravityObserver, Quaternion> gravityChange;
    void Start()
    {
        CameraManager cameraManager = FindObjectOfType<CameraManager>();
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        gravityChange = new Subject<GravityObserver, Quaternion>();
        gravityChange.AddObserver(playerManager);
        gravityChange.AddObserver(cameraManager);
        Physics.gravity = initGravity;
    }

    /// <summary>
    /// This function checks if the gameState is Playing, 
    /// and if so, alters gravity according to the input key.
    /// </summary>
    void Update()
    {
        switch (gameState)
        {
            case GameState.Playing:
                if (Input.GetKeyDown(KeyCode.Alpha1)) RotateAngle(-90);
                else if (Input.GetKeyDown(KeyCode.Alpha2)) RotateAngle(-180);
                else if (Input.GetKeyDown(KeyCode.Alpha3)) RotateAngle(-270);
                break;
            case GameState.Revived:
                if (Input.GetKeyDown(KeyCode.Alpha1)) RotateAngle(-90);
                else if (Input.GetKeyDown(KeyCode.Alpha2)) RotateAngle(-180);
                else if (Input.GetKeyDown(KeyCode.Alpha3)) RotateAngle(-270);
                break;
        }
    }

    /// <summary>
    /// This function rotates the gravity, and notifies GravityObservers.
    /// </summary>
    /// <param name="angle">amount to rotate</param>
    void RotateAngle(int angle)
    {
        if (Time.time - lastChangeTime > 0.5f)
        {
            Physics.gravity = Quaternion.Euler(0, 0, angle) * Physics.gravity;
            gravityChange.NotifyObservers(Quaternion.Euler(0, 0, angle));
            lastChangeTime = Time.time;
        }
    }

    public void OnNotify<GameStateObserver>(GameState gs)
    {
        gameState = gs;
    }
}