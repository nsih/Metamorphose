using Animancer;
using UnityEngine;

namespace GamePlay
{
    public class PlayerWalkState : PlayerBaseState
    {
        Vector2 lastLinearVelocity;
        bool flipX = false;

        public PlayerWalkState(PlayerAnimationController playerAnimationController, AnimancerComponent animancer) : base(playerAnimationController, animancer) { }

        public override void OnEnter()
        {
            Debug.Log("OnEnterWalk");
            var v = playerAnimationController.GetLinearVelocity();
            animancer.Play(playerAnimationController.WalkAnimationSet.Get(v));
            flipX = v.x < 0;
            playerAnimationController.FlipX(flipX);
        }

        public override void Update()
        {
            var v = playerAnimationController.GetLinearVelocity();
            animancer.Play(playerAnimationController.WalkAnimationSet.Get(v));
            Debug.Log(v);

            if(flipX == false && v.x < 0)
            {
                flipX = true;
                playerAnimationController.FlipX(true);
            }
            else if(flipX == true && v.x > 0)
            {
                flipX = false;
                playerAnimationController.FlipX(false);
            }

            lastLinearVelocity = v;
        }
        
        public override void OnExit()
        {
            playerAnimationController.SetLastLinearVelocity(lastLinearVelocity);
            animancer.Stop();
        }
    }
}