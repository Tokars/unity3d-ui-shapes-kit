using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
    [AddComponentMenu("UI/Shapes/Edge Gradient", 100), RequireComponent(typeof(CanvasRenderer))]
    public class EdgeGradient : MaskableGraphic, IShape
    {
        public enum Positions
        {
            Top,
            Bottom,
            Left,
            Right,

            OuterTop,
            OuterBottom,
            OuterLeft,
            OuterRight
        }

        [SerializeField] public GradientProperty[] properties = {new()};

        [System.Serializable]
        public class GradientProperty
        {
            public float Size = 20.0f;
            public Color32 Color = new Color32(127, 127, 127, 255);

            public Positions Position = Positions.Top;
        }

        Vector3 topLeft = Vector3.zero;
        Color32 gradientColor = new Color32(127, 127, 127, 255);

        public void ForceMeshUpdate()
        {
            SetVerticesDirty();
            SetMaterialDirty();
        }

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);

            for (int i = 0; i < properties.Length; i++)
            {
                gradientColor.r = properties[i].Color.r;
                gradientColor.g = properties[i].Color.g;
                gradientColor.b = properties[i].Color.b;
                gradientColor.a = 0;

                switch (properties[i].Position)
                {
                    case Positions.Top:
                        topLeft.x = pixelRect.xMin;
                        topLeft.y = pixelRect.yMax;

                        ShapeUtils.Rects.AddVerticalTwoColorRect(
                            ref vh,
                            topLeft,
                            properties[i].Size,
                            pixelRect.width,
                            properties[i].Color,
                            gradientColor,
                            GeoUtils.ZeroV2
                        );
                        break;

                    case Positions.Bottom:
                        topLeft.x = pixelRect.xMin;
                        topLeft.y = pixelRect.yMin + properties[i].Size;

                        ShapeUtils.Rects.AddVerticalTwoColorRect(
                            ref vh,
                            topLeft,
                            properties[i].Size,
                            pixelRect.width,
                            gradientColor,
                            properties[i].Color,
                            GeoUtils.ZeroV2
                        );
                        break;

                    case Positions.Left:
                        topLeft.x = pixelRect.xMin;
                        topLeft.y = pixelRect.yMax;

                        ShapeUtils.Rects.AddHorizontalTwoColorRect(
                            ref vh,
                            topLeft,
                            pixelRect.height,
                            properties[i].Size,
                            properties[i].Color,
                            gradientColor,
                            GeoUtils.ZeroV2
                        );
                        break;

                    case Positions.Right:
                        topLeft.x = pixelRect.xMax - properties[i].Size;
                        topLeft.y = pixelRect.yMax;

                        ShapeUtils.Rects.AddHorizontalTwoColorRect(
                            ref vh,
                            topLeft,
                            pixelRect.height,
                            properties[i].Size,
                            gradientColor,
                            properties[i].Color,
                            GeoUtils.ZeroV2
                        );
                        break;

                    case Positions.OuterTop:
                        topLeft.x = pixelRect.xMin;
                        topLeft.y = pixelRect.yMax + properties[i].Size;

                        ShapeUtils.Rects.AddVerticalTwoColorRect(
                            ref vh,
                            topLeft,
                            properties[i].Size,
                            pixelRect.width,
                            gradientColor,
                            properties[i].Color,
                            GeoUtils.ZeroV2
                        );
                        break;

                    case Positions.OuterBottom:
                        topLeft.x = pixelRect.xMin;
                        topLeft.y = pixelRect.yMin;

                        ShapeUtils.Rects.AddVerticalTwoColorRect(
                            ref vh,
                            topLeft,
                            properties[i].Size,
                            pixelRect.width,
                            properties[i].Color,
                            gradientColor,
                            GeoUtils.ZeroV2
                        );
                        break;

                    case Positions.OuterLeft:
                        topLeft.x = pixelRect.xMin - properties[i].Size;
                        topLeft.y = pixelRect.yMax;

                        ShapeUtils.Rects.AddHorizontalTwoColorRect(
                            ref vh,
                            topLeft,
                            pixelRect.height,
                            properties[i].Size,
                            gradientColor,
                            properties[i].Color,
                            GeoUtils.ZeroV2
                        );
                        break;

                    case Positions.OuterRight:
                        topLeft.x = pixelRect.xMax;
                        topLeft.y = pixelRect.yMax;

                        ShapeUtils.Rects.AddHorizontalTwoColorRect(
                            ref vh,
                            topLeft,
                            pixelRect.height,
                            properties[i].Size,
                            properties[i].Color,
                            gradientColor,
                            GeoUtils.ZeroV2
                        );
                        break;
                }
            }
        }
    }
}
