using UnityEditor;
using UnityEngine;

namespace USubtitles.Editor
{
	public static class SubtitleEditorVariables
	{
		public static float Version = 1.0f;

		public static Color Color_ContainerBackground = new Color32(72, 72, 72, 255);
		public static Color Color_Marker = new Color32(76, 153, 127, 255);
		public static Color Color_MarkerSelected = new Color32(63, 127, 105, 255);
		public static Color Color_OutlineColor = new Color32(83, 83, 83, 255);
		public static Color Color_TimelineBackground = new Color32(32, 32, 32, 255);
		public static Color Color_TimelineBackline = new Color32(45, 45, 45, 255);
		public static Color Color_Timeline = new Color32(209, 148, 66, 255);
		public static Color Color_WaveformColor = new Color32(144, 209, 255, 255);

		public static float Float_Saturation = 1.5f;
	}

	public enum TimelineInteraction
	{
		TimelineInteraction_None,
		TimelineInteraction_Marker,
		TimelineInteraction_Time
	}

	public class DialogueItemWindow : EditorWindow
	{
		private CoopScoopAudioClip _clip = null;
		private int _index = -1;

		private void OnGUI()
		{
			if (!_clip || _index < 0)
				return;

			var so = new SerializedObject(_clip);
			EditorGUILayout.PropertyField(so.FindProperty("Dialogue").GetArrayElementAtIndex(_index), new GUIContent("Current DialogueItem"));
			so.ApplyModifiedProperties();
		}

		public void SetClip(CoopScoopAudioClip clip)
		{
			_clip = clip;
			titleContent = new GUIContent(_clip.name + " (" + _index + ")");
		}

		public void SetIndex(int dialogueIndex) => _index = dialogueIndex;
	}

	public class WaveEditor : EditorWindow
	{
		private Texture2D
			_buttonPreviewPlay = null,
			_buttonPreviewPause = null,
			_buttonPreviewStop = null,
			_buttonPreviewZoomIn = null,
			_buttonPreviewAdd = null,
			_buttonPreviewZoomOut = null;

		private AudioPlayer _player = new AudioPlayer();

		DialogueItemWindow _dialogueItemWindow = null;

		private CoopScoopAudioClip _clip = null;

		public float _wavePosition = 0.0f;
		private float _zoom = 1.0f;
		private TimelineInteraction _timelineInteraction = TimelineInteraction.TimelineInteraction_None;
		private int _dialogueIndex = 0;

		private WaveformDisplay _mainWaveform = new WaveformDisplay(), _smallWaveform = new WaveformDisplay();
		private Vector2 scrollPos;

		private void OnEnable()
		{
			load_resources();

			if (_clip)
				SetCPClip(_clip);
		}

		public void LoadFromFile(string assetPath)
		{
			CoopScoopAudioClip clip = AssetDatabase.LoadAssetAtPath<CoopScoopAudioClip>(assetPath);
			SetCPClip(clip);
		}

		public void LoadFromFile(CoopScoopAudioClip clip)
		{
			SetCPClip(clip);
		}

		private void SetCPClip(CoopScoopAudioClip clip)
		{
			_clip = clip;
			titleContent = new GUIContent(_clip.name);
			{
				DialogueItemWindow[] windows = Resources.FindObjectsOfTypeAll<DialogueItemWindow>();
				_dialogueItemWindow = windows.Length == 0 ? EditorWindow.GetWindow<DialogueItemWindow>() : windows[0];
			}
			_dialogueItemWindow.SetClip(_clip);
			SetClip(_clip.Clip);
			EditorUtility.SetDirty(_clip);
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
		}

		private void SetClip(AudioClip clip)
		{
			_clip.Clip = clip;
			_mainWaveform.SetClip(clip);
			_smallWaveform.SetClip(clip);
			_player.SetClip(_clip.Clip);
			Repaint();
		}

