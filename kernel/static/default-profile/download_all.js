const fs = require('fs')
const path = require('path')
const fetch = require('node-fetch')

const contentServerUrl = 'https://content.decentraland.org/contents/'
const contentsDir = 'contents'

const downloadFile = async (url, path) => {
  const res = await fetch(url)
  const fileStream = fs.createWriteStream(path)
  await new Promise((resolve, reject) => {
    res.body.pipe(fileStream)
    res.body.on('error', (err) => {
      reject(err)
    })
    fileStream.on('finish', function () {
      resolve()
    })
  })
}

const catalog = require('./basecatalog.json')

let contentPath
try {
  contentPath = path.join(__dirname, contentsDir)
  fs.mkdirSync(contentPath)
} catch (e) {}

const hashes = catalog.reduce(
  (hashes, wearable) =>
    hashes.concat(
      wearable.representations.reduce(
        (hashes, representation) =>
          hashes.concat(representation.contents.reduce((hashes, content) => hashes.concat([content.hash]), [])),
        []
      )
    ),
  []
)

hashes.forEach(async (url) => {
  await downloadFile(contentServerUrl + url, path.join(contentPath, url))
})
