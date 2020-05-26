import { TestableScript } from './support/ClientHelpers'
import { inject } from '../../lib/client/index'

export default class TestMethods extends TestableScript {
  @inject('Test7') test7: any = null

  async doTest() {
    await this.test7.setNumber(Math.random())
  }
}
