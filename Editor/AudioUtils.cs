using UnityEngine;

namespace UAudio.USubtitles.Editor
{
	public class AudioUtils
	{
		public static float[] GetWaveform(AudioClip audio, int size, float sat)
		{
			float[] samples = new float[audio.channels * audio.samples];
			float[] waveform = new float[size];
			audio.GetData(samples, 0);
			int packSize = audio.samples * audio.channels / size;
			float max = 0f;
			int c = 0;
			int s = 0;
			for (int i = 0; i < audio.channels * audio.samples; i++)
			{
				waveform[c] += Mathf.Abs(samples[i]);
				s++;
				if (s > packSize)
				{
					if (max < waveform[c])
						max = waveform[c];
					c++;
					s = 0;
				}
			}
			for (int i = 0; i < size; i++)
			{
				waveform[i] /= max * sat;
				if (waveform[i] > 1f)
					waveform[i] = 1f;
			}

			return waveform;
		}

		public static Texture2D PaintWaveformSpectrum(float[] waveform, int height, Color c)
		{
			Texture2D tex = new Texture2D(waveform.Length, height, TextureFormat.RGBA32, false);

			// Make it transparent.
			for (int x = 0; x < waveform.Length; x++)
				for (int y = 0; y <= height; y++)
					tex.SetPixel(x, y, Color.clear);

			for (int x = 0; x < waveform.Length; x++)
				for (int y = 0; y <= waveform[x] * (float)height / 2f; y++)
				{
					tex.SetPixel(x, (height / 2) + y, c);
					tex.SetPixel(x, (height / 2) - y, c);
				}

			tex.Apply();

			return tex;
		}
	}
}