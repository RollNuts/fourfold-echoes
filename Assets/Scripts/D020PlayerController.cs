using UnityEngine;

namespace FourfoldEchoes.Product
{
    public sealed class D020PlayerController : MonoBehaviour
    {
        [Header("Input")]
        public KeyCode attackKey = KeyCode.J;
        public KeyCode dodgeKey = KeyCode.Space;

        [Header("Movement")]
        public float moveSpeed = 3.2f;
        public float dodgeDistance = 1.15f;
        public float dodgeCooldownSeconds = 0.52f;
        public Vector2 minBounds = new Vector2(-4.15f, -2.85f);
        public Vector2 maxBounds = new Vector2(4.15f, 2.85f);

        [Header("Attack")]
        public float attackRange = 1.35f;
        public float attackCooldownSeconds = 0.34f;
        public int attackDamage = 1;
        public GameObject attackRead;

        private float attackCooldown;
        private float dodgeCooldown;
        private float attackReadTimer;
        private Vector2 lastMove = Vector2.up;

        public int AttackCount { get; private set; }
        public int AttackHitCount { get; private set; }
        public int DodgeCount { get; private set; }
        public float AttackCooldown01 => attackCooldownSeconds <= 0f ? 0f : Mathf.Clamp01(attackCooldown / attackCooldownSeconds);
        public float DodgeCooldown01 => dodgeCooldownSeconds <= 0f ? 0f : Mathf.Clamp01(dodgeCooldown / dodgeCooldownSeconds);

        private void Awake()
        {
            if (attackRead != null)
            {
                attackRead.SetActive(false);
            }
        }

        private void Update()
        {
            Tick(ReadMoveInput(), IsAttackPressed(), Input.GetKeyDown(dodgeKey), Time.deltaTime);
        }

        public void Tick(Vector2 move, bool attackPressed, bool dodgePressed, float deltaTime)
        {
            var dt = Mathf.Max(0f, deltaTime);
            attackCooldown = Mathf.Max(0f, attackCooldown - dt);
            dodgeCooldown = Mathf.Max(0f, dodgeCooldown - dt);

            if (attackReadTimer > 0f)
            {
                attackReadTimer = Mathf.Max(0f, attackReadTimer - dt);
                if (attackRead != null)
                {
                    attackRead.SetActive(true);
                }
            }
            else if (attackRead != null && attackRead.activeSelf)
            {
                attackRead.SetActive(false);
            }

            if (move.sqrMagnitude > 1f)
            {
                move.Normalize();
            }

            if (move.sqrMagnitude > 0.0001f)
            {
                lastMove = move;
                Move(move * moveSpeed * dt);
                Face(move);
            }

            if (dodgePressed)
            {
                TryDodge(move.sqrMagnitude > 0.0001f ? move : lastMove);
            }

            if (attackPressed)
            {
                TryAttack();
            }
        }

        public bool TryAttack()
        {
            if (attackCooldown > 0f)
            {
                return false;
            }

            AttackCount++;
            attackCooldown = Mathf.Max(0f, attackCooldownSeconds);
            attackReadTimer = 0.14f;
            if (attackRead != null)
            {
                attackRead.SetActive(true);
            }

            var hit = false;
            var enemies = Object.FindObjectsByType<D020EnemyDummy>(FindObjectsSortMode.None);
            for (var i = 0; i < enemies.Length; i++)
            {
                var enemy = enemies[i];
                if (enemy == null || enemy.IsDefeated)
                {
                    continue;
                }

                var delta = enemy.transform.position - transform.position;
                delta.y = 0f;
                if (delta.magnitude <= attackRange && enemy.TakeHit(attackDamage))
                {
                    AttackHitCount++;
                    hit = true;
                }
            }

            return hit;
        }

        public bool TryDodge(Vector2 direction)
        {
            if (dodgeCooldown > 0f)
            {
                return false;
            }

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = lastMove;
            }

            direction.Normalize();
            Move(direction * dodgeDistance);
            Face(direction);
            DodgeCount++;
            dodgeCooldown = Mathf.Max(0f, dodgeCooldownSeconds);
            return true;
        }

        public void ResetForSmoke(Vector3 position)
        {
            transform.position = Clamp(position);
            attackCooldown = 0f;
            dodgeCooldown = 0f;
            attackReadTimer = 0f;
            AttackCount = 0;
            AttackHitCount = 0;
            DodgeCount = 0;
            lastMove = Vector2.up;
            if (attackRead != null)
            {
                attackRead.SetActive(false);
            }
        }

        private void Move(Vector2 delta)
        {
            var position = transform.position + new Vector3(delta.x, 0f, delta.y);
            transform.position = Clamp(position);
        }

        private Vector3 Clamp(Vector3 position)
        {
            position.x = Mathf.Clamp(position.x, minBounds.x, maxBounds.x);
            position.z = Mathf.Clamp(position.z, minBounds.y, maxBounds.y);
            return position;
        }

        private void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < 0.0001f)
            {
                return;
            }

            var yaw = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, yaw, 0f);
        }

        private Vector2 ReadMoveInput()
        {
            var x = 0f;
            var y = 0f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) x += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) y -= 1f;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) y += 1f;
            return new Vector2(x, y);
        }

        private bool IsAttackPressed()
        {
            return Input.GetKeyDown(attackKey) || Input.GetMouseButtonDown(0);
        }
    }
}
