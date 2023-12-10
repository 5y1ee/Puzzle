using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct COORDINATE
{
    public int row, col;
}

public class BlockScript : MonoBehaviour
{
    public enum BLOCK_COLOR
    {
        RED = 1, GREEN, BLUE, YELLOW, END
    };
    public enum BLOCK_DIRECTION
    {
        EAST, WEST, SOUTH, NORTH, END
    };


    // 
    [SerializeField] private GameObject m_BoardManager;
    [SerializeField] private GameObject m_PoolManager;
    [SerializeField] private BLOCK_COLOR m_color;
    [SerializeField] private BLOCK_DIRECTION m_direction;
    [SerializeField] private COORDINATE m_coord;
    [SerializeField] private COORDINATE m_start;
    [SerializeField] private int m_Connected;
    
    // Property
    public BLOCK_COLOR Color { get { return m_color; } set {  m_color = value; } }
    public BLOCK_DIRECTION Direction { get { return m_direction; } set { m_direction = value; } }
    public COORDINATE Coord { get { return m_coord; } set { m_coord = value; } }
    public COORDINATE Coord_start { get { return m_start; } set { m_start = value; } }
    public int Connected { get { return m_Connected; } set { m_Connected = value; } }
    public GameObject PoolManager { get { return m_PoolManager; } set { m_PoolManager = value; } }

    // Method
    void Start()
    {
        //Button button = GetComponent<Button>();
        //if (button != null)
        //    button.onClick.AddListener(OnButtonClick);
    }

    //public void OnButtonClick()
    //{
    //    Debug.Log(this.name);
    //}

    private void OnMouseDown()
    {
        Debug.Log(this.name + " is Clicked " + m_coord.row + "," + m_coord.col);

        if (m_Connected < 3)
            return;

        m_BoardManager = transform.parent.gameObject;
        m_BoardManager.GetComponent<BoardManagerScript>().ExplodeBlock(m_coord.row, m_coord.col);
    }

    public void InitBlock()
    {
        gameObject.SetActive(false);
        m_Connected = 0;
        m_coord = m_start = new COORDINATE();
        m_color = (BLOCK_COLOR)UnityEngine.Random.Range(1, (int)BLOCK_COLOR.END);
        m_direction = (BLOCK_DIRECTION)UnityEngine.Random.Range(0, (int)BLOCK_DIRECTION.END);

        switch(m_color)
        {
            case BLOCK_COLOR.RED:
                gameObject.GetComponent<SpriteRenderer>().color = UnityEngine.Color.red; break;
            case BLOCK_COLOR.GREEN:
                gameObject.GetComponent<SpriteRenderer>().color = UnityEngine.Color.green; break;
            case BLOCK_COLOR.BLUE:
                gameObject.GetComponent<SpriteRenderer>().color = UnityEngine.Color.blue; break;
            case BLOCK_COLOR.YELLOW:
                gameObject.GetComponent<SpriteRenderer>().color = UnityEngine.Color.yellow; break;
        }

        transform.SetParent(m_PoolManager.transform);
    }

    public void MoveBlock(Vector3 dest)
    {
        StartCoroutine(Co_MoveBlock(dest));
    }

    private IEnumerator Co_MoveBlock(Vector3 destPos)
    {
        float elapsedTime = 0.0f, moveTime = 1.0f;

        Vector3 curPos = transform.position;
        //Vector3 destPos = dest.position;

        while (elapsedTime < moveTime)
        {
            transform.position = Vector3.Lerp(curPos, destPos, elapsedTime / moveTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = destPos;

    }

}
