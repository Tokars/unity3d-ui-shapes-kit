//#define CENTER_ROUNDED_CAPS

using UIShapeKit.Prop;
using UnityEngine;
using UnityEngine.UI;
using RoundingProperties = UIShapeKit.Prop.RoundingProperties;

namespace UIShapeKit.ShapeUtils
{
    public class Lines
    {
        private static Vector3 s_tmpPos = Vector3.zero;
        private static Vector2 s_tmpPos2 = Vector2.zero;

        [System.Serializable]
        public class LineProperties
        {
            public enum LineCapTypes
            {
                Close,
                Projected,
                Round
            }

            [SerializeField] public LineCapTypes lineCap = LineCapTypes.Close;
            [SerializeField] public bool closed = false;
            [SerializeField] public RoundingProperties roundedCapResolution = new();

            public void OnCheck()
            {
                roundedCapResolution.OnCheck(1);
            }
        }

        public static void AddLine(
            ref VertexHelper vh,
            LineProperties lineProperties,
            PointsList.PointListProperties pointListProperties,
            Vector2 positionOffset,
            OutlineProperties outlineProperties,
            Color32 color,
            Vector2 uv,
            ref PointsList.PointsData pointsData,
            GeoUtils.EdgeGradientData edgeGradientData
        )
        {
            pointListProperties.SetPoints();
            pointsData.IsClosed = lineProperties.closed && pointListProperties.positions.Length > 2;

            pointsData.GenerateRoundedCaps = lineProperties.lineCap == LineProperties.LineCapTypes.Round;

            pointsData.LineWeight = outlineProperties.lineWeight;

            if (pointsData.GenerateRoundedCaps)
            {
                lineProperties.roundedCapResolution.UpdateAdjusted(outlineProperties.HalfLineWeight, 0.0f, 2.0f);
                pointsData.RoundedCapResolution = lineProperties.roundedCapResolution.AdjustedResolution;
            }

            if (!PointsList.SetLineData(pointListProperties, ref pointsData))
            {
                return;
            }


            // scale uv x for caps
            float uvXMin = 0.0f;
            float uvXLength = 1.0f;

            if (
                !lineProperties.closed &&
                lineProperties.lineCap != LineProperties.LineCapTypes.Close
            )
            {
                float uvStartOffset = outlineProperties.lineWeight / pointsData.TotalLength;

                uvXMin = uvStartOffset * 0.5f;
                uvXLength = 1.0f - uvXMin * 2.0f;
            }

            float innerOffset = outlineProperties.GetCenterDistance() -
                                (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset) *
                                edgeGradientData.InnerScale;
            float outerOffset = outlineProperties.GetCenterDistance() +
                                (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset) *
                                edgeGradientData.InnerScale;

            float capOffsetAmount = 0.0f;

            if (!lineProperties.closed && lineProperties.lineCap == LineProperties.LineCapTypes.Close)
            {
                capOffsetAmount = edgeGradientData.ShadowOffset * (edgeGradientData.InnerScale * 2.0f - 1.0f);
            }


            int numVertices = vh.currentVertCount;
            int startVertex = numVertices - 1;
            int baseIndex;

            uv.x = uvXMin + pointsData.NormalizedPositionDistances[0] * uvXLength;
            uv.y = 0.0f;

            {
                s_tmpPos.x = positionOffset.x + pointsData.Positions[0].x +
                           pointsData.PositionNormals[0].x * innerOffset +
                           pointsData.StartCapOffset.x * capOffsetAmount;
                s_tmpPos.y = positionOffset.y + pointsData.Positions[0].y +
                           pointsData.PositionNormals[0].y * innerOffset +
                           pointsData.StartCapOffset.y * capOffsetAmount;
            }

            vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

            uv.y = 1.0f;

            {
                s_tmpPos.x = positionOffset.x + pointsData.Positions[0].x +
                           pointsData.PositionNormals[0].x * outerOffset +
                           pointsData.StartCapOffset.x * capOffsetAmount;
                s_tmpPos.y = positionOffset.y + pointsData.Positions[0].y +
                           pointsData.PositionNormals[0].y * outerOffset +
                           pointsData.StartCapOffset.y * capOffsetAmount;
            }

            vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

            for (int i = 1; i < pointsData.NumPositions - 1; i++)
            {
                uv.x = uvXMin + pointsData.NormalizedPositionDistances[i] * uvXLength;
                uv.y = 0.0f;

                {
                    s_tmpPos.x = positionOffset.x + pointsData.Positions[i].x +
                               pointsData.PositionNormals[i].x * innerOffset;
                    s_tmpPos.y = positionOffset.y + pointsData.Positions[i].y +
                               pointsData.PositionNormals[i].y * innerOffset;
                }

                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                uv.y = 1.0f;

                {
                    s_tmpPos.x = positionOffset.x + pointsData.Positions[i].x +
                               pointsData.PositionNormals[i].x * outerOffset;
                    s_tmpPos.y = positionOffset.y + pointsData.Positions[i].y +
                               pointsData.PositionNormals[i].y * outerOffset;
                }

                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                baseIndex = startVertex + i * 2;
                vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
                vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
            }

            // add end vertices
            int endIndex = pointsData.NumPositions - 1;
            uv.x = uvXMin + pointsData.NormalizedPositionDistances[endIndex] * uvXLength;
            uv.y = 0.0f;

            {
                s_tmpPos.x = positionOffset.x + pointsData.Positions[endIndex].x +
                           pointsData.PositionNormals[endIndex].x * innerOffset +
                           pointsData.EndCapOffset.x * capOffsetAmount;
                s_tmpPos.y = positionOffset.y + pointsData.Positions[endIndex].y +
                           pointsData.PositionNormals[endIndex].y * innerOffset +
                           pointsData.EndCapOffset.y * capOffsetAmount;
            }

            vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

            uv.y = 1.0f;

            {
                s_tmpPos.x = positionOffset.x + pointsData.Positions[endIndex].x +
                           pointsData.PositionNormals[endIndex].x * outerOffset +
                           pointsData.EndCapOffset.x * capOffsetAmount;
                s_tmpPos.y = positionOffset.y + pointsData.Positions[endIndex].y +
                           pointsData.PositionNormals[endIndex].y * outerOffset +
                           pointsData.EndCapOffset.y * capOffsetAmount;
            }

            vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

            baseIndex = startVertex + endIndex * 2;
            vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
            vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);

