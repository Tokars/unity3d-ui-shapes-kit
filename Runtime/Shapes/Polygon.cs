using UIShapeKit.Prop;
using UIShapeKit.ShapeUtils;
using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
    [AddComponentMenu("UI/Shapes/Polygon", 30), RequireComponent(typeof(CanvasRenderer))]
    public class Polygon : MaskableGraphic, IShape
    {
        [SerializeField] public ShapeProperties shapeProperties = new();
        [SerializeField] public PointsList.PointListsProperties pointListsProperties = new();
        [SerializeField] public Polygons.PolygonProperties polygonProperties = new();
        [SerializeField] public ShadowsProperties shadowProperties = new();
        [SerializeField] public AntiAliasingProperties antiAliasingProperties = new();

        private PointsList.PointsData[] _pointsListData = {new()};
        private GeoUtils.EdgeGradientData _edgeGradientData;
        private Rect _pixelRect;

        public void ForceMeshUpdate()
        {
            if (_pointsListData == null || _pointsListData.Length != pointListsProperties.pointListProperties.Length)
            {
                System.Array.Resize(ref _pointsListData, pointListsProperties.pointListProperties.Length);
            }

            for (int i = 0; i < _pointsListData.Length; i++)
            {
                _pointsListData[i].NeedsUpdate = true;
                pointListsProperties.pointListProperties[i].generatorData.needsUpdate = true;
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
            if (_pointsListData == null || _pointsListData.Length != pointListsProperties.pointListProperties.Length)
            {
                System.Array.Resize(ref _pointsListData, pointListsProperties.pointListProperties.Length);
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

            if (_pointsListData == null || _pointsListData.Length != pointListsProperties.pointListProperties.Length)
            {
                System.Array.Resize(ref _pointsListData, pointListsProperties.pointListProperties.Length);

                for (int i = 0; i < _pointsListData.Length; i++)
                {
                    _pointsListData[i].NeedsUpdate = true;
                    _pointsListData[i].IsClosed = true;
                }
            }

            _pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);

            antiAliasingProperties.UpdateAdjusted(canvas);
            shadowProperties.UpdateAdjusted();

            for (int i = 0; i < pointListsProperties.pointListProperties.Length; i++)
            {
                pointListsProperties.pointListProperties[i].generatorData.skipLastPosition = true;
                pointListsProperties.pointListProperties[i].SetPoints();
            }

            for (int i = 0; i < pointListsProperties.pointListProperties.Length; i++)
            {
                if (
                    pointListsProperties.pointListProperties[i].positions != null &&
                    pointListsProperties.pointListProperties[i].positions.Length > 2
                )
                {
                    polygonProperties.UpdateAdjusted(pointListsProperties.pointListProperties[i]);

                    // shadows
                    if (shadowProperties.ShadowsEnabled)
                    {
                        for (int j = 0; j < shadowProperties.shadows.Length; j++)
                        {
                            _edgeGradientData.SetActiveData(
                                1.0f - shadowProperties.shadows[j].softness,
                                shadowProperties.shadows[j].size,
                                antiAliasingProperties.Adjusted
                            );

                            ShapeUtils.Polygons.AddPolygon(
                                ref vh,
                                polygonProperties,
                                pointListsProperties.pointListProperties[i],
                                shadowProperties.GetCenterOffset(_pixelRect.center, j),
                                shadowProperties.shadows[j].color,
                                GeoUtils.ZeroV2,
                                ref _pointsListData[i],
                                _edgeGradientData
                            );
                        }
                    }
                }
            }


            for (int i = 0; i < pointListsProperties.pointListProperties.Length; i++)
            {
                if (
                    pointListsProperties.pointListProperties[i].positions != null &&
                    pointListsProperties.pointListProperties[i].positions.Length > 2
                )
                {
                    polygonProperties.UpdateAdjusted(pointListsProperties.pointListProperties[i]);

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

                        ShapeUtils.Polygons.AddPolygon(
                            ref vh,
                            polygonProperties,
                            pointListsProperties.pointListProperties[i],
                            _pixelRect.center,
                            shapeProperties.fillColor,
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
