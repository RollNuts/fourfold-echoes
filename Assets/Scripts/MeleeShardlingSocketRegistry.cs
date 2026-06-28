using System;
using System.Collections.Generic;
using UnityEngine;

namespace FourfoldEchoes.Product
{
    public enum MeleeShardlingSocketRole
    {
        Ground,
        ChestCore,
        WeakPoint,
        Back,
        AttackOrigin,
        ForwardHit,
        RedSeamVfx,
        Cast,
        HitVfx,
    }

    [Serializable]
    public struct MeleeShardlingSocketBinding
    {
        [SerializeField] private MeleeShardlingSocketRole role;
        [SerializeField] private string socketName;
        [SerializeField] private Transform socketTransform;

        public MeleeShardlingSocketBinding(MeleeShardlingSocketRole role, string socketName, Transform socketTransform)
        {
            this.role = role;
            this.socketName = socketName;
            this.socketTransform = socketTransform;
        }

        public MeleeShardlingSocketRole Role => role;
        public string SocketName => socketName;
        public Transform SocketTransform => socketTransform;
    }

    [DisallowMultipleComponent]
    public sealed class MeleeShardlingSocketRegistry : MonoBehaviour
    {
        [SerializeField] private MeleeShardlingSocketBinding[] sockets = Array.Empty<MeleeShardlingSocketBinding>();

        public IReadOnlyList<MeleeShardlingSocketBinding> Sockets => sockets;
        public int SocketCount => sockets.Length;

        public void ConfigureForRuntime(MeleeShardlingSocketBinding[] bindings)
        {
            sockets = bindings == null ? Array.Empty<MeleeShardlingSocketBinding>() : (MeleeShardlingSocketBinding[])bindings.Clone();
        }

        public bool TryGetSocket(MeleeShardlingSocketRole role, out Transform socketTransform)
        {
            for (var index = 0; index < sockets.Length; index++)
            {
                if (sockets[index].Role == role && sockets[index].SocketTransform != null)
                {
                    socketTransform = sockets[index].SocketTransform;
                    return true;
                }
            }

            socketTransform = null;
            return false;
        }

        public bool TryGetSocket(string socketName, out Transform socketTransform)
        {
            for (var index = 0; index < sockets.Length; index++)
            {
                if (string.Equals(sockets[index].SocketName, socketName, StringComparison.Ordinal) && sockets[index].SocketTransform != null)
                {
                    socketTransform = sockets[index].SocketTransform;
                    return true;
                }
            }

            socketTransform = null;
            return false;
        }
    }
}
