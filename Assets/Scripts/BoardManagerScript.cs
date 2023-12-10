using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    //public struct BLOCK_INFO
    //{
    //    public int row, col;
    //}

    // 
    //[SerializeField] private BlockScript[,] m_Board;    // 블록 배치에 대한 2차원 배열
    [SerializeField] private GameObject[,] m_Board;    // 블록 배치에 대한 2차원 배열
    [SerializeField] private Vector3[,] m_Position;    // 게임 보드 포지션 정보
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
    private List<COORDINATE> Coord_list = new List<COORDINATE>();

    // Property
    public GameObject[,] BoardOBJ { get { return m_Board; } }
    public Vector3[,] PositionOBJ { get { return m_Position; } }
    public MATRIX Matrix {  get { return m_matrix; } set {  m_matrix = value; } }

    // Method
    private void Awake()
    {
        // 초기 stage 설정
        Row = m_matrix.row = 10;
        Col = m_matrix.col = 10;
        m_matrix.cnt = Row * Col;
        SetPosition();

        m_Board = new GameObject[Row, Col];
        m_Position = new Vector3[Row, Col];

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
            COORDINATE coord = new COORDINATE { col = i % Col, row = i / Col };
            var worldPos = _grid.GetCellCenterWorld(new Vector3Int(coord.col, Col - coord.row));   // 행렬처럼 보이기 위해 월드포지션 수정
            var obj = m_BlockPool.objects.PopStack().gameObject;
            obj.SetActive(true);
            obj.GetComponent<BlockScript>().Coord = coord;
            obj.transform.position = worldPos;
            obj.transform.SetParent(transform);

            m_Board[coord.row, coord.col] = obj.GetComponent<BlockScript>().gameObject;
            m_Position[coord.row, coord.col] = worldPos;
        }

    }

    public void Board_DFS(GameObject[,] board, bool[,] visited, int r, int c, ref int num, int sR, int sC)
    {
        if (visited[r, c] == true) return;
        // 방문 표시 및 시작 블록의 좌표 저장 -> 추후 connected / text 갱신에 활용
        visited[r, c] = true;
        BlockScript blockScript = board[r, c].GetComponent<BlockScript>();
        blockScript.Coord_start = new COORDINATE { row = sR, col = sC };

        for (int i = 0; i < 4; ++i)
        {
            int nr = r + dr[i];
            int nc = c + dc[i];

            if (nr < 0 || nc < 0 || nr >= Row || nc >= Col) continue;
            if (board[nr, nc] == null) continue;
            if (visited[nr, nc] == true || board[nr, nc].GetComponent<BlockScript>().Color != blockScript.Color) continue;

            ++num;
            Board_DFS(board, visited, nr, nc, ref num, sR, sC);
        }
        return;
    }

    public void UpdateBoard()
    {
        Debug.Log("UpdateBoard");

        int num;
        string str;
        bool[,] visited = new bool[Row, Col];
        Array.Clear(visited, 0, m_matrix.cnt);

        for (int r = 0; r < Row; ++r)
        {
            for (int c = 0; c < Col; ++c)
            {
                if (m_Board[r, c] == null) continue;
                BlockScript blockScript = m_Board[r, c].GetComponent<BlockScript>();
                if (visited[r, c] == true)
                {
                    num = m_Board[blockScript.Coord_start.row, blockScript.Coord_start.col].GetComponent<BlockScript>().Connected;
                    blockScript.Connected = num;
                    str = blockScript.Direction.ToString().Substring(0, 1) + num.ToString();
                    m_Board[r, c].gameObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = str;
                    continue;
                }
                num = 1;
                Board_DFS(m_Board, visited, r, c, ref num, r, c);
                blockScript.Connected = num;
                str = blockScript.Direction.ToString().Substring(0, 1) + num.ToString();
                m_Board[r, c].gameObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = str;
            }

        }

    }

    public void Explode_DFS(bool[,] visited, int r, int c)
    {
        Debug.Log(r + " " + c + " explode");

        if (visited[r, c] == true) return;
        // 방문 처리
        visited[r, c] = true;
        Coord_list.Add(new COORDINATE { row = r, col = c });

        for (int i = 0; i < 4; ++i)
        {
            int nr = r + dr[i];
            int nc = c + dc[i];

            if (nr < 0 || nc < 0 || nr >= Row || nc >= Col) continue;
            if (m_Board[nr, nc] == null) continue;
            if (visited[nr, nc] == true || m_Board[nr, nc].GetComponent<BlockScript>().Color != m_Board[r, c].GetComponent<BlockScript>().Color) continue;

            Explode_DFS(visited, nr, nc);
        }

        // 폭발 로직
        m_Board[r, c].GetComponent<BlockScript>().InitBlock();
        m_BlockPool.objects.PushStack(m_Board[r, c].GetComponent<BlockScript>());
        m_Board[r, c] = null;

    }

    public void ExplodeBlock(int r, int c)
    {
        Debug.Log(r + "," + c + " explode");

        var dir = m_Board[r, c].GetComponent<BlockScript>().Direction;

        Coord_list.Clear();
        bool[,] visited = new bool[Row, Col];
        Array.Clear(visited, 0, m_matrix.cnt);
        Explode_DFS(visited, r, c);
        FillBlock(dir);
    }

    public void FillBlock(BlockScript.BLOCK_DIRECTION dir)
    {
        // Direction of Fill
        // North : ASC Row
        // South : DEC Row
        // West : ASC Col
        // East : DEC Col

        SortedSet<int> Rows = new SortedSet<int>();
        SortedSet<int> Cols = new SortedSet<int>();
        foreach (var i in Coord_list)
        {
            Rows.Add(i.row); Cols.Add(i.col);
        }

        Queue<COORDINATE> q = new Queue<COORDINATE>();

        switch (dir)
        {
            case BlockScript.BLOCK_DIRECTION.NORTH:
                // Cols를 기준으로 null 개수 찾기 -> 추가할 블록의 수
                // 아래에서 부터 순회하여 null을 queue에 넣고 queue size가 0이 아닐 때 조우하는 블록은 null을 만난 곳을 dest로 이동시키기
                for (int i = 0; i < Cols.Count; ++i)
                {
                    int c = Cols.ElementAt(i);
                    q.Clear();
                    for (int r = m_matrix.row - 1; r >= 0; --r)
                    {
                        if (m_Board[r, c] == null)
                        {
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                        else if (q.Count != 0)
                        {
                            COORDINATE dest = q.Dequeue();
                            Vector3 pos = m_Position[dest.row, dest.col];
                            m_Board[r, c].GetComponent<BlockScript>().MoveBlock(pos);
                            m_Board[r, c].GetComponent<BlockScript>().Coord = new COORDINATE { row = dest.row, col = dest.col };
                            m_Board[dest.row, dest.col] = m_Board[r, c];
                            m_Board[r, c] = null;
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                    }

                    int k = 1;
                    while (q.Count > 0)
                    {
                        Vector3 worldPos = _grid.GetCellCenterWorld(new Vector3Int(c, m_matrix.row + k));
                        GameObject obj = m_BlockPool.objects.PopStack().gameObject;
                        obj.SetActive(true);
                        obj.transform.position = worldPos;
                        obj.transform.SetParent(transform);
                        COORDINATE dest = q.Dequeue();
                        Vector3 pos = m_Position[dest.row, dest.col];
                        //obj.GetComponent<BlockScript>().Coord = new COORDINATE { row = k - 1, col = c };
                        obj.GetComponent<BlockScript>().Coord = dest;
                        obj.GetComponent<BlockScript>().MoveBlock(pos);
                        m_Board[dest.row, dest.col] = obj;
                        ++k;
                    }
                }
                break;
            case BlockScript.BLOCK_DIRECTION.SOUTH:
                for (int i = 0; i < Cols.Count; ++i)
                {
                    int c = Cols.ElementAt(i);
                    q.Clear();
                    //for (int r = m_matrix.row - 1; r >= 0; --r)
                    for (int r = 0; r < m_matrix.row; ++r)
                    {
                        if (m_Board[r, c] == null)
                        {
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                        else if (q.Count != 0)
                        {
                            COORDINATE dest = q.Dequeue();
                            Vector3 pos = m_Position[dest.row, dest.col];
                            m_Board[r, c].GetComponent<BlockScript>().MoveBlock(pos);
                            m_Board[r, c].GetComponent<BlockScript>().Coord = new COORDINATE { row = dest.row, col = dest.col };
                            m_Board[dest.row, dest.col] = m_Board[r, c];
                            m_Board[r, c] = null;
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                    }

                    int k = 1;
                    while (q.Count > 0)
                    {
                        Vector3 worldPos = _grid.GetCellCenterWorld(new Vector3Int(c, -k));
                        GameObject obj = m_BlockPool.objects.PopStack().gameObject;
                        obj.SetActive(true);
                        obj.transform.position = worldPos;
                        obj.transform.SetParent(transform);
                        COORDINATE dest = q.Dequeue();
                        Vector3 pos = m_Position[dest.row, dest.col];
                        //obj.GetComponent<BlockScript>().Coord = new COORDINATE { row = -k + 1, col = c };
                        obj.GetComponent<BlockScript>().Coord = dest;
                        obj.GetComponent<BlockScript>().MoveBlock(pos);
                        m_Board[dest.row, dest.col] = obj;
                        ++k;
                    }
                }
                break;

            case BlockScript.BLOCK_DIRECTION.WEST:
                // 열이 증가하는 방향으로 이동
                for (int i = 0; i < Rows.Count; ++i)
                {
                    int r = Rows.ElementAt(i);
                    q.Clear();
                    //for (int c = 0; c < m_matrix.col; ++c)
                    for (int c = m_matrix.col - 1; c >= 0; --c)
                    {
                        if (m_Board[r, c] == null)
                        {
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                        else if (q.Count != 0)
                        {
                            COORDINATE dest = q.Dequeue();
                            Vector3 pos = m_Position[dest.row, dest.col];
                            m_Board[r, c].GetComponent<BlockScript>().MoveBlock(pos);
                            m_Board[r, c].GetComponent<BlockScript>().Coord = new COORDINATE { row = dest.row, col = dest.col };
                            m_Board[dest.row, dest.col] = m_Board[r, c];
                            m_Board[r, c] = null;
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                    }

                    int k = 1;
                    while (q.Count > 0)
                    {
                        Debug.Log(k + "," + r);
                        Vector3 worldPos = _grid.GetCellCenterWorld(new Vector3Int(-k, m_matrix.row - r));
                        GameObject obj = m_BlockPool.objects.PopStack().gameObject;
                        obj.SetActive(true);
                        obj.transform.position = worldPos;
                        obj.transform.SetParent(transform);
                        COORDINATE dest = q.Dequeue();
                        Vector3 pos = m_Position[dest.row, dest.col];
                        //obj.GetComponent<BlockScript>().Coord = new COORDINATE { row = r, col = -k };
                        obj.GetComponent<BlockScript>().Coord = dest;
                        obj.GetComponent<BlockScript>().MoveBlock(pos);
                        m_Board[dest.row, dest.col] = obj;
                        ++k;
                    }
                }

                break;
            case BlockScript.BLOCK_DIRECTION.EAST:
                for (int i = 0; i < Rows.Count; ++i)
                {
                    int r = Rows.ElementAt(i);
                    q.Clear();
                    for (int c = 0; c < m_matrix.col; ++c)
                    //for (int c = m_matrix.col - 1; c >= 0; --c)
                    {
                        if (m_Board[r, c] == null)
                        {
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                        else if (q.Count != 0)
                        {
                            COORDINATE dest = q.Dequeue();
                            Vector3 pos = m_Position[dest.row, dest.col];
                            m_Board[r, c].GetComponent<BlockScript>().MoveBlock(pos);
                            m_Board[r, c].GetComponent<BlockScript>().Coord = new COORDINATE { row = dest.row, col = dest.col };
                            m_Board[dest.row, dest.col] = m_Board[r, c];
                            m_Board[r, c] = null;
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                    }

                    int k = 0;
                    while (q.Count > 0)
                    {
                        Debug.Log(k + "," + r);
                        Vector3 worldPos = _grid.GetCellCenterWorld(new Vector3Int(m_matrix.col + k, m_matrix.row - r));
                        GameObject obj = m_BlockPool.objects.PopStack().gameObject;
                        obj.SetActive(true);
                        obj.transform.position = worldPos;
                        obj.transform.SetParent(transform);
                        COORDINATE dest = q.Dequeue();
                        Vector3 pos = m_Position[dest.row, dest.col];
                        //obj.GetComponent<BlockScript>().Coord = new COORDINATE { row = r, col = -k };
                        obj.GetComponent<BlockScript>().Coord = dest;
                        obj.GetComponent<BlockScript>().MoveBlock(pos);
                        m_Board[dest.row, dest.col] = obj;
                        ++k;
                    }
                }

                break;
        }

        UpdateBoard();

    }





}
