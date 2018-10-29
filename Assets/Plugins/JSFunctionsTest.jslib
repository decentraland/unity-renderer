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
        var deltaTime = 60;
        setInterval(function() {
          callback(deltaTime);
        }, deltaTime);
      },
      onEvent: function(callback) {
        //console.log("onEvent called on entity", arguments);
      },
      addEntity: function(entityId) {
        var JSONParams = {
          entityIdParam: entityId
        };

        SendMessage(
          "SceneController",
          "CreateEntity",
          JSON.stringify(JSONParams)
        );
      },
      updateEntity: function(entityId, components) {
        // components: Record<string, Component>
        var newComponents = {};
        for (var key in components) {
          if (key.startsWith("engine.")) {
            newComponents[key.replace("engine.", "")] = components[key];
          } else {
            newComponents[key] = components[key];
          }
        }

        var JSONParams = {
          entityIdParam: entityId,
          entityComponents: newComponents
        };

        SendMessage(
          "SceneController",
          "UpdateEntity",
          JSON.stringify(JSONParams)
        );
      },
      removeEntity: function(entityId) {},
      componentAdded: function(entityId, componentName, component) {},
      componentRemoved: function(entityId, componentName) {},
      setParent: function(entityId, parentId) {
        var JSONParams = {
          entityIdParam: entityId,
          parentIdParam: parentId
        };

        SendMessage(
          "SceneController",
          "SetEntityParent",
          JSON.stringify(JSONParams)
        );
      },
      subscribe: function(eventName) {
        console.log("entity subscribed to event: " + eventName);
      },
      unsubscribe: function(eventName) {
        console.log("entity unsubscribed from event: " + eventName);
      }
    };
  }
});
