var config = {
  "org": {
    "domain": "decentraland.auth0.com",
    "client_id": "yqFiSmQsxk3LK46JOIB4NJ3wK4HzZVxG"
  },
  "today": {
    "domain": "dcl-stg.auth0.com",
    "client_id": "0UB0I7w6QA3AgSvbXh9rGvDuhKrJV1C0"
  },
  "zone": {
    "domain": "dcl-test.auth0.com",
    "client_id": "lTUEMnFpYb0aiUKeIRPbh7pBxKM6sccx"
  }
};

var TLD = window.location.hostname.match(/(\w+)$/)[0];
var credentials = config[TLD] || config['zone'];

var APP_DOMAIN = location.origin;
var DOMAIN = credentials.domain;
var CLIENT_ID = credentials.client_id;
var AUDIENCE = 'decentraland.org';
var LOGIN_REDIRECT = APP_DOMAIN + '/auth/login_callback.html';
var LOGOUT_REDIRECT = APP_DOMAIN + '/auth/logout_callback.html';
var REDIRECT_BASE_URL = APP_DOMAIN;
