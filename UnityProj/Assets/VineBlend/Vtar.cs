using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//�ɱ�����ץȡ��Ŀ������ �ӿ���

public interface Vtar
{

    public Transform LookAtPoint { get; }
    public Transform PosPoint { get; }

    public void setCatch(Transform a,Transform b); //������ץȡ���ÿ��Ƶ�
    public void catchOver();//���ץȡ��Ҫ���ֵ���Ϊ
    public void onDrop(); //���ӵ���Ҫִ�еĲ���

    public void catchTrack();//��֤ʱ�� ��vine��LaterUpdate�׶ε���

    public Vector3 getIKPoint();//ƥ��;�У�����IKƥ��λ������λ��
    public Vector3 getLookPoint();//����IK����LookAt��λ��

    public GameObject gobj();//����gameobjec

    public int BSIdx { get; }//����BlendShapeͨ��

    public bool checkCollide(Vector3 dir,float dis);//��ײ��ⷴ�� - �������ƾ�ͷ�˶�


}
