using System;
using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "Outline Settings", menuName = "SO/Outline Settings", order = 359)]
    public class LayerSettings : ScriptableObject
    {
        [SerializeField] private LayerMask m_outlineLayer;
        [SerializeField] private LayerMask m_defaultLayer;
        [SerializeField] private LayerMask m_onTopLayer;
        
        public int outlineLayer => (int) Math.Log(m_outlineLayer, 2);
        public int defaultLayer => (int) Math.Log(m_defaultLayer, 2);
        public int onTopLayer => (int) Math.Log(m_onTopLayer, 2);

    }
}