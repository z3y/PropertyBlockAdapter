#if UDON
using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections.Generic;
using System.Configuration;
using UdonSharpEditor;
using UnityEditor;
using VRC.SDKBase.Editor.BuildPipeline;
#endif

namespace z3y.Shaders
{
    #if UDON
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PropertyBlockAdapter : UdonSharpBehaviour
    {
        public Renderer[] renderers;
        public int[] arrayIndex;
        public Color[] baseColors;
        [ColorUsage(false, true)] public Color[] emissionColors;
        public Vector4[] tileOffsets;

        void Start()
        {
            #if UNITY_EDITOR
            return;
            #endif
            for (int i = 0; i < renderers.Length; i++)
            {
                var propertyBlock = new MaterialPropertyBlock();
                
                if (renderers[i].HasPropertyBlock())
                {
                    renderers[i].GetPropertyBlock(propertyBlock);
                }
                propertyBlock.SetFloat("_TextureIndex", arrayIndex[i]);
                propertyBlock.SetColor("_Color", baseColors[i]);
                propertyBlock.SetColor("_EmissionColor", emissionColors[i]);
                propertyBlock.SetVector("_MainTex_ST", tileOffsets[i]);
                renderers[i].SetPropertyBlock(propertyBlock);
            }

            gameObject.SetActive(false);
        }
    }
#endif

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(PropertyBlockAdapter))]
    public class PropertyBlockAdapterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target))
            {
                return;
            }

            if (GUILayout.Button("Update Properties"))
            {
                SetProperties((PropertyBlockAdapter)target);
            }

            base.OnInspectorGUI();
        }

        // [MenuItem("z3y/Update PropertyBlockAdapter")]
        public static void SetPropertiesStatic()
        {
            var obj = GameObject.Find("PropertyBlockAdapter");
            if (obj == null)
            {
                return;
            }

            if (PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab)
            {
                PrefabUtility.UnpackPrefabInstance(obj, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }

            var pba = obj.GetUdonSharpComponent<PropertyBlockAdapter>();
            SetProperties(pba);
        }
        public static void SetProperties(PropertyBlockAdapter pba)
        {
            var renderersEditor = FindObjectsOfType<Renderer>();

            var renderersWithPropertyBlock = new List<Renderer>();
            var arrayIndex = new List<int>();
            var baseColors = new List<Color>();
            var emissionColors = new List<Color>();
            var tileOffsets = new List<Vector4>();

            for (int i = 0; i < renderersEditor.Length; i++)
            {
                var renderer = renderersEditor[i];
                if (renderer.GetComponent(typeof(InstancedPropertyBlocks)) == null || renderer.sharedMaterial == null)
                {
                    continue;
                }
                
                var block = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(block);

                renderersWithPropertyBlock.Add(renderer);
                arrayIndex.Add(block.GetInt("_TextureIndex"));
                baseColors.Add(block.GetColor("_Color"));
                emissionColors.Add(block.GetColor("_EmissionColor"));
                tileOffsets.Add(block.GetVector("_MainTex_ST"));
            }

            pba.UpdateProxy();
            pba.renderers = renderersWithPropertyBlock.ToArray();
            pba.arrayIndex = arrayIndex.ToArray();
            pba.tileOffsets = tileOffsets.ToArray();
            pba.baseColors = baseColors.ToArray();
            pba.emissionColors = emissionColors.ToArray();
            pba.ApplyProxyModifications();
        }
    }

    public class SetInstancedArrayProperties : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 60;

        bool IVRCSDKBuildRequestedCallback.OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            PropertyBlockAdapterEditor.SetPropertiesStatic();
            return true;
        }
    }
    #endif
}
#endif