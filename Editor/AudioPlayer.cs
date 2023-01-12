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
		public AudioState _prev = AudioState.AudioState_Stopped;

		public AudioState Prev => _prev;
		public AudioState State { get; private set; } = AudioState.AudioState_Stopped;

		public float WavePosition = 0.0f;

		public void SetClip(AudioClip clip)
		{
			SetState(AudioState.AudioState_Stopped);
			_clip = clip;
		}

		public void Update()
		{
			bool isClipPlaying = AudioUtility.IsClipPlaying();
			if (State == AudioState.AudioState_Playing && !isClipPlaying)
				SetState(AudioState.AudioState_Stopped);
		}

		public void SetState(AudioState state, uint samplePosition = 0)
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

		public void SetPosition(uint samplePosition) => AudioUtility.SetClipSamplePosition(_clip, (int)samplePosition);

		public AudioState GetState()
		{
			return State;
		}
	}
}