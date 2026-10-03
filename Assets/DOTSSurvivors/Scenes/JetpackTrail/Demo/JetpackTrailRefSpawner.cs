using UnityEngine;

namespace JetpackTrailRef
{
    public class JetpackTrailRefSpawner : MonoBehaviour
    {
        public float Cooldown = 3f;
        public float IntervalBetweenAttacks = 0.2f;
        public int AttackCount = 5;
        public float DegreeAngleBetweenAttacks = 20f;
        public float MovementSpeed = 10f;
        public float TimeToLive = 2.25f;
        public float DelayCleanupTime = 1f;
        public Vector2 ScreenHalfExtents = new Vector2(10f, 6f);

        static Material s_TrailMaterial;

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
            m_BaseAngle = Random.Range(0f, Mathf.PI * 2f);
        }

        void SpawnHead(float angle)
        {
            var direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));

            var head = new GameObject("JetpackHead");
            head.transform.SetPositionAndRotation(
                transform.position, Quaternion.Euler(0f, angle * Mathf.Rad2Deg, 0f));

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(visual.GetComponent<Collider>());
            visual.name = "HeadVisual";
            visual.transform.SetParent(head.transform, false);
            visual.transform.localScale = Vector3.one * 0.35f;

            var trail = head.AddComponent<TrailRenderer>();
            trail.time = 1f;
            trail.minVertexDistance = 0.1f;
            trail.widthMultiplier = 0.2f;
            trail.widthCurve = new AnimationCurve(
                new Keyframe(0f, 1f, -0.14751776f, -0.14751776f),
                new Keyframe(1f, 0.567797f, 0f, 0f));
            var gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(new Color(1f, 0.39200467f, 0f), 0.1f),
                    new GradientColorKey(new Color(1f, 0.88631666f, 0.023584902f), 1f)
                },
                new[]
                {
                    new GradientAlphaKey(0f, 0.05f),
                    new GradientAlphaKey(1f, 0.125f),
                    new GradientAlphaKey(1f, 0.5f),
                    new GradientAlphaKey(0f, 1f)
                });
            trail.colorGradient = gradient;
            trail.numCapVertices = 5;
            trail.material = TrailMaterial;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            head.AddComponent<JetpackTrailRendererController>();
            head.AddComponent<JetpackTrailRefHead>().Init(
                direction, MovementSpeed, TimeToLive, ScreenHalfExtents, DelayCleanupTime);
        }

        static Material TrailMaterial =>
            s_TrailMaterial != null ? s_TrailMaterial : s_TrailMaterial = new Material(Shader.Find("Sprites/Default"));
    }
}
