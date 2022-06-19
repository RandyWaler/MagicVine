using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineCon : MonoBehaviour
{
    Animator animator;


    //ץȡĿ��ӿ���
    Vtar vinetar;//ץȡĿ��ڵ�
    public Vtar Vinetar
    {
        get {
            return vinetar;
        }
    }


    //״̬���
    public enum VineState
    {
        Idle,//���е�
        go,//����ץȡ��
        get//��ץȡ��Ŀ��
    }

    VineState vstate=VineState.Idle;

    public VineState VState {
        get {
            return vstate;
        }
    }

    


    //�쳤ƥ��������
    [SerializeField]
    Transform vineroot;//�쳤����ʼ���ڵ㣨���ǽű����ҵĽڵ㣩

    [SerializeField]
    int deep;//�ڵ���ȣ����ڵ�֮�¶��ٽ���Ҫ�����쳤����

    List<Transform> vinebones;
    List<float> vboneX;
    float baseBoneLinkLength;//�����Ļ�������

    bool onIK = false;//�Ƿ���IKƥ�䣨��֡�¼����ƿ�����

    [SerializeField]
    Transform ikpoint;//IKƥ��λ��  ץȡʱ��Ŀ��Vtar����λ��

    Vector3 nowikpoint;//��ǰƥ��λ��

    [SerializeField]
    float ikmoveSpeed=5.0f;//ƥ���ٶ�

    

    //��תƥ��������
    [SerializeField]
    Transform tarLookPoint;//Ŀ���lookĿ�� ����ץȡ��ʼ�ͻ�lookat��Ŀ��(Vine���ڸ��ڵ�������ת��Ŀ���)

    [SerializeField]
    float vineRoSpeed = 180.0f;//������תƥ���ٶ�

    bool roflag=false;//������תƥ�俪�����


    //BlendShape ����ƥ��
    bool bsflag = false;//�Ƿ���в���ƥ��

    [SerializeField]
    SkinnedMeshRenderer skmesh;//���� ��Ҫ����BlendShapeͨ��Ȩ��

    [SerializeField]
    float bsSpeed=300f;//BlendShape�����ٶ�


    //Z������˶�����
    [SerializeField]
    float zmoveSpeed = 10.0f;//�����ֿ���Z�����˶��ٶ�

    [SerializeField]
    float zMinDis = 3.0f;//����������� - �����ᵼ������Ť��



    //hash
    int gohash = Animator.StringToHash("go");


    //temp - GC�Կ�

    float angle;//Update
    Vector3 lookvec;
    float roStep;
    float dis;
    float moveStep;

    float zdis;
    Vector3 zmoveStep;


    //�쳤ƥ��

    void ikgotoPoint(Vector3 point)
    {
        //����ƥ��Ŀ��

        foreach (var vb in vinebones) {
            vb.LookAt(vinetar.getLookPoint(),Vector3.down);
            vb.RotateAround(vb.position, vb.up, 90.0f);
            vb.RotateAround(vb.position, vb.right, -45.0f);

        }

        dis = (Vector3.Distance(point, vinebones[0].position) - baseBoneLinkLength) / (deep-2); //��del���ָ��ӹ���  ĩ������ǲ��ܶ������Ӱ��BlendShape

        //�ڵ�λ��
        for (int i = 1; i < vinebones.Count-2; ++i) {//λ���и��㲻����ֻ�ƶ�
            vinebones[i].localPosition = new Vector3(vboneX[i - 1] - dis, 0, 0); //���������-X�����죬������Ҫ��               
        }

    }

    //Mono
    private void Awake()
    {
        animator = gameObject.GetComponent<Animator>();

        //��ʼ��ƥ��ڵ�
        vinebones = new List<Transform>();
        vboneX = new List<float>();
        vinebones.Add(vineroot);
        Transform pre = vineroot;

        baseBoneLinkLength = 0;

        for (int i = 0; i < deep; ++i) {
            pre = pre.GetChild(0);
            vinebones.Add(pre);
            baseBoneLinkLength -= pre.localPosition.x;//��ע�⣡���������ﱾ���е����Ĺ����ڵ�����-xΪ���췽��ģ������Ҫʹ��-����
            vboneX.Add(pre.localPosition.x);
        }
    }
    private void LateUpdate()
    {
        if (vstate == VineState.go) {//��Ҫִ��ƥ����㶯��

            //��תƥ��
            if (roflag) {

                lookvec = new Vector3(vinetar.gobj().transform.position.x - transform.position.x, 0, vinetar.gobj().transform.position.z - transform.position.z);
                angle = Vector3.Angle(lookvec, transform.forward);
                roStep = Time.deltaTime * vineRoSpeed;

                if (angle > roStep) {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookvec, Vector3.up), roStep);
                }
                else {//���ƥ���
                    transform.LookAt(new Vector3(vinetar.gobj().transform.position.x, transform.position.y, vinetar.gobj().transform.position.z));
                    roflag = false;
                }

            }else
                transform.LookAt(new Vector3(vinetar.gobj().transform.position.x, transform.position.y, vinetar.gobj().transform.position.z));



            //IKƥ�䣨����ĩ�˿�����
            if (onIK) { //����IKƥ��

                //����Ŀ�귴��ƥ���λ��
                ikpoint.position = vinetar.getIKPoint();

                //��ǰƥ����ƶ�
                dis = Vector3.Distance(nowikpoint, ikpoint.position);
                moveStep = ikmoveSpeed * Time.deltaTime;

                if (dis > moveStep) {
                    nowikpoint = Vector3.MoveTowards(nowikpoint, ikpoint.position, moveStep);
                }
                else {//IKƥ�����
                    nowikpoint = ikpoint.position;
                    onIK = false;
                    bsflag = true;
                }

                //����ǰƥ������IK
                ikgotoPoint(nowikpoint);

            }

            if(bsflag) { //����ĩ��BlendShape����ƥ��

                //���� BlendShape Ȩ������
                moveStep = Time.deltaTime * bsSpeed;
                if (100 - skmesh.GetBlendShapeWeight(vinetar.BSIdx) > moveStep) {
                    skmesh.SetBlendShapeWeight(vinetar.BSIdx, skmesh.GetBlendShapeWeight(vinetar.BSIdx) + moveStep);
                }
                else {
                    skmesh.SetBlendShapeWeight(vinetar.BSIdx, 100);
                    vstate = VineState.get;
                    bsflag = false;

                    vinetar.catchOver();//ִ�б�ץȡ��Ϊ
                }

                //�����ӹ�����IK
                //ע�⣡�����LaterUpdate�����и��ǣ�������Animator�����µĶ���Ч��
                ikgotoPoint(ikpoint.position);
            }

        }

        if (vstate == VineState.get) {//ץȡ״̬�����ӹ�����IK 

            vinetar.catchTrack();//ʱ��֤ vineTar ��׷�� ��֤LookAtPoint��������

            //�����ӹ�����IK
            //ע�⣡�����LaterUpdate�����и��ǣ�������Animator�����µĶ���Ч��
            ikgotoPoint(ikpoint.position);
        }
    }


    //�ⲿ�ӿ�

    //�����ֿ���ikλ��Z���������˶�
    public void vineZMove(float sign) //����������
    {
        //�ⲿ����ҲӦcheck
        if (vstate != VineState.get)
            return;

        zmoveStep = zmoveSpeed * tarLookPoint.forward*sign;
        zdis = Vector3.Distance(ikpoint.position+ zmoveStep, gameObject.transform.position);
        
        if(zdis>=zMinDis)
            ikpoint.position += zmoveStep;
    }

    //Idel����״̬��ץȡ��ӦĿ��
    public void vinego(Vtar tar) 
    {
        //�ⲿ����ҲӦcheck
        if (vstate != VineState.Idle||animator.GetBool(gohash))
            return;
        

        //ƥ���ʼ��
        vinetar = tar;
        animator.SetBool(gohash, true);
        vstate = VineState.go;
        roflag = true;

        tarLookPoint.position = transform.position;
        tarLookPoint.LookAt(new Vector3(tar.gobj().transform.position.x,tarLookPoint.position.y, tar.gobj().transform.position.z));


        //����ץȡ��Ϣ
        tar.setCatch(tarLookPoint, ikpoint);


    }

    public void vinedrop()//get��ץȡ���ӵ�Ŀ��
    {

        //����Ŀ��drop���������ֱ�������Ϊ
        vinetar.onDrop();
        
        //Vine����
        animator.SetBool(gohash, false);
        vstate = VineState.Idle;
        skmesh.SetBlendShapeWeight(vinetar.BSIdx, 0);
        vinetar = null;

    }


    //����֡�¼��ӿ�
    public void vineikon()//vinego����ĩ��֡�¼�����������IKƥ��
    {
        onIK = true;
        nowikpoint = vinebones[vinebones.Count - 1].position;
    }



}
