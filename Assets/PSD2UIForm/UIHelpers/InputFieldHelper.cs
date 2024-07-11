﻿/*
    联系作者:
    https://blog.csdn.net/final5788
    https://github.com/sunsvip
 */

#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;

namespace UGF.EditorTools.Psd2UGUI
{
    [DisallowMultipleComponent]
    public class InputFieldHelper : UIHelperBase
    {
        [SerializeField] private PsdLayerNode background;
        [SerializeField] private PsdLayerNode placeholder;
        [SerializeField] private PsdLayerNode text;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(background, placeholder, text);
        }

        public override void ParseAndAttachUIElements()
        {
            background = LayerNode.FindSubLayerNode(GUIType.Background, GUIType.Image, GUIType.RawImage);
            placeholder = LayerNode.FindSubLayerNode(GUIType.InputField_Placeholder);
            text = LayerNode.FindSubLayerNode(GUIType.InputField_Text, GUIType.Text);
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var input = uiRoot.GetComponent<InputField>();
            UGUIParser.SetRectTransform(background, input);

            var bgImage = input.targetGraphic as Image;
            bgImage.sprite = UGUIParser.LayerNode2Sprite(background, bgImage.type == Image.Type.Sliced);
            UGUIParser.SetTextStyle(placeholder, input.placeholder as Text);
            UGUIParser.SetTextStyle(text, input.textComponent);
        }
    }
}
#endif