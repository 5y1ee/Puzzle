using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockScript : MonoBehaviour
{
    public enum BLOCK_COLOR
    {
        RED = 1, GREEN, BLUE, YELLOW, END
    };
    [System.Serializable]
    public struct COORDINATE
    {
        public int x, y;
    }

    // 
    [SerializeField] private GameObject m_BoardManager;
    [SerializeField] private BLOCK_COLOR m_color;
    [SerializeField] private COORDINATE m_coord;
    [SerializeField] private COORDINATE m_start;
    [SerializeField] private int m_Connected;
    
    // Property
    public BLOCK_COLOR Color {  get { return m_color; } set {  m_color = value; } }
    public COORDINATE Coord {  get { return m_coord; } set { m_coord = value; } }
    public COORDINATE Coord_start { get { return m_start; } set { m_start = value; } }
    public int Connected { get { return m_Connected; } set { m_Connected = value; } }

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
        Debug.Log(this.name + "is Clicked");

        if (m_Connected < 3)
            return;

        m_BoardManager = transform.parent.gameObject;
        m_BoardManager.GetComponent<BoardManagerScript>().ExplodeBlock(m_coord.y, m_coord.x);
    }


}
