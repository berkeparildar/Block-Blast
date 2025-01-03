using Runtime.Controllers;
using Runtime.Events;
using UnityEngine;

namespace Runtime.Managers
{
    public class CameraManager : MonoBehaviour
    {
        [SerializeField] private CameraController _cameraController;

        private void OnEnable()
        {
            SubscribeEvents();
        }
        
        private void SubscribeEvents()
        {
            GridEvents.Instance.OnGridSizeSet += _cameraController.CenterCameraOnGrid;
        }
        
        private void UnsubscribeEvents()
        {
            GridEvents.Instance.OnGridSizeSet -= _cameraController.CenterCameraOnGrid;
        }

        private void OnDisable()
        {
            UnsubscribeEvents();
        }
    }
}