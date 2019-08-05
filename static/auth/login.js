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

  webAuth.authorize();
});
