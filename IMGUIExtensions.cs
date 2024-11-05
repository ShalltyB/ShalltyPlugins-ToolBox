using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ToolBox.Extensions
{
	internal static class IMGUIExtensions
	{
		public static void SetGlobalFontSize(int size)
		{
			foreach (GUIStyle style in GUI.skin)
				style.fontSize = size;
			GUI.skin = GUI.skin;
		}

		public static void ResetFontSize()
		{
			SetGlobalFontSize(0);
		}

		private static readonly GUIStyle _customBoxStyle = new GUIStyle { normal = new GUIStyleState { background = Texture2D.whiteTexture } };
#if HONEYSELECT || PLAYHOME || KOIKATSU
		private static readonly Color _backgroundColor = new Color(1f, 1f, 1f, 0.5f);
#elif AISHOUJO || HONEYSELECT2
        private static readonly Color _backgroundColor = new Color(0f, 0f, 0f, 0.5f);
#endif
		public static readonly Texture2D _simpleTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

		public static void DrawBackground(Rect rect)
		{
			Color c = GUI.backgroundColor;
			GUI.backgroundColor = _backgroundColor;
			GUI.Box(rect, "", _customBoxStyle);
			GUI.backgroundColor = c;

		}


		private class DisableCameraControlOnClick : UIBehaviour, IPointerDownHandler, IPointerUpHandler, IEndDragHandler
		{
			private bool _cameraControlEnabled = true;

			public void OnPointerDown(PointerEventData eventData)
			{
				this.SetCameraControlEnabled(false);
			}

			public void OnPointerUp(PointerEventData eventData)
			{
				this.SetCameraControlEnabled(true);
			}

			public void OnEndDrag(PointerEventData eventData)
			{
				this.SetCameraControlEnabled(true);
			}

			private void SetCameraControlEnabled(bool e)
			{
				if (Camera.main != null)
				{
					CameraControl control = Camera.main.GetComponent<CameraControl>();
					if (control != null)
					{
						this._cameraControlEnabled = e;
						control.NoCtrlCondition = this.NoCtrlCondition;
					}
				}
			}

			private bool NoCtrlCondition()
			{
				return !this._cameraControlEnabled;
			}

			public override void OnDisable()
			{
				base.OnDisable();
				if (this._cameraControlEnabled == false && Camera.main?.GetComponent<CameraControl>()?.NoCtrlCondition == this.NoCtrlCondition)
					this.SetCameraControlEnabled(true);
			}
		}
		private static Canvas _canvas = null;
		public static RectTransform CreateUGUIPanelForIMGUI(bool addDisableCameraControlComponent = false)
		{
			if (_canvas == null)
			{
				GameObject g = GameObject.Find("IMGUIBackgrounds");
				if (g != null)
					_canvas = g.GetComponent<Canvas>();
				if (_canvas == null)
				{
					GameObject go = new GameObject("IMGUIBackgrounds", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
					go.hideFlags |= HideFlags.HideInHierarchy;
					_canvas = go.GetComponent<Canvas>();
					_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
					_canvas.pixelPerfect = true;
					_canvas.sortingOrder = 999;

					CanvasScaler cs = go.GetComponent<CanvasScaler>();
					cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
					cs.referencePixelsPerUnit = 100;
					cs.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

					GraphicRaycaster gr = go.GetComponent<GraphicRaycaster>();
					gr.ignoreReversedGraphics = true;
					gr.blockingObjects = GraphicRaycaster.BlockingObjects.None;
					GameObject.DontDestroyOnLoad(go);
				}
			}
			GameObject background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
			background.transform.SetParent(_canvas.transform, false);
			background.transform.localPosition = Vector3.zero;
			background.transform.localRotation = Quaternion.identity;
			background.transform.localScale = Vector3.one;
			RawImage image = background.GetComponent<RawImage>();
			image.color = new Color32(127, 127, 127, 2);
			image.raycastTarget = true;
			RectTransform rt = (RectTransform)background.transform;
			rt.anchorMin = new Vector2(0.5f, 0.5f);
			rt.anchorMax = new Vector2(0.5f, 0.5f);
			rt.pivot = new Vector2(0f, 1f);
			background.gameObject.SetActive(false);

			if (addDisableCameraControlComponent)
				rt.gameObject.AddComponent<DisableCameraControlOnClick>();

			return rt;
		}

		public static void FitRectTransformToRect(RectTransform transform, Rect rect)
		{
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_canvas.transform, new Vector2(rect.xMin, rect.yMax), _canvas.worldCamera, out Vector2 min) && RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)_canvas.transform, new Vector2(rect.xMax, rect.yMin), _canvas.worldCamera, out Vector2 max))
			{
				transform.offsetMin = new Vector2(min.x, -min.y);
				transform.offsetMax = new Vector2(max.x, -max.y);
			}
		}

        #region KKPE Extensions

        public static float _repeatTimer = 0f;
        public static bool _repeatCalled = false;
        private const float _repeatBeforeDuration = 0.5f;
        public static int _incIndex = 0;

        public static readonly Color _redColor = Color.red;
        public static readonly Color _greenColor = Color.green;
        public static readonly Color _blueColor = Color.Lerp(Color.blue, Color.cyan, 0.5f);
        public static float _inc = 1f;

        public static void UpdateRepeat()
        {
            if (_repeatCalled)
                _repeatTimer += Time.unscaledDeltaTime;
            else
                _repeatTimer = 0f;
            _repeatCalled = false;
        }

        public static bool RepeatControl()
        {
            _repeatCalled = true;
            if (Mathf.Approximately(_repeatTimer, 0f))
                return true;
            return Event.current.type == EventType.Repaint && _repeatTimer > _repeatBeforeDuration;
        }

        public static void IncEditor(int maxHeight = 76, bool label = false)
        {
            IncEditor(ref _incIndex, out _inc, maxHeight, label);
        }

        public static void IncEditor(ref int incIndex, out float inc, int maxHeight = 76, bool label = false)
        {
            GUILayout.BeginVertical();
            if (label)
            {
                GUILayout.Label("10^1", GUI.skin.box, GUILayout.MaxWidth(45));
                if (GUILayout.Button("+", GUILayout.MaxWidth(45)))
                    incIndex = Mathf.Clamp(incIndex + 1, -5, 1);

                GUILayout.BeginHorizontal(GUILayout.MaxWidth(45));
                GUILayout.FlexibleSpace();
                incIndex = Mathf.RoundToInt(GUILayout.VerticalSlider(incIndex, 1f, -5f, GUILayout.MaxHeight(maxHeight)));
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                if (GUILayout.Button("-", GUILayout.MaxWidth(45)))
                    incIndex = Mathf.Clamp(incIndex - 1, -5, 1);
                GUILayout.Label("10^-5", GUI.skin.box, GUILayout.MaxWidth(45));
            }
            else
            {
                GUILayout.BeginHorizontal(GUILayout.MaxWidth(40));

                GUILayout.BeginVertical();
                if (GUILayout.Button("+", GUILayout.MaxWidth(20), GUILayout.Height(37)))
                    incIndex = Mathf.Clamp(incIndex + 1, -5, 1);
                GUILayout.Space(1);
                if (GUILayout.Button("-", GUILayout.MaxWidth(20), GUILayout.Height(37)))
                    incIndex = Mathf.Clamp(incIndex - 1, -5, 1);
                GUILayout.EndVertical();

                GUILayout.FlexibleSpace();
                incIndex = Mathf.RoundToInt(GUILayout.VerticalSlider(incIndex, 1f, -5f, GUILayout.MaxHeight(maxHeight)));
                GUILayout.FlexibleSpace();

                GUILayout.EndHorizontal();
            }
            inc = Mathf.Pow(10, incIndex);
            GUILayout.EndVertical();
        }

        public static Vector3 Vector3Editor(Vector3 value, string xLabel = "X:\t", string yLabel = "Y:\t", string zLabel = "Z:\t", Action onValueChanged = null)
        {
            return Vector3Editor(value, _redColor, _greenColor, _blueColor, _inc, xLabel, yLabel, zLabel, onValueChanged);
        }

        public static Vector3 Vector3Editor(Vector3 value, float customInc, string xLabel = "X:\t", string yLabel = "Y:\t", string zLabel = "Z:\t", Action onValueChanged = null)
        {
            return Vector3Editor(value, _redColor, _greenColor, _blueColor, customInc, xLabel, yLabel, zLabel, onValueChanged);
        }

        public static Vector3 Vector3Editor(Vector3 value, Color color, string xLabel = "X:\t", string yLabel = "Y:\t", string zLabel = "Z:\t", Action onValueChanged = null)
        {
            return Vector3Editor(value, color, color, color, _inc, xLabel, yLabel, zLabel, onValueChanged);
        }

        public static Vector3 Vector3Editor(Vector3 value, Color color, float customInc, string xLabel = "X:\t", string yLabel = "Y:\t", string zLabel = "Z:\t", Action onValueChanged = null)
        {
            return Vector3Editor(value, color, color, color, customInc, xLabel, yLabel, zLabel, onValueChanged);
        }

        public static Vector3 Vector3Editor(Vector3 value, Color xColor, Color yColor, Color zColor, float customInc, string xLabel = "X:\t", string yLabel = "Y:\t", string zLabel = "Z:\t", Action onValueChanged = null)
        {
            string customIncString = customInc.ToString("+0.###;-0.###");
            string minusCustomIncString = (-customInc).ToString("+0.###;-0.###");

            GUILayout.BeginVertical();
            Color c = GUI.color;
            GUI.color = xColor;
            GUILayout.BeginHorizontal();
            GUILayout.Label(xLabel, GUILayout.ExpandWidth(false));

            string oldValue = value.x.ToString("0.00000");
            string newValue = GUILayout.TextField(oldValue, GUILayout.MaxWidth(60));
            if (oldValue != newValue)
            {
                float res;
                if (float.TryParse(newValue, out res))
                {
                    value.x = res;
                    if (onValueChanged != null)
                        onValueChanged();
                }
            }
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(160f));
			if (GUILayout.Button("0", GUILayout.ExpandWidth(false)))
			{
                value.x = 0;
                if (onValueChanged != null)
                    onValueChanged();
            }
            if (GUILayout.RepeatButton(minusCustomIncString) && RepeatControl())
            {
                value -= customInc * Vector3.right;
                if (onValueChanged != null)
                    onValueChanged();
            }
            if (GUILayout.RepeatButton(customIncString) && RepeatControl())
            {
                value += customInc * Vector3.right;
                if (onValueChanged != null)
                    onValueChanged();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUI.color = yColor;
            GUILayout.BeginHorizontal();
            GUILayout.Label(yLabel, GUILayout.ExpandWidth(false));

            oldValue = value.y.ToString("0.00000");
            newValue = GUILayout.TextField(oldValue, GUILayout.MaxWidth(60));
            if (oldValue != newValue)
            {
                float res;
                if (float.TryParse(newValue, out res))
                {
                    value.y = res;
                    if (onValueChanged != null)
                        onValueChanged();
                }
            }
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(160f));
            if (GUILayout.Button("0", GUILayout.ExpandWidth(false)))
            {
                value.y = 0;
                if (onValueChanged != null)
                    onValueChanged();
            }
            if (GUILayout.RepeatButton(minusCustomIncString) && RepeatControl())
            {
                value -= customInc * Vector3.up;
                if (onValueChanged != null)
                    onValueChanged();
            }
            if (GUILayout.RepeatButton(customIncString) && RepeatControl())
            {
                value += customInc * Vector3.up;
                if (onValueChanged != null)
                    onValueChanged();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUI.color = zColor;
            GUILayout.BeginHorizontal();
            GUILayout.Label(zLabel, GUILayout.ExpandWidth(false));

            oldValue = value.z.ToString("0.00000");
            newValue = GUILayout.TextField(oldValue, GUILayout.MaxWidth(60));
            if (oldValue != newValue)
            {
                float res;
                if (float.TryParse(newValue, out res))
                {
                    value.z = res;
                    if (onValueChanged != null)
                        onValueChanged();
                }
            }
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(160f));
            if (GUILayout.Button("0", GUILayout.ExpandWidth(false)))
            {
                value.z = 0;
                if (onValueChanged != null)
                    onValueChanged();
            }
            if (GUILayout.RepeatButton(minusCustomIncString) && RepeatControl())
            {
                value -= customInc * Vector3.forward;
                if (onValueChanged != null)
                    onValueChanged();
            }
            if (GUILayout.RepeatButton(customIncString) && RepeatControl())
            {
                value += customInc * Vector3.forward;
                if (onValueChanged != null)
                    onValueChanged();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
            GUI.color = c;
            GUILayout.EndHorizontal();
            return value;
        }


        public static Quaternion QuaternionEditor(Quaternion value, float customInc, string xLabel = "X (Pitch):\t", string yLabel = "Y (Yaw):\t", string zLabel = "Z (Roll):\t", Action onValueChanged = null)
        {
            return QuaternionEditor(value, _redColor, _greenColor, _blueColor, customInc, xLabel, yLabel, zLabel, onValueChanged);
        }

        public static Quaternion QuaternionEditor(Quaternion value, Color xColor, Color yColor, Color zColor, float customInc, string xLabel = "X (Pitch):\t", string yLabel = "Y (Yaw):\t", string zLabel = "Z (Roll):\t", Action onValueChanged = null)
        {
            string customIncString = customInc.ToString("+0.#####;-0.#####");
            string minusCustomIncString = (-customInc).ToString("+0.#####;-0.#####");

            GUILayout.BeginVertical();
            Color c = GUI.color;
            GUI.color = xColor;
            GUILayout.BeginHorizontal();
            GUILayout.Label(xLabel, GUILayout.ExpandWidth(false));

            string oldValue = value.eulerAngles.x.ToString("0.00000");
            string newValue = GUILayout.TextField(oldValue, GUILayout.MaxWidth(60));
            if (oldValue != newValue)
            {
                float res;
                if (float.TryParse(newValue, out res))
                {
                    value = Quaternion.Euler(res, value.eulerAngles.y, value.eulerAngles.z);
                    if (onValueChanged != null)
                        onValueChanged();
                }
            }
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(160f));
            if (GUILayout.RepeatButton(minusCustomIncString) && RepeatControl())
            {
                value *= Quaternion.AngleAxis(-customInc, Vector3.right);
                if (onValueChanged != null)
                    onValueChanged();
            }
            if (GUILayout.RepeatButton(customIncString) && RepeatControl())
            {
                value *= Quaternion.AngleAxis(customInc, Vector3.right);
                if (onValueChanged != null)
                    onValueChanged();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUI.color = yColor;
            GUILayout.BeginHorizontal();
            GUILayout.Label(yLabel, GUILayout.ExpandWidth(false));

            oldValue = value.eulerAngles.y.ToString("0.00000");
            newValue = GUILayout.TextField(oldValue, GUILayout.MaxWidth(60));
            if (oldValue != newValue)
            {
                float res;
                if (float.TryParse(newValue, out res))
                {
                    value = Quaternion.Euler(value.eulerAngles.x, res, value.eulerAngles.z);
                    if (onValueChanged != null)
                        onValueChanged();
                }
            }
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(160f));
            if (GUILayout.RepeatButton(minusCustomIncString) && RepeatControl())
            {
                value *= Quaternion.AngleAxis(-customInc, Vector3.up);
                if (onValueChanged != null)
                    onValueChanged();
            }
            if (GUILayout.RepeatButton(customIncString) && RepeatControl())
            {
                value *= Quaternion.AngleAxis(customInc, Vector3.up);
                if (onValueChanged != null)
                    onValueChanged();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();

            GUI.color = zColor;
            GUILayout.BeginHorizontal();
            GUILayout.Label(zLabel, GUILayout.ExpandWidth(false));

            oldValue = value.eulerAngles.z.ToString("0.00000");
            newValue = GUILayout.TextField(oldValue, GUILayout.MaxWidth(60));
            if (oldValue != newValue)
            {
                float res;
                if (float.TryParse(newValue, out res))
                {
                    value.z = res;
                    value = Quaternion.Euler(value.eulerAngles.x, value.eulerAngles.y, res);
                    if (onValueChanged != null)
                        onValueChanged();
                }
            }
            GUILayout.FlexibleSpace();

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(160f));
            if (GUILayout.RepeatButton(minusCustomIncString) && RepeatControl())
            {
                value *= Quaternion.AngleAxis(-customInc, Vector3.forward);
                if (onValueChanged != null)
                    onValueChanged();
            }
            if (GUILayout.RepeatButton(customIncString) && RepeatControl())
            {
                value *= Quaternion.AngleAxis(customInc, Vector3.forward);
                if (onValueChanged != null)
                    onValueChanged();
            }
            GUILayout.EndHorizontal();
            GUILayout.EndHorizontal();
            GUI.color = c;
            GUILayout.EndHorizontal();
            return value;
        }

        public static float FloatEditor(float value, float min, float max, string label = "Label\t", string format = "0.000", float inputWidth = 40f, Func<float, float> onReset = null)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, GUILayout.ExpandWidth(false));
            value = GUILayout.HorizontalSlider(value, min, max);
            string oldValue = value.ToString("0.000");
            string newValue = GUILayout.TextField(oldValue, GUILayout.Width(inputWidth));
            if (oldValue != newValue)
            {
                float res;
                if (float.TryParse(newValue, out res))
                    value = res;
            }

            Color c = GUI.color;
            GUI.color = Color.red;
            if (onReset != null && GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
                value = onReset(value);
            GUI.color = c;
            GUILayout.EndHorizontal();
            return value;
        }

        #endregion

        public static void DrawSingleSlider(string sliderName, ref float value, float minValue, float maxValue, float incrementSize = 0.1f)
        {
            if (sliderName != null)
                GUILayout.Label(sliderName, GUILayout.Width(14));

            value = GUILayout.HorizontalSlider(value, minValue, maxValue);

            float.TryParse(GUILayout.TextField(value.ToString(maxValue >= 100 ? "F1" : "F3", CultureInfo.InvariantCulture)), out value);

            if (GUILayout.Button("-")) value -= incrementSize;
            if (GUILayout.Button("+")) value += incrementSize;
        }

        public static void BoolValue(string label, bool value, Action<bool> onChanged = null)
		{
            bool newValue = GUILayout.Toggle(value, label);
            if (onChanged != null && newValue != value)
                onChanged(newValue);
		}

        public static void FloatValue(string label, float value, float left, float right, string valueFormat = "", Action<float> onChanged = null, bool labelOnTop = false)
        {
			GUILayout.BeginVertical();

            if (label != null && labelOnTop)
                GUILayout.Label(label, GUILayout.ExpandWidth(false));

            GUILayout.BeginHorizontal();

            if (label != null && !labelOnTop)
                GUILayout.Label(label, GUILayout.ExpandWidth(false));

            float newValue = GUILayout.HorizontalSlider(value, left, right);
            string valueString = newValue.ToString(valueFormat);
            string newValueString = GUILayout.TextField(valueString, GUILayout.Width(50f));

            if (newValueString != valueString)
            {
                float parseResult;
                if (float.TryParse(newValueString, out parseResult))
                    newValue = parseResult;
            }
            GUILayout.EndHorizontal();

			GUILayout.EndVertical();

            if (onChanged != null && !Mathf.Approximately(value, newValue))
                onChanged(newValue);
        }

        public static void FloatValue(string label, float value, float left, float right, string valueFormat = "", Action<float> onChanged = null)
		{
			GUILayout.BeginHorizontal();
			if (label != null)
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
			float newValue = GUILayout.HorizontalSlider(value, left, right);
			string valueString = newValue.ToString(valueFormat);
			string newValueString = GUILayout.TextField(valueString, GUILayout.Width(50f));

			if (newValueString != valueString)
			{
				float parseResult;
				if (float.TryParse(newValueString, out parseResult))
					newValue = parseResult;
			}
			GUILayout.EndHorizontal();

			if (onChanged != null && !Mathf.Approximately(value, newValue))
				onChanged(newValue);
		}

		public static void FloatValue(string label, float value, string valueFormat = "", Action<float> onChanged = null)
		{
			GUILayout.BeginHorizontal();
			if (label != null)
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
			string valueString = value.ToString(valueFormat);
			string newValueString = GUILayout.TextField(valueString, GUILayout.ExpandWidth(true));

			if (newValueString != valueString)
			{
				float parseResult;
				if (float.TryParse(newValueString, out parseResult))
				{
					if (onChanged != null)
						onChanged(parseResult);
				}
			}
			GUILayout.EndHorizontal();

		}
        public static void IntValue(string label, int value, int left, int right, string valueFormat = "", Action<int> onChanged = null, bool labelOnTop = false)
        {
            GUILayout.BeginVertical();

            if (label != null && labelOnTop)
                GUILayout.Label(label, GUILayout.ExpandWidth(false));

            GUILayout.BeginHorizontal();

            if (label != null && !labelOnTop)
                GUILayout.Label(label, GUILayout.ExpandWidth(false));

            int newValue = Mathf.RoundToInt(GUILayout.HorizontalSlider(value, left, right));
            string valueString = newValue.ToString(valueFormat);
            string newValueString = GUILayout.TextField(valueString, GUILayout.Width(50f));

            if (newValueString != valueString)
            {
                int parseResult;
                if (int.TryParse(newValueString, out parseResult))
                    newValue = parseResult;
            }
            GUILayout.EndHorizontal();

            GUILayout.EndVertical();

            if (onChanged != null && !Mathf.Approximately(value, newValue))
                onChanged(newValue);
        }

        public static void IntValue(string label, int value, int left, int right, string valueFormat = "", Action<int> onChanged = null)
		{
			GUILayout.BeginHorizontal();
			if (label != null)
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
			int newValue = Mathf.RoundToInt(GUILayout.HorizontalSlider(value, left, right));
			string valueString = newValue.ToString(valueFormat);
			string newValueString = GUILayout.TextField(valueString, GUILayout.Width(50f));

			if (newValueString != valueString)
			{
				int parseResult;
				if (int.TryParse(newValueString, out parseResult))
					newValue = parseResult;
			}
			GUILayout.EndHorizontal();

			if (onChanged != null && !Mathf.Approximately(value, newValue))
				onChanged(newValue);
		}

		public static void IntValue(string label, int value, string valueFormat = "", Action<int> onChanged = null)
		{
			GUILayout.BeginHorizontal();
			if (label != null)
				GUILayout.Label(label, GUILayout.ExpandWidth(false));
			string valueString = value.ToString(valueFormat);
			string newValueString = GUILayout.TextField(valueString, GUILayout.ExpandWidth(true));

			if (newValueString != valueString)
			{
				int parseResult;
				if (int.TryParse(newValueString, out parseResult))
				{
					if (onChanged != null)
						onChanged(parseResult);
				}
			}
			GUILayout.EndHorizontal();

		}

		public static void ColorValue(string label,
									  Color color,
									  Color reset,
#if HONEYSELECT
									  UI_ColorInfo.UpdateColor onChanged,
#else
                                      Action<Color> onChanged,
#endif
									  bool simplePicker = false,
									  bool simplePickerShowAlpha = true,
									  bool simplePickerHSV = false)

		{
			ColorValue(label, color, reset, "", onChanged, simplePicker, simplePickerShowAlpha, simplePickerHSV);
		}

		public static void ColorValue(string label,
									  Color color,
									  Color reset,
									  string tooltip,
#if HONEYSELECT
									  UI_ColorInfo.UpdateColor onChanged,
#else
                                      Action<Color> onChanged,
#endif
									  bool simplePicker = false,
									  bool simplePickerShowAlpha = true,
									  bool simplePickerHSV = false, float height = 60f)
		{
			if (simplePicker == false)
			{
				GUILayout.BeginHorizontal();
				if (label != null)
					GUILayout.Label(new GUIContent(label, tooltip), GUILayout.ExpandWidth(false));
				if (GUILayout.Button(GUIContent.none, GUILayout.Height(height)))
				{
#if HONEYSELECT
					if (Studio.Studio.Instance.colorMenu.updateColorFunc == onChanged)
						Studio.Studio.Instance.colorPaletteCtrl.visible = !Studio.Studio.Instance.colorPaletteCtrl.visible;
					else
						Studio.Studio.Instance.colorPaletteCtrl.visible = true;
					if (Studio.Studio.Instance.colorPaletteCtrl.visible)
					{
						Studio.Studio.Instance.colorMenu.updateColorFunc = onChanged;
						Studio.Studio.Instance.colorMenu.SetColor(color, UI_ColorInfo.ControlType.PresetsSample);
					}
#else
	                if (Studio.Studio.Instance.colorPalette.visible)
		                Studio.Studio.Instance.colorPalette.visible = false;
	                else
		                Studio.Studio.Instance.colorPalette.Setup(label, color, onChanged, true);
#endif
				}
				Rect layoutRectangle = GUILayoutUtility.GetLastRect();
				layoutRectangle.xMin += 6;
				layoutRectangle.xMax -= 6;
				layoutRectangle.yMin += 6;
				layoutRectangle.yMax -= 6;
				_simpleTexture.SetPixel(0, 0, color);
				_simpleTexture.Apply(false);
				GUI.DrawTexture(layoutRectangle, _simpleTexture, ScaleMode.StretchToFill, true);
				if (GUILayout.Button("Reset", GUILayout.ExpandWidth(false)))
				{
#if HONEYSELECT
					if (onChanged == Studio.Studio.Instance.colorMenu.updateColorFunc)
						Studio.Studio.Instance.colorMenu.SetColor(reset, UI_ColorInfo.ControlType.PresetsSample);
#endif
					onChanged(reset);
				}
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.BeginVertical();
				GUILayout.BeginHorizontal();
				if (label != null)
					GUILayout.Label(new GUIContent(label, tooltip), GUILayout.ExpandWidth(false));
				GUILayout.FlexibleSpace();
				bool shouldReset = GUILayout.Button("Reset", GUILayout.ExpandWidth(false));
				GUILayout.EndHorizontal();

				Color newColor = color;
				if (simplePickerHSV)
				{
					Color.RGBToHSV(color, out float h, out float s, out float v);
					h *= 360;

					GUILayout.BeginHorizontal();
					GUILayout.Label("H", GUILayout.ExpandWidth(false));
					h = GUILayout.HorizontalSlider(h, 0f, 359.99f);
					if (float.TryParse(GUILayout.TextField(h.ToString("0.0"), GUILayout.Width(50)), out float newValue))
						h = newValue;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("S", GUILayout.ExpandWidth(false));
					s = GUILayout.HorizontalSlider(s, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(s.ToString("0.000"), GUILayout.Width(50)), out newValue))
						s = newValue;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("V", GUILayout.ExpandWidth(false));
					v = GUILayout.HorizontalSlider(v, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(v.ToString("0.000"), GUILayout.Width(50)), out newValue))
						v = newValue;
					GUILayout.EndHorizontal();

					newColor = Color.HSVToRGB(Mathf.Clamp01(h / 360), Mathf.Clamp01(s), v);
					newColor.a = color.a;
				}
				else
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("R", GUILayout.ExpandWidth(false));
					newColor.r = GUILayout.HorizontalSlider(newColor.r, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(newColor.r.ToString("0.000"), GUILayout.Width(50)), out float newValue))
						newColor.r = newValue;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("G", GUILayout.ExpandWidth(false));
					newColor.g = GUILayout.HorizontalSlider(newColor.g, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(newColor.g.ToString("0.000"), GUILayout.Width(50)), out newValue))
						newColor.g = newValue;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("B", GUILayout.ExpandWidth(false));
					newColor.b = GUILayout.HorizontalSlider(newColor.b, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(newColor.b.ToString("0.000"), GUILayout.Width(50)), out newValue))
						newColor.b = newValue;
					GUILayout.EndHorizontal();
				}

				if (simplePickerShowAlpha)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("A", GUILayout.ExpandWidth(false));
					newColor.a = GUILayout.HorizontalSlider(newColor.a, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(newColor.a.ToString("0.000"), GUILayout.Width(50)), out float newValue))
						newColor.a = newValue;
					GUILayout.EndHorizontal();
				}

				GUILayout.Box("", GUILayout.Height(40));
				_simpleTexture.SetPixel(0, 0, color);
				_simpleTexture.Apply(false);
				GUI.DrawTexture(GUILayoutUtility.GetLastRect(), _simpleTexture, ScaleMode.StretchToFill, true);

				if (color != newColor)
					onChanged(newColor);
				GUILayout.EndVertical();
				if (shouldReset)
					onChanged(reset);
			}
		}

		public static void ColorValue(string label,
									  Color color,
#if HONEYSELECT
									  UI_ColorInfo.UpdateColor onChanged,
#else
									  Action<Color> onChanged,
#endif
									  bool simplePicker = false,
									  bool simplePickerShowAlpha = true,
									  bool simplePickerHSV = false, float height = 60f)
		{
			if (simplePicker == false)
			{
				GUILayout.BeginHorizontal();
				if (label != null)
					GUILayout.Label(label, GUILayout.ExpandWidth(false));
				if (GUILayout.Button(GUIContent.none, GUILayout.Height(height)))
				{
#if HONEYSELECT
					if (Studio.Studio.Instance.colorMenu.updateColorFunc == onChanged)
						Studio.Studio.Instance.colorPaletteCtrl.visible = !Studio.Studio.Instance.colorPaletteCtrl.visible;
					else
						Studio.Studio.Instance.colorPaletteCtrl.visible = true;
					if (Studio.Studio.Instance.colorPaletteCtrl.visible)
					{
						Studio.Studio.Instance.colorMenu.updateColorFunc = onChanged;
						Studio.Studio.Instance.colorMenu.SetColor(color, UI_ColorInfo.ControlType.PresetsSample);
					}
#else
	                if (Studio.Studio.Instance.colorPalette.visible)
		                Studio.Studio.Instance.colorPalette.visible = false;
	                else
		                Studio.Studio.Instance.colorPalette.Setup(label, color, onChanged, true);
#endif
				}
				Rect layoutRectangle = GUILayoutUtility.GetLastRect();
				layoutRectangle.xMin += 6;
				layoutRectangle.xMax -= 6;
				layoutRectangle.yMin += 6;
				layoutRectangle.yMax -= 6;
				_simpleTexture.SetPixel(0, 0, color);
				_simpleTexture.Apply(false);
				GUI.DrawTexture(layoutRectangle, _simpleTexture, ScaleMode.StretchToFill, true);
				GUILayout.EndHorizontal();
			}
			else
			{
				GUILayout.BeginVertical();
				if (label != null)
					GUILayout.Label(label, GUILayout.ExpandWidth(false));

				Color newColor = color;
				if (simplePickerHSV)
				{
					Color.RGBToHSV(newColor, out float h, out float s, out float v);
					h *= 360;

					GUILayout.BeginHorizontal();
					GUILayout.Label("H", GUILayout.ExpandWidth(false));
					h = GUILayout.HorizontalSlider(h, 0f, 359.99f);
					if (float.TryParse(GUILayout.TextField(h.ToString("0.0"), GUILayout.Width(50)), out float newValue))
						h = newValue;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("S", GUILayout.ExpandWidth(false));
					s = GUILayout.HorizontalSlider(s, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(s.ToString("0.000"), GUILayout.Width(50)), out newValue))
						s = newValue;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("V", GUILayout.ExpandWidth(false));
					v = GUILayout.HorizontalSlider(v, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(v.ToString("0.000"), GUILayout.Width(50)), out newValue))
						v = newValue;
					GUILayout.EndHorizontal();

					newColor = Color.HSVToRGB(Mathf.Clamp01(h / 360), Mathf.Clamp01(s), v);
					newColor.a = color.a;
				}
				else
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("R", GUILayout.ExpandWidth(false));
					newColor.r = GUILayout.HorizontalSlider(newColor.r, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(newColor.r.ToString("0.000"), GUILayout.Width(50)), out float newValue))
						newColor.r = newValue;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("G", GUILayout.ExpandWidth(false));
					newColor.g = GUILayout.HorizontalSlider(newColor.g, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(newColor.g.ToString("0.000"), GUILayout.Width(50)), out newValue))
						newColor.g = newValue;
					GUILayout.EndHorizontal();

					GUILayout.BeginHorizontal();
					GUILayout.Label("B", GUILayout.ExpandWidth(false));
					newColor.b = GUILayout.HorizontalSlider(newColor.b, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(newColor.b.ToString("0.000"), GUILayout.Width(50)), out newValue))
						newColor.b = newValue;
					GUILayout.EndHorizontal();
				}

				if (simplePickerShowAlpha)
				{
					GUILayout.BeginHorizontal();
					GUILayout.Label("A", GUILayout.ExpandWidth(false));
					newColor.a = GUILayout.HorizontalSlider(newColor.a, 0f, 1f);
					if (float.TryParse(GUILayout.TextField(newColor.a.ToString("0.000"), GUILayout.Width(50)), out float newValue))
						newColor.a = newValue;
					GUILayout.EndHorizontal();
				}

				GUILayout.Box("", GUILayout.Height(40));
				_simpleTexture.SetPixel(0, 0, color);
				_simpleTexture.Apply(false);
				GUI.DrawTexture(GUILayoutUtility.GetLastRect(), _simpleTexture, ScaleMode.StretchToFill, true);

				if (color != newColor)
					onChanged(newColor);
				GUILayout.EndVertical();
			}
		}

		public static void DrawColorBox(Color color, float height = 40)
		{
            GUILayout.Box("", GUILayout.Height(height));
            _simpleTexture.SetPixel(0, 0, color);
            _simpleTexture.Apply(false);
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), _simpleTexture, ScaleMode.StretchToFill, true);
        }

        public static void DrawColorButton(string text, Color color, float height,  Action onClick)
        {
			var guiColor = GUI.color;
			GUILayout.Label(text);
			GUI.color = Color.white;
            if (GUILayout.Button("", GUILayout.Height(height)))
            {
                onClick();
            }
            _simpleTexture.SetPixel(0, 0, color);
            _simpleTexture.Apply(false);
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), _simpleTexture, ScaleMode.StretchToFill, true);
            GUI.color = guiColor;
        }

        private static SortedList<int, string> _layerNames;
		public static void LayerMaskValue(string label, int value, int columns = 2, Action<int> onChanged = null)
		{
			if (_layerNames == null)
			{
				_layerNames = new SortedList<int, string>(32);
				for (int i = 0; i < 32; i++)
				{
					string name = LayerMask.LayerToName(i);
					if (string.IsNullOrEmpty(name) == false)
						_layerNames.Add(i, name);
				}
			}
			LayerMaskValue(label, value, _layerNames, columns, onChanged);
		}

		public static void LayerMaskValue(string label, int value, SortedList<int, string> layerNames, int columns = 2, Action<int> onChanged = null)
		{
			GUILayout.BeginVertical();
			if (label != null)
				GUILayout.Label(label);
			int newValue = value;
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();
			int columnSize = Mathf.CeilToInt((float)layerNames.Count / columns);
			int shown = 0;
			foreach (KeyValuePair<int, string> kvp in layerNames)
			{
				if (shown != 0 && shown % columnSize == 0)
				{
					GUILayout.EndVertical();
					GUILayout.BeginVertical();
				}
				if (GUILayout.Toggle((newValue & (1 << kvp.Key)) != 0, $"{kvp.Key}: {kvp.Value}"))
					newValue |= 1 << kvp.Key;
				else
					newValue &= ~(1 << kvp.Key);
				++shown;
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();

			if (value != newValue && onChanged != null)
				onChanged(newValue);
		}
	}
}