using UnityEngine;
using DG.Tweening;
public class Pieces : MonoBehaviour
{
    public int x;
    public int y;
    public Board board;

    public enum type{
        elephant,
        giraffe,
        hippo,
        monkey,
        panda,
        parrot,
        pengin, 
        pig, 
        rabbit, 
        snake
    }

    public type pieceType;
    public void Setup( int x_, int y_ , Board board_){
        x= x_;
        y= y_;
        board= board_;

        transform.localScale = Vector3.one*0.35f;
        transform.DOScale(Vector3.one, 0.35f);
    }

    public void Move(int desX, int DesY){
        transform.DOMove(new Vector3(desX, DesY, -5f), 0.25f).SetEase(Ease.InOutCubic).onComplete = ()=>{
            x= desX;
            y = DesY; 
        };
    }

    public void Remove(bool animated){
        if(animated){
            transform.DORotate(new Vector3(0,0,-120f),0.85f);
            transform.DOScale(Vector3.one*1.2f,0.085f).onComplete=()=>{
                transform.DOScale(Vector3.zero,0.1f).onComplete =()=>{
                    Destroy(gameObject);
                };
            };
        }else{

        }
    }
    [ContextMenu("Test Move")]
    public void MoveTest(){
            Move(0,0);
    }
}


