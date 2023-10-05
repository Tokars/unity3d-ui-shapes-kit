using System.Collections.Generic;
using UIShapeKit.Prop;
using UIShapeKit.ShapeUtils.PointsGenerators;
using UnityEngine;

namespace UIShapeKit.ShapeUtils
{
    public class PointsList
    {
        private static Vector2 s_tmpPos;
        private static Vector2 s_tmpBackV;
        private static Vector2 s_tmpBackNormV;
        private static Vector2 s_tmpForwV;
        private static Vector2 s_tmpForwNormV;
        private static Vector2 s_tmpBackPos;
        private static Vector2 s_tmpForwPos;
        private static List<Vector2> s_tmpCachedPositions = new();

        [System.Serializable]
        public class PointListsProperties
        {
            public PointListsProperties()
            {
                pointListProperties = new[] {new PointListProperties()};
            }

            [SerializeField] public PointListProperties[] pointListProperties;
        }

        [System.Serializable]
        public class PointListProperties
        {
            [SerializeField] public PointListGeneratorData generatorData = new();

            [SerializeField] public Vector2[] positions =
            {
                new Vector2(-20.0f, 0.0f), new Vector2(20.0f, 0.0f), new Vector2(20.0f, -20.0f)
            };

            [SerializeField, Range(0.0f, Mathf.PI)]
            public float maxAngle = 0.2f;

            [SerializeField, MinNum(0.0f)] public float roundingDistance = 0.0f;
            [SerializeField] public RoundingProperties cornerRounding = new();

            [SerializeField] public bool showHandles = true;

            public void SetPoints()
            {
                if (
                    generatorData.needsUpdate &&
                    generatorData.generator != PointListGeneratorData.Generators.Custom
                )
                {
                    PointsGenerator.SetPoints(
                        ref positions,
                        generatorData
                    );
                }

                generatorData.needsUpdate = false;
            }
        }

        [System.Serializable]
        public class PointListGeneratorData
        {
            public enum Generators
            {
                Custom,
                Rect,
                Round,
                RadialGraph,
                LineGraph,
                AngleLine,
                Star,
                Gear
            }

            [SerializeField] public Generators generator = Generators.Custom;
            [SerializeField] public bool needsUpdate = true;


            [SerializeField] public Vector2 center = Vector2.zero;

            [SerializeField, MinNum(1.0f)] public float width = 10.0f;
            [SerializeField, MinNum(1.0f)] public float height = 10.0f;
            [SerializeField, MinNum(1.0f)] public float radius = 10.0f;

            [SerializeField, Range(-1.0f, 1.0f)] public float direction = 1.0f;

            [SerializeField] public float[] floatValues;
            [SerializeField] public float minFloatValue = 0.0f;
            [SerializeField] public float maxFloatValue = 1.0f;

            [SerializeField] public int intStartOffset = 0;
            [SerializeField] public float floatStartOffset = 0.0f;

            [SerializeField] public float length = 1.0f;
            [SerializeField] public float endRadius = 0.0f;
            [SerializeField, MinNum(2)] public int resolution = 10;
            [SerializeField] public bool centerPoint = false;
            [SerializeField] public bool skipLastPosition = false;

            [SerializeField] public float angle = 0.0f;

            [SerializeField] public float innerScaler = 0.8f;
            [SerializeField] public float outerScaler = 0.5f;
        }

        public struct PointsData
        {
            public bool NeedsUpdate;
            public bool IsClosed;

            public List<Vector2> Positions;
            public int NumPositions;

            public Vector2[] PositionTangents;
            public Vector2[] PositionNormals;

            public float TotalLength;
            public float[] PositionDistances;
            public float[] NormalizedPositionDistances;

            public Vector2 StartCapOffset;
            public Vector2 EndCapOffset;

            public bool GenerateRoundedCaps;
            public int RoundedCapResolution;
            public Vector2[] StartCapOffsets;
            public Vector2[] StartCapUVs;

            public Vector2[] EndCapOffsets;
            public Vector2[] EndCapUVs;

            public float LineWeight;
        }

