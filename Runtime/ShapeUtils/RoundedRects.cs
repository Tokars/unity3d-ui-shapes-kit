﻿using UIShapeKit.Prop;
using UnityEngine;
using UnityEngine.UI;
using RoundedCornerProperties = UIShapeKit.Prop.RoundingProperties;

namespace UIShapeKit.ShapeUtils
{
    public class RoundedRects
    {
        public struct RoundedCornerUnitPositionData
        {
            public Vector2[] TLUnitPositions;
            public Vector2[] TRUnitPositions;
            public Vector2[] BRUnitPositions;
            public Vector2[] BLUnitPositions;
        }

        static void SetCornerUnitPositions(
            RoundedProperties roundedProperties,
            ref RoundedCornerUnitPositionData cornerUnitPositions
        )
        {
            SetUnitPosition(ref cornerUnitPositions.TLUnitPositions, roundedProperties.tlResolution.AdjustedResolution,
                GeoUtils.HalfPI + Mathf.PI, roundedProperties.tlResolution.MakeSharpCorner);
            SetUnitPosition(ref cornerUnitPositions.TRUnitPositions, roundedProperties.trResolution.AdjustedResolution,
                0.0f, roundedProperties.trResolution.MakeSharpCorner);
            SetUnitPosition(ref cornerUnitPositions.BRUnitPositions, roundedProperties.brResolution.AdjustedResolution,
                GeoUtils.HalfPI, roundedProperties.brResolution.MakeSharpCorner);
            SetUnitPosition(ref cornerUnitPositions.BLUnitPositions, roundedProperties.blResolution.AdjustedResolution,
                Mathf.PI, roundedProperties.blResolution.MakeSharpCorner);
        }

        static void SetUnitPosition(
            ref Vector2[] unitPositions,
            int resolution,
            float baseAngle,
            bool makeSharpCorner
        )
        {
            bool needsUpdate = false;

            if (
                unitPositions == null ||
                unitPositions.Length != resolution
            )
            {
                unitPositions = new Vector2[resolution];

                for (int i = 0; i < unitPositions.Length; i++)
                {
                    unitPositions[i] = GeoUtils.ZeroV2;
                }

                needsUpdate = true;
            }

            if (needsUpdate)
            {
                float angleIncrement = GeoUtils.HalfPI / ((float)resolution - 1.0f);
                float angle;

                if (makeSharpCorner)
                {
                    angle = baseAngle + GeoUtils.HalfPI * 0.5f;
                    float length = Mathf.Sqrt(2.0f);

                    for (int i = 0; i < resolution; i++)
                    {
                        unitPositions[i].x = Mathf.Sin(angle) * length;
                        unitPositions[i].y = Mathf.Cos(angle) * length;
                    }
                }
                else
                {
                    for (int i = 0; i < resolution; i++)
                    {
                        angle = baseAngle + angleIncrement * i;

                        unitPositions[i].x = Mathf.Sin(angle);
                        unitPositions[i].y = Mathf.Cos(angle);
                    }
                }
            }
        }

        [System.Serializable]
        public class RoundedProperties
        {
            public enum RoundedType
            {
                None,
                Uniform,
                Individual
            }

            public enum ResolutionType
            {
                Uniform,
                Individual
            }

            [SerializeField] public RoundedType type = RoundedType.None;
            [SerializeField] public ResolutionType resolutionMode = ResolutionType.Uniform;
            [SerializeField] public float uniformRadius = 15.0f;
            [SerializeField] public bool useMaxRadius = false;
            [SerializeField] public float tlRadius = 15.0f;
            [SerializeField] public RoundedCornerProperties tlResolution = new();
            [SerializeField] public float trRadius = 15.0f;
            [SerializeField] public RoundedCornerProperties trResolution = new();
            [SerializeField] public float brRadius = 15.0f;
            [SerializeField] public RoundedCornerProperties brResolution = new();
            [SerializeField] public float blRadius = 15.0f;
            [SerializeField] public RoundedCornerProperties blResolution = new();
            [SerializeField] public RoundedCornerProperties uniformResolution = new();

//			public int Resolution = 15;


            public float AdjustedTLRadius { get; private set; }
            public float AdjustedTRRadius { get; private set; }
            public float AdjustedBRRadius { get; private set; }
            public float AdjustedBLRadius { get; private set; }

