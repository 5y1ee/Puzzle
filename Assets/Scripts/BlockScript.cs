using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    public enum BLOCK_COLOR
    {
        RED, GREEN, BLUE, YELLOW, END
    };
    [System.Serializable]
    public struct COORDINATE
    {
        public int x, y;
    }

    // 
    [SerializeField] BLOCK_COLOR m_color;
    [SerializeField] COORDINATE m_coord;
    
    // Property
    public BLOCK_COLOR Color {  get { return m_color; } set {  m_color = value; } }
    public COORDINATE Coord {  get { return m_coord; } set { m_coord = value; } }


    void Start()
    {
        //Debug.Log(gameObject.name);
        //gameObject.SetActive(false);
    }






}
