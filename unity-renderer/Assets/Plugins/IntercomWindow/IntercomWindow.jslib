var IntercomWindow = {

  OpenIntercom : function() {
    const windowHeight = window.innerHeight
    const windowWidth = window.innerWidth
    const popupHeight = Math.min(700, windowHeight)
    const popupWidth = Math.min(400, windowWidth)
    const top = Math.max(windowHeight - popupHeight, 0)
    const left = Math.max(windowWidth - popupWidth - 20, 0)
    
    window.open('https://intercom.decentraland.org', 'intercom', `popup,top=${top},left=${left},width=${popupWidth},height=${popupHeight}`)
    
    console.log("I SHOULD BE OPENING INTERCOM 2")
  }
};

mergeInto(LibraryManager.library, IntercomWindow);;
