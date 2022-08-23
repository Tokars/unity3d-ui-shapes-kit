using ThisOtherThing.UI;
using ThisOtherThing.UI.ShapeUtils;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [CustomEditor(typeof(AntiAliasingGraphic))]
    public class E_AntiAliasingGraphic : GraphicEditor
    {
        private AntiAliasingGraphic _aag;

        private SerializedProperty _antiAliasingPropertiesProp;
        private SerializedProperty _compound;
        private SerializedProperty edgeGradientData;

        protected override void OnEnable()
        {
            _aag = (AntiAliasingGraphic)target;

            _antiAliasingPropertiesProp = serializedObject.FindProperty("AntiAliasingProperties");
            _compound = serializedObject.FindProperty("compoundGraphic");
        }

        protected override void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_compound, true);
            EditorGUILayout.PropertyField(_antiAliasingPropertiesProp, true);
            // todo: on change.
            _aag.CompoundRefresh();
            serializedObject.ApplyModifiedProperties();
        }
    }

    // [ExecuteInEditMode]
    public class AntiAliasingGraphic : GraphicComposite
    {
        public GeoUtils.AntiAliasingProperties AntiAliasingProperties =
            new GeoUtils.AntiAliasingProperties();


        GeoUtils.EdgeGradientData edgeGradientData;


        private RoundedRects.RoundedProperties _rr;
        RoundedRects.RoundedCornerUnitPositionData _unitPositionData;

#if UNITY_EDITOR

        public override void CompoundRefresh()
        {
            compoundGraphic.Refresh();
        }
#endif

        public override void PopulateMesh(ref VertexHelper vh)
        {
            Rect pixelRect =
                RectTransformUtility.PixelAdjustRect(compoundGraphic.rectTransform, compoundGraphic.canvas);
            AntiAliasingProperties.UpdateAdjusted(compoundGraphic.canvas);

            if (AntiAliasingProperties.Adjusted > 0.0f)
            {
                edgeGradientData.SetActiveData(
                    1.0f,
                    0.0f,
                    AntiAliasingProperties.Adjusted
                );
            }
            else
                edgeGradientData.Reset();

            RoundedRects.AddRoundedRect(
                ref vh,
                pixelRect.center,
                pixelRect.width,
                pixelRect.height,
                _rr,
                compoundGraphic.color,
                GeoUtils.ZeroV2,
                ref _unitPositionData,
                edgeGradientData
            );
        }
    }
}