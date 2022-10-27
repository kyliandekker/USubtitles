using System.Collections.Generic;
using UnityEngine;

namespace USubtitles
{
	[CreateAssetMenu(fileName = "UAudioClip", menuName = "/UAudioClip", order = 1)]
	public class UAudioClip : ScriptableObject
	{
		public AudioClip Clip = null;
		public List<DialogueItem> Dialogue = new List<DialogueItem>();
	}
}