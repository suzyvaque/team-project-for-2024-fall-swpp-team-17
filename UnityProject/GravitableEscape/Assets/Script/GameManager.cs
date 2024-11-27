using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OurGame;

public class GameManager : MonoBehaviour
{
    public CameraManager cameraManager;
    public PlayerManager playerManager;
    public Vector3 wormholeTargetPos;
    public GameState gameState; // TODO: make this singleton?
    Subject<GameStateObserver, GameState> gameStateObs;
    void Start()
    {
        // cameraManager = GameObject.Find("CameraManager").GetComponent<CameraManager>();
        gameState = GameState.Playing;

        gameStateObs = new Subject<GameStateObserver, GameState>();
        cameraManager = FindObjectOfType<CameraManager>();
        playerManager = FindObjectOfType<PlayerManager>();
        gameStateObs.AddObserver(cameraManager);
        gameStateObs.AddObserver(playerManager);
        gameStateObs.NotifyObservers(gameState);
    }

    // Update is called once per frame
    void Update()
    {

    }

    /// <summary>
    /// This function is called by WormholeManager when player is close enough to the wormhole.
    /// It triggers animation of warping into the wormhole.
    /// </summary>
    /// <param name="wormhole">transform of the wormhole object</param>
    /// <param name="targetPos">position to move after animation</param>
    public void startWormhole(Transform wormhole, Vector3 targetPos)
    {
        wormholeTargetPos = targetPos;
        cameraManager.SetWormhole(wormhole);
        gameState = GameState.WormholeEffect;
        gameStateObs.NotifyObservers(gameState);
    }

    public bool foo = false;
    /// <summary>
    /// This function is called when the animation warping into the wormhole ends.
    /// </summary>
    public void exitWormhole()
    {
        foo = true;
        playerManager.Teleport(wormholeTargetPos);
        gameState = GameState.Playing;
        gameStateObs.NotifyObservers(gameState);
    }
}
