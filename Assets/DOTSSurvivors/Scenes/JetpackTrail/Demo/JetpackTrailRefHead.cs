using UnityEngine;

namespace JetpackTrailRef
{
    public class JetpackTrailRefHead : MonoBehaviour
    {
        const float k_BouncePadding = 0.15f;

        Vector3 m_Direction;
        float m_Speed;
        float m_TimeToLive;
        float m_DelayCleanupTime;
        Vector2 m_HalfExtents;
        bool m_Dead;

        public void Init(Vector3 direction, float speed, float timeToLive, Vector2 halfExtents, float delayCleanupTime)
        {
            m_Direction = direction;
            m_Speed = speed;
            m_TimeToLive = timeToLive;
            m_HalfExtents = halfExtents;
            m_DelayCleanupTime = delayCleanupTime;
        }

        void Update()
        {
            if (m_Dead) return;

            var deltaTime = Time.deltaTime;

            if (Physics.Raycast(transform.position, m_Direction, out var hit, 0.5f,
                    Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore) &&
                hit.collider.TryGetComponent<JetpackTrailRefObstacle>(out _))
            {
                var normal = hit.normal;
                normal.y = 0f;
                normal.Normalize();
                if (Vector3.Dot(m_Direction, normal) < 0f)
                {
                    m_Direction = Vector3.Reflect(m_Direction, normal).normalized;
                    transform.position -= normal * k_BouncePadding;
                }
            }

            transform.position += m_Direction * (m_Speed * deltaTime);

            var position = transform.position;
            var bounced = false;
            if (position.x < -m_HalfExtents.x)
            {
                position.x = -m_HalfExtents.x + k_BouncePadding;
                m_Direction.x = -m_Direction.x;
                bounced = true;
            }
            else if (position.x > m_HalfExtents.x)
            {
                position.x = m_HalfExtents.x - k_BouncePadding;
                m_Direction.x = -m_Direction.x;
                bounced = true;
            }

            if (position.z < -m_HalfExtents.y)
            {
                position.z = -m_HalfExtents.y + k_BouncePadding;
                m_Direction.z = -m_Direction.z;
                bounced = true;
            }
            else if (position.z > m_HalfExtents.y)
            {
                position.z = m_HalfExtents.y - k_BouncePadding;
                m_Direction.z = -m_Direction.z;
                bounced = true;
            }

            if (bounced) transform.position = position;

            m_TimeToLive -= deltaTime;
            if (m_TimeToLive > 0f) return;

            m_Dead = true;
            GetComponent<JetpackTrailRendererController>()?.EndTrailRenderer();
            Destroy(gameObject, m_DelayCleanupTime);
        }
    }
}
