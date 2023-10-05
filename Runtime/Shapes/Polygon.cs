using UIShapeKit.ShapeUtils;
using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
    [AddComponentMenu("UI/Shapes/Polygon", 30), RequireComponent(typeof(CanvasRenderer))]
    public class Polygon : MaskableGraphic, IShape
    {
        [SerializeField] public GeoUtils.ShapeProperties shapeProperties = new();
        [SerializeField] public PointsList.PointListsProperties pointListsProperties = new();
        [SerializeField] public Polygons.PolygonProperties polygonProperties = new();
        [SerializeField] public GeoUtils.ShadowsProperties shadowProperties = new();
        [SerializeField] public GeoUtils.AntiAliasingProperties antiAliasingProperties = new();

        private PointsList.PointsData[] _pointsListData = {new()};
        private GeoUtils.EdgeGradientData _edgeGradientData;
        private Rect _pixelRect;

        public void ForceMeshUpdate()
        {
            if (_pointsListData == null || _pointsListData.Length != pointListsProperties.PointListProperties.Length)
            {
                System.Array.Resize(ref _pointsListData, pointListsProperties.PointListProperties.Length);
            }

            for (int i = 0; i < _pointsListData.Length; i++)
            {
                _pointsListData[i].NeedsUpdate = true;
                pointListsProperties.PointListProperties[i].GeneratorData.NeedsUpdate = true;
            }

            SetVerticesDirty();
            SetMaterialDirty();
        }

        protected override void OnEnable()
        {
            for (int i = 0; i < _pointsListData.Length; i++)
            {
                _pointsListData[i].IsClosed = true;
            }

            base.OnEnable();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            if (_pointsListData == null || _pointsListData.Length != pointListsProperties.PointListProperties.Length)
            {
                System.Array.Resize(ref _pointsListData, pointListsProperties.PointListProperties.Length);
            }

            for (int i = 0; i < _pointsListData.Length; i++)
            {
                _pointsListData[i].NeedsUpdate = true;
                _pointsListData[i].IsClosed = true;
            }


            antiAliasingProperties.OnCheck();

            ForceMeshUpdate();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            if (_pointsListData == null || _pointsListData.Length != pointListsProperties.PointListProperties.Length)
            {
                System.Array.Resize(ref _pointsListData, pointListsProperties.PointListProperties.Length);

                for (int i = 0; i < _pointsListData.Length; i++)
                {
                    _pointsListData[i].NeedsUpdate = true;
                    _pointsListData[i].IsClosed = true;
                }
            }

            _pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);

            antiAliasingProperties.UpdateAdjusted(canvas);
            shadowProperties.UpdateAdjusted();

            for (int i = 0; i < pointListsProperties.PointListProperties.Length; i++)
            {
                pointListsProperties.PointListProperties[i].GeneratorData.SkipLastPosition = true;
                pointListsProperties.PointListProperties[i].SetPoints();
            }

            for (int i = 0; i < pointListsProperties.PointListProperties.Length; i++)
            {
                if (
                    pointListsProperties.PointListProperties[i].Positions != null &&
                    pointListsProperties.PointListProperties[i].Positions.Length > 2
                )
                {
                    polygonProperties.UpdateAdjusted(pointListsProperties.PointListProperties[i]);

                    // shadows
                    if (shadowProperties.ShadowsEnabled)
                    {
                        for (int j = 0; j < shadowProperties.Shadows.Length; j++)
                        {
                            _edgeGradientData.SetActiveData(
                                1.0f - shadowProperties.Shadows[j].Softness,
                                shadowProperties.Shadows[j].Size,
                                antiAliasingProperties.Adjusted
                            );

                            ShapeUtils.Polygons.AddPolygon(
                                ref vh,
                                polygonProperties,
                                pointListsProperties.PointListProperties[i],
                                shadowProperties.GetCenterOffset(_pixelRect.center, j),
                                shadowProperties.Shadows[j].Color,
                                GeoUtils.ZeroV2,
                                ref _pointsListData[i],
                                _edgeGradientData
                            );
                        }
                    }
                }
            }


            for (int i = 0; i < pointListsProperties.PointListProperties.Length; i++)
            {
                if (
                    pointListsProperties.PointListProperties[i].Positions != null &&
                    pointListsProperties.PointListProperties[i].Positions.Length > 2
                )
                {
                    polygonProperties.UpdateAdjusted(pointListsProperties.PointListProperties[i]);

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

                        ShapeUtils.Polygons.AddPolygon(
                            ref vh,
                            polygonProperties,
                            pointListsProperties.PointListProperties[i],
                            _pixelRect.center,
                            shapeProperties.FillColor,
                            GeoUtils.ZeroV2,
                            ref _pointsListData[i],
                            _edgeGradientData
                        );
                    }
                }
            }
        }
    }
}
