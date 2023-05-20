using Codice.Client.BaseCommands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;
using USubtitles;
using USubtitles.Editor;

struct Interaction
{
	public InteractionType LastInteraction;
	public int Index;
}

public enum InteractionType
{
	TimelineInteraction_None,
	TimelineInteraction_Marker,
	TimelineInteraction_Time
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

[ExecuteInEditMode]
[CustomEditor(typeof(UAudioClip), true)]
public class UAudioClipEditor : Editor
{
	private UAudioClip _clip = null;

	private const int LEFT_CLICK = 0;
	private const int RIGHT_CLICK = 1;
	private Vector2 _markerClearRectSize = new Vector2(10.0f, 15.0f);
	private Vector2 _markerTopRectSize = new Vector2(20, 15);

	private AudioPlayer _audioPlayer = new AudioPlayer();
	private PreviewRenderUtility m_PreviewUtility = null;

	private bool _multiEditing = false;

	static bool s_AutoPlay;
	static bool s_Loop;

	static private GUIContent _playIcon, _pauseIcon, _autoPlayIcon, _loopIcon, _stopIcon, _plusIcon, _minusIcon;

	private WaveformDisplay _waveform = new WaveformDisplay();

	private Interaction _currentInteraction = new Interaction();

	private int _selectedMarker = -1;

	private List<DrawRect> _drawList = new List<DrawRect>();

	private Rect _previewWindowRect, _zoomedPreviewWindowRect;

	private Material m_HandleLinesMaterial;

	/*##################
	 * Initialization
	##################*/

	private void Init()
	{
		AudioUtility.StopAllClips();

		_clip = target as UAudioClip;
		_audioPlayer.SetClip(_clip.Clip);
		_waveform.SetClip(_clip.Clip);

		var path = AssetDatabase.GetAssetPath(_clip.Clip);
		AssetImporter importer = AssetImporter.GetAtPath(path);
		_audioImporter = importer as AudioImporter;

		_autoPlayIcon = EditorGUIUtility.TrIconContent("preAudioAutoPlayOff", "Turn Auto Play on/off");
		_playIcon = EditorGUIUtility.TrIconContent("d_PlayButton", "Play");
		_loopIcon = EditorGUIUtility.TrIconContent("d_preAudioLoopOff", "Loop on/off");
		_pauseIcon = EditorGUIUtility.TrIconContent("d_PauseButton", "Pause");
		_stopIcon = EditorGUIUtility.TrIconContent("d_PreMatQuad", "Stop");
		_loopIcon = EditorGUIUtility.TrIconContent("d_preAudioLoopOff", "Loop on/off");
		_plusIcon = EditorGUIUtility.TrIconContent("d_Toolbar Plus", "Add Marker");
		_minusIcon = EditorGUIUtility.TrIconContent("d_Toolbar Minus", "Remove Marker");
	}

	private void OnEnable()
	{
		m_HandleLinesMaterial = EditorGUIUtility.LoadRequired("SceneView/HandleLines.mat") as Material;
	}

	public override bool RequiresConstantRepaint()
	{
		return true;
	}

	public void OnDisable()
	{
		_audioPlayer.SetState(AudioState.AudioState_Stopped);

		EditorPrefs.SetBool("AutoPlayAudio", s_AutoPlay);

		if (m_PreviewUtility != null)
		{
			m_PreviewUtility.Cleanup();
			m_PreviewUtility = null;
		}
		m_HandleLinesMaterial = null;
	}

	/*##################
	 * Settings
	##################*/

	public override bool HasPreviewGUI() => targets != null;

	/*##################
	 * Utilities
	##################*/

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

	/*##################
	 * Zoom
	##################*/

	private const float ZOOM_STRENGTH = 0.1f, ZOOM_MAX = 3.0f, ZOOM_MIN = 1.0f;
	private float _zoom = ZOOM_MIN;
	private Vector2 _scrollPos = Vector2.zero;

