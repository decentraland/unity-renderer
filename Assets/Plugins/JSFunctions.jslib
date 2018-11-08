/* 
* EXAMPLE JSON STRUCTURE
* {
    "id":"1",
    "parentId":"0",
    "components":{      
      "shape":{
        "billboard": "0",
        "tag": "box",
        "visible": "true",
        "withCollisions": "false",
        "src":"http://127.0.0.1:8080/GLTF/Lantern/glTF-Binary/Lantern.glb" // Not implemented if not a GLTF-SHAPE
      },
      "transform":{
        "position":{"x":5,"y":1,"z":5},
        "rotation":{"x":0,"y":0,"z":0},
        "scale":{"x":1,"y":1,"z":1},
        "tag": "transform"
      }
    }
  }
* 
*/

mergeInto(LibraryManager.library, {
  StartDecentraland: function() {
    window.startDecentraland();
  }
});
