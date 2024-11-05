using UnityEngine;
using System.IO;
using System.Text;
using System;

namespace ToolBox
{
    public class GUISkinSerializer
    {
        public static void SaveGUISkin(GUISkin skin, string filePath)
        {
            StringBuilder sb = new StringBuilder();

            SaveGUIStyle(sb, "box", skin.box);
            SaveGUIStyle(sb, "button", skin.button);
            SaveGUIStyle(sb, "toggle", skin.toggle);
            SaveGUIStyle(sb, "label", skin.label);
            SaveGUIStyle(sb, "textField", skin.textField);
            SaveGUIStyle(sb, "textArea", skin.textArea);
            SaveGUIStyle(sb, "window", skin.window);
            SaveGUIStyle(sb, "horizontalSlider", skin.horizontalSlider);
            SaveGUIStyle(sb, "horizontalSliderThumb", skin.horizontalSliderThumb);
            SaveGUIStyle(sb, "verticalSlider", skin.verticalSlider);
            SaveGUIStyle(sb, "verticalSliderThumb", skin.verticalSliderThumb);
            SaveGUIStyle(sb, "horizontalScrollbar", skin.horizontalScrollbar);
            SaveGUIStyle(sb, "horizontalScrollbarThumb", skin.horizontalScrollbarThumb);
            SaveGUIStyle(sb, "horizontalScrollbarLeftButton", skin.horizontalScrollbarLeftButton);
            SaveGUIStyle(sb, "horizontalScrollbarRightButton", skin.horizontalScrollbarRightButton);
            SaveGUIStyle(sb, "verticalScrollbar", skin.verticalScrollbar);
            SaveGUIStyle(sb, "verticalScrollbarThumb", skin.verticalScrollbarThumb);
            SaveGUIStyle(sb, "verticalScrollbarUpButton", skin.verticalScrollbarUpButton);
            SaveGUIStyle(sb, "verticalScrollbarDownButton", skin.verticalScrollbarDownButton);
            SaveGUIStyle(sb, "scrollView", skin.scrollView);

            foreach (GUIStyle style in skin.customStyles)
            {
                SaveGUIStyle(sb, style.name, style);
            }

            sb.AppendLine("DoubleClickSelectsWord:" + skin.settings.doubleClickSelectsWord);
            sb.AppendLine("TripleClickSelectsLine:" + skin.settings.tripleClickSelectsLine);

            sb.AppendLine("CursorColor:" + ColorUtility.ToHtmlStringRGBA(skin.settings.cursorColor));
            sb.AppendLine("SelectionColor:" + ColorUtility.ToHtmlStringRGBA(skin.settings.selectionColor));

            sb.AppendLine("CursorFlashSpeed:" + skin.settings.cursorFlashSpeed);

            File.WriteAllText(filePath, sb.ToString());
        }

        private static void SaveGUIStyle(StringBuilder sb, string styleName, GUIStyle style)
        {
            sb.AppendLine("StyleName:" + styleName);
            sb.AppendLine("FontSize:" + style.fontSize);

            sb.AppendLine("Padding:" + RectOffsetToString(style.padding));
            sb.AppendLine("Margin:" + RectOffsetToString(style.margin));
            sb.AppendLine("Border:" + RectOffsetToString(style.border));
            sb.AppendLine("Overflow:" + RectOffsetToString(style.overflow));
            sb.AppendLine("ContentOffset:" + Vector2ToString(style.contentOffset));
            sb.AppendLine("FixedSize:" + Vector2ToString(new Vector2(style.fixedWidth, style.fixedHeight)));

            sb.AppendLine("FontStyle:" + ((int)style.fontStyle));
            sb.AppendLine("Alignment:" + ((int)style.alignment));
            sb.AppendLine("TextClipping:" + ((int)style.clipping));
            sb.AppendLine("ImagePosition:" + ((int)style.imagePosition));

            sb.AppendLine("WordWrap:" + style.wordWrap);
            sb.AppendLine("RichText:" + style.richText);
            sb.AppendLine("StretchHeight:" + style.stretchHeight);
            sb.AppendLine("StretchWidth:" + style.stretchWidth);

            sb.AppendLine("NormalTextColor:" + ColorUtility.ToHtmlStringRGBA(style.normal.textColor));
            sb.AppendLine("HoverTextColor:" + ColorUtility.ToHtmlStringRGBA(style.hover.textColor));
            sb.AppendLine("ActiveTextColor:" + ColorUtility.ToHtmlStringRGBA(style.active.textColor));
            sb.AppendLine("FocusedTextColor:" + ColorUtility.ToHtmlStringRGBA(style.focused.textColor));
            sb.AppendLine("OnNormalTextColor:" + ColorUtility.ToHtmlStringRGBA(style.onNormal.textColor));
            sb.AppendLine("OnHoverTextColor:" + ColorUtility.ToHtmlStringRGBA(style.onHover.textColor));
            sb.AppendLine("OnActiveTextColor:" + ColorUtility.ToHtmlStringRGBA(style.onActive.textColor));
            sb.AppendLine("OnFocusedTextColor:" + ColorUtility.ToHtmlStringRGBA(style.onFocused.textColor));

            sb.AppendLine();
        }

