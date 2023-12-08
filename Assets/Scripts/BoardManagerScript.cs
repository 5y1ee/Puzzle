using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private MATRIX m_matrix;
    [SerializeField] public int[,] board;
    [SerializeField] private Grid _grid;
    [SerializeField] private Transform m_Pool;
    [SerializeField] private ObjectPool<BlockScript> m_BlockPool;
    
    private float grid_len, grid_gap;

    // Property
    public MATRIX Matrix {  get { return m_matrix; } set {  m_matrix = value; } }

    // Method
    private void Awake()
    {
        // 초기 stage 설정
        m_matrix.row = 10;
        m_matrix.col = 10;
        m_matrix.cnt = m_matrix.row * m_matrix.col;
        SetPosition();

        //board = new int[Row, Col];
    }
    private void Start()
    {

    }

    public void RunStage(uint stage)
    {
        m_BlockPool = m_Pool.GetComponent<BlockPoolScript>().BlockPool;
        GenerateTile(stage);
    }

    private void SetPosition()
    {
        grid_len = _grid.cellSize.x;
        grid_gap = _grid.cellGap.x;
        float _len = grid_len + grid_gap;
        transform.position = new Vector3(_len * m_matrix.row / 2 * (-1), _len * m_matrix.col * (-1));
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
            BlockScript.COORDINATE coord = new BlockScript.COORDINATE { x = i % m_matrix.col, y = i / m_matrix.col };
            var worldPos = _grid.GetCellCenterWorld(new Vector3Int(coord.x, coord.y));
            //Instantiate(_tile, worldPos, Quaternion.identity, transform);

            var obj = m_BlockPool.objects.PopStack().gameObject;
            obj.SetActive(true);
            obj.GetComponent<BlockScript>().Coord = coord;
            obj.transform.position = worldPos;
            obj.transform.SetParent(transform);

            Debug.Log(obj.name + obj.activeSelf);
        }


    }

}
