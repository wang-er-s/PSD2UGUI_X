/*
    联系作者:
    https://blog.csdn.net/final5788
    https://github.com/sunsvip
 */

#if UNITY_EDITOR
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UGF.EditorTools.Psd2UGUI
{
    [DisallowMultipleComponent]
    public class TMPToggleHelper : UIHelperBase
    {
        [SerializeField] private PsdLayerNode background;
        [SerializeField] private PsdLayerNode checkmark;
        [SerializeField] private PsdLayerNode label;

        public override PsdLayerNode[] GetDependencies()
        {
            return CalculateDependencies(background, checkmark, label);
        }

        public override void ParseAndAttachUIElements()
        {
            background = LayerNode.FindSubLayerNode(GUIType.Background, GUIType.Image, GUIType.RawImage);
            checkmark = LayerNode.FindSubLayerNode(GUIType.Toggle_Checkmark);
            label = LayerNode.FindSubLayerNode(GUIType.Toggle_Label, GUIType.TMPText, GUIType.Text);
        }

        protected override void InitUIElements(GameObject uiRoot)
        {
            var tgCom = uiRoot.GetComponent<Toggle>();
            UGUIParser.SetRectTransform(LayerNode, tgCom);

            var bgCom = tgCom.targetGraphic as Image;
            if (bgCom != null)
            {
                UGUIParser.SetRectTransform(background, bgCom);
                bgCom.sprite = UGUIParser.LayerNode2Sprite(background, bgCom.type == Image.Type.Sliced);
            }

            var markCom = tgCom.graphic as Image;
            if (markCom != null)
            {
                UGUIParser.SetRectTransform(checkmark, markCom);
                markCom.sprite = UGUIParser.LayerNode2Sprite(checkmark, markCom.type == Image.Type.Sliced);
            }

            var textCom = tgCom.transform.Find("Label")?.GetComponent<TextMeshProUGUI>();
            if (textCom != null) textCom.gameObject.SetActive(label != null);
            UGUIParser.SetTextStyle(label, textCom);
            UGUIParser.SetRectTransform(label, textCom);
        }
    }
}
#endif