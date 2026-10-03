using Partity;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace JetpackTrailRef
{
    /// 反弹写 Direction 而非 Velocity：VelocitySystem 每帧用 Speed×Direction 重算 Velocity，
    /// 改 Velocity 下一帧即被覆盖。边界对齐 Spawner.ScreenHalfExtents（x/z 平面，Y 不参与）。
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(VelocitySystem))]
    public partial struct JetpackTrailRefBounceSystem : ISystem
    {
        const float k_HalfX = 10f;
        const float k_HalfZ = 6f;

        public void OnUpdate(ref SystemState state)
        {
            foreach (var (direction, transform) in
                     SystemAPI.Query<RefRW<Direction>, RefRW<LocalTransform>>().WithAll<Particle>())
            {
                var d = direction.ValueRW.Value;
                var p = transform.ValueRW.Position;

                if (p.x < -k_HalfX && d.x < 0f) { d.x = -d.x; p.x = -k_HalfX; }
                else if (p.x > k_HalfX && d.x > 0f) { d.x = -d.x; p.x = k_HalfX; }

                if (p.z < -k_HalfZ && d.z < 0f) { d.z = -d.z; p.z = -k_HalfZ; }
                else if (p.z > k_HalfZ && d.z > 0f) { d.z = -d.z; p.z = k_HalfZ; }

                direction.ValueRW.Value = d;
                transform.ValueRW.Position = p;
            }
        }
    }
}
