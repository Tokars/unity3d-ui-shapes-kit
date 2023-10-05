using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
    [AddComponentMenu("UI/Shapes/Pixel Line", 100), RequireComponent(typeof(CanvasRenderer))]
    public class PixelLine : MaskableGraphic, IShape
    {
        [SerializeField] public float lineWeight = 1.0f;
        [SerializeField] public GeoUtils.SnappedPositionAndOrientationProperties snappedProperties = new();

        private Vector3 _center = Vector3.zero;

        public void ForceMeshUpdate()
        {
            SetVerticesDirty();
            SetMaterialDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            ForceMeshUpdate();
        }
#endif

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();

            Rect pixelRect = RectTransformUtility.PixelAdjustRect(rectTransform, canvas);

            float pixelSizeScaler = 1.0f;

            if (canvas != null)
            {
                pixelSizeScaler = 1.0f / canvas.scaleFactor;
            }

            float adjustedLineWeight = lineWeight * pixelSizeScaler;

            switch (snappedProperties.Position)
            {
                case GeoUtils.SnappedPositionAndOrientationProperties.PositionTypes.Center:
                    _center.x = pixelRect.center.x;
                    _center.y = pixelRect.center.y;
                    break;
                case GeoUtils.SnappedPositionAndOrientationProperties.PositionTypes.Top:
                    _center.x = pixelRect.center.x;
                    _center.y = pixelRect.yMax - adjustedLineWeight;
                    break;
                case GeoUtils.SnappedPositionAndOrientationProperties.PositionTypes.Bottom:
                    _center.x = pixelRect.center.x;
                    _center.y = pixelRect.yMin;
                    break;
                case GeoUtils.SnappedPositionAndOrientationProperties.PositionTypes.Left:
                    _center.x = pixelRect.xMin;
                    _center.y = pixelRect.center.y;
                    break;
                case GeoUtils.SnappedPositionAndOrientationProperties.PositionTypes.Right:
                    _center.x = pixelRect.xMax;
                    _center.y = pixelRect.center.y;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }

            float width = 0.0f;
            float height = 0.0f;

            switch (snappedProperties.Orientation)
            {
                case GeoUtils.SnappedPositionAndOrientationProperties.OrientationTypes.Horizontal:
                    width = pixelRect.width;
                    height = adjustedLineWeight;

                    //				topLeft.x -= width * 0.5f + adjustedLineWeight;
                    break;
                case GeoUtils.SnappedPositionAndOrientationProperties.OrientationTypes.Vertical:
                    width = adjustedLineWeight;
                    height = pixelRect.height;

                    //				topLeft.y += height * 0.5f - adjustedLineWeight;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }

            ShapeUtils.Rects.AddRect(
                ref vh,
                _center,
                width,
                height,
                color,
                GeoUtils.ZeroV2
            );
        }
    }
}
