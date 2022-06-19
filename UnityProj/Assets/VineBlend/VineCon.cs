using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineCon : MonoBehaviour
{
    Animator animator;


    //抓取目标接口类
    Vtar vinetar;//抓取目标节点
    public Vtar Vinetar
    {
        get {
            return vinetar;
        }
    }


    //状态相关
    public enum VineState
    {
        Idle,//空闲的
        go,//正在抓取的
        get//已抓取到目标
    }

    VineState vstate=VineState.Idle;

    public VineState VState {
        get {
            return vstate;
        }
    }

    


    //伸长匹配控制相关
    [SerializeField]
    Transform vineroot;//伸长的起始根节点（不是脚本所挂的节点）

    [SerializeField]
    int deep;//节点深度，根节点之下多少阶需要进行伸长控制

    List<Transform> vinebones;
    List<float> vboneX;
    float baseBoneLinkLength;//骨链的基础长度

    bool onIK = false;//是否开启IK匹配（由帧事件控制开启）

    [SerializeField]
    Transform ikpoint;//IK匹配位点  抓取时由目标Vtar反馈位点

    Vector3 nowikpoint;//当前匹配位置

    [SerializeField]
    float ikmoveSpeed=5.0f;//匹配速度

    

    //旋转匹配控制相关
    [SerializeField]
    Transform tarLookPoint;//目标点look目标 ，在抓取开始就会lookat向目标(Vine所在根节点是逐渐旋转向目标的)

    [SerializeField]
    float vineRoSpeed = 180.0f;//藤曼旋转匹配速度

    bool roflag=false;//控制旋转匹配开启与否


    //BlendShape 缠绕匹配
    bool bsflag = false;//是否进行缠绕匹配

    [SerializeField]
    SkinnedMeshRenderer skmesh;//网格 需要控制BlendShape通道权重

    [SerializeField]
    float bsSpeed=300f;//BlendShape增长速度


    //Z纵深方向运动控制
    [SerializeField]
    float zmoveSpeed = 10.0f;//鼠标滚轮控制Z进深运动速度

    [SerializeField]
    float zMinDis = 3.0f;//物体最近距离 - 过近会导致藤曼扭曲



    //hash
    int gohash = Animator.StringToHash("go");


    //temp - GC对抗

    float angle;//Update
    Vector3 lookvec;
    float roStep;
    float dis;
    float moveStep;

    float zdis;
    Vector3 zmoveStep;


    //伸长匹配

    void ikgotoPoint(Vector3 point)
    {
        //看向匹配目标

        foreach (var vb in vinebones) {
            vb.LookAt(vinetar.getLookPoint(),Vector3.down);
            vb.RotateAround(vb.position, vb.up, 90.0f);
            vb.RotateAround(vb.position, vb.right, -45.0f);

        }

        dis = (Vector3.Distance(point, vinebones[0].position) - baseBoneLinkLength) / (deep-2); //将del均分给子骨骼  末端两块骨不能动否则会影响BlendShape

        //节点位移
        for (int i = 1; i < vinebones.Count-2; ++i) {//位移中根点不动，只移动
            vinebones[i].localPosition = new Vector3(vboneX[i - 1] - dis, 0, 0); //这里骨链是-X轴延伸，所以需要减               
        }

    }

    //Mono
    private void Awake()
    {
        animator = gameObject.GetComponent<Animator>();

        //初始化匹配节点
        vinebones = new List<Transform>();
        vboneX = new List<float>();
        vinebones.Add(vineroot);
        Transform pre = vineroot;

        baseBoneLinkLength = 0;

        for (int i = 0; i < deep; ++i) {
            pre = pre.GetChild(0);
            vinebones.Add(pre);
            baseBoneLinkLength -= pre.localPosition.x;//【注意！！！】这里本例中导出的骨骼节点是以-x为延伸方向的，因此需要使用-叠加
            vboneX.Add(pre.localPosition.x);
        }
    }
    private void LateUpdate()
    {
        if (vstate == VineState.go) {//需要执行匹配计算动画

            //旋转匹配
            if (roflag) {

                lookvec = new Vector3(vinetar.gobj().transform.position.x - transform.position.x, 0, vinetar.gobj().transform.position.z - transform.position.z);
                angle = Vector3.Angle(lookvec, transform.forward);
                roStep = Time.deltaTime * vineRoSpeed;

                if (angle > roStep) {
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookvec, Vector3.up), roStep);
                }
                else {//完成匹配的
                    transform.LookAt(new Vector3(vinetar.gobj().transform.position.x, transform.position.y, vinetar.gobj().transform.position.z));
                    roflag = false;
                }

            }else
                transform.LookAt(new Vector3(vinetar.gobj().transform.position.x, transform.position.y, vinetar.gobj().transform.position.z));



            //IK匹配（动画末端开启）
            if (onIK) { //进行IK匹配

                //根据目标反馈匹配点位置
                ikpoint.position = vinetar.getIKPoint();

                //当前匹配点移动
                dis = Vector3.Distance(nowikpoint, ikpoint.position);
                moveStep = ikmoveSpeed * Time.deltaTime;

                if (dis > moveStep) {
                    nowikpoint = Vector3.MoveTowards(nowikpoint, ikpoint.position, moveStep);
                }
                else {//IK匹配完成
                    nowikpoint = ikpoint.position;
                    onIK = false;
                    bsflag = true;
                }

                //按当前匹配点计算IK
                ikgotoPoint(nowikpoint);

            }

            if(bsflag) { //进行末端BlendShape缠绕匹配

                //控制 BlendShape 权重增长
                moveStep = Time.deltaTime * bsSpeed;
                if (100 - skmesh.GetBlendShapeWeight(vinetar.BSIdx) > moveStep) {
                    skmesh.SetBlendShapeWeight(vinetar.BSIdx, skmesh.GetBlendShapeWeight(vinetar.BSIdx) + moveStep);
                }
                else {
                    skmesh.SetBlendShapeWeight(vinetar.BSIdx, 100);
                    vstate = VineState.get;
                    bsflag = false;

                    vinetar.catchOver();//执行被抓取行为
                }

                //保持子骨骼的IK
                //注意！！如果LaterUpdate不进行覆盖，则会表现Animator控制下的动画效果
                ikgotoPoint(ikpoint.position);
            }

        }

        if (vstate == VineState.get) {//抓取状态保持子骨骼的IK 

            vinetar.catchTrack();//时序保证 vineTar 先追踪 保证LookAtPoint反馈无误

            //保持子骨骼的IK
            //注意！！如果LaterUpdate不进行覆盖，则会表现Animator控制下的动画效果
            ikgotoPoint(ikpoint.position);
        }
    }


    //外部接口

    //鼠标滚轮控制ik位点Z方向纵深运动
    public void vineZMove(float sign) //传入正负向
    {
        //外部调用也应check
        if (vstate != VineState.get)
            return;

        zmoveStep = zmoveSpeed * tarLookPoint.forward*sign;
        zdis = Vector3.Distance(ikpoint.position+ zmoveStep, gameObject.transform.position);
        
        if(zdis>=zMinDis)
            ikpoint.position += zmoveStep;
    }

    //Idel空闲状态，抓取对应目标
    public void vinego(Vtar tar) 
    {
        //外部调用也应check
        if (vstate != VineState.Idle||animator.GetBool(gohash))
            return;
        

        //匹配初始化
        vinetar = tar;
        animator.SetBool(gohash, true);
        vstate = VineState.go;
        roflag = true;

        tarLookPoint.position = transform.position;
        tarLookPoint.LookAt(new Vector3(tar.gobj().transform.position.x,tarLookPoint.position.y, tar.gobj().transform.position.z));


        //设置抓取信息
        tar.setCatch(tarLookPoint, ikpoint);


    }

    public void vinedrop()//get已抓取，扔掉目标
    {

        //调用目标drop方法，表现被丢弃行为
        vinetar.onDrop();
        
        //Vine重置
        animator.SetBool(gohash, false);
        vstate = VineState.Idle;
        skmesh.SetBlendShapeWeight(vinetar.BSIdx, 0);
        vinetar = null;

    }


    //动画帧事件接口
    public void vineikon()//vinego动画末端帧事件触发，开启IK匹配
    {
        onIK = true;
        nowikpoint = vinebones[vinebones.Count - 1].position;
    }



}
