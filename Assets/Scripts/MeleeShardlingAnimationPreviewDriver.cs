using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    [DisallowMultipleComponent]
    public sealed class MeleeShardlingAnimationPreviewDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string[] stateNames = Array.Empty<string>();
        [SerializeField] private float secondsPerState = 0.75f;
        [SerializeField] private bool playOnEnable = true;

        private int stateIndex;
        private float elapsed;

        public Animator Animator => animator;
        public IReadOnlyList<string> StateNames => stateNames;
        public float SecondsPerState => secondsPerState;
        public bool PlayOnEnable => playOnEnable;
        public int CurrentStateIndex => stateIndex;
        public string CurrentStateName => stateNames.Length == 0 ? string.Empty : stateNames[stateIndex];

        public void ConfigureForPreview(Animator targetAnimator, string[] orderedStateNames, float stateSeconds)
        {
            animator = targetAnimator;
            stateNames = orderedStateNames == null ? Array.Empty<string>() : (string[])orderedStateNames.Clone();
            secondsPerState = Mathf.Max(0.1f, stateSeconds);
            playOnEnable = true;
            stateIndex = 0;
            elapsed = 0f;
        }

        public bool PreviewNextState()
        {
            return PlayState(stateIndex + 1);
        }

        public bool PlayState(int index)
        {
            if (!TryResolveAnimator() || stateNames.Length == 0)
            {
                return false;
            }

            stateIndex = WrapIndex(index);
            elapsed = 0f;
            animator.Play(stateNames[stateIndex], 0, 0f);
            return true;
        }

        private void OnEnable()
        {
            if (playOnEnable && Application.isPlaying)
            {
                PlayState(0);
            }
        }

        private void Update()
        {
            if (!playOnEnable || stateNames.Length == 0 || !TryResolveAnimator())
            {
                return;
            }

            elapsed += Time.deltaTime;
            if (elapsed >= secondsPerState)
            {
                PreviewNextState();
            }
        }

        private bool TryResolveAnimator()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }

            return animator != null;
        }

        private int WrapIndex(int index)
        {
            var count = stateNames.Length;
            return ((index % count) + count) % count;
        }
    }
}
