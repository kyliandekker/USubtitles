using UnityEngine;

namespace USubtitles.Editor
{
	public enum AudioState
	{
		AudioState_Playing,
		AudioState_Paused,
		AudioState_Stopped,
	}

	public class AudioPlayer
	{
		private AudioClip _clip = null;
		public AudioClip Clip => _clip;

		public AudioState _prev = AudioState.AudioState_Stopped;

		public AudioState Prev => _prev;
		public AudioState State { get; private set; } = AudioState.AudioState_Stopped;

		public float WavePosition = 0.0f;

		/// <summary>
		/// Sets the clip.
		/// </summary>
		/// <param name="clip">The audio clip.</param>
		public void SetClip(AudioClip clip)
		{
			SetState(AudioState.AudioState_Stopped);
			_clip = clip;
		}

		/// <summary>
		/// Updates the state of the player.
		/// </summary>
		public void Update()
		{
			bool isClipPlaying = AudioUtility.IsClipPlaying();
			if (State == AudioState.AudioState_Playing && !isClipPlaying)
				SetState(AudioState.AudioState_Stopped);
		}

		/// <summary>
		/// Sets the current state of the player.
		/// </summary>
		/// <param name="state">The new state the player will have.</param>
		/// <param name="samplePosition">Optional sample position to be set.</param>
		public void SetState(AudioState state, float samplePosition = 0)
		{
			_prev = State;
			State = state;
			switch (State)
			{
				case AudioState.AudioState_Playing:
					if (Prev == AudioState.AudioState_Paused)
						AudioUtility.ResumeClip();
					else
						AudioUtility.PlayClip(_clip, (int)samplePosition, false);
					break;
				case AudioState.AudioState_Paused:
					AudioUtility.PauseClip();
					break;
				case AudioState.AudioState_Stopped:
					AudioUtility.StopAllClips();
					break;
			}
		}

		/// <summary>
		/// Sets the position of the current playback.
		/// </summary>
		/// <param name="samplePosition">The sample position in bytes.</param>
		public void SetPosition(float samplePosition)
		{
			WavePosition = samplePosition;
			AudioUtility.SetClipSamplePosition(_clip, (int)samplePosition);
		}

		/// <summary>
		/// Returns the state of the player (stopped, playing, paused).
		/// </summary>
		/// <returns></returns>
		public AudioState GetState() => State;
	}
}