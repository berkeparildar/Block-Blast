using UnityEngine;

namespace Runtime.Controllers
{
    public class CameraController : MonoBehaviour
    {
        private const float CAMERA_SPACING = 0.5f;
        private Camera m_Camera;
        private void Awake()
        {
            m_Camera = Camera.main;
        }

        internal void CenterCameraOnGrid(int columnCount, int rowCount)
        {
            float centerX = (columnCount - 1)  * 0.5f;
            float centerY = (rowCount) * 0.5f; 
            
            m_Camera.transform.position = new Vector3(centerX, centerY, m_Camera.transform.position.z);
            
            float halfHeightNeeded = rowCount * 0.5f;
            float halfWidthNeeded  = columnCount  * 0.5f / m_Camera.aspect;
            
            float requiredSize = Mathf.Max(halfHeightNeeded, halfWidthNeeded);
            m_Camera.orthographicSize = requiredSize + CAMERA_SPACING;
        }
    }
}