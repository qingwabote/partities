using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace PartityCrateDemo
{
    public class CrateEmitter : MonoBehaviour
    {
        private const float VIEW_DEPTH_IN_FRONT_OF_CAMERA = 3f;
        private const float VIEW_DEPTH_BEHIND_CANVAS_PLANE_FOR_COINS = -99.5f;

        [SerializeField] private float _emitterScale = 1f;
        [SerializeField] private CrateEmitterType _type;
        [SerializeField] private Partity.EntityWorld _effectWorld;

        private int _enableGeneration;

        private async void OnEnable()
        {
            if (_effectWorld == null)
            {
                Debug.LogError($"{nameof(CrateEmitter)} on {name} has no {nameof(Partity.EntityWorld)} assigned.");
                return;
            }

            _effectWorld.enabled = true;
            var generation = ++_enableGeneration;
            var world = await _effectWorld.Value;
            if (this == null || generation != _enableGeneration || !isActiveAndEnabled) return;

            InstantiateEmitter(world);
        }

        private void OnDisable()
        {
            Stop();
        }

        public void Stop()
        {
            if (_effectWorld == null) return;

            var ready = _effectWorld.Value;
            if (!ready.IsCompleted)
            {
                _effectWorld.enabled = false;
                return;
            }

            var entityManager = ready.Result.EntityManager;
            entityManager.DestroyEntity(entityManager.CreateEntityQuery(new EntityQueryBuilder(Allocator.Temp)
                .WithAll<SceneTag>()
                .WithNone<Prefab, EffectEmitterPrefabs>()));
            _effectWorld.enabled = false;
        }

        private void InstantiateEmitter(World world)
        {
            var entityManager = world.EntityManager;
            var prefabs = entityManager.CreateEntityQuery(typeof(EffectEmitterPrefabs)).GetSingleton<EffectEmitterPrefabs>();
            var prefab = Entity.Null;
            switch (_type)
            {
                case CrateEmitterType.LeftCoin:
                    prefab = prefabs.LeftCoin;
                    break;
                case CrateEmitterType.RightCoin:
                    prefab = prefabs.RightCoin;
                    break;
                case CrateEmitterType.Dots:
                    prefab = prefabs.Dots;
                    break;
            }
            if (prefab == Entity.Null)
                throw new InvalidOperationException($"Baked {nameof(EffectEmitterPrefabs)} carries no prefab for emitter type {_type}.");

            var emitterEntity = entityManager.Instantiate(prefab);

            var mainCamera = Camera.main;
            var position = transform.position;
            if (mainCamera != null)
            {
                var viewDepth = _type == CrateEmitterType.Dots
                    ? VIEW_DEPTH_IN_FRONT_OF_CAMERA
                    : VIEW_DEPTH_BEHIND_CANVAS_PLANE_FOR_COINS;
                var forward = mainCamera.transform.forward;
                var cameraToEmitter = position - mainCamera.transform.position;
                var perpendicular = cameraToEmitter - forward * Vector3.Dot(cameraToEmitter, forward);
                position = mainCamera.transform.position + forward * viewDepth + perpendicular;
            }
            entityManager.SetComponentData(emitterEntity, new LocalTransform
            {
                Position = position,
                Rotation = transform.rotation,
                Scale = _emitterScale,
            });
        }
    }
}
