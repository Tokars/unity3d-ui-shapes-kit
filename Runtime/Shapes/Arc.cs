using UIShapeKit.Prop;
using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
    [AddComponentMenu("UI/Shapes/Arc", 50), RequireComponent(typeof(CanvasRenderer))]
    public class Arc : MaskableGraphic, IShape
    {
        [SerializeField] public ShapeProperties shapeProperties = new();
        [SerializeField] public ShapeUtils.Ellipses.EllipseProperties ellipseProperties = new();
        [SerializeField] public ShapeUtils.Arcs.ArcProperties arcProperties = new();
        [SerializeField] public ShapeUtils.Lines.LineProperties lineProperties = new();
        [SerializeField] public ShapeUtils.PointsList.PointListProperties pointListProperties = new();
        [SerializeField] public OutlineProperties outlineProperties = new();
        [SerializeField] public ShadowsProperties shadowProperties = new();
        [SerializeField] public AntiAliasingProperties antiAliasingProperties = new();


        private ShapeUtils.PointsList.PointsData _pointsData = new();
        private GeoUtils.UnitPositionData _unitPositionData;
        private GeoUtils.EdgeGradientData _edgeGradientData;
        private Vector2 _radius = Vector2.one;

        protected override void OnEnable()
        {
            pointListProperties.generatorData.generator =
                ShapeUtils.PointsList.PointListGeneratorData.Generators.Round;

            pointListProperties.generatorData.center.x = 0.0f;
            pointListProperties.generatorData.center.y = 0.0f;

            base.OnEnable();
        }

        public void ForceMeshUpdate()
        {
            pointListProperties.generatorData.needsUpdate = true;
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

            pointListProperties.generatorData.width = _radius.x * 2.0f;
            pointListProperties.generatorData.height = _radius.y * 2.0f;

            ellipseProperties.UpdateAdjusted(_radius, outlineProperties.GetOuterDistance());
            arcProperties.UpdateAdjusted(ellipseProperties.AdjustedResolution, ellipseProperties.baseAngle);
            antiAliasingProperties.UpdateAdjusted(canvas);

            pointListProperties.generatorData.resolution = ellipseProperties.AdjustedResolution * 2;
            pointListProperties.generatorData.length = arcProperties.Length;

            switch (arcProperties.Direction)
            {
                case ShapeUtils.Arcs.ArcProperties.ArcDirection.Forward:
                    pointListProperties.generatorData.direction = 1.0f;
                    pointListProperties.generatorData.floatStartOffset = ellipseProperties.baseAngle * 0.5f;
                    break;

                case ShapeUtils.Arcs.ArcProperties.ArcDirection.Centered:
                    pointListProperties.generatorData.direction = -1.0f;
                    pointListProperties.generatorData.floatStartOffset =
                        ellipseProperties.baseAngle * 0.5f + (arcProperties.Length * 0.5f);
                    break;

                case ShapeUtils.Arcs.ArcProperties.ArcDirection.Backward:
                    pointListProperties.generatorData.direction = -1.0f;
                    pointListProperties.generatorData.floatStartOffset = ellipseProperties.baseAngle * 0.5f;
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
                        outlineProperties.type == OutlineProperties.LineType.Center ||
                        outlineProperties.type == OutlineProperties.LineType.Inner
                    ) &&
                    (
                        _radius.x + outlineProperties.GetInnerDistance() < 0.0f ||
                        _radius.y + outlineProperties.GetInnerDistance() < 0.0f
                    )
                )
                {
                    if (outlineProperties.type == OutlineProperties.LineType.Center)
                    {
                        _radius *= 2.0f;
                    }

                    for (int i = 0; i < shadowProperties.shadows.Length; i++)
                    {
                        _edgeGradientData.SetActiveData(
                            1.0f - shadowProperties.shadows[i].softness,
                            shadowProperties.shadows[i].size,
                            antiAliasingProperties.Adjusted
                        );

                        ShapeUtils.Arcs.AddSegment(
                            ref vh,
                            shadowProperties.GetCenterOffset(pixelRect.center, i),
                            _radius,
                            ellipseProperties,
                            arcProperties,
                            shadowProperties.shadows[i].color,
                            GeoUtils.ZeroV2,
                            ref _unitPositionData,
                            _edgeGradientData
                        );
                    }
                }
                else
                {
                    for (int i = 0; i < shadowProperties.shadows.Length; i++)
                    {
                        _edgeGradientData.SetActiveData(
                            1.0f - shadowProperties.shadows[i].softness,
                            shadowProperties.shadows[i].size,
                            antiAliasingProperties.Adjusted
                        );

                        if (lineProperties.lineCap == ShapeUtils.Lines.LineProperties.LineCapTypes.Close)
                        {
                            ShapeUtils.Arcs.AddArcRing(
                                ref vh,
                                shadowProperties.GetCenterOffset(pixelRect.center, i),
                                _radius,
                                ellipseProperties,
                                arcProperties,
                                outlineProperties,
                                shadowProperties.shadows[i].color,
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
                                shadowProperties.shadows[i].color,
                                GeoUtils.ZeroV2,
                                ref _pointsData,
                                _edgeGradientData
                            );
                        }
                    }
                }
            }

            // fill
            if (shadowProperties.showShape)
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
                        outlineProperties.type == OutlineProperties.LineType.Center ||
                        outlineProperties.type == OutlineProperties.LineType.Inner
                    ) &&
                    (
                        _radius.x + outlineProperties.GetInnerDistance() < 0.0f ||
                        _radius.y + outlineProperties.GetInnerDistance() < 0.0f
                    )
                )
                {
                    if (outlineProperties.type == OutlineProperties.LineType.Center)
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
                        shapeProperties.fillColor,
                        GeoUtils.ZeroV2,
                        ref _unitPositionData,
                        _edgeGradientData
                    );
                }
                else
                {
                    if (lineProperties.lineCap == ShapeUtils.Lines.LineProperties.LineCapTypes.Close)
                    {
                        ShapeUtils.Arcs.AddArcRing(
                            ref vh,
                            pixelRect.center,
                            _radius,
                            ellipseProperties,
                            arcProperties,
                            outlineProperties,
                            shapeProperties.fillColor,
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
                            shapeProperties.fillColor,
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
