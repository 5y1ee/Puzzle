using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

public class EditorWindowScript : EditorWindow
{
    Object m_BoardManager;
    int[,] color_2d, connect_2d;

    [MenuItem("Tools/2D Array Console")]
    static void init()
    {
        Debug.Log("Init");

        EditorWindow wnd = GetWindow<EditorWindowScript>();
        wnd.titleContent = new GUIContent("2D Array Console");
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }

    private void OnGUI()
    {
        // Color 2d
        GUILayout.BeginHorizontal();
        m_BoardManager = EditorGUILayout.ObjectField(m_BoardManager, typeof(Object), true);
        GUILayout.EndHorizontal();

        if (m_BoardManager == null) return;

        GameObject obj = (GameObject)m_BoardManager;
        BoardManagerScript obj_script = obj.GetComponent<BoardManagerScript>();

        if (!Application.isPlaying || obj_script.initPool == false) return;
        if (EditorApplication.isPaused) return;

        int row = obj_script.Matrix.row;
        int col = obj_script.Matrix.col;

        color_2d = new int[row, col];
        var board = obj_script.BoardOBJ;
        for (int r = 0; r < row; ++r)
        {
            for (int c = 0; c < col; ++c)
            {
                if (board[r, c] == null) color_2d[r, c] = -1;
                else color_2d[r, c] = (int)board[r, c].GetComponent<BlockScript>().Color;
            }
                
        }

        for (int r = 0; r < row; ++r)
        {
            GUILayout.BeginHorizontal();
            for (int c = 0; c < col; ++c)
                color_2d[r, c] = EditorGUILayout.IntField(color_2d[r, c]);
            GUILayout.EndHorizontal();
        }

        // Connect 2d
        GUILayout.BeginHorizontal();
        m_BoardManager = EditorGUILayout.ObjectField(m_BoardManager, typeof(Object), true);
        GUILayout.EndHorizontal();

        connect_2d = new int[row, col];
        for (int r = 0; r < row; ++r)
        {
            for (int c = 0; c < col; ++c)
            {
                if (board[r, c] == null) connect_2d[r, c] = -1;
                else connect_2d[r, c] = board[r, c].GetComponent<BlockScript>().Connected;
            }
        }

        for (int r = 0; r < row; ++r)
        {
            GUILayout.BeginHorizontal();
            for (int c = 0; c < col; ++c)
                connect_2d[r, c] = EditorGUILayout.IntField(connect_2d[r, c]);
            GUILayout.EndHorizontal();
        }

    }

}

#endif