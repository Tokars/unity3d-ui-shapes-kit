using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// UI part of CompoundGraphic.
    /// </summary>
    [ExecuteInEditMode]
    public abstract class GraphicComposite : MonoBehaviour
    {
        [SerializeField] protected CompoundGraphic compoundGraphic = null;

#if UNITY_EDITOR
        public abstract void CompoundRefresh();


        protected virtual void OnEnable()
        {
            compoundGraphic = GetComponent<CompoundGraphic>();
        }
#endif
        public abstract void PopulateMesh(ref VertexHelper vh);
    }
}