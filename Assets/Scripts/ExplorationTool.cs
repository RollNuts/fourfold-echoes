using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class ExplorationTool : MonoBehaviour
    {
        [Header("Input")]
        public KeyCode useKey = KeyCode.E;
        public float range = 2.7f;
        public float cooldownSeconds = 0.45f;

        [Header("Scene References")]
        public Transform player;
        public ExplorationNode[] nodes;
        public GameObject pulseRead;

        [Header("Audio")]
        public AudioClip pulse;
        public AudioClip targetHit;
        public AudioClip fail;

        private AudioSource audioSource;
        private float cooldownTimer;
        private float pulseTimer;

        public bool IsReady => cooldownTimer <= 0f;
        public float Cooldown01 => cooldownSeconds <= 0f ? 0f : Mathf.Clamp01(cooldownTimer / cooldownSeconds);
        public int NodeCount => nodes == null ? 0 : nodes.Length;

        private void Awake()
        {
            if (player == null)
            {
                player = transform;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0f;
                audioSource.volume = 0.72f;
            }

            if (pulseRead != null)
            {
                pulseRead.SetActive(false);
            }
        }

        private void Update()
        {
            if (cooldownTimer > 0f)
            {
                cooldownTimer = Mathf.Max(0f, cooldownTimer - Time.deltaTime);
            }

            if (pulseTimer > 0f)
            {
                pulseTimer -= Time.deltaTime;
                if (pulseRead != null)
                {
                    var scale = 1f + Mathf.Sin((0.18f - pulseTimer) * 18f) * 0.08f;
                    pulseRead.transform.localScale = new Vector3(scale, 0.035f, scale);
                    pulseRead.SetActive(true);
                }
            }
            else if (pulseRead != null && pulseRead.activeSelf)
            {
                pulseRead.SetActive(false);
            }

            if (Input.GetKeyDown(useKey))
            {
                TryUse();
            }
        }

        public bool TryUse()
        {
            if (!IsReady)
            {
                return false;
            }

            cooldownTimer = Mathf.Max(0f, cooldownSeconds);
            pulseTimer = 0.18f;
            Play(pulse);

            var node = FindBestNode();
            if (node != null && node.TryActivate(player, range))
            {
                Play(targetHit);
                return true;
            }

            Play(fail);
            return false;
        }

        public void ResetForSmoke()
        {
            cooldownTimer = 0f;
            pulseTimer = 0f;
            if (pulseRead != null)
            {
                pulseRead.SetActive(false);
            }
        }

        private ExplorationNode FindBestNode()
        {
            if (nodes == null)
            {
                return null;
            }

            ExplorationNode best = null;
            var bestDistance = float.PositiveInfinity;
            for (var i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                if (node == null || !node.CanUse(player, range))
                {
                    continue;
                }

                var distance = node.DistanceTo(player);
                if (distance < bestDistance)
                {
                    best = node;
                    bestDistance = distance;
                }
            }

            return best;
        }

        private void Play(AudioClip clip)
        {
            if (audioSource != null && clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }

        private void OnDrawGizmosSelected()
        {
            var origin = player != null ? player.position : transform.position;
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(origin, range);
        }
    }
}