            public void UpdateAdjusted(Rect rect, float offset)
            {
                switch (type)
                {
                    case RoundedType.Uniform:
                        if (useMaxRadius)
                        {
                            AdjustedTLRadius = Mathf.Min(rect.width, rect.height) * 0.5f;
                            AdjustedTRRadius = AdjustedTLRadius;
                            AdjustedBRRadius = AdjustedTLRadius;
                            AdjustedBLRadius = AdjustedTLRadius;
                        }
                        else
                        {
                            AdjustedTLRadius = uniformRadius;
                            AdjustedTRRadius = AdjustedTLRadius;
                            AdjustedBRRadius = AdjustedTLRadius;
                            AdjustedBLRadius = AdjustedTLRadius;
                        }

                        break;
                    case RoundedType.Individual:
                        AdjustedTLRadius = tlRadius;
                        AdjustedTRRadius = trRadius;
                        AdjustedBRRadius = brRadius;
                        AdjustedBLRadius = blRadius;
                        break;
                    case RoundedType.None:
                        AdjustedTLRadius = 0.0f;
                        AdjustedTRRadius = AdjustedTLRadius;
                        AdjustedBRRadius = AdjustedTLRadius;
                        AdjustedBLRadius = AdjustedTLRadius;
                        break;
                    default:
                        throw new System.ArgumentOutOfRangeException();
                }

                if (resolutionMode == ResolutionType.Uniform)
                {
                    tlResolution.UpdateAdjusted(AdjustedTLRadius, offset, uniformResolution, 4.0f);
                    trResolution.UpdateAdjusted(AdjustedTRRadius, offset, uniformResolution, 4.0f);
                    brResolution.UpdateAdjusted(AdjustedBRRadius, offset, uniformResolution, 4.0f);
                    blResolution.UpdateAdjusted(AdjustedBLRadius, offset, uniformResolution, 4.0f);
                }
                else
                {
                    tlResolution.UpdateAdjusted(AdjustedTLRadius, offset, 4.0f);
                    trResolution.UpdateAdjusted(AdjustedTRRadius, offset, 4.0f);
                    brResolution.UpdateAdjusted(AdjustedBRRadius, offset, 4.0f);
                    blResolution.UpdateAdjusted(AdjustedBLRadius, offset, 4.0f);
                }
            }

            public void OnCheck(Rect rect)
            {
                float shorterSide = Mathf.Min(rect.width, rect.height);
                float halfShorterSide = shorterSide * 0.5f;

                // check radii don't overlap
                switch (type)
                {
                    case RoundedType.Uniform:
                        uniformRadius = Mathf.Clamp(uniformRadius, 0.0f, halfShorterSide);
                        break;
                    case RoundedType.Individual:
                        tlRadius = Mathf.Max(tlRadius, 0.0f);
                        trRadius = Mathf.Max(trRadius, 0.0f);
                        brRadius = Mathf.Max(brRadius, 0.0f);
                        blRadius = Mathf.Max(blRadius, 0.0f);
                        break;
                }

                tlResolution.OnCheck();
                trResolution.OnCheck();
                brResolution.OnCheck();
                blResolution.OnCheck();

                uniformResolution.OnCheck();
            }
        }

        static Vector3 tmpV3 = Vector3.zero;
        static Vector3 tmpPos = Vector3.zero;
        static Vector2 tmpUV = Vector2.zero;

