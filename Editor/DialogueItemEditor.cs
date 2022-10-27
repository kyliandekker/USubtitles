using UnityEditor;
using UnityEngine;

namespace UAudio.USubtitles.Editor
{
	[CustomPropertyDrawer(typeof(Line))]
	public class LineDrawer : PropertyDrawer
	{
		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			var amountRect = new Rect(position.x, position.y, position.width / 3, position.height);
			var unitRect = new Rect(position.x + (position.width / 3), position.y, (position.width / 3) * 2, position.height);

			_ = EditorGUI.PropertyField(amountRect, property.FindPropertyRelative("Language"), GUIContent.none);
			_ = EditorGUI.PropertyField(unitRect, property.FindPropertyRelative("Text"), GUIContent.none);
		}
	}

	[CustomPropertyDrawer(typeof(DialogueItem))]
	public class DialogueItemDrawer : PropertyDrawer
	{
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
		{
			float height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("SamplePosition"));
			height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Lines"));
			height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("UseColor"));
			height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Bold"));
			height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Italics"));
			height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Clear"));
			height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Color"));
			return height;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
		{
			EditorGUI.BeginProperty(position, label, property);

			bool useColor = property.FindPropertyRelative("UseColor").boolValue;
			var temp = GUI.enabled;
			GUI.enabled = false;

			float height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("SamplePosition"));
			position.height = height;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("SamplePosition"));
			position.y += height;

			GUI.enabled = temp;

			height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Lines"));
			position.height = height;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("Lines"));
			position.y += height;

			height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("UseColor"));
			position.height = height;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("UseColor"));
			position.y += height;

			height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Bold"));
			position.height = height;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("Bold"));
			position.y += height;

			height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Italics"));
			position.height = height;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("Italics"));
			position.y += height;

			height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Clear"));
			position.height = height;
			EditorGUI.PropertyField(position, property.FindPropertyRelative("Clear"));
			position.y += height;

			if (useColor)
			{
				height = EditorGUI.GetPropertyHeight(property.FindPropertyRelative("Color"));
				position.height = height;
				EditorGUI.PropertyField(position, property.FindPropertyRelative("Color"));
				position.y += height;
			}

			EditorGUI.EndProperty();
		}
	}
}