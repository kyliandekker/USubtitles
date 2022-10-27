using UnityEditor;
using UnityEngine;

namespace UAudio.USubtitles.Editor
{
	public class Utils : MonoBehaviour
	{
		public static Texture2D LoadImage(string filename)
		{
			Texture2D texture = EditorGUIUtility.Load($"Assets/USubtitles/Images/{filename}.png") as Texture2D;
			return texture;
		}
	}
}