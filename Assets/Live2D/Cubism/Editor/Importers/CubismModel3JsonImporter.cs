/*
 * Copyright(c) Live2D Inc. All rights reserved.
 *
 * Use of this source code is governed by the Live2D Open Software license
 * that can be found at https://www.live2d.com/eula/live2d-open-software-license-agreement_en.html.
 */

using Live2D.Cubism.Core;
using Live2D.Cubism.Framework;
using Live2D.Cubism.Framework.Expression;
using Live2D.Cubism.Framework.Json;
using Live2D.Cubism.Framework.Motion;
using Live2D.Cubism.Framework.MotionFade;
using Live2D.Cubism.Framework.Pose;
using Live2D.Cubism.Rendering.Masking;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Live2D.Cubism.Editor.Importers
{
    [Serializable]
    public sealed class CubismModel3JsonImporter : CubismImporterBase
    {
        [NonSerialized] private CubismModel3Json _model3Json;

        public CubismModel3Json Model3Json
        {
            get
            {
                if (_model3Json == null)
                {
                    _model3Json = CubismModel3Json.LoadAtPath(AssetPath);
                }

#if UNITY_2018_3_OR_NEWER
                if (_modelPrefab == null)
                {
                    _modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetPath.Replace(".model3.json", ".prefab"));
                    if (_modelPrefab != null)
                    {
                        _modelPrefabGuid = AssetGuid.GetGuid(_modelPrefab);
                    }
                }
#endif

                return _model3Json;
            }
        }

        [SerializeField] private string _modelPrefabGuid;
        [NonSerialized] private GameObject _modelPrefab;

        private GameObject ModelPrefab
        {
            get
            {
                if (_modelPrefab == null)
                {
                    _modelPrefab = AssetGuid.LoadAsset<GameObject>(_modelPrefabGuid);
                }

                return _modelPrefab;
            }
            set
            {
                _modelPrefab = value;
                _modelPrefabGuid = AssetGuid.GetGuid(value);
            }
        }

        [SerializeField] private string _mocAssetGuid;
        [NonSerialized] private CubismMoc _mocAsset;

        private CubismMoc MocAsset
        {
            get
            {
                if (_mocAsset == null)
                {
                    _mocAsset = AssetGuid.LoadAsset<CubismMoc>(_mocAssetGuid);
                }

                return _mocAsset;
            }
            set
            {
                _mocAsset = value;
                _mocAssetGuid = AssetGuid.GetGuid(value);
            }
        }

        private bool ShouldImportAsOriginalWorkflow => CubismUnityEditorMenu.ShouldImportAsOriginalWorkflow;

        [InitializeOnLoadMethod]
        private static void RegisterImporter()
        {
            CubismImporter.RegisterImporter<CubismModel3JsonImporter>(".model3.json");
        }

        public override void Import()
        {
            var isImporterDirty = false;
            var model = Model3Json.ToModel(CubismImporter.OnPickMaterial, CubismImporter.OnPickTexture, ShouldImportAsOriginalWorkflow);
            if (model == null)
            {
                return;
            }

            var assetPath = AssetPath.Replace(".model3.json", "");
            var modelName = Path.GetFileName(assetPath).Replace(".model3.json", "");

            var moc = model.Moc;
            moc.name = modelName;

            if (MocAsset == null)
            {
                AssetDatabase.CreateAsset(moc, $"{assetPath}.asset");
                MocAsset = moc;
                isImporterDirty = true;
            }

            if (ModelPrefab == null)
            {
                CubismImporter.SendModelImportEvent(this, model);
                foreach (var texture in Model3Json.Textures)
                {
                    CubismImporter.SendModelTextureImportEvent(this, model, texture);
                }

                var modelMaskTexture = ScriptableObject.CreateInstance<CubismMaskTexture>();
                modelMaskTexture.name = model.name + "MaskTexture";
                var filePath = string.Format("{0}/{1}.asset", Path.GetDirectoryName(AssetPath), modelMaskTexture.name);
                if (!File.Exists(filePath))
                {
                    AssetDatabase.CreateAsset(modelMaskTexture, filePath);
                }

#if UNITY_2018_3_OR_NEWER
                ModelPrefab = PrefabUtility.SaveAsPrefabAsset(model.gameObject, $"{assetPath}.prefab");
#else
                ModelPrefab = PrefabUtility.CreatePrefab($"{assetPath}.prefab", model.gameObject);
#endif

                isImporterDirty = true;
            }
            else
            {
                var cubismModel = ModelPrefab.FindCubismModel();
                if (cubismModel.Moc == null)
                {
                    CubismModel.ResetMocReference(cubismModel,
                        AssetDatabase.LoadAssetAtPath<CubismMoc>($"{assetPath}.asset"));
                }

                var source = Object.Instantiate(ModelPrefab).FindCubismModel();
                CopyUserData(source, model);
                Object.DestroyImmediate(source.gameObject, true);

                CubismImporter.SendModelImportEvent(this, model);
                foreach (var texture in Model3Json.Textures)
                {
                    CubismImporter.SendModelTextureImportEvent(this, model, texture);
                }

                CubismModel.ResetMocReference(model, MocAsset);
                model.gameObject.layer = ModelPrefab.layer;

#if UNITY_2018_3_OR_NEWER
                ModelPrefab = PrefabUtility.SaveAsPrefabAsset(model.gameObject, $"{assetPath}.prefab");
#else
                ModelPrefab = PrefabUtility.ReplacePrefab(model.gameObject, ModelPrefab, ReplacePrefabOptions.ConnectToPrefab);
#endif

                CubismImporter.LogReimport(AssetPath, AssetDatabase.GUIDToAssetPath(_modelPrefabGuid));
            }

            Object.DestroyImmediate(model.gameObject, true);

            if (MocAsset != null)
            {
                EditorUtility.CopySerialized(moc, MocAsset);
                CubismMoc.ResetUnmanagedMoc(MocAsset);
                EditorUtility.SetDirty(MocAsset);
            }

            if (isImporterDirty)
            {
                Save();
            }
            else
            {
                AssetDatabase.SaveAssets();
            }
        }

        private static void CopyUserData(CubismModel source, CubismModel destination, bool copyComponentsOnly = false)
        {
            CopyUserData(source.Parameters, destination.Parameters, copyComponentsOnly);
            CopyUserData(source.Parts, destination.Parts, copyComponentsOnly);
            CopyUserData(source.Drawables, destination.Drawables, copyComponentsOnly);

            foreach (var sourceComponent in source.GetComponents(typeof(Component)))
            {
                if (!sourceComponent.MoveOnCubismReimport(copyComponentsOnly))
                {
                    continue;
                }

                if (sourceComponent.GetType() == typeof(CubismUpdateController)
                    || sourceComponent.GetType() == typeof(CubismFadeController)
                    || sourceComponent.GetType() == typeof(CubismExpressionController)
                    || sourceComponent.GetType() == typeof(CubismPoseController)
                    || sourceComponent.GetType() == typeof(CubismParameterStore))
                {
                    continue;
                }

                var destinationComponent = destination.GetOrAddComponent(sourceComponent.GetType());
                EditorUtility.CopySerialized(sourceComponent, destinationComponent);
            }
        }

        private static void CopyUserData<T>(T[] source, T[] destination, bool copyComponentsOnly) where T : MonoBehaviour
        {
            foreach (var destinationT in destination)
            {
                var sourceT = source.FirstOrDefault(p => p.name == destinationT.name);
                if (sourceT == null)
                {
                    continue;
                }

                foreach (var child in sourceT.transform.GetComponentsInChildren<Transform>().Where(t => t != sourceT.transform).Select(t => t.gameObject))
                {
                    Object.Instantiate(child, destinationT.transform);
                }

                foreach (var sourceComponent in sourceT.GetComponents(typeof(Component)))
                {
                    if (!sourceComponent.MoveOnCubismReimport(copyComponentsOnly))
                    {
                        continue;
                    }

                    var destinationComponent = destinationT.GetOrAddComponent(sourceComponent.GetType());
                    EditorUtility.CopySerialized(sourceComponent, destinationComponent);
                }
            }
        }
    }
}
