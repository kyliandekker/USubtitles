using System;
using UnityEngine;

namespace USubtitles.Editor
{
	public static class SubtitleEditorVariables
	{
		public static float Version = 1.2f;

		public static Preferences Preferences = new Preferences();
	}

	public enum TimelineInteraction
	{
		TimelineInteraction_None,
		TimelineInteraction_Marker,
		TimelineInteraction_Time
	}
}