        public static GUISkin LoadGUISkin(string filePath, GUISkin replace = null)
        {
            GUISkin skin = ScriptableObject.CreateInstance<GUISkin>();
            skin.name = "NewGUISkin";
            skin.customStyles = new GUIStyle[0];

            string[] lines = filePath.Split(new[] { "\r\n", "\r", "\n" }, System.StringSplitOptions.None);

            GUIStyle currentStyle = null;
            string currentStyleName = string.Empty;

            foreach (string line in lines)
            {
                if (line.StartsWith("StyleName:"))
                {
                    if (currentStyle != null)
                    {
                        AssignStyleToSkin(skin, currentStyleName, currentStyle);
                    }
                    currentStyleName = line.Substring("StyleName:".Length);
                    currentStyle = new GUIStyle();
                }
                else if (line.StartsWith("FontSize:"))
                {
                    currentStyle.fontSize = int.Parse(line.Substring("FontSize:".Length));
                }
                else if (line.StartsWith("Padding:"))
                {
                    currentStyle.padding = StringToRectOffset(line.Substring("Padding:".Length));
                }
                else if (line.StartsWith("Margin:"))
                {
                    currentStyle.margin = StringToRectOffset(line.Substring("Margin:".Length));
                }
                else if (line.StartsWith("Border:"))
                {
                    currentStyle.border = StringToRectOffset(line.Substring("Border:".Length));
                }
                else if (line.StartsWith("Overflow:"))
                {
                    currentStyle.overflow = StringToRectOffset(line.Substring("Overflow:".Length));
                }
                else if (line.StartsWith("ContentOffset:"))
                {
                    currentStyle.contentOffset = StringToVector2(line.Substring("ContentOffset:".Length));
                }
                else if (line.StartsWith("FixedSize:"))
                {
                    Vector2 fixedSize = StringToVector2(line.Substring("FixedSize:".Length));
                    currentStyle.fixedWidth = fixedSize.x;
                    currentStyle.fixedHeight = fixedSize.y;
                }
                else if (line.StartsWith("FontStyle:"))
                {
                    currentStyle.fontStyle = (FontStyle)int.Parse(line.Substring("FontStyle:".Length));
                }
                else if (line.StartsWith("Alignment:"))
                {
                    currentStyle.alignment = (TextAnchor)int.Parse(line.Substring("Alignment:".Length));
                }
                else if (line.StartsWith("TextClipping:"))
                {
                    currentStyle.clipping = (TextClipping)int.Parse(line.Substring("TextClipping:".Length));
                }
                else if (line.StartsWith("ImagePosition:"))
                {
                    currentStyle.imagePosition = (ImagePosition)int.Parse(line.Substring("ImagePosition:".Length));
                }
                else if (line.StartsWith("WordWrap:"))
                {
                    currentStyle.wordWrap = bool.Parse(line.Substring("WordWrap:".Length));
                }
                else if (line.StartsWith("RichText:"))
                {
                    currentStyle.richText = bool.Parse(line.Substring("RichText:".Length));
                }
                else if (line.StartsWith("StretchHeight:"))
                {
                    currentStyle.stretchHeight = bool.Parse(line.Substring("StretchHeight:".Length));
                }
                else if (line.StartsWith("StretchWidth:"))
                {
                    currentStyle.stretchWidth = bool.Parse(line.Substring("StretchWidth:".Length));
                }
                else if (line.StartsWith("NormalTextColor:"))
                {
                    Color normalTextColor;
                    ColorUtility.TryParseHtmlString("#" + line.Substring("NormalTextColor:".Length), out normalTextColor);
                    currentStyle.normal.textColor = normalTextColor;
                }
                else if (line.StartsWith("HoverTextColor:"))
                {
                    Color hoverTextColor;
                    ColorUtility.TryParseHtmlString("#" + line.Substring("HoverTextColor:".Length), out hoverTextColor);
                    currentStyle.hover.textColor = hoverTextColor;
                }
                else if (line.StartsWith("ActiveTextColor:"))
                {
                    Color activeTextColor;
                    ColorUtility.TryParseHtmlString("#" + line.Substring("ActiveTextColor:".Length), out activeTextColor);
                    currentStyle.active.textColor = activeTextColor;
                }
                else if (line.StartsWith("FocusedTextColor:"))
                {
                    Color focusedTextColor;
                    ColorUtility.TryParseHtmlString("#" + line.Substring("FocusedTextColor:".Length), out focusedTextColor);
                    currentStyle.focused.textColor = focusedTextColor;
                }
                else if (line.StartsWith("OnNormalTextColor:"))
                {
                    Color onNormalTextColor;
                    ColorUtility.TryParseHtmlString("#" + line.Substring("OnNormalTextColor:".Length), out onNormalTextColor);
                    currentStyle.onNormal.textColor = onNormalTextColor;
                }
                else if (line.StartsWith("OnHoverTextColor:"))
                {
                    Color onHoverTextColor;
                    ColorUtility.TryParseHtmlString("#" + line.Substring("OnHoverTextColor:".Length), out onHoverTextColor);
                    currentStyle.onHover.textColor = onHoverTextColor;
                }
                else if (line.StartsWith("OnActiveTextColor:"))
                {
                    Color onActiveTextColor;
                    ColorUtility.TryParseHtmlString("#" + line.Substring("OnActiveTextColor:".Length), out onActiveTextColor);
                    currentStyle.onActive.textColor = onActiveTextColor;
                }
                else if (line.StartsWith("OnFocusedTextColor:"))
                {
                    Color onFocusedTextColor;
                    ColorUtility.TryParseHtmlString("#" + line.Substring("OnFocusedTextColor:".Length), out onFocusedTextColor);
                    currentStyle.onFocused.textColor = onFocusedTextColor;
                }
                else if (line.StartsWith("DoubleClickSelectsWord:"))
                {
                    skin.settings.doubleClickSelectsWord = bool.Parse(line.Substring("DoubleClickSelectsWord:".Length));
                }
                else if (line.StartsWith("TripleClickSelectsLine:"))
                {
                    skin.settings.tripleClickSelectsLine = bool.Parse(line.Substring("TripleClickSelectsLine:".Length));
                }
                else if (line.StartsWith("CursorColor:"))
                {
                    Color cursorColor;
                    ColorUtility.TryParseHtmlString("#" + line.Substring("CursorColor:".Length), out cursorColor);
                    skin.settings.cursorColor = cursorColor;
                }
                else if (line.StartsWith("SelectionColor:"))
                {
                    Color selectionColor;
                    ColorUtility.TryParseHtmlString("#" + line.Substring("SelectionColor:".Length), out selectionColor);
                    skin.settings.selectionColor = selectionColor;
                }
                else if (line.StartsWith("CursorFlashSpeed:"))
                {
                    skin.settings.cursorFlashSpeed = float.Parse(line.Substring("CursorFlashSpeed:".Length));
                }
            }

            if (currentStyle != null)
            {
                AssignStyleToSkin(skin, currentStyleName, currentStyle);
            }

            if (replace != null)
            {
                CopyTextures(replace, skin, true);
                skin.name = replace.name;
            }

            return skin;
        }

