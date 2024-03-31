using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveDoor : MonoBehaviour
{
    public float speed;
    private Vector3 initPos;
    private Vector3 targetPos;

    // Start is called before the first frame update
    void Start()
    {
        initPos = gameObject.transform.localPosition;
        // float moverange = initPos.z - 9f;
        //targetPos = new Vector3(initPos.x, initPos.y, initPos.z - 10f);
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.localPosition = new Vector3(initPos.x, initPos.y, -10f + Mathf.PingPong(Time.time * speed, 10));
        //if(gameObject.transform.localPosition.z - targetPos.z > 9f)
        //{
        //    gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, targetPos, 0.01f);
        //}
        //else
        //{
        //    gameObject.transform.position = Vector3.Lerp(initPos, gameObject.transform.position, 0.01f);
        //}
        //Debug.Log(Time.time);
    }
}
