using System;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public enum MeleeShardlingAnimationCue
    {
        BaseContactLeft,
        BaseContactRight,
        AttackWindup,
        HitActiveStart,
        HitPeak,
        HitActiveEnd,
        LoopPressurePulse,
        Recover,
        HitVfx,
        ArmorClack,
        GroundContact,
        StunOpen,
        DeathStart,
        DeathVfx,
        HideAllowed,
        CastCharge,
        CastReady,
        ChannelPulse,
        ProjectileRelease,
        CastRecover,
        InteractContact,
        InteractVfx,
    }

    public struct MeleeShardlingAnimationCueEvent
    {
        public MeleeShardlingAnimationCueEvent(MeleeShardlingAnimationCue cue, string eventName, int eventCount)
        {
            Cue = cue;
            EventName = eventName;
            EventCount = eventCount;
        }

        public MeleeShardlingAnimationCue Cue { get; }
        public string EventName { get; }
        public int EventCount { get; }
    }

    [DisallowMultipleComponent]
    public sealed class MeleeShardlingAnimationEventRelay : MonoBehaviour
    {
        [SerializeField] private Collider forwardHitbox;

        public event Action<MeleeShardlingAnimationCueEvent> CueRaised;

        public string LastEventName { get; private set; } = string.Empty;
        public MeleeShardlingAnimationCue LastCue { get; private set; }
        public bool HasRaisedCue { get; private set; }
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
            Record(MeleeShardlingAnimationCue.BaseContactLeft, "base_contact_left");
        }

        public void base_contact_right()
        {
            Record(MeleeShardlingAnimationCue.BaseContactRight, "base_contact_right");
        }

        public void attack_windup()
        {
            Record(MeleeShardlingAnimationCue.AttackWindup, "attack_windup");
        }

        public void hit_active_start()
        {
            HitActiveCount++;
            SetForwardHitbox(true);
            Record(MeleeShardlingAnimationCue.HitActiveStart, "hit_active_start");
        }

        public void hit_peak()
        {
            Record(MeleeShardlingAnimationCue.HitPeak, "hit_peak");
        }

        public void hit_active_end()
        {
            SetForwardHitbox(false);
            Record(MeleeShardlingAnimationCue.HitActiveEnd, "hit_active_end");
        }

        public void loop_pressure_pulse()
        {
            Record(MeleeShardlingAnimationCue.LoopPressurePulse, "loop_pressure_pulse");
        }

        public void recover()
        {
            SetForwardHitbox(false);
            Record(MeleeShardlingAnimationCue.Recover, "recover");
        }

        public void hit_vfx()
        {
            Record(MeleeShardlingAnimationCue.HitVfx, "hit_vfx");
        }

        public void armor_clack()
        {
            Record(MeleeShardlingAnimationCue.ArmorClack, "armor_clack");
        }

        public void ground_contact()
        {
            Record(MeleeShardlingAnimationCue.GroundContact, "ground_contact");
        }

        public void stun_open()
        {
            Record(MeleeShardlingAnimationCue.StunOpen, "stun_open");
        }

        public void death_start()
        {
            SetForwardHitbox(false);
            Record(MeleeShardlingAnimationCue.DeathStart, "death_start");
        }

        public void death_vfx()
        {
            Record(MeleeShardlingAnimationCue.DeathVfx, "death_vfx");
        }

        public void hide_allowed()
        {
            Record(MeleeShardlingAnimationCue.HideAllowed, "hide_allowed");
        }

        public void cast_charge()
        {
            Record(MeleeShardlingAnimationCue.CastCharge, "cast_charge");
        }

        public void cast_ready()
        {
            Record(MeleeShardlingAnimationCue.CastReady, "cast_ready");
        }

        public void channel_pulse()
        {
            Record(MeleeShardlingAnimationCue.ChannelPulse, "channel_pulse");
        }

        public void projectile_release()
        {
            CastReleaseCount++;
            Record(MeleeShardlingAnimationCue.ProjectileRelease, "projectile_release");
        }

        public void cast_recover()
        {
            Record(MeleeShardlingAnimationCue.CastRecover, "cast_recover");
        }

        public void interact_contact()
        {
            Record(MeleeShardlingAnimationCue.InteractContact, "interact_contact");
        }

        public void interact_vfx()
        {
            Record(MeleeShardlingAnimationCue.InteractVfx, "interact_vfx");
        }

        private void Record(MeleeShardlingAnimationCue cue, string eventName)
        {
            LastCue = cue;
            LastEventName = eventName;
            EventCount++;
            HasRaisedCue = true;
            CueRaised?.Invoke(new MeleeShardlingAnimationCueEvent(cue, eventName, EventCount));
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
