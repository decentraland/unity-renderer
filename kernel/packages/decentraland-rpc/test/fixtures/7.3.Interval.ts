import { TestableScript } from './support/ClientHelpers'
import { inject } from '../../lib/client/index'

export default class Interval extends TestableScript {
  @inject('Test7') test7: any = null

  async doTest() {
    setInterval(async () => {
      if (await this.test7.isReady()) {
        await this.test7.doSomething()
      }
    }, 100)
  }
}
