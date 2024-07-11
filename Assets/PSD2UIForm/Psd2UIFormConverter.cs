﻿/*
    联系作者:
    https://blog.csdn.net/final5788
    https://github.com/sunsvip
 */

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspose.PSD;
using Aspose.PSD.FileFormats.Psd;
using Aspose.PSD.FileFormats.Psd.Layers;
using Aspose.PSD.FileFormats.Psd.Layers.SmartObjects;
using Aspose.PSD.ImageLoadOptions;
using HarmonyLib;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;
using FontStyle = UnityEngine.FontStyle;

namespace UGF.EditorTools.Psd2UGUI
{
    [CustomEditor(typeof(Psd2UIFormConverter))]
    public class Psd2UIFormConverterInspector : Editor
    {
        private GUILayoutOption btHeight;
        private GUIContent exportUISpritesBt;
        private GUIContent generateUIFormBt;

        private GUIContent parsePsd2NodesBt;
        private Psd2UIFormConverter targetLogic;

        private void OnEnable()
        {
            btHeight = GUILayout.Height(30);
            targetLogic = target as Psd2UIFormConverter;
            parsePsd2NodesBt = new GUIContent("解析psd图层", "把psd图层解析为可编辑节点树");
            exportUISpritesBt = new GUIContent("导出Images", "导出勾选的psd图层为碎图");
            generateUIFormBt = new GUIContent("生成UIForm", "根据解析后的节点树生成UIForm Prefab");
            if (string.IsNullOrWhiteSpace(Psd2UIFormSettings.Instance.UIFormOutputDir))
                Debug.LogWarning("UIForm输出路径为空!");
        }

        private void OnDisable()
        {
            Psd2UIFormSettings.Save();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.BeginVertical("box");
            {
                //EditorGUILayout.BeginHorizontal();
                //{
                //    EditorGUILayout.LabelField("自动压缩图片:", GUILayout.Width(150));
                //    Psd2UIFormSettings.Instance.CompressImage = EditorGUILayout.Toggle(Psd2UIFormSettings.Instance.CompressImage);
                //    EditorGUILayout.EndHorizontal();
                //}
                if (GUILayout.Button("查看使用文档"))
                {
                    Application.OpenURL("https://blog.csdn.net/final5788");
                    GUIUtility.ExitGUI();
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField("UI图片导出路径:", GUILayout.Width(150));
                    Psd2UIFormSettings.Instance.UIImagesOutputDir =
                        EditorGUILayout.TextField(Psd2UIFormSettings.Instance.UIImagesOutputDir);
                    if (GUILayout.Button("选择路径", GUILayout.Width(80)))
                    {
                        var retPath = EditorUtility.OpenFolderPanel("选择导出路径",
                            Psd2UIFormSettings.Instance.UIImagesOutputDir, null);
                        if (!string.IsNullOrWhiteSpace(retPath))
                        {
                            if (!retPath.StartsWith("Assets/"))
                                retPath = Path.GetRelativePath(Directory.GetParent(Application.dataPath).FullName,
                                    retPath);

                            Psd2UIFormSettings.Instance.UIImagesOutputDir = retPath;
                            Psd2UIFormSettings.Save();
                        }

                        GUIUtility.ExitGUI();
                    }

                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.BeginHorizontal();
                {
                    Psd2UIFormSettings.Instance.UseUIFormOutputDir = EditorGUILayout.ToggleLeft("使用UIForm导出路径:",
                        Psd2UIFormSettings.Instance.UseUIFormOutputDir, GUILayout.Width(150));
                    EditorGUI.BeginDisabledGroup(!Psd2UIFormSettings.Instance.UseUIFormOutputDir);
                    {
                        Psd2UIFormSettings.Instance.UIFormOutputDir =
                            EditorGUILayout.TextField(Psd2UIFormSettings.Instance.UIFormOutputDir);
                        if (GUILayout.Button("选择路径", GUILayout.Width(80)))
                        {
                            var retPath = EditorUtility.OpenFolderPanel("选择导出路径",
                                Psd2UIFormSettings.Instance.UIFormOutputDir, null);
                            if (!string.IsNullOrWhiteSpace(retPath))
                            {
                                if (!retPath.StartsWith("Assets/"))
                                    retPath = Path.GetRelativePath(Directory.GetParent(Application.dataPath).FullName,
                                        retPath);

                                Psd2UIFormSettings.Instance.UIFormOutputDir = retPath;
                                Psd2UIFormSettings.Save();
                            }

                            GUIUtility.ExitGUI();
                        }

                        EditorGUI.EndDisabledGroup();
                    }
                    EditorGUILayout.EndHorizontal();
                }

                EditorGUILayout.EndVertical();
            }


            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(parsePsd2NodesBt, btHeight))
                    Psd2UIFormConverter.ParsePsd2LayerPrefab(targetLogic.PsdAssetName, targetLogic);

                if (GUILayout.Button(exportUISpritesBt, btHeight)) targetLogic.ExportSprites();

                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button(generateUIFormBt, btHeight)) targetLogic.GenerateUIForm();

