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
    public class RawImageHelper : UIHelperBase
    {
        [SerializeField] private PsdLayerNode rawImage;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(rawImage);
        }

        public override void ParseAndAttachUIElements()
        {
            rawImage = LayerNode;
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var imgCom = uiRoot.GetComponentInChildren<RawImage>();
            UGUIParser.SetRectTransform(rawImage, imgCom);
            imgCom.texture = UGUIParser.LayerNode2Texture(rawImage);
        }
    }
}
#endif