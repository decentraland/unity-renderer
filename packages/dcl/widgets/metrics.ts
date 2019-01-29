import { DEBUG, PREVIEW, EDITOR } from 'config'

export interface Metrics {
  triangles: number
  bodies: number
  entities: number
  materials: number
  textures: number
}

export interface ParcelMetrics {
  dom: HTMLDivElement
  update: Function
}

export function drawMetrics(metrics: Metrics): ParcelMetrics {
  if (!EDITOR) {
    const parcelMetrics = createParcelMetrics(metrics)
    parcelMetrics.dom.style.visibility = 'hidden'

    if (DEBUG || PREVIEW) {
      parcelMetrics.dom.style.visibility = 'visible'
    }

    document.body.appendChild(parcelMetrics.dom)
    return parcelMetrics
  }
}

function createParcelMetrics(metrics: Metrics): ParcelMetrics {
  const containerParcelMetrics = document.createElement('div')
  containerParcelMetrics.style.cssText =
    'width:100px;opacity:0.9;cursor:pointer;position:absolute;z-index:100000;right:0px;bottom:0px;'
  containerParcelMetrics.setAttribute('class', 'parcel-metrics')

  for (let metric of Object.keys(metrics)) {
    const dom = getMetricDOM(metric, metrics[metric])
    containerParcelMetrics.appendChild(dom)
  }

  return {
    dom: containerParcelMetrics,
    update: (newMetrics: Metrics) => update(containerParcelMetrics, newMetrics)
  }
}

function getMetricDOM(name: string, value: string): HTMLDivElement {
  const div = document.createElement('div')
  div.id = `ui-${name}`
  div.style.cssText =
    'color:rgb(255,0,0);background-color:rgb(51,17,17);font-family:Helvetica,Arial,sans-serif;font-size:9px;font-weight:bold;line-height:15px;'
  div.innerText = `${capitalizeFirstLetter(name)}: ${value}`
  return div
}

function update(parcelMetrics: HTMLDivElement, metrics: Metrics): void {
  for (let metric of Object.keys(metrics)) {
    const dom = document.getElementById('ui-' + metric)
    dom.innerText = `${capitalizeFirstLetter(metric)}: ${metrics[metric]}`
  }
}

function capitalizeFirstLetter(str: string): string {
  return str.charAt(0).toUpperCase() + str.slice(1)
}
