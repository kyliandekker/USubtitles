using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace USubtitles.Editor
{
	[ExecuteInEditMode]
	[CustomEditor(typeof(UAudioClip), true)]
	public class UAudioClipEditor : UnityEditor.Editor
	{
		private UAudioClip _clip = null;

		// Timeline variables
		private AudioPlayer _player = new AudioPlayer();
		private WaveformDisplay _smallWaveform = new WaveformDisplay();

		private float _zoom = 1.0f;
		private Vector2 scrollPos;
		private int _dialogueIndex = -1;
		private TimelineInteraction _timelineInteraction = TimelineInteraction.TimelineInteraction_None;

		// Toolbar textures
		private Texture2D
			_buttonPreviewPlay = null,
			_buttonPreviewPause = null,
			_buttonPreviewStop = null,
			_buttonPreviewZoomIn = null,
			_buttonPreviewZoomOut = null,
			_buttonPreviewAdd = null,
			_buttonPreviewRemove = null;

		SerializedProperty _currentDialogueItem = null;
		protected Rect rect;

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			if (!_clip || !_smallWaveform.Clip)
			{
				_clip = target as UAudioClip;
				SetClip(_clip.Clip);
				LoadResources();
				EditorPreferences.Load();
			}

			if (!_clip)
				return;

			_player.Update();

			Vector2 size = new Vector2(EditorGUIUtility.currentViewWidth, Screen.height);

			Rect toolbarRect = new Rect(0, 0, size.x, size.y / 20);

			Rect smallWaveformRect = new Rect(0, 0, size.x, size.y / 5);
			smallWaveformRect.y += toolbarRect.height;

			float buttonMargin = 25;
			Rect dialogueRect = smallWaveformRect;
			dialogueRect.y += smallWaveformRect.height;
			if (_currentDialogueItem != null)
				dialogueRect.height = EditorGUI.GetPropertyHeight(_currentDialogueItem) + buttonMargin;
			dialogueRect.y += buttonMargin;
			dialogueRect.x += buttonMargin;
			dialogueRect.width -= buttonMargin * 2;

			DrawToolbar(toolbarRect);

			DrawSmallWaveform(smallWaveformRect);

			bool showDialogueItem = _dialogueIndex > -1 && _dialogueIndex < _clip.Dialogue.Count && _currentDialogueItem != null;

			float totalHeight = smallWaveformRect.height + toolbarRect.height + (showDialogueItem ? dialogueRect.height : 0);
			rect = EditorGUILayout.GetControlRect(GUILayout.Width(size.x), GUILayout.Height(totalHeight));

			if (showDialogueItem)
			{
				serializedObject.Update();
				_ = EditorGUI.PropertyField(dialogueRect, _currentDialogueItem, new GUIContent("Current DialogueItem"));
			}

			_ = serializedObject.ApplyModifiedProperties();
		}

		private void LoadResources()
		{
			_buttonPreviewPlay = Utils.LoadImage("button_preview_play");
			_buttonPreviewPause = Utils.LoadImage("button_preview_pause");
			_buttonPreviewStop = Utils.LoadImage("button_preview_stop");
			_buttonPreviewZoomIn = Utils.LoadImage("button_zoomin");
			_buttonPreviewZoomOut = Utils.LoadImage("button_zoomout");
			_buttonPreviewAdd = Utils.LoadImage("button_preview_add");
			_buttonPreviewRemove = Utils.LoadImage("button_preview_remove");
		}

		public void AddMarker(uint samplePosition)
		{
			if (_clip.Dialogue.Find(x => x.SamplePosition == samplePosition) != null)
				return;

			DialogueItem dialogueItem = new DialogueItem();
			dialogueItem.SamplePosition = samplePosition;
			_clip.Dialogue.Add(dialogueItem);

			SetMarker(_clip.Dialogue.Count - 1);
			Repaint();
		}

		private void DrawToolbar(Rect toolbarRect)
		{
			float buttonMargin = 10;
			float buttonSize = 30;
			float toolbarDivide = toolbarRect.height / 2;
			Rect buttonPos = new Rect(new Vector3(toolbarRect.x + buttonMargin, toolbarRect.y + toolbarDivide - (buttonSize / 2)), new Vector2(buttonSize, buttonSize));
			if (GUI.Button(buttonPos, new GUIContent(_buttonPreviewStop, "Stop")))
				_player.SetState(AudioState.AudioState_Stopped);

			EditorGUI.DrawRect(toolbarRect, SubtitleEditorVariables.Color_ToolbarBackground);

			buttonPos.x += buttonSize + buttonMargin;
			if (_player.State == AudioState.AudioState_Playing)
			{
				if (GUI.Button(buttonPos, new GUIContent(_buttonPreviewPause, "Pause")))
					_player.SetState(AudioState.AudioState_Paused);
			}
			else
			{
				if (GUI.Button(buttonPos, new GUIContent(_buttonPreviewPlay, _player.Prev == AudioState.AudioState_Playing ? "Resume" : "Play")))
					_player.SetState(AudioState.AudioState_Playing, CalculateSamples(_player.WavePosition));
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
				AddMarker(CalculateSamples(_player.WavePosition));
			bool enabled = GUI.enabled;
			GUI.enabled = _dialogueIndex > -1 && _dialogueIndex < _clip.Dialogue.Count ? true : false;
			buttonPos.x += buttonSize + buttonMargin;
			if (GUI.Button(buttonPos, new GUIContent(_buttonPreviewRemove, "Remove Marker")))
				RemoveMarker();
			GUI.enabled = enabled;
		}

		private void RemoveMarker()
		{
			if (_currentDialogueItem != null && _dialogueIndex > -1 && _dialogueIndex < _clip.Dialogue.Count)
			{
				_currentDialogueItem = null;
				_clip.Dialogue.RemoveAt(_dialogueIndex);
				_dialogueIndex = -1;
				serializedObject.Update();
				Repaint();
			}
		}

		private void SetClip(AudioClip clip)
		{
			_smallWaveform.SetClip(clip);
			_player.SetClip(clip);
		}

		public void SetZoom(float zoom) => _zoom = zoom;

		public uint CalculateSamples(float position)
		{
			float time_percentage = 1.0f / _clip.Clip.length * position;
			int samples = _clip.Clip.samples;
			uint playFrom = (uint)Mathf.FloorToInt(samples * time_percentage);

			return playFrom;
		}
		public void SetWavePosition(float position, bool zoom = false)
		{
			_player.WavePosition = position / (zoom ? _zoom : 1.0f);
			_player.SetPosition(CalculateSamples(_player.WavePosition));
		}

		struct DrawRect
		{
			public Rect rect;
			public Color color;

			public DrawRect(Rect rect, Color color)
			{
				this.rect = rect;
				this.color = color;
			}
		};

		private void DrawSmallWaveform(Rect rect)
		{
			Rect zoomedRect = rect;
			zoomedRect.width *= _zoom;
			zoomedRect.x = -scrollPos.x;

			Rect scrollRect = new Rect(rect.position, rect.size);
			scrollPos = GUI.BeginScrollView(scrollRect, scrollPos, zoomedRect, true, false, GUI.skin.horizontalScrollbar, GUIStyle.none);

			#region TIMELINE
			EditorGUI.DrawRect(rect, SubtitleEditorVariables.Color_TimelineBackground);
			Rect outlineRect = new Rect(rect.position, new Vector3(rect.width, 1));
			for (int i = 0; i < 3; i++)
			{
				outlineRect.y += rect.height / 4;
				EditorGUI.DrawRect(outlineRect, SubtitleEditorVariables.Color_TimelineBackline);
			}
#endregion

#region TIMELINE_WAVEFORM
			_smallWaveform.Draw(zoomedRect);
#endregion

#region TIMELINE_LINE
			float lineX = rect.size.x / (_clip ? _clip.Clip.length : 0) * _player.WavePosition;
			Rect line = new Rect(rect.position, new Vector2(1, rect.size.y));
			line.x += lineX;
			line.x *= _zoom;
			line.x -= scrollPos.x;
			EditorGUI.DrawRect(line, Color.white);

			// Red playing line
			if (_player.State == AudioState.AudioState_Playing)
			{
				float playLineX = rect.size.x / (_clip ? _clip.Clip.length : 0) * AudioUtility.GetClipPosition();
				Rect playLine = new Rect(rect.position, new Vector2(1, rect.size.y));
				playLine.x += playLineX;
				playLine.x *= _zoom;
				playLine.x -= scrollPos.x;
				EditorGUI.DrawRect(playLine, Color.red);
				Repaint();
			}
#endregion
			Event e = Event.current;
			List<DrawRect> drawList = new List<DrawRect>();

			for (int i = 0; i < _clip.Dialogue.Count; i++)
			{
				float markerX = rect.size.x / _clip.Clip.samples * _clip.Dialogue[i].SamplePosition;

				float marker_height = 15.0f;
				float marker_width = 10.0f;

				if (_clip.Dialogue[i].Clear)
				{
					Rect clearRect = new Rect(new Vector2(markerX + (marker_width / 2), rect.y - marker_height), new Vector2(marker_width, marker_height));
					drawList.Add(new DrawRect(clearRect, SubtitleEditorVariables.Color_MarkerColorClear));
				}

				Rect markerRect = new Rect(new Vector2(markerX, rect.y), new Vector2(1, rect.size.y));
				markerRect.x *= _zoom;
				markerRect.x -= scrollPos.x;

				Rect extraRect = new Rect(new Vector2(markerRect.x, markerRect.y - 7.5f), new Vector2(20, 15));

				Rect fullRect = markerRect;
				fullRect.y = extraRect.y;
				fullRect.x -= 17.5f;
				fullRect.width = 35f;


				Color color = SubtitleEditorVariables.Color_Marker;
				if (fullRect.Contains(e.mousePosition))
					color = SubtitleEditorVariables.Color_MarkerHover;
				if (i == _dialogueIndex)
					color = SubtitleEditorVariables.Color_MarkerSelected;

				EditorGUI.DrawRect(markerRect, color);
				drawList.Add(new DrawRect(extraRect, color));

				switch (e.type)
				{
					case EventType.MouseDrag:
					{
						if (_timelineInteraction != TimelineInteraction.TimelineInteraction_Marker && _timelineInteraction != TimelineInteraction.TimelineInteraction_None)
							break;

						if (!fullRect.Contains(e.mousePosition))
							break;

						if (_clip)
						{
							_timelineInteraction = TimelineInteraction.TimelineInteraction_Marker;
							float mousePosx = e.mousePosition.x;
							mousePosx /= _zoom;
							mousePosx += scrollPos.x;
							float test = 1.0f / rect.width * mousePosx;
							_clip.Dialogue[i].SamplePosition = (uint)Mathf.FloorToInt(_clip.Clip.samples * test);
							e.Use();
							Repaint();
						}
						break;
					}
					case EventType.MouseDown:
					{
						if (_timelineInteraction != TimelineInteraction.TimelineInteraction_Marker && _timelineInteraction != TimelineInteraction.TimelineInteraction_None)
							break;

						if (!fullRect.Contains(e.mousePosition))
							break;

						_timelineInteraction = TimelineInteraction.TimelineInteraction_Marker;
						SetMarker(i);
						e.Use();
						Repaint();
						break;
					}
				}
			}

			switch (e.type)
			{
				case EventType.MouseUp:
				{
					_timelineInteraction = TimelineInteraction.TimelineInteraction_None;
					break;
				}
				case EventType.KeyDown:
				{
					if (e.keyCode == KeyCode.Space)
					{
						if (_player.GetState() == AudioState.AudioState_Playing)
							_player.SetState(AudioState.AudioState_Paused);
						else
							_player.SetState(AudioState.AudioState_Playing, CalculateSamples(_player.WavePosition));
					}
					else if (e.keyCode == KeyCode.Delete)
						RemoveMarker();
					else if (e.keyCode == KeyCode.M)
						AddMarker(CalculateSamples(_player.WavePosition));
					break;
				}
				case EventType.MouseDown:
				case EventType.MouseDrag:
				case EventType.ScrollWheel:
				case EventType.DragPerform:
				case EventType.DragUpdated:
				{
					if (_timelineInteraction != TimelineInteraction.TimelineInteraction_Time && _timelineInteraction != TimelineInteraction.TimelineInteraction_None)
						break;

					if (!rect.Contains(e.mousePosition))
						break;

					if (e.type == EventType.ScrollWheel)
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
						Repaint();
					}
					else if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
					{
						_timelineInteraction = TimelineInteraction.TimelineInteraction_Time;
						_dialogueIndex = -1;

						SetWavePosition(_clip.Clip.length / rect.size.x * (e.mousePosition.x - (rect.x * _zoom)), true);
						Repaint();
					}
					else if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
					{
						_timelineInteraction = TimelineInteraction.TimelineInteraction_Time;
						_dialogueIndex = -1;

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
					}
					e.Use();
					break;
				}
			}

			GUI.EndScrollView();

			for (int i = 0; i < drawList.Count; i++)
				EditorGUI.DrawRect(drawList[i].rect, drawList[i].color);
		}

		public override bool RequiresConstantRepaint() => true;

		private void SetMarker(int index)
		{
			_dialogueIndex = index;
			serializedObject.Update();
			_currentDialogueItem = serializedObject.FindProperty("Dialogue").GetArrayElementAtIndex(_dialogueIndex);
		}
		
		[MenuItem("Assets/Create/UAudioClip", priority = 1)]
		private static void CreateUAudioClipFromAudioClip()
		{
			for (int i = 0; i < Selection.objects.Length; i++)
			{
				Object obj = Selection.objects[i];

				UAudioClip uAudioClip = ScriptableObject.CreateInstance<UAudioClip>();
				uAudioClip.Clip = obj as AudioClip;
				string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(AssetDatabase.GetAssetPath(obj) + ".asset");

				AssetDatabase.CreateAsset(uAudioClip, assetPathAndName);

				AssetDatabase.SaveAssets();
				AssetDatabase.Refresh();
				EditorUtility.FocusProjectWindow();
			}
		}

		[MenuItem("Assets/Create/UAudioClip", true)]
		private static bool CreateUAudioClipFromAudioClipValidation() => Selection.activeObject is AudioClip;
	}
}