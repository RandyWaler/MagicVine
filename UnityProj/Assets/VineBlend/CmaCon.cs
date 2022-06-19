using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmaCon : MonoBehaviour
{
    [SerializeField]
    VineCon vine;


    [SerializeField]
    Transform troY;//最高父物体 Y轴横转

    [SerializeField]
    Transform troX;//次高父物体 X轴纵转

    [SerializeField]
    float roSpeedX = 90.0f;//Heading 横传速度

    [SerializeField]
    float roSpeedY = 90.0f;//Pitch 纵转速度

    [SerializeField]
    float roXmax=0f;//X轴纵转限制

    [SerializeField]
    float roXmin=0f;

    

    Camera cma;

    float roX;//X旋转记录值（受万向锁影响 不能用transform.eulerAngles.x 需要自行记录）

    private void Awake()
    {
        cma = gameObject.GetComponentInChildren<Camera>();
        roX = transform.parent.eulerAngles.x;
    }



    //temp
    RaycastHit raycastHit;

    float mouseMove;//鼠标运动轴线量

    float rostep;//本帧旋转步长

    float cldCheckDis;//碰撞检测距离 - 需要根据 rostep 计算弧长

    Transform vineTar;

    private void Update() //这里现在Update里旋转镜头/节点  之后LaterUpdate里触发IK更新 避免抖动
    {


        if (!vineTar && vine.VState == VineCon.VineState.get) {
            vineTar = vine.Vinetar.gobj().transform;
        } else if (vine.VState != VineCon.VineState.get)
            vineTar = null;


        if (!Cursor.visible) {

            //镜头控制
            if (vine.VState != VineCon.VineState.go) {//藤曼正在匹配时不旋转镜头

                mouseMove = Input.GetAxis("Mouse Y");
                if (mouseMove >= 0.03 || mouseMove <= -0.03) {//鼠标是否运动

                    rostep = -mouseMove * 10 * Time.deltaTime * roSpeedY;//mouseMove参与step计算 运动更平滑
                    if (!vineTarCollideCheckX()) {//碰撞检测
                        
                        if (roX + rostep >= roXmin && roX + rostep <= roXmax) {
                            roX += rostep;
                            troX.Rotate(troX.right, rostep, Space.World);
                        }
                    }
                    
                }

                mouseMove = Input.GetAxis("Mouse X");
                if (mouseMove >= 0.03 || mouseMove <= -0.03) {//鼠标是否运动
                    rostep = mouseMove * 10 * Time.deltaTime * roSpeedX;
                    if (!vineTarCollideCheckY()) {
                        troY.Rotate(Vector3.up, rostep, Space.World);//mouseMove参与step计算 运动更平滑
                    }
                }
            }

            //藤曼控制

            if (vine.VState == VineCon.VineState.get) {

                if (Input.GetMouseButtonDown(0)) //释放物体
                    vine.vinedrop();
                else {//滚轮控制物体远近
                    mouseMove = Input.GetAxis("Mouse ScrollWheel");
                    if (mouseMove >= 0.03 || mouseMove <= -0.03) {
                        vine.vineZMove(Mathf.Sign(mouseMove));
                    }
                }

            }else if (vine.VState == VineCon.VineState.Idle) {

                if (Input.GetMouseButtonDown(0)) {//抓取物体
                    if (Physics.Raycast(cma.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), out raycastHit)) {
                        if (raycastHit.collider.CompareTag("vinetar")) {
                            vine.vinego(raycastHit.collider.GetComponent<Vtar>());
                        }
                    }
                }

            }
        }
        else if (Input.GetMouseButtonDown(0)) {
            Cursor.visible = false;
        }
    }

    
    //旋转X轴对应镜头上下运动 up - down
    bool vineTarCollideCheckX() {
        if (!vineTar)
            return false;

        //根据 rostep 计算弧长作为碰撞检测距离 注意加abs 取绝对值
        cldCheckDis = Vector3.Distance(vine.transform.position, vine.Vinetar.gobj().transform.position) * Mathf.Abs(rostep) * Mathf.Deg2Rad;

        if (mouseMove > 0)
            return vine.Vinetar.checkCollide(vineTar.up,cldCheckDis);
        else
            return vine.Vinetar.checkCollide(-vineTar.up,cldCheckDis);
    }

    //旋转Y轴对应镜头左右运动 left - right
    bool vineTarCollideCheckY() {
        if (!vineTar)
            return false;

        cldCheckDis = Vector3.Distance(vine.transform.position, vine.Vinetar.gobj().transform.position) * Mathf.Abs(rostep) * Mathf.Deg2Rad;

        if (mouseMove > 0)
            return vine.Vinetar.checkCollide(-vineTar.right,cldCheckDis);//注意由于get状态 vineTar 已 LookAt向 Vine 导致左右镜面对称
        else
            return vine.Vinetar.checkCollide(vineTar.right,cldCheckDis);
    }

}

