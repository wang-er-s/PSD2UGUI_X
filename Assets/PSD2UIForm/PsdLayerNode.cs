/*
    联系作者:
    https://blog.csdn.net/final5788
    https://github.com/sunsvip
 */

#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Aspose.PSD.FileFormats.Png;
using Aspose.PSD.FileFormats.Psd;
using Aspose.PSD.FileFormats.Psd.Layers;
using Aspose.PSD.FileFormats.Psd.Layers.SmartObjects;
using Aspose.PSD.ImageOptions;
using UnityEditor;
using UnityEngine;

namespace UGF.EditorTools.Psd2UGUI
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(PsdLayerNode))]
    public class PsdLayerNodeInspector : Editor
    {
        private PsdLayerNode targetLogic;

        private void OnEnable()
        {
            targetLogic = target as PsdLayerNode;
            targetLogic.RefreshLayerTexture();
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            base.OnInspectorGUI();
            EditorGUI.BeginChangeCheck();
            {
                targetLogic.UIType = (GUIType)EditorGUILayout.EnumPopup("UI Type", targetLogic.UIType);
                if (EditorGUI.EndChangeCheck()) targetLogic.SetUIType(targetLogic.UIType);
            }
            serializedObject.ApplyModifiedProperties();
        }

        public override bool HasPreviewGUI()
        {
            var layerNode = target as PsdLayerNode;
            return layerNode != null && layerNode.PreviewTexture != null;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            var layerNode = target as PsdLayerNode;
            GUI.DrawTexture(r, layerNode.PreviewTexture, ScaleMode.ScaleToFit);
            //base.OnPreviewGUI(r, background);
        }

        public override string GetInfoString()
        {
            var layerNode = target as PsdLayerNode;
            return layerNode.LayerInfo;
        }
    }

    [ExecuteInEditMode]
    [DisallowMultipleComponent]
    public partial class PsdLayerNode : MonoBehaviour
    {
        [ReadOnlyField]
        public int BindPsdLayerIndex = -1;

        [ReadOnlyField]
        [SerializeField]
        private PsdLayerType mLayerType = PsdLayerType.Unknown;

        [SerializeField]
        public bool MarkToExportImage;

        [HideInInspector]
        public GUIType UIType;

        /// <summary>
        ///     绑定的psd图层
        /// </summary>
        private Layer mBindPsdLayer;

        public Texture2D PreviewTexture { get; private set; }
        public string LayerInfo { get; private set; }
        public Rect LayerRect { get; private set; }

        public PsdLayerType LayerType => mLayerType;

        public bool IsMainUIType => UGUIParser.IsMainUIType(UIType);

        public Layer BindPsdLayer
        {
            get => mBindPsdLayer;
            set
            {
                mBindPsdLayer = value;
                mLayerType = mBindPsdLayer.GetLayerType();
                LayerRect = mBindPsdLayer.GetLayerRect();
                LayerInfo = $"{LayerRect}";
            }
        }

        private void OnDestroy()
        {
            if (PreviewTexture != null) DestroyImmediate(PreviewTexture);
        }

        public void SetUIType(GUIType uiType, bool triggerParseFunc = true)
        {
            UIType = uiType;
            RemoveUIHelper();

            if (triggerParseFunc) RefreshUIHelper(true);
        }

        public void RefreshUIHelper(bool refreshParent = false)
        {
            if (UIType == GUIType.Null) return;

            var uiHelperTp = UGUIParser.Instance.GetHelperType(UIType);
            if (uiHelperTp != null)
            {
                var helper =
                    (gameObject.GetComponent(uiHelperTp) ?? gameObject.AddComponent(uiHelperTp)) as UIHelperBase;
                helper.ParseAndAttachUIElements();
            }

            if (refreshParent)
            {
                var parentHelper = transform.parent?.GetComponent<UIHelperBase>();
                parentHelper?.ParseAndAttachUIElements();
            }

            EditorUtility.SetDirty(this);
        }

        private void RemoveUIHelper()
        {
            var uiHelpers = GetComponents<UIHelperBase>();
            if (uiHelpers != null)
                foreach (var uiHelper in uiHelpers)
                    DestroyImmediate(uiHelper);

            EditorUtility.SetDirty(this);
        }

        /// <summary>
        ///     是否需要导出此图层
        /// </summary>
        /// <returns></returns>
        public bool NeedExportImage()
        {
            return gameObject.activeSelf && MarkToExportImage;
        }

        public string GetName()
        {
            if (!IsSingleImg())
                return string.IsNullOrWhiteSpace(name) ? UIType.ToString() : name;
            return Regex.Match(name, @"\((.*?)\)").Groups[1].Value;
        }

        public bool IsSingleImg()
        {
            if (string.IsNullOrWhiteSpace(name)) return false;
            return Regex.Match(name, @"\((.*?)\)").Success;
        }

        public bool RefreshLayerTexture(bool forceRefresh = false)
        {
            if (!forceRefresh && PreviewTexture != null) return true;

            if (BindPsdLayer == null || BindPsdLayer.Disposed) return false;

            var pngOpt = new PngOptions
            {
                ColorType = PngColorType.TruecolorWithAlpha
            };
            if (BindPsdLayer.CanSave(pngOpt))
            {
                if (PreviewTexture != null) DestroyImmediate(PreviewTexture);

                PreviewTexture = ConvertPsdLayer2Texture2D();
            }

            return PreviewTexture != null;
        }

        /// <summary>
        ///     把psd图层转成Texture2D
        /// </summary>
        /// <param name="psdLayer"></param>
        /// <returns>Texture2D</returns>
        public Texture2D ConvertPsdLayer2Texture2D()
        {
            if (BindPsdLayer == null || BindPsdLayer.Disposed) return null;

            var ms = new MemoryStream();
            var pngOpt = new PngOptions
            {
                ColorType = PngColorType.TruecolorWithAlpha,
                FullFrame = true
            };
            //var smartLayer = Psd2UIFormConverter.Instance.ConvertToSmartObjectLayer(BindPsdLayer);
            //smartLayer.Save(ms, pngOpt);
            BindPsdLayer.MergeLayerOpacity();
            BindPsdLayer.Save(ms, pngOpt);

            //var bitmap = BindPsdLayer.ToBitmap();
            //bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            var buffer = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(buffer, 0, buffer.Length);

            var texture = new Texture2D(BindPsdLayer.Width, BindPsdLayer.Height);
            texture.alphaIsTransparency = true;
            texture.LoadImage(buffer);
            texture.Apply();
            ms.Dispose();
            return texture;
        }

        /// <summary>
        ///     从第一层子节点按类型查找LayerNode
        /// </summary>
        /// <param name="uiTp"></param>
        /// <returns></returns>
        public PsdLayerNode FindSubLayerNode(GUIType uiTp)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i)?.GetComponent<PsdLayerNode>();

                if (child != null && child.UIType == uiTp) return child;
            }

            return null;
        }

        /// <summary>
        ///     依次查找给定多个类型,返回最先找到的类型
        /// </summary>
        /// <param name="uiTps"></param>
        /// <returns></returns>
        public PsdLayerNode FindSubLayerNode(params GUIType[] uiTps)
        {
            foreach (var tp in uiTps)
            {
                var result = FindSubLayerNode(tp);
                if (result != null) return result;
            }

            return null;
        }

        public PsdLayerNode FindLayerNodeInChildren(GUIType uiTp)
        {
            var layers = GetComponentsInChildren<PsdLayerNode>(true);
            if (layers != null && layers.Length > 0) return layers.FirstOrDefault(layer => layer.UIType == uiTp);

            return null;
        }

        /// <summary>
        ///     判断该图层是否为文本图层
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public bool IsTextLayer(out TextLayer layer)
        {
            layer = null;
            if (BindPsdLayer == null) return false;

            if (BindPsdLayer is SmartObjectLayer smartLayer)
            {
                layer = smartLayer.GetSmartObjectInnerTextLayer() as TextLayer;
                return layer != null;
            }

            if (BindPsdLayer is TextLayer txtLayer)
            {
                layer = txtLayer;
                return layer != null;
            }

            return false;
        }

        internal void InitPsdLayers(PsdImage psdInstance)
        {
            var layers = psdInstance.Layers;
            if (BindPsdLayerIndex >= 0 && BindPsdLayerIndex < layers.Length)
                BindPsdLayer = psdInstance.Layers[BindPsdLayerIndex];
        }
    }
}

#endif