using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class ExplorationNode : MonoBehaviour
    {
        [Header("Activation")]
        public float activationRadius = 2.4f;
        public bool revealOnAwake;

        [Header("Response")]
        public GameObject responseTarget;
        public GameObject idleRead;
        public GameObject activeRead;
        public Renderer[] highlightRenderers;

        [SerializeField]
        private bool used;

        public bool Used => used;
        public bool IsSolved => used;

        private void Awake()
        {
            ApplyState(used || revealOnAwake);
        }

        public bool CanUse(Transform user, float range)
        {
            if (user == null || used)
            {
                return false;
            }

            var allowedRange = Mathf.Max(range, activationRadius);
            return Vector3.Distance(user.position, transform.position) <= allowedRange;
        }

        public bool TryActivate(Transform user, float range)
        {
            if (!CanUse(user, range))
            {
                return false;
            }

            used = true;
            ApplyState(true);
            return true;
        }

        public void SetSolved(bool solved)
        {
            used = solved;
            ApplyState(solved || revealOnAwake);
        }

        public void ResetNode()
        {
            used = false;
            ApplyState(revealOnAwake);
        }

        public float DistanceTo(Transform user)
        {
            if (user == null)
            {
                return float.PositiveInfinity;
            }

            return Vector3.Distance(user.position, transform.position);
        }

        private void ApplyState(bool active)
        {
            if (responseTarget != null)
            {
                responseTarget.SetActive(active);
            }

            if (idleRead != null)
            {
                idleRead.SetActive(!active);
            }

            if (activeRead != null)
            {
                activeRead.SetActive(active);
            }

            if (highlightRenderers == null)
            {
                return;
            }

            for (var i = 0; i < highlightRenderers.Length; i++)
            {
                var target = highlightRenderers[i];
                if (target != null)
                {
                    target.enabled = active;
                }
            }
        }
    }
}