	private void HandleTimelineZoom()
	{
		Event evt = Event.current;
		switch (evt.type)
		{
			case EventType.ScrollWheel:
			{
				if (_previewWindowRect.Contains(evt.mousePosition))
				{
					// Get the relative position of the mouse position (so that it scrolls towards the mouse position)
					var relX = evt.mousePosition.x - _previewWindowRect.x;

					float initialZoom = _zoom;
					Vector2 initialScrollPos = _scrollPos;

					float strength = ZOOM_STRENGTH * (evt.delta.y < 0 ? 1 : -1);
					relX *= evt.delta.y < 0 ? 1 : -1;
					_zoom += strength;
					_scrollPos += new Vector2(relX * ZOOM_STRENGTH, 0);

					if (_zoom < ZOOM_MIN || _zoom > ZOOM_MAX)
					{
						_scrollPos = initialScrollPos;
						_zoom = initialZoom;
					}

					evt.Use();
				}
				break;
			}
		}
	}

	/*##################
	 * Markers & Marker Functions
	##################*/

	private void HandleMarkerEvents(Rect fullRect, int i)
	{
		Event evt = Event.current;
		switch (evt.type)
		{
			case EventType.MouseDrag:
			case EventType.MouseDown:
			{
				if (fullRect.Contains(evt.mousePosition) || (_currentInteraction.LastInteraction == InteractionType.TimelineInteraction_Marker && _currentInteraction.Index == i))
				{
					if (_currentInteraction.LastInteraction != InteractionType.TimelineInteraction_Marker && _currentInteraction.LastInteraction != InteractionType.TimelineInteraction_None)
						break;

					if (evt.button == LEFT_CLICK)
					{
						// Set this marker as the current marker and drag it to the new position.
						_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_Marker;
						SetMarker(i);

						float test = _clip.Clip.length / _zoomedPreviewWindowRect.size.x * (_scrollPos.x + evt.mousePosition.x);
						_clip.Dialogue[i].SamplePosition = CalculateSamples(test);
						evt.Use();
						Repaint();
					}
					else if (evt.button == RIGHT_CLICK)
					{
						// Right click.
						if (evt.button == RIGHT_CLICK)
						{
							_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_Marker;
							ShowMarkerContextMenu(i);
							evt.Use();
							Repaint();
						}
					}
				}
				break;
			}
			case EventType.KeyDown:
			{
				// Deleting a selected marker.
				if (evt.keyCode == KeyCode.Delete)
				{
					RemoveMarker(i);
					evt.Use();
				}
				break;
			}
		}
	}

