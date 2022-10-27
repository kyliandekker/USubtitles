using UnityEditor;
using UnityEngine;

namespace USubtitles.Editor
{
	[CustomEditor(typeof(UAudioClip), true)]
	public class UAudioClipEditor : UnityEditor.Editor
	{
		void OnEnable()
		{
			WaveEditor[] windows = Resources.FindObjectsOfTypeAll<WaveEditor>();
			WaveEditor window = windows.Length == 0 ? EditorWindow.GetWindow<WaveEditor>() : windows[0];
			window.LoadFromFile((UAudioClip)target);
		}

		[MenuItem("Assets/Create UAudioClip", priority = 1)]
		private static void CreateUAudioClipFromAudioClip()
		{
			for (int i = 0; i < Selection.objects.Length; i++)
			{
				Object obj = Selection.objects[i];

				UAudioClip uAudioClip = new UAudioClip();
				uAudioClip.Clip = obj as AudioClip;
				string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(AssetDatabase.GetAssetPath(obj) + ".asset");

				AssetDatabase.CreateAsset(uAudioClip, assetPathAndName);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				EditorUtility.FocusProjectWindow();
			}
		}

		[MenuItem("Assets/Create UAudioClip", true)]
		private static bool CreateUAudioClipFromAudioClipValidation() => Selection.activeObject is AudioClip;
	}
}