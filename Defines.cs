namespace USubtitles.Editor
{
	public static class SubtitleEditorVariables
	{
		public const float Version = 1.3f;
		public static Preferences Preferences = new Preferences();
	}

	public enum InteractionType
	{
		TimelineInteraction_None,
		TimelineInteraction_Marker,
		TimelineInteraction_Time
	}
}