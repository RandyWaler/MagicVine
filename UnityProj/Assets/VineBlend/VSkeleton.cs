using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VSkeleton : VBox1
{
    private Animator animator;


    [SerializeField]
    private float standTime = 5;

    private float nowStandTime = -1;

    private int takeHash = Animator.StringToHash("Take");
    private int fallsHash = Animator.StringToHash("FallS");
    private int falleHash = Animator.StringToHash("FallE");
    private int standHash = Animator.StringToHash("Stand");
    private int fallingHash = Animator.StringToHash("Falling");

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
        base.onDrop();
    }


    private void OnCollisionEnter(Collision collision) {
        if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash == fallingHash) {
            animator.SetTrigger(falleHash);
            nowStandTime = 0f;
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
