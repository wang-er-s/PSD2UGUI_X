/*
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
    public class TextHelper : UIHelperBase
    {
        [SerializeField] private PsdLayerNode text;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(text);
        }

        public override void ParseAndAttachUIElements()
        {
            if (LayerNode.IsTextLayer(out var _))
                text = LayerNode;
            else
                LayerNode.SetUIType(UGUIParser.Instance.DefaultImage);
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var textCom = uiRoot.GetComponentInChildren<Text>();
            UGUIParser.SetTextStyle(text, textCom);

            UGUIParser.SetRectTransform(text, textCom);
        }
    }
}
#endif