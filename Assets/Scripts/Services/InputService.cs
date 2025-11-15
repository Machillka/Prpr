using Framework.Core.Service;
using UnityEngine;

namespace Prpr.Services
{
    [Service]
    public class InputService : MonoBehaviour
    {
        private MainInputs _inputActions;
        private MainInputs.PlayerActions _playerActions;

        public Vector2 playerInputs;// => _playerActions.Move.ReadValue<Vector2>();
        public bool isJumpPressed => _playerActions.Jump.WasPressedThisFrame();

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

