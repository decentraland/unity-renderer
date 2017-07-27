import { Panel } from './utils/Panel'

export function createStats() {
  let mode = 0

  const container = document.createElement('div')
  container.style.cssText = 'position:fixed;top:0;left:0;cursor:pointer;opacity:0.9;z-index:10000'
  container.setAttribute('class', 'client-stats')

  container.addEventListener(
    'click',
    function(event) {
      event.preventDefault()
      showPanel(++mode % container.children.length)
    },
    false
  )

  function addPanel(panel) {
    container.appendChild(panel.getDOM())
    return panel
  }

  function showPanel(id) {
    for (let i = 0; i < container.children.length; i++) {
      container.children[i]['style'].display = i === id ? 'block' : 'none'
    }

    mode = id
  }

  let beginTime = (performance || Date).now()
  let prevTime = beginTime
  let frames = 0

  let msPanel = addPanel(new Panel('MS', '#0f0', '#020'))
  let fpsPanel = addPanel(new Panel('FPS', '#0ff', '#002'))
  let memPanel

  if (self.performance && self.performance['memory']) {
    memPanel = addPanel(new Panel('MB', '#f08', '#201'))
  }

  showPanel(0)

  return {
    REVISION: 16,

    dom: container,

    addPanel: addPanel,
    showPanel: showPanel,

    begin: function() {
      beginTime = (performance || Date).now()
    },

    end: function() {
      frames++

      let time = (performance || Date).now()

      msPanel.update(time - beginTime, 200)

      if (time > prevTime + 1000) {
        fpsPanel.update((frames * 1000) / (time - prevTime), 100)

        prevTime = time
        frames = 0

        if (memPanel) {
          let memory = performance['memory']
          memPanel.update(memory.usedJSHeapSize / 1048576, memory.jsHeapSizeLimit / 1048576)
        }
      }

      return time
    },

    update: function() {
      beginTime = this.end()
    }
  }
}
