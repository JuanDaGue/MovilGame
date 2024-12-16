using System;
using UnityEngine;

public class Board : MonoBehaviour
{
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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Tiles = new Tile[width, height];
        Pieces= new Pieces[width, height];
        SetUpBoard();
        PositionCamera();
        SetUpPieces();
    }



    // Update is called once per frame
    void Update()
    {
        
    }
    void SetUpBoard(){
        for(int x=0; x<width; x++){
            for(int y=0; y<height; y++){
                var o = Instantiate(tileObject, new Vector3(x,y,-5),Quaternion.identity);
                o.transform.parent = transform;
                Tiles[x,y]= o.GetComponent<Tile>();
               Tiles[x,y]?.Setup(x,y, this);
            }
        }

    }
        private void PositionCamera()
    {
        float newPosX =(float)width/2f;
        float newPosY =(float)height/2f;
        Camera.main.transform.position = new Vector3(newPosX-0.5f,newPosY-0.5f + cameraVerticalOffset,-10f);

        float horizontal  = width+1;
        float vertical = height/2f+1;
        Camera.main.orthographicSize = horizontal>vertical?horizontal +cameraSizeOffset :vertical+ cameraSizeOffset;
    }

    private void SetUpPieces()
    {
        for(int x=0; x<width; x++){
            for(int y=0; y<height; y++){
                var selectedPiece = availablePieces[UnityEngine.Random.Range(0,availablePieces.Length)];
                var o = Instantiate(selectedPiece, new Vector3(x,y,-5),Quaternion.identity);
                o.transform.parent = transform;
                Pieces[x,y]=o.GetComponent<Pieces>();
                Pieces[x,y]?.Setup(x,y, this);
            }
        }
    }
    public void TileDown(Tile tile_){
        starTile= tile_;
    }
    public void TileOver(Tile tile_){
        endTile= tile_;
    }    
    public void TileUp(Tile tile_){
        if(starTile!= null && endTile != null){
            SwapTiles();
        }
        starTile= null;
        endTile = null;
    }

    private void SwapTiles()
    {
        var StarPiece = Pieces[starTile.x, starTile.y];
        var EndPiece = Pieces[endTile.x, endTile.y];

        StarPiece.Move(endTile.x, endTile.y);
        EndPiece.Move(starTile.x, starTile.y);

        Pieces[starTile.x, starTile.y] = EndPiece;
        Pieces[endTile.x, endTile.y]= StarPiece;
    }
}
