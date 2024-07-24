var IntercomWindow = {

  OpenIntercom : function() {
    const windowHeight = window.innerHeight
    const windowWidth = window.innerWidth
    const popupHeight = Math.min(700, windowHeight)
    const popupWidth = Math.min(450, windowWidth)
    const top = Math.max(windowHeight - popupHeight, 0)
    const left = Math.max(windowWidth - popupWidth - 20, 0)
    
    window.open('https://decentraland.org/help', 'intercom', `popup,top=${top},left=${left},width=${popupWidth},height=${popupHeight}`)
  }
};

mergeInto(LibraryManager.library, IntercomWindow);;