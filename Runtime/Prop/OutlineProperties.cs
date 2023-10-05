using UnityEngine;

namespace UIShapeKit.Prop
{
    [System.Serializable]
    public class OutlineProperties
    {
        public enum LineType
        {
            Inner,
            Center,
            Outer
        }

        [SerializeField] public LineType type = LineType.Center;
        [SerializeField] public float lineWeight = 2.0f;

        public float HalfLineWeight { private set; get; }

        public float GetOuterDistance()
        {
            switch (type)
            {
                case LineType.Inner:
                    return 0.0f;

                case LineType.Outer:
                    return lineWeight;

                case LineType.Center:
                    return lineWeight * 0.5f;

                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        public float GetCenterDistance()
        {
            switch (type)
            {
                case LineType.Inner:
                    return lineWeight * -0.5f;

                case LineType.Outer:
                    return lineWeight * 0.5f;

                case LineType.Center:
                    return 0.0f;

                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        public float GetInnerDistance()
        {
            switch (type)
            {
                case LineType.Inner:
                    return -lineWeight;

                case LineType.Outer:
                    return 0.0f;

                case LineType.Center:
                    return lineWeight * -0.5f;

                default:
                    throw new System.ArgumentOutOfRangeException();
            }
        }

        public void OnCheck()
        {
            lineWeight = Mathf.Max(lineWeight, 0.0f);
        }

        public void UpdateAdjusted()
        {
            HalfLineWeight = lineWeight * 0.5f;
        }
    }
}
