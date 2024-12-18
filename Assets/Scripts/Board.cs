using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using System.Linq;

public class Board : MonoBehaviour
{
    public float timeBetwenPieces = 0.01f;
    public int width;
    public int height;
    public GameObject tileObject;
    public float cameraSizeOffset;
    public float cameraVerticalOffset;
    public int PointsPerMatch=10;
    Tile[,] Tiles;
    Pieces[,] Pieces;
    Tile starTile;
    Tile endTile;
    public GameObject[] availablePieces;
    bool swappingPieces = false;
    List<Pieces.type>selectedPieceTypes;

    void Start()
    {
        selectedPieceTypes = SelectRandomPieceTypes();
        Tiles = new Tile[width, height];
        Pieces = new Pieces[width, height];
        SetUpBoard();
        PositionCamera();
        StartCoroutine(SetUpPieces());
    }

    void Update()
    {
        
    }

    void SetUpBoard()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var o = Instantiate(tileObject, new Vector3(x, y, -5), Quaternion.identity);
                o.transform.parent = transform;
                Tiles[x, y] = o.GetComponent<Tile>();
                Tiles[x, y]?.Setup(x, y, this);
            }
        }
    }

    private void PositionCamera()
    {
        float newPosX = (float)width / 2f;
        float newPosY = (float)height / 2f;
        Camera.main.transform.position = new Vector3(newPosX - 0.5f, newPosY - 0.5f + cameraVerticalOffset, -10f);

        float horizontal = width + 1;
        float vertical = height / 2f + 1;
        Camera.main.orthographicSize = horizontal > vertical ? horizontal + cameraSizeOffset : vertical + cameraSizeOffset;
    }

    private IEnumerator SetUpPieces()
    {   
        int maxIterations_ = 50;
        int currentIteration = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {   
                yield return new WaitForSeconds(timeBetwenPieces);
                if (Pieces[x, y] == null)
                {
                    currentIteration = 0;
                    var newPiece = CreatePieceAt(x, y);
                    while (HasPreviousMatches(x, y))
                    {
                        ClearPieceAt(x, y);
                        newPiece = CreatePieceAt(x, y);
                        currentIteration++;
                        if (currentIteration >= maxIterations_)
                        {
                            break;
                        }
                    }
                }
            }
        }
        yield return null;
    }

    private void ClearPieceAt(int x, int y)
    {
        var pieceToClear = Pieces[x, y];
        if (pieceToClear != null)
        {
            pieceToClear.Remove(true);
            Pieces[x, y] = null;
        }
    }

    private Pieces CreatePieceAt(int x, int y)
    {
        var selectedPiece = availablePieces[UnityEngine.Random.Range(0, selectedPieceTypes.Count)];
        var o = Instantiate(selectedPiece, new Vector3(x, y + 1, -5), Quaternion.identity);
        o.transform.parent = transform;
        Pieces[x, y] = o.GetComponent<Pieces>();
        Pieces[x, y]?.Setup(x, y, this);
        Pieces[x, y]?.Move(x, y);
        return Pieces[x, y];
    }

    public void TileDown(Tile tile_)
    {
        if (!swappingPieces && GameManager.Instance.gameState == GameManager.GameState.InGame)
        {
            starTile = tile_;
        }
    }

    public void TileOver(Tile tile_)
    {
        if (!swappingPieces && GameManager.Instance.gameState == GameManager.GameState.InGame)
        {
            endTile = tile_;
        }
    }

    public void TileUp(Tile tile_)
    {
        if (!swappingPieces && GameManager.Instance.gameState == GameManager.GameState.InGame)
        {
            if (starTile != null && endTile != null && IsCloseTo(starTile, endTile))
            {
                StartCoroutine(SwapTiles());
            }
        }
    }

    IEnumerator SwapTiles()
    {
        swappingPieces = true;
        var StartPiece = Pieces[starTile.x, starTile.y];
        var EndPiece = Pieces[endTile.x, endTile.y];

        StartPiece?.Move(endTile.x, endTile.y);
        EndPiece?.Move(starTile.x, starTile.y);

        Pieces[starTile.x, starTile.y] = EndPiece;
        Pieces[endTile.x, endTile.y] = StartPiece;
        yield return new WaitForSeconds(0.6f);
        var startMatches = GetMatchByPiece(starTile.x, starTile.y, 3);
        var endMatches = GetMatchByPiece(endTile.x, endTile.y, 3);

        var allMatches = startMatches?.Union(endMatches)?.ToList() ?? new List<Pieces>();

        if (allMatches.Count == 0)
        {
            StartPiece?.Move(starTile.x, starTile.y);
            EndPiece?.Move(endTile.x, endTile.y);
            Pieces[starTile.x, starTile.y] = StartPiece;
            Pieces[endTile.x, endTile.y] = EndPiece;
        }
        else
        {
            ClearPieces(allMatches);
            AwardPoints(allMatches);
        }
        starTile = null;
        endTile = null;
        swappingPieces = false;
        yield return null;
    }

    private void ClearPieces(List<Pieces> piecesToClear)
    {
        piecesToClear.ForEach(piece => {
            ClearPieceAt(piece.x, piece.y);
        });
        List<int> columns = GetColumns(piecesToClear);
        List<Pieces> collapsePieces = CollapseColumns(columns, 0.3f);
        FindMatchesRecursively(collapsePieces);
    }

    private void FindMatchesRecursively(List<Pieces> collapsePieces)
    {
        StartCoroutine(FindMatchesRecursivelyCoroutine(collapsePieces));
    }

    IEnumerator FindMatchesRecursivelyCoroutine(List<Pieces> collapsePieces)
    {
        yield return new WaitForSeconds(0.3f);
        List<Pieces> newMatches = new List<Pieces>();
        collapsePieces.ForEach(piece => {
            var matches = GetMatchByPiece(piece.x, piece.y);
            if (matches != null)
            {
                newMatches = newMatches.Union(matches).ToList();
                ClearPieces(matches);
                AwardPoints(matches);
            }
        });
        if (newMatches.Count > 0)
        {
            var newCollapsedPieces = CollapseColumns(GetColumns(newMatches), 0.3f);
            FindMatchesRecursively(newCollapsedPieces);
        }
        else
        {
            yield return new WaitForSeconds(timeBetwenPieces);
            StartCoroutine(SetUpPieces());
            swappingPieces = false;
        }
        yield return null;
    }

    private List<Pieces> CollapseColumns(List<int> columns, float delay)
    {
        List<Pieces> movingPieces = new List<Pieces>();
        for (int i = 0; i < columns.Count; i++)
        {
            var column = columns[i];
            for (int y = 0; y < height; y++)
            {
                if (Pieces[column, y] == null)
                {
                    for (int yPlus = y + 1; yPlus < height; yPlus++)
                    {
                        if (Pieces[column, yPlus] != null)
                        {
                            Pieces[column, yPlus].Move(column, y);
                            Pieces[column, y] = Pieces[column, yPlus];
                            if (!movingPieces.Contains(Pieces[column, y]))
                            {
                                movingPieces.Add(Pieces[column, y]);
                            }
                            Pieces[column, yPlus] = null;
                            break;
                        }
                    }
                }
            }
        }
        return movingPieces;
    }

    private List<int> GetColumns(List<Pieces> piecesToClear)
    {
        var result = new List<int>();
        piecesToClear.ForEach(piece => {
            if (!result.Contains(piece.x))
            {
                result.Add(piece.x);
            }
        });
        return result;
    }

    public bool IsCloseTo(Tile start, Tile end)
    {
        if (math.abs(start.x - end.x) == 1 && start.y == end.y)
        {
            return true;
        }
        if (math.abs(start.y - end.y) == 1 && start.x == end.x)
        {
            return true;
        }
        return false;
    }

    public List<Pieces> GetMatchByDirection(int xpos, int ypos, UnityEngine.Vector2 direction, int minPieces = 3)
    {
        List<Pieces> matches = new List<Pieces>();
        Pieces startPiece = Pieces[xpos, ypos];
        matches.Add(startPiece);

        int nextX;
        int nextY;
        int maxVal = width > height ? width : height;
        for (int i = 1; i < maxVal; i++)
        {
            nextX = xpos + ((int)direction.x * i);
            nextY = ypos + ((int)direction.y * i);
            if (nextX >= 0 && nextX < width && nextY >= 0 && nextY < height)
            {
                var nextPiece = Pieces[nextX, nextY];
                if (nextPiece != null && nextPiece.pieceType == startPiece.pieceType)
                {
                    matches.Add(nextPiece);
                }
                else
                {
                    break;
                }
            }
        }
        if (matches.Count >= minPieces)
        {
            return matches;
        }
        return null;
    }

    public List<Pieces> GetMatchByPiece(int xpos, int ypos, int minPieces = 3)
    {
        var upMatches = GetMatchByDirection(xpos, ypos, new UnityEngine.Vector2(0, 1), 2);
        var downMatches = GetMatchByDirection(xpos, ypos, new UnityEngine.Vector2(0, -1), 2);
        var rightMatches = GetMatchByDirection(xpos, ypos, new UnityEngine.Vector2(1, 0), 2);
        var leftMatches = GetMatchByDirection(xpos, ypos, new UnityEngine.Vector2(-1, 0), 2);
        upMatches ??= new List<Pieces>();
        downMatches ??= new List<Pieces>();
        rightMatches ??= new List<Pieces>();
        leftMatches ??= new List<Pieces>();
        var verticalMatches = upMatches.Union(downMatches).ToList();
        var horizontalMatches = leftMatches.Union(rightMatches).ToList();
        var foundMatches = new List<Pieces>();
        if (verticalMatches.Count >= minPieces)
        {
            foundMatches = foundMatches.Union(verticalMatches).ToList();
        }
        if (horizontalMatches.Count >= minPieces)
        {
            foundMatches = foundMatches.Union(horizontalMatches).ToList();
        }
        return foundMatches;
    }

    bool HasPreviousMatches(int xpos, int ypos)
    {
        var downMatches = GetMatchByDirection(xpos, ypos, new UnityEngine.Vector2(0, -1), 2) ?? new List<Pieces>();
        var leftMatches = GetMatchByDirection(xpos, ypos, new UnityEngine.Vector2(-1, 0), 2) ?? new List<Pieces>();
        
        return (downMatches.Count > 0 || leftMatches.Count > 0);
    }

    private List<Pieces.type> SelectRandomPieceTypes()
        {
            var allTypes = Enum.GetValues(typeof(Pieces.type)).Cast<Pieces.type>().ToList();
            var selectedTypes = allTypes.OrderBy(x => UnityEngine.Random.value).Take(4).ToList();

            return selectedTypes;
        }

        public void AwardPoints(List<Pieces> allMatches){
            GameManager.Instance.AddPoints(allMatches.Count*PointsPerMatch);
        }
}
