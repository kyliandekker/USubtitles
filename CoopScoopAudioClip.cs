using System.Collections.Generic;
using UnityEngine;

namespace USubtitles
{
	[CreateAssetMenu(fileName = "CoopScoopAudioClip", menuName = "CoopScoop/CoopScoopAudioClip", order = 1)]
	public class CoopScoopAudioClip : ScriptableObject
	{
		public AudioClip Clip = null;
		public List<DialogueItem> Dialogue = new List<DialogueItem>();
	}
}