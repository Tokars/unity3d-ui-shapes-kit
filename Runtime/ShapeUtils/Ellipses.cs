using UIShapeKit.Prop;
using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.ShapeUtils
{
    public class Ellipses
    {
        private static Vector3 s_tmpVertPos = Vector3.zero;
        private static Vector2 s_tmpUVPos = Vector2.zero;
        private static Vector3 s_tmpInnerRadius = Vector3.one;
        private static Vector3 s_tmpOuterRadius = Vector3.one;

        [System.Serializable]
        public class EllipseProperties
        {
            [SerializeField] public EllipseFitting fitting = EllipseFitting.UniformInner;
            [SerializeField] public float baseAngle = 0.0f;
            [SerializeField] public ResolutionType resolution = ResolutionType.Calculated;
            [SerializeField] public int fixedResolution = 50;
            [SerializeField] public float resolutionMaxDistance = 4.0f;

            public enum EllipseFitting
            {
                Ellipse,
                UniformInner,
                UniformOuter
            }

            public enum ResolutionType
            {
                Calculated,
                Fixed
            }

            public int AdjustedResolution { private set; get; }

            public void OnCheck()
            {
                fixedResolution = Mathf.Max(fixedResolution, 3);
                resolutionMaxDistance = Mathf.Max(resolutionMaxDistance, 0.1f);
            }

            public void UpdateAdjusted(Vector2 radius, float offset)
            {
                radius.x += offset;
                radius.y += offset;

                switch (resolution)
                {
                    case ResolutionType.Calculated:
                        float circumference;

                        if (radius.x == radius.y)
                        {
                            circumference = GeoUtils.TwoPI * radius.x;
                        }
                        else
                        {
                            circumference = Mathf.PI * (
                                3.0f * (radius.x + radius.y) -
                                Mathf.Sqrt(
                                    (3.0f * radius.x + radius.y) *
                                    (radius.x + 3.0f * radius.y)
                                )
                            );
                        }

                        AdjustedResolution = Mathf.CeilToInt(circumference / resolutionMaxDistance);
                        break;
                    case ResolutionType.Fixed:
                        AdjustedResolution = fixedResolution;
                        break;
                    default:
                        throw new System.ArgumentOutOfRangeException();
                }
            }
        }

        public static void SetRadius(
            ref Vector2 radius,
            float width,
            float height,
            EllipseProperties properties
        )
        {
            width *= 0.5f;
            height *= 0.5f;

            switch (properties.fitting)
            {
                case EllipseProperties.EllipseFitting.UniformInner:
                    radius.x = Mathf.Min(width, height);
                    radius.y = radius.x;
                    break;
                case EllipseProperties.EllipseFitting.UniformOuter:
                    radius.x = Mathf.Max(width, height);
                    radius.y = radius.x;
                    break;
                case EllipseProperties.EllipseFitting.Ellipse:
                    radius.x = width;
                    radius.y = height;
                    break;
            }
        }

        public static void AddCircle(
            ref VertexHelper vh,
            Vector2 center,
            Vector2 radius,
            EllipseProperties ellipseProperties,
            Color32 color,
            Vector2 uv,
            ref GeoUtils.UnitPositionData unitPositionData,
            GeoUtils.EdgeGradientData edgeGradientData
        )
        {
            GeoUtils.SetUnitPositionData(
                ref unitPositionData,
                ellipseProperties.AdjustedResolution,
                ellipseProperties.baseAngle
            );

            int numVertices = vh.currentVertCount;

            s_tmpUVPos.x = 0.5f;
            s_tmpUVPos.y = 0.5f;
            vh.AddVert(center, color, s_tmpUVPos, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

            // add first circle vertex
            s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[0].x * (radius.x + edgeGradientData.ShadowOffset) *
                edgeGradientData.InnerScale;
            s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[0].y * (radius.y + edgeGradientData.ShadowOffset) *
                edgeGradientData.InnerScale;
            s_tmpVertPos.z = 0.0f;

            s_tmpUVPos.x = (unitPositionData.UnitPositions[0].x * edgeGradientData.InnerScale + 1.0f) * 0.5f;
            s_tmpUVPos.y = (unitPositionData.UnitPositions[0].y * edgeGradientData.InnerScale + 1.0f) * 0.5f;
            vh.AddVert(s_tmpVertPos, color, s_tmpUVPos, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

            for (int i = 1; i < ellipseProperties.AdjustedResolution; i++)
            {
                s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[i].x *
                    (radius.x + edgeGradientData.ShadowOffset) * edgeGradientData.InnerScale;
                s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[i].y *
                    (radius.y + edgeGradientData.ShadowOffset) * edgeGradientData.InnerScale;
                s_tmpVertPos.z = 0.0f;

                s_tmpUVPos.x = (unitPositionData.UnitPositions[i].x * edgeGradientData.InnerScale + 1.0f) * 0.5f;
                s_tmpUVPos.y = (unitPositionData.UnitPositions[i].y * edgeGradientData.InnerScale + 1.0f) * 0.5f;
                vh.AddVert(s_tmpVertPos, color, s_tmpUVPos, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                vh.AddTriangle(numVertices, numVertices + i, numVertices + i + 1);
            }

            vh.AddTriangle(numVertices, numVertices + ellipseProperties.AdjustedResolution, numVertices + 1);

            if (edgeGradientData.IsActive)
            {
                radius.x += edgeGradientData.ShadowOffset + edgeGradientData.SizeAdd;
                radius.y += edgeGradientData.ShadowOffset + edgeGradientData.SizeAdd;

                int outerFirstIndex = numVertices + ellipseProperties.AdjustedResolution;

                color.a = 0;

                // add first point
                s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[0].x * radius.x;
                s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[0].y * radius.y;
                s_tmpVertPos.z = 0.0f;

                s_tmpUVPos.x = (unitPositionData.UnitPositions[0].x + 1.0f) * 0.5f;
                s_tmpUVPos.y = (unitPositionData.UnitPositions[0].y + 1.0f) * 0.5f;
                vh.AddVert(s_tmpVertPos, color, s_tmpUVPos, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                for (int i = 1; i < ellipseProperties.AdjustedResolution; i++)
                {
                    s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[i].x * radius.x;
                    s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[i].y * radius.y;
                    s_tmpVertPos.z = 0.0f;

                    s_tmpUVPos.x = (unitPositionData.UnitPositions[i].x + 1.0f) * 0.5f;
                    s_tmpUVPos.y = (unitPositionData.UnitPositions[i].y + 1.0f) * 0.5f;
                    vh.AddVert(s_tmpVertPos, color, s_tmpUVPos, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                    vh.AddTriangle(numVertices + i + 1, outerFirstIndex + i, outerFirstIndex + i + 1);
                    vh.AddTriangle(numVertices + i + 1, outerFirstIndex + i + 1, numVertices + i + 2);
                }

                vh.AddTriangle(numVertices + 1, outerFirstIndex, outerFirstIndex + 1);
                vh.AddTriangle(numVertices + 2, numVertices + 1, outerFirstIndex + 1);
            }
        }


        public static void AddRing(
            ref VertexHelper vh,
            Vector2 center,
            Vector2 radius,
            OutlineProperties outlineProperties,
            EllipseProperties ellipseProperties,
            Color32 color,
            Vector2 uv,
            ref GeoUtils.UnitPositionData unitPositionData,
            GeoUtils.EdgeGradientData edgeGradientData
        )
        {
            GeoUtils.SetUnitPositionData(
                ref unitPositionData,
                ellipseProperties.AdjustedResolution,
                ellipseProperties.baseAngle
            );

            float halfLineWeightOffset = (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset) *
                                         edgeGradientData.InnerScale;

            s_tmpInnerRadius.x = radius.x + outlineProperties.GetCenterDistance() - halfLineWeightOffset;
            s_tmpInnerRadius.y = radius.y + outlineProperties.GetCenterDistance() - halfLineWeightOffset;

            s_tmpOuterRadius.x = radius.x + outlineProperties.GetCenterDistance() + halfLineWeightOffset;
            s_tmpOuterRadius.y = radius.y + outlineProperties.GetCenterDistance() + halfLineWeightOffset;

            int numVertices = vh.currentVertCount;
            int startVertex = numVertices - 1;

            int baseIndex;

            float uvMaxResolution = (float)ellipseProperties.AdjustedResolution;

            for (int i = 0; i < ellipseProperties.AdjustedResolution; i++)
            {
                uv.x = i / uvMaxResolution;

                s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[i].x * s_tmpInnerRadius.x;
                s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[i].y * s_tmpInnerRadius.y;
                s_tmpVertPos.z = 0.0f;
                uv.y = 0.0f;
                vh.AddVert(s_tmpVertPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[i].x * s_tmpOuterRadius.x;
                s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[i].y * s_tmpOuterRadius.y;
                s_tmpVertPos.z = 0.0f;
                uv.y = 1.0f;
                vh.AddVert(s_tmpVertPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                if (i > 0)
                {
                    baseIndex = startVertex + i * 2;
                    vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
                    vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
                }
            }

            // add last quad
            {
                uv.x = 1.0f;

                s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[0].x * s_tmpInnerRadius.x;
                s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[0].y * s_tmpInnerRadius.y;
                s_tmpVertPos.z = 0.0f;
                uv.y = 0.0f;
                vh.AddVert(s_tmpVertPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[0].x * s_tmpOuterRadius.x;
                s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[0].y * s_tmpOuterRadius.y;
                s_tmpVertPos.z = 0.0f;
                uv.y = 1.0f;
                vh.AddVert(s_tmpVertPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                baseIndex = startVertex + ellipseProperties.AdjustedResolution * 2;
                vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
                vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
            }

            if (edgeGradientData.IsActive)
            {
                halfLineWeightOffset = outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset +
                                       edgeGradientData.SizeAdd;

                s_tmpInnerRadius.x = radius.x + outlineProperties.GetCenterDistance() - halfLineWeightOffset;
                s_tmpInnerRadius.y = radius.y + outlineProperties.GetCenterDistance() - halfLineWeightOffset;

                s_tmpOuterRadius.x = radius.x + outlineProperties.GetCenterDistance() + halfLineWeightOffset;
                s_tmpOuterRadius.y = radius.y + outlineProperties.GetCenterDistance() + halfLineWeightOffset;

                color.a = 0;

                int edgesBaseIndex;
                int innerBaseIndex;

                for (int i = 0; i < ellipseProperties.AdjustedResolution; i++)
                {
                    uv.x = i / uvMaxResolution;

                    s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[i].x * s_tmpInnerRadius.x;
                    s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[i].y * s_tmpInnerRadius.y;
                    s_tmpVertPos.z = 0.0f;
                    uv.y = 0.0f;
                    vh.AddVert(s_tmpVertPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                    s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[i].x * s_tmpOuterRadius.x;
                    s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[i].y * s_tmpOuterRadius.y;
                    s_tmpVertPos.z = 0.0f;
                    uv.y = 1.0f;
                    vh.AddVert(s_tmpVertPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                    edgesBaseIndex = baseIndex + i * 2;
                    innerBaseIndex = startVertex + i * 2;

                    if (i > 0)
                    {
                        // inner quad
                        vh.AddTriangle(innerBaseIndex - 1, innerBaseIndex + 1, edgesBaseIndex + 3);
                        vh.AddTriangle(edgesBaseIndex + 1, innerBaseIndex - 1, edgesBaseIndex + 3);

                        // outer quad
                        vh.AddTriangle(innerBaseIndex, edgesBaseIndex + 2, innerBaseIndex + 2);
                        vh.AddTriangle(edgesBaseIndex + 2, edgesBaseIndex + 4, innerBaseIndex + 2);
                    }
                }

                // add last quads
                {
                    uv.x = 1.0f;

                    s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[0].x * s_tmpInnerRadius.x;
                    s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[0].y * s_tmpInnerRadius.y;
                    s_tmpVertPos.z = 0.0f;
                    uv.y = 0.0f;
                    vh.AddVert(s_tmpVertPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                    s_tmpVertPos.x = center.x + unitPositionData.UnitPositions[0].x * s_tmpOuterRadius.x;
                    s_tmpVertPos.y = center.y + unitPositionData.UnitPositions[0].y * s_tmpOuterRadius.y;
                    s_tmpVertPos.z = 0.0f;
                    uv.y = 1.0f;
                    vh.AddVert(s_tmpVertPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                    edgesBaseIndex = baseIndex + ellipseProperties.AdjustedResolution * 2;
                    innerBaseIndex = startVertex + ellipseProperties.AdjustedResolution * 2;

                    // inner quad
                    vh.AddTriangle(innerBaseIndex - 1, innerBaseIndex + 1, edgesBaseIndex + 3);
                    vh.AddTriangle(edgesBaseIndex + 1, innerBaseIndex - 1, edgesBaseIndex + 3);

                    // outer quad
                    vh.AddTriangle(innerBaseIndex, edgesBaseIndex + 2, innerBaseIndex + 2);
                    vh.AddTriangle(edgesBaseIndex + 2, edgesBaseIndex + 4, innerBaseIndex + 2);
                }
            }
        }
    }
}
