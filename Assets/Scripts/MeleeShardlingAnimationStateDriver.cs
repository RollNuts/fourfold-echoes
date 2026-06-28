using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public enum MeleeShardlingAnimationAction
    {
        Idle,
        Walk,
        Run,
        AttackStart,
        AttackLoop,
        AttackEnd,
        HitFront,
        HitBack,
        Knockdown,
        Death,
        CastStart,
        ChannelLoop,
        CastRelease,
        Interact,
    }

    [DisallowMultipleComponent]
    public sealed class MeleeShardlingAnimationStateDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string[] stateNames = Array.Empty<string>();
        [SerializeField] private float crossFadeSeconds = 0.05f;

        public Animator Animator => animator;
        public IReadOnlyList<string> StateNames => stateNames;
        public float CrossFadeSeconds => crossFadeSeconds;
        public MeleeShardlingAnimationAction LastRequestedAction { get; private set; }
        public string LastRequestedState { get; private set; } = string.Empty;

        public void ConfigureForRuntime(Animator targetAnimator, string[] orderedStateNames, float fadeSeconds)
        {
            animator = targetAnimator;
            stateNames = orderedStateNames == null ? Array.Empty<string>() : (string[])orderedStateNames.Clone();
            crossFadeSeconds = Mathf.Max(0f, fadeSeconds);
            LastRequestedAction = MeleeShardlingAnimationAction.Idle;
            LastRequestedState = string.Empty;
        }

        public bool TryPlay(MeleeShardlingAnimationAction action, float normalizedTime = 0f)
        {
            return TryPlay(action, crossFadeSeconds, normalizedTime);
        }

        public bool TryPlayImmediate(MeleeShardlingAnimationAction action, float normalizedTime = 0f)
        {
            return TryPlay(action, 0f, normalizedTime);
        }

        public bool TryGetStateName(MeleeShardlingAnimationAction action, out string stateName)
        {
            var index = (int)action;
            if (stateNames == null || index < 0 || index >= stateNames.Length || string.IsNullOrEmpty(stateNames[index]))
            {
                stateName = string.Empty;
                return false;
            }

            stateName = stateNames[index];
            return true;
        }

        public static bool IsLoopingAction(MeleeShardlingAnimationAction action)
        {
            return action == MeleeShardlingAnimationAction.Idle
                || action == MeleeShardlingAnimationAction.Walk
                || action == MeleeShardlingAnimationAction.Run
                || action == MeleeShardlingAnimationAction.AttackLoop
                || action == MeleeShardlingAnimationAction.ChannelLoop;
        }

        private bool TryPlay(MeleeShardlingAnimationAction action, float transitionSeconds, float normalizedTime)
        {
            if (!TryResolveAnimator() || !TryGetStateName(action, out var stateName))
            {
                return false;
            }

            LastRequestedAction = action;
            LastRequestedState = stateName;
            if (transitionSeconds <= 0f)
            {
                animator.Play(stateName, 0, normalizedTime);
            }
            else
            {
                animator.CrossFadeInFixedTime(stateName, transitionSeconds, 0, normalizedTime);
            }

            return true;
        }

        private bool TryResolveAnimator()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            return animator != null;
        }
    }
}
