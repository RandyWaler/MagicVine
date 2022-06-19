using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Collider + Rigdbody 刚体物体
public class VBox1 : MonoBehaviour,Vtar
{

    //Base
    protected Rigidbody rbody;
    protected Collider cld;


    //被抓取相关
    [SerializeField]
    protected Vector3 posdel;//位置相对藤曼匹配位点的偏移量 -- 注意要用lookatPoint的空间

    [SerializeField]
    protected int blendShapeIdx;//末端形状匹配对应的 BS 通道
    public int BSIdx
    {
        get {
            return blendShapeIdx;
        }
    }

    protected Transform LookatPoint;//被抓取后需要保持LookAt的位点  -- 反馈位点的偏移矫正也使用该空间
    public Transform LookAtPoint
    {
        get {
            return LookatPoint;
        }
    }

    protected Transform posPoint;//被抓去后需要保持位置匹配的位点
    public Transform PosPoint
    {
        get {
            return posPoint;
        }
    }


    protected bool beCatch = false;//是否被抓取
    public bool BCatch {
        get {
            return beCatch;
        }
    }


    //碰撞相关

    [SerializeField]
    protected Vector3 boxCldSize;//Box碰撞大小

    [SerializeField]
    protected Vector3 boxCenter;//Box中心相对节点偏移量
    
    

    //位点反馈
    public Vector3 getIKPoint()
    {
        return transform.position + LookatPoint.forward * posdel.z + LookatPoint.up * posdel.y + LookatPoint.right * posdel.x;
    }
    public Vector3 getLookPoint()   //这里距离可能过近，导致vine需要反向收缩，骨链lookat的位点不加Z方向进深矫正，保证一定看向目标位置
{
        return transform.position + LookatPoint.up * posdel.y + LookatPoint.right * posdel.x;
    }



    //被抓取接口
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

            //针对Box刚体
            rbody.isKinematic = true;
            rbody.useGravity = false;
            cld.enabled = false;
        }
    }
    public virtual void onDrop()
    {
        if (beCatch) {
            beCatch = false;
            rbody.isKinematic = false;//解除静态化
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


    //碰撞检测接口
    public bool checkCollide(Vector3 dir, float dis) {

        //这里投射起始点向回提了0.05f，并增加检测距离
        //防止由于紧贴碰撞表面导致投射检测失效

        if (Physics.BoxCast(transform.position - dir * 0.05f+boxCenter,
                           boxCldSize/2,
                           dir,transform.rotation,dis + 0.05f)) //Box碰撞投射
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
