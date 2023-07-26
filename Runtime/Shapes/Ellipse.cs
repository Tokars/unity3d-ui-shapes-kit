using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
    [AddComponentMenu("UI/Shapes/Ellipse", 1), RequireComponent(typeof(CanvasRenderer))]
    public class Ellipse : MaskableGraphic, IShape
    {
        [SerializeField] public GeoUtils.OutlineShapeProperties shapeProperties = new();
        [SerializeField] public ShapeUtils.Ellipses.EllipseProperties ellipseProperties = new();
        [SerializeField] public GeoUtils.OutlineProperties outlineProperties = new();
        [SerializeField] public GeoUtils.ShadowsProperties shadowProperties = new();
        [SerializeField] public GeoUtils.AntiAliasingProperties antiAliasingProperties = new();

        public Sprite Sprite;

        private GeoUtils.UnitPositionData _unitPositionData;
        private GeoUtils.EdgeGradientData _edgeGradientData;
        private Vector2 _radius = Vector2.one;

        public void ForceMeshUpdate()
        {
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

            outlineProperties.UpdateAdjusted();
            shadowProperties.UpdateAdjusted();

            Rect pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);

            ShapeUtils.Ellipses.SetRadius(
                ref _radius,
                pixelRect.width,
                pixelRect.height,
                ellipseProperties
            );

            ellipseProperties.UpdateAdjusted(_radius, 0.0f);
            antiAliasingProperties.UpdateAdjusted(canvas);


            // draw fill shadows
            if (shadowProperties.ShadowsEnabled)
            {
                if (shapeProperties.DrawFill && shapeProperties.DrawFillShadow)
                {
                    for (int i = 0; i < shadowProperties.Shadows.Length; i++)
                    {
                        _edgeGradientData.SetActiveData(
                            1.0f - shadowProperties.Shadows[i].Softness,
                            shadowProperties.Shadows[i].Size,
                            antiAliasingProperties.Adjusted
                        );

                        ShapeUtils.Ellipses.AddCircle(
                            ref vh,
                            shadowProperties.GetCenterOffset(pixelRect.center, i),
                            _radius,
                            ellipseProperties,
                            shadowProperties.Shadows[i].Color,
                            GeoUtils.ZeroV2,
                            ref _unitPositionData,
                            _edgeGradientData
                        );
                    }
                }
            }

            if (shadowProperties.ShowShape && shapeProperties.DrawFill)
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

                ShapeUtils.Ellipses.AddCircle(
                    ref vh,
                    (Vector3)pixelRect.center,
                    _radius,
                    ellipseProperties,
                    shapeProperties.FillColor,
                    GeoUtils.ZeroV2,
                    ref _unitPositionData,
                    _edgeGradientData
                );
            }

            if (shadowProperties.ShadowsEnabled)
            {
                // draw outline shadows
                if (shapeProperties.DrawOutline && shapeProperties.DrawOutlineShadow)
                {
                    for (int i = 0; i < shadowProperties.Shadows.Length; i++)
                    {
                        _edgeGradientData.SetActiveData(
                            1.0f - shadowProperties.Shadows[i].Softness,
                            shadowProperties.Shadows[i].Size,
                            antiAliasingProperties.Adjusted
                        );

                        ShapeUtils.Ellipses.AddRing(
                            ref vh,
                            shadowProperties.GetCenterOffset(pixelRect.center, i),
                            _radius,
                            outlineProperties,
                            ellipseProperties,
                            shadowProperties.Shadows[i].Color,
                            GeoUtils.ZeroV2,
                            ref _unitPositionData,
                            _edgeGradientData
                        );
                    }
                }
            }


            // fill
            if (shadowProperties.ShowShape && shapeProperties.DrawOutline)
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

                ShapeUtils.Ellipses.AddRing(
                    ref vh,
                    (Vector3)pixelRect.center,
                    _radius,
                    outlineProperties,
                    ellipseProperties,
                    shapeProperties.OutlineColor,
                    Vector2.zero,
                    ref _unitPositionData,
                    _edgeGradientData
                );
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
