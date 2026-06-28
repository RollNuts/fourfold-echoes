using UnityEngine;

namespace FourfoldEchoes.Product
{
    [DisallowMultipleComponent]
    public sealed class MeleeShardlingAnimationEventRelay : MonoBehaviour
    {
        [SerializeField] private Collider forwardHitbox;

        public string LastEventName { get; private set; } = string.Empty;
        public int EventCount { get; private set; }
        public int HitActiveCount { get; private set; }
        public int CastReleaseCount { get; private set; }
        public Collider ForwardHitbox => forwardHitbox;

        public void BindForwardHitbox(Collider collider)
        {
            forwardHitbox = collider;
            SetForwardHitbox(false);
        }

        private void Awake()
        {
            SetForwardHitbox(false);
        }

        private void OnDisable()
        {
            SetForwardHitbox(false);
        }

        public void base_contact_left()
        {
            Record("base_contact_left");
        }

        public void base_contact_right()
        {
            Record("base_contact_right");
        }

        public void attack_windup()
        {
            Record("attack_windup");
        }

        public void hit_active_start()
        {
            HitActiveCount++;
            SetForwardHitbox(true);
            Record("hit_active_start");
        }

        public void hit_peak()
        {
            Record("hit_peak");
        }

        public void hit_active_end()
        {
            SetForwardHitbox(false);
            Record("hit_active_end");
        }

        public void loop_pressure_pulse()
        {
            Record("loop_pressure_pulse");
        }

        public void recover()
        {
            SetForwardHitbox(false);
            Record("recover");
        }

        public void hit_vfx()
        {
            Record("hit_vfx");
        }

        public void armor_clack()
        {
            Record("armor_clack");
        }

        public void ground_contact()
        {
            Record("ground_contact");
        }

        public void stun_open()
        {
            Record("stun_open");
        }

        public void death_start()
        {
            SetForwardHitbox(false);
            Record("death_start");
        }

        public void death_vfx()
        {
            Record("death_vfx");
        }

        public void hide_allowed()
        {
            Record("hide_allowed");
        }

        public void cast_charge()
        {
            Record("cast_charge");
        }

        public void cast_ready()
        {
            Record("cast_ready");
        }

        public void channel_pulse()
        {
            Record("channel_pulse");
        }

        public void projectile_release()
        {
            CastReleaseCount++;
            Record("projectile_release");
        }

        public void cast_recover()
        {
            Record("cast_recover");
        }

        public void interact_contact()
        {
            Record("interact_contact");
        }

        public void interact_vfx()
        {
            Record("interact_vfx");
        }

        private void Record(string eventName)
        {
            LastEventName = eventName;
            EventCount++;
        }

        private void SetForwardHitbox(bool enabled)
        {
            if (forwardHitbox != null)
            {
                forwardHitbox.enabled = enabled;
            }
        }
    }
}
