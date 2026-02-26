using Animancer;
using Common;
using UnityEngine;
using Reflex.Attributes;

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

        [Inject] private IInputService _input;

        StateMachine stateMachine;
        Vector2 lastLinearVelocity;

        public DirectionalAnimationSet8 IdleAnimationSet => idleAnimationSet;
        public DirectionalAnimationSet8 WalkAnimationSet => walkAnimationSet;

        void Awake()
        {
            animancer.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

            lastLinearVelocity = Vector2.zero;
            stateMachine = new StateMachine();

            var idleState = new PlayerIdleState(this, animancer);
            var walkState = new PlayerWalkState(this, animancer);

            stateMachine.AddTransition(idleState, walkState, new FuncPredicate(() => GetSpeed() > WALK_SPEED_THRESHOLD));
            stateMachine.AddTransition(walkState, idleState, new FuncPredicate(() => GetSpeed() <= WALK_SPEED_THRESHOLD));

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

        // input 기반으로 변경 - MovePosition은 linearVelocity를 갱신하지 않음
        public float GetSpeed() => _input != null ? _input.MoveDirection.magnitude : 0f;

        // walk 방향 판단은 여전히 input 기반으로
        public Vector2 GetLinearVelocity() => _input != null ? _input.MoveDirection : Vector2.zero;
        public Vector2 GetLastLinearVelocity() => lastLinearVelocity;
        public void SetLastLinearVelocity(Vector2 velocity) => lastLinearVelocity = velocity;
    }
}