            if (lineProperties.closed)
            {
                uv.x = 1.0f;
                uv.y = 0.0f;

                {
                    s_tmpPos.x = positionOffset.x + pointsData.Positions[0].x +
                               pointsData.PositionNormals[0].x * innerOffset +
                               pointsData.StartCapOffset.x * capOffsetAmount;
                    s_tmpPos.y = positionOffset.y + pointsData.Positions[0].y +
                               pointsData.PositionNormals[0].y * innerOffset +
                               pointsData.StartCapOffset.y * capOffsetAmount;
                }

                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                uv.y = 1.0f;

                {
                    s_tmpPos.x = positionOffset.x + pointsData.Positions[0].x +
                               pointsData.PositionNormals[0].x * outerOffset +
                               pointsData.StartCapOffset.x * capOffsetAmount;
                    s_tmpPos.y = positionOffset.y + pointsData.Positions[0].y +
                               pointsData.PositionNormals[0].y * outerOffset +
                               pointsData.StartCapOffset.y * capOffsetAmount;
                }

                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                baseIndex = startVertex + endIndex * 2 + 2;
                vh.AddTriangle(baseIndex - 1, baseIndex, baseIndex + 1);
                vh.AddTriangle(baseIndex, baseIndex + 2, baseIndex + 1);
            }