        public static void SetPositions(PointListProperties pointListProperties, ref PointsData lineData)
        {
            if (lineData.Positions == null)
            {
                lineData.Positions = new List<Vector2>(pointListProperties.positions.Length);
            }

            CheckMinPointDistances(
                ref pointListProperties.positions,
                ref s_tmpCachedPositions,
                lineData.LineWeight * 0.5f,
                lineData.IsClosed
            );

            lineData.Positions.Clear();


            int inputNumPositions = s_tmpCachedPositions.Count;

            if (lineData.Positions.Capacity < inputNumPositions)
            {
                lineData.Positions.Capacity = lineData.Positions.Capacity + inputNumPositions + 1;
            }

            // add first position
            if (lineData.IsClosed)
            {
                InterpolatePoints(
                    ref lineData,
                    s_tmpCachedPositions[inputNumPositions - 1],
                    s_tmpCachedPositions[0],
                    s_tmpCachedPositions[1],
                    pointListProperties,
                    0
                );
            }
            else
            {
                lineData.Positions.Add(s_tmpCachedPositions[0]);
            }

            for (int i = 1; i < inputNumPositions - 1; i++)
            {
                InterpolatePoints(
                    ref lineData,
                    s_tmpCachedPositions[i - 1],
                    s_tmpCachedPositions[i],
                    s_tmpCachedPositions[i + 1],
                    pointListProperties,
                    i
                );
            }

            // add end point
            if (lineData.IsClosed)
            {
                InterpolatePoints(
                    ref lineData,
                    s_tmpCachedPositions[inputNumPositions - 2],
                    s_tmpCachedPositions[inputNumPositions - 1],
                    s_tmpCachedPositions[0],
                    pointListProperties,
                    inputNumPositions - 1
                );
            }
            else
            {
                lineData.Positions.Add(s_tmpCachedPositions[inputNumPositions - 1]);
            }

            lineData.NumPositions = lineData.Positions.Count;
        }

        private static void CheckMinPointDistances(
            ref Vector2[] inPositions,
            ref List<Vector2> outPositions,
            float minDistance,
            bool isClosed
        )
        {
            outPositions.Clear();

            if (outPositions.Capacity < inPositions.Length)
                outPositions.Capacity = inPositions.Length;

            float minSqrDistance = minDistance * minDistance;
            float sqrDistance;

            outPositions.Add(inPositions[0]);

            for (int i = 2; i < inPositions.Length; i++)
            {
                s_tmpPos.x = inPositions[i].x - inPositions[i - 1].x;
                s_tmpPos.y = inPositions[i].y - inPositions[i - 1].y;

                sqrDistance = s_tmpPos.x * s_tmpPos.x + s_tmpPos.y * s_tmpPos.y;

                if (sqrDistance < minSqrDistance)
                {
                    s_tmpPos.x *= 0.5f;
                    s_tmpPos.x += inPositions[i - 1].x;

                    s_tmpPos.y *= 0.5f;
                    s_tmpPos.y += inPositions[i - 1].y;

                    outPositions.Add(s_tmpPos);

                    i++;
                }
                else
                {
                    outPositions.Add(inPositions[i - 1]);
                }
            }

            if (!isClosed)
            {
                outPositions.Add(inPositions[^1]);
            }
            else
            {
                s_tmpPos.x = inPositions[^1].x - inPositions[0].x;
                s_tmpPos.y = inPositions[^1].y - inPositions[0].y;

                sqrDistance = s_tmpPos.x * s_tmpPos.x + s_tmpPos.y * s_tmpPos.y;

                if (sqrDistance < minSqrDistance)
                {
                    s_tmpPos.x *= 0.5f;
                    s_tmpPos.x += inPositions[0].x;

                    s_tmpPos.y *= 0.5f;
                    s_tmpPos.y += inPositions[0].y;

                    outPositions[0] = s_tmpPos;
                }
                else
                {
                    outPositions.Add(inPositions[^1]);
                }
            }
        }

