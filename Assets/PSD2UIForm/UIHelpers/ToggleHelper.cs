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
    public class ToggleHelper : UIHelperBase
    {
        [SerializeField]
        private PsdLayerNode background;

        [SerializeField]
        private PsdLayerNode checkmark;

        [SerializeField]
        private PsdLayerNode label;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(background, checkmark, label);
        }

        public override void ParseAndAttachUIElements()
        {
            background = LayerNode.FindSubLayerNode(GUIType.Background, GUIType.Image, GUIType.RawImage);
            checkmark = LayerNode.FindSubLayerNode(GUIType.Toggle_Checkmark);
            label = LayerNode.FindSubLayerNode(GUIType.Toggle_Label, GUIType.Text, GUIType.TMPText);
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var tgCom = uiRoot.GetComponent<Toggle>();
            LayerNode.SetRectTransform(tgCom);

            var bgCom = tgCom.targetGraphic as Image;
            if (bgCom != null)
            {
                bgCom.sprite = background.LayerNode2Sprite();
                background.SetRectTransform(bgCom);
            }

            var markCom = tgCom.graphic as Image;
            if (markCom != null)
            {
                markCom.sprite = checkmark.LayerNode2Sprite();
                checkmark.SetRectTransform(markCom);
            }

            var textCom = tgCom.transform.Find("Label")?.GetComponent<Text>();
            if (label != null)
            {
                if (textCom != null) textCom.gameObject.SetActive(label != null);
                label.SetTextStyle(textCom);
                label.SetRectTransform(textCom);
            }
            else
            {
                Object.DestroyImmediate(textCom.gameObject);
            }
        }
    }
}
#endif