            if (edgeGradientData.IsActive)
            {
                byte colorAlpha = color.a;

                innerOffset = outlineProperties.GetCenterDistance() -
                              (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset);
                outerOffset = outlineProperties.GetCenterDistance() +
                              (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset);

                innerOffset -= edgeGradientData.SizeAdd;
                outerOffset += edgeGradientData.SizeAdd;

                color.a = 0;

                int outerBaseIndex = numVertices + pointsData.NumPositions * 2;

                if (lineProperties.closed)
                    outerBaseIndex += 2;

                uv.x = uvXMin + pointsData.NormalizedPositionDistances[0] * uvXLength;
                uv.y = 0.0f;

                {
                    s_tmpPos.x = positionOffset.x + pointsData.Positions[0].x +
                               pointsData.PositionNormals[0].x * innerOffset;
                    s_tmpPos.y = positionOffset.y + pointsData.Positions[0].y +
                               pointsData.PositionNormals[0].y * innerOffset;
                }

                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                uv.y = 1.0f;

                {
                    s_tmpPos.x = positionOffset.x + pointsData.Positions[0].x +
                               pointsData.PositionNormals[0].x * outerOffset;
                    s_tmpPos.y = positionOffset.y + pointsData.Positions[0].y +
                               pointsData.PositionNormals[0].y * outerOffset;
                }

                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                for (int i = 1; i < pointsData.NumPositions; i++)
                {
                    uv.x = uvXMin + pointsData.NormalizedPositionDistances[i] * uvXLength;
                    uv.y = 0.0f;

                    {
                        s_tmpPos.x = positionOffset.x + pointsData.Positions[i].x +
                                   pointsData.PositionNormals[i].x * innerOffset;
                        s_tmpPos.y = positionOffset.y + pointsData.Positions[i].y +
                                   pointsData.PositionNormals[i].y * innerOffset;
                    }

                    vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                    uv.y = 1.0f;

                    {
                        s_tmpPos.x = positionOffset.x + pointsData.Positions[i].x +
                                   pointsData.PositionNormals[i].x * outerOffset;
                        s_tmpPos.y = positionOffset.y + pointsData.Positions[i].y +
                                   pointsData.PositionNormals[i].y * outerOffset;
                    }

                    vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                    // inner quad
                    vh.AddTriangle(startVertex + i * 2 - 1, startVertex + i * 2 + 1, outerBaseIndex + i * 2);
                    vh.AddTriangle(startVertex + i * 2 - 1, outerBaseIndex + i * 2, outerBaseIndex + i * 2 - 2);

                    // outer quad
                    vh.AddTriangle(startVertex + i * 2, outerBaseIndex + i * 2 - 1, startVertex + i * 2 + 2);
                    vh.AddTriangle(startVertex + i * 2 + 2, outerBaseIndex + i * 2 - 1, outerBaseIndex + i * 2 + 1);
                }

                if (lineProperties.closed)
                {
                    int lastIndex = pointsData.NumPositions;

                    uv.x = 1.0f;
                    uv.y = 0.0f;

                    {
                        s_tmpPos.x = positionOffset.x + pointsData.Positions[0].x +
                                   pointsData.PositionNormals[0].x * innerOffset;
                        s_tmpPos.y = positionOffset.y + pointsData.Positions[0].y +
                                   pointsData.PositionNormals[0].y * innerOffset;
                    }

                    vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                    uv.y = 1.0f;

                    {
                        s_tmpPos.x = positionOffset.x + pointsData.Positions[0].x +
                                   pointsData.PositionNormals[0].x * outerOffset;
                        s_tmpPos.y = positionOffset.y + pointsData.Positions[0].y +
                                   pointsData.PositionNormals[0].y * outerOffset;
                    }

                    vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                    // inner quad
                    vh.AddTriangle(startVertex + lastIndex * 2 - 1, startVertex + lastIndex * 2 + 1,
                        outerBaseIndex + lastIndex * 2);
                    vh.AddTriangle(startVertex + lastIndex * 2 - 1, outerBaseIndex + lastIndex * 2,
                        outerBaseIndex + lastIndex * 2 - 2);

                    // outer quad
                    vh.AddTriangle(startVertex + lastIndex * 2, outerBaseIndex + lastIndex * 2 - 1,
                        startVertex + lastIndex * 2 + 2);
                    vh.AddTriangle(startVertex + lastIndex * 2 + 2, outerBaseIndex + lastIndex * 2 - 1,
                        outerBaseIndex + lastIndex * 2 + 1);
                }

                color.a = colorAlpha;
            }

            // close line or add caps
            if (!lineProperties.closed)
            {
                AddStartCap(
                    ref vh,
                    lineProperties,
                    positionOffset,
                    outlineProperties,
                    color,
                    uv,
                    uvXMin,
                    uvXLength,
                    pointsData,
                    edgeGradientData
                );

                AddEndCap(
                    ref vh,
                    lineProperties,
                    positionOffset,
                    outlineProperties,
                    color,
                    uv,
                    uvXMin,
                    uvXLength,
                    pointsData,
                    edgeGradientData
                );
            }
        }

