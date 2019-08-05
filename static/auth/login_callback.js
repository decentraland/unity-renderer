window.addEventListener('load', function () {
  var webAuth = new auth0.WebAuth({
    domain: DOMAIN,
    clientID: CLIENT_ID,
    redirectUri: LOGIN_REDIRECT,
    responseType: 'token id_token',
    scope: 'openid',
    leeway: 60,
    audience: AUDIENCE
  });

  webAuth.parseHash(function (err, authResult) {
    if (authResult && authResult.accessToken && authResult.idToken) {
      send(authResult.accessToken);
    } else if (err) {
      console.log(err);
    }
  });
});

function send(token) {
  var origin = APP_DOMAIN;
  if (window.self !== window.top) {
    // Is within an Iframe
    window.parent.postMessage(
      {
        type: 'DECENTRALAND_USER_TOKEN',
        token: token,
        from: 'IFRAME',
      },
      origin
    );
  } else if (window.opener && window.opener !== window) {
    // Is within a Popup
    window.opener.postMessage(
      {
        type: 'DECENTRALAND_USER_TOKEN',
        token: token,
        from: 'POPUP'
      },
      origin
    );
  } else {
    // Not an iframe nor a popup, then redirect to callback url
    document.location.href = REDIRECT_BASE_URL + '?user_token=' + token;
  }
}
