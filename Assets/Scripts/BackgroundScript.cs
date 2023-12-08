using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundScript : MonoBehaviour
{
    [SerializeField]
    private GameObject Wave_parent, Cloud_parent;
    [SerializeField]
    List<GameObject> waves = new List<GameObject>();
    [SerializeField]
    List<GameObject> clouds = new List<GameObject>();
    List<float> cloud_speed = new List<float>();
    List<float> wave_speed = new List<float>();

    int Wave_cnt, Cloud_cnt;
    float viewWidth, Speed, curTime = 0.0f;

    Transform Wave_pos, Cloud_pos;

    void Awake()
    {
        viewWidth = Camera.main.orthographicSize * 2.2f;

        Wave_pos = Wave_parent.transform;
        Cloud_pos = Cloud_parent.transform;

        Wave_cnt = Wave_parent.transform.childCount;
        Cloud_cnt = Cloud_parent.transform.childCount;
        
        for (int i=0; i<Wave_cnt; ++i)
        {
            waves.Add(Wave_parent.transform.GetChild(i).gameObject);
            wave_speed.Add(1.0f);
        }

        for (int i = 0; i<Cloud_cnt; ++i)
        {
            clouds.Add(Cloud_parent.transform.GetChild(i).gameObject);
            cloud_speed.Add(0.8f + i * 0.1f);
        }

    }

    private void Start()
    {
        
    }

    void VerticalMove(GameObject obj)
    {
        var i = Mathf.Cos(curTime);
        Transform curPos = obj.transform;
        obj.transform.position = curPos.position + (Vector3.up * i * Time.deltaTime);
    }
    void HorizontalMove(GameObject obj, List<float> speed, int idx)
    {
        if (!obj.activeSelf) { return; }

        Transform curPos = obj.transform;
        obj.transform.position = curPos.position + (Vector3.right * speed[idx] * Time.deltaTime);

        if (obj.transform.position.x > viewWidth + obj.transform.localScale.x)
        {
            obj.transform.position = new Vector3(viewWidth * (-1), obj.transform.position.y, obj.transform.position.z);
            speed[idx] = Speed;
        }

    }

    void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > Mathf.PI) curTime = 0;
        Speed = 0.5f + Mathf.Sin(curTime);

        VerticalMove(Wave_parent);
        HorizontalMove(Wave_parent, wave_speed, 0);
        for (int i = 0; i < Cloud_cnt; ++i)
            HorizontalMove(clouds[i], cloud_speed, i);
    }
}
