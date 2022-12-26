using UnityEditor;
using UnityEngine;

namespace USubtitles.Editor
{
	public class WaveformDisplay
	{
		private AudioClip _clip = null, _currentClip = null;
		public AudioClip Clip => _clip;

		private Texture2D texture = null;

		private Color _waveformColor = SubtitleEditorVariables.Color_WaveformColor;

		private float _saturation = SubtitleEditorVariables.Float_Saturation;

		public void SetClip(AudioClip clip) => _clip = clip;

		public void Draw(Rect rect)
		{
			if (_currentClip != _clip)
			{
				_currentClip = _clip;
				texture = AudioUtils.PaintWaveformSpectrum(AudioUtils.GetWaveform(_clip, (int)rect.width, _saturation), (int)rect.height, _waveformColor);
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