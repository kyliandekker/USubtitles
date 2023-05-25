using UnityEditor;
using UnityEngine;

namespace UAudio.USubtitles.Editor
{
	public static class USubtitleEditorVariables
	{
		public const string Version = "1.4";
		public static USubtitlePreferences Preferences = new USubtitlePreferences();
	}

	public class USubtitlePreferences
	{
		public Color Color_Marker = new Color32(76, 153, 127, 255);
		public Color Color_MarkerSelected = new Color32(63, 127, 105, 255);
		public Color Color_MarkerHover = new Color32(113, 155, 140, 255);
		public Color Color_Waveform = new Color32(144, 209, 255, 255);
		public Color Color_MarkerClear = new Color32(165, 67, 67, 255);
	}

	public class USubtitleEditorPreferences
	{
		private static bool prefsLoaded = false;

		public static void Load()
		{
			USubtitleEditorVariables.Preferences = new USubtitlePreferences();

			SetColor("Color_Marker", ref USubtitleEditorVariables.Preferences.Color_Marker);
			SetColor("Color_MarkerHover", ref USubtitleEditorVariables.Preferences.Color_MarkerHover);
			SetColor("Color_MarkerSelected", ref USubtitleEditorVariables.Preferences.Color_MarkerSelected);
			SetColor("Color_Waveform", ref USubtitleEditorVariables.Preferences.Color_Waveform);
			SetColor("Color_MarkerClear", ref USubtitleEditorVariables.Preferences.Color_MarkerClear);

			prefsLoaded = true;
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
			EditorGUILayout.LabelField("Version: " + USubtitleEditorVariables.Version);
			EditorGUILayout.Space(space);
			USubtitleEditorVariables.Preferences.Color_Marker = EditorGUILayout.ColorField(new GUIContent("Marker"), USubtitleEditorVariables.Preferences.Color_Marker);
			USubtitleEditorVariables.Preferences.Color_MarkerSelected = EditorGUILayout.ColorField(new GUIContent("Marker Selected"), USubtitleEditorVariables.Preferences.Color_MarkerSelected);
			USubtitleEditorVariables.Preferences.Color_MarkerHover = EditorGUILayout.ColorField(new GUIContent("Marker Hover"), USubtitleEditorVariables.Preferences.Color_MarkerHover);
			USubtitleEditorVariables.Preferences.Color_MarkerClear = EditorGUILayout.ColorField(new GUIContent("Marker Clear"), USubtitleEditorVariables.Preferences.Color_MarkerClear);
			EditorGUILayout.Space(space);
			USubtitleEditorVariables.Preferences.Color_Waveform = EditorGUILayout.ColorField(new GUIContent("Waveform"), USubtitleEditorVariables.Preferences.Color_Waveform);

			if (GUI.changed)
				Save();
		}

		private static void Save()
		{
			EditorPrefs.SetString("Color_Marker", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_Marker));
			EditorPrefs.SetString("Color_MarkerSelected", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_MarkerSelected));
			EditorPrefs.SetString("Color_MarkerHover", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_MarkerHover));
			EditorPrefs.SetString("Color_MarkerClear", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_MarkerClear));
			EditorPrefs.SetString("Color_Waveform", "#" + ColorUtility.ToHtmlStringRGBA(USubtitleEditorVariables.Preferences.Color_Waveform));
		}
	}
}