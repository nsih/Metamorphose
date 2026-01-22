using Animancer;
using Common;

namespace GamePlay
{
    public abstract class PlayerBaseState : IState
    {
        protected readonly PlayerAnimationController playerAnimationController;
        protected readonly AnimancerComponent animancer;

        protected PlayerBaseState(PlayerAnimationController playerAnimationController, AnimancerComponent animancer)
        {
            this.playerAnimationController = playerAnimationController;
            this.animancer = animancer;
        }

        public virtual void OnEnter()
        {

        }

        public virtual void Update()
        {

        }

        public virtual void FixedUpdate()
        {

        }

        public virtual void OnExit()
        {
            
        }
    }
}