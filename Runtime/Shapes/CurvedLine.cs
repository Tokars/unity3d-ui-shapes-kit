using System.Collections.Generic;
using UI.Bezier;
using UIShapeKit.ShapeUtils;
using UnityEngine;
using UnityEngine.UI;

namespace UIShapeKit.Shapes
{
#if UNITY_EDITOR
    using UnityEditor;

    [CustomEditor(typeof(CurvedLine))]
    public class CurvedLineEditor : Editor
    {
        private CurvedLine _t;

        private void OnEnable()
        {
            _t = (CurvedLine) target;
        }

        private float _positionSliderValue = 0F;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Slide by path:");
            float value = EditorGUILayout.Slider(_positionSliderValue, 0.0F, 1.0F);
            if (_positionSliderValue != value)
            {
                _positionSliderValue = value;
                _t.SetSliderPos(_positionSliderValue);
            }

            base.OnInspectorGUI();
        }
    }

#endif


    [AddComponentMenu("UI/Shapes/Curved Line", 30), RequireComponent(typeof(CanvasRenderer))]
    public class CurvedLine : CustomLine, IShape
    {
        [SerializeField] private GeoUtils.ShapeProperties shapeProperties = new();
        [SerializeField] private PointsList.PointListsProperties pointListsProperties = new();
        [SerializeField] private Lines.LineProperties lineProperties = new();
        [SerializeField] private GeoUtils.OutlineProperties outlineProperties = new();
        [SerializeField] private GeoUtils.ShadowsProperties shadowProperties = new();
        [SerializeField] private GeoUtils.AntiAliasingProperties antiAliasingProperties = new();

        public Sprite sprite;

        PointsList.PointsData[] _pointsListData = {new()};
        GeoUtils.EdgeGradientData _edgeGradientData;

        public void ForceMeshUpdate()
        {
            if (pointListsProperties.PointListProperties.Length == 0)
                pointListsProperties.PointListProperties = new PointsList.PointListProperties[1];

            pointListsProperties.PointListProperties[0].Positions = new Vector2[] { };
            pointListsProperties.PointListProperties[0].Positions = drawPoints.ToArray();

            
            if (_pointsListData == null || _pointsListData.Length != pointListsProperties.PointListProperties.Length)
            {
                System.Array.Resize(ref _pointsListData, pointListsProperties.PointListProperties.Length);
            }
            


            for (int i = 0; i < _pointsListData.Length; i++)
            {
                _pointsListData[i].NeedsUpdate = true;
                pointListsProperties.PointListProperties[i].GeneratorData.NeedsUpdate = true;
            }
            // UpdateGeometry();
            SetVerticesDirty();
            SetMaterialDirty();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            lineProperties.OnCheck();
            outlineProperties.OnCheck();
            antiAliasingProperties.OnCheck();
            // UpdateGeometry();

            ForceMeshUpdate();
        }
#endif

        protected override void UpdateMaterial()
        {
            base.UpdateMaterial();

            // check if this sprite has an associated alpha texture (generated when splitting RGBA = RGB + A as two textures without alpha)

            if (sprite == null)
            {
                canvasRenderer.SetAlphaTexture(null);
                return;
            }

            Texture2D alphaTex = sprite.associatedAlphaSplitTexture;

            if (alphaTex != null)
            {
                canvasRenderer.SetAlphaTexture(alphaTex);
            }
        }

        public override Texture mainTexture
        {
            get
            {
                if (sprite == null)
                {
                    if (material != null && material.mainTexture != null)
                    {
                        return material.mainTexture;
                    }

                    return s_WhiteTexture;
                }

                return sprite.texture;
            }
        }

        public void ApplyPointsData(TransformPoint[] initPos)
        {
            _initialPositions = initPos;
            print($"init points: {_initialPositions.Length}");
            PrepareMeshPoints();
        }

        private float LineLength
        {
            get
            {
                float sum = 0f;

                for (int n = 0; n < _initialPositions.Length - 1; n++)
                {
                    sum += Vector2.Distance(_initialPositions[n].ScreenPosition,
                        _initialPositions[n + 1].ScreenPosition);
                }

                return sum;
            }
        }

        float GetLength(int index)
        {
            if (index <= 0)
            {
                return 0f;
            }

            float sum = 0f;
            for (int n = 0; n < index; n++)
            {
                sum += Vector2.Distance(_initialPositions[n].ScreenPosition, _initialPositions[n + 1].ScreenPosition);
            }

            return sum;
        }

        [Range(0, 1), SerializeField] float m_lengthRatio = 1f;

        public void SetSliderPos(float positionSliderValue)
        {
            LengthRatio = positionSliderValue;
        }

        public float LengthRatio
        {
            get { return m_lengthRatio; }
            set
            {
                m_lengthRatio = value;

                int  index = (int) (Mathf.Abs(detailedPoints.Count - 1) * value);

                print($"- - - index: {index}");
                
                drawPoints = detailedPoints.GetRange(0, index);
                ForceMeshUpdate();
            }
        }


        private TransformPoint[] _initialPositions = { };

        Vector2 EvaluatePoint(int index, float t)
        {
            return Vector2.Lerp(_initialPositions[index].ScreenPosition, _initialPositions[index + 1].ScreenPosition,
                t);
        }

        private void PrepareMeshPoints()
        {
            print("- - - - prepare mesh points - - - -");
            detailedPoints.Clear();
            for (int n = 0; n < _initialPositions.Length - 1; n++)
            {

                if (GetLength(n) / LineLength > m_lengthRatio)
                {
                    break;
                }

                DetailSegment(n);
            }

            ForceMeshUpdate();

            void DetailSegment(int index)
            {
                print($"index: {index}");
                var divideCount = _initialPositions[index].PopulatePointCount;

                detailedPoints.Add(_initialPositions[index].ScreenPosition);
                for (int n = 0; n < divideCount; n++)
                {
                    // Vector3 p0 = EvaluatePoint(index, 1f / divideCount * n);
                    // Vector3 p1 = EvaluatePoint(index, 1f / divideCount * (n + 1));

                    detailedPoints.Add(Vector2.Lerp(_initialPositions[index].ScreenPosition,
                    _initialPositions[index + 1].ScreenPosition,
                    1f / divideCount * (n + 1)));

                    // if (detailedPoints.Contains(p0) == false)
                        // detailedPoints.Add(p0);
                    // if (detailedPoints.Contains(p1) == false)
                        // detailedPoints.Add(p1);
                }

                drawPoints = detailedPoints;
            }
        }


#if UNITY_EDITOR


        private void OnDrawGizmos()
        {
            for (int i = 0; i < detailedPoints.Count; i++)
            {
                // ScreenGizmos.DrawSphere(can, cam, line.CurvePoints[i], 18);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(detailedPoints[i], 16);
            }
        }

#endif

        [SerializeField] private List<Vector2> detailedPoints = new List<Vector2>();

        private List<Vector2> drawPoints = new List<Vector2>();

        protected override void OnPopulateMesh(VertexHelper vh)
		{
			vh.Clear();

			outlineProperties.UpdateAdjusted();
			shadowProperties.UpdateAdjusted();

			if (_pointsListData == null || _pointsListData.Length != pointListsProperties.PointListProperties.Length)
			{
				System.Array.Resize(ref _pointsListData, pointListsProperties.PointListProperties.Length);

				for (int i = 0; i < _pointsListData.Length; i++)
				{
					_pointsListData[i].NeedsUpdate = true;
					pointListsProperties.PointListProperties[i].GeneratorData.NeedsUpdate = true;
				}
			}

			for (int i = 0; i < pointListsProperties.PointListProperties.Length; i++)
				pointListsProperties.PointListProperties[i].SetPoints();

			for (int i = 0; i < pointListsProperties.PointListProperties.Length; i++)
			{
				if (
					pointListsProperties.PointListProperties[i].Positions != null &&
					pointListsProperties.PointListProperties[i].Positions.Length > 0
				) {
					antiAliasingProperties.UpdateAdjusted(canvas);

					// shadows
					if (shadowProperties.ShadowsEnabled)
					{
						for (int j = 0; j < shadowProperties.Shadows.Length; j++)
						{
							_edgeGradientData.SetActiveData(
								1.0f - shadowProperties.Shadows[j].Softness,
								shadowProperties.Shadows[j].Size,
								antiAliasingProperties.Adjusted
							);

							ShapeUtils.Lines.AddLine(
								ref vh,
								lineProperties,
								pointListsProperties.PointListProperties[i],
								shadowProperties.GetCenterOffset(GeoUtils.ZeroV2, j),
								outlineProperties,
								shadowProperties.Shadows[j].Color,
								GeoUtils.ZeroV2,
								ref _pointsListData[i],
								_edgeGradientData
							);
						}
					}
				}
			}

			for (int i = 0; i < pointListsProperties.PointListProperties.Length; i++)
			{
				if (
					pointListsProperties.PointListProperties[i].Positions != null &&
					pointListsProperties.PointListProperties[i].Positions.Length > 0
				) {
					// fill
					if (shadowProperties.ShowShape)
					{
						if (antiAliasingProperties.Adjusted > 0.0f)
						{
							_edgeGradientData.SetActiveData(
								1.0f,
								0.0f,
								antiAliasingProperties.Adjusted
							);
						}
						else
						{
							_edgeGradientData.Reset();
						}

						ShapeUtils.Lines.AddLine(
							ref vh,
							lineProperties,
							pointListsProperties.PointListProperties[i],
							GeoUtils.ZeroV2,
							outlineProperties,
							shapeProperties.FillColor,
							GeoUtils.ZeroV2,
							ref _pointsListData[i],
							_edgeGradientData
						);
					}
				}
			}
		}
    }


    public struct TransformPoint
    {
        public Vector2 ScreenPosition;
        public byte PopulatePointCount;

        public TransformPoint(Vector2 screenPosition, byte populatePointCount)
        {
            ScreenPosition = screenPosition;
            PopulatePointCount = populatePointCount;
        }
    }
}