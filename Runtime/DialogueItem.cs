using System.Collections.Generic;
using UnityEngine;

namespace UAudio.USubtitles
{
	[System.Serializable]
	public class Line
	{
		public SystemLanguage Language;
		public string Text;
	}

	[System.Serializable]
	public class DialogueItem
	{
		public uint SamplePosition = 0;
		public Color Color = Color.white;
		public List<Line> Lines = new List<Line>();
		public bool UseColor = false;
		public bool Bold = false;
		public bool Italics = false;
		public bool Clear = false;
	}
}