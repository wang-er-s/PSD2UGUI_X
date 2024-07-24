﻿/*
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
    public class DropdownHelper : UIHelperBase
    {
        [SerializeField]
        private PsdLayerNode background;

        [SerializeField]
        private PsdLayerNode label;

        [SerializeField]
        private PsdLayerNode arrow;

        [SerializeField]
        private PsdLayerNode scrollView;

        [SerializeField]
        private PsdLayerNode toggleItem;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(background, label, arrow, scrollView, toggleItem);
        }

        public override void ParseAndAttachUIElements()
        {
            background = LayerNode.FindSubLayerNode(GUIType.Background, GUIType.Image, GUIType.RawImage);
            label = LayerNode.FindSubLayerNode(GUIType.Dropdown_Label, GUIType.Text, GUIType.TMPText);
            arrow = LayerNode.FindSubLayerNode(GUIType.Dropdown_Arrow);
            scrollView = LayerNode.FindSubLayerNode(GUIType.ScrollView);
            toggleItem = LayerNode.FindSubLayerNode(GUIType.Toggle);
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var dpd = uiRoot.GetComponent<Dropdown>();
            background.SetRectTransform(dpd);
            var bgImg = dpd.targetGraphic as Image;
            bgImg.sprite = background.LayerNode2Sprite() ?? bgImg.sprite;

            label.SetTextStyle(dpd.captionText);
            label.SetRectTransform(dpd.captionText);
            var arrowImg = dpd.transform.Find("Arrow")?.GetComponent<Image>();
            if (arrowImg != null)
            {
                arrow.SetRectTransform(arrowImg);
                arrowImg.sprite = arrow.LayerNode2Sprite();
            }

            if (scrollView != null)
            {
                var svTmp = uiRoot.GetComponentInChildren<ScrollRect>(true);
                var sViewGo = scrollView.GetComponent<ScrollViewHelper>()?.CreateUI(svTmp.gameObject);
                if (sViewGo != null)
                {
                    var sViewRect = sViewGo.GetComponent<RectTransform>();
                    scrollView.SetRectTransform(sViewRect);
                    sViewRect.anchorMin = Vector2.zero;
                    sViewRect.anchorMax = new Vector2(1, 0);
                    sViewRect.anchoredPosition = new Vector2(0, -2);
                }
            }
            else //若没有滚动列表元素,则隐藏默认滚动列表的元素
            {
                var svTmp = uiRoot.GetComponentInChildren<ScrollRect>(true);
                svTmp.GetComponent<Image>().enabled = false;
                if (svTmp.horizontalScrollbar != null)
                {
                    var hbarTmp = svTmp.horizontalScrollbar;
                    svTmp.horizontalScrollbar = null;
                    hbarTmp.gameObject.SetActive(false);
                }

                if (svTmp.verticalScrollbar != null)
                {
                    var vbarTmp = svTmp.verticalScrollbar;
                    svTmp.verticalScrollbar = null;
                    vbarTmp.gameObject.SetActive(false);
                }
            }

            if (toggleItem != null)
            {
                var itemTmp = dpd.itemText != null ? dpd.itemText.transform.parent : null;
                if (itemTmp != null) toggleItem.GetComponent<ToggleHelper>()?.CreateUI(itemTmp.gameObject);
            }

            var scrollRect = uiRoot.GetComponentInChildren<ScrollRect>(true);
            if (scrollRect != null)
            {
                var layerout = scrollRect.content?.GetComponent<LayoutGroup>();
                if (layerout != null) layerout.enabled = true;
                var sizeFilter = scrollRect.content?.GetComponent<ContentSizeFitter>();
                if (sizeFilter != null) sizeFilter.enabled = true;
            }
        }
    }
}
#endif