        private static void InterpolatePoints(
            ref PointsList.PointsData lineData,
            Vector2 prevPosition,
            Vector2 position,
            Vector2 nextPosition,
            PointListProperties pointListProperties,
            int index
        )
        {
            s_tmpBackV.x = prevPosition.x - position.x;
            s_tmpBackV.y = prevPosition.y - position.y;
            float backLength = Mathf.Sqrt(s_tmpBackV.x * s_tmpBackV.x + s_tmpBackV.y * s_tmpBackV.y);
            s_tmpBackNormV.x = s_tmpBackV.x / backLength;
            s_tmpBackNormV.y = s_tmpBackV.y / backLength;

            s_tmpForwV.x = nextPosition.x - position.x;
            s_tmpForwV.y = nextPosition.y - position.y;
            float forwLength = Mathf.Sqrt(s_tmpForwV.x * s_tmpForwV.x + s_tmpForwV.y * s_tmpForwV.y);
            s_tmpForwNormV.x = s_tmpForwV.x / forwLength;
            s_tmpForwNormV.y = s_tmpForwV.y / forwLength;

            float cos = (s_tmpBackNormV.x * s_tmpForwNormV.x + s_tmpBackNormV.y * s_tmpForwNormV.y);
            float angle = Mathf.Acos(cos);

            // ignore points along straight line
            if (cos <= -0.9999f)
                return;

            if (pointListProperties.roundingDistance > 0.0f)
            {
                AddRoundedPoints(
                    ref lineData,
                    s_tmpBackNormV,
                    position,
                    s_tmpForwNormV,
                    pointListProperties,
                    angle,
                    Mathf.Min(backLength, forwLength) * 0.49f
                );
            }
            else
            {
                if (angle < pointListProperties.maxAngle)
                {
                    lineData.Positions.Add(position + s_tmpBackNormV * 0.5f);
                    lineData.Positions.Add(position + s_tmpForwNormV * 0.5f);
                }
                else
                {
                    lineData.Positions.Add(position);
                }
            }
        }

        private static void AddRoundedPoints(
            ref PointsList.PointsData lineData,
            Vector2 backNormV,
            Vector2 position,
            Vector2 forwNormV,
            PointListProperties pointListProperties,
            float angle,
            float maxDistance
        )
        {
            float roundingDistance = Mathf.Min(maxDistance, pointListProperties.roundingDistance);

            s_tmpBackPos.x = position.x + backNormV.x * roundingDistance;
            s_tmpBackPos.y = position.y + backNormV.y * roundingDistance;

            s_tmpForwPos.x = position.x + forwNormV.x * roundingDistance;
            s_tmpForwPos.y = position.y + forwNormV.y * roundingDistance;

            pointListProperties.cornerRounding.UpdateAdjusted(roundingDistance / 4.0f, 0.0f,
                (GeoUtils.TwoPI - angle) / Mathf.PI);

            float interpolator;
            int resolution = pointListProperties.cornerRounding.AdjustedResolution;
            float resolutionF = (float)pointListProperties.cornerRounding.AdjustedResolution - 1.0f;

            if (lineData.Positions.Capacity < lineData.Positions.Count + resolution)
            {
                lineData.Positions.Capacity = lineData.Positions.Count + resolution;
            }

            for (int i = 0; i < resolution; i++)
            {
                interpolator = (float)i / resolutionF;

                s_tmpPos.x = Mathf.LerpUnclamped(
                    Mathf.LerpUnclamped(s_tmpBackPos.x, position.x, interpolator),
                    Mathf.LerpUnclamped(position.x, s_tmpForwPos.x, interpolator),
                    interpolator
                );

                s_tmpPos.y = Mathf.LerpUnclamped(
                    Mathf.LerpUnclamped(s_tmpBackPos.y, position.y, interpolator),
                    Mathf.LerpUnclamped(position.y, s_tmpForwPos.y, interpolator),
                    interpolator
                );

                lineData.Positions.Add(s_tmpPos);
            }
        }

