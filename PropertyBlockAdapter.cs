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
        public MeshRenderer[] renderers;
        public int[] arrayIndex;

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
                renderers[i].SetPropertyBlock(propertyBlock);
            }

            gameObject.SetActive(false);
        }
    }
    #endif

    #if !COMPILER_UDONSHARP && UNITY_EDITOR
    public class PropertyBlockAdapterEditor : Editor
    {
        [MenuItem("z3y/Update PropertyBlockAdapter")]
        public static void SetProperties()
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
            var renderersEditor = FindObjectsOfType<MeshRenderer>();
            var renderersEditorClean = new List<MeshRenderer>();
            var arrayIndex = new List<int>();

            for (int i = 0; i < renderersEditor.Length; i++)
            {
                var b = new MaterialPropertyBlock();
                renderersEditor[i].GetPropertyBlock(b);

                int idx = (int) b.GetFloat("_TextureIndex");

                if (idx != 0)
                {
                    for (int j = 0; j < renderersEditor[i].sharedMaterials.Length; j++)
                    {
                        Material mat = renderersEditor[i].sharedMaterials[j];
                        if (mat == null) continue;
                        renderersEditorClean.Add(renderersEditor[i]);
                        arrayIndex.Add(idx);
                    }
                }

            }

            pba.UpdateProxy();
            pba.renderers = renderersEditorClean.ToArray();
            pba.arrayIndex = arrayIndex.ToArray();
            pba.ApplyProxyModifications();
        }
    }

    public class SetInstancedArrayProperties : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 60;

        bool IVRCSDKBuildRequestedCallback.OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            PropertyBlockAdapterEditor.SetProperties();
            return true;
        }
    }
    #endif
}
#endif