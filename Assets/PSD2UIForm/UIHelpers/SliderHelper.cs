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
    public class SliderHelper : UIHelperBase
    {
        [SerializeField]
        private PsdLayerNode background;

        [SerializeField]
        private PsdLayerNode fill;

        [SerializeField]
        private PsdLayerNode handle;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(background, fill, handle);
        }

        public override void ParseAndAttachUIElements()
        {
            background = LayerNode.FindSubLayerNode(GUIType.Background, GUIType.Image, GUIType.RawImage);
            fill = LayerNode.FindSubLayerNode(GUIType.Slider_Fill);
            handle = LayerNode.FindSubLayerNode(GUIType.Slider_Handle);
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var slider = uiRoot.GetComponent<Slider>();
            background.SetRectTransform(slider);

            var bg = uiRoot.transform.Find("Background")?.GetComponent<Image>();
            if (bg != null) bg.sprite = background.LayerNode2Sprite();
            var fillImg = slider.fillRect?.GetComponent<Image>();
            if (fillImg != null) fillImg.sprite = fill.LayerNode2Sprite();
            var handleImg = slider.handleRect?.GetComponent<Image>();
            if (handleImg != null)
            {
                var noHandle = handle == null;
                handleImg.gameObject.SetActive(!noHandle);
                slider.transition = noHandle ? Selectable.Transition.None : Selectable.Transition.ColorTint;
                slider.interactable = !noHandle;
                handle.LayerNode2Sprite();
                handle.SetRectTransform(handleImg, false);
            }
        }
    }
}
#endif