        public static void AddStartCap(
            ref VertexHelper vh,
            LineProperties lineProperties,
            Vector2 positionOffset,
            OutlineProperties outlineProperties,
            Color32 color,
            Vector2 uv,
            float uvXMin,
            float uvXLength,
            PointsList.PointsData pointsData,
            GeoUtils.EdgeGradientData edgeGradientData
        )
        {
            int currentVertCount = vh.currentVertCount;
            int startIndex = currentVertCount - pointsData.NumPositions * 2;

            if (edgeGradientData.IsActive)
            {
                startIndex -= pointsData.NumPositions * 2;
            }

            s_tmpPos2.x = positionOffset.x + pointsData.Positions[0].x;
            s_tmpPos2.y = positionOffset.y + pointsData.Positions[0].y;

            switch (lineProperties.lineCap)
            {
                case LineProperties.LineCapTypes.Close:
                    AddCloseCap(
                        ref vh,
                        true,
                        startIndex,
                        s_tmpPos2,
                        pointsData.PositionNormals[0],
                        pointsData.StartCapOffset,
                        0,
                        lineProperties,
                        outlineProperties,
                        color,
                        uv,
                        pointsData,
                        edgeGradientData,
                        currentVertCount
                    );

                    break;
                case LineProperties.LineCapTypes.Projected:
                    AddProjectedCap(
                        ref vh,
                        true,
                        startIndex,
                        s_tmpPos2,
                        pointsData.PositionNormals[0],
                        pointsData.StartCapOffset,
                        0,
                        lineProperties,
                        outlineProperties,
                        color,
                        uv,
                        pointsData,
                        edgeGradientData,
                        currentVertCount
                    );

                    break;
                case LineProperties.LineCapTypes.Round:
                    AddRoundedCap(
                        ref vh,
                        true,
                        startIndex,
                        s_tmpPos2,
                        pointsData.PositionNormals[0],
                        pointsData.StartCapOffset,
                        0,
                        lineProperties,
                        outlineProperties,
                        color,
                        uv,
                        pointsData,
                        edgeGradientData,
                        pointsData.StartCapOffsets,
                        pointsData.StartCapUVs,
                        uvXMin,
                        uvXLength,
                        currentVertCount
                    );
                    break;
            }
        }

        public static void AddEndCap(
            ref VertexHelper vh,
            LineProperties lineProperties,
            Vector2 positionOffset,
            OutlineProperties outlineProperties,
            Color32 color,
            Vector2 uv,
            float uvXMin,
            float uvXLength,
            PointsList.PointsData pointsData,
            GeoUtils.EdgeGradientData edgeGradientData
        )
        {
            int currentVertCount = vh.currentVertCount;
            int startIndex = currentVertCount;

            if (edgeGradientData.IsActive)
            {
                startIndex -= pointsData.NumPositions * 2;
            }

            int lastPositionIndex = pointsData.NumPositions - 1;

            s_tmpPos2.x = positionOffset.x + pointsData.Positions[lastPositionIndex].x;
            s_tmpPos2.y = positionOffset.y + pointsData.Positions[lastPositionIndex].y;

            switch (lineProperties.lineCap)
            {
                case LineProperties.LineCapTypes.Close:

                    startIndex -= 4;

                    AddCloseCap(
                        ref vh,
                        false,
                        startIndex,
                        s_tmpPos2,
                        pointsData.PositionNormals[lastPositionIndex],
                        pointsData.EndCapOffset,
                        1,
                        lineProperties,
                        outlineProperties,
                        color,
                        uv,
                        pointsData,
                        edgeGradientData,
                        currentVertCount
                    );

                    break;
                case LineProperties.LineCapTypes.Projected:

                    startIndex -= 6;

                    AddProjectedCap(
                        ref vh,
                        false,
                        startIndex,
                        s_tmpPos2,
                        pointsData.PositionNormals[lastPositionIndex],
                        pointsData.EndCapOffset,
                        1,
                        lineProperties,
                        outlineProperties,
                        color,
                        uv,
                        pointsData,
                        edgeGradientData,
                        currentVertCount
                    );

                    break;
                case LineProperties.LineCapTypes.Round:
#if CENTER_ROUNDED_CAPS
					startIndex -= pointsData.RoundedCapResolution + 3;
#else
                    startIndex -= pointsData.RoundedCapResolution + 2;
#endif

                    if (edgeGradientData.IsActive)
                    {
                        startIndex -= pointsData.RoundedCapResolution;
                    }

                    AddRoundedCap(
                        ref vh,
                        false,
                        startIndex,
                        s_tmpPos2,
                        pointsData.PositionNormals[lastPositionIndex],
                        pointsData.EndCapOffset,
                        1,
                        lineProperties,
                        outlineProperties,
                        color,
                        uv,
                        pointsData,
                        edgeGradientData,
                        pointsData.EndCapOffsets,
                        pointsData.EndCapUVs,
                        uvXMin,
                        uvXLength,
                        currentVertCount
                    );

                    break;
            }
        }

