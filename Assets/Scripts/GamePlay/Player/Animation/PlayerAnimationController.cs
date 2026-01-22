using Animancer;
using Common;
using UnityEngine;

namespace GamePlay
{
    public class PlayerAnimationController : MonoBehaviour
    {
        const float WALK_SPEED_THRESHOLD = 0.1f;

        [Header("Player")]
        [SerializeField] Rigidbody2D rigid;

        [Header("Animations")]
        [SerializeField] AnimancerComponent animancer;
        [SerializeField] DirectionalAnimationSet8 idleAnimationSet;
        [SerializeField] DirectionalAnimationSet8 walkAnimationSet;

        StateMachine stateMachine;
        Vector2 lastLinearVelocity;

        public DirectionalAnimationSet8 IdleAnimationSet => idleAnimationSet;
        public DirectionalAnimationSet8 WalkAnimationSet => walkAnimationSet;

        void Awake()
        {
            lastLinearVelocity = Vector2.zero;
            stateMachine = new StateMachine();

            // states
            var idleState = new PlayerIdleState(this, animancer);
            var walkState = new PlayerWalkState(this, animancer);

            // transitions
            stateMachine.AddTransition(idleState, walkState, new FuncPredicate(() => GetSpeed() > WALK_SPEED_THRESHOLD));
            stateMachine.AddTransition(walkState, idleState, new FuncPredicate(() => GetSpeed() <= WALK_SPEED_THRESHOLD));

            // set initial state
            stateMachine.SetState(idleState);
        }

        void Update()
        {
            stateMachine.Update();
        }

        public void FlipX(bool flip)
        {
            animancer.transform.localScale = new Vector3(flip ? -1 : 1, 1, 1);
        }

        public float GetSpeed() => rigid.linearVelocity.magnitude;
        public Vector2 GetLinearVelocity() => rigid.linearVelocity;
        public Vector2 GetLastLinearVelocity() => lastLinearVelocity;
        public void SetLastLinearVelocity(Vector2 velocity) => lastLinearVelocity = velocity;
    }
}