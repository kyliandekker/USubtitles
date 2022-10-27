using UnityEditor;
using UnityEngine;

namespace UAudio.USubtitles.Editor
{
	public class EditorPreferences
	{
		public static Color Default_ContainerBackground = new Color32(72, 72, 72, 255);
		public static Color Default_Marker = new Color32(76, 153, 127, 255);
		public static Color Default_MarkerSelected = new Color32(63, 127, 105, 255);
		public static Color Default_OutlineColor = new Color32(83, 83, 83, 255);
		public static Color Default_TimelineBackground = new Color32(32, 32, 32, 255);
		public static Color Default_TimelineBackline = new Color32(45, 45, 45, 255);
		public static Color Default_Timeline = new Color32(209, 148, 66, 255);
		public static Color Default_WaveformColor = new Color32(144, 209, 255, 255);
		public static Color Default_MarkerColorClear = new Color32(165, 67, 67, 255);

		public static float Default_Float_Saturation = 1.5f;

		private static bool prefsLoaded = false;

		private static void Load()
		{
			SetColor("Color_ContainerBackground", ref SubtitleEditorVariables.Color_ContainerBackground, Default_ContainerBackground);
			SetColor("Color_Marker", ref SubtitleEditorVariables.Color_Marker, Default_Marker);
			SetColor("Color_MarkerSelected", ref SubtitleEditorVariables.Color_MarkerSelected, Default_MarkerSelected);
			SetColor("Color_Outline", ref SubtitleEditorVariables.Color_OutlineColor, Default_OutlineColor);
			SetColor("Color_TimelineBackground", ref SubtitleEditorVariables.Color_TimelineBackground, Default_TimelineBackground);
			SetColor("Color_TimelineBackline", ref SubtitleEditorVariables.Color_TimelineBackline, Default_TimelineBackline);
			SetColor("Color_Timeline", ref SubtitleEditorVariables.Color_Timeline, Default_Timeline);
			SetColor("Color_WaveformColor", ref SubtitleEditorVariables.Color_WaveformColor, Default_WaveformColor);
			SetColor("Color_MarkerColorClear", ref SubtitleEditorVariables.Color_MarkerColorClear, Default_MarkerColorClear);
			SetFloat("Float_Saturation", ref SubtitleEditorVariables.Float_Saturation, Default_Float_Saturation);

			prefsLoaded = true;
		}

		private static void SetColor(string key, ref Color color, Color defaultValue)
		{
			if (EditorPrefs.HasKey(key))
			{
				if (!ColorUtility.TryParseHtmlString(EditorPrefs.GetString(key), out color))
					color = defaultValue;
			}
			else
				color = defaultValue;

		}

		private static void SetFloat(string key, ref float refFloat, float defaultValue)
		{
			if (EditorPrefs.HasKey(key))
				refFloat = EditorPrefs.GetFloat(key);
			else
				refFloat = defaultValue;
		}

		[PreferenceItem("Subtitle Editor")]
		private static void CustomPreferencesGUI()
		{
			if (!prefsLoaded)
				Load();

			EditorGUILayout.LabelField("Version: " + SubtitleEditorVariables.Version.ToString());
			SubtitleEditorVariables.Color_ContainerBackground = EditorGUILayout.ColorField(new GUIContent("Container Background"), SubtitleEditorVariables.Color_ContainerBackground);
			SubtitleEditorVariables.Color_Marker = EditorGUILayout.ColorField(new GUIContent("Marker"), SubtitleEditorVariables.Color_Marker);
			SubtitleEditorVariables.Color_MarkerSelected = EditorGUILayout.ColorField(new GUIContent("Marker Selected"), SubtitleEditorVariables.Color_MarkerSelected);
			SubtitleEditorVariables.Color_MarkerColorClear = EditorGUILayout.ColorField(new GUIContent("Marker Clear Color"), SubtitleEditorVariables.Color_MarkerColorClear);
			SubtitleEditorVariables.Color_OutlineColor = EditorGUILayout.ColorField(new GUIContent("Outline"), SubtitleEditorVariables.Color_OutlineColor);
			SubtitleEditorVariables.Color_TimelineBackground = EditorGUILayout.ColorField(new GUIContent("Timeline Background"), SubtitleEditorVariables.Color_TimelineBackground);
			SubtitleEditorVariables.Color_TimelineBackline = EditorGUILayout.ColorField(new GUIContent("Timeline Backline"), SubtitleEditorVariables.Color_TimelineBackline);
			SubtitleEditorVariables.Color_Timeline = EditorGUILayout.ColorField(new GUIContent("Timeline"), SubtitleEditorVariables.Color_Timeline);
			SubtitleEditorVariables.Color_WaveformColor = EditorGUILayout.ColorField(new GUIContent("Waveform"), SubtitleEditorVariables.Color_WaveformColor);
			SubtitleEditorVariables.Float_Saturation = EditorGUILayout.FloatField(new GUIContent("Saturation"), SubtitleEditorVariables.Float_Saturation);

			if (GUI.changed)
			{
				Save();

				WaveEditor[] windows = Resources.FindObjectsOfTypeAll<WaveEditor>();
				WaveEditor window = windows.Length == 0 ? EditorWindow.GetWindow<WaveEditor>() : windows[0];
				window.Repaint();
			}
		}

		private static void Save()
		{
			EditorPrefs.SetString("Color_ContainerBackground", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Color_ContainerBackground));
			EditorPrefs.SetString("Color_Marker", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Color_Marker));
			EditorPrefs.SetString("Color_MarkerSelected", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Color_MarkerSelected));
			EditorPrefs.SetString("Color_Outline", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Color_OutlineColor));
			EditorPrefs.SetString("Color_TimelineBackground", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Color_TimelineBackground));
			EditorPrefs.SetString("Color_TimelineBackline", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Color_TimelineBackline));
			EditorPrefs.SetString("Color_Timeline", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Color_Timeline));
			EditorPrefs.SetString("Color_WaveformColor", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Color_WaveformColor));
			EditorPrefs.SetString("Color_MarkerColorClear", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Color_MarkerColorClear));
			EditorPrefs.SetFloat("Float_Saturation", SubtitleEditorVariables.Float_Saturation);
		}
	}
}