using UnityEngine;

namespace JetpackTrailRef
{
    public class JetpackTrailRefSpawner : MonoBehaviour
    {
        public JetpackTrailRefHead HeadPrefab;
        public float Cooldown = 3f;
        public float IntervalBetweenAttacks = 0.2f;
        public int AttackCount = 5;
        public float DegreeAngleBetweenAttacks = 20f;
        public bool UseFixedBaseAngle;
        public float FixedBaseAngleDegrees = 90f;
        public float MovementSpeed = 10f;
        public float TimeToLive = 2.25f;
        public float DelayCleanupTime = 1f;
        public Vector2 ScreenHalfExtents = new Vector2(10f, 6f);

        float m_CooldownTimer;
        float m_NextAttackTimer;
        int m_AttackCount;
        float m_BaseAngle;

        void Update()
        {
            var deltaTime = Time.deltaTime;
            if (m_AttackCount == 0)
            {
                m_CooldownTimer -= deltaTime;
                if (m_CooldownTimer > 0f) return;
            }

            m_NextAttackTimer -= deltaTime;
            if (m_NextAttackTimer > 0f) return;

            if (m_AttackCount == 0) PickBaseAngle();

            SpawnHead(m_BaseAngle + Mathf.Deg2Rad * DegreeAngleBetweenAttacks * m_AttackCount);

            m_NextAttackTimer = IntervalBetweenAttacks;
            m_AttackCount++;
            if (m_AttackCount < AttackCount) return;

            m_AttackCount = 0;
            m_NextAttackTimer = 0f;
            m_CooldownTimer = Cooldown;
        }

        void PickBaseAngle()
        {
            m_BaseAngle = UseFixedBaseAngle
                ? FixedBaseAngleDegrees * Mathf.Deg2Rad
                : Random.Range(0f, Mathf.PI * 2f);
        }

        void SpawnHead(float angle)
        {
            var direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));

            var head = Instantiate(HeadPrefab, transform.position, Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f));
            head.Init(direction, MovementSpeed, TimeToLive, ScreenHalfExtents, DelayCleanupTime);
        }
    }
}
