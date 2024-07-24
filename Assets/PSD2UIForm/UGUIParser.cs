/*
    联系作者:
    https://blog.csdn.net/final5788
    https://github.com/sunsvip
 */

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace UGF.EditorTools.Psd2UGUI
{
    public enum GUIType
    {
        Null = 0,
        Image,
        RawImage,
        Text,
        Button,
        Dropdown,
        InputField,
        Toggle,
        Slider,
        ScrollView,
        Mask,
        FillColor, //纯色填充
        TMPText,
        TMPButton,
        TMPDropdown,
        TMPInputField,
        TMPToggle,

        //UI的子类型, 以101开始。 0-100预留给UI类型, 新类型从尾部追加
        Background = 101, //通用背景

        //Button的子类型
        Button_Highlight,
        Button_Press,
        Button_Select,
        Button_Disable,
        Button_Text,

        //Dropdown/TMPDropdown的子类型
        Dropdown_Label,
        Dropdown_Arrow,

        //InputField/TMPInputField的子类型
        InputField_Placeholder,
        InputField_Text,

        //Toggle的子类型
        Toggle_Checkmark,
        Toggle_Label,

        //Slider的子类型
        Slider_Fill,
        Slider_Handle,

        //ScrollView的子类型
        ScrollView_Viewport, //列表可视区域的遮罩图
        ScrollView_HorizontalBarBG, //水平滑动栏背景
        ScrollView_HorizontalBar, //水平滑块
        ScrollView_VerticalBarBG, //垂直滑动栏背景
        ScrollView_VerticalBar //垂直滑动块
    }

    [Serializable]
    public class UGUIParseRule
    {
        public GUIType UIType;
        public string[] TypeMatches; //类型匹配标识
        public GameObject UIPrefab; //UI模板
        public string UIHelper; //UIHelper类型全名
        public string Comment; //注释
    }

    [CreateAssetMenu(fileName = "Psd2UIFormConfig", menuName = "ScriptableObject/Psd2UIForm Config【Psd2UIForm工具配置】")]
    public class UGUIParser : SerializedScriptableObject
    {
        public const char UITYPE_SPLIT_CHAR = '.';
        public const int UITYPE_MAX = 100;
        private static UGUIParser mInstance;

        [SerializeField]
        [PropertyOrder(2)]
        [LabelText("默认文本类型")]
        private GUIType defaultTextType;

        [SerializeField]
        [PropertyOrder(3)]
        private GameObject uiFormTemplate;

        [SerializeField]
        [PropertyOrder(4)]
        [ListDrawerSettings(ListElementLabelName = "UIType", ShowIndexLabels = true)]
        private List<UGUIParseRule> rules;

        [HideInInspector]
        [SerializeField]
        private string readmeDoc = "使用说明";

        public GameObject UIFormTemplate => uiFormTemplate;

        public static UGUIParser Instance
        {
            get
            {
                if (mInstance == null)
                {
                    var guid = AssetDatabase.FindAssets("t:UGUIParser").FirstOrDefault();
                    mInstance = AssetDatabase.LoadAssetAtPath<UGUIParser>(AssetDatabase.GUIDToAssetPath(guid));
                }

                return mInstance;
            }
        }

        public static bool IsMainUIType(GUIType tp)
        {
            return (int)tp <= UITYPE_MAX;
        }

        public Type GetHelperType(GUIType uiType)
        {
            if (uiType == GUIType.Null) return null;
            var rule = GetRule(uiType);
            if (rule == null || string.IsNullOrWhiteSpace(rule.UIHelper)) return null;

            return Type.GetType(rule.UIHelper);
        }

        public UGUIParseRule GetRule(GUIType uiType)
        {
            foreach (var rule in rules)
            {
                if (rule.UIType == uiType)
                    return rule;
            }

            return null;
        }

        /// <summary>
        ///     根据图层命名解析UI类型
        /// </summary>
        /// <param name="layer"></param>
        /// <param name="comType"></param>
        /// <returns></returns>
        public bool TryParse(PsdLayerNode layer, out UGUIParseRule result)
        {
            result = null;
            var layerName = layer.BindPsdLayer.Name;
            if (HasUITypeFlag(layerName, out var tpFlag))
            {
                var tpTag = tpFlag.Substring(1);
                foreach (var rule in rules)
                {
                    foreach (var item in rule.TypeMatches)
                    {
                        if (String.Compare(tpTag, item.ToLower(), StringComparison.Ordinal) == 0)
                        {
                            result = rule;
                            return true;
                        }
                    }
                }
            }

            switch (layer.LayerType)
            {
                case PsdLayerType.TextLayer:
                    result = rules.First(itm => itm.UIType == defaultTextType);
                    break;
                case PsdLayerType.LayerGroup:
                    result = rules.First(itm => itm.UIType == GUIType.Null);
                    break;
                default:
                    result = rules.First(itm => itm.UIType == GUIType.Image);
                    break;
            }

            return result != null;
        }

        public static bool HasUITypeFlag(string layerName, out string tpFlag)
        {
            tpFlag = null;
            if (string.IsNullOrWhiteSpace(layerName) || layerName.EndsWith(UITYPE_SPLIT_CHAR)) return false;
            var startIdx = layerName.LastIndexOf(UITYPE_SPLIT_CHAR);
            if (startIdx <= 0) return false;
            tpFlag = layerName.Substring(startIdx);
            return true;
        }

        /// <summary>
        ///     导出UI设计师使用规则文档
        /// </summary>
        [Button("导出使用文档",ButtonSizes.Large)]
        [PropertyOrder(0)]
        internal void ExportReadmeDoc()
        {
            var exportDir = EditorUtility.SaveFolderPanel("选择文档导出路径", Application.dataPath, null);
            if (string.IsNullOrWhiteSpace(exportDir) || !Directory.Exists(exportDir)) return;

            var docFile = Path.Combine(exportDir, "Psd2UGUI设计师使用文档.doc");
            var strBuilder = new StringBuilder();
            strBuilder.AppendLine("使用说明:");
            strBuilder.AppendLine("单元素UI：即单个图层的UI，如Image、Text、单图Button，可以直接在图层命名结尾加上\".类型\"来标记UI类型。\n如\"A.btn\"表示按钮。\n\n多元素UI: 对于多个图片组成的复合型UI，可以通过使用\"组\"包裹多个UI元素。在“组”命名结尾加上\".类型\"来标记UI类型。\n组里的图层命名后夹\".类型\"来标记为UI子元素类型。\n\n各种UI类型支持任意组合：如一个组类型标记为Button，组内包含一个按钮背景图层，一个艺术字图层(非文本图层)，就可以组成一个按钮内带有艺术字图片的按钮。");
            strBuilder.AppendLine(Environment.NewLine + Environment.NewLine);
            strBuilder.AppendLine("UI类型标识: 图层/组命名以'.类型'结尾");
            strBuilder.AppendLine("UI类型标识列表:");

            foreach (var rule in rules)
            {
                if (rule.UIType == GUIType.Null) continue;

                strBuilder.AppendLine($"{rule.UIType}: {rule.Comment}");
                strBuilder.Append("类型标识: ");
                foreach (var tag in rule.TypeMatches) strBuilder.Append($".{tag}, ");

                strBuilder.AppendLine();
                strBuilder.AppendLine();
            }

            try
            {
                File.WriteAllText(docFile, strBuilder.ToString(), Encoding.UTF8);
                EditorUtility.RevealInFinder(docFile);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}
#endif