        public static void AddCloseCap(
            ref VertexHelper vh,
            bool isStart,
            int firstVertIndex,
            Vector2 position,
            Vector2 normal,
            Vector2 capOffset,
            int invertIndices,
            LineProperties lineProperties,
            OutlineProperties outlineProperties,
            Color32 color,
            Vector2 uv,
            PointsList.PointsData pointsData,
            GeoUtils.EdgeGradientData edgeGradientData,
            int currentVertCount
        )
        {
            if (edgeGradientData.IsActive)
            {
                int baseIndex = currentVertCount;

                float innerOffset = outlineProperties.GetCenterDistance() -
                                    (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset) -
                                    edgeGradientData.SizeAdd;
                float outerOffset = outlineProperties.GetCenterDistance() +
                                    (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset) +
                                    edgeGradientData.SizeAdd;

                float capOffsetAmount = edgeGradientData.SizeAdd + edgeGradientData.ShadowOffset;

                color.a = 0;

                uv.y = 0.0f;

                {
                    s_tmpPos.x = position.x + normal.x * innerOffset + capOffset.x * capOffsetAmount;
                    s_tmpPos.y = position.y + normal.y * innerOffset + capOffset.y * capOffsetAmount;
                }

                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                uv.y = 1.0f;

                {
                    s_tmpPos.x = position.x + normal.x * outerOffset + capOffset.x * capOffsetAmount;
                    s_tmpPos.y = position.y + normal.y * outerOffset + capOffset.y * capOffsetAmount;
                }

                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                vh.AddTriangle(firstVertIndex, baseIndex + invertIndices, baseIndex + 1 - invertIndices);
                vh.AddTriangle(firstVertIndex + invertIndices, baseIndex + 1, firstVertIndex + 1 - invertIndices);

                int antiAliasedIndex = firstVertIndex + pointsData.NumPositions * 2;

                if (invertIndices != 0)
                {
                    vh.AddTriangle(firstVertIndex, baseIndex, antiAliasedIndex);
                    vh.AddTriangle(firstVertIndex + 1, antiAliasedIndex + 1, baseIndex + 1);
                }
                else
                {
                    vh.AddTriangle(firstVertIndex, antiAliasedIndex, baseIndex);
                    vh.AddTriangle(firstVertIndex + 1, baseIndex + 1, antiAliasedIndex + 1);
                }
            }
        }

