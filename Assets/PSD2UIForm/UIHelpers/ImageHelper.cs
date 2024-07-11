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
    public class ImageHelper : UIHelperBase
    {
        [SerializeField] private PsdLayerNode image;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(image);
        }

        public override void ParseAndAttachUIElements()
        {
            image = LayerNode;
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var imgCom = uiRoot.GetComponentInChildren<Image>();
            UGUIParser.SetRectTransform(image, imgCom);
            imgCom.sprite = UGUIParser.LayerNode2Sprite(image, imgCom.type == Image.Type.Sliced);
        }
    }
}
#endif