		private void load_resources()
		{
			_buttonPreviewPlay = Utils.LoadImage("button_preview_play");
			_buttonPreviewPause = Utils.LoadImage("button_preview_pause");
			_buttonPreviewStop = Utils.LoadImage("button_preview_stop");
			_buttonPreviewZoomIn = Utils.LoadImage("button_zoomin");
			_buttonPreviewZoomOut = Utils.LoadImage("button_zoomout");
			_buttonPreviewAdd = Utils.LoadImage("button_preview_add");
		}

		private void onguiToolbar(Rect toolbarRect)
		{
			float buttonMargin = 10;
			float buttonSize = 30;
			float toolbarDivide = toolbarRect.height / 2;
			Rect buttonPos = new Rect(new Vector3(toolbarRect.x + buttonMargin, toolbarRect.y + toolbarDivide - (buttonSize / 2)), new Vector2(buttonSize, buttonSize));
			if (GUI.Button(buttonPos, new GUIContent(_buttonPreviewStop, "Stop")))
				_player.SetState(AudioState.AudioState_Stopped);

			buttonPos.x += buttonSize + buttonMargin;
			if (_player.State == AudioState.AudioState_Playing)
			{
				if (GUI.Button(buttonPos, new GUIContent(_buttonPreviewPause, "Pause")))
					_player.SetState(AudioState.AudioState_Paused);
			}
			else
			{
				if (GUI.Button(buttonPos, new GUIContent(_buttonPreviewPlay, _player.Prev == AudioState.AudioState_Playing ? "Resume" : "Play")))
					_player.SetState(AudioState.AudioState_Playing, CalculateSamples(_wavePosition));
			}
			buttonPos.x += buttonSize + buttonMargin;
			if (GUI.Button(buttonPos, new GUIContent(_buttonPreviewZoomIn, "Zoom In")))
			{
				if (_zoom < 3.0f)
					SetZoom(_zoom + 0.1f);
				Repaint();
			}
			buttonPos.x += buttonSize + buttonMargin;
			if (GUI.Button(buttonPos, new GUIContent(_buttonPreviewZoomOut, "Zoom Out")))
			{
				if (_zoom > 1.0f)
					SetZoom(_zoom - 0.1f);
				Repaint();
			}
			buttonPos.x += buttonSize + buttonMargin;
			if (GUI.Button(buttonPos, new GUIContent(_buttonPreviewAdd, "Add Marker")))
			{
				AddMarker(CalculateSamples(_wavePosition));
				Repaint();
			}
		}

		private void OnGUI()
		{
			_player.Update();

			float width = (float)position.width;
			float height = (float)position.height;

			Draw(new Vector2(width, height), SubtitleEditorVariables.Color_TimelineBackground);
		}

		public void SetPlayer(AudioPlayer player) => _player = player;

		public void SetWavePosition(float position, bool zoom = false)
		{
			_wavePosition = position / (zoom ? _zoom : 1.0f);
			_player.SetPosition(CalculateSamples(_wavePosition));
		}

		public float GetTime() => _wavePosition * _clip.Clip.samples;

		public void SetZoom(float zoom) => _zoom = zoom;

		public uint CalculateSamples(float position)
		{
			float time_percentage = 1.0f / _clip.Clip.length * position;
			int samples = _clip.Clip.samples;
			uint playFrom = (uint)Mathf.FloorToInt(samples * time_percentage);

			return playFrom;
		}

