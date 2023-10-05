using UIShapeKit.Prop;
using UIShapeKit.ShapeUtils;
using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
    [AddComponentMenu("UI/Shapes/Line", 30), RequireComponent(typeof(CanvasRenderer))]
    public class Line : MaskableGraphic, IShape
    {
        [SerializeField] public ShapeProperties shapeProperties = new();
        [SerializeField] public PointsList.PointListsProperties pointListsProperties = new();
        [SerializeField] public Lines.LineProperties lineProperties = new();
        [SerializeField] public OutlineProperties outlineProperties = new();
        [SerializeField] public ShadowsProperties shadowProperties = new();
        [SerializeField] public AntiAliasingProperties antiAliasingProperties = new();

        public Sprite Sprite;

        private PointsList.PointsData[] _pointsListData = {new()};
        private GeoUtils.EdgeGradientData _edgeGradientData;

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

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            lineProperties.OnCheck();
            outlineProperties.OnCheck();
            antiAliasingProperties.OnCheck();

            ForceMeshUpdate();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            outlineProperties.UpdateAdjusted();
            shadowProperties.UpdateAdjusted();

            if (_pointsListData == null || _pointsListData.Length != pointListsProperties.pointListProperties.Length)
            {
                System.Array.Resize(ref _pointsListData, pointListsProperties.pointListProperties.Length);

                for (int i = 0; i < _pointsListData.Length; i++)
                {
                    _pointsListData[i].NeedsUpdate = true;
                    pointListsProperties.pointListProperties[i].generatorData.needsUpdate = true;
                }
            }

            for (int i = 0; i < pointListsProperties.pointListProperties.Length; i++)
                pointListsProperties.pointListProperties[i].SetPoints();

            for (int i = 0; i < pointListsProperties.pointListProperties.Length; i++)
            {
                if (
                    pointListsProperties.pointListProperties[i].positions != null &&
                    pointListsProperties.pointListProperties[i].positions.Length > 1
                )
                {
                    antiAliasingProperties.UpdateAdjusted(canvas);

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

                            ShapeUtils.Lines.AddLine(
                                ref vh,
                                lineProperties,
                                pointListsProperties.pointListProperties[i],
                                shadowProperties.GetCenterOffset(GeoUtils.ZeroV2, j),
                                outlineProperties,
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
                    pointListsProperties.pointListProperties[i].positions.Length > 1
                )
                {
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

                        ShapeUtils.Lines.AddLine(
                            ref vh,
                            lineProperties,
                            pointListsProperties.pointListProperties[i],
                            GeoUtils.ZeroV2,
                            outlineProperties,
                            shapeProperties.fillColor,
                            GeoUtils.ZeroV2,
                            ref _pointsListData[i],
                            _edgeGradientData
                        );
                    }
                }
            }
        }

        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();

            // check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)

            if (Sprite == null)
            {
                canvasRenderer.SetAlphaTexture(null);
                return;
            }

            Texture2D alphaTex = Sprite.associatedAlphaSplitTexture;

            if (alphaTex != null)
            {
                canvasRenderer.SetAlphaTexture(alphaTex);
            }
        }

        public override Texture mainTexture
        {
            get
            {
                if (Sprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }

                    return s_WhiteTexture;
                }

                return Sprite.texture;
            }
        }
    }
}
