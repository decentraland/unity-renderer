const fs = require("fs")
const path = require("path")
const {fetch} = require("undici")

const contentsDir = "contents"

const downloadFile = async (url, path) => {
  const res = await fetch(url)
  const content = new Uint8Array(await res.arrayBuffer())
  fs.writeFileSync(path, content)
}

let contentPath
try {
  contentPath = path.join(__dirname, contentsDir)
  fs.mkdirSync(contentPath)
} catch (e) {}

const downloadUrls = new Set()

const filterUrn = new Set([
  "urn:decentraland:off-chain:base-avatars:BaseFemale",
  "urn:decentraland:off-chain:base-avatars:bun_shoes",
  "urn:decentraland:off-chain:base-avatars:f_eyebrows_00",
  "urn:decentraland:off-chain:base-avatars:f_eyes_00",
  "urn:decentraland:off-chain:base-avatars:f_jeans",
  "urn:decentraland:off-chain:base-avatars:f_mouth_00",
  "urn:decentraland:off-chain:base-avatars:f_sweater",
  "urn:decentraland:off-chain:base-avatars:standard_hair",
])

async function main() {
  const response = await fetch(
    "https://peer.decentraland.org/lambdas/collections/wearables?collectionId=urn:decentraland:off-chain:base-avatars"
  )
  if (!response.ok)
    throw new Error(
      "https://peer.decentraland.org/lambdas/collections/wearables?collectionId=urn:decentraland:off-chain:base-avatars not ok"
    )
  const catalog = await response.json()

  for (let wearable of catalog.wearables) {
    if (filterUrn.has(wearable.id)) {
      downloadUrls.add(wearable.thumbnail)
      for (let representation of wearable.data.representations) {
        for (let contentItem of representation.contents) {
          downloadUrls.add(contentItem.url)
        }
      }
    }
  }

  for (let url of downloadUrls) {
    const parts = url.split("/")
    const finalPath = path.join(contentPath, parts[parts.length - 1])
    if (!fs.existsSync(finalPath)) {
      console.log(`Downloading ${finalPath} from ${url}`)
      await downloadFile(url, finalPath)
    }
  }
}

main().catch((e) => {
  console.error(e)
  process.exit(1)
})