		public void DrawBigWaveform(Rect bigRect, Color backgroundColor, ref Event evt)
		{
			if (!_clip)
				return;

			EditorGUI.DrawRect(bigRect, backgroundColor);
			Rect zoomedRect = bigRect;
			zoomedRect.width *= _zoom;
			zoomedRect.x = -scrollPos.x;

			Rect outlineRect = new Rect(bigRect.position, new Vector3(bigRect.width, 1));
			for (int i = 0; i < 3; i++)
			{
				outlineRect.y += bigRect.height / 4;
				EditorGUI.DrawRect(outlineRect, SubtitleEditorVariables.Color_TimelineBackline);
			}

			_mainWaveform.Draw(zoomedRect);
			float lineX = bigRect.size.x / (_clip ? _clip.Clip.length : 0) * _wavePosition;
			Rect line = new Rect(bigRect.position, new Vector2(1, bigRect.size.y));
			line.x += lineX;
			line.x *= _zoom;
			line.x -= scrollPos.x;
			EditorGUI.DrawRect(line, Color.white);

			if (_player.State == AudioState.AudioState_Playing)
			{
				float playLineX = bigRect.size.x / (_clip ? _clip.Clip.length : 0) * AudioUtility.GetClipPosition();
				Rect playLine = new Rect(bigRect.position, new Vector2(1, bigRect.size.y));
				playLine.x += playLineX;
				playLine.x *= _zoom;
				playLine.x -= scrollPos.x;
				EditorGUI.DrawRect(playLine, Color.red);
				Repaint();
			}

			Event e = Event.current;
			switch (e.type)
			{
				case EventType.ScrollWheel:
				{
					if (bigRect.Contains(e.mousePosition))
					{
						if (e.delta.y < 0)
						{
							if (_zoom < 3.0f)
								SetZoom(_zoom + 0.1f);
						}
						else
						{
							if (_zoom > 1.0f)
								SetZoom(_zoom - 0.1f);
						}
						e.Use();
						Repaint();
					}
					break;
				}
			}

			outlineRect = new Rect(bigRect.position, new Vector3(bigRect.width, 1));
			EditorGUI.DrawRect(outlineRect, SubtitleEditorVariables.Color_Timeline);

			Rect scrollRect = new Rect(bigRect.position, bigRect.size);
			scrollPos = GUI.BeginScrollView(scrollRect, scrollPos, zoomedRect, true, false, GUI.skin.horizontalScrollbar, GUIStyle.none);
			GUI.EndScrollView();
		}

		public void DrawSmallWaveform(Rect smallRect, Color backgroundColor, ref Event evt)
		{
			EditorGUI.DrawRect(smallRect, backgroundColor);

			_smallWaveform.Draw(smallRect);

			float lineX = smallRect.size.x / (_clip ? _clip.Clip.length : 0) * _wavePosition;
			Rect line = new Rect(smallRect.position, new Vector2(1, smallRect.size.y));
			line.x += lineX;
			EditorGUI.DrawRect(line, Color.white);

			if (_player.State == AudioState.AudioState_Playing)
			{
				float playLineX = smallRect.size.x / (_clip ? _clip.Clip.length : 0) * AudioUtility.GetClipPosition();
				Rect playLine = new Rect(smallRect.position, new Vector2(1, smallRect.size.y));
				playLine.x += playLineX;
				EditorGUI.DrawRect(playLine, Color.red);
				Repaint();
			}
		}

