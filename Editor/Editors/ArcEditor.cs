using UIShapeKit.Shapes;
using UIShapeKit.ShapeUtils;
using UnityEditor;
using UnityEditor.UI;

namespace UIShapeKit.Editor.Editors
{
	[CustomEditor(typeof(Arc))]
	[CanEditMultipleObjects]
	public class ArcEditor : GraphicEditor
    {

        private Arc _arc;
		protected SerializedProperty materialProp;
		protected SerializedProperty raycastTargetProp;

		protected SerializedProperty shapePropertiesProp;
		protected SerializedProperty ellipsePropertiesProp;
		protected SerializedProperty arcPropertiesProp;
		protected SerializedProperty lineCapProp;
		protected SerializedProperty CapRoundingPropertiesProp;
		protected SerializedProperty outlinePropertiesProp;
		protected SerializedProperty shadowPropertiesProp;
		protected SerializedProperty antiAliasingPropertiesProp;

		bool capExpanded = false;

		protected override void OnEnable()
		{
            
            _arc = (Arc)target;

			materialProp = serializedObject.FindProperty("m_Material");
			raycastTargetProp = serializedObject.FindProperty("m_RaycastTarget");

            
            shapePropertiesProp = serializedObject.FindProperty(nameof(_arc.shapeProperties));
			ellipsePropertiesProp = serializedObject.FindProperty(nameof(_arc.ellipseProperties));
			arcPropertiesProp = serializedObject.FindProperty(nameof(_arc.arcProperties));
			outlinePropertiesProp = serializedObject.FindProperty(nameof(_arc.outlineProperties));
			shadowPropertiesProp = serializedObject.FindProperty(nameof(_arc.shadowProperties));
			antiAliasingPropertiesProp = serializedObject.FindProperty(nameof(_arc.antiAliasingProperties));

			lineCapProp = serializedObject.FindProperty(nameof(_arc.lineProperties)).FindPropertyRelative("LineCap");
			CapRoundingPropertiesProp = serializedObject.FindProperty("LineProperties").FindPropertyRelative("RoundedCapResolution");
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

			EditorGUILayout.PropertyField(shapePropertiesProp, true);
			EditorGUILayout.PropertyField(ellipsePropertiesProp, true);
			EditorGUILayout.PropertyField(arcPropertiesProp, true);

			capExpanded = EditorGUILayout.Foldout(capExpanded, "Cap");
			if (capExpanded)
			{
				EditorGUILayout.PropertyField(lineCapProp);

				if (lineCapProp.enumValueIndex == (int)Lines.LineProperties.LineCapTypes.Round)
				{
					EditorGUILayout.PropertyField(CapRoundingPropertiesProp);
				}
			}

			EditorGUILayout.PropertyField(outlinePropertiesProp, true);

			EditorGUILayout.PropertyField(shadowPropertiesProp, true);
			EditorGUILayout.PropertyField(antiAliasingPropertiesProp, true);

			serializedObject.ApplyModifiedProperties();
		}
	}
}
