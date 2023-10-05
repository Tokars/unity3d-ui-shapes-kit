using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using EdgeGradient = UIShapeKit.Shapes.EdgeGradient;

namespace UIShapeKit.Editor.Editors
{
	[CustomEditor(typeof(EdgeGradient))]
	[CanEditMultipleObjects]
	public class EdgeGradientEditor : GraphicEditor
    {
        private EdgeGradient _edgeGradient;
		protected SerializedProperty materialProp;
		protected SerializedProperty raycastTargetProp;

		protected SerializedProperty propertiesProp;

		protected override void OnEnable()
        {
            _edgeGradient = (EdgeGradient)target;
			materialProp = serializedObject.FindProperty("m_Material");
			raycastTargetProp = serializedObject.FindProperty("m_RaycastTarget");

			propertiesProp = serializedObject.FindProperty(nameof(_edgeGradient.properties));
		}

		protected override void OnDisable()
		{
			Tools.hidden = false;
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.PropertyField(materialProp);
			EditorGUILayout.PropertyField(raycastTargetProp);
			EditorGUILayout.Space();

			EditorGUILayout.PropertyField(propertiesProp, new GUIContent("Edges"), true);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
