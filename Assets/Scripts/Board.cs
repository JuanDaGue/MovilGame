using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using System.Linq;

public class Board : MonoBehaviour
{
    public float timeBetwenPieces =  0.01f;
    public int width;
    public int height;
    public GameObject tileObject;
    public float cameraSizeOffset;
    public float cameraVerticalOffset;

    Tile[,] Tiles;
    Pieces[,] Pieces;
    Tile starTile;
    Tile endTile;
    public GameObject[] availablePieces;
    bool swappingPieces= false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Tiles = new Tile[width, height];
        Pieces = new Pieces[width, height];
        SetUpBoard();
        PositionCamera();
        StartCoroutine(SetUpPieces());
    }

    // Update is called once per frame
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
        int maxIterations_= 50;
        int currentIteration=0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {   
                yield return new WaitForSeconds(timeBetwenPieces);
                if(Pieces[x,y]== null){
                currentIteration=0;
                var newPice= CreatedpieceAt(x,y);
                while(HasPreviousMatches(x,y)){
                    ClearPieceAt(x,y);
                    newPice = CreatedpieceAt(x,y);
                    currentIteration++;
                    if(currentIteration>= maxIterations_){
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
        var pieceToClear =  Pieces[x,y] ;
        pieceToClear.Remove(true);
        Pieces[x,y] = null;
    }

    private Pieces CreatedpieceAt(int x, int y){
        var selectedPiece = availablePieces[UnityEngine.Random.Range(0, availablePieces.Length)];
        var o = Instantiate(selectedPiece, new Vector3(x, y+1, -5), Quaternion.identity);
        o.transform.parent = transform;
        Pieces[x, y] = o.GetComponent<Pieces>();
        Pieces[x, y]?.Setup(x, y, this);
        Pieces[x, y]?.Move(x, y);
        return Pieces[x,y];
    }
    public void TileDown(Tile tile_)
    {
        if(!swappingPieces){
            starTile = tile_;
        }
    }

    public void TileOver(Tile tile_)
    {
        if(!swappingPieces){
            endTile = tile_;
            }
    }

    public void TileUp(Tile tile_)
    {
        if(!swappingPieces){
            if (starTile != null && endTile != null && IsCloseTo(starTile, endTile))
            {
                StartCoroutine(SwapTiles());
            }
        }
    }
    //Function for swap the gameObject 
    IEnumerator SwapTiles()
    {
        swappingPieces= true;
        var StartPiece = Pieces[starTile.x, starTile.y];
        var EndPiece = Pieces[endTile.x, endTile.y];

        StartPiece.Move(endTile.x, endTile.y);
        EndPiece.Move(starTile.x, starTile.y);

        Pieces[starTile.x, starTile.y] = EndPiece;
        Pieces[endTile.x, endTile.y] = StartPiece;
        yield return new WaitForSeconds(0.6f);
        var startMatches = GetMatchByPiece(starTile.x, starTile.y,3 );
        var endMatches = GetMatchByPiece(endTile.x, endTile.y,3);

        var allMatches = startMatches.Union(endMatches).ToList();



        if(allMatches.Count==0){
            StartPiece.Move(starTile.x, starTile.y);
            EndPiece.Move(endTile.x, endTile.y) ;
            Pieces[starTile.x, starTile.y]= StartPiece;
            Pieces[endTile.x, endTile.y]= EndPiece;
        }
        else{
            ClearPiece(allMatches);
        }
        starTile = null;
        endTile = null;
        swappingPieces= false;
        yield return null;
    }

    private void ClearPiece(List<Pieces> piecesToClear)
    {
        piecesToClear.ForEach(piece=>{
            ClearPieceAt(piece.x,piece.y);
        });
        List<int> colums = GetColums(piecesToClear);
        List<Pieces>collapsePieces = collapseColums(colums, 0.3f);
        FindMatchRecursively(collapsePieces);
    }

    private void FindMatchRecursively(List<Pieces> collapsePieces)
    {
        StartCoroutine(FindMatchRecursivelyCoroutine(collapsePieces));
    }

    IEnumerator FindMatchRecursivelyCoroutine(List<Pieces> collapsePieces)
    {
        yield return new WaitForSeconds(0.3f);
        List <Pieces> newMatches = new List<Pieces>();
        collapsePieces.ForEach(piece=>{
            var matches = GetMatchByPiece(piece.x, piece.y);
            if(matches != null){
                newMatches = newMatches.Union(matches).ToList();
                ClearPiece(matches);
            }
        });
        if(newMatches.Count >0){
            var newCollapsedPieces = collapseColums(GetColums(newMatches), 0.3f);
            FindMatchRecursively(newCollapsedPieces);
        }
        else{
            yield return new WaitForSeconds(timeBetwenPieces);
            StartCoroutine(SetUpPieces());
            swappingPieces=false;
        }
        yield return null;
    }

    private List<Pieces> collapseColums(List<int> colums, float v)
    {
        List<Pieces> movingPieces = new List<Pieces>();
        for (int i= 0; i< colums.Count; i++){
                var column = colums[i];
                for (int y=0;y< height; y++){
                    if(Pieces[column, y]== null){
                        for (int yplus =y+1; yplus< height; yplus++){
                            if(Pieces[column, yplus] != null){
                                Pieces[column, yplus].Move(column, y);
                                Pieces[column, y]= Pieces[column, yplus];
                                if(! movingPieces.Contains(Pieces[column, y])){
                                    movingPieces.Add(Pieces[column,y]);
                                }
                                Pieces[column, yplus]=null;
                                break;
                            }
                        }
                    }
                }
        }
        return movingPieces;
    }

    private List<int> GetColums(List<Pieces> piecesToClear)
    {
        var result = new List<int>();
        piecesToClear.ForEach(piece=>{
                if(!result.Contains(piece.x)){
                    result.Add(piece.x);
                }
        });
        return result;
    }

    public bool IsCloseTo(Tile Start, Tile end)
    {
        if (math.abs(Start.x - end.x) == 1 && Start.y == end.y)
        {
            return true;
        }
        if (math.abs(Start.y - end.y) == 1 && Start.x == end.x)
        {
            return true;
        }
        return false;
    }

    public List<Pieces> GetMatchByDirection(int xpos, int ypos, UnityEngine.Vector2 direction, int minPieces = 3)
    {
        List<Pieces> matches = new List<Pieces>();
        Pieces StartPiece = Pieces[xpos, ypos];
        matches.Add(StartPiece);

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
                if (nextPiece != null && nextPiece.pieceType == StartPiece.pieceType)
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
    bool HasPreviousMatches(int xpos, int ypos){
        var downMatches = GetMatchByDirection(xpos, ypos, new UnityEngine.Vector2(0, -1), 2)?? new List<Pieces>();
        var leftMatches = GetMatchByDirection(xpos, ypos, new UnityEngine.Vector2(-1, 0), 2) ?? new List<Pieces>();
        
        return (downMatches.Count>0 || leftMatches.Count>0);
    }

}
