using Runtime.Events;
using UnityEngine;

namespace Runtime.Managers
{
    public class CameraManager : MonoBehaviour
    {
        private const float CameraSpacing = 0.5f;
        private Camera _mCamera;
        
        private void Awake()
        {
            _mCamera = Camera.main;
        }
        
        private void OnEnable()
        {
            SubscribeEvents();
        }
        
        private void SubscribeEvents()
        {
            GridEvents.Instance.OnGridSizeSet += CenterCameraOnGrid;
        }

        private void CenterCameraOnGrid(int columnCount, int rowCount)
        {
            float centerX = (columnCount - 1)  * 0.5f;
            float centerY = (rowCount) * 0.5f; 
            
            _mCamera.transform.position = new Vector3(centerX, centerY, _mCamera.transform.position.z);
            
            float halfHeightNeeded = rowCount * 0.5f;
            float halfWidthNeeded  = columnCount  * 0.5f / _mCamera.aspect;
            
            float requiredSize = Mathf.Max(halfHeightNeeded, halfWidthNeeded);
            _mCamera.orthographicSize = requiredSize + CameraSpacing;
        }
    }
}