	/// <summary>
	/// Draws a marker based on index.
	/// </summary>
	/// <param name="i"></param>
	private void DrawMarker(int i)
	{
		// The exact x sample position of the marker.
		float markerX = _previewWindowRect.width / (_clip ? _clip.Clip.samples : 0) * _clip.Dialogue[i].SamplePosition;

		Rect markerRect = new Rect(((_previewWindowRect.x + markerX) * _zoom) - _scrollPos.x, _previewWindowRect.y, 2, _previewWindowRect.height);

		// The top part of the marker line.
		Rect topRect = new Rect(new Vector2(markerRect.x, markerRect.y - (_markerTopRectSize.y / 2)), _markerTopRectSize);

		// The full clickable area of the marker (needed for events).
		Rect fullRect = markerRect;
		fullRect.y = topRect.y;
		fullRect.x -= _markerTopRectSize.x / 2;
		fullRect.width = _markerTopRectSize.x / 2 * 3;

		string s = _clip.Dialogue[i].Lines.Count > 0 ? _clip.Dialogue[i].Lines[0].Text : "";
		Rect labeblRect = markerRect;
		labeblRect.x += 2;
		labeblRect.y = fullRect.size.y / 2;
		labeblRect.height = EditorGUIUtility.singleLineHeight;
		GUIStyle style = new GUIStyle(GUI.skin.label);
		style.richText = true;
		if (_clip.Dialogue[i].Bold && _clip.Dialogue[i].Italic)
			style.fontStyle = FontStyle.BoldAndItalic;
		else if (_clip.Dialogue[i].Bold)
			style.fontStyle = FontStyle.Bold;
		else if (_clip.Dialogue[i].Italic)
			style.fontStyle = FontStyle.Italic;
		{
			float width = _previewWindowRect.width - labeblRect.x;
			if (_clip.Dialogue.Count >= (i + 1) + 1)
			{
				float nextMarkerX = _previewWindowRect.width / (_clip ? _clip.Clip.samples : 0) * _clip.Dialogue[i + 1].SamplePosition;
				width = nextMarkerX - labeblRect.x;
			}
			labeblRect.width = width;
			labeblRect.width -= 2;

			float actualWidth = GUI.skin.label.CalcSize(new GUIContent(s)).x;
			if (actualWidth < labeblRect.width)
				labeblRect.width = actualWidth;
		}

		HandleMarkerEvents(fullRect, i); 
		EditorGUI.DrawRect(labeblRect, new Color32(0, 0, 0, 155));
		EditorGUI.LabelField(labeblRect, new GUIContent(s), style);

		Event evt = Event.current;
		Color color = USubtitleEditorVariables.Preferences.Color_Marker;
		if (fullRect.Contains(evt.mousePosition))
			color = USubtitleEditorVariables.Preferences.Color_MarkerHover;
		if (i == _selectedMarker)
			color = USubtitleEditorVariables.Preferences.Color_MarkerSelected;

		EditorGUI.DrawRect(markerRect, color);

		// If the sample has the clear option enabled, a rect appear on top of it to signify this.
		if (_clip.Dialogue[i].Clear)
		{
			Rect clearRect = new Rect(new Vector2(markerX + (_markerClearRectSize.x / 2), _previewWindowRect.y - _markerClearRectSize.y), new Vector2(_markerClearRectSize.x, _markerClearRectSize.y));
			_drawList.Add(new DrawRect(clearRect, USubtitleEditorVariables.Preferences.Color_MarkerClear));
		}

		_drawList.Add(new DrawRect(topRect, color));
	}

	/// <summary>
	/// Draws all the markers.
	/// </summary>
	private void DrawMarkers()
	{
		for (int i = 0; i < _clip.Dialogue.Count; i++)
			DrawMarker(i);
	}

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
		menu.AddItem(new GUIContent("Options/Italic"), _clip.Dialogue[index].Italic, SwitchItalic, index);
		menu.ShowAsContext();
		Repaint();
	}

	/// <summary>
	/// Shows the marker context menu.
	/// </summary>
	/// <param name="index">Index of the dialogue item/marker.</param>
	private void ShowTimelineMarkerContextMenu()
	{
		GenericMenu menu = new GenericMenu();
		menu.AddItem(new GUIContent("Create Marker"), false, OnMarkerCreate);
		menu.ShowAsContext();
		Repaint();
	}

	private void OnMarkerCreate()
	{
		AddMarker((uint)_audioPlayer.WavePosition);
	}

