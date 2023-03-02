using Cinemachine;
using DCL.Camera;
using UnityEngine;
using UnityEngine.Rendering;

namespace DCL
{
    public interface ISceneReferences
    {
        MouseCatcher mouseCatcher { get ;  }
        GameObject groundVisual { get ;  }
        InputController inputController { get ;  }
        GameObject cursorCanvas { get ;  }
        PlayerAvatarController playerAvatarController { get ;  }
        CameraController cameraController { get ;  }
        UnityEngine.Camera mainCamera { get ;  }
        GameObject bridgeGameObject { get ;  }
        Light environmentLight { get ;  }
        Volume postProcessVolume { get ;  }
        CinemachineFreeLook thirdPersonCamera { get ; }
        CinemachineVirtualCamera firstPersonCamera { get ; }
        void Dispose();
    }
}