        public static bool SetLineData(PointListProperties pointListProperties, ref PointsData lineData)
        {
            if (
                pointListProperties.positions == null ||
                pointListProperties.positions.Length <= 1
            )
            {
                return false;
            }

            bool needsUpdate = lineData.NeedsUpdate || lineData.Positions == null;

            if (needsUpdate)
            {
                SetPositions(
                    pointListProperties,
                    ref lineData
                );
            }

            int numPositions = lineData.NumPositions;


            if (
                lineData.PositionNormals == null ||
                lineData.PositionNormals.Length != numPositions
            )
            {
                lineData.PositionTangents = new Vector2[numPositions];
                lineData.PositionNormals = new Vector2[numPositions];
                lineData.PositionDistances = new float[numPositions];
                lineData.NormalizedPositionDistances = new float[numPositions];

                for (int i = 0; i < numPositions; i++)
                {
                    lineData.PositionNormals[i] = GeoUtils.ZeroV2;
                    lineData.PositionTangents[i] = GeoUtils.ZeroV2;
                }

                needsUpdate = true;
            }

            if (needsUpdate)
            {
                int numPositionsMinusOne = numPositions - 1;

                lineData.TotalLength = 0.0f;

                float distance;
                Vector2 lastUnitTangent = GeoUtils.ZeroV2;
                Vector2 currentUnitTangent = GeoUtils.ZeroV2;

                // set data for first point
                if (!lineData.IsClosed)
                {
                    lineData.PositionTangents[0].x = lineData.Positions[0].x - lineData.Positions[1].x;
                    lineData.PositionTangents[0].y = lineData.Positions[0].y - lineData.Positions[1].y;

                    distance = Mathf.Sqrt(
                        lineData.PositionTangents[0].x * lineData.PositionTangents[0].x +
                        lineData.PositionTangents[0].y * lineData.PositionTangents[0].y
                    );

                    lineData.PositionDistances[0] = distance;
                    lineData.TotalLength += distance;

                    lineData.PositionNormals[0].x = lineData.PositionTangents[0].y / distance;
                    lineData.PositionNormals[0].y = -lineData.PositionTangents[0].x / distance;

                    lastUnitTangent.x = -lineData.PositionTangents[0].x / distance;
                    lastUnitTangent.y = -lineData.PositionTangents[0].y / distance;

                    lineData.StartCapOffset.x = -lastUnitTangent.x;
                    lineData.StartCapOffset.y = -lastUnitTangent.y;
                }
                else
                {
                    lastUnitTangent.x = lineData.Positions[0].x - lineData.Positions[numPositionsMinusOne].x;
                    lastUnitTangent.y = lineData.Positions[0].y - lineData.Positions[numPositionsMinusOne].y;

                    distance = Mathf.Sqrt(
                        lastUnitTangent.x * lastUnitTangent.x +
                        lastUnitTangent.y * lastUnitTangent.y
                    );

                    lastUnitTangent.x /= distance;
                    lastUnitTangent.y /= distance;

                    SetPointData(
                        lineData.Positions[0],
                        lineData.Positions[1],
                        ref currentUnitTangent,
                        ref lineData.PositionTangents[0],
                        ref lineData.PositionNormals[0],
                        ref lastUnitTangent,
                        ref lineData.PositionDistances[0]
                    );

                    lineData.TotalLength += lineData.PositionDistances[0];
                }


                for (int i = 1; i < numPositionsMinusOne; i++)
                {
                    SetPointData(
                        lineData.Positions[i],
                        lineData.Positions[i + 1],
                        ref currentUnitTangent,
                        ref lineData.PositionTangents[i],
                        ref lineData.PositionNormals[i],
                        ref lastUnitTangent,
                        ref lineData.PositionDistances[i]
                    );

                    lineData.TotalLength += lineData.PositionDistances[i];
                }

                // set data for last point
                if (!lineData.IsClosed)
                {
                    lineData.PositionTangents[numPositionsMinusOne].x = lineData.Positions[numPositionsMinusOne].x -
                                                                        lineData.Positions[numPositionsMinusOne - 1].x;
                    lineData.PositionTangents[numPositionsMinusOne].y = lineData.Positions[numPositionsMinusOne].y -
                                                                        lineData.Positions[numPositionsMinusOne - 1].y;

                    distance = Mathf.Sqrt(
                        lineData.PositionTangents[numPositionsMinusOne].x *
                        lineData.PositionTangents[numPositionsMinusOne].x +
                        lineData.PositionTangents[numPositionsMinusOne].y *
                        lineData.PositionTangents[numPositionsMinusOne].y
                    );

                    lineData.EndCapOffset.x = lineData.PositionTangents[numPositionsMinusOne].x / distance;
                    lineData.EndCapOffset.y = lineData.PositionTangents[numPositionsMinusOne].y / distance;

                    lineData.PositionNormals[numPositionsMinusOne].x =
                        -lineData.PositionTangents[numPositionsMinusOne].y / distance;
                    lineData.PositionNormals[numPositionsMinusOne].y =
                        lineData.PositionTangents[numPositionsMinusOne].x / distance;
                }
                else
                {
                    SetPointData(
                        lineData.Positions[numPositionsMinusOne],
                        lineData.Positions[0],
                        ref currentUnitTangent,
                        ref lineData.PositionTangents[numPositionsMinusOne],
                        ref lineData.PositionNormals[numPositionsMinusOne],
                        ref lastUnitTangent,
                        ref lineData.PositionDistances[numPositionsMinusOne]
                    );

                    lineData.TotalLength += lineData.PositionDistances[numPositionsMinusOne];
                }


                if (lineData.GenerateRoundedCaps)
                {
                    SetRoundedCapPointData(
                        Mathf.Atan2(-lineData.PositionNormals[0].x, -lineData.PositionNormals[0].y),
                        ref lineData.StartCapOffsets,
                        ref lineData.StartCapUVs,
                        lineData.RoundedCapResolution,
                        true
                    );

                    SetRoundedCapPointData(
                        Mathf.Atan2(lineData.PositionNormals[numPositionsMinusOne].x,
                            lineData.PositionNormals[numPositionsMinusOne].y),
                        ref lineData.EndCapOffsets,
                        ref lineData.EndCapUVs,
                        lineData.RoundedCapResolution,
                        false
                    );
                }

                float accumulatedLength = 0.0f;
                for (int i = 0; i < lineData.PositionDistances.Length; i++)
                {
                    lineData.NormalizedPositionDistances[i] = accumulatedLength / lineData.TotalLength;
                    accumulatedLength += lineData.PositionDistances[i];
                }
            }

            lineData.NeedsUpdate = false;

            return true;
        }

