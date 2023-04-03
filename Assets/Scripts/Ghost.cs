using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Ghost : MonoBehaviour
{
    [SerializeField] Tile ghostTile;
    [SerializeField] Game game;
    [SerializeField] GameController controller;

    [SerializeField] Tilemap tilemap;
    public Vector3Int[] tetrominoPositions { get; private set; }
    public Vector3Int position { get; private set; }

    [SerializeField] bool ghostMode = false;
    public bool GhostMode { get => ghostMode; set => ghostMode = value; }

    void Awake() {
        this.tetrominoPositions = new Vector3Int[4];
    }

    private void LateUpdate() {
        if(ghostMode){
            ClearGhost();
            CopyCurrentTetromino();
            DropGhostTetromino();
            SetGhostTetromino();
        }
    }

    void ClearGhost() {
        foreach(Vector3Int tetrominoPosition in this.tetrominoPositions){
            Vector3Int tilePosition = tetrominoPosition + this.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    void CopyCurrentTetromino(){
        for(int x = 0; x < this.tetrominoPositions.Length; x++){
            this.tetrominoPositions[x] = this.controller.tetrominoPositions[x];
        }
    }

    void DropGhostTetromino(){
        Vector3Int position = this.controller.pos;
        
        int current = position.y;
        int bottom = -this.game.BoardSize.y / 2 - 1;

        this.game.ClearCurrentTetromino();

        for(int row = current; row >= bottom; row--) {
            position.y = row;
            if(this.game.ValidPos(position)){
                this.position = position;
            } else {
                break;
            }
        }

        this.game.SetCurrentTetromino();
    }

    void SetGhostTetromino(){
        foreach(Vector3Int tetrominoPosition in this.tetrominoPositions){
            Vector3Int tilePosition = tetrominoPosition + this.position;
            tilemap.SetTile(tilePosition, this.ghostTile);
        }
    }
}
