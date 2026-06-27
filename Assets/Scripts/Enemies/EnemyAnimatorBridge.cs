using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class EnemyAnimatorBridge : MonoBehaviour
    {
        [Header("Animator")]
        public Animator animator;
        public string stateParameter = "State";
        public string speedParameter = "Speed";
        public string telegraphTrigger = "Telegraph";
        public string attackTrigger = "Attack";
        public string hitTrigger = "Hit";
        public string deathTrigger = "Death";

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        public void SetState(EnemyState state)
        {
            TrySetInteger(stateParameter, (int)state);
        }

        public void SetSpeed(float speed01)
        {
            TrySetFloat(speedParameter, speed01);
        }

        public void TriggerTelegraph()
        {
            TrySetTrigger(telegraphTrigger);
        }

        public void TriggerAttack()
        {
            TrySetTrigger(attackTrigger);
        }

        public void TriggerHit()
        {
            TrySetTrigger(hitTrigger);
        }

        public void TriggerDeath()
        {
            TrySetTrigger(deathTrigger);
        }

        private void TrySetInteger(string parameter, int value)
        {
            if (CanUse(parameter, AnimatorControllerParameterType.Int))
            {
                animator.SetInteger(parameter, value);
            }
        }

        private void TrySetFloat(string parameter, float value)
        {
            if (CanUse(parameter, AnimatorControllerParameterType.Float))
            {
                animator.SetFloat(parameter, value);
            }
        }

        private void TrySetTrigger(string parameter)
        {
            if (CanUse(parameter, AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger(parameter);
            }
        }

        private bool CanUse(string parameter, AnimatorControllerParameterType type)
        {
            if (animator == null || string.IsNullOrEmpty(parameter))
            {
                return false;
            }

            var parameters = animator.parameters;
            for (var index = 0; index < parameters.Length; index++)
            {
                var item = parameters[index];
                if (item.type == type && item.name == parameter)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
