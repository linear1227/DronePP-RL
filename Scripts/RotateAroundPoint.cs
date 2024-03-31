using UnityEngine;

public class RotateAroundPoint : MonoBehaviour
{
    public Transform objectToRotate; // 指定需要旋转的物体
    private Vector3 point; // 指定旋转的中心点
    public Vector3 plus;
    public float speed; // 旋转速度

    private void Awake()
    {
        point = objectToRotate.transform.position;
        point += plus;
    }

    void Update()
    {
        // 物体围绕point点按speed指定的速度旋转
        objectToRotate.RotateAround(point, Vector3.up, speed * Time.deltaTime);
    }
}