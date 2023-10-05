using UnityEngine;

namespace UIShapeKit
{
    public class GeoUtils
    {
        public struct EdgeGradientData
        {
            public bool IsActive;
            public float InnerScale;
            public float ShadowOffset;
            public float SizeAdd;

            public void SetActiveData(
                float innerScale,
                float shadowOffset,
                float sizeAdd
            )
            {
                IsActive = true;

                InnerScale = innerScale;

                ShadowOffset = shadowOffset;
                SizeAdd = sizeAdd;
            }

            public void Reset()
            {
                IsActive = false;

                InnerScale = 1.0f;

                ShadowOffset = 0.0f;
                SizeAdd = 0.0f;
            }
        }

        public struct UnitPositionData
        {
            public Vector3[] UnitPositions;

            public float LastBaseAngle;
            public float LastDirection;
        }

        public static readonly Vector3 UpV3 = Vector3.up;
        public static readonly Vector3 DownV3 = Vector3.down;
        public static readonly Vector3 LeftV3 = Vector3.left;
        public static readonly Vector3 RightV3 = Vector3.right;

        public static readonly Vector3 ZeroV3 = Vector3.zero;
        public static readonly Vector2 ZeroV2 = Vector2.zero;

        public static readonly Vector3 UINormal = Vector3.back;
        public static readonly Vector4 UITangent = new Vector4(1.0f, 0.0f, 0.0f, -1.0f);

        public static readonly float HalfPI = Mathf.PI * 0.5f;
        public static readonly float TwoPI = Mathf.PI * 2.0f;

        public static float GetAdjustedAntiAliasing(
            Canvas canvas,
            float antiAliasing
        )
        {
            return antiAliasing * (1.0f / canvas.scaleFactor);
        }

        public static void AddOffset(
            ref float width,
            ref float height,
            float offset
        )
        {
            width += offset * 2.0f;
            height += offset * 2.0f;
        }


        public static void SetUnitPositionData(
            ref UnitPositionData unitPositionData,
            int resolution,
            float baseAngle = 0.0f,
            float direction = 1.0f
        )
        {
            bool needsUpdate = false;

            if (
                unitPositionData.UnitPositions == null ||
                unitPositionData.UnitPositions.Length != resolution)
            {
                unitPositionData.UnitPositions = new Vector3[resolution];

                for (int i = 0; i < unitPositionData.UnitPositions.Length; i++)
                {
                    unitPositionData.UnitPositions[i] = ZeroV3;
                }

                needsUpdate = true;
            }

            needsUpdate |=
                baseAngle != unitPositionData.LastBaseAngle ||
                direction != unitPositionData.LastDirection;

            if (needsUpdate)
            {
                float angleIncrement = TwoPI / (float)resolution;
                angleIncrement *= direction;
                float angle;

                for (int i = 0; i < resolution; i++)
                {
                    angle = baseAngle + (angleIncrement * i);

                    unitPositionData.UnitPositions[i].x = Mathf.Sin(angle);
                    unitPositionData.UnitPositions[i].y = Mathf.Cos(angle);
                }

                unitPositionData.LastBaseAngle = baseAngle;
                unitPositionData.LastDirection = direction;
            }
        }

        public static void SetUnitPositions(
            ref Vector2[] positions,
            int resolution,
            float angleOffset = 0.0f,
            float radius = 1.0f
        )
        {
            float angle = angleOffset;
            float angleIncrement = GeoUtils.TwoPI / (float)(resolution);

            bool needsUpdate = false;

            if (
                positions == null ||
                positions.Length != resolution
            )
            {
                positions = new Vector2[resolution];

                needsUpdate = true;
            }

            // check for radius change
            if (!needsUpdate)
            {
                needsUpdate |= (positions[0].x * positions[0].x + positions[0].y * positions[0].y != radius * radius);
            }

            if (needsUpdate)
            {
                for (int i = 0; i < resolution; i++)
                {
                    positions[i].x = Mathf.Sin(angle) * radius;
                    positions[i].y = Mathf.Cos(angle) * radius;

                    angle += angleIncrement;
                }
            }
        }

        public static float RadianAngleDifference(float angle1, float angle2)
        {
            float diff = (angle2 - angle1 + Mathf.PI) % TwoPI - Mathf.PI;
            return diff < -Mathf.PI ? diff + TwoPI : diff;
        }

        public static int SimpleMap(int x, int in_max, int out_max)
        {
            return x * out_max / in_max;
        }

        public static float SimpleMap(float x, float in_max, float out_max)
        {
            return x * out_max / in_max;
        }

        public static float Map(float x, float in_min, float in_max, float out_min, float out_max)
        {
            return (x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;
        }
    }
}
