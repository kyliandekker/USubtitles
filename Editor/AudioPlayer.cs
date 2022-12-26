using UnityEditor;
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

		private AudioState _state = AudioState.AudioState_Stopped;
		public AudioState _prev = AudioState.AudioState_Stopped;

		public AudioState Prev => _prev;
		public AudioState State => _state;

		public float WavePosition = 0.0f;

		public void SetClip(AudioClip clip)
		{
			SetState(AudioState.AudioState_Stopped);
			_clip = clip;
		}

		public void Update()
		{
			bool isClipPlaying = AudioUtility.IsClipPlaying();
			if (_state == AudioState.AudioState_Playing && !isClipPlaying)
				SetState(AudioState.AudioState_Stopped);
		}

		public void SetState(AudioState state, uint samplePosition = 0)
		{
			_prev = State;
			_state = state;
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

		internal void SetPosition(uint samplePosition) => AudioUtility.SetClipSamplePosition(_clip, (int)samplePosition);
	}
}