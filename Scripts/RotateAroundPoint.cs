using UnityEngine;

public class RotateAroundPoint : MonoBehaviour
{
    public Transform objectToRotate; // ָ����Ҫ��ת������
    private Vector3 point; // ָ����ת�����ĵ�
    public Vector3 plus;
    public float speed; // ��ת�ٶ�

    private void Awake()
    {
        point = objectToRotate.transform.position;
        point += plus;
    }

    void Update()
    {
        // ����Χ��point�㰴speedָ�����ٶ���ת
        objectToRotate.RotateAround(point, Vector3.up, speed * Time.deltaTime);
    }
}