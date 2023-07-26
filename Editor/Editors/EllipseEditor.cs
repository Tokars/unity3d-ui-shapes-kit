using UnityEditor;
using UnityEditor.UI;
using Ellipse = UIShapeKit.Shapes.Ellipse;

namespace UIShapeKit.Editor.Editors
{
    [CustomEditor(typeof(Ellipse))]
    [CanEditMultipleObjects]
    public class EllipseEditor : GraphicEditor
    {
        Ellipse ellipse;

        protected SerializedProperty MaterialProp;
        protected SerializedProperty SpriteProp;
        protected SerializedProperty RaycastTargetProp;

        protected SerializedProperty ShapePropertiesProp;
        protected SerializedProperty EllipsePropertiesProp;
        protected SerializedProperty OutlinePropertiesProp;
        protected SerializedProperty ShadowPropertiesProp;
        protected SerializedProperty AntiAliasingPropertiesProp;

        protected override void OnEnable()
        {
            ellipse = (Ellipse)target;

            MaterialProp = serializedObject.FindProperty("m_Material");
            SpriteProp = serializedObject.FindProperty("Sprite");
            RaycastTargetProp = serializedObject.FindProperty("m_RaycastTarget");

            ShapePropertiesProp = serializedObject.FindProperty(nameof(ellipse.shapeProperties));
            EllipsePropertiesProp = serializedObject.FindProperty(nameof(ellipse.ellipseProperties));
            OutlinePropertiesProp = serializedObject.FindProperty(nameof(ellipse.outlineProperties));
            ShadowPropertiesProp = serializedObject.FindProperty(nameof(ellipse.shadowProperties));
            AntiAliasingPropertiesProp = serializedObject.FindProperty(nameof(ellipse.antiAliasingProperties));
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
            EditorGUILayout.PropertyField(EllipsePropertiesProp, true);

            if (ellipse.shapeProperties.DrawOutline)
            {
                EditorGUILayout.PropertyField(OutlinePropertiesProp, true);
            }

            EditorGUILayout.PropertyField(ShadowPropertiesProp, true);
            EditorGUILayout.PropertyField(AntiAliasingPropertiesProp, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
