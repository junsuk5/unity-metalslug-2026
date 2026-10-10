using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    public Transform target;
    public float followSpeed = 20;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 start = transform.position;
        Vector3 pos = target.position;

        if (start.x < -0.078f) start.x = -0.078f;
        if (start.x > 66.7f) start.x = 66.7f;

        Vector3 end = Vector3.MoveTowards(start, pos, followSpeed * Time.deltaTime);

        end.y = start.y;
        end.z = start.z;
        transform.position = end;

//        GetComponent<tk2dCamera>().ZoomFactor -= Time.deltaTime * 0.5f;
//        if (GetComponent<tk2dCamera>().ZoomFactor < 1.0f) GetComponent<tk2dCamera>().ZoomFactor = 1.0f;   
    }
}
