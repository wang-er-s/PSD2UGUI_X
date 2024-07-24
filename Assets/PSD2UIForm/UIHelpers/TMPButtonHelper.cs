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
    public class TMPButtonHelper : UIHelperBase
    {
        [SerializeField]
        private PsdLayerNode background;

        [SerializeField]
        private PsdLayerNode text;

        [Header("Sprite Swap:")]
        [SerializeField]
        private PsdLayerNode highlight;

        [SerializeField]
        private PsdLayerNode press;

        [SerializeField]
        private PsdLayerNode select;

        [SerializeField]
        private PsdLayerNode disable;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(background, text, highlight, press, select, disable);
        }

        public override void ParseAndAttachUIElements()
        {
            background = LayerNode;
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
            btImg.sprite = background.LayerNode2Sprite();
            background.SetRectTransform(button);
            if (text == null)
            {
                DestroyImmediate(uiRoot.GetComponentInChildren<TextMeshProUGUI>().gameObject);
            }
            else
            {
                text.SetTextStyle(uiRoot.GetComponentInChildren<TextMeshProUGUI>());
            }

            var useSpriteSwap = highlight != null || press != null || select != null || disable != null;
            button.transition = useSpriteSwap ? Selectable.Transition.SpriteSwap : Selectable.Transition.ColorTint;
            if (button.transition == Selectable.Transition.SpriteSwap)
            {
                var spState = new SpriteState();
                spState.highlightedSprite = highlight.LayerNode2Sprite();
                spState.pressedSprite = press.LayerNode2Sprite();
                spState.selectedSprite = select.LayerNode2Sprite();
                spState.disabledSprite = disable.LayerNode2Sprite();
                button.spriteState = spState;
            }
        }
    }
}
#endif