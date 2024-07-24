/*
    联系作者:
    https://blog.csdn.net/final5788
    https://github.com/sunsvip
 */

#if UNITY_EDITOR
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGF.EditorTools.Psd2UGUI
{
    [DisallowMultipleComponent]
    public class TMPInputFieldHelper : UIHelperBase
    {
        [SerializeField]
        private PsdLayerNode background;

        [SerializeField]
        private PsdLayerNode placeholder;

        [SerializeField]
        private PsdLayerNode text;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(background, placeholder, text);
        }

        public override void ParseAndAttachUIElements()
        {
            background = LayerNode.FindSubLayerNode(GUIType.Background, GUIType.Image, GUIType.RawImage);
            placeholder = LayerNode.FindSubLayerNode(GUIType.InputField_Placeholder);
            text = LayerNode.FindSubLayerNode(GUIType.InputField_Text, GUIType.TMPText);
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var input = uiRoot.GetComponent<TMP_InputField>();
            background.SetRectTransform(input);

            var bgImage = input.targetGraphic as Image;
            bgImage.sprite = background.LayerNode2Sprite();
            placeholder.SetTextStyle(input.placeholder as TextMeshProUGUI);
            text.SetTextStyle(input.textComponent as TextMeshProUGUI);
        }
    }
}
#endif