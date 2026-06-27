using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class D020RelicReward : MonoBehaviour
    {
        public string rewardId = "d020.region01.relic.01";
        public float pickupRadius = 1.05f;
        public bool autoCollectOnTouch = true;
        public Transform player;
        public ExplorationNode requiredNode;
        public D020EnemyDummy requiredEnemy;
        public GameObject idleRead;
        public GameObject collectedRead;
        public AudioClip pickupClip;

        [SerializeField]
        private bool collected;

        private AudioSource audioSource;

        public bool IsCollected => collected;
        public bool IsUnlocked => (requiredNode == null || requiredNode.IsSolved) &&
                                  (requiredEnemy == null || requiredEnemy.IsDefeated);
        public int CollectCount { get; private set; }

        private void Awake()
        {
            EnsureAudioSource();
            ApplyState();
        }

        private void Update()
        {
            if (autoCollectOnTouch)
            {
                TryCollect(player);
            }
        }

        public bool CanCollect(Transform collector)
        {
            return !collected && IsUnlocked && collector != null && DistanceTo(collector) <= pickupRadius;
        }

        public bool TryCollect()
        {
            return TryCollect(player);
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
            PlayPickup();
            return true;
        }

        public void SetCollected(bool value)
        {
            collected = value;
            ApplyState();
        }

        public void ResetReward()
        {
            collected = false;
            CollectCount = 0;
            ApplyState();
        }

        public float DistanceTo(Transform collector)
        {
            if (collector == null)
            {
                return float.PositiveInfinity;
            }

            var delta = collector.position - transform.position;
            delta.y = 0f;
            return delta.magnitude;
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

        private void PlayPickup()
        {
            EnsureAudioSource();
            if (audioSource != null && pickupClip != null)
            {
                audioSource.PlayOneShot(pickupClip);
            }
        }

        private void EnsureAudioSource()
        {
            if (audioSource != null)
            {
                return;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.78f;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }
    }
}
