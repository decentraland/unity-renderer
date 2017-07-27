import { expect } from 'chai'
import { createSchemaValidator } from 'engine/components/helpers/schemaValidator'
import * as BABYLON from 'babylonjs'

describe('schema validator tests', function() {
  it('result should contain all default values (empty)', () => {
    const schemaValidator = createSchemaValidator({})
    const result = schemaValidator({})
    expect(result).to.deep.eq({})
  })

  it('result should contain all default values', () => {
    const schemaValidator = createSchemaValidator({
      abc: { type: 'string', default: 'ABC' },
      withoutDefaultValue: { type: 'string' }
    })
    const result = schemaValidator({})
    expect(result).to.deep.eq({ abc: 'ABC' })
  })

  it('parse several values', () => {
    const schemaValidator = createSchemaValidator({
      stringFromNumber: { type: 'string', default: 'ABC' },
      stringFromBoolean: { type: 'string', default: 'ABC' },
      numberFromString: { type: 'number', default: 1 },
      floatFromString: { type: 'float', default: 1 },
      intFromString: { type: 'int', default: 1 },
      booleanFromString: { type: 'boolean', default: false },
      booleanFromStringFalse: { type: 'boolean', default: false },
      vector3: { type: 'vector3' },
      vector3FromScalar: { type: 'vector3' },
      numberDefaulted: { type: 'number', default: 11 },
      floatArray: { type: 'floatArray' }
    })

    const untouchedVector = new BABYLON.Vector3(10, 10, 10)

    const result = schemaValidator({
      stringFromNumber: 1.0,
      stringFromBoolean: true,
      numberFromString: '1.1',
      floatFromString: '2.1',
      intFromString: '3.1',
      booleanFromString: 'true',
      booleanFromStringFalse: 'false',
      vector3: untouchedVector,
      vector3FromScalar: 100,
      sideDefaulted: 123987,
      numberDefaulted: {},
      floatArray: [1, 2, 3, 4, 5, 6, 7, 8]
    })

    expect(result.stringFromNumber).to.eq('1')
    expect(result.stringFromBoolean).to.eq('true')
    expect(result.intFromString).to.eq(3)
    expect(result.floatFromString).to.eq(2.1)
    expect(result.numberFromString).to.eq(1.1)
    expect(result.booleanFromString).to.eq(true)
    expect(result.booleanFromStringFalse).to.eq(false)
    expect(result.vector3).to.eq(untouchedVector)
    expect(result.vector3FromScalar).to.deep.eq({ x: 100, y: 100, z: 100 })
    expect(result.numberDefaulted).to.eq(11)
    expect(result.floatArray).to.deep.eq([1, 2, 3, 4, 5, 6, 7, 8])
  })
})
