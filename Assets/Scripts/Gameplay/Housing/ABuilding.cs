using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.Buildings
{
    public class ABuilding : MonoBehaviour
    {
        [SerializeField]
        protected int width;
        [SerializeField]
        protected int depth;
        
        public bool InverseRotation;

        [HideInInspector]
        public Area m_Area;
        protected GameObject m_ColliderPlane;
    }
}
