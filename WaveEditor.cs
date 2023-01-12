using System;
using UnityEngine;

namespace USubtitles.Editor
{
	public static class SubtitleEditorVariables
	{
		public static float Version = 1.2f;

		public static Color Color_ContainerBackground = new Color32(72, 72, 72, 255);
		public static Color Color_ToolbarBackground = new Color32(45, 55, 51, 255);
		public static Color Color_Marker = new Color32(76, 153, 127, 255);
		public static Color Color_MarkerSelected = new Color32(63, 127, 105, 255);
		public static Color Color_MarkerHover = new Color32(113, 155, 140, 255);
		public static Color Color_OutlineColor = new Color32(83, 83, 83, 255);
		public static Color Color_TimelineBackground = new Color32(32, 32, 32, 255);
		public static Color Color_TimelineBackline = new Color32(45, 45, 45, 255);
		public static Color Color_Timeline = new Color32(209, 148, 66, 255);
		public static Color Color_WaveformColor = new Color32(144, 209, 255, 255);
		public static Color Color_MarkerColorClear = new Color32(165, 67, 67, 255);

		public static float Float_Saturation = 1.5f;
	}

	public enum TimelineInteraction
	{
		TimelineInteraction_None,
		TimelineInteraction_Marker,
		TimelineInteraction_Time
	}
}