using UIShapeKit.Prop;
using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
    [AddComponentMenu("UI/Shapes/Rectangle", 1), RequireComponent(typeof(CanvasRenderer))]
    public class Rectangle : MaskableGraphic, IShape
    {
        [SerializeField] public OutlineShapeProperties shapeProperties = new();
        [SerializeField] public ShapeUtils.RoundedRects.RoundedProperties roundedProperties = new();
        [SerializeField] public OutlineProperties outlineProperties = new();
        [SerializeField] public ShadowsProperties shadowProperties = new();
        [SerializeField] public AntiAliasingProperties antiAliasingProperties = new();

        [SerializeField] public Sprite sprite;

        private ShapeUtils.RoundedRects.RoundedCornerUnitPositionData _unitPositionData;
        private GeoUtils.EdgeGradientData _edgeGradientData;

        public void ForceMeshUpdate()
        {
            SetVerticesDirty();
            SetMaterialDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            roundedProperties.OnCheck(rectTransform.rect);
            outlineProperties.OnCheck();
            antiAliasingProperties.OnCheck();

            ForceMeshUpdate();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);

            roundedProperties.UpdateAdjusted(pixelRect, 0.0f);
            antiAliasingProperties.UpdateAdjusted(canvas);
            outlineProperties.UpdateAdjusted();
            shadowProperties.UpdateAdjusted();

            // draw fill shadows
            if (shadowProperties.ShadowsEnabled)
            {
                if (shapeProperties.drawFill && shapeProperties.drawFillShadow)
                {
                    for (int i = 0; i < shadowProperties.shadows.Length; i++)
                    {
                        _edgeGradientData.SetActiveData(
                            1.0f - shadowProperties.shadows[i].softness,
                            shadowProperties.shadows[i].size,
                            antiAliasingProperties.Adjusted
                        );

                        ShapeUtils.RoundedRects.AddRoundedRect(
                            ref vh,
                            shadowProperties.GetCenterOffset(pixelRect.center, i),
                            pixelRect.width,
                            pixelRect.height,
                            roundedProperties,
                            shadowProperties.shadows[i].color,
                            GeoUtils.ZeroV2,
                            ref _unitPositionData,
                            _edgeGradientData
                        );
                    }
                }
            }

            if (shadowProperties.showShape && shapeProperties.drawFill)
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

                ShapeUtils.RoundedRects.AddRoundedRect(
                    ref vh,
                    pixelRect.center,
                    pixelRect.width,
                    pixelRect.height,
                    roundedProperties,
                    shapeProperties.fillColor,
                    GeoUtils.ZeroV2,
                    ref _unitPositionData,
                    _edgeGradientData
                );
            }

            if (shadowProperties.ShadowsEnabled)
            {
                // draw outline shadows
                if (shapeProperties.drawOutline && shapeProperties.drawOutlineShadow)
                {
                    for (int i = 0; i < shadowProperties.shadows.Length; i++)
                    {
                        _edgeGradientData.SetActiveData(
                            1.0f - shadowProperties.shadows[i].softness,
                            shadowProperties.shadows[i].size,
                            antiAliasingProperties.Adjusted
                        );

                        ShapeUtils.RoundedRects.AddRoundedRectLine(
                            ref vh,
                            shadowProperties.GetCenterOffset(pixelRect.center, i),
                            pixelRect.width,
                            pixelRect.height,
                            outlineProperties,
                            roundedProperties,
                            shadowProperties.shadows[i].color,
                            GeoUtils.ZeroV2,
                            ref _unitPositionData,
                            _edgeGradientData
                        );
                    }
                }
            }

            // fill
            if (shadowProperties.showShape && shapeProperties.drawOutline)
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

                ShapeUtils.RoundedRects.AddRoundedRectLine(
                    ref vh,
                    pixelRect.center,
                    pixelRect.width,
                    pixelRect.height,
                    outlineProperties,
                    roundedProperties,
                    shapeProperties.outlineColor,
                    GeoUtils.ZeroV2,
                    ref _unitPositionData,
                    _edgeGradientData
                );
            }
        }

        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();

            // check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)

            if (sprite == null)
            {
                canvasRenderer.SetAlphaTexture(null);
                return;
            }

            Texture2D alphaTex = sprite.associatedAlphaSplitTexture;

            if (alphaTex != null)
            {
                canvasRenderer.SetAlphaTexture(alphaTex);
            }
        }

        public override Texture mainTexture
        {
            get
            {
                if (sprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }

                    return s_WhiteTexture;
                }

                return sprite.texture;
            }
        }
    }
}
