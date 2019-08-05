window.addEventListener('load', function () {
  if (window.self !== window.top) {
    // Is within an Iframe
    window.parent.postMessage(
      {
        type: 'DECENTRALAND_LOGOUT'
      },
      '*'
    );
  } else {
    // Not an iframe nor a popup, then redirect to callback url
    document.location.href = REDIRECT_BASE_URL;
  }
});
