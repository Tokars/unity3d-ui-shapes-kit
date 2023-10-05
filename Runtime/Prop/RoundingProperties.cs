using UnityEngine;

namespace UIShapeKit.Prop
{
    [System.Serializable]
    public class RoundingProperties
    {
        public enum ResolutionType
        {
            Calculated,
            Fixed
        }

        [SerializeField] public ResolutionType resolution = ResolutionType.Calculated;
        [SerializeField, MinNum(2)] public int fixedResolution = 10;
        [SerializeField, MinNum(0.01f)] public float resolutionMaxDistance = 4.0f;

        public int AdjustedResolution { private set; get; }
        public bool MakeSharpCorner { private set; get; }

        public void OnCheck(int minFixedResolution = 2)
        {
            fixedResolution = Mathf.Max(fixedResolution, minFixedResolution);
            resolutionMaxDistance = Mathf.Max(resolutionMaxDistance, 0.1f);
        }

        public void UpdateAdjusted(float radius, float offset, float numCorners)
        {
            UpdateAdjusted(radius, offset, this, numCorners);
        }

        public void UpdateAdjusted(
            float radius,
            float offset,
            RoundingProperties overrideProperties,
            float numCorners
        )
        {
            MakeSharpCorner = radius < 0.001f;

            radius += offset;

            switch (overrideProperties.resolution)
            {
                case ResolutionType.Calculated:
                    float circumference = GeoUtils.TwoPI * radius;

                    AdjustedResolution =
                        Mathf.CeilToInt(circumference / overrideProperties.resolutionMaxDistance / numCorners);
                    AdjustedResolution = Mathf.Max(AdjustedResolution, 2);
                    break;
                case ResolutionType.Fixed:
                    AdjustedResolution = overrideProperties.fixedResolution;
                    break;
            }
        }
    }
}
