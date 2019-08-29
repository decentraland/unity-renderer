// tslint:disable:no-console
// this file uses console.log because it is a helper

import glob = require('glob')
import path = require('path')
import multihashes = require('multihashes')
import fs = require('fs-extra')
import keccak = require('keccakjs')

const parcelInfoDir = path.resolve(__dirname, '../public/local-ipfs/parcel_info')
const sceneMappingDir = path.resolve(__dirname, '../public/local-ipfs/scene_mapping')
const ipfsMockContent = path.resolve(__dirname, '../public/local-ipfs/contents/')
console.log(`> generating content server mappings mock`)

fs.ensureDirSync(parcelInfoDir)
fs.ensureDirSync(sceneMappingDir)

const sceneMappings: { root_cid: string; scene_cid: string; parcel_id: string; parcelPosition: string }[] = []
const parcelInfos: { rootCid: string; mappingData: any }[] = []
const ipfsMock: any[] = []
const ipfsMockFilename = path.resolve(__dirname, '../public/local-ipfs/mappings')

glob
  .sync(path.resolve(__dirname, '../public/test-scenes/*/scene.json'), { absolute: true })
  .concat(glob.sync(path.resolve(__dirname, '../public/ecs-scenes/*/scene.json'), { absolute: true }))
  .concat(glob.sync(path.resolve(__dirname, '../public/hell-map/*/scene.json'), { absolute: true }))
  .forEach(file => {
    const manifest = require(file)
    const dirName = path.dirname(file)
    const rootCid = hash(dirName.replace(process.cwd(), ''))
    const mappings = generateIPFSMock(path.dirname(file))
    const mappingData = {
      publisher:
        (manifest._blockhainMock && manifest._blockhainMock.operator) ||
        (manifest._blockhainMock && manifest._blockhainMock.owner) ||
        manifest.owner ||
        '0x0f5d2fb29fb7d3cfee444a200298f468908cc942',
      parcel_id: manifest.scene.parcels[0],
      parcelPosition: manifest.scene.parcels[0],
      root_cid: rootCid,
      contents: mappings
    }
    parcelInfos.push({ rootCid, mappingData })

    for (let p in manifest.scene.parcels) {
      const parcelPosition = manifest.scene.parcels[p]
      sceneMappings.push({ parcelPosition, parcel_id: parcelPosition, root_cid: rootCid, scene_cid: '' })
      ipfsMock.push({ ...mappingData, parcel_id: parcelPosition, parcelPosition })
    }
  })

function catchAll(err) {
  if (!err) return
  console.log('Unable to write ipfsMock files or parcel info files')
  console.log(err)
  process.exit(1)
}

fs.writeFile(ipfsMockFilename, JSON.stringify(ipfsMock, null, 2), catchAll)

Promise.all(
  parcelInfos.map(info => {
    return new Promise((resolve, reject) =>
      fs.writeFile(path.join(parcelInfoDir, info.rootCid), JSON.stringify(info.mappingData, null, 2), err =>
        err ? reject(err) : resolve()
      )
    )
  })
).catch(catchAll)

Promise.all(
  sceneMappings.map(mapping => {
    return new Promise((resolve, reject) =>
      fs.writeFile(path.join(sceneMappingDir, mapping.parcelPosition), JSON.stringify(mapping), err =>
        err ? reject(err) : resolve()
      )
    )
  })
).catch(catchAll)

function hash(str: string) {
  // TODO: Use the correct hash (the UNIXFS hashing algorithm) for the 'Qm' IPFS multihash
  // More info: https://github.com/eordano/ipfs-playground/blob/master/src/ipfs.js
  const hash = new keccak()
  hash.update(str)

  return 'Qm' + multihashes.toB58String(new Buffer(hash.digest('hex'), 'hex'), 'sha2-256')
}

function generateIPFSMock(folder: string) {
  const mappings: Array<{ file: string; hash: string }> = []

  const files = new Set<string>(glob.sync(folder + '/**/*', { absolute: true }))

  files.forEach(file => {
    try {
      if (!fs.statSync(file).isFile()) return
    } catch (err) {
      return
    }

    const key = file.replace(folder + '/', '')
    const cid = hash(file.replace(process.cwd(), ''))
    const targetFile = ipfsMockContent + '/' + cid

    mappings.push({ file: key.toLowerCase(), hash: cid })

    try {
      fs.ensureSymlinkSync(file, targetFile, 'file')
    } catch (e) {
      try {
        fs.removeSync(targetFile)
      } finally {
        fs.ensureSymlinkSync(file, targetFile, 'file')
      }
    }
  })

  return mappings
}