	/// <summary>
	/// Adds a new marker on a specified position.
	/// </summary>
	/// <param name="samplePosition">The position in sample bytes.</param>
	public void AddMarker(uint samplePosition)
	{
		if (_clip.Dialogue.Find(x => x.SamplePosition == samplePosition) != null)
			return;

		Undo.RecordObject(_clip, "Added marker");
		DialogueItem dialogueItem = new DialogueItem();
		dialogueItem.SamplePosition = samplePosition;
		_clip.Dialogue.Add(dialogueItem);

		SetMarker(_clip.Dialogue.Count - 1);
		EditorUtility.SetDirty(target);
		Repaint();
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
	/// Removes a marker.
	/// </summary>
	/// <param name="index">Index of the dialogue item/marker.</param>
	private void RemoveMarker(int index)
	{
		if (index > -1 && index < _clip.Dialogue.Count)
		{
			Undo.RecordObject(_clip, "Removed marker");
			if (_currentInteraction.Index == index)
				_currentInteraction.Index = -1;
			_clip.Dialogue.RemoveAt(index);
			serializedObject.Update();
			EditorUtility.SetDirty(target);
			Repaint();
		}
	}

	/// <summary>
	/// Sets the current marker.
	/// </summary>
	/// <param name="index">Index of the dialogue item/marker.</param>
	private void SetMarker(int index)
	{
		_currentInteraction.Index = index;
		serializedObject.Update();
		Repaint();
	}

	/// <summary>
	/// Toggles the italics option.
	/// </summary>
	/// <param name="i">Index of the dialogue item.</param>
	private void SwitchItalic(object i)
	{
		Undo.RecordObject(_clip, "Removed marker");
		int index = (int)i;
		_clip.Dialogue[index].Italic = !_clip.Dialogue[index].Italic;
		serializedObject.Update();
		EditorUtility.SetDirty(target);
		Repaint();
	}

	/// <summary>
	/// Toggles the bold option.
	/// </summary>
	/// <param name="i">Index of the dialogue item.</param>
	private void SwitchBold(object i)
	{
		Undo.RecordObject(_clip, "Removed marker");
		int index = (int)i;
		_clip.Dialogue[index].Bold = !_clip.Dialogue[index].Bold;
		serializedObject.Update();
		EditorUtility.SetDirty(target);
		Repaint();
	}

	/// <summary>
	/// Toggles the clear text option.
	/// </summary>
	/// <param name="i">Index of the dialogue item.</param>
	private void SwitchClear(object i)
	{
		Undo.RecordObject(_clip, "Removed marker");
		int index = (int)i;
		_clip.Dialogue[index].Clear = !_clip.Dialogue[index].Clear;
		serializedObject.Update();
		EditorUtility.SetDirty(target);
		Repaint();
	}

	private AudioImporter _audioImporter = null;
	public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
	{
		if (_audioImporter == null)
			Init();

		if (!ShaderUtil.hardwareSupportsRectRenderTexture)
			return null;

		if (m_PreviewUtility == null)
			m_PreviewUtility = new PreviewRenderUtility();

		m_PreviewUtility.BeginStaticPreview(new Rect(0, 0, width, height));
		m_HandleLinesMaterial.SetPass(0);

		_waveform.Draw(new Rect(0.05f * width * EditorGUIUtility.pixelsPerPoint, 0.05f * width * EditorGUIUtility.pixelsPerPoint, 1.9f * width * EditorGUIUtility.pixelsPerPoint, 1.9f * height * EditorGUIUtility.pixelsPerPoint), true);
		return m_PreviewUtility.EndStaticPreview();
	}

	public override void OnPreviewGUI(Rect re, GUIStyle background)
	{
		if (!_clip || !_clip.Clip || _clip.Clip != _waveform.Clip)
			Init();

		_previewWindowRect = re;

		_zoomedPreviewWindowRect = _previewWindowRect;
		_zoomedPreviewWindowRect.width *= _zoom;
		_zoomedPreviewWindowRect.x = -_scrollPos.x;

		_drawList.Clear();

		_audioPlayer.Update();

		HandleTimelineZoom();

		if (Event.current.type == EventType.Repaint)
			background.Draw(_previewWindowRect, false, false, false, false);

		int c = AudioUtility.GetChannelCount(_clip.Clip);

		Event evt = Event.current;

		// Handling whether the waveform can be previewed.
		bool previewAble = AudioUtility.HasPreview(_clip.Clip) || !(AudioUtility.IsTrackerFile(_clip.Clip));
		if (!previewAble)
		{
			float labelY = (_previewWindowRect.height > 150) ? _previewWindowRect.y + (_previewWindowRect.height / 2) - 10 : _previewWindowRect.y + (_previewWindowRect.height / 2) - 25;
			if (_previewWindowRect.width > 64)
			{
				if (AudioUtility.IsTrackerFile(_clip.Clip))
					EditorGUI.DropShadowLabel(new Rect(_previewWindowRect.x, labelY, _previewWindowRect.width, 20), string.Format("Module file with " + AudioUtility.GetChannelCount(_clip.Clip) + " channels."));
				else
					EditorGUI.DropShadowLabel(new Rect(_previewWindowRect.x, labelY, _previewWindowRect.width, 20), "Can not show PCM data for this file");
			}

			if (_audioPlayer.State == AudioState.AudioState_Playing)
			{
				float t = AudioUtility.GetClipPosition();

				TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)(t * 1000.0f));

				EditorGUI.DropShadowLabel(new Rect(_previewWindowRect.x, _previewWindowRect.y, _previewWindowRect.width, 20), string.Format("Playing - {0:00}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds));
			}
		}
		else
		{
			_waveform ??= new WaveformDisplay();
			if (!_waveform.Clip)
				_waveform.SetClip(_clip.Clip);

			Rect scrollRect = _previewWindowRect;
			// Scrollbar around waveform.
			_scrollPos = GUI.BeginScrollView(scrollRect, _scrollPos, _zoomedPreviewWindowRect, true, false, GUI.skin.horizontalScrollbar, GUIStyle.none);
			{
				_waveform.Draw(_zoomedPreviewWindowRect);

				// Waveform lines.
				Rect rect_waveMarker = new Rect(_previewWindowRect.x, _previewWindowRect.y, 2, _previewWindowRect.height);
				float markerLineX = _previewWindowRect.width / (_clip ? _clip.Clip.samples : 0) * _audioPlayer.WavePosition;
				float playLineX = _previewWindowRect.width / (_clip ? _clip.Clip.length : 0) * AudioUtility.GetClipPosition();
				Rect rect_timeLineMarker = new Rect(((rect_waveMarker.x + markerLineX) * _zoom) - _scrollPos.x, rect_waveMarker.y, rect_waveMarker.width, rect_waveMarker.height);
				EditorGUI.DrawRect(rect_timeLineMarker, Color.red);
				EditorGUI.DrawRect(new Rect(((rect_waveMarker.x + playLineX) * _zoom) - _scrollPos.x, rect_waveMarker.y, rect_waveMarker.width, rect_waveMarker.height), Color.white);

				Rect fullRect = rect_timeLineMarker;
				fullRect.x -= 6;
				fullRect.width = 12;

				if (evt.type == EventType.MouseDown && evt.button == RIGHT_CLICK)
					if (fullRect.Contains(evt.mousePosition))
						ShowTimelineMarkerContextMenu();

				DrawMarkers();
			}
			GUI.EndScrollView();

			// Channel text.
			for (int i = 0; i < c; ++i)
				if (c > 1 && _previewWindowRect.width > 64)
				{
					var labelRect = new Rect(_previewWindowRect.x + 5, _previewWindowRect.y + (_previewWindowRect.height / c * i), 30, 20);
					EditorGUI.DropShadowLabel(labelRect, "ch " + (i + 1));
				}

			// Time text.
			float t = AudioUtility.GetClipPosition();
			TimeSpan ts = new TimeSpan(0, 0, 0, 0, (int)(t * 1000.0f));
			if (_previewWindowRect.width > 64)
				EditorGUI.DropShadowLabel(new Rect(_previewWindowRect.x, _previewWindowRect.y, _previewWindowRect.width, 20), string.Format("{0:00}:{1:00}.{2:000}", ts.Minutes, ts.Seconds, ts.Milliseconds));
			else
				EditorGUI.DropShadowLabel(new Rect(_previewWindowRect.x, _previewWindowRect.y, _previewWindowRect.width, 20), string.Format("{0:00}:{1:00}", ts.Minutes, ts.Seconds));

			switch (evt.type)
			{
				case EventType.MouseDrag:
				case EventType.MouseDown:
				{
					if (evt.button == LEFT_CLICK)
					{
						if (_previewWindowRect.Contains(evt.mousePosition))
						{
							if (_currentInteraction.LastInteraction != InteractionType.TimelineInteraction_Time && _currentInteraction.LastInteraction != InteractionType.TimelineInteraction_None)
								break;

							_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_Time;

							float startSample = _clip.Clip.samples / _zoomedPreviewWindowRect.width * (_scrollPos.x + evt.mousePosition.x);
							if (_audioPlayer.State != AudioState.AudioState_Playing || _clip.Clip != _audioPlayer.Clip)
							{
								_audioPlayer.SetPosition(startSample);
								if (s_AutoPlay)
									_audioPlayer.SetState(AudioState.AudioState_Playing, startSample);
							}
							else
								_audioPlayer.SetPosition(startSample);
							Repaint();
							evt.Use();
						}
					}
					break;
				}
				case EventType.MouseUp:
				{
					if (_currentInteraction.LastInteraction != InteractionType.TimelineInteraction_None && _currentInteraction.LastInteraction != InteractionType.TimelineInteraction_Time)
						EditorUtility.SetDirty(target);
					_currentInteraction.LastInteraction = InteractionType.TimelineInteraction_None;
					break;
				}
			}

			// Draw all the objects on top.
			for (int i = 0; i < _drawList.Count; i++)
				EditorGUI.DrawRect(_drawList[i].rect, _drawList[i].color);

			if (_audioPlayer.State == AudioState.AudioState_Playing)
				Repaint();
		}
	}

	public override void OnPreviewSettings()
	{
		_multiEditing = targets.Length > 1;

		{
			using (new EditorGUI.DisabledScope(_multiEditing && _audioPlayer.State == AudioState.AudioState_Playing))
			{
				if (GUILayout.Button(_stopIcon, EditorStyles.toolbarButton))
					_audioPlayer.SetState(AudioState.AudioState_Stopped);
			}

			using (new EditorGUI.DisabledScope(_multiEditing && _audioPlayer.State != AudioState.AudioState_Playing))
			{
				if (GUILayout.Button(_audioPlayer.State == AudioState.AudioState_Playing ? _pauseIcon : _playIcon, EditorStyles.toolbarButton))
				{
					if (_audioPlayer.State != AudioState.AudioState_Playing)
						_audioPlayer.SetState(AudioState.AudioState_Playing);
					else
						_audioPlayer.SetState(AudioState.AudioState_Paused);
				}
			}

			bool loop = s_Loop;
			s_Loop = GUILayout.Toggle(s_Loop, _loopIcon, EditorStyles.toolbarButton);
			s_AutoPlay = GUILayout.Toggle(s_AutoPlay, _autoPlayIcon, EditorStyles.toolbarButton);
			if ((loop != s_Loop) && _audioPlayer.State == AudioState.AudioState_Playing)
				AudioUtility.LoopClip(_audioPlayer.Clip, s_Loop);

			using (new EditorGUI.DisabledScope(_multiEditing))
			{
				if (GUILayout.Button(_plusIcon, EditorStyles.toolbarButton))
					AddMarker((uint)_audioPlayer.WavePosition);
				if (GUILayout.Button(_minusIcon, EditorStyles.toolbarButton))
					RemoveMarker(_currentInteraction.Index);
			}
		}
	}

	[MenuItem("Assets/Create/UAudioClip", priority = 1)]
	private static void CreateUAudioClipFromAudioClip()
	{
		for (int i = 0; i < Selection.objects.Length; i++)
		{
            UnityEngine.Object obj = Selection.objects[i];

			UAudioClip uAudioClip = ScriptableObject.CreateInstance<UAudioClip>();
			uAudioClip.Clip = obj as AudioClip;
			string path = AssetDatabase.GetAssetPath(obj);
			int fileExtPos = path.LastIndexOf(".");
			if (fileExtPos >= 0)
				path = path[..fileExtPos];
			string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + ".asset");

			AssetDatabase.CreateAsset(uAudioClip, assetPathAndName);

			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();
			EditorUtility.FocusProjectWindow();
		}
	}

	[MenuItem("Assets/Create/UAudioClip", true)]
	private static bool CreateUAudioClipFromAudioClipValidation() => Selection.activeObject is AudioClip;
}
