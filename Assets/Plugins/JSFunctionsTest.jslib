/* 
* EXAMPLE JSON STRUCTURE
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
  StartDecentraland: function() {
    window.startDecentraland();
  }
});
