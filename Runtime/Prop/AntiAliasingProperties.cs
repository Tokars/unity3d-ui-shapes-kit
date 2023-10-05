using UnityEngine;

namespace UIShapeKit.Prop
{
    [System.Serializable]
    public class AntiAliasingProperties
    {
        [SerializeField] public float antiAliasing = 1.25f;

        public float Adjusted { get; private set; }

        public void UpdateAdjusted(Canvas canvas)
        {
            if (canvas != null)
            {
                Adjusted = antiAliasing * (1.0f / canvas.scaleFactor);
            }
            else
            {
                Adjusted = antiAliasing;
            }
        }

        public void OnCheck()
        {
            antiAliasing = Mathf.Max(antiAliasing, 0.0f);
        }
    }
}
