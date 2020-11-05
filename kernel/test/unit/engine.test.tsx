import { expect } from 'chai'
import { Entity, BoxShape, Transform, Vector3, engine } from 'decentraland-ecs/src';

describe('Engine tests', () => {
  it('getComponentGroup returns cached group', () => {

    let box = new Entity()
    box.addComponent(new BoxShape())
    box.addComponent(
      new Transform({
        position: new Vector3(8, 0, 8),
      })
    )

    const firstComponentGroup = engine.getComponentGroup(BoxShape, Transform)
    const secondComponentGroup = engine.getComponentGroup(BoxShape, Transform)
    const thirdComponentGroup = engine.getComponentGroup(BoxShape, Transform)

    expect(firstComponentGroup).to.equal(
      secondComponentGroup,
      'returnCachedComponentGroup01'
    )

    expect(secondComponentGroup).to.equal(
      thirdComponentGroup,
      'returnCachedComponentGroup02'
    )

    const fourthComponentGroup = engine.getComponentGroup(BoxShape)
    expect(thirdComponentGroup).to.not.equal(
      fourthComponentGroup,
      'returnCachedComponentGroup03'
    )
  })
})
