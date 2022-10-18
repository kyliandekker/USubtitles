using UnityEditor;
using UnityEngine;

namespace USubtitles.Editor
{
	[CustomEditor(typeof(CoopScoopAudioClip))]
	public class CoopScoopAudioClipEditor : UnityEditor.Editor
	{
		void OnEnable()
		{
			WaveEditor[] windows = Resources.FindObjectsOfTypeAll<WaveEditor>();
			WaveEditor window = windows.Length == 0 ? EditorWindow.GetWindow<WaveEditor>() : windows[0];
			window.LoadFromFile((CoopScoopAudioClip)target);
		}

		[MenuItem("Assets/Create CoopScoopAudioClip", priority = 1)]
		private static void CreateCoopScoopAudioClipFromAudioClip()
		{
			CoopScoopAudioClip coopScoopAudioClip = new CoopScoopAudioClip();
			coopScoopAudioClip.Clip = Selection.activeObject as AudioClip;
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(AssetDatabase.GetAssetPath(Selection.activeObject) + ".asset");

			AssetDatabase.CreateAsset(coopScoopAudioClip, assetPathAndName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
			Selection.activeObject = coopScoopAudioClip;
		}

		[MenuItem("Assets/Create CoopScoopAudioClip", true)]
		private static bool CreateCoopScoopAudioClipFromAudioClipValidation()
		{
			return Selection.activeObject is AudioClip;
		}
	}
}