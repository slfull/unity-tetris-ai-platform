using UnityEngine;
using TMPro;

public class BoardUI : MonoBehaviour
{
    public Board board;
    public TextPopAnimator scoreText;
    public TextPopAnimator cleartypeText;
    public TextPopAnimator comboText;
    public TextPopAnimator b2bCountText;

    private string scoreString = "";
    private string comboString = "";
    private string cleartypeString = "";
    private string b2bCountString = "";
    void Awake()
    {
        board = GetComponent<Board>();
        board.onUiUpdate += OnUiUpdate;
    }

    private void OnDisable()
    {
        board = GetComponent<Board>();
        board.onUiUpdate -= OnUiUpdate;
    }
    private void OnUiUpdate(int score, int combo, int cleartype, int b2bCount)
    {
        if (scoreString != "Score: " + score.ToString())
        {
            scoreString = "Score: " + score.ToString();
            scoreText.Show(scoreString);
            string cleartypeStringComp = "";
        switch (cleartype)
        {
            case 1: cleartypeStringComp = "Single"; break;
            case 2: cleartypeStringComp = "Double"; break;
            case 3: cleartypeStringComp = "Triple"; break;
            case 4: cleartypeStringComp = "Tetris"; break;
            case 5: cleartypeStringComp = "TSpinDouble"; break;
            case 6: cleartypeStringComp = "TSpinTriple"; break;
            default: break;
        }

            cleartypeString = cleartypeStringComp;
            cleartypeText.Show(cleartypeString);
        
        }

        if (comboString != combo.ToString() + "X Combo ")
        {
            comboString = combo.ToString() + "X Combo ";
            comboText.Show(comboString);
        }


        if (b2bCountString != b2bCount.ToString() + "X b2b ")
        {
            b2bCountString = b2bCount.ToString() + "X b2b ";
            b2bCountText.Show(b2bCountString);
        }



        

    }
}