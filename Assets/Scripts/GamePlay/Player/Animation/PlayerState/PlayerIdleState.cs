using Animancer;
using UnityEngine;

namespace GamePlay
{
    public class PlayerIdleState : PlayerBaseState
    {
        public PlayerIdleState(PlayerAnimationController playerAnimationController, AnimancerComponent animancer) : base(playerAnimationController, animancer) { }
        public override void OnEnter()
        {
            Debug.Log("OnEnterIdle");
            var v = playerAnimationController.GetLastLinearVelocity();
            animancer.Play(playerAnimationController.IdleAnimationSet.Get(v));
        }

        public override void OnExit()
        {
            animancer.Stop();
        }
    }
}