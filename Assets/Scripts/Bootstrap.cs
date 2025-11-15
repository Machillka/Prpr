using UnityEngine;
using Framework.Core.Infra;
using Prpr.Player;

[DefaultExecutionOrder(-1000)]
public class Bootstrap : BootstrapperBase
{
    public PlayerController plt;

    // TODO: 设计一个 by Group -> 内部有一个 Order 的自动执行初始化的方法 (Module)
    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
        plt = FindAnyObjectByType<PlayerController>();
        if (plt == null)
            Debug.LogWarning("Can't find player controller");
        plt.Initialization(ServiceManager);

    }
}
