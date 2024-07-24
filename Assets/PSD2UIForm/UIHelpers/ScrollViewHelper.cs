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
    public class ScrollViewHelper : UIHelperBase
    {
        [SerializeField]
        private PsdLayerNode background;

        [SerializeField]
        private PsdLayerNode viewport;

        [SerializeField]
        private PsdLayerNode horizontalBarBG;

        [SerializeField]
        private PsdLayerNode horizontalBar;

        [SerializeField]
        private PsdLayerNode verticalBarBG;

        [SerializeField]
        private PsdLayerNode verticalBar;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(background, viewport, horizontalBarBG, horizontalBar, verticalBarBG,
                verticalBar);
        }

        public override void ParseAndAttachUIElements()
        {
            background = LayerNode.FindSubLayerNode(GUIType.Background, GUIType.Image, GUIType.RawImage);
            viewport = LayerNode.FindSubLayerNode(GUIType.ScrollView_Viewport, GUIType.Mask);
            horizontalBarBG = LayerNode.FindSubLayerNode(GUIType.ScrollView_HorizontalBarBG);
            horizontalBar = LayerNode.FindSubLayerNode(GUIType.ScrollView_HorizontalBar);
            verticalBarBG = LayerNode.FindSubLayerNode(GUIType.ScrollView_VerticalBarBG);
            verticalBar = LayerNode.FindSubLayerNode(GUIType.ScrollView_VerticalBar);
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var listView = uiRoot.GetComponent<ScrollRect>();
            background.SetRectTransform(listView);
            var bgCom = listView.GetComponent<Image>();
            if (bgCom != null)
            {
                bgCom.sprite = background.LayerNode2Sprite();
                if (viewport == null)
                {
                    var maskImg = listView.viewport.GetComponent<Image>();
                    maskImg.sprite = bgCom.sprite;
                }
            }

            if (viewport != null)
            {
                var maskImg = listView.viewport.GetComponent<Image>();
                maskImg.sprite = viewport.LayerNode2Sprite();
            }

            var hbar = listView.horizontalScrollbar;
            var vbar = listView.verticalScrollbar;

            if (horizontalBarBG != null && hbar != null)
            {
                var hbarBg = hbar.GetComponent<Image>();
                hbarBg.sprite = horizontalBarBG.LayerNode2Sprite();
                horizontalBarBG.SetRectTransform(hbarBg, false, false);
                var hbarRect = hbar.GetComponent<RectTransform>();
                hbarRect.anchorMin = new Vector2(1, 0);
                hbarRect.anchorMax = Vector2.one;
            }
            else
            {
                var hbarGo = listView.horizontalScrollbar;
                listView.horizontalScrollbar = null;
                if (hbarGo != null) hbarGo.gameObject.SetActive(false);
            }

            if (verticalBarBG != null && vbar != null)
            {
                var vbarBg = vbar.GetComponent<Image>();
                vbarBg.sprite = verticalBarBG.LayerNode2Sprite();
                verticalBarBG.SetRectTransform(vbarBg, false, true, false);
                var vbarRect = vbar.GetComponent<RectTransform>();
                vbarRect.anchorMin = new Vector2(1, 0);
                vbarRect.anchorMax = Vector2.one;
            }
            else
            {
                var vbarGo = listView.verticalScrollbar;
                listView.verticalScrollbar = null;
                if (vbarGo != null) vbarGo.gameObject.SetActive(false);
            }

            if (horizontalBar != null && hbar != null)
            {
                var hbarHandle = hbar.targetGraphic as Image;
                hbarHandle.sprite = horizontalBar.LayerNode2Sprite();
                //horizontalBar.SetRectTransform( hbarHandle, false, true, true);
            }

            if (verticalBar != null && vbar != null)
            {
                var vbarHandle = vbar.targetGraphic as Image;
                vbarHandle.sprite = verticalBar.LayerNode2Sprite();
                //verticalBar.SetRectTransform( vbarHandle, false, true, true);
            }
        }
    }
}
#endif