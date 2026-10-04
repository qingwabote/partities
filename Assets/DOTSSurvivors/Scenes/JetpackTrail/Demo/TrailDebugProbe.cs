using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

namespace JetpackTrailRef
{
    /// Single-particle trail debug rig. No physics: the particle walks a hard-coded
    /// frame-aligned waypoint table (one waypoint per frame, ping-pong at the ends), so every
    /// frame-final position IS a waypoint — the recorded polyline equals the authored path
    /// exactly, V vertex included, no sampling-phase kinks. The gizmo overlay reads the same
    /// stream back for at-a-glance attribution against the rendered ribbon.
    /// </summary>
    public class TrailDebugProbe : MonoBehaviour
    {
        public Vector3[] Path = BuildDefaultPath();
        public float Lifetime = 20f;
        public float TrailTtl = 0.375f;
        public float MinVertexDistance = 0.05f;
        public float WidthOverTrail = 0.45f;
        public Mesh Mesh;
        public Material Material;

        Entity m_Entity;
        bool m_Spawned;
        int m_PathIndex;
        int m_PathStep = 1;

        /// V with the same geometry as the old bounce (arms ±66.4° off +x, vertex at origin):
        /// 12 waypoints per arm at 0.15 spacing — every vertex pass is an exact 132.8° turn.
        static Vector3[] BuildDefaultPath()
        {
            var vertex = new Vector3(0f, 0.5f, 0f);
            var dirIn = new Vector3(0.4f, 0f, -0.917f);
            var dirOut = new Vector3(0.4f, 0f, 0.917f);
            const int arm = 12;
            const float spacing = 0.15f;
            var path = new Vector3[arm * 2 + 1];
            for (int i = 0; i <= arm; i++)
                path[i] = vertex - dirIn * (spacing * (arm - i));
            for (int i = 1; i <= arm; i++)
                path[arm + i] = vertex + dirOut * (spacing * i);
            return path;
        }

        void Update()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;
            if (!Application.isPlaying) return;

            var em = world.EntityManager;
            if (!m_Spawned || !em.Exists(m_Entity))
            {
                // death respawn: the rig runs its waypoint loop indefinitely
                m_Spawned = true;
                m_PathIndex = 0;
                m_PathStep = 1;
                Spawn(world);
            }

            if (Path == null || Path.Length < 2) return;

            m_PathIndex += m_PathStep;
            if (m_PathIndex >= Path.Length) { m_PathIndex = Path.Length - 2; m_PathStep = -1; }
            else if (m_PathIndex < 0) { m_PathIndex = 1; m_PathStep = 1; }

            em.SetComponentData(m_Entity, new LocalTransform
            {
                Position = Path[m_PathIndex],
                Rotation = quaternion.identity,
                Scale = 1f,
            });
        }

        [ContextMenu("Respawn")]
        public void Spawn()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            if (world != null)
            {
                m_Spawned = true;
                m_PathIndex = 0;
                m_PathStep = 1;
                Spawn(world);
            }
        }

        void Spawn(World world)
        {
            var em = world.EntityManager;

            using var old = em.CreateEntityQuery(typeof(Partity.TrailRenderer))
                .ToEntityArray(Allocator.Temp);
            foreach (var e in old)
                em.DestroyEntity(e);

            var start = Path != null && Path.Length > 0 ? Path[0] : Vector3.zero;
            m_Entity = em.CreateEntity();
            em.AddComponentData(m_Entity, new LocalTransform
            {
                Position = start,
                Rotation = quaternion.identity,
                Scale = 1f,
            });
            em.AddComponentData(m_Entity, new Partity.Particle
            {
                Position = start,
                Rotation = quaternion.identity,
                Scale = 1f,
                Lerp = 0.5f,
            });
            em.AddComponentData(m_Entity, new Partity.Lifetime { Life = Lifetime, Time = 0f });
            em.AddComponentData(m_Entity, new Partity.TrailRenderer
            {
                Lifetime = TrailTtl,
                MinVertexDistance = MinVertexDistance,
                WidthOverTrail = WidthOverTrail,
                Mesh = Mesh,
                Material = Material,
            });
            em.AddComponentData(m_Entity, new Partity.TrailState
            {
                LastPosition = new float3(float.MaxValue),
            });
            em.AddComponentData(m_Entity, new Partity.TrailData());
            em.AddComponentData(m_Entity, new URPMaterialPropertyBaseColor
            {
                Value = new float4(0f, 0.76f, 2f, 1f),
            });
            em.AddComponentData(m_Entity, new LocalToWorld
            {
                // runtime-built entity: TransformSystemGroup only updates an existing
                // LocalToWorld, baking is what normally adds it
                Value = float4x4.Translate(start),
            });
        }

        void OnDrawGizmos()
        {
            // the authored path as a reference (white), always visible in edit mode too
            if (Path != null && Path.Length > 1)
            {
                Gizmos.color = Color.white;
                for (int i = 0; i < Path.Length - 1; i++)
                    Gizmos.DrawLine(Path[i], Path[i + 1]);
            }

            if (!Application.isPlaying) return;
            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null) return;
            var trs = world.GetExistingSystemManaged<Partity.TrailRenderSystem>();
            if (trs == null) return;
            var em = world.EntityManager;

            using var ents = em.CreateEntityQuery(typeof(Partity.TrailData))
                .ToEntityArray(Allocator.Temp);
            var arr = trs.Stream.Read.Source.Value;
            foreach (var e in ents)
            {
                var d = em.GetComponentData<Partity.TrailData>(e);
                int b = (int)d.Base, c = (int)d.Count;
                if (c < 2) continue;

                var prev = float3.zero;
                for (int i = 0; i < c; i++)
                {
                    int t = (b + i) * 4;
                    var p = new float3(arr[t], arr[t + 1], arr[t + 2]);
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(p, 0.05f);
                    if (i > 0)
                    {
                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(prev, p);
                    }
                    prev = p;
                }

                if (!em.HasComponent<LocalToWorld>(e)) continue;
                var head = em.GetComponentData<LocalToWorld>(e).Position;
                int n = (b + c - 1) * 4;
                var newest = new float3(arr[n], arr[n + 1], arr[n + 2]);
                Gizmos.color = Color.red;
                Gizmos.DrawSphere(head, 0.1f);
                // the chord the shader's head extrapolation replaces
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(newest, head);
            }
        }
    }
}