		private void DrawBigDialogueItems(Rect bigRect, ref Event evt)
		{
			for (int i = 0; i < _clip.Dialogue.Count; i++)
			{
				float markerX = bigRect.size.x / _clip.Clip.samples * _clip.Dialogue[i].SamplePosition;
				Rect markerRect = new Rect(new Vector2(markerX, bigRect.y), new Vector2(1, bigRect.size.y));
				markerRect.x *= _zoom;
				markerRect.x -= scrollPos.x;

				Color color = SubtitleEditorVariables.Color_Marker;
				if (i == _dialogueIndex)
				{
					color = SubtitleEditorVariables.Color_MarkerSelected;
				}
				EditorGUI.DrawRect(markerRect, color);

				Rect extraRect = new Rect(new Vector2(markerRect.x, markerRect.y - 7.5f), new Vector2(20, 15));
				EditorGUI.DrawRect(extraRect, color);

				switch (evt.type)
				{
					case EventType.MouseDrag:
					{
						if (_timelineInteraction != TimelineInteraction.TimelineInteraction_Marker && _timelineInteraction != TimelineInteraction.TimelineInteraction_None)
							break;

						markerRect.x -= 15;
						markerRect.width = 30;

						if (!markerRect.Contains(evt.mousePosition) && !extraRect.Contains(evt.mousePosition))
							break;

						if (_clip)
						{
							_timelineInteraction = TimelineInteraction.TimelineInteraction_Marker;
							float mousePosx = evt.mousePosition.x;
							mousePosx /= _zoom;
							mousePosx += scrollPos.x;
							float test = 1.0f / bigRect.width * mousePosx;
							_clip.Dialogue[i].SamplePosition = (uint)Mathf.FloorToInt(_clip.Clip.samples * test);
							evt.Use();
							Repaint();
						}
						break;
					}
					case EventType.MouseDown:
					{
						if (_timelineInteraction != TimelineInteraction.TimelineInteraction_Marker && _timelineInteraction != TimelineInteraction.TimelineInteraction_None)
							break;

						if (!markerRect.Contains(evt.mousePosition) && !extraRect.Contains(evt.mousePosition))
							break;

						_dialogueIndex = i;
						_dialogueItemWindow.SetIndex(_dialogueIndex);
						_dialogueItemWindow.SetClip(_clip);
						_dialogueItemWindow.Show();
						_dialogueItemWindow.Focus();
						evt.Use();
						Repaint();
						break;
					}
				}
			}
		}

		private void DrawSmallDialogueItems(Rect smallRect, ref Event evt)
		{
			for (int i = 0; i < _clip.Dialogue.Count; i++)
			{
				Color color = SubtitleEditorVariables.Color_Marker;
				if (i == _dialogueIndex)
					color = SubtitleEditorVariables.Color_MarkerSelected;

				float markerX = smallRect.size.x / _clip.Clip.samples * _clip.Dialogue[i].SamplePosition;
				Rect markerRect = new Rect(new Vector2(markerX, smallRect.y), new Vector2(1, smallRect.size.y));
				EditorGUI.DrawRect(markerRect, color);

				Rect extraRect = new Rect(new Vector2(markerRect.x, markerRect.y - 4f), new Vector2(10, 8));
				EditorGUI.DrawRect(extraRect, color);

				switch (evt.type)
				{
					case EventType.MouseDrag:
					{
						if (_timelineInteraction != TimelineInteraction.TimelineInteraction_Marker && _timelineInteraction != TimelineInteraction.TimelineInteraction_None)
							break;

						if (!markerRect.Contains(evt.mousePosition) && !extraRect.Contains(evt.mousePosition))
							break;

						if (_clip)
						{
							_timelineInteraction = TimelineInteraction.TimelineInteraction_Marker;
							float test = 1.0f / smallRect.width * evt.mousePosition.x;
							_clip.Dialogue[i].SamplePosition = (uint)Mathf.FloorToInt(_clip.Clip.samples * test);
							evt.Use();
							Repaint();
						}
						break;
					}
					case EventType.MouseDown:
					{
						if (_timelineInteraction != TimelineInteraction.TimelineInteraction_Marker && _timelineInteraction != TimelineInteraction.TimelineInteraction_None)
							break;

						if (!markerRect.Contains(evt.mousePosition) && !extraRect.Contains(evt.mousePosition))
							break;

						_dialogueIndex = i;
						_dialogueItemWindow.SetIndex(_dialogueIndex);
						_dialogueItemWindow.SetClip(_clip);
						_dialogueItemWindow.Show();
						_dialogueItemWindow.Focus();
						evt.Use();
						Repaint();
						break;
					}
				}
			}
		}

		public void DrawContainer(Rect rect)
		{
			EditorGUI.DrawRect(rect, SubtitleEditorVariables.Color_ContainerBackground);
		}

