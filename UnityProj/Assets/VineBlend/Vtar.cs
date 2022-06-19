using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//可被藤曼抓取的目标物体 接口类

public interface Vtar
{

    public Transform LookAtPoint { get; }
    public Transform PosPoint { get; }

    public void setCatch(Transform a,Transform b); //即将被抓取设置控制点
    public void catchOver();//完成抓取需要表现的行为
    public void onDrop(); //被扔掉需要执行的操作

    public void catchTrack();//保证时序 由vine在LaterUpdate阶段调用

    public Vector3 getIKPoint();//匹配途中，反馈IK匹配位点世界位置
    public Vector3 getLookPoint();//反馈IK骨骼LookAt的位置

    public GameObject gobj();//返回gameobjec

    public int BSIdx { get; }//返回BlendShape通道

    public bool checkCollide(Vector3 dir,float dis);//碰撞检测反馈 - 用于限制镜头运动


}
