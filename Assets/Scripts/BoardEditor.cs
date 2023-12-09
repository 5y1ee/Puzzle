using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.VisualScripting;

[CustomEditor(typeof(BoardManagerScript))]
public class BoardEditor : Editor
{
    BoardManagerScript m_BoardManager;
    int[,] color_2d, connect_2d;

    private void OnEnable()
    {
        m_BoardManager = target as BoardManagerScript;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!Application.isPlaying || m_BoardManager.initPool == false)
            return;

        int row = m_BoardManager.Matrix.row;
        int col = m_BoardManager.Matrix.col;

        // color 디버그
        EditorGUILayout.LabelField("Block Colors");

        color_2d = new int[row, col];
        //color_2d = _board.Board.Clone() as int[,];
        var board = m_BoardManager.BoardOBJ;
        for (int r=0; r < row; ++r)
        {
            for (int c=0; c < col; ++c)
                color_2d[r, c] = (int)board[r, c].Color;
        }

        for (int r = 0; r < row; ++r)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < col; ++c)
            {
                color_2d[r, c] = EditorGUILayout.IntField(color_2d[r, c]);
            }
            EditorGUILayout.EndHorizontal();
        }

        // connected 디버그
        EditorGUILayout.LabelField("Block Connected");

        connect_2d = new int[row, col];
        for (int r = 0; r < row; ++r)
        {
            for (int c = 0; c < col; ++c)
            {
                connect_2d[r, c] = board[r, c].Connected;
            }
        }

        for (int r = 0; r < row; ++r)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < col; ++c)
            {
                connect_2d[r, c] = EditorGUILayout.IntField(connect_2d[r, c]);
            }
            EditorGUILayout.EndHorizontal();
        }

    }

}
