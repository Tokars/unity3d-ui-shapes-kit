using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
    [AddComponentMenu("UI/Shapes/Arc", 50), RequireComponent(typeof(CanvasRenderer))]
    public class Arc : MaskableGraphic, IShape
    {
        [SerializeField] public GeoUtils.ShapeProperties shapeProperties = new();
        [SerializeField] public ShapeUtils.Ellipses.EllipseProperties ellipseProperties = new();
        [SerializeField] public ShapeUtils.Arcs.ArcProperties arcProperties = new();
        [SerializeField] public ShapeUtils.Lines.LineProperties lineProperties = new();
        [SerializeField] public ShapeUtils.PointsList.PointListProperties pointListProperties = new();
        [SerializeField]public GeoUtils.OutlineProperties outlineProperties = new();
        [SerializeField]public GeoUtils.ShadowsProperties shadowProperties = new();
        [SerializeField]public GeoUtils.AntiAliasingProperties antiAliasingProperties = new();


        private ShapeUtils.PointsList.PointsData _pointsData = new();
        private GeoUtils.UnitPositionData _unitPositionData;
        private GeoUtils.EdgeGradientData _edgeGradientData;
        private Vector2 _radius = Vector2.one;

        protected override void OnEnable()
        {
            pointListProperties.GeneratorData.Generator =
                ShapeUtils.PointsList.PointListGeneratorData.Generators.Round;

            pointListProperties.GeneratorData.Center.x = 0.0f;
            pointListProperties.GeneratorData.Center.y = 0.0f;

            base.OnEnable();
        }

        public void ForceMeshUpdate()
        {
            pointListProperties.GeneratorData.NeedsUpdate = true;
            _pointsData.NeedsUpdate = true;

            SetVerticesDirty();
            SetMaterialDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            ellipseProperties.OnCheck();
            outlineProperties.OnCheck();
            antiAliasingProperties.OnCheck();

            ForceMeshUpdate();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);

            outlineProperties.UpdateAdjusted();
            shadowProperties.UpdateAdjusted();

            ShapeUtils.Ellipses.SetRadius(
                ref _radius,
                pixelRect.width,
                pixelRect.height,
                ellipseProperties
            );

            pointListProperties.GeneratorData.Width = _radius.x * 2.0f;
            pointListProperties.GeneratorData.Height = _radius.y * 2.0f;

            ellipseProperties.UpdateAdjusted(_radius, outlineProperties.GetOuterDistace());
            arcProperties.UpdateAdjusted(ellipseProperties.AdjustedResolution, ellipseProperties.BaseAngle);
            antiAliasingProperties.UpdateAdjusted(canvas);

            pointListProperties.GeneratorData.Resolution = ellipseProperties.AdjustedResolution * 2;
            pointListProperties.GeneratorData.Length = arcProperties.Length;

            switch (arcProperties.Direction)
            {
                case ShapeUtils.Arcs.ArcProperties.ArcDirection.Forward:
                    pointListProperties.GeneratorData.Direction = 1.0f;
                    pointListProperties.GeneratorData.FloatStartOffset = ellipseProperties.BaseAngle * 0.5f;
                    break;

                case ShapeUtils.Arcs.ArcProperties.ArcDirection.Centered:
                    pointListProperties.GeneratorData.Direction = -1.0f;
                    pointListProperties.GeneratorData.FloatStartOffset =
                        ellipseProperties.BaseAngle * 0.5f + (arcProperties.Length * 0.5f);
                    break;

                case ShapeUtils.Arcs.ArcProperties.ArcDirection.Backward:
                    pointListProperties.GeneratorData.Direction = -1.0f;
                    pointListProperties.GeneratorData.FloatStartOffset = ellipseProperties.BaseAngle * 0.5f;
                    break;
            }

            // shadows
            if (shadowProperties.ShadowsEnabled)
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

                // use segment if LineWeight is overshooting the center
                if (
                    (
                        outlineProperties.Type == GeoUtils.OutlineProperties.LineType.Center ||
                        outlineProperties.Type == GeoUtils.OutlineProperties.LineType.Inner
                    ) &&
                    (
                        _radius.x + outlineProperties.GetInnerDistace() < 0.0f ||
                        _radius.y + outlineProperties.GetInnerDistace() < 0.0f
                    )
                )
                {
                    if (outlineProperties.Type == GeoUtils.OutlineProperties.LineType.Center)
                    {
                        _radius *= 2.0f;
                    }

                    for (int i = 0; i < shadowProperties.Shadows.Length; i++)
                    {
                        _edgeGradientData.SetActiveData(
                            1.0f - shadowProperties.Shadows[i].Softness,
                            shadowProperties.Shadows[i].Size,
                            antiAliasingProperties.Adjusted
                        );

                        ShapeUtils.Arcs.AddSegment(
                            ref vh,
                            shadowProperties.GetCenterOffset(pixelRect.center, i),
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
                else
                {
                    for (int i = 0; i < shadowProperties.Shadows.Length; i++)
                    {
                        _edgeGradientData.SetActiveData(
                            1.0f - shadowProperties.Shadows[i].Softness,
                            shadowProperties.Shadows[i].Size,
                            antiAliasingProperties.Adjusted
                        );

                        if (lineProperties.LineCap == ShapeUtils.Lines.LineProperties.LineCapTypes.Close)
                        {
                            ShapeUtils.Arcs.AddArcRing(
                                ref vh,
                                shadowProperties.GetCenterOffset(pixelRect.center, i),
                                _radius,
                                ellipseProperties,
                                arcProperties,
                                outlineProperties,
                                shadowProperties.Shadows[i].Color,
                                GeoUtils.ZeroV2,
                                ref _unitPositionData,
                                _edgeGradientData
                            );
                        }
                        else
                        {
                            ShapeUtils.Lines.AddLine(
                                ref vh,
                                lineProperties,
                                pointListProperties,
                                shadowProperties.GetCenterOffset(pixelRect.center, i),
                                outlineProperties,
                                shadowProperties.Shadows[i].Color,
                                GeoUtils.ZeroV2,
                                ref _pointsData,
                                _edgeGradientData
                            );
                        }
                    }
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

                // use segment if LineWeight is overshooting the center
                if (
                    (
                        outlineProperties.Type == GeoUtils.OutlineProperties.LineType.Center ||
                        outlineProperties.Type == GeoUtils.OutlineProperties.LineType.Inner
                    ) &&
                    (
                        _radius.x + outlineProperties.GetInnerDistace() < 0.0f ||
                        _radius.y + outlineProperties.GetInnerDistace() < 0.0f
                    )
                )
                {
                    if (outlineProperties.Type == GeoUtils.OutlineProperties.LineType.Center)
                    {
                        _radius.x *= 2.0f;
                        _radius.y *= 2.0f;
                    }

                    ShapeUtils.Arcs.AddSegment(
                        ref vh,
                        pixelRect.center,
                        _radius,
                        ellipseProperties,
                        arcProperties,
                        shapeProperties.FillColor,
                        GeoUtils.ZeroV2,
                        ref _unitPositionData,
                        _edgeGradientData
                    );
                }
                else
                {
                    if (lineProperties.LineCap == ShapeUtils.Lines.LineProperties.LineCapTypes.Close)
                    {
                        ShapeUtils.Arcs.AddArcRing(
                            ref vh,
                            pixelRect.center,
                            _radius,
                            ellipseProperties,
                            arcProperties,
                            outlineProperties,
                            shapeProperties.FillColor,
                            GeoUtils.ZeroV2,
                            ref _unitPositionData,
                            _edgeGradientData
                        );
                    }
                    else
                    {
                        ShapeUtils.Lines.AddLine(
                            ref vh,
                            lineProperties,
                            pointListProperties,
                            pixelRect.center,
                            outlineProperties,
                            shapeProperties.FillColor,
                            GeoUtils.ZeroV2,
                            ref _pointsData,
                            _edgeGradientData
                        );
                    }
                }
            }
        }
    }
}