            base.OnInspectorGUI();
        }

        public override bool HasPreviewGUI()
        {
            return targetLogic.BindPsdAsset != null;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            GUI.DrawTexture(r, targetLogic.BindPsdAsset.texture, ScaleMode.ScaleToFit);
            //base.OnPreviewGUI(r, background);
        }
    }

    /// <summary>
    ///     Psd文件转成UIForm prefab
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Psd2UIFormConverter : MonoBehaviour
    {
        private const string RecordLayerOperation = "Change Export Image";

        private static bool licenseInited;
        [ReadOnlyField] [SerializeField] public string psdAssetChangeTime; //文件修改时间标识
        [Tooltip("UIForm名字")] [SerializeField] private string uiFormName;
        [Tooltip("关联的psd文件")] [SerializeField] private Sprite psdAsset;
        [Header("Debug:")] [SerializeField] private bool drawLayerRectGizmos = true;
        [SerializeField] private Color drawLayerRectGizmosColor = Color.green;

        private PsdImage psdInstance; //psd文件解析实例
        private GUIStyle uiTypeLabelStyle;
        public static Psd2UIFormConverter Instance { get; private set; }
        public string PsdAssetName => psdAsset != null ? AssetDatabase.GetAssetPath(psdAsset) : null;
        public Sprite BindPsdAsset => psdAsset;
        public Vector2Int UIFormCanvasSize => new(psdInstance.Width, psdInstance.Height);

        private void Start()
        {
            RefreshNodesBindLayer();
        }

        private void OnEnable()
        {
            Instance = this;
            uiTypeLabelStyle = new GUIStyle();
            uiTypeLabelStyle.fontSize = 13;
            uiTypeLabelStyle.fontStyle = FontStyle.BoldAndItalic;
            ColorUtility.TryParseHtmlString("#7ED994", out var color);
            uiTypeLabelStyle.normal.textColor = color;

            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;

            if (psdInstance == null && !string.IsNullOrWhiteSpace(PsdAssetName)) RefreshNodesBindLayer();
        }

        private void OnDisable()
        {
            EditorApplication.hierarchyWindowItemOnGUI -= OnHierarchyGUI;
        }

        private void OnDestroy()
        {
            if (psdInstance != null && !psdInstance.Disposed) psdInstance.Dispose();
        }

        private void OnDrawGizmos()
        {
            if (drawLayerRectGizmos)
            {
                var nodes = GetComponentsInChildren<PsdLayerNode>();
                Gizmos.color = drawLayerRectGizmosColor;
                foreach (var item in nodes)
                    if (item.NeedExportImage())
                        Gizmos.DrawWireCube(item.LayerRect.position * 0.01f, item.LayerRect.size * 0.01f);
            }
        }

        [InitializeOnLoadMethod]
        private static void InitAsposeLicense()
        {
            if (licenseInited) return;
            var harmonyHook = new Harmony("test");
            harmonyHook.PatchAll();
            var LData = "DQo8TGljZW5zZT4NCjxEYXRhPg0KPExpY2Vuc2VkVG8+VGhlIFdvcmxkIEJhbms8L0xpY2Vuc2VkV" +
                        "G8+DQo8RW1haWxUbz5ra3VtYXIzQHdvcmxkYmFua2dyb3VwLm9yZzwvRW1haWxUbz4NCjxMaWNlbnNlV" +
                        "HlwZT5EZXZlbG9wZXIgU21hbGwgQnVzaW5lc3M8L0xpY2Vuc2VUeXBlPg0KPExpY2Vuc2VOb3RlPjEgRGV2Z" +
                        "WxvcGVyIEFuZCAxIERlcGxveW1lbnQgTG9jYXRpb248L0xpY2Vuc2VOb3RlPg0KPE9yZGVySUQ+MjEwMzE2MTg" +
                        "1OTU3PC9PcmRlcklEPg0KPFVzZXJJRD43NDQ5MTY8L1VzZXJJRD4NCjxPRU0+VGhpcyBpcyBub3QgYSByZWRpc3R" +
                        "yaWJ1dGFibGUgbGljZW5zZTwvT0VNPg0KPFByb2R1Y3RzPg0KPFByb2R1Y3Q+QXNwb3NlLlRvdGFsIGZvciAuTkV" +
                        "UPC9Qcm9kdWN0Pg0KPC9Qcm9kdWN0cz4NCjxFZGl0aW9uVHlwZT5Qcm9mZXNzaW9uYWw8L0VkaXRpb25UeXBlPg0KP" +
                        "FNlcmlhbE51bWJlcj4wM2ZiMTk5YS01YzhhLTQ4ZGItOTkyZS1kMDg0ZmYwNjZkMGM8L1NlcmlhbE51bWJlcj4NCj" +
                        "xTdWJzY3JpcHRpb25FeHBpcnk+MjAyMjA1MTY8L1N1YnNjcmlwdGlvbkV4cGlyeT4NCjxMaWNlbnNlVmVyc2lvbj4z" +
                        "LjA8L0xpY2Vuc2VWZXJzaW9uPg0KPExpY2Vuc2VJbnN0cnVjdGlvbnM+aHR0cHM6Ly9wdXJjaGFzZS5hc3Bvc2UuY29tL" +
                        "3BvbGljaWVzL3VzZS1saWNlbnNlPC9MaWNlbnNlSW5zdHJ1Y3Rpb25zPg0KPC9EYXRhPg0KPFNpZ25hdHVyZT5XbkJYNn" +
                        "JOdHpCclNMV3pBdFlqOEtkdDFLSUI5MlFrL2xEbFNmMlM1TFRIWGdkcS9QQ2NqWHVORmp0NEJuRmZwNFZLc3VsSjhWeFE" +
                        "xakIwbmM0R1lWcWZLek14SFFkaXFuZU03NTJaMjlPbmdyVW40Yk0rc1l6WWVSTE9UOEpxbE9RN05rRFU0bUk2Z1VyQ3dxc" +
                        "jdnUVYxbDJJWkJxNXMzTEFHMFRjQ1ZncEE9PC9TaWduYXR1cmU+DQo8L0xpY2Vuc2U+DQo=";
            new License().SetLicense(new MemoryStream(Convert.FromBase64String(LData)));
            licenseInited = true;
            harmonyHook.UnpatchAll();
        }

        private void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            if (Event.current == null) return;
            var node = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (node == null || node == gameObject) return;
            if (!node.TryGetComponent<PsdLayerNode>(out var layer)) return;

            var tmpRect = selectionRect;
            tmpRect.x = 35;
            tmpRect.width = 10;
            Undo.RecordObject(layer, RecordLayerOperation);
            EditorGUI.BeginChangeCheck();
            {
                layer.markToExport = EditorGUI.Toggle(tmpRect, layer.markToExport);
                if (EditorGUI.EndChangeCheck())
                {
                    if (Selection.gameObjects.Length > 1) SetExportImageTg(Selection.gameObjects, layer.markToExport);
                    EditorUtility.SetDirty(layer);
                }
            }
            tmpRect.width = Mathf.Clamp(selectionRect.xMax * 0.2f, 100, 200);
            tmpRect.x = selectionRect.xMax - tmpRect.width;
            if (EditorGUI.DropdownButton(tmpRect, new GUIContent(layer.UIType.ToString()), FocusType.Passive))
            {
                var dropdownMenu = PopUITypesMenu(layer, selectUIType =>
                {
                    layer.SetUIType(selectUIType);
                    EditorUtility.SetDirty(layer);
                });

                dropdownMenu.ShowAsContext();
            }
        }

        private GenericMenu PopUITypesMenu(PsdLayerNode layer, Action<GUIType> onSelectEnum)
        {
            var names = Enum.GetValues(typeof(GUIType));
            var dropdownMenu = new GenericMenu();
            foreach (GUIType item in names)
            {
                var itemName = UGUIParser.IsMainUIType(item) ? item.ToString() : item.ToString().Replace('_', '/');
                dropdownMenu.AddItem(new GUIContent(itemName), item.Equals(layer.UIType),
                    () => { onSelectEnum(item); });
            }

            return dropdownMenu;
        }

        /// <summary>
        ///     批量勾选导出图片
        /// </summary>
        /// <param name="selects"></param>
        /// <param name="exportImg"></param>
        private void SetExportImageTg(GameObject[] selects, bool exportImg)
        {
            var selectLayerNodes = selects.Where(item => item?.GetComponent<PsdLayerNode>() != null).ToArray();
            foreach (var layer in selectLayerNodes) layer.GetComponent<PsdLayerNode>().markToExport = exportImg;
        }

        private void RefreshNodesBindLayer()
        {
            if (psdInstance == null || psdInstance.Disposed)
            {
                if (!File.Exists(PsdAssetName))
                {
                    Debug.LogError("刷新节点绑定图层失败! psd文件不存在");
                    return;
                }

                var psdOpts = new PsdLoadOptions
                {
                    LoadEffectsResource = true,
                    ReadOnlyMode = false
                };
                try
                {
                    psdInstance = Image.Load(PsdAssetName, psdOpts) as PsdImage;
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    return;
                }
            }

            var layers = GetComponentsInChildren<PsdLayerNode>(true);
            foreach (var layer in layers) layer.InitPsdLayers(psdInstance);

            var spRender = gameObject.GetComponent<SpriteRenderer>() ?? gameObject.AddComponent<SpriteRenderer>();
            spRender.sprite = psdAsset;
        }

        [MenuItem("Assets/Psd2UIForm Editor", priority = 0)]
        private static void Psd2UIFormPrefabMenu()
        {
            if (Selection.activeObject == null) return;
            var assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (Path.GetExtension(assetPath).ToLower().CompareTo(".psd") != 0)
            {
                Debug.LogWarning($"选择的文件({assetPath})不是psd格式, 工具只支持psd转换为UIForm");
                return;
            }

            var psdLayerPrefab = GetPsdLayerPrefabPath(assetPath);
            if (!File.Exists(psdLayerPrefab))
            {
                if (ParsePsd2LayerPrefab(assetPath)) OpenPsdLayerEditor(psdLayerPrefab);
            }
            else
            {
                OpenPsdLayerEditor(psdLayerPrefab);
            }
        }

        public bool CheckPsdAssetHasChanged()
        {
            if (psdAsset == null) return false;
            var fileTag = GetAssetChangeTag(PsdAssetName);
            return psdAssetChangeTime.CompareTo(fileTag) != 0;
        }

        public static string GetAssetChangeTag(string fileName)
        {
            return new FileInfo(fileName).LastWriteTimeUtc.ToString("yyyyMMddHHmmss");
        }

        /// <summary>
        ///     打开psd图层信息prefab
        /// </summary>
        /// <param name="psdLayerPrefab"></param>
        public static void OpenPsdLayerEditor(string psdLayerPrefab)
        {
            PrefabStageUtility.OpenPrefab(psdLayerPrefab);
        }

        /// <summary>
        ///     把Psd图层解析成节点prefab
        /// </summary>
        /// <param name="psdPath"></param>
        /// <returns></returns>
        public static bool ParsePsd2LayerPrefab(string psdFile, Psd2UIFormConverter instanceRoot = null)
        {
            if (!File.Exists(psdFile))
            {
                Debug.LogError($"Error: Psd文件不存在:{psdFile}");
                return false;
            }

            var texImporter = AssetImporter.GetAtPath(psdFile) as TextureImporter;
            if (texImporter.textureType != TextureImporterType.Sprite)
            {
                texImporter.textureType = TextureImporterType.Sprite;
                texImporter.mipmapEnabled = false;
                texImporter.alphaIsTransparency = true;
                texImporter.SaveAndReimport();
            }

            var prefabFile = GetPsdLayerPrefabPath(psdFile);
            var rootName = Path.GetFileNameWithoutExtension(prefabFile);

            var needDestroyInstance = instanceRoot == null;
            if (instanceRoot != null)
            {
                ParsePsdLayer2Root(psdFile, instanceRoot);
                instanceRoot.RefreshNodesBindLayer();
                return true;
            }

            var rootLayer = CreatePsdLayerRoot(rootName);
            rootLayer.psdAssetChangeTime = GetAssetChangeTag(psdFile);
            rootLayer.SetPsdAsset(psdFile);
            ParsePsdLayer2Root(psdFile, rootLayer);
            PrefabUtility.SaveAsPrefabAsset(rootLayer.gameObject, prefabFile, out var savePrefabSuccess);
            if (needDestroyInstance) DestroyImmediate(rootLayer.gameObject);
            AssetDatabase.Refresh();
            if (savePrefabSuccess && AssetDatabase.GUIDFromAssetPath(StageUtility.GetCurrentStage().assetPath) !=
                AssetDatabase.GUIDFromAssetPath(prefabFile))
                PrefabStageUtility.OpenPrefab(prefabFile);

            return savePrefabSuccess;
        }

        private static void ParsePsdLayer2Root(string psdFile, Psd2UIFormConverter converter)
        {
            //清空已有节点重新解析
            for (var i = converter.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(converter.transform.GetChild(i).gameObject);

            var psdOpts = new PsdLoadOptions
            {
                LoadEffectsResource = true,
                ReadOnlyMode = false
            };
            try
            {
                using (var psd = Image.Load(psdFile, psdOpts) as PsdImage)
                {
                    var layerNodes = new List<GameObject> { converter.gameObject };
                    for (var i = 0; i < psd.Layers.Length; i++)
                    {
                        var layer = psd.Layers[i];
                        if (layer.Name.StartsWith("#")) continue;
                        var curLayerType = layer.GetLayerType();
                        if (curLayerType == PsdLayerType.SectionDividerLayer)
                        {
                            var layerGroup = (layer as SectionDividerLayer).GetRelatedLayerGroup();
                            var layerGroupIdx = ArrayUtility.IndexOf(psd.Layers, layerGroup);
                            var layerGropNode = CreatePsdLayerNode(layerGroup, layerGroupIdx);
                            layerNodes.Add(layerGropNode.gameObject);
                        }
                        else if (curLayerType == PsdLayerType.LayerGroup)
                        {
                            var lastLayerNode = layerNodes.Last();
                            layerNodes.Remove(lastLayerNode);

                            if (layerNodes.Count > 0)
                            {
                                var parentLayerNode = layerNodes.Last();
                                lastLayerNode.transform.SetParent(parentLayerNode.transform);
                            }
                        }
                        else
                        {
                            var newLayerNode = CreatePsdLayerNode(layer, i);
                            newLayerNode.transform.SetParent(layerNodes.Last().transform);
                            newLayerNode.transform.localPosition = Vector3.zero;
                        }
                    }
                }

                converter.psdAssetChangeTime = GetAssetChangeTag(psdFile);
                var childrenNodes = converter.GetComponentsInChildren<PsdLayerNode>(true);
                foreach (var item in childrenNodes) item.RefreshUIHelper();

                EditorUtility.SetDirty(converter.gameObject);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void SetPsdAsset(string psdFile)
        {
            psdAsset = AssetDatabase.LoadAssetAtPath<Sprite>(psdFile);
            if (string.IsNullOrWhiteSpace(Psd2UIFormSettings.Instance.UIImagesOutputDir))
                Psd2UIFormSettings.Instance.UIImagesOutputDir = Path.GetDirectoryName(psdFile);

            if (string.IsNullOrWhiteSpace(uiFormName)) uiFormName = psdAsset.name;
        }

        /// <summary>
        ///     获取解析好的psd layers文件
        /// </summary>
        /// <param name="psd"></param>
        /// <returns></returns>
        public static string GetPsdLayerPrefabPath(string psd)
        {
            return Path.Combine(Path.GetDirectoryName(psd),
                Path.GetFileNameWithoutExtension(psd) + "_psd_layers_parsed.prefab");
        }

        private static Psd2UIFormConverter CreatePsdLayerRoot(string rootName)
        {
            var node = new GameObject(rootName);
            node.gameObject.tag = "EditorOnly";
            var layerRoot = node.AddComponent<Psd2UIFormConverter>();
            return layerRoot;
        }

        private static PsdLayerNode CreatePsdLayerNode(Layer layer, int bindLayerIdx)
        {
            var nodeName = layer.Name;
            if (string.IsNullOrWhiteSpace(nodeName))
            {
                nodeName = $"PsdLayer-{bindLayerIdx}";
            }
            else
            {
                if (UGUIParser.HasUITypeFlag(nodeName, out var tpFlag))
                    nodeName = nodeName.Substring(0, nodeName.Length - tpFlag.Length);
            }

            var node = new GameObject(nodeName);
            node.gameObject.tag = "EditorOnly";
            var layerNode = node.AddComponent<PsdLayerNode>();
            layerNode.BindPsdLayerIndex = bindLayerIdx;
            InitLayerNodeData(layerNode, layer);
            return layerNode;
        }

        /// <summary>
        ///     根据psd图层信息解析并初始化图层UI类型、是否导出等信息
        /// </summary>
        /// <param name="layerNode"></param>
        /// <param name="layer"></param>
        private static void InitLayerNodeData(PsdLayerNode layerNode, Layer layer)
        {
            if (layer == null || layer.Disposed) return;
            var layerTp = layer.GetLayerType();
            layerNode.BindPsdLayer = layer;
            if (UGUIParser.Instance.TryParse(layerNode, out var initRule)) layerNode.SetUIType(initRule.UIType, false);

            layerNode.markToExport = layerTp != PsdLayerType.LayerGroup && !(layerTp == PsdLayerType.TextLayer &&
                                                                             layerNode.UIType.ToString()
                                                                                 .EndsWith("Text") &&
                                                                             layerNode.UIType != GUIType.FillColor);
            layerNode.gameObject.SetActive(layer.IsVisible);
        }

        /// <summary>
        ///     导出psd图层为Sprites碎图
        /// </summary>
        /// <param name="psdAssetName"></param>
        internal void ExportSprites()
        {
            //var pngOpts = new PngOptions()
            //{
            //    ColorType = Aspose.PSD.FileFormats.Png.PngColorType.Truecolor
            //};
            //this.psdInstance.Save("Assets/AAAGame/Sprites/UI/Preview.png", pngOpts);

            //return;
            var exportLayers = GetComponentsInChildren<PsdLayerNode>().Where(node => node.NeedExportImage());
            var exportDir = GetUIFormImagesOutputDir();
            if (!Directory.Exists(exportDir)) Directory.CreateDirectory(exportDir);

            var exportIdx = 0;
            var totalCount = exportLayers.Count();
            foreach (var layer in exportLayers)
            {
                var assetName = layer.ExportImageAsset();
                if (assetName == null)
                    Debug.LogWarning($"导出图层[name:{layer.name}, layerIdx:{layer.BindPsdLayerIndex}]图片失败!");

                ++exportIdx;
                EditorUtility.DisplayProgressBar($"导出进度({exportIdx}/{totalCount})", $"导出UI图片:{assetName}",
                    exportIdx / (float)totalCount);
            }

            EditorUtility.ClearProgressBar();
            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     根据解析后的节点树生成UIForm Prefab
        /// </summary>
        internal void GenerateUIForm()
        {
            if (Psd2UIFormSettings.Instance.UseUIFormOutputDir &&
                string.IsNullOrWhiteSpace(Psd2UIFormSettings.Instance.UIFormOutputDir))
            {
                Debug.LogError($"生成UIForm失败! UIForm导出路径为空:{Psd2UIFormSettings.Instance.UIFormOutputDir}");
                return;
            }

            if (Psd2UIFormSettings.Instance.UseUIFormOutputDir)
            {
                ExportUIPrefab(Psd2UIFormSettings.Instance.UIFormOutputDir);
            }
            else
            {
                var lastSaveDir = string.IsNullOrWhiteSpace(Psd2UIFormSettings.Instance.LastUIFormOutputDir)
                    ? "Assets"
                    : Psd2UIFormSettings.Instance.LastUIFormOutputDir;
                var selectDir = EditorUtility.SaveFolderPanel("保存目录", lastSaveDir, null);
                if (!string.IsNullOrWhiteSpace(selectDir))
                {
                    if (!selectDir.StartsWith("Assets/"))
                        selectDir = Path.GetRelativePath(Directory.GetParent(Application.dataPath).FullName, selectDir);
                    Psd2UIFormSettings.Instance.LastUIFormOutputDir = selectDir;
                    ExportUIPrefab(selectDir);
                }
            }
        }

        private bool ExportUIPrefab(string outputDir)
        {
            if (!string.IsNullOrWhiteSpace(outputDir))
                if (!Directory.Exists(outputDir))
                    try
                    {
                        Directory.CreateDirectory(outputDir);
                        AssetDatabase.Refresh();
                    }
                    catch (Exception err)
                    {
                        Debug.LogError($"导出UI prefab失败:{err.Message}");
                        return false;
                    }

            if (string.IsNullOrWhiteSpace(uiFormName))
            {
                Debug.LogError("导出UI Prefab失败: UI Form Name为空, 请填写UI Form Name.");
                return false;
            }

            var prefabName = Path.Combine(outputDir, $"{uiFormName}.prefab");
            if (File.Exists(prefabName))
                if (!EditorUtility.DisplayDialog("警告", $"prefab文件已存在, 是否覆盖:{prefabName}", "覆盖生成", "取消生成"))
                    return false;

            var uiHelpers = GetAvailableUIHelpers();
            if (uiHelpers == null || uiHelpers.Length < 1) return false;

            var uiFormRoot =
                Instantiate(UGUIParser.Instance.UIFormTemplate, Vector3.zero, Quaternion.identity);
            uiFormRoot.name = uiFormName;
            Vector3 canvasPosition = uiFormRoot.GetComponent<RectTransform>().anchoredPosition;
            var curIdx = 0;
            var totalCount = uiHelpers.Length;
            foreach (var uiHelper in uiHelpers)
            {
                EditorUtility.DisplayProgressBar($"生成UIFrom:({curIdx++}/{totalCount})", $"正在生成UI元素:{uiHelper.name}",
                    curIdx /
                    (float)totalCount);
                var uiElement = uiHelper.CreateUI();
                if (uiElement == null) continue;

                var goPath = GetGameObjectInstanceIdPath(uiHelper.gameObject, out var goNames);
                var parentNode = GetOrCreateNodeByInstanceIdPath(uiFormRoot, goPath, goNames);
                uiElement.transform.SetParent(parentNode.transform, true);
                uiElement.transform.position += canvasPosition;
            }

            var uiStrKeys = uiFormRoot.GetComponentsInChildren<UIStringKey>(true);
            for (var i = uiStrKeys.Length - 1; i >= 0; i--) DestroyImmediate(uiStrKeys[i]);

            var uiPrefab = PrefabUtility.SaveAsPrefabAsset(uiFormRoot, prefabName, out var saveSuccess);
            if (saveSuccess)
            {
                DestroyImmediate(uiFormRoot);
                Selection.activeGameObject = uiPrefab;
            }

            EditorUtility.ClearProgressBar();
            return true;
        }

        private GameObject GetOrCreateNodeByInstanceIdPath(GameObject uiFormRoot, string[] goPath, string[] goNames)
        {
            var result = uiFormRoot;
            if (goPath != null && goNames != null)
                for (var i = 0; i < goPath.Length; i++)
                {
                    var nodeId = goPath[i];
                    var nodeName = goNames[i];
                    GameObject targetNode = null;
                    foreach (Transform child in result.transform)
                    {
                        if (child.gameObject == result) continue;

                        var idKey = child.GetComponent<UIStringKey>();
                        if (idKey != null && nodeId == idKey.Key)
                        {
                            targetNode = child.gameObject;
                            break;
                        }
                    }

                    if (targetNode == null)
                    {
                        targetNode = new GameObject(nodeName);
                        targetNode.AddComponent<RectTransform>();
                        targetNode.transform.SetParent(result.transform, false);
                        targetNode.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                        var targetNodeKey = targetNode.GetComponent<UIStringKey>() ??
                                            targetNode.AddComponent<UIStringKey>();
                        targetNodeKey.Key = nodeId;
                    }

                    result = targetNode;
                }

            return result;
        }

        private string[] GetGameObjectInstanceIdPath(GameObject go, out string[] names)
        {
            names = null;
            if (go == null || go.transform.parent == null || go.transform.parent == transform) return null;

            var parentGo = go.transform.parent;
            var result = new string[1] { parentGo.gameObject.GetInstanceID().ToString() };
            names = new string[1] { parentGo.gameObject.name };
            while (parentGo.parent != null && parentGo.parent != transform)
            {
                ArrayUtility.Insert(ref result, 0, parentGo.parent.gameObject.GetInstanceID().ToString());
                ArrayUtility.Insert(ref names, 0, parentGo.parent.gameObject.name);
                parentGo = parentGo.parent;
            }

            return result;
        }

        private UIHelperBase[] GetAvailableUIHelpers()
        {
            var uiHelpers = GetComponentsInChildren<UIHelperBase>();
            uiHelpers = uiHelpers.Where(ui => ui.LayerNode.IsMainUIType).ToArray();

            var dependInstIds = new List<int>();
            foreach (var item in uiHelpers)
            foreach (var depend in item.GetDependencies())
            {
                var dependId = depend.gameObject.GetInstanceID();
                if (!dependInstIds.Contains(dependId)) dependInstIds.Add(dependId);
            }

            for (var i = uiHelpers.Length - 1; i >= 0; i--)
            {
                var uiHelper = uiHelpers[i];
                if (dependInstIds.Contains(uiHelper.gameObject.GetInstanceID()))
                    ArrayUtility.RemoveAt(ref uiHelpers, i);
            }

            return uiHelpers;
        }

        /// <summary>
        ///     把图片设置为为Sprite或Texture类型
        /// </summary>
        /// <param name="dir"></param>
        public static void ConvertTexturesType(string[] texAssets, bool isImage = true)
        {
            foreach (var item in texAssets)
            {
                var texImporter = AssetImporter.GetAtPath(item) as TextureImporter;
                if (texImporter == null)
                {
                    Debug.LogError($"TextureImporter为空:{item}");
                    continue;
                }

                if (isImage)
                {
                    texImporter.textureType = TextureImporterType.Sprite;
                    texImporter.spriteImportMode = SpriteImportMode.Single;
                    texImporter.alphaSource = TextureImporterAlphaSource.FromInput;
                    texImporter.alphaIsTransparency = true;
                    texImporter.mipmapEnabled = false;
                }
                else
                {
                    texImporter.textureType = TextureImporterType.Default;
                    texImporter.textureShape = TextureImporterShape.Texture2D;
                    texImporter.alphaSource = TextureImporterAlphaSource.FromInput;
                    texImporter.alphaIsTransparency = true;
                    texImporter.mipmapEnabled = false;
                }

                texImporter.SaveAndReimport();
            }
        }

        /// <summary>
        ///     压缩图片文件
        /// </summary>
        /// <param name="asset">文件名(相对路径Assets)</param>
        /// <returns></returns>
        public static bool CompressImageFile(string asset)
        {
            var assetPath = asset.StartsWith("Assets/")
                ? Path.GetFullPath(asset, Directory.GetParent(Application.dataPath).FullName)
                : asset;
            var compressTool = Utility.Assembly.GetType("UGF.EditorTools.CompressTool");
            if (compressTool == null) return false;

            var compressMethod =
                compressTool.GetMethod("CompressImageOffline", new[] { typeof(string), typeof(string) });
            if (compressMethod == null) return false;

            return (bool)compressMethod.Invoke(null, new object[] { assetPath, assetPath });
        }

        /// <summary>
        ///     获取UIForm对应的图片导出目录
        /// </summary>
        /// <returns></returns>
        public string GetUIFormImagesOutputDir()
        {
            return Path.Combine(Psd2UIFormSettings.Instance.UIImagesOutputDir, uiFormName);
        }

        public SmartObjectLayer ConvertToSmartObjectLayer(Layer layer)
        {
            var smartObj = psdInstance.SmartObjectProvider.ConvertToSmartObject(new[] { layer });
            return smartObj;
        }
    }
}
#endif