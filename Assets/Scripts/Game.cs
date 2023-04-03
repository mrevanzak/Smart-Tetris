using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Game : MonoBehaviour
{
    public TetrominoData[] tetrominos;

    [SerializeField] Tilemap tilemap;
    [SerializeField] GameController controller;
    [SerializeField] Vector3Int spawnPosition;
    [SerializeField] Vector2Int boardSize = new Vector2Int(10, 20);
    [SerializeField] int level = 1;
    [SerializeField] public List<float> observations = new List<float>();
    [SerializeField] PlayAgent agent;

    public Tilemap TileMap { get => tilemap; private set => tilemap = value; }
    public GameController Gamecontroller { get => controller; private set => controller = value; }
    public Vector2Int BoardSize { get => boardSize; private set => boardSize = value; }

    public PlayAgent PAgent { get => agent; private set => agent = value; }

    public RectInt bounds {
        get {
            Vector2Int pos = new Vector2Int(-this.boardSize.x / 2, -this.boardSize.y / 2);
            return new RectInt(pos, this.boardSize);
        }
    }

    // for randomizer
    private TetrominoData[] nextTetrominos;
    private int currentTetrominoCount;


    private void Awake() {
        if(agent == null){
            InitGame();
            InitRandomizer();
            SpawnTetrominoRandomizer();
            controller.GetNewObservation();
        }
    }

    public void AgentInit() {
        ResetGame();
    }

    public void ResetGame() {
        // clear board
        tilemap.ClearAllTiles();
        // reset randomizer
        currentTetrominoCount = 9999;
        // spawn new piece
        SpawnTetrominoRandomizer();

        controller.GetNewObservation();
    }

    public void InitGame(){
        for(int x = 0; x < tetrominos.Length; x++){
            tetrominos[x].Init();
        }
        for(int x = 0; x < observations.Count; x++){
            observations[x] = 0;
        }
    }

    public void GameOver() {
        tilemap.ClearAllTiles();

        if(agent != null){
            agent.GameOverReward();
            agent.EndEpisode();
        }
    }

    #region Randomize Algorithm (7-bag algorithm)

    private int halfLength;

    public void InitRandomizer() {
        currentTetrominoCount = 0;
        nextTetrominos = new TetrominoData[tetrominos.Length];
        for(int x = 0; x < tetrominos.Length; x++){
            nextTetrominos[x] = tetrominos[x];
        }
        halfLength = nextTetrominos.Length / 2;
        randomizeTetrominos();
    }

    void randomizeTetrominos(){
        for(int x = nextTetrominos.Length - 1; x > halfLength; x--){ // swap at random index (half for eff)
            int y = Random.Range(0, x + 1);
            TetrominoData data = nextTetrominos[x];
            nextTetrominos[x] = nextTetrominos[y];
            nextTetrominos[y] = data;
        }
    }

    public void SpawnTetrominoRandomizer() {
        if(currentTetrominoCount >= nextTetrominos.Length){
            // reset randomizer
            currentTetrominoCount = 0;
            randomizeTetrominos();
        }
        SpawnTetromino(nextTetrominos[currentTetrominoCount]);
        currentTetrominoCount++;
    }

    #endregion

    #region Spawn

    public void SpawnRandomTetromino(){
        int random = Random.Range(0, tetrominos.Length);
        TetrominoData data = this.tetrominos[random];
        SpawnTetromino(data);
    }

    public void SpawnTetromino(TetrominoData data){
        this.controller.Init(this, spawnPosition, data);

        if (ValidPos(spawnPosition)) {
            SetCurrentTetromino();
        } else {
            GameOver();
        }
    }

    #endregion

    #region Set/Clear

    public void SetCurrentTetromino() {
        foreach(Vector3Int tetrominoPosition in this.controller.tetrominoPositions){
            Vector3Int tilePosition = tetrominoPosition + this.controller.pos;
            tilemap.SetTile(tilePosition, this.controller.data.tile);
        }
    }

    public void ClearCurrentTetromino() {
        foreach(Vector3Int tetrominoPosition in this.controller.tetrominoPositions){
            Vector3Int tilePosition = tetrominoPosition + this.controller.pos;
            tilemap.SetTile(tilePosition, null);
        }
    }

    #endregion

    public bool ValidPos(Vector3Int pos){
        foreach(Vector3Int otherPos in this.controller.tetrominoPositions){
            if(!bounds.Contains((Vector2Int) (otherPos + pos)))
                return false;
            if(this.tilemap.HasTile(otherPos + pos))
                return false;
        }
        return true;
    }

    #region Clearing Lines

    public int CheckLines(out bool perfectClear) {
        RectInt bound = this.bounds;
        int row = bound.yMin;

        int linesCleared = 0;
        perfectClear = true;

        while(row < bounds.yMax){
            if(ClearableLine(row)){
                ClearLine(row);
                linesCleared++;
            } else {
                row++;
            }
        }

        // check for PC and check the board
        perfectClear = CheckBoard();

        return linesCleared;
    }

    void ClearLine(int row) {
        RectInt bound = this.bounds;
        for(int col = bound.xMin; col < bound.xMax; col++){
            this.tilemap.SetTile(new Vector3Int(col, row, 0), null);
        }

        // drop everything down
        while(row < bound.yMax){
            for(int col = bound.xMin; col < bound.xMax; col++){
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = this.tilemap.GetTile(position);
                
                position = new Vector3Int(col, row, 0);
                this.tilemap.SetTile(position, above);
            }

            row++;
        }
    }

    bool ClearableLine(int row) {
        RectInt bound = this.bounds;
        for(int col = bound.xMin; col < bound.xMax; col++){
            if(!this.tilemap.HasTile(new Vector3Int(col, row, 0))){
                return false;
            }
        }
        return true;
    }

    bool CheckBoard(){
        RectInt bound = this.bounds;
        int row = bound.yMin;
        int lowestRow = bounds.yMax;
        bool result = true;
        bool emptyRow = true;

        int count = 0;
        while(row < bounds.yMax){
            for(int col = bound.xMin; col < bound.xMax; col++){
                if(this.tilemap.HasTile(new Vector3Int(col, row, 0))){
                    result = false;
                    emptyRow = false;
                }
                count++;
            }
            if(emptyRow && row < lowestRow){
                lowestRow = row;
            }
            row++;
            emptyRow = true;
        }
        //if(agent != null)
        //    agent.CheckDroppedReward(lowestRow);
        return result;
    }

    #endregion

    #region Scoring

    public bool CheckTSpin(Vector3Int pos){
        if((this.tilemap.HasTile(pos + BaseData.VectorDownRight) && // check for base
            this.tilemap.HasTile(pos + BaseData.VectorDownLeft)) &&
           (this.tilemap.HasTile(pos + BaseData.VectorUpRight) || // check for overhang
            this.tilemap.HasTile(pos + BaseData.VectorUpLeft))){ 
                return true;
           }

        return false;
    }

    public void CalculateScoring(int clearedLines, bool tspin, int comboCount, bool backToBack, bool perfectClear) {
        float score = 0;
        if(tspin && clearedLines > 0){
            score += (level) * (4) * (clearedLines + 1);
            switch(clearedLines){
                case 1:
                    Debug.Log("T-Spin single");
                    break;
                case 2:
                    Debug.Log("T-Spin double");
                    break;
                case 3:
                    Debug.Log("T-Spin triple");
                    break;
                default:
                    break;
            }
        } else if(perfectClear && clearedLines > 0){
            if(perfectClear && backToBack){
                Debug.Log("Perfect Clear Back to Back");
                score += (ulong)(level) * (32);
            } else {
                switch(clearedLines){
                    case 1:
                        score += (ulong)(level) * (8);
                        Debug.Log("Perfect Clear single");
                        break;
                    case 2:
                        score += (ulong)(level) * (12);
                        Debug.Log("Perfect Clear double");
                        break;
                    case 3:
                        score += (ulong)(level) * (18);
                        Debug.Log("Perfect Clear triple");
                        break;
                    case 4:
                        score += (ulong)(level) * (2);
                        Debug.Log("Perfect Clear tetris");
                        break;
                    default:
                        break;
                }
            }
        } else {
            switch(clearedLines){
                case 1:
                    score += (ulong) level * 1;
                    break;
                case 2:
                    score += (ulong) level * 3;
                    break;
                case 3:
                    score += (ulong) level * 5;
                    break;
                case 4:
                    score += (ulong) level * 8;
                    break;
                default:
                    break;
            }
        }

        if(comboCount > 0){
            Debug.Log(comboCount + " combos");
            score += (float) comboCount * 0.5f;
        }

        if(backToBack && !perfectClear){
            Debug.Log("Back to Back");
            score += score/2; // (x 1.5)
        }
    }

    public void CalculateAgentScoring(int clearedLines, bool tspin, int comboCount, bool backToBack, bool perfectClear) {
        float score = 0.0f;
        if(tspin && clearedLines > 0){
            score += (level) * (4) * (clearedLines + 1);
            switch(clearedLines){
                case 1:
                    agent.sTspin += 1;
                    break;
                case 2:
                    agent.dTspin += 1;
                    break;
                case 3:
                    agent.tTspin += 1;
                    break;
                default:
                    break;
            }
        } else if(perfectClear && clearedLines > 0){
            if(perfectClear && backToBack){
                agent.b2b += 1;
                score += (level) * (32);
            } else {
                switch(clearedLines){
                    case 1:
                        score += (level) * (8);
                        agent.sPc += 1;
                        break;
                    case 2:
                        score += (level) * (12);
                        agent.dPc += 1;
                        break;
                    case 3:
                        score += (level) * (18);
                        agent.tPc += 1;
                        break;
                    case 4:
                        score += (level) * (20);
                        agent.qPc += 1;
                        break;
                    default:
                        break;
                }
            }
        } else {
            switch(clearedLines){
                case 1:
                    score += level * 1;
                    break;
                case 2:
                    score += level * 3;
                    break;
                case 3:
                    score += level * 5;
                    break;
                case 4:
                    score += level * 8;
                    break;
                default:
                    break;
            }
        }

        if(comboCount > 0){
            if(agent.maxCombo < comboCount)
                agent.maxCombo = comboCount;
            score += comboCount * 0.5f;
        }

        if(backToBack && !perfectClear){
            agent.b2b += 1;
            score += score/2; // (x 1.5)
        }

        agent.curLines += clearedLines;
        if(agent.curLines > agent.maxLines)
            agent.maxLines = agent.curLines;

        agent.TetrisScoringReward((int) score);
    }

    #endregion
}
