using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPoolScript : MonoBehaviour
{
    [SerializeField] private GameObject BoardManager;
    [SerializeField] private GameObject _block;
    [SerializeField] ObjectPool<BlockScript> m_BlockPool;

    // Property
    public ObjectPool<BlockScript> BlockPool {  get { return m_BlockPool; } }
    
    // Method
    void Start()
    {
        BlockPooling();
    }

    void BlockPooling()
    {
        int cnt = (int)BlockScript.BLOCK_COLOR.END;
        m_BlockPool = new ObjectPool<BlockScript>(200, (int n) =>
        {
            var obj = Instantiate(_block, transform);
            obj.SetActive(false);
           
            var block = obj.GetComponent<BlockScript>();
            block.Color = (BlockScript.BLOCK_COLOR)UnityEngine.Random.Range(1, cnt);
            block.Direction = (BlockScript.BLOCK_DIRECTION)UnityEngine.Random.Range(0, (int)BlockScript.BLOCK_DIRECTION.END);

            switch(block.Color)
            {
                case BlockScript.BLOCK_COLOR.RED:
                    obj.GetComponent<SpriteRenderer>().color = Color.red; break;
                case BlockScript.BLOCK_COLOR.GREEN:
                    obj.GetComponent<SpriteRenderer>().color = Color.green; break;
                case BlockScript.BLOCK_COLOR.BLUE:
                    obj.GetComponent<SpriteRenderer>().color = Color.blue; break;
                case BlockScript.BLOCK_COLOR.YELLOW:
                    obj.GetComponent<SpriteRenderer>().color = Color.yellow; break;
            }
            block.PoolManager = this.gameObject;

            obj.name = "Block_" + n;
            return block;
        });
    }

}