        public static void AddRoundedRect(
            ref VertexHelper vh,
            Vector2 center,
            float width,
            float height,
            RoundedProperties roundedProperties,
            Color32 color,
            Vector2 uv,
            ref RoundedCornerUnitPositionData cornerUnitPositions,
            GeoUtils.EdgeGradientData edgeGradientData
        )
        {
            if (roundedProperties.type == RoundedProperties.RoundedType.None)
            {
                Rects.AddRect(
                    ref vh,
                    center,
                    width,
                    height,
                    color,
                    edgeGradientData
                );

                return;
            }


            SetCornerUnitPositions(
                roundedProperties,
                ref cornerUnitPositions
            );

            int numVertices = vh.currentVertCount;

            tmpUV.x = 0.5f;
            tmpUV.y = 0.5f;

            vh.AddVert(center, color, tmpUV, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

            float sizeSub = Mathf.Min(height, width);
            sizeSub *= 1.0f - edgeGradientData.InnerScale;

            AddRoundedRectVerticesRing(
                ref vh,
                center,
                width - sizeSub,
                height - sizeSub,
                width - sizeSub,
                height - sizeSub,
                roundedProperties.AdjustedTLRadius * edgeGradientData.InnerScale,
                (roundedProperties.AdjustedTLRadius + edgeGradientData.ShadowOffset) * edgeGradientData.InnerScale,
                roundedProperties.AdjustedTRRadius * edgeGradientData.InnerScale,
                (roundedProperties.AdjustedTRRadius + edgeGradientData.ShadowOffset) * edgeGradientData.InnerScale,
                roundedProperties.AdjustedBRRadius * edgeGradientData.InnerScale,
                (roundedProperties.AdjustedBRRadius + edgeGradientData.ShadowOffset) * edgeGradientData.InnerScale,
                roundedProperties.AdjustedBLRadius * edgeGradientData.InnerScale,
                (roundedProperties.AdjustedBLRadius + edgeGradientData.ShadowOffset) * edgeGradientData.InnerScale,
                cornerUnitPositions,
                color,
                uv,
                false
            );


            // set indices
            int numNewVertices = vh.currentVertCount - numVertices;
            for (int i = 0; i < numNewVertices - 1; i++)
            {
                vh.AddTriangle(numVertices, numVertices + i, numVertices + i + 1);
            }

            // set last triangle
            vh.AddTriangle(numVertices, vh.currentVertCount - 1, numVertices + 1);


            if (edgeGradientData.IsActive)
            {
                float outerRadiusMod = 0.0f; // = roundedProperties.AdjustedRadius;
                outerRadiusMod += edgeGradientData.ShadowOffset;
                outerRadiusMod += edgeGradientData.SizeAdd;

                color.a = 0;

                AddRoundedRectVerticesRing(
                    ref vh,
                    center,
                    width,
                    height,
                    width,
                    height,
                    roundedProperties.AdjustedTLRadius,
                    roundedProperties.AdjustedTLRadius + outerRadiusMod,
                    roundedProperties.AdjustedTRRadius,
                    roundedProperties.AdjustedTRRadius + outerRadiusMod,
                    roundedProperties.AdjustedBRRadius,
                    roundedProperties.AdjustedBRRadius + outerRadiusMod,
                    roundedProperties.AdjustedBLRadius,
                    roundedProperties.AdjustedBLRadius + outerRadiusMod,
                    cornerUnitPositions,
                    color,
                    uv,
                    true
                );
            }
        }

        public static void AddRoundedRectLine(
            ref VertexHelper vh,
            Vector2 center,
            float width,
            float height,
            OutlineProperties outlineProperties,
            RoundedProperties roundedProperties,
            Color32 color,
            Vector2 uv,
            ref RoundedCornerUnitPositionData cornerUnitPositions,
            GeoUtils.EdgeGradientData edgeGradientData
        )
        {
            float fullWidth = width + outlineProperties.GetOuterDistance() * 2.0f;
            float fullHeight = height + outlineProperties.GetOuterDistance() * 2.0f;

            if (roundedProperties.type == RoundedProperties.RoundedType.None)
            {
                Rects.AddRectRing(
                    ref vh,
                    outlineProperties,
                    center,
                    width,
                    height,
                    color,
                    uv,
                    edgeGradientData
                );

                return;
            }

            SetCornerUnitPositions(
                roundedProperties,
                ref cornerUnitPositions
            );

            float outerRadiusMod;

            byte alpha = color.a;

            if (edgeGradientData.IsActive)
            {
                color.a = 0;

                outerRadiusMod =
                    outlineProperties.GetCenterDistance() - outlineProperties.HalfLineWeight -
                    edgeGradientData.ShadowOffset;
                outerRadiusMod -= edgeGradientData.SizeAdd;

                AddRoundedRectVerticesRing(
                    ref vh,
                    center,
                    width,
                    height,
                    fullWidth,
                    fullHeight,
                    roundedProperties.AdjustedTLRadius,
                    roundedProperties.AdjustedTLRadius + outerRadiusMod,
                    roundedProperties.AdjustedTRRadius,
                    roundedProperties.AdjustedTRRadius + outerRadiusMod,
                    roundedProperties.AdjustedBRRadius,
                    roundedProperties.AdjustedBRRadius + outerRadiusMod,
                    roundedProperties.AdjustedBLRadius,
                    roundedProperties.AdjustedBLRadius + outerRadiusMod,
                    cornerUnitPositions,
                    color,
                    uv,
                    false
                );

                color.a = alpha;
            }

            outerRadiusMod =
                Mathf.LerpUnclamped(
                    outlineProperties.GetCenterDistance(),
                    outlineProperties.GetCenterDistance() - outlineProperties.HalfLineWeight -
                    edgeGradientData.ShadowOffset,
                    edgeGradientData.InnerScale);

            AddRoundedRectVerticesRing(
                ref vh,
                center,
                width,
                height,
                fullWidth,
                fullHeight,
                roundedProperties.AdjustedTLRadius,
                roundedProperties.AdjustedTLRadius + outerRadiusMod,
                roundedProperties.AdjustedTRRadius,
                roundedProperties.AdjustedTRRadius + outerRadiusMod,
                roundedProperties.AdjustedBRRadius,
                roundedProperties.AdjustedBRRadius + outerRadiusMod,
                roundedProperties.AdjustedBLRadius,
                roundedProperties.AdjustedBLRadius + outerRadiusMod,
                cornerUnitPositions,
                color,
                uv,
                edgeGradientData.IsActive
            );

            outerRadiusMod =
                outlineProperties.GetCenterDistance() +
                (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset) * edgeGradientData.InnerScale;

            AddRoundedRectVerticesRing(
                ref vh,
                center,
                width,
                height,
                fullWidth,
                fullHeight,
                roundedProperties.AdjustedTLRadius,
                roundedProperties.AdjustedTLRadius + outerRadiusMod,
                roundedProperties.AdjustedTRRadius,
                roundedProperties.AdjustedTRRadius + outerRadiusMod,
                roundedProperties.AdjustedBRRadius,
                roundedProperties.AdjustedBRRadius + outerRadiusMod,
                roundedProperties.AdjustedBLRadius,
                roundedProperties.AdjustedBLRadius + outerRadiusMod,
                cornerUnitPositions,
                color,
                uv,
                true
            );

            if (edgeGradientData.IsActive)
            {
                outerRadiusMod =
                    outlineProperties.GetCenterDistance() +
                    outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset;
                outerRadiusMod += edgeGradientData.SizeAdd;

                color.a = 0;

                AddRoundedRectVerticesRing(
                    ref vh,
                    center,
                    width,
                    height,
                    fullWidth,
                    fullHeight,
                    roundedProperties.AdjustedTLRadius,
                    roundedProperties.AdjustedTLRadius + outerRadiusMod,
                    roundedProperties.AdjustedTRRadius,
                    roundedProperties.AdjustedTRRadius + outerRadiusMod,
                    roundedProperties.AdjustedBRRadius,
                    roundedProperties.AdjustedBRRadius + outerRadiusMod,
                    roundedProperties.AdjustedBLRadius,
                    roundedProperties.AdjustedBLRadius + outerRadiusMod,
                    cornerUnitPositions,
                    color,
                    uv,
                    true
                );
            }
        }

        static void AddRoundedRectVerticesRing(
            ref VertexHelper vh,
            Vector2 center,
            float width,
            float height,
            float fullWidth,
            float fullHeight,
            float tlRadius,
            float tlOuterRadius,
            float trRadius,
            float trOuterRadius,
            float brRadius,
            float brOuterRadius,
            float blRadius,
            float blOuterRadius,
            RoundedCornerUnitPositionData cornerUnitPositions,
            Color32 color,
            Vector2 uv,
            bool addIndices
        )
        {
            float xMin = center.x - width * 0.5f;
            float yMin = center.y - height * 0.5f;

            float xMax = center.x + width * 0.5f;
            float yMax = center.y + height * 0.5f;

            float xMinUV = center.x - fullWidth * 0.5f;
            float yMinUV = center.y - fullHeight * 0.5f;

            // TR
            tmpV3.x = xMax - trRadius;
            tmpV3.y = yMax - trRadius;

            if (trOuterRadius < 0.0f)
            {
                tmpV3.x += trOuterRadius;
                tmpV3.y += trOuterRadius;

                trOuterRadius = 0.0f;
            }

            for (int i = 0; i < cornerUnitPositions.TRUnitPositions.Length; i++)
            {
                tmpPos.x = tmpV3.x + cornerUnitPositions.TRUnitPositions[i].x * trOuterRadius;
                tmpPos.y = tmpV3.y + cornerUnitPositions.TRUnitPositions[i].y * trOuterRadius;
                tmpPos.z = tmpV3.z;

                tmpUV.x = (tmpPos.x - xMinUV) / fullWidth;
                tmpUV.y = (tmpPos.y - yMinUV) / fullHeight;

                vh.AddVert(tmpPos, color, tmpUV, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);
            }

            // BR
            tmpV3.x = xMax - brRadius;
            tmpV3.y = yMin + brRadius;

            if (brOuterRadius < 0.0f)
            {
                tmpV3.x += brOuterRadius;
                tmpV3.y -= brOuterRadius;

                brOuterRadius = 0.0f;
            }

            for (int i = 0; i < cornerUnitPositions.BRUnitPositions.Length; i++)
            {
                tmpPos.x = tmpV3.x + cornerUnitPositions.BRUnitPositions[i].x * brOuterRadius;
                tmpPos.y = tmpV3.y + cornerUnitPositions.BRUnitPositions[i].y * brOuterRadius;
                tmpPos.z = tmpV3.z;

                tmpUV.x = (tmpPos.x - xMinUV) / fullWidth;
                tmpUV.y = (tmpPos.y - yMinUV) / fullHeight;

                vh.AddVert(tmpPos, color, tmpUV, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);
            }

            // BL
            tmpV3.x = xMin + blRadius;
            tmpV3.y = yMin + blRadius;

            if (blOuterRadius < 0.0f)
            {
                tmpV3.x -= blOuterRadius;
                tmpV3.y -= blOuterRadius;

                blOuterRadius = 0.0f;
            }

            for (int i = 0; i < cornerUnitPositions.BLUnitPositions.Length; i++)
            {
                tmpPos.x = tmpV3.x + cornerUnitPositions.BLUnitPositions[i].x * blOuterRadius;
                tmpPos.y = tmpV3.y + cornerUnitPositions.BLUnitPositions[i].y * blOuterRadius;
                tmpPos.z = tmpV3.z;

                tmpUV.x = (tmpPos.x - xMinUV) / fullWidth;
                tmpUV.y = (tmpPos.y - yMinUV) / fullHeight;

                vh.AddVert(tmpPos, color, tmpUV, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);
            }

            // TL
            tmpV3.x = xMin + tlRadius;
            tmpV3.y = yMax - tlRadius;

            if (tlOuterRadius < 0.0f)
            {
                tmpV3.x -= tlOuterRadius;
                tmpV3.y += tlOuterRadius;

                tlOuterRadius = 0.0f;
            }

            for (int i = 0; i < cornerUnitPositions.TLUnitPositions.Length; i++)
            {
                tmpPos.x = tmpV3.x + cornerUnitPositions.TLUnitPositions[i].x * tlOuterRadius;
                tmpPos.y = tmpV3.y + cornerUnitPositions.TLUnitPositions[i].y * tlOuterRadius;
                tmpPos.z = tmpV3.z;

                tmpUV.x = (tmpPos.x - xMinUV) / fullWidth;
                tmpUV.y = (tmpPos.y - yMinUV) / fullHeight;

                vh.AddVert(tmpPos, color, tmpUV, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);
            }


            // add last circle vertex
            tmpPos.x = tmpV3.x + cornerUnitPositions.TRUnitPositions[0].x * tlOuterRadius;
            tmpPos.y = tmpV3.y + cornerUnitPositions.TRUnitPositions[0].y * tlOuterRadius;
            tmpPos.z = tmpV3.z;

            tmpUV.x = (tmpPos.x - xMinUV) / fullWidth;
            tmpUV.y = (tmpPos.y - yMinUV) / fullHeight;

            vh.AddVert(tmpPos, color, tmpUV, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);


            if (addIndices)
            {
                AddRoundedRingIndices(
                    ref vh,
                    cornerUnitPositions
                );
            }
        }

        static void AddRoundedRingIndices(
            ref VertexHelper vh,
            RoundedCornerUnitPositionData cornerUnitPositions
        )
        {
            int totalResolution =
                cornerUnitPositions.TLUnitPositions.Length +
                cornerUnitPositions.TRUnitPositions.Length +
                cornerUnitPositions.BRUnitPositions.Length +
                cornerUnitPositions.BLUnitPositions.Length;

            int numNewVertices = totalResolution + 1;

            int innerStartIndex = vh.currentVertCount - numNewVertices - numNewVertices - 1;
            int outerStartIndex = vh.currentVertCount - numNewVertices;

            for (int i = 0; i < totalResolution; i++)
            {
                vh.AddTriangle(innerStartIndex + i + 1, outerStartIndex + i, outerStartIndex + i + 1);
                vh.AddTriangle(innerStartIndex + i + 1, outerStartIndex + i + 1, innerStartIndex + i + 2);
            }

            vh.AddTriangle(innerStartIndex + 1, outerStartIndex + totalResolution, outerStartIndex);
            vh.AddTriangle(innerStartIndex + 1, outerStartIndex - 1, outerStartIndex + totalResolution);
        }
    }
}
