using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    
    [SerializeField] Game game;
    [SerializeField] GameController controller;
    [SerializeField] Ghost ghostTetromino;

    [Header("Game Settings")]

    [SerializeField] bool enableGhostTetromino = true;

    void Start(){
        ghostTetromino.GhostMode = enableGhostTetromino;

        StartGame();
    }

    void StartGame() {
        // init games
        game.InitGame();

        // start the game
        game.SpawnRandomTetromino();
    }
}
