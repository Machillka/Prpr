using Framework.Core.Service;
using UnityEngine;

namespace Prpr.Services
{
    [Service]
    public class InputService : MonoBehaviour
    {
        private MainInputs _inputActions;
        private MainInputs.PlayerActions _playerActions;

        public Vector2 playerInputs;
        public bool IsJumpPressed => _playerActions.Jump.WasPressedThisFrame();
        public bool IsDashPressed => _playerActions.Dash.WasPressedThisFrame();

        private void Awake()
        {
            _inputActions = new MainInputs();
            _playerActions = _inputActions.Player;
        }

        private void Update()
        {
            playerInputs = _playerActions.Move.ReadValue<Vector2>();
        }

        private void OnEnable()
        {
            _inputActions.Enable();
        }

        private void OnDisable()
        {
            _inputActions.Disable();
        }
    }
}

