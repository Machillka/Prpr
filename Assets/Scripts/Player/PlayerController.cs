using Framework.Core.EventSystem;
using Framework.Core.Service;
using Framework.Toolkit.Movement;
using Prpr.Services;
using UnityEngine;

namespace Prpr.Player
{
    public class PlayerController : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private Bootstrap _boot;
        private ServiceLocator _serviceLocator;
        private InputService _inputController;

        [SerializeField]
        private MovementProfileSO _profile;
        private PlayerMovement _moveController;

        public Transform groundCheckTransform;

        /// <summary>
        /// 初始化方法交给外部, 防止因为时序问题出错
        /// </summary>
        /// <param name="serviceLocator">服务定位器缓存</param>
        public void Initialization(ServiceLocator serviceLocator)
        {
            _rb = GetComponent<Rigidbody2D>();
            _serviceLocator = serviceLocator;
            _inputController = _serviceLocator.Resolve<InputService>();
            if (_inputController == null)
            {
                Debug.LogWarning("Input Controller Can't be found");
            }
            if (_profile == null)
            {
                Debug.LogWarning("Player Movement Profile is null!");
            }

            _moveController = new PlayerMovement();
            _moveController.Initialization(_profile, groundCheckTransform, _rb);
            _moveController.EnableMovement(true);
        }

        private void Update()
        {
            // NOTE: 是否需要把所有的输入再进行一次封装？？？ -> 比如封装成一个 "Context"
            _moveController.SetInputs(
                _inputController.playerInputs,
                _inputController.isJumpPressed
            );
            _moveController.UpdateDrive(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            _moveController.FixedUpdateDrive();
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere((Vector2)groundCheckTransform.position + _profile.groundCheckOffset, _profile.groundCheckDistance);
        }
    }

}

