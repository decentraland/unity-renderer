// preload configurations
import 'config'

// initialize the renderer and all of the elements
// we will need interact with like the camera
import './renderer'

// initialize the entity system
import './entities'

// register the components that will be used by the entity system
import './components/index'

export { domReadyFuture } from './renderer'
