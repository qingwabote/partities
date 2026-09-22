using Unity.Entities;
using UnityEngine;

namespace PartityCrateDemo
{
    public enum CrateEmitterType
    {
        LeftCoin,
        RightCoin,
        Dots,
    }

    public struct EffectEmitterPrefabs : IComponentData
    {
        public Entity LeftCoin;
        public Entity RightCoin;
        public Entity Dots;
        public Entity FallingGems;
    }

#if UNITY_EDITOR
    public class EffectEmitterPrefabsAuthoring : MonoBehaviour
    {
        public GameObject LeftCoinEmitterPrefab;
        public GameObject RightCoinEmitterPrefab;
        public GameObject DotsEmitterPrefab;
        public GameObject FallingGemsEmitterPrefab;

        private class Baker : Baker<EffectEmitterPrefabsAuthoring>
        {
            public override void Bake(EffectEmitterPrefabsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                DependsOn(authoring.LeftCoinEmitterPrefab);
                DependsOn(authoring.RightCoinEmitterPrefab);
                DependsOn(authoring.DotsEmitterPrefab);
                DependsOn(authoring.FallingGemsEmitterPrefab);
                AddComponent(entity, new EffectEmitterPrefabs
                {
                    LeftCoin = GetEntity(authoring.LeftCoinEmitterPrefab, TransformUsageFlags.Dynamic),
                    RightCoin = GetEntity(authoring.RightCoinEmitterPrefab, TransformUsageFlags.Dynamic),
                    Dots = GetEntity(authoring.DotsEmitterPrefab, TransformUsageFlags.Dynamic),
                    FallingGems = GetEntity(authoring.FallingGemsEmitterPrefab, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
#endif
}
