using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "ResourceBar", menuName = "Scriptables/Resource Bar", order = 0)]
    public class ResourceBarSO : ScriptableObject
    {
        [Header("Core Settings")] 
        public float resourceCurrent = 100;
        public float resourceDefault = 100;
        public int resourceMax = 100;
        public int resourceAbsoluteMax = 1000;
        [Space] public ShapeType shapeOfBar;
        [Space] public bool increasigByTime = false;

        public enum ShapeType
        {
            [InspectorName("Rectangle (Horizontal)")]
            RectangleHorizontal,

            [InspectorName("Rectangle (Vertical)")]
            RectangleVertical,
            [InspectorName("Circle")] Circle,
            Arc
        }

        [Header("Arc Settings")] [Range(0, 360)]
        public int endDegreeValue = 360;

        [Header("Animation Speed")] [SerializeField, EnumToggleButtons]
        private AnimationSpeed animationSpeed = AnimationSpeed.Medium;

        [Range(0, 0.5f)] public float _animationTime = 0.25f;

        public enum AnimationSpeed
        {
            [InspectorName("0.125s")] Fast,
            [InspectorName("0.25s")] Medium,
            [InspectorName("0.5s")] Slow,
            None
        }

        [Header("Text Settings")] public DisplayType howToDisplayValueText = DisplayType.Percentage;

        public enum DisplayType
        {
            [InspectorName("Long (50|100)")] LongValue,
            [InspectorName("Short (50)")] ShortValue,
            [InspectorName("Percent (85%)")] Percentage,
            None
        }
        
        [Header("Gradient Settings")] 
        public bool useGradient;
        
        [ShowIf("useGradient")]
        public Gradient barGradient;
        
        [HideIf("useGradient")]
        public Color barColor;
    }
}