using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VSkeleton : VBox1
{
    private Animator animator;


    [SerializeField]
    private float standTime = 5;

    private float nowStandTime = -1;

    private int idleHash = Animator.StringToHash("Idle");
    private int takeHash = Animator.StringToHash("Take");
    private int fallsHash = Animator.StringToHash("FallS");
    private int falleHash = Animator.StringToHash("FallE");
    private int standHash = Animator.StringToHash("Stand");
    private int fallingHash = Animator.StringToHash("Falling");

    bool falleFlag = false;

    protected override void Awake() {
        base.Awake();

        animator = gameObject.GetComponent<Animator>();
    }

    public override void catchOver() {
        animator.SetTrigger(takeHash);
        base.catchOver();
    }

    public override void onDrop() {
        animator.SetTrigger(fallsHash);
        falleFlag = true;
        base.onDrop();
    }


    //这里骷髅兵的释放点没有高度限制，可能贴地释放，因此要用OnCollisionStay持续检测+boolflag保证Trigger只Set一次
    private void OnCollisionStay (Collision collision) {
        if (falleFlag&&animator.GetCurrentAnimatorStateInfo(0).shortNameHash == fallingHash) {
            animator.SetTrigger(falleHash);
            nowStandTime = 0f;
            falleFlag = false;

        }
    }




    private void Update() {

        if (nowStandTime >= 0) {
            nowStandTime += Time.deltaTime;
            if (nowStandTime >= standTime) {
                animator.SetTrigger(standHash);
                nowStandTime = -1;
            }
        }
    }



}