        private static void SetRoundedCapPointData(
            float centerAngle,
            ref Vector2[] offsets,
            ref Vector2[] uvs,
            int resolution,
            bool isStart
        )
        {
            float angleIncrement = Mathf.PI / (float)(resolution + 1);
            float baseAngle = centerAngle;
            float angle;

            if (offsets == null || offsets.Length != resolution)
            {
                offsets = new Vector2[resolution];
                uvs = new Vector2[resolution];
            }

            baseAngle += angleIncrement;

            for (int i = 0; i < resolution; i++)
            {
                angle = baseAngle + (angleIncrement * i);

                offsets[i].x = Mathf.Sin(angle);
                offsets[i].y = Mathf.Cos(angle);

                // set angle for uvs
                angle = angleIncrement * i + Mathf.PI * 0.14f;

                if (isStart)
                {
                    angle += Mathf.PI;
                }

                uvs[i].x = Mathf.Abs(Mathf.Sin(angle));

                uvs[i].y = Mathf.Cos(angle) * 0.5f + 0.5f;
            }
        }

        private static void SetPointData(
            Vector2 currentPoint,
            Vector2 nextPoint,
            ref Vector2 currentUnitTangent,
            ref Vector2 positionTangent,
            ref Vector2 positionNormal,
            ref Vector2 lastUnitTangent,
            ref float distance
        )
        {
            positionTangent.x = currentPoint.x - nextPoint.x;
            positionTangent.y = currentPoint.y - nextPoint.y;

            distance = Mathf.Sqrt(
                positionTangent.x * positionTangent.x +
                positionTangent.y * positionTangent.y
            );

            currentUnitTangent.x = positionTangent.x / distance;
            currentUnitTangent.y = positionTangent.y / distance;

            positionNormal.x = -(lastUnitTangent.x + currentUnitTangent.x);
            positionNormal.y = -(lastUnitTangent.y + currentUnitTangent.y);

            if (positionNormal.x == 0.0f && positionNormal.y == 0.0f)
            {
                positionNormal.x = -lastUnitTangent.y;
                positionNormal.y = lastUnitTangent.x;
            }

            // normalize line normal
            float normalMag = Mathf.Sqrt(
                positionNormal.x * positionNormal.x +
                positionNormal.y * positionNormal.y
            );
            positionNormal.x /= normalMag;
            positionNormal.y /= normalMag;

            float inBetweenAngle = Mathf.Acos(Vector2.Dot(
                lastUnitTangent,
                currentUnitTangent
            )) * 0.5f;

            float angleAdjustedLength = 1.0f / Mathf.Sin(inBetweenAngle);

            if (
                currentUnitTangent.x * positionNormal.y - currentUnitTangent.y * positionNormal.x > 0.0f
            )
            {
                angleAdjustedLength *= -1.0f;
            }

            positionNormal.x *= angleAdjustedLength;
            positionNormal.y *= angleAdjustedLength;

            lastUnitTangent.x = -currentUnitTangent.x;
            lastUnitTangent.y = -currentUnitTangent.y;
        }
    }
}
