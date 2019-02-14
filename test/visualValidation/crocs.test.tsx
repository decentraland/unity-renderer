// tslint:disable:no-commented-out-code
import { loadTestParcel, saveScreenshot, wait } from '../testHelpers'
// import { expect } from 'chai'
// import { Sound as SoundComponent } from 'engine/components/ephemeralComponents/Sound'

loadTestParcel('Crocs', 200, 10, function(root, futureParcelScene) {
  // let audios: SoundComponent[] = []

  wait(1000)

  // 1 it('find a component with audio', async () => {
  // 1   const glParcelScene = await futureParcelScene

  // 1   glParcelScene.context.rootEntity.traverse($ => {
  // 1     if ($ && $ instanceof BaseEntity) {
  // 1       Object.values($.components)
  // 1         .filter($ => $ instanceof SoundComponent)
  // 1         .forEach($ => audios.push($ as SoundComponent))
  // 1     }
  // 1   })

  // 1   expect(audios.length).to.not.eq(0)
  // 1 })

  saveScreenshot(`crocs.png`, {
    from: [199.7, 1, 10.5],
    lookAt: [201.1, -1, 10.5]
  })

  wait(10)

  // it('has a sound', () => {
  //   expect(audios.length).to.not.eq(0)
  // })

  it('disposes the parcelScene', async () => {
    const parcelScene = await futureParcelScene
    parcelScene.dispose()
  })

  // it('every sound should be stopped', () => {
  //   expect(audios.every($ => ($.sound as any) === undefined)).eq(
  //     true,
  //     'every sound component should have no attached sound'
  //   )
  // })
})
