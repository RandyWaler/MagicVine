using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Collider + Rigdbody ��������
public class VBox1 : MonoBehaviour,Vtar
{

    //Base
    protected Rigidbody rbody;
    protected Collider cld;


    //��ץȡ���
    [SerializeField]
    protected Vector3 posdel;//λ���������ƥ��λ���ƫ���� -- ע��Ҫ��lookatPoint�Ŀռ�

    [SerializeField]
    protected int blendShapeIdx;//ĩ����״ƥ���Ӧ�� BS ͨ��
    public int BSIdx
    {
        get {
            return blendShapeIdx;
        }
    }

    protected Transform LookatPoint;//��ץȡ����Ҫ����LookAt��λ��  -- ����λ���ƫ�ƽ���Ҳʹ�øÿռ�
    public Transform LookAtPoint
    {
        get {
            return LookatPoint;
        }
    }

    protected Transform posPoint;//��ץȥ����Ҫ����λ��ƥ���λ��
    public Transform PosPoint
    {
        get {
            return posPoint;
        }
    }


    protected bool beCatch = false;//�Ƿ�ץȡ
    public bool BCatch {
        get {
            return beCatch;
        }
    }


    //��ײ���

    [SerializeField]
    protected Vector3 boxCldSize;//Box��ײ��С

    [SerializeField]
    protected Vector3 boxCenter;//Box������Խڵ�ƫ����
    
    

    //λ�㷴��
    public Vector3 getIKPoint()
    {
        return transform.position + LookatPoint.forward * posdel.z + LookatPoint.up * posdel.y + LookatPoint.right * posdel.x;
    }
    public Vector3 getLookPoint()   //���������ܹ���������vine��Ҫ��������������lookat��λ�㲻��Z��������������֤һ������Ŀ��λ��
{
        return transform.position + LookatPoint.up * posdel.y + LookatPoint.right * posdel.x;
    }



    //��ץȡ�ӿ�
    public virtual void setCatch(Transform a,Transform b)
    {
        if (!beCatch) {
           
            LookatPoint = a;
            posPoint = b;

        }
    }
    public virtual void catchOver() {
        if (!beCatch) {
            beCatch = true;

            //���Box����
            rbody.isKinematic = true;
            rbody.useGravity = false;
            cld.enabled = false;
        }
    }
    public virtual void onDrop()
    {
        if (beCatch) {
            beCatch = false;
            rbody.isKinematic = false;//�����̬��
            rbody.useGravity = true;
            cld.enabled = true;
        }
    }


    //Mono
    protected virtual void Awake()
    {
        rbody= gameObject.GetComponent<Rigidbody>();
        cld = gameObject.GetComponent<Collider>();
    }
    
    //
    public GameObject gobj()
    {
        return gameObject;
    }


    //��ײ���ӿ�
    public bool checkCollide(Vector3 dir, float dis) {

        //����Ͷ����ʼ���������0.05f�������Ӽ�����
        //��ֹ���ڽ�����ײ���浼��Ͷ����ʧЧ

        if (Physics.BoxCast(transform.position - dir * 0.05f+boxCenter,
                           boxCldSize/2,
                           dir,transform.rotation,dis + 0.05f)) //Box��ײͶ��
            return true;
        else
            return false;
    }

    public void catchTrack() {
        if (beCatch) {
            transform.position = posPoint.position - LookatPoint.forward * posdel.z - LookatPoint.up * posdel.y - LookatPoint.right * posdel.x;
            transform.LookAt(new Vector3(LookatPoint.position.x, transform.position.y, LookatPoint.position.z));
        }
    }
}
