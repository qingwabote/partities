#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Partity
{
    /// <summary>
    /// Creates the static trail render assets next to the demo scene: the ribbon parameter-grid
    /// mesh (once, from RibbonMeshFactory) and the ribbon material (shader Partity/TrailRibbon,
    /// instancing on, _TrailRingK baked). The history texture is NOT set on the material — it is
    /// runtime state, bound per batch through the MPB at submit time.
    /// </summary>
    public static class TrailAssetMenu
    {
        const string k_Dir = "Assets/Scenes/Projectile 14";

        [MenuItem("Partity/Create Trail Ribbon Assets")]
        public static void Create()
        {
            var mesh = RibbonMeshFactory.Create(TrailRegistry.K);
            AssetDatabase.CreateAsset(mesh, $"{k_Dir}/TrailRibbon24R.asset");

            var material = new Material(Shader.Find("Partity/TrailRibbon"))
            {
                enableInstancing = true
            };
            material.SetFloat("_TrailRingK", TrailRegistry.K);
            AssetDatabase.CreateAsset(material, $"{k_Dir}/TrailRibbon2.mat");

            AssetDatabase.SaveAssets();
            Debug.Log($"Trail ribbon assets created in {k_Dir} (K={TrailRegistry.K}).");
        }
    }
}
#endif