        public static void AddProjectedCap(
            ref VertexHelper vh,
            bool isStart,
            int firstVertIndex,
            Vector2 position,
            Vector2 normal,
            Vector2 capOffset,
            int invertIndices,
            LineProperties lineProperties,
            OutlineProperties outlineProperties,
            Color32 color,
            Vector2 uv,
            PointsList.PointsData pointsData,
            GeoUtils.EdgeGradientData edgeGradientData,
            int currentVertCount
        )
        {
            int baseIndex = currentVertCount;

            if (isStart)
            {
                uv.x = 0.0f;
            }
            else
            {
                uv.x = 1.0f;
            }

            float innerOffset = outlineProperties.GetCenterDistance() -
                                (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset) *
                                edgeGradientData.InnerScale;
            float outerOffset = outlineProperties.GetCenterDistance() +
                                (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset) *
                                edgeGradientData.InnerScale;

            float capOffsetAmount = edgeGradientData.ShadowOffset + outlineProperties.lineWeight * 0.5f;
            capOffsetAmount *= edgeGradientData.InnerScale;

            // add lineWeight to position
            {
                s_tmpPos.x = position.x + normal.x * innerOffset + capOffset.x * capOffsetAmount;
                s_tmpPos.y = position.y + normal.y * innerOffset + capOffset.y * capOffsetAmount;
            }

            uv.y = 0.0f;
            vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

            {
                s_tmpPos.x = position.x + normal.x * outerOffset + capOffset.x * capOffsetAmount;
                s_tmpPos.y = position.y + normal.y * outerOffset + capOffset.y * capOffsetAmount;
            }

            uv.y = 1.0f;
            vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

            vh.AddTriangle(firstVertIndex, baseIndex + invertIndices, baseIndex + 1 - invertIndices);
            vh.AddTriangle(firstVertIndex + invertIndices, baseIndex + 1, firstVertIndex + 1 - invertIndices);

            if (edgeGradientData.IsActive)
            {
                innerOffset = outlineProperties.GetCenterDistance() -
                              (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset) -
                              edgeGradientData.SizeAdd;
                outerOffset = outlineProperties.GetCenterDistance() +
                              (outlineProperties.HalfLineWeight + edgeGradientData.ShadowOffset) +
                              edgeGradientData.SizeAdd;

                capOffsetAmount = outlineProperties.HalfLineWeight + edgeGradientData.SizeAdd +
                                  edgeGradientData.ShadowOffset;

                color.a = 0;

                {
                    s_tmpPos.x = position.x + normal.x * innerOffset + capOffset.x * capOffsetAmount;
                    s_tmpPos.y = position.y + normal.y * innerOffset + capOffset.y * capOffsetAmount;
                }

                uv.y = 0.0f;
                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);


                {
                    s_tmpPos.x = position.x + normal.x * outerOffset + capOffset.x * capOffsetAmount;
                    s_tmpPos.y = position.y + normal.y * outerOffset + capOffset.y * capOffsetAmount;
                }

                uv.y = 1.0f;
                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                int antiAliasedIndex = firstVertIndex + pointsData.NumPositions * 2;
                baseIndex += 2;

                if (invertIndices != 0)
                {
                    vh.AddTriangle(firstVertIndex, baseIndex, antiAliasedIndex);
                    vh.AddTriangle(firstVertIndex + 1, antiAliasedIndex + 1, baseIndex + 1);

                    vh.AddTriangle(baseIndex - 2, baseIndex - 1, baseIndex);
                    vh.AddTriangle(baseIndex + 1, baseIndex, baseIndex - 1);

                    vh.AddTriangle(firstVertIndex, baseIndex - 2, baseIndex);
                    vh.AddTriangle(firstVertIndex + 1, baseIndex + 1, baseIndex - 1);
                }
                else
                {
                    vh.AddTriangle(firstVertIndex, antiAliasedIndex, baseIndex);
                    vh.AddTriangle(firstVertIndex + 1, baseIndex + 1, antiAliasedIndex + 1);

                    vh.AddTriangle(baseIndex - 2, baseIndex, baseIndex - 1);
                    vh.AddTriangle(baseIndex + 1, baseIndex - 1, baseIndex);

                    vh.AddTriangle(firstVertIndex, baseIndex, baseIndex - 2);
                    vh.AddTriangle(firstVertIndex + 1, baseIndex - 1, baseIndex + 1);
                }
            }
        }

        public static void AddRoundedCap(
            ref VertexHelper vh,
            bool isStart,
            int firstVertIndex,
            Vector2 position,
            Vector2 normal,
            Vector2 capOffset,
            int invertIndices,
            LineProperties lineProperties,
            OutlineProperties outlineProperties,
            Color32 color,
            Vector2 uv,
            PointsList.PointsData pointsData,
            GeoUtils.EdgeGradientData edgeGradientData,
            Vector2[] capOffsets,
            Vector2[] uvOffsets,
            float uvXMin,
            float uvXLength,
            int currentVertCount
        )
        {
            int baseIndex = currentVertCount;

            float innerOffset = outlineProperties.GetCenterDistance();
            float capOffsetAmount = (edgeGradientData.ShadowOffset + outlineProperties.HalfLineWeight) *
                                    edgeGradientData.InnerScale;

            if (isStart)
            {
                uv.x = uvXMin;
            }
            else
            {
                uv.x = uvXMin + uvXLength;
            }

#if CENTER_ROUNDED_CAPS
			// add center vert
			tmpPos.x = position.x;
			tmpPos.y = position.y;
			uv.y = 0.5f;

			vh.AddVert(tmpPos, color, uv, UI.GeoUtils.ZeroV2, UI.GeoUtils.UINormal, UI.GeoUtils.UITangent);
#endif

            for (int i = 0; i < capOffsets.Length; i++)
            {
                {
                    s_tmpPos.x = position.x + normal.x * innerOffset + capOffsets[i].x * capOffsetAmount;
                    s_tmpPos.y = position.y + normal.y * innerOffset + capOffsets[i].y * capOffsetAmount;
                }

                if (isStart)
                {
                    uv.x = Mathf.LerpUnclamped(uvXMin, 0.0f, uvOffsets[i].x);
                }
                else
                {
                    uv.x = Mathf.LerpUnclamped(uvXMin + uvXLength, 1.0f, uvOffsets[i].x);
                }

                uv.y = uvOffsets[i].y;

                vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                if (i > 0)
                {
#if CENTER_ROUNDED_CAPS
					vh.AddTriangle(baseIndex, baseIndex + i - 1, baseIndex + i);
#else
                    vh.AddTriangle(firstVertIndex, baseIndex + i - 1, baseIndex + i);
#endif
                }
            }

            // last fans
            if (isStart)
            {
#if CENTER_ROUNDED_CAPS
				// starting triangle
				vh.AddTriangle(baseIndex + 1, baseIndex, firstVertIndex);
				
				// end triangles
				vh.AddTriangle(baseIndex, baseIndex + capOffsets.Length - 1, baseIndex + capOffsets.Length);
				vh.AddTriangle(baseIndex, baseIndex + capOffsets.Length, firstVertIndex + 1);
#else
                vh.AddTriangle(baseIndex + capOffsets.Length - 1, firstVertIndex + 1, firstVertIndex);
#endif
            }
            else
            {
#if CENTER_ROUNDED_CAPS
				// starting triangle
				vh.AddTriangle(baseIndex + 1, baseIndex, firstVertIndex + 1);

				// end triangles
				vh.AddTriangle(baseIndex, baseIndex + capOffsets.Length - 1, baseIndex + capOffsets.Length);
				vh.AddTriangle(baseIndex, baseIndex + capOffsets.Length, firstVertIndex);
#else
                vh.AddTriangle(baseIndex, firstVertIndex, firstVertIndex + 1);
#endif
            }

            if (edgeGradientData.IsActive)
            {
                color.a = 0;

                innerOffset = outlineProperties.GetCenterDistance();

                capOffsetAmount = outlineProperties.HalfLineWeight + edgeGradientData.SizeAdd +
                                  edgeGradientData.ShadowOffset;

                int antiAliasedIndex = firstVertIndex + pointsData.NumPositions * 2;

                for (int i = 0; i < capOffsets.Length; i++)
                {
                    {
                        s_tmpPos.x = position.x + normal.x * innerOffset + capOffsets[i].x * capOffsetAmount;
                        s_tmpPos.y = position.y + normal.y * innerOffset + capOffsets[i].y * capOffsetAmount;
                    }

                    if (isStart)
                    {
                        uv.x = Mathf.LerpUnclamped(uvXMin, 0.0f, uvOffsets[i].x);
                    }
                    else
                    {
                        uv.x = Mathf.LerpUnclamped(uvXMin + uvXLength, 1.0f, uvOffsets[i].x);
                    }

                    uv.y = uvOffsets[i].y;

                    vh.AddVert(s_tmpPos, color, uv, GeoUtils.ZeroV2, GeoUtils.UINormal, GeoUtils.UITangent);

                    if (i > 0)
                    {
                        vh.AddTriangle(baseIndex + i - 1, baseIndex + capOffsets.Length + i - 1, baseIndex + i);
                        vh.AddTriangle(baseIndex + capOffsets.Length + i, baseIndex + i,
                            baseIndex + capOffsets.Length + i - 1);
                    }
                }

                if (!isStart)
                {
                    vh.AddTriangle(baseIndex, firstVertIndex + 1, antiAliasedIndex + 1);
                    vh.AddTriangle(antiAliasedIndex + 1, baseIndex + capOffsets.Length, baseIndex);

                    vh.AddTriangle(baseIndex + capOffsets.Length * 2 - 1, antiAliasedIndex, firstVertIndex);
                    vh.AddTriangle(baseIndex + capOffsets.Length - 1, baseIndex + capOffsets.Length * 2 - 1,
                        firstVertIndex);
                }
                else
                {
                    vh.AddTriangle(firstVertIndex + 1, baseIndex + capOffsets.Length - 1,
                        baseIndex + capOffsets.Length * 2 - 1);
                    vh.AddTriangle(antiAliasedIndex + 1, firstVertIndex + 1, baseIndex + capOffsets.Length * 2 - 1);

                    vh.AddTriangle(antiAliasedIndex, baseIndex, firstVertIndex);
                    vh.AddTriangle(baseIndex + capOffsets.Length, baseIndex, antiAliasedIndex);
                }
            }
        }
    }
}
