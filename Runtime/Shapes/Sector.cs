using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
    [AddComponentMenu("UI/Shapes/Sector", 50), RequireComponent(typeof(CanvasRenderer))]
    public class Sector : MaskableGraphic, IShape
    {
        [SerializeField] public GeoUtils.ShapeProperties shapeProperties = new();
        [SerializeField] public ShapeUtils.Ellipses.EllipseProperties ellipseProperties = new();
        [SerializeField] public ShapeUtils.Arcs.ArcProperties arcProperties = new();
        [SerializeField] public GeoUtils.ShadowsProperties shadowProperties = new();
        [SerializeField] public GeoUtils.AntiAliasingProperties antiAliasingProperties = new();

        private GeoUtils.UnitPositionData _unitPositionData;
        private GeoUtils.EdgeGradientData _edgeGradientData;
        private Vector2 _radius = Vector2.one;

        private Rect _pixelRect;

        public void ForceMeshUpdate()
        {
            SetVerticesDirty();
            SetMaterialDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            ellipseProperties.OnCheck();
            antiAliasingProperties.OnCheck();

            ForceMeshUpdate();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            _pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);

            ShapeUtils.Ellipses.SetRadius(
                ref _radius,
                _pixelRect.width,
                _pixelRect.height,
                ellipseProperties
            );

            ellipseProperties.UpdateAdjusted(_radius, 0.0f);
            arcProperties.UpdateAdjusted(ellipseProperties.AdjustedResolution, ellipseProperties.BaseAngle);
            antiAliasingProperties.UpdateAdjusted(canvas);
            shadowProperties.UpdateAdjusted();

            // shadows
            if (shadowProperties.ShadowsEnabled)
            {
                for (int i = 0; i < shadowProperties.Shadows.Length; i++)
                {
                    _edgeGradientData.SetActiveData(
                        1.0f - shadowProperties.Shadows[i].Softness,
                        shadowProperties.Shadows[i].Size,
                        antiAliasingProperties.Adjusted
                    );

                    ShapeUtils.Arcs.AddSegment(
                        ref vh,
                        shadowProperties.GetCenterOffset(_pixelRect.center, i),
                        _radius,
                        ellipseProperties,
                        arcProperties,
                        shadowProperties.Shadows[i].Color,
                        GeoUtils.ZeroV2,
                        ref _unitPositionData,
                        _edgeGradientData
                    );
                }
            }

            // fill
            if (shadowProperties.ShowShape)
            {
                if (antiAliasingProperties.Adjusted > 0.0f)
                {
                    _edgeGradientData.SetActiveData(
                        1.0f,
                        0.0f,
                        antiAliasingProperties.Adjusted
                    );
                }
                else
                {
                    _edgeGradientData.Reset();
                }

                ShapeUtils.Arcs.AddSegment(
                    ref vh,
                    (Vector3)_pixelRect.center,
                    _radius,
                    ellipseProperties,
                    arcProperties,
                    shapeProperties.FillColor,
                    GeoUtils.ZeroV2,
                    ref _unitPositionData,
                    _edgeGradientData
                );
            }
        }
    }
}
