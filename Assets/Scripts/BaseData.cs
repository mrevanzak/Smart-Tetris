using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BaseData
{

    public static readonly Vector3Int VectorUp = new Vector3Int(0, 1, 0);
    public static readonly Vector3Int VectorDown = new Vector3Int(0, -1, 0);
    public static readonly Vector3Int VectorLeft = new Vector3Int(-1, 0, 0);
    public static readonly Vector3Int VectorRight = new Vector3Int(1, 0, 0);

    public static readonly Vector3Int VectorUpLeft = VectorUp + VectorLeft;
    public static readonly Vector3Int VectorDownLeft = VectorDown + VectorLeft;
    public static readonly Vector3Int VectorUpRight = VectorUp + VectorRight;
    public static readonly Vector3Int VectorDownRight = VectorDown + VectorRight;

    // Rotation Matrix  | cos -sin |
    //              R = | sin  cos |
    public static readonly float cos = Mathf.Cos(Mathf.PI / 2f);
    public static readonly float sin = Mathf.Sin(Mathf.PI / 2f);
    public static readonly float[] RotationMatrix = new float[] { cos, sin, -sin, cos };

    #region BASE TETROMINO LOCATIONS

    public static readonly Dictionary<Tetromino, Vector2Int[]> TetrominoLocations = new Dictionary<Tetromino, Vector2Int[]>()
    {
        { Tetromino.I, new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int( 0, 1), new Vector2Int( 1, 1), new Vector2Int( 2, 1) } },
        { Tetromino.J, new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int(-1, 0), new Vector2Int( 0, 0), new Vector2Int( 1, 0) } },
        { Tetromino.L, new Vector2Int[] { new Vector2Int( 1, 1), new Vector2Int(-1, 0), new Vector2Int( 0, 0), new Vector2Int( 1, 0) } },
        { Tetromino.O, new Vector2Int[] { new Vector2Int( 0, 1), new Vector2Int( 1, 1), new Vector2Int( 0, 0), new Vector2Int( 1, 0) } },
        { Tetromino.S, new Vector2Int[] { new Vector2Int( 0, 1), new Vector2Int( 1, 1), new Vector2Int(-1, 0), new Vector2Int( 0, 0) } },
        { Tetromino.T, new Vector2Int[] { new Vector2Int( 0, 1), new Vector2Int(-1, 0), new Vector2Int( 0, 0), new Vector2Int( 1, 0) } },
        { Tetromino.Z, new Vector2Int[] { new Vector2Int(-1, 1), new Vector2Int( 0, 1), new Vector2Int( 0, 0), new Vector2Int( 1, 0) } },
    };

    #endregion

    #region WALL KICKING

    /*
                J, L, T, S, Z Tetromino Wall Kick Data
                Test 1	    Test 2	 Test 3	  Test 4   Test 5
    0>>1	basic rotation	(-1, 0)	(-1, 1)	 ( 0,-2)¹ (-1,-2)
    1>>0	basic rotation	( 1, 0)	( 1,-1)	 ( 0, 2)  ( 1, 2)
    1>>2	basic rotation	( 1, 0)	( 1,-1)	 ( 0, 2)  ( 1, 2)
    2>>1	basic rotation	(-1, 0)	(-1, 1)¹ ( 0,-2)  (-1,-2)
    2>>3	basic rotation	( 1, 0)	( 1, 1)¹ ( 0,-2)  ( 1,-2)
    3>>2	basic rotation	(-1, 0)	(-1,-1)	 ( 0, 2)  (-1, 2)
    3>>0	basic rotation	(-1, 0)	(-1,-1)	 ( 0, 2)  (-1, 2)
    0>>3	basic rotation	( 1, 0)	( 1, 1)	 ( 0,-2)¹ ( 1,-2)

    */

    private static readonly Vector2Int[,] WallKickJLTSOZ = new Vector2Int[,] {
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1,-1), new Vector2Int(0, 2), new Vector2Int( 1, 2) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1,-1), new Vector2Int(0, 2), new Vector2Int( 1, 2) },
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0,-2), new Vector2Int(-1,-2) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1, 1), new Vector2Int(0,-2), new Vector2Int( 1,-2) },
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1,-1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int(-1,-1), new Vector2Int(0, 2), new Vector2Int(-1, 2) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int( 1, 1), new Vector2Int(0,-2), new Vector2Int( 1,-2) },
    };

    private static readonly Vector2Int[,] WallKickI = new Vector2Int[,] {
        { new Vector2Int(0, 0), new Vector2Int(-2, 0), new Vector2Int( 1, 0), new Vector2Int(-2,-1), new Vector2Int( 1, 2) },
        { new Vector2Int(0, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 1), new Vector2Int(-1,-2) },
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 2), new Vector2Int( 2,-1) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int(-2, 0), new Vector2Int( 1,-2), new Vector2Int(-2, 1) },
        { new Vector2Int(0, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 1), new Vector2Int(-1,-2) },
        { new Vector2Int(0, 0), new Vector2Int(-2, 0), new Vector2Int( 1, 0), new Vector2Int(-2,-1), new Vector2Int( 1, 2) },
        { new Vector2Int(0, 0), new Vector2Int( 1, 0), new Vector2Int(-2, 0), new Vector2Int( 1,-2), new Vector2Int(-2, 1) },
        { new Vector2Int(0, 0), new Vector2Int(-1, 0), new Vector2Int( 2, 0), new Vector2Int(-1, 2), new Vector2Int( 2,-1) },
    };

    public static readonly Dictionary<Tetromino, Vector2Int[,]> WallKicks = new Dictionary<Tetromino, Vector2Int[,]>()
    {
        { Tetromino.I, WallKickI },
        { Tetromino.J, WallKickJLTSOZ },
        { Tetromino.L, WallKickJLTSOZ },
        { Tetromino.O, WallKickJLTSOZ },
        { Tetromino.S, WallKickJLTSOZ },
        { Tetromino.T, WallKickJLTSOZ },
        { Tetromino.Z, WallKickJLTSOZ },
    };

    #endregion
}
