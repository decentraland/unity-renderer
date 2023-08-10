import type { RpcServerPort } from '@dcl/rpc/dist/types'
import type { PortContext } from './context'

import { registerEthereumControllerServiceServerImplementation } from './EthereumController'
import { registerDevToolsServiceServerImplementation } from './DevTools'
import { registerEngineApiServiceServerImplementation } from './EngineAPI'
import { registerEnvironmentApiServiceServerImplementation } from './EnvironmentAPI'
import { registerPermissionServiceServerImplementation } from './Permissions'
import { registerUserIdentityServiceServerImplementation } from './UserIdentity'
import { registerParcelIdentityServiceServerImplementation } from './ParcelIdentity'
import { registerUserActionModuleServiceServerImplementation } from './UserActionModule'
import { registerSocialControllerServiceServerImplementation } from './SocialController'
import { registerRestrictedActionsServiceServerImplementation } from './RestrictedActions'
import { registerRuntimeServiceServerImplementation } from './Runtime'
import { registerTestingServiceServerImplementation } from './Testing'
import { registerCommunicationsControllerServiceServerImplementation } from './CommunicationsController'
import { registerPlayersServiceServerImplementation } from './Players'
import { registerPortableExperiencesServiceServerImplementation } from './PortableExperiences'
import { registerSignedFetchServiceServerImplementation } from './SignedFetch'
import { registerSceneServiceServerImplementation } from './Scene'
import { registerCommsApiServiceServerImplementation } from './CommsAPI'

export async function registerServices(serverPort: RpcServerPort<PortContext>) {
  registerDevToolsServiceServerImplementation(serverPort)
  registerEngineApiServiceServerImplementation(serverPort)
  registerEnvironmentApiServiceServerImplementation(serverPort)
  registerPermissionServiceServerImplementation(serverPort)
  registerUserIdentityServiceServerImplementation(serverPort)
  registerUserActionModuleServiceServerImplementation(serverPort)
  registerEthereumControllerServiceServerImplementation(serverPort)
  registerParcelIdentityServiceServerImplementation(serverPort)
  registerSocialControllerServiceServerImplementation(serverPort)
  registerRestrictedActionsServiceServerImplementation(serverPort)
  registerRuntimeServiceServerImplementation(serverPort)
  registerTestingServiceServerImplementation(serverPort)
  registerCommunicationsControllerServiceServerImplementation(serverPort)
  registerPlayersServiceServerImplementation(serverPort)
  registerPortableExperiencesServiceServerImplementation(serverPort)
  registerSignedFetchServiceServerImplementation(serverPort)
  registerSceneServiceServerImplementation(serverPort)
  registerCommsApiServiceServerImplementation(serverPort)
}
