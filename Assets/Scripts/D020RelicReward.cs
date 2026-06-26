using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class D020RelicReward : MonoBehaviour
    {
        [Header("Reward")]
        public string rewardId = "reward.d020.relic";
        public float pickupRadius = 1.35f;
        public bool autoCollectOnTouch = true;

        [Header("Scene References")]
        public Transform player;
        public GameObject idleRead;
        public GameObject collectedRead;

        [Header("Audio")]
        public AudioClip pickupClip;

        private AudioSource audioSource;
        private bool collected;

        public bool IsCollected => collected;
        public int CollectCount { get; private set; }

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
                audioSource.volume = 0.78f;
            }

            ApplyState();
        }

        private void Update()
        {
            if (autoCollectOnTouch && CanCollect(player))
            {
                TryCollect(player);
            }
        }

        public bool CanCollect(Transform collector)
        {
            if (collector == null || collected)
            {
                return false;
            }

            return Vector3.Distance(collector.position, transform.position) <= pickupRadius;
        }

        public bool TryCollect(Transform collector)
        {
            if (!CanCollect(collector))
            {
                return false;
            }

            collected = true;
            CollectCount++;
            ApplyState();
            if (audioSource != null && pickupClip != null)
            {
                audioSource.PlayOneShot(pickupClip);
            }

            return true;
        }

        public void ResetReward()
        {
            collected = false;
            ApplyState();
        }

        public float DistanceTo(Transform collector)
        {
            if (collector == null)
            {
                return float.PositiveInfinity;
            }

            return Vector3.Distance(collector.position, transform.position);
        }

        private void ApplyState()
        {
            if (idleRead != null)
            {
                idleRead.SetActive(!collected);
            }

            if (collectedRead != null)
            {
                collectedRead.SetActive(collected);
            }
        }
    }
}
