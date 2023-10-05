using UIShapeKit.Shapes;
using UnityEditor;
using UnityEditor.UI;

namespace UIShapeKit.Editor.Editors
{
    [CustomEditor(typeof(Rectangle))]
    [CanEditMultipleObjects]
    public class RectangleEditor : GraphicEditor
    {
        private Rectangle _rectangle;

        protected SerializedProperty MaterialProp;
        protected SerializedProperty SpriteProp;
        protected SerializedProperty RaycastTargetProp;

        protected SerializedProperty ShapePropertiesProp;
        protected SerializedProperty RoundedPropertiesProp;
        protected SerializedProperty OutlinePropertiesProp;
        protected SerializedProperty ShadowPropertiesProp;
        protected SerializedProperty AntiAliasingPropertiesProp;

        protected override void OnEnable()
        {
            _rectangle = (Rectangle)target;

            MaterialProp = serializedObject.FindProperty("m_Material");
            SpriteProp = serializedObject.FindProperty("Sprite");
            RaycastTargetProp = serializedObject.FindProperty("m_RaycastTarget");

            ShapePropertiesProp = serializedObject.FindProperty(nameof(_rectangle.shapeProperties));
            RoundedPropertiesProp = serializedObject.FindProperty(nameof(_rectangle.roundedProperties));
            OutlinePropertiesProp = serializedObject.FindProperty(nameof(_rectangle.outlineProperties));
            ShadowPropertiesProp = serializedObject.FindProperty(nameof(_rectangle.shadowProperties));
            AntiAliasingPropertiesProp = serializedObject.FindProperty(nameof(_rectangle.antiAliasingProperties));
        }

        protected override void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(MaterialProp);
            EditorGUILayout.PropertyField(SpriteProp);
            EditorGUILayout.PropertyField(RaycastTargetProp);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ShapePropertiesProp, true);
            EditorGUILayout.PropertyField(RoundedPropertiesProp, true);

            if (_rectangle.shapeProperties.DrawOutline)
            {
                EditorGUILayout.PropertyField(OutlinePropertiesProp, true);
            }

            EditorGUILayout.PropertyField(ShadowPropertiesProp, true);
            EditorGUILayout.PropertyField(AntiAliasingPropertiesProp, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
