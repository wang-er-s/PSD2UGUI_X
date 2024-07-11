﻿/*
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
    public class TMPButtonHelper : UIHelperBase
    {
        [SerializeField] private PsdLayerNode background;
        [SerializeField] private PsdLayerNode text;

        [Header("Sprite Swap:")] [SerializeField]
        private PsdLayerNode highlight;

        [SerializeField] private PsdLayerNode press;
        [SerializeField] private PsdLayerNode select;
        [SerializeField] private PsdLayerNode disable;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(background, text, highlight, press, select, disable);
        }

        public override void ParseAndAttachUIElements()
        {
            background = LayerNode.FindSubLayerNode(GUIType.Background, GUIType.Image, GUIType.RawImage);
            text = LayerNode.FindSubLayerNode(GUIType.Button_Text, GUIType.TMPText, GUIType.Text);
            highlight = LayerNode.FindSubLayerNode(GUIType.Button_Highlight);
            press = LayerNode.FindSubLayerNode(GUIType.Button_Press);
            select = LayerNode.FindSubLayerNode(GUIType.Button_Select);
            disable = LayerNode.FindSubLayerNode(GUIType.Button_Disable);
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var button = uiRoot.GetComponent<Button>();
            var btImg = button.GetComponent<Image>();
            var useSliceSp = btImg.type == Image.Type.Sliced;
            btImg.sprite = UGUIParser.LayerNode2Sprite(background, useSliceSp);
            UGUIParser.SetRectTransform(background, button);
            UGUIParser.SetTextStyle(text, uiRoot.GetComponentInChildren<TextMeshProUGUI>());

            var useSpriteSwap = highlight != null || press != null || select != null || disable != null;
            button.transition = useSpriteSwap ? Selectable.Transition.SpriteSwap : Selectable.Transition.ColorTint;
            if (button.transition == Selectable.Transition.SpriteSwap)
            {
                var spState = new SpriteState();
                spState.highlightedSprite = UGUIParser.LayerNode2Sprite(highlight, useSliceSp);
                spState.pressedSprite = UGUIParser.LayerNode2Sprite(press, useSliceSp);
                spState.selectedSprite = UGUIParser.LayerNode2Sprite(select, useSliceSp);
                spState.disabledSprite = UGUIParser.LayerNode2Sprite(disable, useSliceSp);
                button.spriteState = spState;
            }
        }
    }
}
#endif