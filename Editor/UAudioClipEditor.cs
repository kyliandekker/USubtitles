using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace USubtitles.Editor
{
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

	struct Interaction
	{
		public InteractionType LastInteraction;
		public int Index;
	}
	 
	[ExecuteInEditMode]
	[CustomEditor(typeof(UAudioClip), true)]
	public class UAudioClipEditor : UnityEditor.Editor
	{
		private const bool DEBUG_DRAW = false;
		private const int RIGHT_CLICK = 1;
		private const int LEFT_CLICK = 0;

		// Button variables.
		private const float buttonSize = 30;
		private const float buttonMargin = 10;

		// Button variables.
		private Vector2 _markerClearRectSize = new Vector2(10.0f, 15.0f);
		private Vector2 _markerTopRectSize = new Vector2(20, 15);

		// Timeline variables
		private UAudioClip _clip = null;
		private AudioPlayer _player = new AudioPlayer();
		private WaveformDisplay _waveform = new WaveformDisplay();

		// Zoom variables
		private const float _zoomStrength = 0.1f, _zoomMax = 3.0f, _zoomMin = 1.0f;
		private float _zoom = _zoomMin;
		private Vector2 _scrollPos = Vector2.zero;

		private int _dialogueIndex = -1;

		private Interaction _currentInteraction = new Interaction();

		private List<DrawRect> _drawList = new List<DrawRect>();

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
		protected Rect fullRect = new Rect();

		public override void OnInspectorGUI()
		{
			// Clear the drawing list so that no previous rects remain.
			_drawList.Clear();

			serializedObject.Update();

			// Fallback option for when the audio clip has not been set.
			if (!_clip || !_waveform.Clip)
			{
				_clip = target as UAudioClip;
				SetClip(_clip.Clip);
				LoadResources();
				EditorPreferences.Load();
			}

			// If there still is no clip, do not draw anything at all.
			if (!_clip)
				return;

			// Update the player (necessary for detecting end of audio clip).
			_player.Update();

			Vector2 size = new Vector2(EditorGUIUtility.currentViewWidth, Screen.height);

			// Calculate the size of the toolbar.
			Rect toolbarRect = new Rect(0, 0, size.x, size.y / 20);

			DrawToolbar(toolbarRect);

			// Calculate the size of the waveform.
			Rect waveformRect = new Rect(0, 0, size.x, size.y / 5);
			waveformRect.y += toolbarRect.height;

			DrawWaveform(waveformRect);

			// Calculate the size and position of the current dialogue item.
			float margin = 25;
			Rect dialogueItemRect = waveformRect;
			dialogueItemRect.y += waveformRect.height;
			if (_currentDialogueItem != null)
				dialogueItemRect.height = EditorGUI.GetPropertyHeight(_currentDialogueItem) + margin;
			dialogueItemRect.y += margin;
			dialogueItemRect.x += margin;
			dialogueItemRect.width -= margin * 2;

			bool showDialogueItem = _dialogueIndex > -1 && _dialogueIndex < _clip.Dialogue.Count && _currentDialogueItem != null;

			if (showDialogueItem)
				_ = EditorGUI.PropertyField(dialogueItemRect, _currentDialogueItem, new GUIContent("Current DialogueItem"));

			float totalHeight = waveformRect.height + toolbarRect.height + (showDialogueItem ? dialogueItemRect.height : 0);
			fullRect = EditorGUILayout.GetControlRect(GUILayout.Width(size.x), GUILayout.Height(totalHeight));

			Event e = Event.current;
			switch (e.type)
			{
				case EventType.MouseUp:
				{
					_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_None;
					break;
				}
				case EventType.KeyDown:
				{
					if (e.keyCode == KeyCode.Space && e.control)
					{
						_player.SetState(AudioState.AudioState_Stopped);
						_player.SetState(AudioState.AudioState_Playing, CalculateSamples(_player.WavePosition));
						e.Use();
					}
					// Play hotkey.
					else if (e.keyCode == KeyCode.Space)
					{
						if (_player.GetState() == AudioState.AudioState_Playing)
						{
							_player.SetState(AudioState.AudioState_Paused);
							e.Use();
						}
						else
						{
							_player.SetState(AudioState.AudioState_Playing, CalculateSamples(_player.WavePosition));
							e.Use();
						}
					}
					break;
				}
			}

			_ = serializedObject.ApplyModifiedProperties();
		}

		/// <summary>
		/// Loads the resources.
		/// </summary>
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

		/*##################
		 * Utilities.
		##################*/

		/// <summary>
		/// Sets the audio clip of the waveform and player.
		/// </summary>
		/// <param name="clip">The audio clip.</param>
		private void SetClip(AudioClip clip)
		{
			_waveform.SetClip(clip);
			_player.SetClip(clip);
		}

		/// <summary>
		/// Sets the zoom.
		/// </summary>
		/// <param name="zoom">The zoom of the timeline.</param>
		public void SetZoom(float zoom) => _zoom = zoom;

		/// <summary>
		/// Calculates the sample based on the position in the waveform and the length of the clip.
		/// </summary>
		/// <param name="position">Position of the timeline line.</param>
		/// <returns></returns>
		public uint CalculateSamples(float position)
		{
			float time_percentage = 1.0f / _clip.Clip.length * position;
			int samples = _clip.Clip.samples;
			uint playFrom = (uint)Mathf.FloorToInt(samples * time_percentage);

			return playFrom;
		}

		/// <summary>
		/// Sets the position of playback in the player.
		/// </summary>
		/// <param name="position">Position of the timeline line.</param>
		/// <param name="zoom">The zoom of the timeline.</param>
		public void SetWavePosition(float position)
		{
			_player.WavePosition = position;
			_player.SetPosition(CalculateSamples(_player.WavePosition));
		}

		/*##################
		 * Drawing GUI Elements.
		##################*/

		/// <summary>
		/// Utility function for drawing the toolbar buttons. Automatically adds margin to the lastPosition rect.
		/// </summary>
		/// <param name="position">Position the button needs to be drawn in.</param>
		/// <param name="content">GUI Content of the button (icon and text).</param>
		/// <returns></returns>
		private bool DrawToolbarButton(ref Rect position, GUIContent content)
		{
			Rect buttonPosition = position;
			position.x += buttonSize + buttonMargin;
			return GUI.Button(buttonPosition, content);
		}

		/// <summary>
		/// Draws the toolbar.
		/// </summary>
		/// <param name="toolbarRect">The total space reserved for the toolbar.</param>
		private void DrawToolbar(Rect toolbarRect)
		{
			// Draw the background.
			EditorGUI.DrawRect(toolbarRect, SubtitleEditorVariables.Preferences.Color_ToolbarBackground);

			float toolbarDivide = toolbarRect.height / 2;
			Rect buttonPos = new Rect(new Vector3(toolbarRect.x + buttonMargin, toolbarRect.y + toolbarDivide - (buttonSize / 2)), new Vector2(buttonSize, buttonSize));

			// Button drawing.
			if (DrawToolbarButton(ref buttonPos, new GUIContent(_buttonPreviewStop, "Stop")))
				_player.SetState(AudioState.AudioState_Stopped);

			if (_player.State == AudioState.AudioState_Playing)
			{
				if (DrawToolbarButton(ref buttonPos, new GUIContent(_buttonPreviewPause, "Pause")))
					_player.SetState(AudioState.AudioState_Paused);
			}
			else
			{
				if (DrawToolbarButton(ref buttonPos, new GUIContent(_buttonPreviewPlay, _player.Prev == AudioState.AudioState_Playing ? "Resume" : "Play")))
					_player.SetState(AudioState.AudioState_Playing, CalculateSamples(_player.WavePosition));
			}

			if (DrawToolbarButton(ref buttonPos, new GUIContent(_buttonPreviewZoomIn, "Zoom In")))
			{
				if (_zoom < _zoomMax)
					SetZoom(_zoom + _zoomStrength);
				Repaint();
			}

			if (DrawToolbarButton(ref buttonPos, new GUIContent(_buttonPreviewZoomOut, "Zoom Out")))
			{
				if (_zoom > _zoomMin)
					SetZoom(_zoom - _zoomStrength);
				Repaint();
			}

			if (DrawToolbarButton(ref buttonPos, new GUIContent(_buttonPreviewAdd, "Add Marker")))
				AddMarker(CalculateSamples(_player.WavePosition));
			bool enabled = GUI.enabled;
			GUI.enabled = _dialogueIndex > -1 && _dialogueIndex < _clip.Dialogue.Count ? true : false;

			if (DrawToolbarButton(ref buttonPos, new GUIContent(_buttonPreviewRemove, "Remove Marker")))
				RemoveMarker(_dialogueIndex);
			GUI.enabled = enabled;
		}

		/// <summary>
		/// Draws all the markers in the waveform.
		/// </summary>
		/// <param name="waveFormRect">The total space reserved for the waveform.</param>
		private void DrawMarkers(Rect waveFormRect, Rect zoomedRect)
		{
			for (int i = 0; i < _clip.Dialogue.Count; i++)
			{
				// The exact x sample position of the marker.
				float markerX = waveFormRect.size.x / _clip.Clip.samples * _clip.Dialogue[i].SamplePosition;

				// If the sample has the clear option enabled, a rect appear on top of it to signify this.
				if (_clip.Dialogue[i].Clear)
				{
					Rect clearRect = new Rect(new Vector2(markerX + (_markerClearRectSize.x / 2), waveFormRect.y - _markerClearRectSize.y), new Vector2(_markerClearRectSize.x, _markerClearRectSize.y));
					_drawList.Add(new DrawRect(clearRect, SubtitleEditorVariables.Preferences.Color_MarkerClear));
				}

				// The marker line itself.
				Rect markerRect = new Rect(new Vector2(markerX, waveFormRect.y), new Vector2(1, waveFormRect.size.y));
				markerRect.x *= _zoom;
				markerRect.x -= _scrollPos.x;

				// The top part of the marker line.
				Rect topRect = new Rect(new Vector2(markerRect.x, markerRect.y - (_markerTopRectSize.y / 2)), _markerTopRectSize);

				// The full clickable area of the marker (needed for events).
				Rect fullRect = markerRect;
				fullRect.y = topRect.y;
				fullRect.x -= _markerTopRectSize.x / 2;
				fullRect.width = _markerTopRectSize.x / 2 * 3;

				if (DEBUG_DRAW)
					_drawList.Add(new DrawRect(fullRect, Color.red));

				Event e = Event.current;

				// Set the color based on whether the user is hovering over it, whether it is selected or not.
				Color color = SubtitleEditorVariables.Preferences.Color_Marker;
				if (fullRect.Contains(e.mousePosition))
					color = SubtitleEditorVariables.Preferences.Color_MarkerHover;
				if (i == _dialogueIndex)
					color = SubtitleEditorVariables.Preferences.Color_MarkerSelected;

				EditorGUI.DrawRect(markerRect, color);

				// The top rect needs to be drawn outside of the scrollable area.
				_drawList.Add(new DrawRect(topRect, color));

				if (_currentInteraction.Index == i && _currentInteraction.LastInteraction == InteractionType.TimelineInteraction_Marker && e.type == EventType.MouseDrag && e.button == LEFT_CLICK)
				{
					float test = _clip.Clip.length / zoomedRect.size.x * (_scrollPos.x + e.mousePosition.x);
					SetMarker(i);
					_clip.Dialogue[i].SamplePosition = CalculateSamples(test);
					if (_clip.Dialogue[i].SamplePosition > _clip.Clip.samples)
						_clip.Dialogue[i].SamplePosition = (uint)_clip.Clip.samples;
					e.Use();
					SetWavePosition(_clip.Clip.length / _clip.Clip.samples * _clip.Dialogue[i].SamplePosition);
					Repaint();
				}

				switch (e.type)
				{
					// Dragging a marker.
					case EventType.MouseDrag:
					{
						if (_currentInteraction.LastInteraction != InteractionType.TimelineInteraction_Marker && _currentInteraction.LastInteraction != InteractionType.TimelineInteraction_None)
							break;

						if (!fullRect.Contains(e.mousePosition))
							break;

						if (e.button != LEFT_CLICK)
							break;

						if (!_clip)
							break;

						// Set this marker as the current marker and drag it to the new position.
						_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_Marker;
						_currentInteraction.Index = i;

						float test = _clip.Clip.length / zoomedRect.size.x * (_scrollPos.x + e.mousePosition.x);
						SetMarker(i);
						_clip.Dialogue[i].SamplePosition = CalculateSamples(test);
						if (_clip.Dialogue[i].SamplePosition > _clip.Clip.samples)
							_clip.Dialogue[i].SamplePosition = (uint) _clip.Clip.samples;
						SetWavePosition(_clip.Clip.length / _clip.Clip.samples * _clip.Dialogue[i].SamplePosition);
						e.Use();
						Repaint();
						break;
					}
					// Left or right clicking on a marker.
					case EventType.MouseDown:
					{
						if (_currentInteraction.LastInteraction != InteractionType.TimelineInteraction_Marker && _currentInteraction.LastInteraction != InteractionType.TimelineInteraction_None)
							break;

						if (!fullRect.Contains(e.mousePosition))
							break;

						// Right click.
						if (e.button == RIGHT_CLICK)
						{
							_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_Marker;
							ShowMarkerContextMenu(i);
							e.Use();
							Repaint();
						}

						// Left click.
						if (e.button != LEFT_CLICK)
							break;

						// Set this marker as the selected marker.
						_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_Marker;
						_currentInteraction.Index = i;
						SetMarker(i);
						SetWavePosition(_clip.Clip.length / _clip.Clip.samples * _clip.Dialogue[i].SamplePosition);
						e.Use();
						Repaint();
						break;
					}
				}
			}
		}

		/// <summary>
		/// Draws all the markers in the waveform.
		/// </summary>
		/// <param name="waveFormRect">The total space reserved for the waveform.</param>
		private void DrawTimelineLine(Rect waveFormRect)
		{
			// Calculate the timeline line based on the wave position.
			float lineX = waveFormRect.size.x / (_clip ? _clip.Clip.length : 0) * _player.WavePosition;

			Rect line = new Rect(waveFormRect.position, new Vector2(1, waveFormRect.size.y));
			line.x += lineX;
			line.x *= _zoom;
			line.x -= _scrollPos.x;
			EditorGUI.DrawRect(line, Color.white);

			// Red playing line.
			if (_player.State == AudioState.AudioState_Playing)
			{
				float playLineX = waveFormRect.size.x / (_clip ? _clip.Clip.length : 0) * AudioUtility.GetClipPosition();
				Rect playLine = new Rect(waveFormRect.position, new Vector2(1, waveFormRect.size.y));
				playLine.x += playLineX;
				playLine.x *= _zoom;
				playLine.x -= _scrollPos.x;
				EditorGUI.DrawRect(playLine, Color.red);
				Repaint();
			}
		}

		/// <summary>
		/// Draws the waveform.
		/// </summary>
		/// <param name="waveFormRect">The total space reserved for the waveform.</param>
		private void DrawWaveform(Rect waveFormRect)
		{
			// Calculate the position of the waveform itself with zoom and position taken into account.
			Rect zoomedRect = waveFormRect;
			zoomedRect.width *= _zoom;
			zoomedRect.x = -_scrollPos.x;

			// Draw the scroll view so there is an easy scroll/position change apart from hotkeys like scroll.
			Rect scrollRect = new Rect(waveFormRect.position, waveFormRect.size);
			_scrollPos = GUI.BeginScrollView(scrollRect, _scrollPos, zoomedRect, true, false, GUI.skin.horizontalScrollbar, GUIStyle.none);

			// Draw the background of the waveform and the lines.
			EditorGUI.DrawRect(waveFormRect, SubtitleEditorVariables.Preferences.Color_TimelineBackground);
			Rect outlineRect = new Rect(waveFormRect.position, new Vector3(waveFormRect.width, 1));
			for (int i = 0; i < 3; i++)
			{
				outlineRect.y += waveFormRect.height / 4;
				EditorGUI.DrawRect(outlineRect, SubtitleEditorVariables.Preferences.Color_TimelineBackline);
			}

			// Draw the waveform.
			_waveform.Draw(zoomedRect);

			DrawMarkers(waveFormRect, zoomedRect);
			DrawTimelineLine(waveFormRect);

			Event e = Event.current;

			EditorGUIUtility.AddCursorRect(waveFormRect, MouseCursor.Text);

			switch (e.type)
			{
				case EventType.MouseUp:
				{
					_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_None;
					break;
				}
				case EventType.KeyDown:
				{
					// Deleting a selected marker.
					if (e.keyCode == KeyCode.Delete && _currentDialogueItem != null)
					{
						RemoveMarker(_dialogueIndex);
						e.Use();
					}
					// Creating a new marker by hotkey.
					else if (e.keyCode == SubtitleEditorVariables.Preferences.KeyCode_Marker)
					{
						AddMarker(CalculateSamples(_player.WavePosition));
						e.Use();
					}
					break;
				}
				case EventType.MouseDown:
				case EventType.MouseDrag:
				case EventType.ScrollWheel:
				case EventType.DragPerform:
				case EventType.DragUpdated:
				{
					if (_currentInteraction.LastInteraction != InteractionType.TimelineInteraction_Time && _currentInteraction.LastInteraction != InteractionType.TimelineInteraction_None)
						break;

					if (!waveFormRect.Contains(e.mousePosition))
						break;

					// Zooming in and out.
					if (e.type == EventType.ScrollWheel)
					{
						var mPos = e.mousePosition;
						var relX = mPos.x - waveFormRect.x;

						// Zooming in.
						if (e.delta.y < 0)
						{
							if (_zoom < _zoomMax)
							{
								SetZoom(_zoom + _zoomStrength);
								_scrollPos += new Vector2(relX * _zoomStrength, 0);
							}
						}
						else
						{
							// Zooming out.
							if (_zoom > _zoomMin)
							{
								SetZoom(_zoom - _zoomStrength);
								_scrollPos -= new Vector2(relX * _zoomStrength, 0);
								if (_scrollPos.x < 0)
									_scrollPos.x = 0;
							}
						}
						e.Use();
						Repaint();
						break;
					}
					// Timeline line positioning.
					else if (e.type == EventType.MouseDown || e.type == EventType.MouseDrag)
					{
						if (e.button != LEFT_CLICK)
							break;

						_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_Time;
						_dialogueIndex = -1;

						var mPos = e.mousePosition;

						SetWavePosition(_clip.Clip.length / zoomedRect.size.x * (_scrollPos.x + mPos.x));
						e.Use();
						Repaint();
						break;
					}
					// Dragging a new audio clip in the waveform.
					else if (e.type == EventType.DragUpdated || e.type == EventType.DragPerform)
					{
						if (e.button != LEFT_CLICK)
							break;

						_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_Time;
						_dialogueIndex = -1;

						// Check if the dragged UnityObject is an audio clip.
						Object dragged_object = DragAndDrop.objectReferences[0];
						bool canBeDropped = dragged_object.GetType() == typeof(AudioClip);
						DragAndDrop.visualMode = canBeDropped ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;

						if (!canBeDropped)
							break;

						if (e.type != EventType.DragPerform)
							break;

						// Accept the drag and set the audio clip.
						DragAndDrop.AcceptDrag();

						if (dragged_object.GetType() == typeof(AudioClip))
							SetClip((AudioClip)dragged_object);

						e.Use();
						Repaint();
						break;
					}
					break;
				}
			}

			GUI.EndScrollView();

			// Draw all the objects on top.
			for (int i = 0; i < _drawList.Count; i++)
				EditorGUI.DrawRect(_drawList[i].rect, _drawList[i].color);
		}

		public override bool RequiresConstantRepaint() => true;

		/*##################
		 * Marker utilities.
		##################*/

		/// <summary>
		/// Adds a new marker on a specified position.
		/// </summary>
		/// <param name="samplePosition">The position in sample bytes.</param>
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

		/// <summary>
		/// Removes a marker.
		/// </summary>
		/// <param name="index">Index of the dialogue item/marker.</param>
		private void RemoveMarker(int index)
		{
			if (index > -1 && index < _clip.Dialogue.Count)
			{
				if (_dialogueIndex == index)
					_currentDialogueItem = null;
				_clip.Dialogue.RemoveAt(index);
				if (_dialogueIndex == index)
					_dialogueIndex = -1;
				serializedObject.Update();
				Repaint();
			}
		}

		/// <summary>
		/// Callback function for context menu.
		/// </summary>
		/// <param name="index">Index of the dialogue item/marker.</param>
		private void OnMarkerRemove(object index)
		{
			SetMarker((int)index);
			RemoveMarker((int)index);
		}

		/// <summary>
		/// Sets the current marker.
		/// </summary>
		/// <param name="index">Index of the dialogue item/marker.</param>
		private void SetMarker(int index)
		{
			_dialogueIndex = index;
			serializedObject.Update();
			_currentDialogueItem = serializedObject.FindProperty("Dialogue").GetArrayElementAtIndex(_dialogueIndex);
			Repaint();
		}

		/// <summary>
		/// Toggles the italics option.
		/// </summary>
		/// <param name="i">Index of the dialogue item.</param>
		private void SwitchItalics(object i)
		{
			int index = (int)i;
			_clip.Dialogue[index].Italics = !_clip.Dialogue[index].Italics;
			serializedObject.Update();
			Repaint();
		}

		/// <summary>
		/// Toggles the bold option.
		/// </summary>
		/// <param name="i">Index of the dialogue item.</param>
		private void SwitchBold(object i)
		{
			int index = (int)i;
			_clip.Dialogue[index].Bold = !_clip.Dialogue[index].Bold;
			serializedObject.Update();
			Repaint();
		}

		/// <summary>
		/// Toggles the clear text option.
		/// </summary>
		/// <param name="i">Index of the dialogue item.</param>
		private void SwitchClear(object i)
		{
			int index = (int)i;
			_clip.Dialogue[index].Clear = !_clip.Dialogue[index].Clear;
			serializedObject.Update();
			Repaint();
		}

		/*##################
		 * Context menus.
		##################*/

		/// <summary>
		/// Shows the marker context menu.
		/// </summary>
		/// <param name="index">Index of the dialogue item/marker.</param>
		private void ShowMarkerContextMenu(int index)
		{
			GenericMenu menu = new GenericMenu();
			menu.AddItem(new GUIContent("Delete"), false, OnMarkerRemove, index);
			menu.AddSeparator("Options/");
			menu.AddItem(new GUIContent("Options/Clear"), _clip.Dialogue[index].Clear, SwitchClear, index);
			menu.AddItem(new GUIContent("Options/Bold"), _clip.Dialogue[index].Bold, SwitchBold, index);
			menu.AddItem(new GUIContent("Options/Italics"), _clip.Dialogue[index].Italics, SwitchItalics, index);
			menu.ShowAsContext();
			Repaint();
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