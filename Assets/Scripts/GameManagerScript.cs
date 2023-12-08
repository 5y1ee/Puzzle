using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerScript : MonoBehaviour
{
    public GameObject Start_btn, Board_mng;
    private uint stageNum;

    private void Start()
    {
        stageNum = 1;
    }

    public void StartGame()
    {
        Start_btn.SetActive(false);
        Board_mng.GetComponent<BoardManagerScript>().RunStage(stageNum++);
    }


}
