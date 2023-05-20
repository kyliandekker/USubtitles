using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace USubtitles.Editor
{
	public static class USubtitleEditorVariables
	{
		public const float Version = 1.3f;
		public static USubtitlePreferences Preferences = new USubtitlePreferences();
	}

	public class USubtitlePreferences
	{
		public Color Color_ContainerBackground = new Color32(72, 72, 72, 255);
		public Color Color_ToolbarBackground = new Color32(45, 55, 51, 255);
		public Color Color_Marker = new Color32(76, 153, 127, 255);
		public Color Color_MarkerSelected = new Color32(63, 127, 105, 255);
		public Color Color_MarkerHover = new Color32(113, 155, 140, 255);
		public Color Color_TimelineBackground = new Color32(32, 32, 32, 255);
		public Color Color_TimelineBackline = new Color32(45, 45, 45, 255);
		public Color Color_Waveform = new Color32(144, 209, 255, 255);
		public Color Color_MarkerClear = new Color32(165, 67, 67, 255);
		public float Float_Saturation = 1.5f;
	}

	public class USubtitleEditorPreferences
	{
		private static bool prefsLoaded = false;

		public static void Load()
		{
			USubtitleEditorVariables.Preferences = new USubtitlePreferences();

			SetColor("Color_ContainerBackground", ref USubtitleEditorVariables.Preferences.Color_ContainerBackground);
			SetColor("Color_ToolbarBackground", ref USubtitleEditorVariables.Preferences.Color_ToolbarBackground);
			SetColor("Color_Marker", ref USubtitleEditorVariables.Preferences.Color_Marker);
			SetColor("Color_MarkerHover", ref USubtitleEditorVariables.Preferences.Color_MarkerHover);
			SetColor("Color_MarkerSelected", ref USubtitleEditorVariables.Preferences.Color_MarkerSelected);
			SetColor("Color_TimelineBackground", ref USubtitleEditorVariables.Preferences.Color_TimelineBackground);
			SetColor("Color_TimelineBackline", ref USubtitleEditorVariables.Preferences.Color_TimelineBackline);
			SetColor("Color_Waveform", ref USubtitleEditorVariables.Preferences.Color_Waveform);
			SetColor("Color_MarkerClear", ref USubtitleEditorVariables.Preferences.Color_MarkerClear);
			SetFloat("Float_Saturation", ref USubtitleEditorVariables.Preferences.Float_Saturation);

			prefsLoaded = true;
		}

		private static void SetKeyCode(string key, ref KeyCode keyCode)
		{
			if (EditorPrefs.HasKey(key))
				keyCode = (KeyCode) EditorPrefs.GetInt(key);
		}

		private static void SetColor(string key, ref Color color)
		{
			if (EditorPrefs.HasKey(key))
				_ = ColorUtility.TryParseHtmlString(EditorPrefs.GetString(key), out color);
		}

		private static void SetFloat(string key, ref float refFloat)
		{
			if (EditorPrefs.HasKey(key))
				refFloat = EditorPrefs.GetFloat(key);
		}

		[PreferenceItem("Subtitle Editor")]
		private static void CustomPreferencesGUI()
		{
			if (!prefsLoaded)
				Load();

			const int space = 25;
			EditorGUILayout.LabelField("Version: " + USubtitleEditorVariables.Version.ToString());
			EditorGUILayout.Space(space);
			USubtitleEditorVariables.Preferences.Color_ContainerBackground = EditorGUILayout.ColorField(new GUIContent("Container Background"), USubtitleEditorVariables.Preferences.Color_ContainerBackground);
			USubtitleEditorVariables.Preferences.Color_ToolbarBackground = EditorGUILayout.ColorField(new GUIContent("Toolbar Background"), USubtitleEditorVariables.Preferences.Color_ToolbarBackground);
			EditorGUILayout.Space(space);
			USubtitleEditorVariables.Preferences.Color_Marker = EditorGUILayout.ColorField(new GUIContent("Marker"), USubtitleEditorVariables.Preferences.Color_Marker);
			USubtitleEditorVariables.Preferences.Color_MarkerSelected = EditorGUILayout.ColorField(new GUIContent("Marker Selected"), USubtitleEditorVariables.Preferences.Color_MarkerSelected);
			USubtitleEditorVariables.Preferences.Color_MarkerHover = EditorGUILayout.ColorField(new GUIContent("Marker Hover"), USubtitleEditorVariables.Preferences.Color_MarkerHover);
			USubtitleEditorVariables.Preferences.Color_MarkerClear = EditorGUILayout.ColorField(new GUIContent("Marker Clear"), USubtitleEditorVariables.Preferences.Color_MarkerClear);
			EditorGUILayout.Space(space);
			USubtitleEditorVariables.Preferences.Color_TimelineBackground = EditorGUILayout.ColorField(new GUIContent("Timeline Background"), USubtitleEditorVariables.Preferences.Color_TimelineBackground);
			USubtitleEditorVariables.Preferences.Color_TimelineBackline = EditorGUILayout.ColorField(new GUIContent("Timeline Backline"), USubtitleEditorVariables.Preferences.Color_TimelineBackline);
			EditorGUILayout.Space(space);
			USubtitleEditorVariables.Preferences.Color_Waveform = EditorGUILayout.ColorField(new GUIContent("Waveform"), USubtitleEditorVariables.Preferences.Color_Waveform);
			EditorGUILayout.Space(space);
			USubtitleEditorVariables.Preferences.Float_Saturation = EditorGUILayout.FloatField(new GUIContent("Saturation"), USubtitleEditorVariables.Preferences.Float_Saturation);

			if (GUI.changed)
				Save();
		}

		private static void Save()
		{
			EditorPrefs.SetString("Color_ContainerBackground", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_ContainerBackground));
			EditorPrefs.SetString("Color_ToolbarBackground", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_ToolbarBackground));
			EditorPrefs.SetString("Color_Marker", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_Marker));
			EditorPrefs.SetString("Color_MarkerSelected", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_MarkerSelected));
			EditorPrefs.SetString("Color_MarkerHover", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_MarkerHover));
			EditorPrefs.SetString("Color_MarkerClear", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_MarkerClear));
			EditorPrefs.SetString("Color_TimelineBackground", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_TimelineBackground));
			EditorPrefs.SetString("Color_TimelineBackline", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_TimelineBackline));
			EditorPrefs.SetString("Color_Waveform", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_Waveform));
			EditorPrefs.SetFloat("Float_Saturation", USubtitleEditorVariables.Preferences.Float_Saturation);
		}
	}
}