        private static void AssignStyleToSkin(GUISkin skin, string styleName, GUIStyle style)
        {
            switch (styleName)
            {
                case "box": skin.box = style; break;
                case "button": skin.button = style; break;
                case "toggle": skin.toggle = style; break;
                case "label": skin.label = style; break;
                case "textField": skin.textField = style; break;
                case "textArea": skin.textArea = style; break;
                case "window": skin.window = style; break;
                case "horizontalSlider": skin.horizontalSlider = style; break;
                case "horizontalSliderThumb": skin.horizontalSliderThumb = style; break;
                case "verticalSlider": skin.verticalSlider = style; break;
                case "verticalSliderThumb": skin.verticalSliderThumb = style; break;
                case "horizontalScrollbar": skin.horizontalScrollbar = style; break;
                case "horizontalScrollbarThumb": skin.horizontalScrollbarThumb = style; break;
                case "horizontalScrollbarLeftButton": skin.horizontalScrollbarLeftButton = style; break;
                case "horizontalScrollbarRightButton": skin.horizontalScrollbarRightButton = style; break;
                case "verticalScrollbar": skin.verticalScrollbar = style; break;
                case "verticalScrollbarThumb": skin.verticalScrollbarThumb = style; break;
                case "verticalScrollbarUpButton": skin.verticalScrollbarUpButton = style; break;
                case "verticalScrollbarDownButton": skin.verticalScrollbarDownButton = style; break;
                case "scrollView": skin.scrollView = style; break;
                default:
                    AddStyleToCustomStyles(skin, style);
                    break;
            }
        }