		public void Draw(Vector2 size, Color backgroundColor)
		{
			if (!_clip)
				return;

			//if (_clip.Dialogue.Count != 0 && _dialogueIndex < _clip.Dialogue.Count)
			//{
			//	var so = new SerializedObject(_clip);
			//	EditorGUILayout.PropertyField(so.FindProperty("Dialogue").GetArrayElementAtIndex(_dialogueIndex), new GUIContent("Current DialogueItem"));
			//	so.ApplyModifiedProperties();
			//}

			Event e = Event.current;

			Rect sizeLeft = new Rect(new Vector3(0, 0), size);
			float margin_top = 25;
			sizeLeft.height -= margin_top;
			sizeLeft.y += margin_top;

			Rect smallWaveformRect = new Rect(sizeLeft.position, new Vector2(sizeLeft.width, sizeLeft.height / 8));
			DrawSmallWaveform(smallWaveformRect, backgroundColor, ref e);

			Rect outlineRect = new Rect(sizeLeft.position, new Vector3(sizeLeft.width, 1));
			EditorGUI.DrawRect(outlineRect, SubtitleEditorVariables.Color_OutlineColor);

			sizeLeft.height -= smallWaveformRect.height;
			sizeLeft.y += smallWaveformRect.height;

			Rect containerRect = sizeLeft;
			DrawContainer(containerRect);

			outlineRect = new Rect(sizeLeft.position, new Vector3(sizeLeft.width, 1));
			EditorGUI.DrawRect(outlineRect, SubtitleEditorVariables.Color_OutlineColor);

			int i = 8;
			float height = sizeLeft.height / i;

			Rect guiRect = new Rect(sizeLeft.position, new Vector3(sizeLeft.width, height));
			onguiToolbar(guiRect);

			sizeLeft.height -= guiRect.height;
			sizeLeft.y += guiRect.height;

			Rect bigWaveformRect = sizeLeft;
			DrawBigWaveform(sizeLeft, backgroundColor, ref e);

			sizeLeft.height -= bigWaveformRect.height;
			sizeLeft.y += bigWaveformRect.height;

			DrawSmallDialogueItems(smallWaveformRect, ref e);
			DrawBigDialogueItems(bigWaveformRect, ref e);

			switch (e.type)
			{
				case EventType.MouseDrag:
				case EventType.MouseDown:
				{
					if (_timelineInteraction != TimelineInteraction.TimelineInteraction_Time && _timelineInteraction != TimelineInteraction.TimelineInteraction_None)
						break;

					if (smallWaveformRect.Contains(e.mousePosition))
					{
						_timelineInteraction = TimelineInteraction.TimelineInteraction_Time;
						SetWavePosition(_clip.Clip.length / smallWaveformRect.size.x * e.mousePosition.x, false);
						e.Use();
						Repaint();
					}
					else if (bigWaveformRect.Contains(e.mousePosition))
					{
						_timelineInteraction = TimelineInteraction.TimelineInteraction_Time;
						SetWavePosition(_clip.Clip.length / bigWaveformRect.size.x * e.mousePosition.x, true);
						e.Use();
						Repaint();
					}
					break;
				}
				case EventType.DragUpdated:
				case EventType.DragPerform:
				{
					if (!bigWaveformRect.Contains(e.mousePosition) && !bigWaveformRect.Contains(e.mousePosition))
						break;

					Object dragged_object = DragAndDrop.objectReferences[0];
					DragAndDrop.visualMode = dragged_object.GetType() == typeof(AudioClip) ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;

					if (e.type == EventType.DragPerform)
					{
						DragAndDrop.AcceptDrag();

						if (dragged_object.GetType() == typeof(AudioClip))
							SetClip((AudioClip)dragged_object);

						e.Use();
						Repaint();
					}
					break;
				}
				case EventType.MouseUp:
				{
					_timelineInteraction = TimelineInteraction.TimelineInteraction_None;
					Repaint();
					break;
				}
			}
		}

		public void AddMarker(uint samplePosition)
		{
			if (_clip.Dialogue.Find(x => x.SamplePosition == samplePosition) != null)
				return;

			DialogueItem dialogueItem = new DialogueItem();
			dialogueItem.SamplePosition = samplePosition;
			_clip.Dialogue.Add(dialogueItem);
		}
	}
}