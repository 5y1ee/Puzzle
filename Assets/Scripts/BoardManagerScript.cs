using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManagerScript : MonoBehaviour
{
    [System.Serializable]
    public struct MATRIX
    {
        public int row;
        public int col;
        public int cnt;
    }

    // 
    [SerializeField] private BlockScript[,] m_Board;    // 블록 배치에 대한 2차원 배열
    [SerializeField] private MATRIX m_matrix;   // 게임 보드에 대한 정보
    [SerializeField] private Grid _grid;        // 그리드로 게임 내 포지션 좌표 설정
    [SerializeField] private Transform m_Pool;  // 블록 풀링 트랜스폼
    [SerializeField] private ObjectPool<BlockScript> m_BlockPool;   // 블록 풀링 객체

    // 내부 변수
    private float grid_len, grid_gap;   
    private int Row, Col;
    // 탐색 순서 left -> down -> right -> up
    private int[] dr = { 0, 1, 0, -1 };
    private int[] dc = { -1, 0, 1, 0 };

    // Property
    public BlockScript[,] BoardOBJ { get { return m_Board; } }
    public MATRIX Matrix {  get { return m_matrix; } set {  m_matrix = value; } }

    // Method
    private void Awake()
    {
        // 초기 stage 설정
        Row = m_matrix.row = 10;
        Col = m_matrix.col = 10;
        m_matrix.cnt = Row * Col;
        SetPosition();

        m_Board = new BlockScript[Row, Col];

    }
    private void Start()
    {

    }

    public bool initPool = false;
    public void RunStage(uint stage)
    {
        if (!initPool)
        {
            m_BlockPool = m_Pool.GetComponent<BlockPoolScript>().BlockPool;
            initPool = true;
        }
        GenerateTile(stage);
        UpdateBoard();

    }

    // Grid Position Setting
    private void SetPosition()
    {
        grid_len = _grid.cellSize.x;
        grid_gap = _grid.cellGap.x;
        float _len = grid_len + grid_gap;
        transform.position = new Vector3(_len * Row / 2 * (-1), _len * Col * (-1));
    }

    private void GenerateTile(uint stage)
    {
        // Stage 정보 받아와서 Row, Col, Cnt 추가 설정 가능
        //
        //


        // Pooling에서 Block pop, Position 설정
        int cnt = m_matrix.cnt;
        for (int i = 0; i < cnt; ++i)
        {
            BlockScript.COORDINATE coord = new BlockScript.COORDINATE { x = i % Col, y = i / Col };
            var worldPos = _grid.GetCellCenterWorld(new Vector3Int(coord.x, Col - coord.y));   // 행렬처럼 보이기 위해 월드포지션 수정
            var obj = m_BlockPool.objects.PopStack().gameObject;
            obj.SetActive(true);
            obj.GetComponent<BlockScript>().Coord = coord;
            obj.transform.position = worldPos;
            obj.transform.SetParent(transform);

            m_Board[coord.y, coord.x] = obj.GetComponent<BlockScript>();
        }

    }

    public void Board_DFS(BlockScript[,] board, bool[,] visited, int r, int c, ref int num, int sR, int sC)
    {
        if (visited[r, c] == true) return;
        // 방문 표시 및 시작 블록의 좌표 저장 -> 추후 connected / text 갱신에 활용
        visited[r, c] = true;
        board[r, c].Coord_start = new BlockScript.COORDINATE { y = sR, x = sC };

        for (int i = 0; i < 4; ++i)
        {
            int nr = r + dr[i];
            int nc = c + dc[i];

            if (nr < 0 || nc < 0 || nr >= Row || nc >= Col) continue;
            if (visited[nr, nc] == true || board[nr, nc].Color != board[r, c].Color) continue;

            ++num;
            Board_DFS(board, visited, nr, nc, ref num, sR, sC);
        }
        return;
    }

    public void UpdateBoard()
    {
        Debug.Log("UpdateBoard");

        bool[,] visited = new bool[Row, Col];
        Array.Clear(visited, 0, m_matrix.cnt);

        for (int r = 0; r < Row; ++r)
        {
            for (int c = 0; c < Col; ++c)
            {
                int num;
                if (visited[r, c] == true)
                {
                    num = m_Board[m_Board[r, c].Coord_start.y, m_Board[r, c].Coord_start.x].Connected;
                    m_Board[r, c].Connected = num;
                    m_Board[r, c].gameObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = num.ToString();
                    continue;
                }
                num = 1;
                Board_DFS(m_Board, visited, r, c, ref num, r, c);
                m_Board[r, c].Connected = num;
                m_Board[r, c].gameObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = num.ToString();
            }

        }

    }

    public void Explode_DFS(bool[,] visited, int r, int c)
    {
        Debug.Log(r + " " + c + " explode");

        if (visited[r, c] == true) return;
        visited[r, c] = true;
        Destroy(m_Board[r, c].gameObject);

        for (int i=0; i<4; ++i)
        {
            int nr = r + dr[i];
            int nc = c + dc[i];

            if (nr < 0 || nc < 0 || nr >= Row || nc >= Col) continue;
            if (visited[nr, nc] == true || m_Board[nr, nc].Color != m_Board[r, c].Color) continue;

            Explode_DFS(visited, nr, nc);
        }


    }

    public void ExplodeBlock(int r, int c)
    {
        Debug.Log(r + " " + c + " explode");

        bool[,] visited = new bool[Row, Col];
        Array.Clear(visited, 0, m_matrix.cnt);

        Explode_DFS(visited, r, c);




    }



}
