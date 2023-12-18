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

    // 
    //[SerializeField] private BlockScript[,] m_Board;    // 블록 배치에 대한 2차원 배열
    [SerializeField] private GameObject[,] m_Board;    // 블록 배치에 대한 2차원 배열
    [SerializeField] private Vector3[,] m_Position;    // 게임 보드 포지션 정보
    [SerializeField] private MATRIX m_matrix;   // 게임 보드에 대한 정보
    [SerializeField] private Grid _grid;        // 그리드로 게임 내 포지션 좌표 설정
    [SerializeField] private Transform m_Pool;  // 블록 풀링 트랜스폼
    [SerializeField] private ObjectPool<BlockScript> m_BlockPool;   // 블록 풀링 객체
    [SerializeField] private List<COORDINATE> Coord_list;

    // 내부 변수
    private float grid_len, grid_gap;   
    private int Row, Col;
    // 탐색 순서 left -> down -> right -> up
    private int[] dr = { 0, 1, 0, -1 };
    private int[] dc = { -1, 0, 1, 0 };

    // Property
    public GameObject[,] BoardOBJ { get { return m_Board; } }
    public Vector3[,] PositionOBJ { get { return m_Position; } }
    public List<COORDINATE> COORD_LIST { get { return Coord_list; } set { Coord_list = value; } }
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
        Coord_list = new List<COORDINATE>();

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

                //if (m_Position[r, c] != m_Board[r, c].transform.position)
                if (blockScript.Coord.row != r || blockScript.Coord.col != c)
                {
                    Debug.Log("##" + m_Board[r, c].name + " " + r + "," + c + " block is not on right position");
                }

                if (visited[r, c] == true)
                {
                    num = m_Board[blockScript.Coord_start.row, blockScript.Coord_start.col].GetComponent<BlockScript>().Connected;
                    blockScript.Connected = num;
                    str = blockScript.Direction.ToString().Substring(0, 1) + num.ToString();
                    if (blockScript.Type != BlockScript.BLOCK_TYPE.NORMAL)
                        str += blockScript.Type.ToString().Substring(0, 1);
                    m_Board[r, c].gameObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = str;
                    continue;
                }
                num = 1;
                Board_DFS(m_Board, visited, r, c, ref num, r, c);
                blockScript.Connected = num;
                str = blockScript.Direction.ToString().Substring(0, 1) + num.ToString();
                if (blockScript.Type != BlockScript.BLOCK_TYPE.NORMAL)
                    str += blockScript.Type.ToString().Substring(0, 1);
                m_Board[r, c].gameObject.transform.GetChild(0).GetComponent<TextMeshPro>().text = str;
            }

        }

    }

    public void Explode_DFS(bool[,] visited, int r, int c)
    {
        Explode_DFS(visited, new COORDINATE { row = r, col = c });
    }
    public void Explode_DFS(bool[,] visited, COORDINATE coord)
    {
        Debug.Log(coord.row + "," + coord.col + " explode");

        int r = coord.row, c = coord.col;
        if (visited[r, c] == true) return;
        // 방문 처리
        visited[r, c] = true;
        //Coord_list.Add(new COORDINATE { row = r, col = c });
        Coord_list.Add(coord);

        for (int i = 0; i < 4; ++i)
        {
            int nr = r + dr[i];
            int nc = c + dc[i];

            if (nr < 0 || nc < 0 || nr >= Row || nc >= Col) continue;
            if (m_Board[nr, nc] == null || m_Board[nr, nc].GetComponent<BlockScript>().Type != BlockScript.BLOCK_TYPE.NORMAL) continue;
            if (visited[nr, nc] == true || m_Board[nr, nc].GetComponent<BlockScript>().Color != m_Board[r, c].GetComponent<BlockScript>().Color) continue;

            Explode_DFS(visited, nr, nc);
        }

        // 폭발 로직
        if (m_Board[r, c].GetComponent<BlockScript>().Type == BlockScript.BLOCK_TYPE.NORMAL)
        {
            m_Board[r, c].GetComponent<BlockScript>().InitBlock();
            m_BlockPool.objects.PushStack(m_Board[r, c].GetComponent<BlockScript>());
            m_Board[r, c] = null;
        }
        else
        {
            if (m_Board[r, c].GetComponent<BlockScript>().Type != BlockScript.BLOCK_TYPE.FLOWER)
            {
                m_Board[r, c].GetComponent<BlockScript>().Color = BlockScript.BLOCK_COLOR.NONE;
                m_Board[r, c].GetComponent<SpriteRenderer>().color = UnityEngine.Color.gray;
            }
            else
            {
                m_Board[r, c].GetComponent<SpriteRenderer>().color += new Color(0f, 0f, 0f, -0.5f);
            }
        }

    }

    public void ExplodeBlock(COORDINATE coord)
    {
        Debug.Log(coord.row + "," + coord.col + " explode");

        //Coord_list.Clear();
        bool[,] visited = new bool[Row, Col];
        Array.Clear(visited, 0, m_matrix.cnt);

        int r = coord.row, c = coord.col;
        int cnt = m_Board[r, c].GetComponent<BlockScript>().Connected;
        var dir = m_Board[r, c].GetComponent<BlockScript>().Direction;

        if (cnt >= 9)
            m_Board[r, c].GetComponent<BlockScript>().Type = BlockScript.BLOCK_TYPE.FLOWER;
        else if (cnt >= 7)
            m_Board[r, c].GetComponent<BlockScript>().Type = BlockScript.BLOCK_TYPE.BOMB;
        else if (cnt >= 5)
            m_Board[r, c].GetComponent<BlockScript>().Type = BlockScript.BLOCK_TYPE.VERTICAL;

        Explode_DFS(visited, coord);

        if (m_Board[r, c] != null && m_Board[r, c].GetComponent<BlockScript>().Type == BlockScript.BLOCK_TYPE.VERTICAL)
        {
            int r_max = 0, r_min = m_matrix.row, c_max = 0, c_min = m_matrix.col;
            foreach (var i in Coord_list)
            {
                if (i.row > r_max) r_max = i.row;
                if (i.row < r_min) r_min = i.row;
                if (i.col > c_max) c_max = i.col;
                if (i.col < c_min) c_min = i.col;
            }
            if (r_max - r_min < c_max - c_min)
                m_Board[r, c].GetComponent<BlockScript>().Type = BlockScript.BLOCK_TYPE.HORIZONTAL;
            // 높이와 너비가 같은 5개, 6개 짜리 블록의 모음은 다양하므로 이 경우엔 랜덤으로 수평/수직을 정한다.
            else if (r_max - r_min ==  c_max - c_min)
            {
                int rnd = UnityEngine.Random.Range(0, 2);
                Debug.Log(rnd);
                if (rnd / 2 == 0)
                    m_Board[r, c].GetComponent<BlockScript>().Type = BlockScript.BLOCK_TYPE.HORIZONTAL;                
            }
        }
        FillBlock(dir);
    }

    public void ExplodeSpecial(int r, int c)
    {
        ExplodeSpecial(new COORDINATE { row = r, col = c });
    }
    public void ExplodeSpecial(COORDINATE coord)
    {
        int r = coord.row, c = coord.col;
        var dir = m_Board[r, c].GetComponent<BlockScript>().Direction;
        var color = m_Board[r, c].GetComponent<BlockScript>().Color;
        //Queue<COORDINATE> q_coord = new Queue<COORDINATE>();
        Queue <GameObject> q_block = new Queue<GameObject>();

        switch (m_Board[r, c].GetComponent<BlockScript>().Type)
        {
            case BlockScript.BLOCK_TYPE.HORIZONTAL:
                for (int nc = 0; nc < m_matrix.col; ++nc)
                {
                    // 폭발 로직
                    if (m_Board[r, nc] == null) continue;
                    if (m_Board[r, nc].GetComponent<BlockScript>().Type == BlockScript.BLOCK_TYPE.NORMAL ||
                        nc == c)
                    {
                        Coord_list.Add(new COORDINATE { row = r, col = nc });
                        m_Board[r, nc].GetComponent<BlockScript>().InitBlock();
                        m_BlockPool.objects.PushStack(m_Board[r, nc].GetComponent<BlockScript>());
                        m_Board[r, nc] = null;
                    }
                    else
                    {
                        //ExplodeSpecial(r, nc);
                        //q_coord.Enqueue(new COORDINATE { row = r, col = nc });
                        q_block.Enqueue(m_Board[r, nc]);
                    }
                }
                FillBlock(dir);

                Coord_list.Clear();
                while (q_block.Count > 0)
                {
                    var tmp = q_block.Dequeue();
                    ExplodeSpecial(tmp.GetComponent<BlockScript>().Coord.row, tmp.GetComponent<BlockScript>().Coord.col);
                }
                break;

            case BlockScript.BLOCK_TYPE.VERTICAL:
                for (int nr = 0; nr < m_matrix.row; ++nr)
                {
                    // 폭발 로직
                    if (m_Board[nr, c] == null) continue;
                    if (m_Board[nr, c].GetComponent<BlockScript>().Type == BlockScript.BLOCK_TYPE.NORMAL ||
                        nr == r)
                    {
                        Coord_list.Add(new COORDINATE { row = nr, col = c });
                        m_Board[nr, c].GetComponent<BlockScript>().InitBlock();
                        m_BlockPool.objects.PushStack(m_Board[nr, c].GetComponent<BlockScript>());
                        m_Board[nr, c] = null;
                    }
                    else
                    {
                        //ExplodeSpecial(nr, c);
                        //q_coord.Enqueue(new COORDINATE { row = nr, col = c });
                        q_block.Enqueue(m_Board[nr, c]);
                    }
                }
                FillBlock(dir);

                Coord_list.Clear();
                //while (q_coord.Count > 0)
                while(q_block.Count > 0)
                {
                    var tmp = q_block.Dequeue();
                    ExplodeSpecial(tmp.GetComponent<BlockScript>().Coord.row, tmp.GetComponent<BlockScript>().Coord.col);
                }
                break;

            case BlockScript.BLOCK_TYPE.BOMB:
                for (int i = -1; i < 2; ++i)
                {
                    for (int k = -1; k < 2; ++k)
                    {
                        int nr = r + i, nc = c + k;
                        if (nr < 0 || nc < 0 || nr >= m_matrix.row || nc >= m_matrix.col) continue;
                        if (m_Board[nr, nc] == null) continue;
                        if (m_Board[nr, nc].GetComponent<BlockScript>().Type == BlockScript.BLOCK_TYPE.NORMAL ||
                            nr == r)
                        {
                            Coord_list.Add(new COORDINATE { row = nr, col = nc });
                            m_Board[nr, nc].GetComponent<BlockScript>().InitBlock();
                            m_BlockPool.objects.PushStack(m_Board[nr, nc].GetComponent<BlockScript>());
                            m_Board[nr, nc] = null;
                        }
                        else
                        {
                            q_block.Enqueue(m_Board[nr, nc]);
                        }

                    }
                }
                FillBlock(dir);

                Coord_list.Clear();
                while (q_block.Count > 0)
                {
                    var tmp = q_block.Dequeue();
                    ExplodeSpecial(tmp.GetComponent<BlockScript>().Coord.row, tmp.GetComponent<BlockScript>().Coord.col);
                }
                break;

            case BlockScript.BLOCK_TYPE.FLOWER:
                for (int nr = 0; nr < m_matrix.row; ++nr)
                {
                    for (int nc = 0; nc < m_matrix.col; ++nc)
                    {
                        if (m_Board[nr, nc].GetComponent<BlockScript>().Color == color)
                        {
                            Coord_list.Add(new COORDINATE { row = nr, col = nc });
                            m_Board[nr, nc].GetComponent<BlockScript>().InitBlock();
                            m_BlockPool.objects.PushStack(m_Board[nr, nc].GetComponent<BlockScript>());
                            m_Board[nr, nc] = null;
                        }
                    }
                }
                FillBlock(dir);
                break;

        }

    }

    public void FillBlock(BlockScript.BLOCK_DIRECTION dir)
    {
        /*
        Direction of Fill
        North : ASC Row
        South : DEC Row
        West : ASC Col
        East : DEC Col
         */

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
                            // 이동할 블록을 큐에 추가
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                        //else if (m_Board[r, c].GetComponent<BlockScript>().Type != BlockScript.BLOCK_TYPE.NORMAL) continue;
                        else if (q.Count != 0)
                        {
                            COORDINATE dest = q.Dequeue();
                            Vector3 pos = m_Position[dest.row, dest.col];
                            m_Board[r, c].GetComponent<BlockScript>().MoveBlock(pos);
                            m_Board[r, c].GetComponent<BlockScript>().Coord = dest;
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
                    for (int r = 0; r < m_matrix.row; ++r)
                    {
                        if (m_Board[r, c] == null)
                        {
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                        //else if (m_Board[r, c].GetComponent<BlockScript>().Type != BlockScript.BLOCK_TYPE.NORMAL) continue;
                        else if (q.Count != 0)
                        {
                            COORDINATE dest = q.Dequeue();
                            Vector3 pos = m_Position[dest.row, dest.col];
                            m_Board[r, c].GetComponent<BlockScript>().MoveBlock(pos);
                            m_Board[r, c].GetComponent<BlockScript>().Coord = dest;
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
                    for (int c = m_matrix.col - 1; c >= 0; --c)
                    {
                        if (m_Board[r, c] == null)
                        {
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                        //else if (m_Board[r, c].GetComponent<BlockScript>().Type != BlockScript.BLOCK_TYPE.NORMAL) continue;
                        else if (q.Count != 0)
                        {
                            COORDINATE dest = q.Dequeue();
                            Vector3 pos = m_Position[dest.row, dest.col];
                            m_Board[r, c].GetComponent<BlockScript>().MoveBlock(pos);
                            m_Board[r, c].GetComponent<BlockScript>().Coord = dest;
                            m_Board[dest.row, dest.col] = m_Board[r, c];
                            m_Board[r, c] = null;
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                    }
                    int k = 1;
                    while (q.Count > 0)
                    {
                        Vector3 worldPos = _grid.GetCellCenterWorld(new Vector3Int(-k, m_matrix.row - r));
                        GameObject obj = m_BlockPool.objects.PopStack().gameObject;
                        obj.SetActive(true);
                        obj.transform.position = worldPos;
                        obj.transform.SetParent(transform);
                        COORDINATE dest = q.Dequeue();
                        Vector3 pos = m_Position[dest.row, dest.col];
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
                    {
                        if (m_Board[r, c] == null)
                        {
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                        //else if (m_Board[r, c].GetComponent<BlockScript>().Type != BlockScript.BLOCK_TYPE.NORMAL) continue;
                        else if (q.Count != 0)
                        {
                            COORDINATE dest = q.Dequeue();
                            Vector3 pos = m_Position[dest.row, dest.col];
                            m_Board[r, c].GetComponent<BlockScript>().MoveBlock(pos);
                            m_Board[r, c].GetComponent<BlockScript>().Coord = dest;
                            m_Board[dest.row, dest.col] = m_Board[r, c];
                            m_Board[r, c] = null;
                            q.Enqueue(new COORDINATE { row = r, col = c });
                        }
                    }
                    int k = 0;
                    while (q.Count > 0)
                    {
                        Vector3 worldPos = _grid.GetCellCenterWorld(new Vector3Int(m_matrix.col + k, m_matrix.row - r));
                        GameObject obj = m_BlockPool.objects.PopStack().gameObject;
                        obj.SetActive(true);
                        obj.transform.position = worldPos;
                        obj.transform.SetParent(transform);
                        COORDINATE dest = q.Dequeue();
                        Vector3 pos = m_Position[dest.row, dest.col];
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
