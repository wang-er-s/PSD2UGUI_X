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
        [SerializeField]
        private PsdLayerNode image;

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
            image.SetRectTransform(imgCom);
            imgCom.sprite = image.LayerNode2Sprite();
            if (IsSlicedSprite(imgCom.sprite))
            {
                imgCom.type = Image.Type.Sliced;
            }
        }
        
        private static bool IsSlicedSprite(Sprite sprite)
        {
            Vector4 border = sprite.border;
            return border.x > 0 || border.y > 0 || border.z > 0 || border.w > 0;
        }
    }
}
#endif