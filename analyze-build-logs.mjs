import { promises as fs } from 'fs'

;(async function main() {
  const raw = await fs.readFile('./build', 'utf-8')
  const lines = raw.split('\n')
  const start = 'Used Assets and files from the Resources folder, sorted by uncompressed size:\r'
  const end = '-------------------------------------------------------------------------------\r'
  const first = lines.indexOf(start)
  const last = lines.indexOf(end, first)
  const rawData = lines.slice(first + 1, last - 1)

  function parseSize(data) {
    return (parseFloat(data.replace(' ', '')) + 1)
      * ((data.indexOf('m') === -1) ? 1 : 1024)
  }
  function parseName(data) {
    return data.split(' ').slice(2).join(' ')
  }
  function category(name) {
    if (name.includes('Built-in Texture2D')) {
      return { extension: 'texture', baseFolder: 'none', labels: ['atlas'] }
    }
    const extension = name.split('.')[name.split('.').length - 1]
    const baseFolder = name.split('/')[0]
    const labels = []
    if (extension === 'anim') {
      labels.push('animation')
    }
    if (extension === 'png' || extension === 'jpg' || extension === 'jpeg') {
      labels.push('texture')
    }
    if (extension === 'json') {
      labels.push('json')
    }
    if (name.includes('bc-csharp')) {
      labels.push('bouncy-castle')
    }
    if (extension === 'cs') {
      labels.push('code')
    }
    if (name.includes('DCL/')) {
      labels.push('DCL')
    }
    if (name.includes('Packages/')) {
      labels.push('packages')
      if (name.includes('Packages/com.unity')) {
        labels.push('unity')
      } else {
        labels.push('third-party')
      }
    }
    if (extension === 'ogg' || extension === 'wav') {
      labels.push('sound')
    }
    if (extension === 'shader' || extension === 'shadergraph') {
      labels.push('shader')
    }
    if (name.includes('ProceduralSkybox')) {
      labels.push('skybox')
    }
    if (name.includes('InfiniteFloor')) {
      labels.push('infinite-floor')
    }
    if (name.includes('Assets/Resources/EmbeddedTextures')) {
      labels.push('bundled-wearables')
    }
    if (extension === 'prefab') {
      labels.push('prefab')
    }
    if (name.includes('TextMesh Pro') || extension === 'ttf') {
      labels.push('fonts')
    }
    return { extension, baseFolder, labels }
  }

  const data = rawData.map(_ => {
    const clean = _.replace('\r', '').split('\t')
    const size = parseSize(clean[0])
    const name = parseName(clean[1])
    const labels = category(name)
    return { size, name, ...labels }
  })
  const allLabels = [ 'atlas','texture','skybox','animation','DCL','shader','fonts','infinite-floor','packages','third-party','unity','prefab','bundled-wearables','json','code','bouncy-castle','sound' ]
  console.log('name,size,extension,'+allLabels.join(','))
  for (let _ of data) {
    const { name, size, extension, labels } = _
    const labelMap = allLabels.map($ => labels.includes($) ? '1' : '0')
    console.log(`${name},${size},${extension},${labelMap.join(',')}`)
  }


})().catch(err => console.error(err))
