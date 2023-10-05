using UIShapeKit.Shapes;
using UnityEditor;
using UnityEditor.UI;

namespace UIShapeKit.Editor.Editors
{
    [CustomEditor(typeof(Sector))]
    [CanEditMultipleObjects]
    public class SectorEditor : GraphicEditor
    {
        protected SerializedProperty MaterialProp;
        protected SerializedProperty RaycastTargetProp;

        protected SerializedProperty ShapePropertiesProp;
        protected SerializedProperty EllipsePropertiesProp;
        protected SerializedProperty ArcPropertiesProp;
        protected SerializedProperty ShadowPropertiesProp;
        protected SerializedProperty AntiAliasingPropertiesProp;

        private Sector _sector;

        protected override void OnEnable()
        {
            _sector = (Sector)target;

            MaterialProp = serializedObject.FindProperty("m_Material");
            RaycastTargetProp = serializedObject.FindProperty("m_RaycastTarget");

            ShapePropertiesProp = serializedObject.FindProperty(nameof(_sector.shapeProperties));
            EllipsePropertiesProp = serializedObject.FindProperty(nameof(_sector.ellipseProperties));
            ArcPropertiesProp = serializedObject.FindProperty(nameof(_sector.arcProperties));
            ShadowPropertiesProp = serializedObject.FindProperty(nameof(_sector.shadowProperties));
            AntiAliasingPropertiesProp = serializedObject.FindProperty(nameof(_sector.antiAliasingProperties));
        }

        protected override void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(MaterialProp);
            EditorGUILayout.PropertyField(RaycastTargetProp);
            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(ShapePropertiesProp, true);
            EditorGUILayout.PropertyField(EllipsePropertiesProp, true);
            EditorGUILayout.PropertyField(ArcPropertiesProp, true);

            EditorGUILayout.PropertyField(ShadowPropertiesProp, true);
            EditorGUILayout.PropertyField(AntiAliasingPropertiesProp, true);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
