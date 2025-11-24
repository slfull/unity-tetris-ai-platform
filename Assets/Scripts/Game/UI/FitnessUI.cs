using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class FitnessUI : MonoBehaviour
{
    private Board board;
    [SerializeField] private TextMeshProUGUI fitnessUI;
    private int pieceCount;
    private float prevFitness;
    private float averageDeltaFitness;
    private int lines;

    private void Start()
    {
        board = GetComponent<Board>();
        pieceCount = 0;
        prevFitness = 0f;
        lines = 0;
        averageDeltaFitness = 0f;
        board.onPieceLock += OnPieceLock;
        board.onSetNextPiece += OnSetNextPiece;
        fitnessUI.text = $"average delta fitness: {averageDeltaFitness}";
    }

    private void OnPieceLock(int line, int combo, bool isRotation, bool b2b)
    {
        lines = line;
    }

    private void OnSetNextPiece()
    {
        uiUpdate(DeltaFitness(lines));
        pieceCount++;
    }

    private void uiUpdate(float deltaFitness)
    {
        averageDeltaFitness *= pieceCount;
        averageDeltaFitness += deltaFitness;
        averageDeltaFitness /= pieceCount + 1;
        fitnessUI.text = $"average delta fitness: {averageDeltaFitness}";
    }

    private float DeltaFitness(int line)
    {
        float fitness = Fitness(line);
        Debug.Log($"[fitness]{fitness}");
        float res = fitness - prevFitness;
        prevFitness = fitness;
        return res;
    }

    private float Fitness(int line)
    {
        bool[] fields = board.GetField(true);
        int width = board.Bounds.width;
        int height = board.Bounds.height;
        var (columnHeight, aggregateHeight, completedLines, holes, bumpiness) 
            = Observations.GetCalculatedObservations(fields, width, height);
        
        float fitness = -0.51f * aggregateHeight + 0.76f * line - 0.36f * holes - 0.18f * bumpiness;
        return fitness;
    }
}
