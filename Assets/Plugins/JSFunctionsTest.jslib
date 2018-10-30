/* 
* EXAMPLE JSON
* {
    "id":"1",
    "parentId":"0",
    "components":{
      "position":{"x":5,"y":0,"z":5},
      "shape":{"tag":"box"},
      "rotation":{"x":0,"y":0,"z":0}
    }
  }
* 
*/

mergeInto(LibraryManager.library, {
  InitializeDecentraland: function() {
    window.dcl = {
      log: function(messagesLog) {
        console.log("log: " + messagesLog);
      },
      onStart: function(callback) {
        //console.log("onStart called on entity", arguments);
      },
      onUpdate: function(callback) {
        var previousTime = performance.now();

        setInterval(function() {
          var currentTime = performance.now();

          callback(currentTime - previousTime);

          previousTime = currentTime;
        }, 30);
      },
      onEvent: function(callback) {
        //console.log("onEvent called on entity", arguments);
      },
      addEntity: function(entityId) {
        var entityJSON = {
          id: entityId
        };

        SendMessage(
          "SceneController",
          "CreateEntity",
          JSON.stringify(entityJSON)
        );
      },
      updateEntity: function(entityId, components) {
        // components: Record<string, Component>
        var newComponents = {};
        for (var key in components) {
          if (key.startsWith("engine."))
            newComponents[key.replace("engine.", "")] = components[key];
        }

        var entityJSON = {
          id: entityId,
          components: newComponents
        };

        SendMessage(
          "SceneController",
          "UpdateEntity",
          JSON.stringify(entityJSON)
        );
      },
      removeEntity: function(entityId) {},
      componentAdded: function(entityId, componentName, component) {},
      componentRemoved: function(entityId, componentName) {},
      setParent: function(entityId, parentId) {
        var entityJSON = {
          id: entityId,
          parentId: parentId
        };

        SendMessage(
          "SceneController",
          "SetEntityParent",
          JSON.stringify(entityJSON)
        );
      },
      subscribe: function(eventName) {
        console.log("entity subscribed to event: " + eventName);
      },
      unsubscribe: function(eventName) {
        console.log("entity unsubscribed from event: " + eventName);
      }
    };

    window.initializeDecentraland();
  }
});
