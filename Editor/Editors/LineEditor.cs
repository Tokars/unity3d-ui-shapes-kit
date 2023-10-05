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

        PointsList.PointListsProperties pointListsProperties;
        RectTransform rectTransform;

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

            rectTransform = _linearLine.rectTransform;
            pointListsProperties = _linearLine.pointListsProperties;

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

        void OnSceneGUI()
        {
            Undo.RecordObject(_linearLine, "LinarLine");

            for (int i = 0; i < pointListsProperties.PointListProperties.Length; i++)
            {
                if (
                    pointListsProperties.PointListProperties[i].ShowHandles &&
                    pointListsProperties.PointListProperties[i].GeneratorData.Generator ==
                    PointsList.PointListGeneratorData.Generators.Custom
                )
                {
                    if (PointListDrawer.Draw(
                            ref pointListsProperties.PointListProperties[i].Positions,
                            rectTransform,
                            _linearLine.lineProperties.Closed,
                            2
                        ))
                        _linearLine.ForceMeshUpdate();
                }
            }


            // if (!Application.isPlaying && linearLine.enabled)
            // {
            // 	linearLine.enabled = false;
            // 	linearLine.enabled = true;
            // }
        }
    }
}
