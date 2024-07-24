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
    public class MaskHelper : UIHelperBase
    {
        [SerializeField]
        private PsdLayerNode mask;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(mask);
        }

        public override void ParseAndAttachUIElements()
        {
            mask = LayerNode;
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var imgCom = uiRoot.GetComponentInChildren<Image>();
            mask.SetRectTransform(imgCom);
            imgCom.sprite = mask.LayerNode2Sprite();
        }
    }
}
#endif