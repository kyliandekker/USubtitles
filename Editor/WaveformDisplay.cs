using UnityEditor;
using UnityEngine;

namespace USubtitles.Editor
{
	public class WaveformDisplay
	{
		private AudioClip _currentClip = null;
		public AudioClip Clip { get; private set; } = null;

		private Texture2D texture = null;

		/// <summary>
		/// Sets the audio clip (used for the waveform).
		/// </summary>
		/// <param name="clip">The audio clip.</param>
		public void SetClip(AudioClip clip) => Clip = clip;

		/// <summary>
		/// Draws the waveform.
		/// </summary>
		/// <param name="rect">The total space reserved for the waveform.</param>
		public void Draw(Rect rect)
		{
			if (_currentClip != Clip)
			{
				_currentClip = Clip;
				texture = AudioUtils.PaintWaveformSpectrum(AudioUtils.GetWaveform(Clip, (int)rect.width, SubtitleEditorVariables.Preferences.Float_Saturation), (int)rect.height, SubtitleEditorVariables.Preferences.Color_Waveform);
			}

			if (texture)
			{
				Color guiColor = GUI.color;
				GUI.color = Color.clear;
				EditorGUI.DrawTextureTransparent(rect, texture);
				GUI.color = guiColor;
			}
		}
	}
}