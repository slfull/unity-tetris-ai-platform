using TMPro;
using UnityEngine;

public class TwoPlayerGameOver : MonoBehaviour
{
    [SerializeField] Board opponent;
    [SerializeField] GameObject gameOverText;
    [SerializeField] TextMeshProUGUI winnerText;
    private Board board;
    
    void Start()
    {
        board = GetComponent<Board>();
        board.onGameOver += OnGameOver;
        opponent.onGameOver += OnOpponentGameOver;
        gameOverText.SetActive(false);
    }

    public void OnGameOver()
    {
        DestroyAll(board);
        DestroyAll(opponent);

        ColdClearAgent cc = GetComponent<ColdClearAgent>();
        if (cc != null)
        {
            Destroy(cc);
            Destroy(GetComponent<BoardToColdClear>());
        }

        HeuristicAgent ha = GetComponent<HeuristicAgent>();
        if (ha != null)
        {
            Destroy(ha);
        }

        gameOverText.SetActive(true);

        if (GetComponent<PlayerTwoControll>() == null)
        {
            winnerText.text = "Player2 Win";
        }
        else
        {
            winnerText.text = "Player1 Win";
        }
    }

    public void OnOpponentGameOver()
    {
        ColdClearAgent cc = GetComponent<ColdClearAgent>();
        if (cc != null)
        {
            Destroy(cc);
            Destroy(GetComponent<BoardToColdClear>());
        }

        HeuristicAgent ha = GetComponent<HeuristicAgent>();
        if (ha != null)
        {
            Destroy(ha);
        }
    }

    private void DestroyAll(Board b)
    {
        Destroy(b.activePiece);
        if(b.savedPiece != null)
        {
            Destroy(b.savedPiece);
        }
        Destroy(b.nextPiece);
        Destroy(b.nextPiece2);
        Destroy(b.nextPiece3);
        Destroy(b.nextPiece4);
        Destroy(b.nextPiece5);
        Destroy(b);
    }
}
