using UIShapeKit.Editor.CustomDrawers;
using UIShapeKit.Shapes;
using UIShapeKit.ShapeUtils;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace UIShapeKit.Editor.Editors
{
    [CustomEditor(typeof(Polygon))]
    public class PolygonEditor : GraphicEditor
    {
        private Polygon _polygon;
        private PointsList.PointListsProperties _pointListsProperties;
        private RectTransform _rectTransform;

        protected SerializedProperty materialProp;
        protected SerializedProperty raycastTargetProp;

        protected SerializedProperty shapePropertiesProp;
        protected SerializedProperty pointListPropertiesProp;
        protected SerializedProperty polygonPropertiesProp;
        protected SerializedProperty shadowPropertiesProp;
        protected SerializedProperty antiAliasingPropertiesProp;

        protected override void OnEnable()
        {
            _polygon = (Polygon)target;

            _rectTransform = _polygon.rectTransform;
            _pointListsProperties = _polygon.pointListsProperties;

            materialProp = serializedObject.FindProperty("m_Material");
            raycastTargetProp = serializedObject.FindProperty("m_RaycastTarget");

            shapePropertiesProp = serializedObject.FindProperty(nameof(_polygon.shapeProperties));
            pointListPropertiesProp = serializedObject.FindProperty(nameof(_polygon.pointListsProperties));
            polygonPropertiesProp = serializedObject.FindProperty(nameof(_polygon.polygonProperties));
            shadowPropertiesProp = serializedObject.FindProperty(nameof(_polygon.shadowProperties));
            antiAliasingPropertiesProp = serializedObject.FindProperty(nameof(_polygon.antiAliasingProperties));
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
            EditorGUILayout.PropertyField(pointListPropertiesProp, true);
            EditorGUILayout.PropertyField(polygonPropertiesProp, true);

            EditorGUILayout.PropertyField(shadowPropertiesProp, true);
            EditorGUILayout.PropertyField(antiAliasingPropertiesProp, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            Undo.RecordObject(_polygon, "test");

            for (int i = 0; i < _pointListsProperties.PointListProperties.Length; i++)
            {
                if (
                    _pointListsProperties.PointListProperties[i].ShowHandles &&
                    _pointListsProperties.PointListProperties[i].GeneratorData.Generator ==
                    PointsList.PointListGeneratorData.Generators.Custom
                )
                {
                    if (PointListDrawer.Draw(
                            ref _pointListsProperties.PointListProperties[i].Positions,
                            _rectTransform,
                            true,
                            3
                        ))
                        _polygon.ForceMeshUpdate();
                }
            }
        }
    }
}
