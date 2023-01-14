﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace USubtitles.Editor
{
	public class Preferences
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
		public KeyCode KeyCode_Marker = KeyCode.M;
		public float Float_Saturation = 1.5f;
	}

	public class EditorPreferences
	{
		private static bool prefsLoaded = false;

		public static void Load()
		{
			SubtitleEditorVariables.Preferences = new Preferences();

			SetColor("Color_ContainerBackground", ref SubtitleEditorVariables.Preferences.Color_ContainerBackground);
			SetColor("Color_ToolbarBackground", ref SubtitleEditorVariables.Preferences.Color_ToolbarBackground);
			SetColor("Color_Marker", ref SubtitleEditorVariables.Preferences.Color_Marker);
			SetColor("Color_MarkerHover", ref SubtitleEditorVariables.Preferences.Color_MarkerHover);
			SetColor("Color_MarkerSelected", ref SubtitleEditorVariables.Preferences.Color_MarkerSelected);
			SetColor("Color_TimelineBackground", ref SubtitleEditorVariables.Preferences.Color_TimelineBackground);
			SetColor("Color_TimelineBackline", ref SubtitleEditorVariables.Preferences.Color_TimelineBackline);
			SetColor("Color_Waveform", ref SubtitleEditorVariables.Preferences.Color_Waveform);
			SetColor("Color_MarkerClear", ref SubtitleEditorVariables.Preferences.Color_MarkerClear);
			SetKeyCode("KeyCode_Marker", ref SubtitleEditorVariables.Preferences.KeyCode_Marker);
			SetFloat("Float_Saturation", ref SubtitleEditorVariables.Preferences.Float_Saturation);

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
				ColorUtility.TryParseHtmlString(EditorPrefs.GetString(key), out color);
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
			EditorGUILayout.LabelField("Version: " + SubtitleEditorVariables.Version.ToString());
			EditorGUILayout.Space(space);
			SubtitleEditorVariables.Preferences.Color_ContainerBackground = EditorGUILayout.ColorField(new GUIContent("Container Background"), SubtitleEditorVariables.Preferences.Color_ContainerBackground);
			SubtitleEditorVariables.Preferences.Color_ToolbarBackground = EditorGUILayout.ColorField(new GUIContent("Toolbar Background"), SubtitleEditorVariables.Preferences.Color_ToolbarBackground);
			EditorGUILayout.Space(space);
			SubtitleEditorVariables.Preferences.Color_Marker = EditorGUILayout.ColorField(new GUIContent("Marker"), SubtitleEditorVariables.Preferences.Color_Marker);
			SubtitleEditorVariables.Preferences.Color_MarkerSelected = EditorGUILayout.ColorField(new GUIContent("Marker Selected"), SubtitleEditorVariables.Preferences.Color_MarkerSelected);
			SubtitleEditorVariables.Preferences.Color_MarkerHover = EditorGUILayout.ColorField(new GUIContent("Marker Hover"), SubtitleEditorVariables.Preferences.Color_MarkerHover);
			SubtitleEditorVariables.Preferences.Color_MarkerClear = EditorGUILayout.ColorField(new GUIContent("Marker Clear"), SubtitleEditorVariables.Preferences.Color_MarkerClear);
			EditorGUILayout.Space(space);
			SubtitleEditorVariables.Preferences.Color_TimelineBackground = EditorGUILayout.ColorField(new GUIContent("Timeline Background"), SubtitleEditorVariables.Preferences.Color_TimelineBackground);
			SubtitleEditorVariables.Preferences.Color_TimelineBackline = EditorGUILayout.ColorField(new GUIContent("Timeline Backline"), SubtitleEditorVariables.Preferences.Color_TimelineBackline);
			EditorGUILayout.Space(space);
			SubtitleEditorVariables.Preferences.Color_Waveform = EditorGUILayout.ColorField(new GUIContent("Waveform"), SubtitleEditorVariables.Preferences.Color_Waveform);
			EditorGUILayout.Space(space);
			SubtitleEditorVariables.Preferences.KeyCode_Marker = (KeyCode)EditorGUILayout.EnumPopup("Add Marker Key", SubtitleEditorVariables.Preferences.KeyCode_Marker);
			EditorGUILayout.Space(space);
			SubtitleEditorVariables.Preferences.Float_Saturation = EditorGUILayout.FloatField(new GUIContent("Saturation"), SubtitleEditorVariables.Preferences.Float_Saturation);

			if (GUI.changed)
				Save();
		}

		private static void Save()
		{
			EditorPrefs.SetString("Color_ContainerBackground", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Preferences.Color_ContainerBackground));
			EditorPrefs.SetString("Color_ToolbarBackground", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Preferences.Color_ToolbarBackground));
			EditorPrefs.SetString("Color_Marker", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Preferences.Color_Marker));
			EditorPrefs.SetString("Color_MarkerSelected", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Preferences.Color_MarkerSelected));
			EditorPrefs.SetString("Color_MarkerHover", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Preferences.Color_MarkerHover));
			EditorPrefs.SetString("Color_MarkerClear", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Preferences.Color_MarkerClear));
			EditorPrefs.SetString("Color_TimelineBackground", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Preferences.Color_TimelineBackground));
			EditorPrefs.SetString("Color_TimelineBackline", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Preferences.Color_TimelineBackline));
			EditorPrefs.SetString("Color_Waveform", "#" + ColorUtility.ToHtmlStringRGBA(SubtitleEditorVariables.Preferences.Color_Waveform));
			EditorPrefs.SetInt("KeyCode_Marker", (int)SubtitleEditorVariables.Preferences.KeyCode_Marker);
			EditorPrefs.SetFloat("Float_Saturation", SubtitleEditorVariables.Preferences.Float_Saturation);
		}
	}
}