using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class ExplorationTool : MonoBehaviour
    {
        public const KeyCode DefaultUseKey = KeyCode.E;
        public const KeyCode DefaultControllerUseButton = KeyCode.JoystickButton3;
        public const int DefaultMouseUseButton = 1;

        [Header("Input")]
        public KeyCode useKey = DefaultUseKey;
        public KeyCode controllerUseButton = DefaultControllerUseButton;
        public int mouseUseButton = DefaultMouseUseButton;
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

        private void Awake()
        {
            if (player == null)
            {
                player = transform;
            }

            EnsureAudioSource();

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

            if (UsePressed())
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

        public static bool IsDefaultUseKey(KeyCode keyCode)
        {
            return keyCode == DefaultUseKey || keyCode == DefaultControllerUseButton;
        }

        public bool AcceptsUseKey(KeyCode keyCode)
        {
            return keyCode == useKey || keyCode == controllerUseButton;
        }

        public bool AcceptsMouseButton(int button)
        {
            return mouseUseButton >= 0 && button == mouseUseButton;
        }

        private bool UsePressed()
        {
            return Input.GetKeyDown(useKey)
                || Input.GetKeyDown(controllerUseButton)
                || (mouseUseButton >= 0 && Input.GetMouseButtonDown(mouseUseButton));
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
            var source = EnsureAudioSource();
            if (source != null && clip != null)
            {
                source.PlayOneShot(clip);
            }
        }

        private AudioSource EnsureAudioSource()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.spatialBlend = 0f;
                audioSource.volume = 0.72f;
            }

            audioSource.playOnAwake = false;
            return audioSource;
        }

        private void OnDrawGizmosSelected()
        {
            var origin = player != null ? player.position : transform.position;
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(origin, range);
        }
    }
}
