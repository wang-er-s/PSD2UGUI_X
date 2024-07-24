using System;
using System.IO;
using System.Text.RegularExpressions;
using Aspose.PSD.FileFormats.Psd.Layers.FillLayers;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGF.EditorTools.Psd2UGUI
{
    public partial class PsdLayerNode
    {
        /// <summary>
        ///     根据图层大小和位置设置UI节点大小和位置
        /// </summary>
        public void SetRectTransform(Component uiNode, bool pos = true,
            bool width = true, bool height = true, int extSize = 0)
        {
            if (uiNode != null)
            {
                var rect = LayerRect;
                var rectTransform = uiNode.GetComponent<RectTransform>();
                if (width)
                    rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rect.size.x + extSize);
                if (height) rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rect.size.y + extSize);
                if (pos)
                    //rectTransform.position = rect.position + rectTransform.rect.size * (rectTransform.pivot - Vector2.one * 0.5f) * 0.01f;
                    rectTransform.SetPositionAndRotation(
                        rect.position + rectTransform.rect.size * (rectTransform.pivot - Vector2.one * 0.5f) * 0.01f,
                        Quaternion.identity);
            }
        }

        /// <summary>
        ///     把LayerNode图片保存到本地并返回
        /// </summary>
        /// <param name="layerNode"></param>
        /// <returns></returns>
        public Texture2D LayerNode2Texture()
        {
            var spAssetName = ExportImageAsset();
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(spAssetName);
            return texture;
        }

        /// <summary>
        ///     把LayerNode图片保存到本地并返回
        /// </summary>
        /// <returns></returns>
        public Sprite LayerNode2Sprite()
        {
            var spAssetName = ExportImageAsset();
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spAssetName);
            return sprite;
            // if (sprite != null)
            // {
            //     if (auto9Slice)
            //     {
            //         var spImpt = AssetImporter.GetAtPath(spAssetName) as TextureImporter;
            //         var rawReadable = spImpt.isReadable;
            //         if (!rawReadable)
            //         {
            //             spImpt.isReadable = true;
            //             spImpt.SaveAndReimport();
            //         }
            //
            //         if (spImpt.spriteBorder == Vector4.zero)
            //         {
            //             spImpt.spriteBorder =
            //                 CalculateTexture9SliceBorder(sprite.texture, BindPsdLayer.Opacity);
            //             spImpt.isReadable = rawReadable;
            //             spImpt.SaveAndReimport();
            //         }
            //     }
            //
            //     return sprite;
            // }
            //
            // return null;
        }

        private string ExportImageAsset()
        {
            var exportDir = Psd2UIFormConverter.Instance.GetUIFormImagesOutputDir();
            var imgName = string.Format("{0}", string.IsNullOrWhiteSpace(name) ? UIType.ToString() : name,
                BindPsdLayerIndex);
            return Path.Combine(exportDir, imgName + ".png");
            // string assetName = null;
            // if (RefreshLayerTexture())
            // {
            //     var bytes = PreviewTexture.EncodeToPNG();
            //     var imgName = string.Format("{0}_{1}", string.IsNullOrWhiteSpace(name) ? UIType.ToString() : name,
            //         BindPsdLayerIndex);
            //     if (IsSingleImg()) imgName = GetName();
            //
            //     var exportDir = Psd2UIFormConverter.Instance.GetUIFormImagesOutputDir();
            //     if (!Directory.Exists(exportDir))
            //     {
            //         try
            //         {
            //             Directory.CreateDirectory(exportDir);
            //             AssetDatabase.Refresh();
            //         }
            //         catch (Exception)
            //         {
            //             return null;
            //         }
            //     }
            //
            //     var imgFileName = Path.Combine(exportDir, imgName + ".png");
            //     if (!File.Exists(imgFileName))
            //         File.WriteAllBytes(imgFileName, bytes);
            //     assetName = imgFileName;
            //     AssetDatabase.Refresh();
            // }
            //
            // return assetName;
        }

        /// <summary>
        ///     自动计算贴图的 9宫 Border
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="alphaThreshold">0-255</param>
        /// <returns></returns>
        private Vector4 CalculateTexture9SliceBorder(Texture2D texture, byte alphaThreshold = 3)
        {
            var width = texture.width;
            var height = texture.height;

            var pixels = texture.GetPixels32();
            var minX = width;
            var minY = height;
            var maxX = 0;
            var maxY = 0;

            // 寻找不透明像素的最小和最大边界
            for (var y = 0; y < height; y++)
            for (var x = 0; x < width; x++)
            {
                var pixelIndex = y * width + x;
                var pixel = pixels[pixelIndex];

                if (pixel.a >= alphaThreshold)
                {
                    minX = Mathf.Min(minX, x);
                    minY = Mathf.Min(minY, y);
                    maxX = Mathf.Max(maxX, x);
                    maxY = Mathf.Max(maxY, y);
                }
            }

            // 计算最优的borderSize
            var borderSizeX = (maxX - minX) / 3;
            var borderSizeY = (maxY - minY) / 3;
            var borderSize = Mathf.Min(borderSizeX, borderSizeY);

            // 根据边界和Border Size计算Nine Slice Border
            var left = minX + borderSize;
            var right = maxX - borderSize;
            var top = minY + borderSize;
            var bottom = maxY - borderSize;

            // 确保边界在纹理范围内
            left = Mathf.Clamp(left, 0, width - 1);
            right = Mathf.Clamp(right, 0, width - 1);
            top = Mathf.Clamp(top, 0, height - 1);
            bottom = Mathf.Clamp(bottom, 0, height - 1);

            return new Vector4(left, top, width - right, height - bottom);
        }

        /// <summary>
        ///     把PS的字体样式同步设置到UGUI Text
        /// </summary>
        /// <param name="txtLayer"></param>
        /// <param name="text"></param>
        public void SetTextStyle(Text text)
        {
            if (text == null) return;
            if (ParseTextLayerInfo(out var str, out var size, out var charSpace,
                    out var lineSpace, out var col, out var style, out var tmpStyle, out var fName))
            {
                var tFont = FindFontAsset(fName);
                if (tFont != null) text.font = tFont;
                text.text = str;
                text.fontSize = size;
                text.fontStyle = style;
                text.color = col;
                text.lineSpacing = lineSpace;
            }
        }

        /// <summary>
        ///     把PS的字体样式同步设置到TextMeshProUGUI
        /// </summary>
        /// <param name="txtLayer"></param>
        /// <param name="text"></param>
        public void SetTextStyle(TextMeshProUGUI text)
        {
            if (ParseTextLayerInfo(out var str, out var size, out var charSpace,
                    out var lineSpace, out var col, out var style, out var tmpStyle, out var fName))
            {
                var tFont = FindTMPFontAsset(fName);
                if (tFont != null) text.font = tFont;
                text.text = str;
                text.fontSize = size;
                text.fontStyle = tmpStyle;
                text.color = col;
                text.characterSpacing = charSpace;
                text.lineSpacing = lineSpace;
            }
        }

        /// <summary>
        ///     根据字体名查找TMP_FontAsset
        /// </summary>
        /// <param name="fontName"></param>
        /// <returns></returns>
        private TMP_FontAsset FindTMPFontAsset(string fontName)
        {
            var fixedFontName = GetFixedFontName(fontName);
            var fontGuids = AssetDatabase.FindAssets("t:TMP_FontAsset");
            foreach (var guid in fontGuids)
            {
                var fontPath = AssetDatabase.GUIDToAssetPath(guid);
                var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
                if (font != null && (font.faceInfo.familyName == fontName || font.faceInfo.familyName == fixedFontName))
                    return font;
            }

            return null;
        }

        /// <summary>
        ///     根据字体名查找Font Asset
        /// </summary>
        /// <param name="fontName"></param>
        /// <returns></returns>
        private Font FindFontAsset(string fontName)
        {
            var fixedFontName = GetFixedFontName(fontName);
            var fontGuids = AssetDatabase.FindAssets("t:font");
            foreach (var guid in fontGuids)
            {
                var fontPath = AssetDatabase.GUIDToAssetPath(guid);
                var font = AssetImporter.GetAtPath(fontPath) as TrueTypeFontImporter;
                if (font != null && (font.fontTTFName == fontName || font.fontTTFName == fixedFontName))
                    return AssetDatabase.LoadAssetAtPath<Font>(fontPath);
            }

            return null;
        }


        /// <summary>
        ///     Warning:Unity导入字库字体FamilyName的特殊字符会被替换为空格,导致按原本的字体名找不到字体
        ///     将字体名特殊字符替换为空格以解决找不到字体的问题
        /// </summary>
        /// <param name="fontName"></param>
        /// <returns></returns>
        private string GetFixedFontName(string fontName)
        {
            var fixedFontName = Regex.Replace(fontName, "[^A-Za-z0-9]+", " ");
            return fixedFontName;
        }

        public Color LayerNode2Color(Color defaultColor)
        {
            if (BindPsdLayer is FillLayer fillLayer)
            {
                var layerColor = fillLayer.GetPixel(fillLayer.Width / 2, fillLayer.Height / 2);
                return new Color(layerColor.R, layerColor.G, layerColor.B, fillLayer.Opacity) / 255;
            }

            return defaultColor;
        }

        private bool ParseTextLayerInfo(out string text, out int fontSize, out float characterSpace,
            out float lineSpace, out Color fontColor, out FontStyle fontStyle,
            out FontStyles tmpFontStyle, out string fontName)
        {
            text = null;
            fontSize = 0;
            characterSpace = 0f;
            lineSpace = 0f;
            fontColor = Color.white;
            fontStyle = FontStyle.Normal;
            tmpFontStyle = FontStyles.Normal;
            fontName = null;
            if (IsTextLayer(out var txtLayer))
            {
                text = txtLayer.Text;
                fontSize = (int)(txtLayer.Font.Size * txtLayer.TransformMatrix[3]);
                fontColor = new Color(txtLayer.TextColor.R, txtLayer.TextColor.G, txtLayer.TextColor.B,
                    txtLayer.Opacity) / 255;
                if (txtLayer.Font.Style.HasFlag(Aspose.PSD.FontStyle.Bold) &&
                    txtLayer.Font.Style.HasFlag(Aspose.PSD.FontStyle.Italic))
                    fontStyle = FontStyle.BoldAndItalic;
                else if (txtLayer.Font.Style.HasFlag(Aspose.PSD.FontStyle.Bold))
                    fontStyle = FontStyle.Bold;
                else if (txtLayer.Font.Style.HasFlag(Aspose.PSD.FontStyle.Italic))
                    fontStyle = FontStyle.Italic;
                else
                    fontStyle = FontStyle.Normal;

                if (txtLayer.Font.Style.HasFlag(Aspose.PSD.FontStyle.Italic)) tmpFontStyle |= FontStyles.Italic;

                if (txtLayer.Font.Style.HasFlag(Aspose.PSD.FontStyle.Bold)) tmpFontStyle |= FontStyles.Bold;

                if (txtLayer.Font.Style.HasFlag(Aspose.PSD.FontStyle.Underline)) tmpFontStyle |= FontStyles.Underline;

                if (txtLayer.Font.Style.HasFlag(Aspose.PSD.FontStyle.Strikeout))
                    tmpFontStyle |= FontStyles.Strikethrough;

                fontName = txtLayer.Font.Name;
                if (txtLayer.TextData.Items.Length > 0)
                {
                    var txtData = txtLayer.TextData.Items[0];
                    characterSpace = txtData.Style.Tracking * 0.1f;
                    lineSpace = (float)txtData.Style.Leading * 0.1f;
                }

                return true;
            }

            return false;
        }
    }
}