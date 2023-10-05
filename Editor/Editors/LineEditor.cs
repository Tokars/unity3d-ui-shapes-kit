using UIShapeKit.Editor.CustomDrawers;
using UIShapeKit.Shapes;
using UIShapeKit.ShapeUtils;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

namespace UIShapeKit.Editor.Editors
{
    [CustomEditor(typeof(Line))]
    [CanEditMultipleObjects]
    public class LineEditor : GraphicEditor
    {
        private Line _linearLine;

        private PointsList.PointListsProperties _pointListsProperties;
        private RectTransform _rectTransform;

        protected SerializedProperty materialProp;
        protected SerializedProperty spriteProp;
        protected SerializedProperty raycastTargetProp;

        protected SerializedProperty shapePropertiesProp;
        protected SerializedProperty pointListPropertiesProp;
        protected SerializedProperty linePropertiesPropertiesProp;
        protected SerializedProperty outlinePropertiesProp;
        protected SerializedProperty shadowPropertiesProp;
        protected SerializedProperty antiAliasingPropertiesProp;

        protected override void OnEnable()
        {
            _linearLine = (Line)target;

            _rectTransform = _linearLine.rectTransform;
            _pointListsProperties = _linearLine.pointListsProperties;

            materialProp = serializedObject.FindProperty("m_Material");
            spriteProp = serializedObject.FindProperty("Sprite");
            raycastTargetProp = serializedObject.FindProperty("m_RaycastTarget");

            shapePropertiesProp = serializedObject.FindProperty(nameof(_linearLine.shapeProperties));
            pointListPropertiesProp = serializedObject.FindProperty(nameof(_linearLine.pointListsProperties));
            linePropertiesPropertiesProp = serializedObject.FindProperty(nameof(_linearLine.lineProperties));
            outlinePropertiesProp = serializedObject.FindProperty(nameof(_linearLine.outlineProperties));
            shadowPropertiesProp = serializedObject.FindProperty(nameof(_linearLine.shadowProperties));
            antiAliasingPropertiesProp = serializedObject.FindProperty(nameof(_linearLine.antiAliasingProperties));
        }

        protected override void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(materialProp);
            EditorGUILayout.PropertyField(spriteProp);
            EditorGUILayout.PropertyField(raycastTargetProp);

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(shapePropertiesProp, true);
            EditorGUILayout.PropertyField(pointListPropertiesProp, true);
            EditorGUILayout.PropertyField(linePropertiesPropertiesProp, true);
            EditorGUILayout.PropertyField(outlinePropertiesProp, true);

            EditorGUILayout.PropertyField(shadowPropertiesProp, true);
            EditorGUILayout.PropertyField(antiAliasingPropertiesProp, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnSceneGUI()
        {
            Undo.RecordObject(_linearLine, "LinarLine");

            for (int i = 0; i < _pointListsProperties.pointListProperties.Length; i++)
            {
                if (
                    _pointListsProperties.pointListProperties[i].showHandles &&
                    _pointListsProperties.pointListProperties[i].generatorData.generator ==
                    PointsList.PointListGeneratorData.Generators.Custom
                )
                {
                    if (PointListDrawer.Draw(
                            ref _pointListsProperties.pointListProperties[i].positions,
                            _rectTransform,
                            _linearLine.lineProperties.closed,
                            2
                        ))
                        _linearLine.ForceMeshUpdate();
                }
            }
        }
    }
}
