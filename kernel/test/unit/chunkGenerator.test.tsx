import { expect } from 'chai'
import { chunkGenerator } from 'unity-interface/dcl'

describe('chunkGenerator unit tests', function() {
  it('should throw if chunk size is less than 1', () => {
    const scenes: any = [{ name: 'a', type: 1, parcels: [{ x: 1, y: 1 }, { x: 2, y: 2 }, { x: 3, y: 3 }] }]

    const gen = chunkGenerator(0, scenes)

    expect(() => gen.next()).to.throw('parcel chunk size (0) cannot be less than 1')
  })

  it('generates chunks of given size for parcels from one scene', () => {
    const scenes: any = [{ name: 'a', type: 1, isPOI: false, parcels: [{ x: 1, y: 1 }, { x: 2, y: 2 }, { x: 3, y: 3 }] }]

    const gen = chunkGenerator(2, scenes)

    const result1 = gen.next()
    expect(result1.value).to.deep.equal([
      {
        isPOI: false,
        name: 'a',
        type: 1,
        parcels: [{ x: 1, y: 1 }, { x: 2, y: 2 }]
      }
    ])

    const result2 = gen.next()
    expect(result2.value).to.deep.equal([
      {
        isPOI: false,
        name: 'a',
        type: 1,
        parcels: [{ x: 3, y: 3 }]
      }
    ])

    const result3 = gen.next()
    expect(result3.done).to.be.equal(true)
  })

  it('generates chunks of given size for multiple scenes', () => {
    const scenes: any = [
      { name: 'a', type: 1, isPOI: true, parcels: [{ x: 1, y: 1 }] },
      { name: 'b', type: 2, isPOI: true, parcels: [{ x: 2, y: 2 }] },
      { name: 'c', type: 3, isPOI: true, parcels: [{ x: 3, y: 3 }] }
    ]

    const gen = chunkGenerator(2, scenes)

    const result1 = gen.next()
    expect(result1.value).to.deep.equal([
      { isPOI: true, name: 'a', type: 1, parcels: [{ x: 1, y: 1 }] },
      { isPOI: true, name: 'b', type: 2, parcels: [{ x: 2, y: 2 }] }
    ])

    const result2 = gen.next()
    expect(result2.value).to.deep.equal([{ isPOI: true, name: 'c', type: 3, parcels: [{ x: 3, y: 3 }] }])

    const result3 = gen.next()
    expect(result3.done).to.be.equal(true)
  })
})
