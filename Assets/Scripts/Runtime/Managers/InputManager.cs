using Runtime.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Managers
{
    public class InputManager : MonoBehaviour
    {
        private Camera _camera;
        private PlayerInputController _inputController;
        
        private void Awake()
        {
            _inputController = new PlayerInputController();
            _camera = Camera.main;
        }
        
        private void OnEnable()
        {
            _inputController.Blast.Enable();
            _inputController.Blast.Tap.performed += OnTapPerformed;
        }
        
        private void OnDisable()
        {
            _inputController.Blast.Tap.performed -= OnTapPerformed;
            _inputController.Blast.Disable();
        }
        
        private void OnTapPerformed(InputAction.CallbackContext context)
        {
            if (LevelEvents.Instance.IsLevelFinished.Invoke()) return;
            Vector2 screenPos = _inputController.Blast.Position.ReadValue<Vector2>();
            Vector2 worldPos  = _camera.ScreenToWorldPoint(screenPos);
            InputEvents.Instance.OnTap?.Invoke(worldPos);
        }
    }
}