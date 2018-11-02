/* 
* EXAMPLE JSON STRUCTURE
* {
    "id":"1",
    "parentId":"0",
    "components":{
      "position":{"x":5,"y":0,"z":5},
      "shape":{
        "tag":"box",
        "src":"http://127.0.0.1:8080/GLTF/Lantern/glTF-Binary/Lantern.glb"
      },
      "rotation":{"x":0,"y":0,"z":0}
    }
  }
* 
*/

mergeInto(LibraryManager.library, {
  StartDecentraland: function() {
    window.startDecentraland();
  }
});