        private static void AddStyleToCustomStyles(GUISkin skin, GUIStyle style)
        {
            var customStyles = skin.customStyles;
            Array.Resize(ref customStyles, customStyles.Length + 1);
            customStyles[customStyles.Length - 1] = style;
            skin.customStyles = customStyles;
        }

        private static string RectOffsetToString(RectOffset ro)
        {
            return ro.left + "," + ro.right + "," + ro.top + "," + ro.bottom;
        }

        private static RectOffset StringToRectOffset(string str)
        {
            string[] values = str.Split(',');
            return new RectOffset(
                int.Parse(values[0]),
                int.Parse(values[1]),
                int.Parse(values[2]),
                int.Parse(values[3])
            );
        }

        private static string Vector2ToString(Vector2 v)
        {
            return v.x + "," + v.y;
        }

        private static Vector2 StringToVector2(string str)
        {
            string[] values = str.Split(',');
            return new Vector2(
                float.Parse(values[0]),
                float.Parse(values[1])
            );
        }

        public static void CopyTextures(GUISkin source, GUISkin target, bool copyColors)
        {
            target.font = source.font;

            CopyStyleTextures(source.box, target.box);
            CopyStyleTextures(source.button, target.button);
            CopyStyleTextures(source.toggle, target.toggle);
            CopyStyleTextures(source.label, target.label);
            CopyStyleTextures(source.textField, target.textField);
            CopyStyleTextures(source.textArea, target.textArea);
            CopyStyleTextures(source.window, target.window);
            CopyStyleTextures(source.horizontalSlider, target.horizontalSlider);
            CopyStyleTextures(source.horizontalSliderThumb, target.horizontalSliderThumb);
            CopyStyleTextures(source.verticalSlider, target.verticalSlider);
            CopyStyleTextures(source.verticalSliderThumb, target.verticalSliderThumb);
            CopyStyleTextures(source.horizontalScrollbar, target.horizontalScrollbar);
            CopyStyleTextures(source.horizontalScrollbarThumb, target.horizontalScrollbarThumb);
            CopyStyleTextures(source.horizontalScrollbarLeftButton, target.horizontalScrollbarLeftButton);
            CopyStyleTextures(source.horizontalScrollbarRightButton, target.horizontalScrollbarRightButton);
            CopyStyleTextures(source.verticalScrollbar, target.verticalScrollbar);
            CopyStyleTextures(source.verticalScrollbarThumb, target.verticalScrollbarThumb);
            CopyStyleTextures(source.verticalScrollbarUpButton, target.verticalScrollbarUpButton);
            CopyStyleTextures(source.verticalScrollbarDownButton, target.verticalScrollbarDownButton);
            CopyStyleTextures(source.scrollView, target.scrollView);

            for (int i = 0; i < source.customStyles.Length; i++)
            {
                if (i < target.customStyles.Length)
                {
                    CopyStyleTextures(source.customStyles[i], target.customStyles[i]);
                }
            }
        }

        private static void CopyStyleTextures(GUIStyle source, GUIStyle target)
        {
            if (source == null || target == null)
                return;

            target.normal.background = source.normal.background;
            target.hover.background = source.hover.background;
            target.active.background = source.active.background;
            target.focused.background = source.focused.background;
            target.onNormal.background = source.onNormal.background;
            target.onHover.background = source.onHover.background;
            target.onActive.background = source.onActive.background;
            target.onFocused.background = source.onFocused.background;
        }

        private static void CopyStyleTextColors(GUIStyle source, GUIStyle target)
        {
            target.normal.textColor = source.normal.textColor;
            target.hover.textColor = source.hover.textColor;
            target.active.textColor = source.active.textColor;
            target.focused.textColor = source.focused.textColor;
            target.onNormal.textColor = source.onNormal.textColor;
            target.onHover.textColor = source.onHover.textColor;
            target.onActive.textColor = source.onActive.textColor;
            target.onFocused.textColor = source.onFocused.textColor;
        }

    }
}
