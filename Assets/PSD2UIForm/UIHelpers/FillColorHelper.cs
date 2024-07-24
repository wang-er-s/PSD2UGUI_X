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
    public class FillColorHelper : UIHelperBase
    {
        [SerializeField]
        private PsdLayerNode fillColor;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(fillColor);
        }

        public override void ParseAndAttachUIElements()
        {
            // if (LayerNode.LayerType != PsdLayerType.FillLayer)
            // {
            //     LayerNode.SetUIType(GUIType.Image);
            //     return;
            // }
            //
            fillColor = LayerNode;
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var imgCom = uiRoot.GetComponentInChildren<RawImage>();
            fillColor.SetRectTransform(imgCom);
            imgCom.color = fillColor.LayerNode2Color(imgCom.color);
        }
    }
}
#endif