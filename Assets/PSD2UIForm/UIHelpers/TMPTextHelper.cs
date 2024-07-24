/*
    联系作者:
    https://blog.csdn.net/final5788
    https://github.com/sunsvip
 */

#if UNITY_EDITOR
using TMPro;
using UnityEngine;

namespace UGF.EditorTools.Psd2UGUI
{
    [DisallowMultipleComponent]
    public class TMPTextHelper : UIHelperBase
    {
        [SerializeField]
        private PsdLayerNode text;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(text);
        }

        public override void ParseAndAttachUIElements()
        {
            if (LayerNode.IsTextLayer(out var _))
                text = LayerNode;
            else
                LayerNode.SetUIType(GUIType.Image);
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var textCom = uiRoot.GetComponentInChildren<TextMeshProUGUI>();
            text.SetTextStyle(textCom);
            text.SetRectTransform(textCom);
        }
    }
}
#endif