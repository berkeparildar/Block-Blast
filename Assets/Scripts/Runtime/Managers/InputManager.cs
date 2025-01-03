using Runtime.Events;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Runtime.Managers
{
    public class InputManager : MonoBehaviour
    {
        private Camera m_Camera;
        
        private PlayerInputController m_InputController;
        
        private void Awake()
        {
            m_InputController = new PlayerInputController();
            m_Camera = Camera.main;
        }
        
        private void OnEnable()
        {
            m_InputController.Blast.Enable();
            m_InputController.Blast.Tap.performed += OnTapPerformed;
        }
        
        private void OnDisable()
        {
            m_InputController.Blast.Tap.performed -= OnTapPerformed;
            m_InputController.Blast.Disable();
        }
        
        private void OnTapPerformed(InputAction.CallbackContext context)
        {
            if (LevelEvents.Instance.IsLevelFinished.Invoke()) return;
            Vector2 screenPos = m_InputController.Blast.Position.ReadValue<Vector2>();
            Vector2 worldPos  = m_Camera.ScreenToWorldPoint(screenPos);
            InputEvents.Instance.OnTap?.Invoke(worldPos);
        }
    }
}