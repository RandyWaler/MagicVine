using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CmaCon : MonoBehaviour
{
    [SerializeField]
    VineCon vine;


    [SerializeField]
    Transform troY;//��߸����� Y���ת

    [SerializeField]
    Transform troX;//�θ߸����� X����ת

    [SerializeField]
    float roSpeedX = 90.0f;//Heading �ᴫ�ٶ�

    [SerializeField]
    float roSpeedY = 90.0f;//Pitch ��ת�ٶ�

    [SerializeField]
    float roXmax=0f;//X����ת����

    [SerializeField]
    float roXmin=0f;

    

    Camera cma;

    float roX;//X��ת��¼ֵ����������Ӱ�� ������transform.eulerAngles.x ��Ҫ���м�¼��

    private void Awake()
    {
        cma = gameObject.GetComponentInChildren<Camera>();
        roX = transform.parent.eulerAngles.x;
    }



    //temp
    RaycastHit raycastHit;

    float mouseMove;//����˶�������

    float rostep;//��֡��ת����

    float cldCheckDis;//��ײ������ - ��Ҫ���� rostep ���㻡��

    Transform vineTar;

    private void Update() //��������Update����ת��ͷ/�ڵ�  ֮��LaterUpdate�ﴥ��IK���� ���ⶶ��
    {


        if (!vineTar && vine.VState == VineCon.VineState.get) {
            vineTar = vine.Vinetar.gobj().transform;
        } else if (vine.VState != VineCon.VineState.get)
            vineTar = null;


        if (!Cursor.visible) {

            //��ͷ����
            if (vine.VState != VineCon.VineState.go) {//��������ƥ��ʱ����ת��ͷ

                mouseMove = Input.GetAxis("Mouse Y");
                if (mouseMove >= 0.03 || mouseMove <= -0.03) {//����Ƿ��˶�

                    rostep = -mouseMove * 10 * Time.deltaTime * roSpeedY;//mouseMove����step���� �˶���ƽ��
                    if (!vineTarCollideCheckX()) {//��ײ���
                        
                        if (roX + rostep >= roXmin && roX + rostep <= roXmax) {
                            roX += rostep;
                            troX.Rotate(troX.right, rostep, Space.World);
                        }
                    }
                    
                }

                mouseMove = Input.GetAxis("Mouse X");
                if (mouseMove >= 0.03 || mouseMove <= -0.03) {//����Ƿ��˶�
                    rostep = mouseMove * 10 * Time.deltaTime * roSpeedX;
                    if (!vineTarCollideCheckY()) {
                        troY.Rotate(Vector3.up, rostep, Space.World);//mouseMove����step���� �˶���ƽ��
                    }
                }
            }

            //��������

            if (vine.VState == VineCon.VineState.get) {

                if (Input.GetMouseButtonDown(0)) //�ͷ�����
                    vine.vinedrop();
                else {//���ֿ�������Զ��
                    mouseMove = Input.GetAxis("Mouse ScrollWheel");
                    if (mouseMove >= 0.03 || mouseMove <= -0.03) {
                        vine.vineZMove(Mathf.Sign(mouseMove));
                    }
                }

            }else if (vine.VState == VineCon.VineState.Idle) {

                if (Input.GetMouseButtonDown(0)) {//ץȡ����
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

    
    //��תX���Ӧ��ͷ�����˶� up - down
    bool vineTarCollideCheckX() {
        if (!vineTar)
            return false;

        //���� rostep ���㻡����Ϊ��ײ������ ע���abs ȡ����ֵ
        cldCheckDis = Vector3.Distance(vine.transform.position, vine.Vinetar.gobj().transform.position) * Mathf.Abs(rostep) * Mathf.Deg2Rad;

        if (mouseMove > 0)
            return vine.Vinetar.checkCollide(vineTar.up,cldCheckDis);
        else
            return vine.Vinetar.checkCollide(-vineTar.up,cldCheckDis);
    }

    //��תY���Ӧ��ͷ�����˶� left - right
    bool vineTarCollideCheckY() {
        if (!vineTar)
            return false;

        cldCheckDis = Vector3.Distance(vine.transform.position, vine.Vinetar.gobj().transform.position) * Mathf.Abs(rostep) * Mathf.Deg2Rad;

        if (mouseMove > 0)
            return vine.Vinetar.checkCollide(-vineTar.right,cldCheckDis);//ע������get״̬ vineTar �� LookAt�� Vine �������Ҿ���Գ�
        else
            return vine.Vinetar.checkCollide(vineTar.right,cldCheckDis);
    }

}

