// source: engineinterface.proto
/**
 * @fileoverview
 * @enhanceable
 * @suppress {messageConventions} JS Compiler reports an error if a variable or
 *     field starts with 'MSG_' and isn't a translatable message.
 * @public
 */
// GENERATED CODE -- DO NOT EDIT!

var jspb = require('google-protobuf');
var goog = jspb;
var global = Function('return this')();

var google_protobuf_empty_pb = require('google-protobuf/google/protobuf/empty_pb.js');
goog.object.extend(proto, google_protobuf_empty_pb);
goog.exportSymbol('proto.engineinterface.PB_AnimationState', null, global);
goog.exportSymbol('proto.engineinterface.PB_Animator', null, global);
goog.exportSymbol('proto.engineinterface.PB_AttachEntityComponent', null, global);
goog.exportSymbol('proto.engineinterface.PB_AudioClip', null, global);
goog.exportSymbol('proto.engineinterface.PB_AudioSource', null, global);
goog.exportSymbol('proto.engineinterface.PB_AvatarShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_BasicMaterial', null, global);
goog.exportSymbol('proto.engineinterface.PB_Billboard', null, global);
goog.exportSymbol('proto.engineinterface.PB_BoxShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_CircleShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_Color3', null, global);
goog.exportSymbol('proto.engineinterface.PB_Color4', null, global);
goog.exportSymbol('proto.engineinterface.PB_Component', null, global);
goog.exportSymbol('proto.engineinterface.PB_Component.ModelCase', null, global);
goog.exportSymbol('proto.engineinterface.PB_ComponentCreated', null, global);
goog.exportSymbol('proto.engineinterface.PB_ComponentDisposed', null, global);
goog.exportSymbol('proto.engineinterface.PB_ComponentRemoved', null, global);
goog.exportSymbol('proto.engineinterface.PB_ComponentUpdated', null, global);
goog.exportSymbol('proto.engineinterface.PB_ConeShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_ContentMapping', null, global);
goog.exportSymbol('proto.engineinterface.PB_CreateEntity', null, global);
goog.exportSymbol('proto.engineinterface.PB_CreateUIScene', null, global);
goog.exportSymbol('proto.engineinterface.PB_CylinderShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_DclMessage', null, global);
goog.exportSymbol('proto.engineinterface.PB_DclMessage.MessageCase', null, global);
goog.exportSymbol('proto.engineinterface.PB_Eyes', null, global);
goog.exportSymbol('proto.engineinterface.PB_Face', null, global);
goog.exportSymbol('proto.engineinterface.PB_GLTFShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_GlobalPointerDown', null, global);
goog.exportSymbol('proto.engineinterface.PB_GlobalPointerUp', null, global);
goog.exportSymbol('proto.engineinterface.PB_Hair', null, global);
goog.exportSymbol('proto.engineinterface.PB_LoadParcelScenes', null, global);
goog.exportSymbol('proto.engineinterface.PB_Material', null, global);
goog.exportSymbol('proto.engineinterface.PB_NFTShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_OBJShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_OpenExternalUrl', null, global);
goog.exportSymbol('proto.engineinterface.PB_OpenNFTDialog', null, global);
goog.exportSymbol('proto.engineinterface.PB_PlaneShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_Position', null, global);
goog.exportSymbol('proto.engineinterface.PB_Quaternion', null, global);
goog.exportSymbol('proto.engineinterface.PB_Query', null, global);
goog.exportSymbol('proto.engineinterface.PB_Ray', null, global);
goog.exportSymbol('proto.engineinterface.PB_RayQuery', null, global);
goog.exportSymbol('proto.engineinterface.PB_RemoveEntity', null, global);
goog.exportSymbol('proto.engineinterface.PB_SendSceneMessage', null, global);
goog.exportSymbol('proto.engineinterface.PB_SendSceneMessage.PayloadCase', null, global);
goog.exportSymbol('proto.engineinterface.PB_SetEntityParent', null, global);
goog.exportSymbol('proto.engineinterface.PB_SetPosition', null, global);
goog.exportSymbol('proto.engineinterface.PB_Shape', null, global);
goog.exportSymbol('proto.engineinterface.PB_Skin', null, global);
goog.exportSymbol('proto.engineinterface.PB_SphereShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_TextShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_TextShapeModel', null, global);
goog.exportSymbol('proto.engineinterface.PB_Texture', null, global);
goog.exportSymbol('proto.engineinterface.PB_Transform', null, global);
goog.exportSymbol('proto.engineinterface.PB_UIButton', null, global);
goog.exportSymbol('proto.engineinterface.PB_UICanvas', null, global);
goog.exportSymbol('proto.engineinterface.PB_UIContainerRect', null, global);
goog.exportSymbol('proto.engineinterface.PB_UIContainerStack', null, global);
goog.exportSymbol('proto.engineinterface.PB_UIImage', null, global);
goog.exportSymbol('proto.engineinterface.PB_UIInputText', null, global);
goog.exportSymbol('proto.engineinterface.PB_UIScrollRect', null, global);
goog.exportSymbol('proto.engineinterface.PB_UIShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_UIStackOrientation', null, global);
goog.exportSymbol('proto.engineinterface.PB_UITextShape', null, global);
goog.exportSymbol('proto.engineinterface.PB_UUIDCallback', null, global);
goog.exportSymbol('proto.engineinterface.PB_UnloadScene', null, global);
goog.exportSymbol('proto.engineinterface.PB_UpdateEntityComponent', null, global);
goog.exportSymbol('proto.engineinterface.PB_Vector3', null, global);
goog.exportSymbol('proto.engineinterface.PB_Wearable', null, global);
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_CreateEntity = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_CreateEntity, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_CreateEntity.displayName = 'proto.engineinterface.PB_CreateEntity';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_RemoveEntity = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_RemoveEntity, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_RemoveEntity.displayName = 'proto.engineinterface.PB_RemoveEntity';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_SetEntityParent = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_SetEntityParent, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_SetEntityParent.displayName = 'proto.engineinterface.PB_SetEntityParent';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_ComponentRemoved = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_ComponentRemoved, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_ComponentRemoved.displayName = 'proto.engineinterface.PB_ComponentRemoved';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Component = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, proto.engineinterface.PB_Component.oneofGroups_);
};
goog.inherits(proto.engineinterface.PB_Component, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Component.displayName = 'proto.engineinterface.PB_Component';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Color4 = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Color4, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Color4.displayName = 'proto.engineinterface.PB_Color4';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Color3 = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Color3, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Color3.displayName = 'proto.engineinterface.PB_Color3';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_TextShapeModel = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_TextShapeModel, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_TextShapeModel.displayName = 'proto.engineinterface.PB_TextShapeModel';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Vector3 = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Vector3, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Vector3.displayName = 'proto.engineinterface.PB_Vector3';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Quaternion = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Quaternion, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Quaternion.displayName = 'proto.engineinterface.PB_Quaternion';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Transform = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Transform, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Transform.displayName = 'proto.engineinterface.PB_Transform';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UpdateEntityComponent = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UpdateEntityComponent, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UpdateEntityComponent.displayName = 'proto.engineinterface.PB_UpdateEntityComponent';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_ComponentCreated = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_ComponentCreated, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_ComponentCreated.displayName = 'proto.engineinterface.PB_ComponentCreated';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_AttachEntityComponent = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_AttachEntityComponent, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_AttachEntityComponent.displayName = 'proto.engineinterface.PB_AttachEntityComponent';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_ComponentDisposed = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_ComponentDisposed, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_ComponentDisposed.displayName = 'proto.engineinterface.PB_ComponentDisposed';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_ComponentUpdated = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_ComponentUpdated, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_ComponentUpdated.displayName = 'proto.engineinterface.PB_ComponentUpdated';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Ray = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Ray, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Ray.displayName = 'proto.engineinterface.PB_Ray';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_RayQuery = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_RayQuery, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_RayQuery.displayName = 'proto.engineinterface.PB_RayQuery';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Query = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Query, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Query.displayName = 'proto.engineinterface.PB_Query';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_SendSceneMessage = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, proto.engineinterface.PB_SendSceneMessage.oneofGroups_);
};
goog.inherits(proto.engineinterface.PB_SendSceneMessage, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_SendSceneMessage.displayName = 'proto.engineinterface.PB_SendSceneMessage';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_SetPosition = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_SetPosition, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_SetPosition.displayName = 'proto.engineinterface.PB_SetPosition';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_ContentMapping = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_ContentMapping, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_ContentMapping.displayName = 'proto.engineinterface.PB_ContentMapping';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Position = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Position, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Position.displayName = 'proto.engineinterface.PB_Position';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_LoadParcelScenes = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, proto.engineinterface.PB_LoadParcelScenes.repeatedFields_, null);
};
goog.inherits(proto.engineinterface.PB_LoadParcelScenes, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_LoadParcelScenes.displayName = 'proto.engineinterface.PB_LoadParcelScenes';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_CreateUIScene = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_CreateUIScene, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_CreateUIScene.displayName = 'proto.engineinterface.PB_CreateUIScene';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UnloadScene = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UnloadScene, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UnloadScene.displayName = 'proto.engineinterface.PB_UnloadScene';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_DclMessage = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, proto.engineinterface.PB_DclMessage.oneofGroups_);
};
goog.inherits(proto.engineinterface.PB_DclMessage, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_DclMessage.displayName = 'proto.engineinterface.PB_DclMessage';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_AnimationState = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_AnimationState, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_AnimationState.displayName = 'proto.engineinterface.PB_AnimationState';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Animator = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Animator, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Animator.displayName = 'proto.engineinterface.PB_Animator';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_AudioClip = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_AudioClip, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_AudioClip.displayName = 'proto.engineinterface.PB_AudioClip';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_AudioSource = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_AudioSource, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_AudioSource.displayName = 'proto.engineinterface.PB_AudioSource';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_AvatarShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, proto.engineinterface.PB_AvatarShape.repeatedFields_, null);
};
goog.inherits(proto.engineinterface.PB_AvatarShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_AvatarShape.displayName = 'proto.engineinterface.PB_AvatarShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Wearable = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, proto.engineinterface.PB_Wearable.repeatedFields_, null);
};
goog.inherits(proto.engineinterface.PB_Wearable, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Wearable.displayName = 'proto.engineinterface.PB_Wearable';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Face = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Face, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Face.displayName = 'proto.engineinterface.PB_Face';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Eyes = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Eyes, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Eyes.displayName = 'proto.engineinterface.PB_Eyes';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Hair = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Hair, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Hair.displayName = 'proto.engineinterface.PB_Hair';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Skin = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Skin, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Skin.displayName = 'proto.engineinterface.PB_Skin';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_BasicMaterial = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_BasicMaterial, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_BasicMaterial.displayName = 'proto.engineinterface.PB_BasicMaterial';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Billboard = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Billboard, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Billboard.displayName = 'proto.engineinterface.PB_Billboard';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_BoxShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_BoxShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_BoxShape.displayName = 'proto.engineinterface.PB_BoxShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_CircleShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_CircleShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_CircleShape.displayName = 'proto.engineinterface.PB_CircleShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_ConeShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_ConeShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_ConeShape.displayName = 'proto.engineinterface.PB_ConeShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_CylinderShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_CylinderShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_CylinderShape.displayName = 'proto.engineinterface.PB_CylinderShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_GlobalPointerDown = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_GlobalPointerDown, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_GlobalPointerDown.displayName = 'proto.engineinterface.PB_GlobalPointerDown';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_GlobalPointerUp = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_GlobalPointerUp, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_GlobalPointerUp.displayName = 'proto.engineinterface.PB_GlobalPointerUp';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_GLTFShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_GLTFShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_GLTFShape.displayName = 'proto.engineinterface.PB_GLTFShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Material = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Material, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Material.displayName = 'proto.engineinterface.PB_Material';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_NFTShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_NFTShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_NFTShape.displayName = 'proto.engineinterface.PB_NFTShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_OBJShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_OBJShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_OBJShape.displayName = 'proto.engineinterface.PB_OBJShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_PlaneShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, proto.engineinterface.PB_PlaneShape.repeatedFields_, null);
};
goog.inherits(proto.engineinterface.PB_PlaneShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_PlaneShape.displayName = 'proto.engineinterface.PB_PlaneShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Shape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Shape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Shape.displayName = 'proto.engineinterface.PB_Shape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_SphereShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_SphereShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_SphereShape.displayName = 'proto.engineinterface.PB_SphereShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_TextShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_TextShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_TextShape.displayName = 'proto.engineinterface.PB_TextShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_Texture = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_Texture, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_Texture.displayName = 'proto.engineinterface.PB_Texture';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UIButton = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UIButton, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UIButton.displayName = 'proto.engineinterface.PB_UIButton';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UICanvas = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UICanvas, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UICanvas.displayName = 'proto.engineinterface.PB_UICanvas';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UIContainerRect = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UIContainerRect, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UIContainerRect.displayName = 'proto.engineinterface.PB_UIContainerRect';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UIContainerStack = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UIContainerStack, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UIContainerStack.displayName = 'proto.engineinterface.PB_UIContainerStack';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UIImage = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UIImage, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UIImage.displayName = 'proto.engineinterface.PB_UIImage';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UUIDCallback = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UUIDCallback, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UUIDCallback.displayName = 'proto.engineinterface.PB_UUIDCallback';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UIInputText = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UIInputText, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UIInputText.displayName = 'proto.engineinterface.PB_UIInputText';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UIScrollRect = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UIScrollRect, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UIScrollRect.displayName = 'proto.engineinterface.PB_UIScrollRect';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UIShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UIShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UIShape.displayName = 'proto.engineinterface.PB_UIShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_UITextShape = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_UITextShape, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_UITextShape.displayName = 'proto.engineinterface.PB_UITextShape';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_OpenExternalUrl = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_OpenExternalUrl, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_OpenExternalUrl.displayName = 'proto.engineinterface.PB_OpenExternalUrl';
}
/**
 * Generated by JsPbCodeGenerator.
 * @param {Array=} opt_data Optional initial data array, typically from a
 * server response, or constructed directly in Javascript. The array is used
 * in place and becomes part of the constructed object. It is not cloned.
 * If no data is provided, the constructed object will be empty, but still
 * valid.
 * @extends {jspb.Message}
 * @constructor
 */
proto.engineinterface.PB_OpenNFTDialog = function(opt_data) {
  jspb.Message.initialize(this, opt_data, 0, -1, null, null);
};
goog.inherits(proto.engineinterface.PB_OpenNFTDialog, jspb.Message);
if (goog.DEBUG && !COMPILED) {
  /**
   * @public
   * @override
   */
  proto.engineinterface.PB_OpenNFTDialog.displayName = 'proto.engineinterface.PB_OpenNFTDialog';
}



if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_CreateEntity.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_CreateEntity.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_CreateEntity} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_CreateEntity.toObject = function(includeInstance, msg) {
  var f, obj = {
    id: jspb.Message.getFieldWithDefault(msg, 1, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_CreateEntity}
 */
proto.engineinterface.PB_CreateEntity.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_CreateEntity;
  return proto.engineinterface.PB_CreateEntity.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_CreateEntity} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_CreateEntity}
 */
proto.engineinterface.PB_CreateEntity.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setId(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_CreateEntity.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_CreateEntity.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_CreateEntity} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_CreateEntity.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getId();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
};


/**
 * optional string id = 1;
 * @return {string}
 */
proto.engineinterface.PB_CreateEntity.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_CreateEntity} returns this
 */
proto.engineinterface.PB_CreateEntity.prototype.setId = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_RemoveEntity.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_RemoveEntity.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_RemoveEntity} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_RemoveEntity.toObject = function(includeInstance, msg) {
  var f, obj = {
    id: jspb.Message.getFieldWithDefault(msg, 1, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_RemoveEntity}
 */
proto.engineinterface.PB_RemoveEntity.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_RemoveEntity;
  return proto.engineinterface.PB_RemoveEntity.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_RemoveEntity} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_RemoveEntity}
 */
proto.engineinterface.PB_RemoveEntity.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setId(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_RemoveEntity.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_RemoveEntity.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_RemoveEntity} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_RemoveEntity.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getId();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
};


/**
 * optional string id = 1;
 * @return {string}
 */
proto.engineinterface.PB_RemoveEntity.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_RemoveEntity} returns this
 */
proto.engineinterface.PB_RemoveEntity.prototype.setId = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_SetEntityParent.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_SetEntityParent.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_SetEntityParent} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_SetEntityParent.toObject = function(includeInstance, msg) {
  var f, obj = {
    entityid: jspb.Message.getFieldWithDefault(msg, 1, ""),
    parentid: jspb.Message.getFieldWithDefault(msg, 2, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_SetEntityParent}
 */
proto.engineinterface.PB_SetEntityParent.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_SetEntityParent;
  return proto.engineinterface.PB_SetEntityParent.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_SetEntityParent} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_SetEntityParent}
 */
proto.engineinterface.PB_SetEntityParent.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setEntityid(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setParentid(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_SetEntityParent.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_SetEntityParent.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_SetEntityParent} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_SetEntityParent.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getEntityid();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getParentid();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
};


/**
 * optional string entityId = 1;
 * @return {string}
 */
proto.engineinterface.PB_SetEntityParent.prototype.getEntityid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_SetEntityParent} returns this
 */
proto.engineinterface.PB_SetEntityParent.prototype.setEntityid = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string parentId = 2;
 * @return {string}
 */
proto.engineinterface.PB_SetEntityParent.prototype.getParentid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_SetEntityParent} returns this
 */
proto.engineinterface.PB_SetEntityParent.prototype.setParentid = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_ComponentRemoved.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_ComponentRemoved.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_ComponentRemoved} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ComponentRemoved.toObject = function(includeInstance, msg) {
  var f, obj = {
    entityid: jspb.Message.getFieldWithDefault(msg, 1, ""),
    name: jspb.Message.getFieldWithDefault(msg, 2, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_ComponentRemoved}
 */
proto.engineinterface.PB_ComponentRemoved.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_ComponentRemoved;
  return proto.engineinterface.PB_ComponentRemoved.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_ComponentRemoved} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_ComponentRemoved}
 */
proto.engineinterface.PB_ComponentRemoved.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setEntityid(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_ComponentRemoved.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_ComponentRemoved.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_ComponentRemoved} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ComponentRemoved.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getEntityid();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
};


/**
 * optional string entityId = 1;
 * @return {string}
 */
proto.engineinterface.PB_ComponentRemoved.prototype.getEntityid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_ComponentRemoved} returns this
 */
proto.engineinterface.PB_ComponentRemoved.prototype.setEntityid = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string name = 2;
 * @return {string}
 */
proto.engineinterface.PB_ComponentRemoved.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_ComponentRemoved} returns this
 */
proto.engineinterface.PB_ComponentRemoved.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};



/**
 * Oneof group definitions for this message. Each group defines the field
 * numbers belonging to that group. When of these fields' value is set, all
 * other fields in the group are cleared. During deserialization, if multiple
 * fields are encountered for a group, only the last value seen will be kept.
 * @private {!Array<!Array<number>>}
 * @const
 */
proto.engineinterface.PB_Component.oneofGroups_ = [[1,8,16,17,18,19,20,21,22,25,26,27,28,29,31,32,54,55,56,64,68,200,201]];

/**
 * @enum {number}
 */
proto.engineinterface.PB_Component.ModelCase = {
  MODEL_NOT_SET: 0,
  TRANSFORM: 1,
  UUIDCALLBACK: 8,
  BOX: 16,
  SPHERE: 17,
  PLANE: 18,
  CONE: 19,
  CYLINDER: 20,
  TEXT: 21,
  NFT: 22,
  CONTAINERRECT: 25,
  CONTAINERSTACK: 26,
  UITEXTSHAPE: 27,
  UIINPUTTEXTSHAPE: 28,
  UIIMAGESHAPE: 29,
  CIRCLE: 31,
  BILLBOARD: 32,
  GLTF: 54,
  OBJ: 55,
  AVATAR: 56,
  BASICMATERIAL: 64,
  TEXTURE: 68,
  AUDIOCLIP: 200,
  AUDIOSOURCE: 201
};

/**
 * @return {proto.engineinterface.PB_Component.ModelCase}
 */
proto.engineinterface.PB_Component.prototype.getModelCase = function() {
  return /** @type {proto.engineinterface.PB_Component.ModelCase} */(jspb.Message.computeOneofCase(this, proto.engineinterface.PB_Component.oneofGroups_[0]));
};



if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Component.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Component.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Component} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Component.toObject = function(includeInstance, msg) {
  var f, obj = {
    transform: (f = msg.getTransform()) && proto.engineinterface.PB_Transform.toObject(includeInstance, f),
    uuidcallback: (f = msg.getUuidcallback()) && proto.engineinterface.PB_UUIDCallback.toObject(includeInstance, f),
    box: (f = msg.getBox()) && proto.engineinterface.PB_BoxShape.toObject(includeInstance, f),
    sphere: (f = msg.getSphere()) && proto.engineinterface.PB_SphereShape.toObject(includeInstance, f),
    plane: (f = msg.getPlane()) && proto.engineinterface.PB_PlaneShape.toObject(includeInstance, f),
    cone: (f = msg.getCone()) && proto.engineinterface.PB_ConeShape.toObject(includeInstance, f),
    cylinder: (f = msg.getCylinder()) && proto.engineinterface.PB_CylinderShape.toObject(includeInstance, f),
    text: (f = msg.getText()) && proto.engineinterface.PB_TextShape.toObject(includeInstance, f),
    nft: (f = msg.getNft()) && proto.engineinterface.PB_NFTShape.toObject(includeInstance, f),
    containerrect: (f = msg.getContainerrect()) && proto.engineinterface.PB_UIContainerRect.toObject(includeInstance, f),
    containerstack: (f = msg.getContainerstack()) && proto.engineinterface.PB_UIContainerStack.toObject(includeInstance, f),
    uitextshape: (f = msg.getUitextshape()) && proto.engineinterface.PB_UITextShape.toObject(includeInstance, f),
    uiinputtextshape: (f = msg.getUiinputtextshape()) && proto.engineinterface.PB_UIInputText.toObject(includeInstance, f),
    uiimageshape: (f = msg.getUiimageshape()) && proto.engineinterface.PB_UIImage.toObject(includeInstance, f),
    circle: (f = msg.getCircle()) && proto.engineinterface.PB_CircleShape.toObject(includeInstance, f),
    billboard: (f = msg.getBillboard()) && proto.engineinterface.PB_Billboard.toObject(includeInstance, f),
    gltf: (f = msg.getGltf()) && proto.engineinterface.PB_GLTFShape.toObject(includeInstance, f),
    obj: (f = msg.getObj()) && proto.engineinterface.PB_OBJShape.toObject(includeInstance, f),
    avatar: (f = msg.getAvatar()) && proto.engineinterface.PB_AvatarShape.toObject(includeInstance, f),
    basicmaterial: (f = msg.getBasicmaterial()) && proto.engineinterface.PB_BasicMaterial.toObject(includeInstance, f),
    texture: (f = msg.getTexture()) && proto.engineinterface.PB_Texture.toObject(includeInstance, f),
    audioclip: (f = msg.getAudioclip()) && proto.engineinterface.PB_AudioClip.toObject(includeInstance, f),
    audiosource: (f = msg.getAudiosource()) && proto.engineinterface.PB_AudioSource.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Component}
 */
proto.engineinterface.PB_Component.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Component;
  return proto.engineinterface.PB_Component.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Component} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Component}
 */
proto.engineinterface.PB_Component.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = new proto.engineinterface.PB_Transform;
      reader.readMessage(value,proto.engineinterface.PB_Transform.deserializeBinaryFromReader);
      msg.setTransform(value);
      break;
    case 8:
      var value = new proto.engineinterface.PB_UUIDCallback;
      reader.readMessage(value,proto.engineinterface.PB_UUIDCallback.deserializeBinaryFromReader);
      msg.setUuidcallback(value);
      break;
    case 16:
      var value = new proto.engineinterface.PB_BoxShape;
      reader.readMessage(value,proto.engineinterface.PB_BoxShape.deserializeBinaryFromReader);
      msg.setBox(value);
      break;
    case 17:
      var value = new proto.engineinterface.PB_SphereShape;
      reader.readMessage(value,proto.engineinterface.PB_SphereShape.deserializeBinaryFromReader);
      msg.setSphere(value);
      break;
    case 18:
      var value = new proto.engineinterface.PB_PlaneShape;
      reader.readMessage(value,proto.engineinterface.PB_PlaneShape.deserializeBinaryFromReader);
      msg.setPlane(value);
      break;
    case 19:
      var value = new proto.engineinterface.PB_ConeShape;
      reader.readMessage(value,proto.engineinterface.PB_ConeShape.deserializeBinaryFromReader);
      msg.setCone(value);
      break;
    case 20:
      var value = new proto.engineinterface.PB_CylinderShape;
      reader.readMessage(value,proto.engineinterface.PB_CylinderShape.deserializeBinaryFromReader);
      msg.setCylinder(value);
      break;
    case 21:
      var value = new proto.engineinterface.PB_TextShape;
      reader.readMessage(value,proto.engineinterface.PB_TextShape.deserializeBinaryFromReader);
      msg.setText(value);
      break;
    case 22:
      var value = new proto.engineinterface.PB_NFTShape;
      reader.readMessage(value,proto.engineinterface.PB_NFTShape.deserializeBinaryFromReader);
      msg.setNft(value);
      break;
    case 25:
      var value = new proto.engineinterface.PB_UIContainerRect;
      reader.readMessage(value,proto.engineinterface.PB_UIContainerRect.deserializeBinaryFromReader);
      msg.setContainerrect(value);
      break;
    case 26:
      var value = new proto.engineinterface.PB_UIContainerStack;
      reader.readMessage(value,proto.engineinterface.PB_UIContainerStack.deserializeBinaryFromReader);
      msg.setContainerstack(value);
      break;
    case 27:
      var value = new proto.engineinterface.PB_UITextShape;
      reader.readMessage(value,proto.engineinterface.PB_UITextShape.deserializeBinaryFromReader);
      msg.setUitextshape(value);
      break;
    case 28:
      var value = new proto.engineinterface.PB_UIInputText;
      reader.readMessage(value,proto.engineinterface.PB_UIInputText.deserializeBinaryFromReader);
      msg.setUiinputtextshape(value);
      break;
    case 29:
      var value = new proto.engineinterface.PB_UIImage;
      reader.readMessage(value,proto.engineinterface.PB_UIImage.deserializeBinaryFromReader);
      msg.setUiimageshape(value);
      break;
    case 31:
      var value = new proto.engineinterface.PB_CircleShape;
      reader.readMessage(value,proto.engineinterface.PB_CircleShape.deserializeBinaryFromReader);
      msg.setCircle(value);
      break;
    case 32:
      var value = new proto.engineinterface.PB_Billboard;
      reader.readMessage(value,proto.engineinterface.PB_Billboard.deserializeBinaryFromReader);
      msg.setBillboard(value);
      break;
    case 54:
      var value = new proto.engineinterface.PB_GLTFShape;
      reader.readMessage(value,proto.engineinterface.PB_GLTFShape.deserializeBinaryFromReader);
      msg.setGltf(value);
      break;
    case 55:
      var value = new proto.engineinterface.PB_OBJShape;
      reader.readMessage(value,proto.engineinterface.PB_OBJShape.deserializeBinaryFromReader);
      msg.setObj(value);
      break;
    case 56:
      var value = new proto.engineinterface.PB_AvatarShape;
      reader.readMessage(value,proto.engineinterface.PB_AvatarShape.deserializeBinaryFromReader);
      msg.setAvatar(value);
      break;
    case 64:
      var value = new proto.engineinterface.PB_BasicMaterial;
      reader.readMessage(value,proto.engineinterface.PB_BasicMaterial.deserializeBinaryFromReader);
      msg.setBasicmaterial(value);
      break;
    case 68:
      var value = new proto.engineinterface.PB_Texture;
      reader.readMessage(value,proto.engineinterface.PB_Texture.deserializeBinaryFromReader);
      msg.setTexture(value);
      break;
    case 200:
      var value = new proto.engineinterface.PB_AudioClip;
      reader.readMessage(value,proto.engineinterface.PB_AudioClip.deserializeBinaryFromReader);
      msg.setAudioclip(value);
      break;
    case 201:
      var value = new proto.engineinterface.PB_AudioSource;
      reader.readMessage(value,proto.engineinterface.PB_AudioSource.deserializeBinaryFromReader);
      msg.setAudiosource(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Component.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Component.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Component} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Component.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getTransform();
  if (f != null) {
    writer.writeMessage(
      1,
      f,
      proto.engineinterface.PB_Transform.serializeBinaryToWriter
    );
  }
  f = message.getUuidcallback();
  if (f != null) {
    writer.writeMessage(
      8,
      f,
      proto.engineinterface.PB_UUIDCallback.serializeBinaryToWriter
    );
  }
  f = message.getBox();
  if (f != null) {
    writer.writeMessage(
      16,
      f,
      proto.engineinterface.PB_BoxShape.serializeBinaryToWriter
    );
  }
  f = message.getSphere();
  if (f != null) {
    writer.writeMessage(
      17,
      f,
      proto.engineinterface.PB_SphereShape.serializeBinaryToWriter
    );
  }
  f = message.getPlane();
  if (f != null) {
    writer.writeMessage(
      18,
      f,
      proto.engineinterface.PB_PlaneShape.serializeBinaryToWriter
    );
  }
  f = message.getCone();
  if (f != null) {
    writer.writeMessage(
      19,
      f,
      proto.engineinterface.PB_ConeShape.serializeBinaryToWriter
    );
  }
  f = message.getCylinder();
  if (f != null) {
    writer.writeMessage(
      20,
      f,
      proto.engineinterface.PB_CylinderShape.serializeBinaryToWriter
    );
  }
  f = message.getText();
  if (f != null) {
    writer.writeMessage(
      21,
      f,
      proto.engineinterface.PB_TextShape.serializeBinaryToWriter
    );
  }
  f = message.getNft();
  if (f != null) {
    writer.writeMessage(
      22,
      f,
      proto.engineinterface.PB_NFTShape.serializeBinaryToWriter
    );
  }
  f = message.getContainerrect();
  if (f != null) {
    writer.writeMessage(
      25,
      f,
      proto.engineinterface.PB_UIContainerRect.serializeBinaryToWriter
    );
  }
  f = message.getContainerstack();
  if (f != null) {
    writer.writeMessage(
      26,
      f,
      proto.engineinterface.PB_UIContainerStack.serializeBinaryToWriter
    );
  }
  f = message.getUitextshape();
  if (f != null) {
    writer.writeMessage(
      27,
      f,
      proto.engineinterface.PB_UITextShape.serializeBinaryToWriter
    );
  }
  f = message.getUiinputtextshape();
  if (f != null) {
    writer.writeMessage(
      28,
      f,
      proto.engineinterface.PB_UIInputText.serializeBinaryToWriter
    );
  }
  f = message.getUiimageshape();
  if (f != null) {
    writer.writeMessage(
      29,
      f,
      proto.engineinterface.PB_UIImage.serializeBinaryToWriter
    );
  }
  f = message.getCircle();
  if (f != null) {
    writer.writeMessage(
      31,
      f,
      proto.engineinterface.PB_CircleShape.serializeBinaryToWriter
    );
  }
  f = message.getBillboard();
  if (f != null) {
    writer.writeMessage(
      32,
      f,
      proto.engineinterface.PB_Billboard.serializeBinaryToWriter
    );
  }
  f = message.getGltf();
  if (f != null) {
    writer.writeMessage(
      54,
      f,
      proto.engineinterface.PB_GLTFShape.serializeBinaryToWriter
    );
  }
  f = message.getObj();
  if (f != null) {
    writer.writeMessage(
      55,
      f,
      proto.engineinterface.PB_OBJShape.serializeBinaryToWriter
    );
  }
  f = message.getAvatar();
  if (f != null) {
    writer.writeMessage(
      56,
      f,
      proto.engineinterface.PB_AvatarShape.serializeBinaryToWriter
    );
  }
  f = message.getBasicmaterial();
  if (f != null) {
    writer.writeMessage(
      64,
      f,
      proto.engineinterface.PB_BasicMaterial.serializeBinaryToWriter
    );
  }
  f = message.getTexture();
  if (f != null) {
    writer.writeMessage(
      68,
      f,
      proto.engineinterface.PB_Texture.serializeBinaryToWriter
    );
  }
  f = message.getAudioclip();
  if (f != null) {
    writer.writeMessage(
      200,
      f,
      proto.engineinterface.PB_AudioClip.serializeBinaryToWriter
    );
  }
  f = message.getAudiosource();
  if (f != null) {
    writer.writeMessage(
      201,
      f,
      proto.engineinterface.PB_AudioSource.serializeBinaryToWriter
    );
  }
};


/**
 * optional PB_Transform transform = 1;
 * @return {?proto.engineinterface.PB_Transform}
 */
proto.engineinterface.PB_Component.prototype.getTransform = function() {
  return /** @type{?proto.engineinterface.PB_Transform} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Transform, 1));
};


/**
 * @param {?proto.engineinterface.PB_Transform|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setTransform = function(value) {
  return jspb.Message.setOneofWrapperField(this, 1, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearTransform = function() {
  return this.setTransform(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasTransform = function() {
  return jspb.Message.getField(this, 1) != null;
};


/**
 * optional PB_UUIDCallback uuidCallback = 8;
 * @return {?proto.engineinterface.PB_UUIDCallback}
 */
proto.engineinterface.PB_Component.prototype.getUuidcallback = function() {
  return /** @type{?proto.engineinterface.PB_UUIDCallback} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UUIDCallback, 8));
};


/**
 * @param {?proto.engineinterface.PB_UUIDCallback|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setUuidcallback = function(value) {
  return jspb.Message.setOneofWrapperField(this, 8, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearUuidcallback = function() {
  return this.setUuidcallback(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasUuidcallback = function() {
  return jspb.Message.getField(this, 8) != null;
};


/**
 * optional PB_BoxShape box = 16;
 * @return {?proto.engineinterface.PB_BoxShape}
 */
proto.engineinterface.PB_Component.prototype.getBox = function() {
  return /** @type{?proto.engineinterface.PB_BoxShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_BoxShape, 16));
};


/**
 * @param {?proto.engineinterface.PB_BoxShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setBox = function(value) {
  return jspb.Message.setOneofWrapperField(this, 16, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearBox = function() {
  return this.setBox(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasBox = function() {
  return jspb.Message.getField(this, 16) != null;
};


/**
 * optional PB_SphereShape sphere = 17;
 * @return {?proto.engineinterface.PB_SphereShape}
 */
proto.engineinterface.PB_Component.prototype.getSphere = function() {
  return /** @type{?proto.engineinterface.PB_SphereShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_SphereShape, 17));
};


/**
 * @param {?proto.engineinterface.PB_SphereShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setSphere = function(value) {
  return jspb.Message.setOneofWrapperField(this, 17, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearSphere = function() {
  return this.setSphere(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasSphere = function() {
  return jspb.Message.getField(this, 17) != null;
};


/**
 * optional PB_PlaneShape plane = 18;
 * @return {?proto.engineinterface.PB_PlaneShape}
 */
proto.engineinterface.PB_Component.prototype.getPlane = function() {
  return /** @type{?proto.engineinterface.PB_PlaneShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_PlaneShape, 18));
};


/**
 * @param {?proto.engineinterface.PB_PlaneShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setPlane = function(value) {
  return jspb.Message.setOneofWrapperField(this, 18, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearPlane = function() {
  return this.setPlane(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasPlane = function() {
  return jspb.Message.getField(this, 18) != null;
};


/**
 * optional PB_ConeShape cone = 19;
 * @return {?proto.engineinterface.PB_ConeShape}
 */
proto.engineinterface.PB_Component.prototype.getCone = function() {
  return /** @type{?proto.engineinterface.PB_ConeShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_ConeShape, 19));
};


/**
 * @param {?proto.engineinterface.PB_ConeShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setCone = function(value) {
  return jspb.Message.setOneofWrapperField(this, 19, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearCone = function() {
  return this.setCone(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasCone = function() {
  return jspb.Message.getField(this, 19) != null;
};


/**
 * optional PB_CylinderShape cylinder = 20;
 * @return {?proto.engineinterface.PB_CylinderShape}
 */
proto.engineinterface.PB_Component.prototype.getCylinder = function() {
  return /** @type{?proto.engineinterface.PB_CylinderShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_CylinderShape, 20));
};


/**
 * @param {?proto.engineinterface.PB_CylinderShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setCylinder = function(value) {
  return jspb.Message.setOneofWrapperField(this, 20, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearCylinder = function() {
  return this.setCylinder(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasCylinder = function() {
  return jspb.Message.getField(this, 20) != null;
};


/**
 * optional PB_TextShape text = 21;
 * @return {?proto.engineinterface.PB_TextShape}
 */
proto.engineinterface.PB_Component.prototype.getText = function() {
  return /** @type{?proto.engineinterface.PB_TextShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_TextShape, 21));
};


/**
 * @param {?proto.engineinterface.PB_TextShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setText = function(value) {
  return jspb.Message.setOneofWrapperField(this, 21, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearText = function() {
  return this.setText(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasText = function() {
  return jspb.Message.getField(this, 21) != null;
};


/**
 * optional PB_NFTShape nft = 22;
 * @return {?proto.engineinterface.PB_NFTShape}
 */
proto.engineinterface.PB_Component.prototype.getNft = function() {
  return /** @type{?proto.engineinterface.PB_NFTShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_NFTShape, 22));
};


/**
 * @param {?proto.engineinterface.PB_NFTShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setNft = function(value) {
  return jspb.Message.setOneofWrapperField(this, 22, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearNft = function() {
  return this.setNft(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasNft = function() {
  return jspb.Message.getField(this, 22) != null;
};


/**
 * optional PB_UIContainerRect containerRect = 25;
 * @return {?proto.engineinterface.PB_UIContainerRect}
 */
proto.engineinterface.PB_Component.prototype.getContainerrect = function() {
  return /** @type{?proto.engineinterface.PB_UIContainerRect} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIContainerRect, 25));
};


/**
 * @param {?proto.engineinterface.PB_UIContainerRect|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setContainerrect = function(value) {
  return jspb.Message.setOneofWrapperField(this, 25, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearContainerrect = function() {
  return this.setContainerrect(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasContainerrect = function() {
  return jspb.Message.getField(this, 25) != null;
};


/**
 * optional PB_UIContainerStack containerStack = 26;
 * @return {?proto.engineinterface.PB_UIContainerStack}
 */
proto.engineinterface.PB_Component.prototype.getContainerstack = function() {
  return /** @type{?proto.engineinterface.PB_UIContainerStack} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIContainerStack, 26));
};


/**
 * @param {?proto.engineinterface.PB_UIContainerStack|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setContainerstack = function(value) {
  return jspb.Message.setOneofWrapperField(this, 26, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearContainerstack = function() {
  return this.setContainerstack(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasContainerstack = function() {
  return jspb.Message.getField(this, 26) != null;
};


/**
 * optional PB_UITextShape uiTextShape = 27;
 * @return {?proto.engineinterface.PB_UITextShape}
 */
proto.engineinterface.PB_Component.prototype.getUitextshape = function() {
  return /** @type{?proto.engineinterface.PB_UITextShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UITextShape, 27));
};


/**
 * @param {?proto.engineinterface.PB_UITextShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setUitextshape = function(value) {
  return jspb.Message.setOneofWrapperField(this, 27, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearUitextshape = function() {
  return this.setUitextshape(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasUitextshape = function() {
  return jspb.Message.getField(this, 27) != null;
};


/**
 * optional PB_UIInputText uiInputTextShape = 28;
 * @return {?proto.engineinterface.PB_UIInputText}
 */
proto.engineinterface.PB_Component.prototype.getUiinputtextshape = function() {
  return /** @type{?proto.engineinterface.PB_UIInputText} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIInputText, 28));
};


/**
 * @param {?proto.engineinterface.PB_UIInputText|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setUiinputtextshape = function(value) {
  return jspb.Message.setOneofWrapperField(this, 28, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearUiinputtextshape = function() {
  return this.setUiinputtextshape(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasUiinputtextshape = function() {
  return jspb.Message.getField(this, 28) != null;
};


/**
 * optional PB_UIImage uiImageShape = 29;
 * @return {?proto.engineinterface.PB_UIImage}
 */
proto.engineinterface.PB_Component.prototype.getUiimageshape = function() {
  return /** @type{?proto.engineinterface.PB_UIImage} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIImage, 29));
};


/**
 * @param {?proto.engineinterface.PB_UIImage|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setUiimageshape = function(value) {
  return jspb.Message.setOneofWrapperField(this, 29, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearUiimageshape = function() {
  return this.setUiimageshape(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasUiimageshape = function() {
  return jspb.Message.getField(this, 29) != null;
};


/**
 * optional PB_CircleShape circle = 31;
 * @return {?proto.engineinterface.PB_CircleShape}
 */
proto.engineinterface.PB_Component.prototype.getCircle = function() {
  return /** @type{?proto.engineinterface.PB_CircleShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_CircleShape, 31));
};


/**
 * @param {?proto.engineinterface.PB_CircleShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setCircle = function(value) {
  return jspb.Message.setOneofWrapperField(this, 31, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearCircle = function() {
  return this.setCircle(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasCircle = function() {
  return jspb.Message.getField(this, 31) != null;
};


/**
 * optional PB_Billboard billboard = 32;
 * @return {?proto.engineinterface.PB_Billboard}
 */
proto.engineinterface.PB_Component.prototype.getBillboard = function() {
  return /** @type{?proto.engineinterface.PB_Billboard} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Billboard, 32));
};


/**
 * @param {?proto.engineinterface.PB_Billboard|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setBillboard = function(value) {
  return jspb.Message.setOneofWrapperField(this, 32, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearBillboard = function() {
  return this.setBillboard(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasBillboard = function() {
  return jspb.Message.getField(this, 32) != null;
};


/**
 * optional PB_GLTFShape gltf = 54;
 * @return {?proto.engineinterface.PB_GLTFShape}
 */
proto.engineinterface.PB_Component.prototype.getGltf = function() {
  return /** @type{?proto.engineinterface.PB_GLTFShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_GLTFShape, 54));
};


/**
 * @param {?proto.engineinterface.PB_GLTFShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setGltf = function(value) {
  return jspb.Message.setOneofWrapperField(this, 54, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearGltf = function() {
  return this.setGltf(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasGltf = function() {
  return jspb.Message.getField(this, 54) != null;
};


/**
 * optional PB_OBJShape obj = 55;
 * @return {?proto.engineinterface.PB_OBJShape}
 */
proto.engineinterface.PB_Component.prototype.getObj = function() {
  return /** @type{?proto.engineinterface.PB_OBJShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_OBJShape, 55));
};


/**
 * @param {?proto.engineinterface.PB_OBJShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setObj = function(value) {
  return jspb.Message.setOneofWrapperField(this, 55, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearObj = function() {
  return this.setObj(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasObj = function() {
  return jspb.Message.getField(this, 55) != null;
};


/**
 * optional PB_AvatarShape avatar = 56;
 * @return {?proto.engineinterface.PB_AvatarShape}
 */
proto.engineinterface.PB_Component.prototype.getAvatar = function() {
  return /** @type{?proto.engineinterface.PB_AvatarShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_AvatarShape, 56));
};


/**
 * @param {?proto.engineinterface.PB_AvatarShape|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setAvatar = function(value) {
  return jspb.Message.setOneofWrapperField(this, 56, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearAvatar = function() {
  return this.setAvatar(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasAvatar = function() {
  return jspb.Message.getField(this, 56) != null;
};


/**
 * optional PB_BasicMaterial basicMaterial = 64;
 * @return {?proto.engineinterface.PB_BasicMaterial}
 */
proto.engineinterface.PB_Component.prototype.getBasicmaterial = function() {
  return /** @type{?proto.engineinterface.PB_BasicMaterial} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_BasicMaterial, 64));
};


/**
 * @param {?proto.engineinterface.PB_BasicMaterial|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setBasicmaterial = function(value) {
  return jspb.Message.setOneofWrapperField(this, 64, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearBasicmaterial = function() {
  return this.setBasicmaterial(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasBasicmaterial = function() {
  return jspb.Message.getField(this, 64) != null;
};


/**
 * optional PB_Texture texture = 68;
 * @return {?proto.engineinterface.PB_Texture}
 */
proto.engineinterface.PB_Component.prototype.getTexture = function() {
  return /** @type{?proto.engineinterface.PB_Texture} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Texture, 68));
};


/**
 * @param {?proto.engineinterface.PB_Texture|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setTexture = function(value) {
  return jspb.Message.setOneofWrapperField(this, 68, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearTexture = function() {
  return this.setTexture(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasTexture = function() {
  return jspb.Message.getField(this, 68) != null;
};


/**
 * optional PB_AudioClip audioClip = 200;
 * @return {?proto.engineinterface.PB_AudioClip}
 */
proto.engineinterface.PB_Component.prototype.getAudioclip = function() {
  return /** @type{?proto.engineinterface.PB_AudioClip} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_AudioClip, 200));
};


/**
 * @param {?proto.engineinterface.PB_AudioClip|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setAudioclip = function(value) {
  return jspb.Message.setOneofWrapperField(this, 200, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearAudioclip = function() {
  return this.setAudioclip(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasAudioclip = function() {
  return jspb.Message.getField(this, 200) != null;
};


/**
 * optional PB_AudioSource audioSource = 201;
 * @return {?proto.engineinterface.PB_AudioSource}
 */
proto.engineinterface.PB_Component.prototype.getAudiosource = function() {
  return /** @type{?proto.engineinterface.PB_AudioSource} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_AudioSource, 201));
};


/**
 * @param {?proto.engineinterface.PB_AudioSource|undefined} value
 * @return {!proto.engineinterface.PB_Component} returns this
*/
proto.engineinterface.PB_Component.prototype.setAudiosource = function(value) {
  return jspb.Message.setOneofWrapperField(this, 201, proto.engineinterface.PB_Component.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Component} returns this
 */
proto.engineinterface.PB_Component.prototype.clearAudiosource = function() {
  return this.setAudiosource(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Component.prototype.hasAudiosource = function() {
  return jspb.Message.getField(this, 201) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Color4.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Color4.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Color4} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Color4.toObject = function(includeInstance, msg) {
  var f, obj = {
    r: jspb.Message.getFloatingPointFieldWithDefault(msg, 1, 0.0),
    g: jspb.Message.getFloatingPointFieldWithDefault(msg, 2, 0.0),
    b: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    a: jspb.Message.getFloatingPointFieldWithDefault(msg, 4, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_Color4.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Color4;
  return proto.engineinterface.PB_Color4.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Color4} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_Color4.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setR(value);
      break;
    case 2:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setG(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setB(value);
      break;
    case 4:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setA(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Color4.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Color4.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Color4} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Color4.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getR();
  if (f !== 0.0) {
    writer.writeFloat(
      1,
      f
    );
  }
  f = message.getG();
  if (f !== 0.0) {
    writer.writeFloat(
      2,
      f
    );
  }
  f = message.getB();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getA();
  if (f !== 0.0) {
    writer.writeFloat(
      4,
      f
    );
  }
};


/**
 * optional float r = 1;
 * @return {number}
 */
proto.engineinterface.PB_Color4.prototype.getR = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 1, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Color4} returns this
 */
proto.engineinterface.PB_Color4.prototype.setR = function(value) {
  return jspb.Message.setProto3FloatField(this, 1, value);
};


/**
 * optional float g = 2;
 * @return {number}
 */
proto.engineinterface.PB_Color4.prototype.getG = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 2, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Color4} returns this
 */
proto.engineinterface.PB_Color4.prototype.setG = function(value) {
  return jspb.Message.setProto3FloatField(this, 2, value);
};


/**
 * optional float b = 3;
 * @return {number}
 */
proto.engineinterface.PB_Color4.prototype.getB = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Color4} returns this
 */
proto.engineinterface.PB_Color4.prototype.setB = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional float a = 4;
 * @return {number}
 */
proto.engineinterface.PB_Color4.prototype.getA = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 4, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Color4} returns this
 */
proto.engineinterface.PB_Color4.prototype.setA = function(value) {
  return jspb.Message.setProto3FloatField(this, 4, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Color3.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Color3.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Color3} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Color3.toObject = function(includeInstance, msg) {
  var f, obj = {
    r: jspb.Message.getFloatingPointFieldWithDefault(msg, 1, 0.0),
    g: jspb.Message.getFloatingPointFieldWithDefault(msg, 2, 0.0),
    b: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_Color3.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Color3;
  return proto.engineinterface.PB_Color3.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Color3} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_Color3.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setR(value);
      break;
    case 2:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setG(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setB(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Color3.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Color3.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Color3} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Color3.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getR();
  if (f !== 0.0) {
    writer.writeFloat(
      1,
      f
    );
  }
  f = message.getG();
  if (f !== 0.0) {
    writer.writeFloat(
      2,
      f
    );
  }
  f = message.getB();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
};


/**
 * optional float r = 1;
 * @return {number}
 */
proto.engineinterface.PB_Color3.prototype.getR = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 1, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Color3} returns this
 */
proto.engineinterface.PB_Color3.prototype.setR = function(value) {
  return jspb.Message.setProto3FloatField(this, 1, value);
};


/**
 * optional float g = 2;
 * @return {number}
 */
proto.engineinterface.PB_Color3.prototype.getG = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 2, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Color3} returns this
 */
proto.engineinterface.PB_Color3.prototype.setG = function(value) {
  return jspb.Message.setProto3FloatField(this, 2, value);
};


/**
 * optional float b = 3;
 * @return {number}
 */
proto.engineinterface.PB_Color3.prototype.getB = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Color3} returns this
 */
proto.engineinterface.PB_Color3.prototype.setB = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_TextShapeModel.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_TextShapeModel.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_TextShapeModel} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_TextShapeModel.toObject = function(includeInstance, msg) {
  var f, obj = {
    billboard: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    value: jspb.Message.getFieldWithDefault(msg, 2, ""),
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 4, 0.0),
    fontsize: jspb.Message.getFloatingPointFieldWithDefault(msg, 5, 0.0),
    fontautosize: jspb.Message.getBooleanFieldWithDefault(msg, 6, false),
    fontweight: jspb.Message.getFieldWithDefault(msg, 7, ""),
    htextalign: jspb.Message.getFieldWithDefault(msg, 8, ""),
    vtextalign: jspb.Message.getFieldWithDefault(msg, 9, ""),
    width: jspb.Message.getFloatingPointFieldWithDefault(msg, 10, 0.0),
    height: jspb.Message.getFloatingPointFieldWithDefault(msg, 11, 0.0),
    adaptwidth: jspb.Message.getBooleanFieldWithDefault(msg, 12, false),
    adaptheight: jspb.Message.getBooleanFieldWithDefault(msg, 13, false),
    paddingtop: jspb.Message.getFloatingPointFieldWithDefault(msg, 14, 0.0),
    paddingright: jspb.Message.getFloatingPointFieldWithDefault(msg, 15, 0.0),
    paddingbottom: jspb.Message.getFloatingPointFieldWithDefault(msg, 16, 0.0),
    paddingleft: jspb.Message.getFloatingPointFieldWithDefault(msg, 17, 0.0),
    linespacing: jspb.Message.getFloatingPointFieldWithDefault(msg, 18, 0.0),
    linecount: jspb.Message.getFieldWithDefault(msg, 19, 0),
    textwrapping: jspb.Message.getBooleanFieldWithDefault(msg, 20, false),
    shadowblur: jspb.Message.getFloatingPointFieldWithDefault(msg, 21, 0.0),
    shadowoffsetx: jspb.Message.getFloatingPointFieldWithDefault(msg, 22, 0.0),
    shadowoffsety: jspb.Message.getFloatingPointFieldWithDefault(msg, 23, 0.0),
    shadowcolor: (f = msg.getShadowcolor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f),
    outlinewidth: jspb.Message.getFloatingPointFieldWithDefault(msg, 25, 0.0),
    outlinecolor: (f = msg.getOutlinecolor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_TextShapeModel}
 */
proto.engineinterface.PB_TextShapeModel.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_TextShapeModel;
  return proto.engineinterface.PB_TextShapeModel.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_TextShapeModel} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_TextShapeModel}
 */
proto.engineinterface.PB_TextShapeModel.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setBillboard(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setValue(value);
      break;
    case 3:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    case 4:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 5:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setFontsize(value);
      break;
    case 6:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setFontautosize(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setFontweight(value);
      break;
    case 8:
      var value = /** @type {string} */ (reader.readString());
      msg.setHtextalign(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setVtextalign(value);
      break;
    case 10:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setWidth(value);
      break;
    case 11:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setHeight(value);
      break;
    case 12:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setAdaptwidth(value);
      break;
    case 13:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setAdaptheight(value);
      break;
    case 14:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingtop(value);
      break;
    case 15:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingright(value);
      break;
    case 16:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingbottom(value);
      break;
    case 17:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingleft(value);
      break;
    case 18:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setLinespacing(value);
      break;
    case 19:
      var value = /** @type {number} */ (reader.readInt32());
      msg.setLinecount(value);
      break;
    case 20:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setTextwrapping(value);
      break;
    case 21:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowblur(value);
      break;
    case 22:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowoffsetx(value);
      break;
    case 23:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowoffsety(value);
      break;
    case 24:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setShadowcolor(value);
      break;
    case 25:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOutlinewidth(value);
      break;
    case 26:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setOutlinecolor(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_TextShapeModel.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_TextShapeModel.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_TextShapeModel} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_TextShapeModel.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getBillboard();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getValue();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      3,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      4,
      f
    );
  }
  f = message.getFontsize();
  if (f !== 0.0) {
    writer.writeFloat(
      5,
      f
    );
  }
  f = message.getFontautosize();
  if (f) {
    writer.writeBool(
      6,
      f
    );
  }
  f = message.getFontweight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getHtextalign();
  if (f.length > 0) {
    writer.writeString(
      8,
      f
    );
  }
  f = message.getVtextalign();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getWidth();
  if (f !== 0.0) {
    writer.writeFloat(
      10,
      f
    );
  }
  f = message.getHeight();
  if (f !== 0.0) {
    writer.writeFloat(
      11,
      f
    );
  }
  f = message.getAdaptwidth();
  if (f) {
    writer.writeBool(
      12,
      f
    );
  }
  f = message.getAdaptheight();
  if (f) {
    writer.writeBool(
      13,
      f
    );
  }
  f = message.getPaddingtop();
  if (f !== 0.0) {
    writer.writeFloat(
      14,
      f
    );
  }
  f = message.getPaddingright();
  if (f !== 0.0) {
    writer.writeFloat(
      15,
      f
    );
  }
  f = message.getPaddingbottom();
  if (f !== 0.0) {
    writer.writeFloat(
      16,
      f
    );
  }
  f = message.getPaddingleft();
  if (f !== 0.0) {
    writer.writeFloat(
      17,
      f
    );
  }
  f = message.getLinespacing();
  if (f !== 0.0) {
    writer.writeFloat(
      18,
      f
    );
  }
  f = message.getLinecount();
  if (f !== 0) {
    writer.writeInt32(
      19,
      f
    );
  }
  f = message.getTextwrapping();
  if (f) {
    writer.writeBool(
      20,
      f
    );
  }
  f = message.getShadowblur();
  if (f !== 0.0) {
    writer.writeFloat(
      21,
      f
    );
  }
  f = message.getShadowoffsetx();
  if (f !== 0.0) {
    writer.writeFloat(
      22,
      f
    );
  }
  f = message.getShadowoffsety();
  if (f !== 0.0) {
    writer.writeFloat(
      23,
      f
    );
  }
  f = message.getShadowcolor();
  if (f != null) {
    writer.writeMessage(
      24,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
  f = message.getOutlinewidth();
  if (f !== 0.0) {
    writer.writeFloat(
      25,
      f
    );
  }
  f = message.getOutlinecolor();
  if (f != null) {
    writer.writeMessage(
      26,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
};


/**
 * optional bool billboard = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getBillboard = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setBillboard = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional string value = 2;
 * @return {string}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getValue = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setValue = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};


/**
 * optional PB_Color3 color = 3;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 3));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
*/
proto.engineinterface.PB_TextShapeModel.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 3, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_TextShapeModel.prototype.hasColor = function() {
  return jspb.Message.getField(this, 3) != null;
};


/**
 * optional float opacity = 4;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 4, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 4, value);
};


/**
 * optional float fontSize = 5;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getFontsize = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 5, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setFontsize = function(value) {
  return jspb.Message.setProto3FloatField(this, 5, value);
};


/**
 * optional bool fontAutoSize = 6;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getFontautosize = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 6, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setFontautosize = function(value) {
  return jspb.Message.setProto3BooleanField(this, 6, value);
};


/**
 * optional string fontWeight = 7;
 * @return {string}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getFontweight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setFontweight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional string hTextAlign = 8;
 * @return {string}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getHtextalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 8, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setHtextalign = function(value) {
  return jspb.Message.setProto3StringField(this, 8, value);
};


/**
 * optional string vTextAlign = 9;
 * @return {string}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getVtextalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setVtextalign = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional float width = 10;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getWidth = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 10, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setWidth = function(value) {
  return jspb.Message.setProto3FloatField(this, 10, value);
};


/**
 * optional float height = 11;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getHeight = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 11, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setHeight = function(value) {
  return jspb.Message.setProto3FloatField(this, 11, value);
};


/**
 * optional bool adaptWidth = 12;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getAdaptwidth = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 12, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setAdaptwidth = function(value) {
  return jspb.Message.setProto3BooleanField(this, 12, value);
};


/**
 * optional bool adaptHeight = 13;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getAdaptheight = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 13, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setAdaptheight = function(value) {
  return jspb.Message.setProto3BooleanField(this, 13, value);
};


/**
 * optional float paddingTop = 14;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getPaddingtop = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 14, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setPaddingtop = function(value) {
  return jspb.Message.setProto3FloatField(this, 14, value);
};


/**
 * optional float paddingRight = 15;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getPaddingright = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 15, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setPaddingright = function(value) {
  return jspb.Message.setProto3FloatField(this, 15, value);
};


/**
 * optional float paddingBottom = 16;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getPaddingbottom = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 16, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setPaddingbottom = function(value) {
  return jspb.Message.setProto3FloatField(this, 16, value);
};


/**
 * optional float paddingLeft = 17;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getPaddingleft = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 17, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setPaddingleft = function(value) {
  return jspb.Message.setProto3FloatField(this, 17, value);
};


/**
 * optional float lineSpacing = 18;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getLinespacing = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 18, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setLinespacing = function(value) {
  return jspb.Message.setProto3FloatField(this, 18, value);
};


/**
 * optional int32 lineCount = 19;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getLinecount = function() {
  return /** @type {number} */ (jspb.Message.getFieldWithDefault(this, 19, 0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setLinecount = function(value) {
  return jspb.Message.setProto3IntField(this, 19, value);
};


/**
 * optional bool textWrapping = 20;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getTextwrapping = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 20, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setTextwrapping = function(value) {
  return jspb.Message.setProto3BooleanField(this, 20, value);
};


/**
 * optional float shadowBlur = 21;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getShadowblur = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 21, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setShadowblur = function(value) {
  return jspb.Message.setProto3FloatField(this, 21, value);
};


/**
 * optional float shadowOffsetX = 22;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getShadowoffsetx = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 22, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setShadowoffsetx = function(value) {
  return jspb.Message.setProto3FloatField(this, 22, value);
};


/**
 * optional float shadowOffsetY = 23;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getShadowoffsety = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 23, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setShadowoffsety = function(value) {
  return jspb.Message.setProto3FloatField(this, 23, value);
};


/**
 * optional PB_Color3 shadowColor = 24;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getShadowcolor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 24));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
*/
proto.engineinterface.PB_TextShapeModel.prototype.setShadowcolor = function(value) {
  return jspb.Message.setWrapperField(this, 24, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.clearShadowcolor = function() {
  return this.setShadowcolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_TextShapeModel.prototype.hasShadowcolor = function() {
  return jspb.Message.getField(this, 24) != null;
};


/**
 * optional float outlineWidth = 25;
 * @return {number}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getOutlinewidth = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 25, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.setOutlinewidth = function(value) {
  return jspb.Message.setProto3FloatField(this, 25, value);
};


/**
 * optional PB_Color3 outlineColor = 26;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_TextShapeModel.prototype.getOutlinecolor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 26));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
*/
proto.engineinterface.PB_TextShapeModel.prototype.setOutlinecolor = function(value) {
  return jspb.Message.setWrapperField(this, 26, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_TextShapeModel} returns this
 */
proto.engineinterface.PB_TextShapeModel.prototype.clearOutlinecolor = function() {
  return this.setOutlinecolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_TextShapeModel.prototype.hasOutlinecolor = function() {
  return jspb.Message.getField(this, 26) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Vector3.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Vector3.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Vector3} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Vector3.toObject = function(includeInstance, msg) {
  var f, obj = {
    x: jspb.Message.getFloatingPointFieldWithDefault(msg, 1, 0.0),
    y: jspb.Message.getFloatingPointFieldWithDefault(msg, 2, 0.0),
    z: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Vector3}
 */
proto.engineinterface.PB_Vector3.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Vector3;
  return proto.engineinterface.PB_Vector3.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Vector3} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Vector3}
 */
proto.engineinterface.PB_Vector3.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setX(value);
      break;
    case 2:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setY(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setZ(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Vector3.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Vector3.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Vector3} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Vector3.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getX();
  if (f !== 0.0) {
    writer.writeFloat(
      1,
      f
    );
  }
  f = message.getY();
  if (f !== 0.0) {
    writer.writeFloat(
      2,
      f
    );
  }
  f = message.getZ();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
};


/**
 * optional float x = 1;
 * @return {number}
 */
proto.engineinterface.PB_Vector3.prototype.getX = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 1, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Vector3} returns this
 */
proto.engineinterface.PB_Vector3.prototype.setX = function(value) {
  return jspb.Message.setProto3FloatField(this, 1, value);
};


/**
 * optional float y = 2;
 * @return {number}
 */
proto.engineinterface.PB_Vector3.prototype.getY = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 2, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Vector3} returns this
 */
proto.engineinterface.PB_Vector3.prototype.setY = function(value) {
  return jspb.Message.setProto3FloatField(this, 2, value);
};


/**
 * optional float z = 3;
 * @return {number}
 */
proto.engineinterface.PB_Vector3.prototype.getZ = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Vector3} returns this
 */
proto.engineinterface.PB_Vector3.prototype.setZ = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Quaternion.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Quaternion.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Quaternion} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Quaternion.toObject = function(includeInstance, msg) {
  var f, obj = {
    x: jspb.Message.getFloatingPointFieldWithDefault(msg, 1, 0.0),
    y: jspb.Message.getFloatingPointFieldWithDefault(msg, 2, 0.0),
    z: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    w: jspb.Message.getFloatingPointFieldWithDefault(msg, 4, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Quaternion}
 */
proto.engineinterface.PB_Quaternion.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Quaternion;
  return proto.engineinterface.PB_Quaternion.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Quaternion} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Quaternion}
 */
proto.engineinterface.PB_Quaternion.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {number} */ (reader.readDouble());
      msg.setX(value);
      break;
    case 2:
      var value = /** @type {number} */ (reader.readDouble());
      msg.setY(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readDouble());
      msg.setZ(value);
      break;
    case 4:
      var value = /** @type {number} */ (reader.readDouble());
      msg.setW(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Quaternion.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Quaternion.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Quaternion} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Quaternion.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getX();
  if (f !== 0.0) {
    writer.writeDouble(
      1,
      f
    );
  }
  f = message.getY();
  if (f !== 0.0) {
    writer.writeDouble(
      2,
      f
    );
  }
  f = message.getZ();
  if (f !== 0.0) {
    writer.writeDouble(
      3,
      f
    );
  }
  f = message.getW();
  if (f !== 0.0) {
    writer.writeDouble(
      4,
      f
    );
  }
};


/**
 * optional double x = 1;
 * @return {number}
 */
proto.engineinterface.PB_Quaternion.prototype.getX = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 1, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Quaternion} returns this
 */
proto.engineinterface.PB_Quaternion.prototype.setX = function(value) {
  return jspb.Message.setProto3FloatField(this, 1, value);
};


/**
 * optional double y = 2;
 * @return {number}
 */
proto.engineinterface.PB_Quaternion.prototype.getY = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 2, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Quaternion} returns this
 */
proto.engineinterface.PB_Quaternion.prototype.setY = function(value) {
  return jspb.Message.setProto3FloatField(this, 2, value);
};


/**
 * optional double z = 3;
 * @return {number}
 */
proto.engineinterface.PB_Quaternion.prototype.getZ = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Quaternion} returns this
 */
proto.engineinterface.PB_Quaternion.prototype.setZ = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional double w = 4;
 * @return {number}
 */
proto.engineinterface.PB_Quaternion.prototype.getW = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 4, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Quaternion} returns this
 */
proto.engineinterface.PB_Quaternion.prototype.setW = function(value) {
  return jspb.Message.setProto3FloatField(this, 4, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Transform.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Transform.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Transform} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Transform.toObject = function(includeInstance, msg) {
  var f, obj = {
    position: (f = msg.getPosition()) && proto.engineinterface.PB_Vector3.toObject(includeInstance, f),
    rotation: (f = msg.getRotation()) && proto.engineinterface.PB_Quaternion.toObject(includeInstance, f),
    scale: (f = msg.getScale()) && proto.engineinterface.PB_Vector3.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Transform}
 */
proto.engineinterface.PB_Transform.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Transform;
  return proto.engineinterface.PB_Transform.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Transform} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Transform}
 */
proto.engineinterface.PB_Transform.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = new proto.engineinterface.PB_Vector3;
      reader.readMessage(value,proto.engineinterface.PB_Vector3.deserializeBinaryFromReader);
      msg.setPosition(value);
      break;
    case 2:
      var value = new proto.engineinterface.PB_Quaternion;
      reader.readMessage(value,proto.engineinterface.PB_Quaternion.deserializeBinaryFromReader);
      msg.setRotation(value);
      break;
    case 3:
      var value = new proto.engineinterface.PB_Vector3;
      reader.readMessage(value,proto.engineinterface.PB_Vector3.deserializeBinaryFromReader);
      msg.setScale(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Transform.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Transform.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Transform} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Transform.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getPosition();
  if (f != null) {
    writer.writeMessage(
      1,
      f,
      proto.engineinterface.PB_Vector3.serializeBinaryToWriter
    );
  }
  f = message.getRotation();
  if (f != null) {
    writer.writeMessage(
      2,
      f,
      proto.engineinterface.PB_Quaternion.serializeBinaryToWriter
    );
  }
  f = message.getScale();
  if (f != null) {
    writer.writeMessage(
      3,
      f,
      proto.engineinterface.PB_Vector3.serializeBinaryToWriter
    );
  }
};


/**
 * optional PB_Vector3 position = 1;
 * @return {?proto.engineinterface.PB_Vector3}
 */
proto.engineinterface.PB_Transform.prototype.getPosition = function() {
  return /** @type{?proto.engineinterface.PB_Vector3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Vector3, 1));
};


/**
 * @param {?proto.engineinterface.PB_Vector3|undefined} value
 * @return {!proto.engineinterface.PB_Transform} returns this
*/
proto.engineinterface.PB_Transform.prototype.setPosition = function(value) {
  return jspb.Message.setWrapperField(this, 1, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Transform} returns this
 */
proto.engineinterface.PB_Transform.prototype.clearPosition = function() {
  return this.setPosition(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Transform.prototype.hasPosition = function() {
  return jspb.Message.getField(this, 1) != null;
};


/**
 * optional PB_Quaternion rotation = 2;
 * @return {?proto.engineinterface.PB_Quaternion}
 */
proto.engineinterface.PB_Transform.prototype.getRotation = function() {
  return /** @type{?proto.engineinterface.PB_Quaternion} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Quaternion, 2));
};


/**
 * @param {?proto.engineinterface.PB_Quaternion|undefined} value
 * @return {!proto.engineinterface.PB_Transform} returns this
*/
proto.engineinterface.PB_Transform.prototype.setRotation = function(value) {
  return jspb.Message.setWrapperField(this, 2, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Transform} returns this
 */
proto.engineinterface.PB_Transform.prototype.clearRotation = function() {
  return this.setRotation(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Transform.prototype.hasRotation = function() {
  return jspb.Message.getField(this, 2) != null;
};


/**
 * optional PB_Vector3 scale = 3;
 * @return {?proto.engineinterface.PB_Vector3}
 */
proto.engineinterface.PB_Transform.prototype.getScale = function() {
  return /** @type{?proto.engineinterface.PB_Vector3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Vector3, 3));
};


/**
 * @param {?proto.engineinterface.PB_Vector3|undefined} value
 * @return {!proto.engineinterface.PB_Transform} returns this
*/
proto.engineinterface.PB_Transform.prototype.setScale = function(value) {
  return jspb.Message.setWrapperField(this, 3, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Transform} returns this
 */
proto.engineinterface.PB_Transform.prototype.clearScale = function() {
  return this.setScale(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Transform.prototype.hasScale = function() {
  return jspb.Message.getField(this, 3) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UpdateEntityComponent.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UpdateEntityComponent.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UpdateEntityComponent} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UpdateEntityComponent.toObject = function(includeInstance, msg) {
  var f, obj = {
    entityid: jspb.Message.getFieldWithDefault(msg, 1, ""),
    classid: jspb.Message.getFieldWithDefault(msg, 2, 0),
    name: jspb.Message.getFieldWithDefault(msg, 3, ""),
    data: jspb.Message.getFieldWithDefault(msg, 4, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UpdateEntityComponent}
 */
proto.engineinterface.PB_UpdateEntityComponent.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UpdateEntityComponent;
  return proto.engineinterface.PB_UpdateEntityComponent.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UpdateEntityComponent} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UpdateEntityComponent}
 */
proto.engineinterface.PB_UpdateEntityComponent.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setEntityid(value);
      break;
    case 2:
      var value = /** @type {number} */ (reader.readInt32());
      msg.setClassid(value);
      break;
    case 3:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 4:
      var value = /** @type {string} */ (reader.readString());
      msg.setData(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UpdateEntityComponent.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UpdateEntityComponent.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UpdateEntityComponent} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UpdateEntityComponent.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getEntityid();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getClassid();
  if (f !== 0) {
    writer.writeInt32(
      2,
      f
    );
  }
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      3,
      f
    );
  }
  f = message.getData();
  if (f.length > 0) {
    writer.writeString(
      4,
      f
    );
  }
};


/**
 * optional string entityId = 1;
 * @return {string}
 */
proto.engineinterface.PB_UpdateEntityComponent.prototype.getEntityid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UpdateEntityComponent} returns this
 */
proto.engineinterface.PB_UpdateEntityComponent.prototype.setEntityid = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional int32 classId = 2;
 * @return {number}
 */
proto.engineinterface.PB_UpdateEntityComponent.prototype.getClassid = function() {
  return /** @type {number} */ (jspb.Message.getFieldWithDefault(this, 2, 0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UpdateEntityComponent} returns this
 */
proto.engineinterface.PB_UpdateEntityComponent.prototype.setClassid = function(value) {
  return jspb.Message.setProto3IntField(this, 2, value);
};


/**
 * optional string name = 3;
 * @return {string}
 */
proto.engineinterface.PB_UpdateEntityComponent.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 3, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UpdateEntityComponent} returns this
 */
proto.engineinterface.PB_UpdateEntityComponent.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 3, value);
};


/**
 * optional string data = 4;
 * @return {string}
 */
proto.engineinterface.PB_UpdateEntityComponent.prototype.getData = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 4, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UpdateEntityComponent} returns this
 */
proto.engineinterface.PB_UpdateEntityComponent.prototype.setData = function(value) {
  return jspb.Message.setProto3StringField(this, 4, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_ComponentCreated.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_ComponentCreated.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_ComponentCreated} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ComponentCreated.toObject = function(includeInstance, msg) {
  var f, obj = {
    id: jspb.Message.getFieldWithDefault(msg, 1, ""),
    classid: jspb.Message.getFieldWithDefault(msg, 2, 0),
    name: jspb.Message.getFieldWithDefault(msg, 3, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_ComponentCreated}
 */
proto.engineinterface.PB_ComponentCreated.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_ComponentCreated;
  return proto.engineinterface.PB_ComponentCreated.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_ComponentCreated} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_ComponentCreated}
 */
proto.engineinterface.PB_ComponentCreated.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setId(value);
      break;
    case 2:
      var value = /** @type {number} */ (reader.readInt32());
      msg.setClassid(value);
      break;
    case 3:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_ComponentCreated.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_ComponentCreated.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_ComponentCreated} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ComponentCreated.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getId();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getClassid();
  if (f !== 0) {
    writer.writeInt32(
      2,
      f
    );
  }
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      3,
      f
    );
  }
};


/**
 * optional string id = 1;
 * @return {string}
 */
proto.engineinterface.PB_ComponentCreated.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_ComponentCreated} returns this
 */
proto.engineinterface.PB_ComponentCreated.prototype.setId = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional int32 classid = 2;
 * @return {number}
 */
proto.engineinterface.PB_ComponentCreated.prototype.getClassid = function() {
  return /** @type {number} */ (jspb.Message.getFieldWithDefault(this, 2, 0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_ComponentCreated} returns this
 */
proto.engineinterface.PB_ComponentCreated.prototype.setClassid = function(value) {
  return jspb.Message.setProto3IntField(this, 2, value);
};


/**
 * optional string name = 3;
 * @return {string}
 */
proto.engineinterface.PB_ComponentCreated.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 3, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_ComponentCreated} returns this
 */
proto.engineinterface.PB_ComponentCreated.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 3, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_AttachEntityComponent.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_AttachEntityComponent.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_AttachEntityComponent} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_AttachEntityComponent.toObject = function(includeInstance, msg) {
  var f, obj = {
    entityid: jspb.Message.getFieldWithDefault(msg, 1, ""),
    name: jspb.Message.getFieldWithDefault(msg, 2, ""),
    id: jspb.Message.getFieldWithDefault(msg, 3, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_AttachEntityComponent}
 */
proto.engineinterface.PB_AttachEntityComponent.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_AttachEntityComponent;
  return proto.engineinterface.PB_AttachEntityComponent.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_AttachEntityComponent} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_AttachEntityComponent}
 */
proto.engineinterface.PB_AttachEntityComponent.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setEntityid(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 3:
      var value = /** @type {string} */ (reader.readString());
      msg.setId(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_AttachEntityComponent.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_AttachEntityComponent.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_AttachEntityComponent} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_AttachEntityComponent.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getEntityid();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
  f = message.getId();
  if (f.length > 0) {
    writer.writeString(
      3,
      f
    );
  }
};


/**
 * optional string entityId = 1;
 * @return {string}
 */
proto.engineinterface.PB_AttachEntityComponent.prototype.getEntityid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_AttachEntityComponent} returns this
 */
proto.engineinterface.PB_AttachEntityComponent.prototype.setEntityid = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string name = 2;
 * @return {string}
 */
proto.engineinterface.PB_AttachEntityComponent.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_AttachEntityComponent} returns this
 */
proto.engineinterface.PB_AttachEntityComponent.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};


/**
 * optional string id = 3;
 * @return {string}
 */
proto.engineinterface.PB_AttachEntityComponent.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 3, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_AttachEntityComponent} returns this
 */
proto.engineinterface.PB_AttachEntityComponent.prototype.setId = function(value) {
  return jspb.Message.setProto3StringField(this, 3, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_ComponentDisposed.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_ComponentDisposed.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_ComponentDisposed} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ComponentDisposed.toObject = function(includeInstance, msg) {
  var f, obj = {
    id: jspb.Message.getFieldWithDefault(msg, 1, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_ComponentDisposed}
 */
proto.engineinterface.PB_ComponentDisposed.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_ComponentDisposed;
  return proto.engineinterface.PB_ComponentDisposed.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_ComponentDisposed} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_ComponentDisposed}
 */
proto.engineinterface.PB_ComponentDisposed.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setId(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_ComponentDisposed.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_ComponentDisposed.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_ComponentDisposed} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ComponentDisposed.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getId();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
};


/**
 * optional string id = 1;
 * @return {string}
 */
proto.engineinterface.PB_ComponentDisposed.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_ComponentDisposed} returns this
 */
proto.engineinterface.PB_ComponentDisposed.prototype.setId = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_ComponentUpdated.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_ComponentUpdated.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_ComponentUpdated} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ComponentUpdated.toObject = function(includeInstance, msg) {
  var f, obj = {
    id: jspb.Message.getFieldWithDefault(msg, 1, ""),
    json: jspb.Message.getFieldWithDefault(msg, 2, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_ComponentUpdated}
 */
proto.engineinterface.PB_ComponentUpdated.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_ComponentUpdated;
  return proto.engineinterface.PB_ComponentUpdated.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_ComponentUpdated} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_ComponentUpdated}
 */
proto.engineinterface.PB_ComponentUpdated.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setId(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setJson(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_ComponentUpdated.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_ComponentUpdated.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_ComponentUpdated} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ComponentUpdated.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getId();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getJson();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
};


/**
 * optional string id = 1;
 * @return {string}
 */
proto.engineinterface.PB_ComponentUpdated.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_ComponentUpdated} returns this
 */
proto.engineinterface.PB_ComponentUpdated.prototype.setId = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string json = 2;
 * @return {string}
 */
proto.engineinterface.PB_ComponentUpdated.prototype.getJson = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_ComponentUpdated} returns this
 */
proto.engineinterface.PB_ComponentUpdated.prototype.setJson = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Ray.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Ray.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Ray} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Ray.toObject = function(includeInstance, msg) {
  var f, obj = {
    origin: (f = msg.getOrigin()) && proto.engineinterface.PB_Vector3.toObject(includeInstance, f),
    direction: (f = msg.getDirection()) && proto.engineinterface.PB_Vector3.toObject(includeInstance, f),
    distance: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Ray}
 */
proto.engineinterface.PB_Ray.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Ray;
  return proto.engineinterface.PB_Ray.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Ray} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Ray}
 */
proto.engineinterface.PB_Ray.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = new proto.engineinterface.PB_Vector3;
      reader.readMessage(value,proto.engineinterface.PB_Vector3.deserializeBinaryFromReader);
      msg.setOrigin(value);
      break;
    case 2:
      var value = new proto.engineinterface.PB_Vector3;
      reader.readMessage(value,proto.engineinterface.PB_Vector3.deserializeBinaryFromReader);
      msg.setDirection(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setDistance(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Ray.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Ray.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Ray} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Ray.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getOrigin();
  if (f != null) {
    writer.writeMessage(
      1,
      f,
      proto.engineinterface.PB_Vector3.serializeBinaryToWriter
    );
  }
  f = message.getDirection();
  if (f != null) {
    writer.writeMessage(
      2,
      f,
      proto.engineinterface.PB_Vector3.serializeBinaryToWriter
    );
  }
  f = message.getDistance();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
};


/**
 * optional PB_Vector3 origin = 1;
 * @return {?proto.engineinterface.PB_Vector3}
 */
proto.engineinterface.PB_Ray.prototype.getOrigin = function() {
  return /** @type{?proto.engineinterface.PB_Vector3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Vector3, 1));
};


/**
 * @param {?proto.engineinterface.PB_Vector3|undefined} value
 * @return {!proto.engineinterface.PB_Ray} returns this
*/
proto.engineinterface.PB_Ray.prototype.setOrigin = function(value) {
  return jspb.Message.setWrapperField(this, 1, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Ray} returns this
 */
proto.engineinterface.PB_Ray.prototype.clearOrigin = function() {
  return this.setOrigin(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Ray.prototype.hasOrigin = function() {
  return jspb.Message.getField(this, 1) != null;
};


/**
 * optional PB_Vector3 direction = 2;
 * @return {?proto.engineinterface.PB_Vector3}
 */
proto.engineinterface.PB_Ray.prototype.getDirection = function() {
  return /** @type{?proto.engineinterface.PB_Vector3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Vector3, 2));
};


/**
 * @param {?proto.engineinterface.PB_Vector3|undefined} value
 * @return {!proto.engineinterface.PB_Ray} returns this
*/
proto.engineinterface.PB_Ray.prototype.setDirection = function(value) {
  return jspb.Message.setWrapperField(this, 2, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Ray} returns this
 */
proto.engineinterface.PB_Ray.prototype.clearDirection = function() {
  return this.setDirection(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Ray.prototype.hasDirection = function() {
  return jspb.Message.getField(this, 2) != null;
};


/**
 * optional float distance = 3;
 * @return {number}
 */
proto.engineinterface.PB_Ray.prototype.getDistance = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Ray} returns this
 */
proto.engineinterface.PB_Ray.prototype.setDistance = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_RayQuery.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_RayQuery.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_RayQuery} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_RayQuery.toObject = function(includeInstance, msg) {
  var f, obj = {
    queryid: jspb.Message.getFieldWithDefault(msg, 1, ""),
    querytype: jspb.Message.getFieldWithDefault(msg, 2, ""),
    ray: (f = msg.getRay()) && proto.engineinterface.PB_Ray.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_RayQuery}
 */
proto.engineinterface.PB_RayQuery.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_RayQuery;
  return proto.engineinterface.PB_RayQuery.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_RayQuery} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_RayQuery}
 */
proto.engineinterface.PB_RayQuery.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setQueryid(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setQuerytype(value);
      break;
    case 3:
      var value = new proto.engineinterface.PB_Ray;
      reader.readMessage(value,proto.engineinterface.PB_Ray.deserializeBinaryFromReader);
      msg.setRay(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_RayQuery.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_RayQuery.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_RayQuery} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_RayQuery.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getQueryid();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getQuerytype();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
  f = message.getRay();
  if (f != null) {
    writer.writeMessage(
      3,
      f,
      proto.engineinterface.PB_Ray.serializeBinaryToWriter
    );
  }
};


/**
 * optional string queryId = 1;
 * @return {string}
 */
proto.engineinterface.PB_RayQuery.prototype.getQueryid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_RayQuery} returns this
 */
proto.engineinterface.PB_RayQuery.prototype.setQueryid = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string queryType = 2;
 * @return {string}
 */
proto.engineinterface.PB_RayQuery.prototype.getQuerytype = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_RayQuery} returns this
 */
proto.engineinterface.PB_RayQuery.prototype.setQuerytype = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};


/**
 * optional PB_Ray ray = 3;
 * @return {?proto.engineinterface.PB_Ray}
 */
proto.engineinterface.PB_RayQuery.prototype.getRay = function() {
  return /** @type{?proto.engineinterface.PB_Ray} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Ray, 3));
};


/**
 * @param {?proto.engineinterface.PB_Ray|undefined} value
 * @return {!proto.engineinterface.PB_RayQuery} returns this
*/
proto.engineinterface.PB_RayQuery.prototype.setRay = function(value) {
  return jspb.Message.setWrapperField(this, 3, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_RayQuery} returns this
 */
proto.engineinterface.PB_RayQuery.prototype.clearRay = function() {
  return this.setRay(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_RayQuery.prototype.hasRay = function() {
  return jspb.Message.getField(this, 3) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Query.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Query.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Query} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Query.toObject = function(includeInstance, msg) {
  var f, obj = {
    queryid: jspb.Message.getFieldWithDefault(msg, 1, ""),
    payload: jspb.Message.getFieldWithDefault(msg, 2, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Query}
 */
proto.engineinterface.PB_Query.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Query;
  return proto.engineinterface.PB_Query.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Query} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Query}
 */
proto.engineinterface.PB_Query.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setQueryid(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setPayload(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Query.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Query.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Query} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Query.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getQueryid();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getPayload();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
};


/**
 * optional string queryId = 1;
 * @return {string}
 */
proto.engineinterface.PB_Query.prototype.getQueryid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_Query} returns this
 */
proto.engineinterface.PB_Query.prototype.setQueryid = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string payload = 2;
 * @return {string}
 */
proto.engineinterface.PB_Query.prototype.getPayload = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_Query} returns this
 */
proto.engineinterface.PB_Query.prototype.setPayload = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};



/**
 * Oneof group definitions for this message. Each group defines the field
 * numbers belonging to that group. When of these fields' value is set, all
 * other fields in the group are cleared. During deserialization, if multiple
 * fields are encountered for a group, only the last value seen will be kept.
 * @private {!Array<!Array<number>>}
 * @const
 */
proto.engineinterface.PB_SendSceneMessage.oneofGroups_ = [[3,4,5,6,7,8,9,10,11,12,13,14,15]];

/**
 * @enum {number}
 */
proto.engineinterface.PB_SendSceneMessage.PayloadCase = {
  PAYLOAD_NOT_SET: 0,
  CREATEENTITY: 3,
  REMOVEENTITY: 4,
  SETENTITYPARENT: 5,
  UPDATEENTITYCOMPONENT: 6,
  ATTACHENTITYCOMPONENT: 7,
  COMPONENTCREATED: 8,
  COMPONENTDISPOSED: 9,
  COMPONENTREMOVED: 10,
  COMPONENTUPDATED: 11,
  QUERY: 12,
  SCENESTARTED: 13,
  OPENEXTERNALURL: 14,
  OPENNFTDIALOG: 15
};

/**
 * @return {proto.engineinterface.PB_SendSceneMessage.PayloadCase}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getPayloadCase = function() {
  return /** @type {proto.engineinterface.PB_SendSceneMessage.PayloadCase} */(jspb.Message.computeOneofCase(this, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0]));
};



if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_SendSceneMessage.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_SendSceneMessage} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_SendSceneMessage.toObject = function(includeInstance, msg) {
  var f, obj = {
    sceneid: jspb.Message.getFieldWithDefault(msg, 1, ""),
    tag: jspb.Message.getFieldWithDefault(msg, 2, ""),
    createentity: (f = msg.getCreateentity()) && proto.engineinterface.PB_CreateEntity.toObject(includeInstance, f),
    removeentity: (f = msg.getRemoveentity()) && proto.engineinterface.PB_RemoveEntity.toObject(includeInstance, f),
    setentityparent: (f = msg.getSetentityparent()) && proto.engineinterface.PB_SetEntityParent.toObject(includeInstance, f),
    updateentitycomponent: (f = msg.getUpdateentitycomponent()) && proto.engineinterface.PB_UpdateEntityComponent.toObject(includeInstance, f),
    attachentitycomponent: (f = msg.getAttachentitycomponent()) && proto.engineinterface.PB_AttachEntityComponent.toObject(includeInstance, f),
    componentcreated: (f = msg.getComponentcreated()) && proto.engineinterface.PB_ComponentCreated.toObject(includeInstance, f),
    componentdisposed: (f = msg.getComponentdisposed()) && proto.engineinterface.PB_ComponentDisposed.toObject(includeInstance, f),
    componentremoved: (f = msg.getComponentremoved()) && proto.engineinterface.PB_ComponentRemoved.toObject(includeInstance, f),
    componentupdated: (f = msg.getComponentupdated()) && proto.engineinterface.PB_ComponentUpdated.toObject(includeInstance, f),
    query: (f = msg.getQuery()) && proto.engineinterface.PB_Query.toObject(includeInstance, f),
    scenestarted: (f = msg.getScenestarted()) && google_protobuf_empty_pb.Empty.toObject(includeInstance, f),
    openexternalurl: (f = msg.getOpenexternalurl()) && proto.engineinterface.PB_OpenExternalUrl.toObject(includeInstance, f),
    opennftdialog: (f = msg.getOpennftdialog()) && proto.engineinterface.PB_OpenNFTDialog.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_SendSceneMessage}
 */
proto.engineinterface.PB_SendSceneMessage.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_SendSceneMessage;
  return proto.engineinterface.PB_SendSceneMessage.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_SendSceneMessage} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_SendSceneMessage}
 */
proto.engineinterface.PB_SendSceneMessage.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setSceneid(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setTag(value);
      break;
    case 3:
      var value = new proto.engineinterface.PB_CreateEntity;
      reader.readMessage(value,proto.engineinterface.PB_CreateEntity.deserializeBinaryFromReader);
      msg.setCreateentity(value);
      break;
    case 4:
      var value = new proto.engineinterface.PB_RemoveEntity;
      reader.readMessage(value,proto.engineinterface.PB_RemoveEntity.deserializeBinaryFromReader);
      msg.setRemoveentity(value);
      break;
    case 5:
      var value = new proto.engineinterface.PB_SetEntityParent;
      reader.readMessage(value,proto.engineinterface.PB_SetEntityParent.deserializeBinaryFromReader);
      msg.setSetentityparent(value);
      break;
    case 6:
      var value = new proto.engineinterface.PB_UpdateEntityComponent;
      reader.readMessage(value,proto.engineinterface.PB_UpdateEntityComponent.deserializeBinaryFromReader);
      msg.setUpdateentitycomponent(value);
      break;
    case 7:
      var value = new proto.engineinterface.PB_AttachEntityComponent;
      reader.readMessage(value,proto.engineinterface.PB_AttachEntityComponent.deserializeBinaryFromReader);
      msg.setAttachentitycomponent(value);
      break;
    case 8:
      var value = new proto.engineinterface.PB_ComponentCreated;
      reader.readMessage(value,proto.engineinterface.PB_ComponentCreated.deserializeBinaryFromReader);
      msg.setComponentcreated(value);
      break;
    case 9:
      var value = new proto.engineinterface.PB_ComponentDisposed;
      reader.readMessage(value,proto.engineinterface.PB_ComponentDisposed.deserializeBinaryFromReader);
      msg.setComponentdisposed(value);
      break;
    case 10:
      var value = new proto.engineinterface.PB_ComponentRemoved;
      reader.readMessage(value,proto.engineinterface.PB_ComponentRemoved.deserializeBinaryFromReader);
      msg.setComponentremoved(value);
      break;
    case 11:
      var value = new proto.engineinterface.PB_ComponentUpdated;
      reader.readMessage(value,proto.engineinterface.PB_ComponentUpdated.deserializeBinaryFromReader);
      msg.setComponentupdated(value);
      break;
    case 12:
      var value = new proto.engineinterface.PB_Query;
      reader.readMessage(value,proto.engineinterface.PB_Query.deserializeBinaryFromReader);
      msg.setQuery(value);
      break;
    case 13:
      var value = new google_protobuf_empty_pb.Empty;
      reader.readMessage(value,google_protobuf_empty_pb.Empty.deserializeBinaryFromReader);
      msg.setScenestarted(value);
      break;
    case 14:
      var value = new proto.engineinterface.PB_OpenExternalUrl;
      reader.readMessage(value,proto.engineinterface.PB_OpenExternalUrl.deserializeBinaryFromReader);
      msg.setOpenexternalurl(value);
      break;
    case 15:
      var value = new proto.engineinterface.PB_OpenNFTDialog;
      reader.readMessage(value,proto.engineinterface.PB_OpenNFTDialog.deserializeBinaryFromReader);
      msg.setOpennftdialog(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_SendSceneMessage.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_SendSceneMessage} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_SendSceneMessage.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getSceneid();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getTag();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
  f = message.getCreateentity();
  if (f != null) {
    writer.writeMessage(
      3,
      f,
      proto.engineinterface.PB_CreateEntity.serializeBinaryToWriter
    );
  }
  f = message.getRemoveentity();
  if (f != null) {
    writer.writeMessage(
      4,
      f,
      proto.engineinterface.PB_RemoveEntity.serializeBinaryToWriter
    );
  }
  f = message.getSetentityparent();
  if (f != null) {
    writer.writeMessage(
      5,
      f,
      proto.engineinterface.PB_SetEntityParent.serializeBinaryToWriter
    );
  }
  f = message.getUpdateentitycomponent();
  if (f != null) {
    writer.writeMessage(
      6,
      f,
      proto.engineinterface.PB_UpdateEntityComponent.serializeBinaryToWriter
    );
  }
  f = message.getAttachentitycomponent();
  if (f != null) {
    writer.writeMessage(
      7,
      f,
      proto.engineinterface.PB_AttachEntityComponent.serializeBinaryToWriter
    );
  }
  f = message.getComponentcreated();
  if (f != null) {
    writer.writeMessage(
      8,
      f,
      proto.engineinterface.PB_ComponentCreated.serializeBinaryToWriter
    );
  }
  f = message.getComponentdisposed();
  if (f != null) {
    writer.writeMessage(
      9,
      f,
      proto.engineinterface.PB_ComponentDisposed.serializeBinaryToWriter
    );
  }
  f = message.getComponentremoved();
  if (f != null) {
    writer.writeMessage(
      10,
      f,
      proto.engineinterface.PB_ComponentRemoved.serializeBinaryToWriter
    );
  }
  f = message.getComponentupdated();
  if (f != null) {
    writer.writeMessage(
      11,
      f,
      proto.engineinterface.PB_ComponentUpdated.serializeBinaryToWriter
    );
  }
  f = message.getQuery();
  if (f != null) {
    writer.writeMessage(
      12,
      f,
      proto.engineinterface.PB_Query.serializeBinaryToWriter
    );
  }
  f = message.getScenestarted();
  if (f != null) {
    writer.writeMessage(
      13,
      f,
      google_protobuf_empty_pb.Empty.serializeBinaryToWriter
    );
  }
  f = message.getOpenexternalurl();
  if (f != null) {
    writer.writeMessage(
      14,
      f,
      proto.engineinterface.PB_OpenExternalUrl.serializeBinaryToWriter
    );
  }
  f = message.getOpennftdialog();
  if (f != null) {
    writer.writeMessage(
      15,
      f,
      proto.engineinterface.PB_OpenNFTDialog.serializeBinaryToWriter
    );
  }
};


/**
 * optional string sceneId = 1;
 * @return {string}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getSceneid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.setSceneid = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string tag = 2;
 * @return {string}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getTag = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.setTag = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};


/**
 * optional PB_CreateEntity createEntity = 3;
 * @return {?proto.engineinterface.PB_CreateEntity}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getCreateentity = function() {
  return /** @type{?proto.engineinterface.PB_CreateEntity} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_CreateEntity, 3));
};


/**
 * @param {?proto.engineinterface.PB_CreateEntity|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setCreateentity = function(value) {
  return jspb.Message.setOneofWrapperField(this, 3, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearCreateentity = function() {
  return this.setCreateentity(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasCreateentity = function() {
  return jspb.Message.getField(this, 3) != null;
};


/**
 * optional PB_RemoveEntity removeEntity = 4;
 * @return {?proto.engineinterface.PB_RemoveEntity}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getRemoveentity = function() {
  return /** @type{?proto.engineinterface.PB_RemoveEntity} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_RemoveEntity, 4));
};


/**
 * @param {?proto.engineinterface.PB_RemoveEntity|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setRemoveentity = function(value) {
  return jspb.Message.setOneofWrapperField(this, 4, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearRemoveentity = function() {
  return this.setRemoveentity(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasRemoveentity = function() {
  return jspb.Message.getField(this, 4) != null;
};


/**
 * optional PB_SetEntityParent setEntityParent = 5;
 * @return {?proto.engineinterface.PB_SetEntityParent}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getSetentityparent = function() {
  return /** @type{?proto.engineinterface.PB_SetEntityParent} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_SetEntityParent, 5));
};


/**
 * @param {?proto.engineinterface.PB_SetEntityParent|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setSetentityparent = function(value) {
  return jspb.Message.setOneofWrapperField(this, 5, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearSetentityparent = function() {
  return this.setSetentityparent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasSetentityparent = function() {
  return jspb.Message.getField(this, 5) != null;
};


/**
 * optional PB_UpdateEntityComponent updateEntityComponent = 6;
 * @return {?proto.engineinterface.PB_UpdateEntityComponent}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getUpdateentitycomponent = function() {
  return /** @type{?proto.engineinterface.PB_UpdateEntityComponent} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UpdateEntityComponent, 6));
};


/**
 * @param {?proto.engineinterface.PB_UpdateEntityComponent|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setUpdateentitycomponent = function(value) {
  return jspb.Message.setOneofWrapperField(this, 6, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearUpdateentitycomponent = function() {
  return this.setUpdateentitycomponent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasUpdateentitycomponent = function() {
  return jspb.Message.getField(this, 6) != null;
};


/**
 * optional PB_AttachEntityComponent attachEntityComponent = 7;
 * @return {?proto.engineinterface.PB_AttachEntityComponent}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getAttachentitycomponent = function() {
  return /** @type{?proto.engineinterface.PB_AttachEntityComponent} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_AttachEntityComponent, 7));
};


/**
 * @param {?proto.engineinterface.PB_AttachEntityComponent|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setAttachentitycomponent = function(value) {
  return jspb.Message.setOneofWrapperField(this, 7, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearAttachentitycomponent = function() {
  return this.setAttachentitycomponent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasAttachentitycomponent = function() {
  return jspb.Message.getField(this, 7) != null;
};


/**
 * optional PB_ComponentCreated componentCreated = 8;
 * @return {?proto.engineinterface.PB_ComponentCreated}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getComponentcreated = function() {
  return /** @type{?proto.engineinterface.PB_ComponentCreated} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_ComponentCreated, 8));
};


/**
 * @param {?proto.engineinterface.PB_ComponentCreated|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setComponentcreated = function(value) {
  return jspb.Message.setOneofWrapperField(this, 8, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearComponentcreated = function() {
  return this.setComponentcreated(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasComponentcreated = function() {
  return jspb.Message.getField(this, 8) != null;
};


/**
 * optional PB_ComponentDisposed componentDisposed = 9;
 * @return {?proto.engineinterface.PB_ComponentDisposed}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getComponentdisposed = function() {
  return /** @type{?proto.engineinterface.PB_ComponentDisposed} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_ComponentDisposed, 9));
};


/**
 * @param {?proto.engineinterface.PB_ComponentDisposed|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setComponentdisposed = function(value) {
  return jspb.Message.setOneofWrapperField(this, 9, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearComponentdisposed = function() {
  return this.setComponentdisposed(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasComponentdisposed = function() {
  return jspb.Message.getField(this, 9) != null;
};


/**
 * optional PB_ComponentRemoved componentRemoved = 10;
 * @return {?proto.engineinterface.PB_ComponentRemoved}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getComponentremoved = function() {
  return /** @type{?proto.engineinterface.PB_ComponentRemoved} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_ComponentRemoved, 10));
};


/**
 * @param {?proto.engineinterface.PB_ComponentRemoved|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setComponentremoved = function(value) {
  return jspb.Message.setOneofWrapperField(this, 10, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearComponentremoved = function() {
  return this.setComponentremoved(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasComponentremoved = function() {
  return jspb.Message.getField(this, 10) != null;
};


/**
 * optional PB_ComponentUpdated componentUpdated = 11;
 * @return {?proto.engineinterface.PB_ComponentUpdated}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getComponentupdated = function() {
  return /** @type{?proto.engineinterface.PB_ComponentUpdated} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_ComponentUpdated, 11));
};


/**
 * @param {?proto.engineinterface.PB_ComponentUpdated|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setComponentupdated = function(value) {
  return jspb.Message.setOneofWrapperField(this, 11, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearComponentupdated = function() {
  return this.setComponentupdated(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasComponentupdated = function() {
  return jspb.Message.getField(this, 11) != null;
};


/**
 * optional PB_Query query = 12;
 * @return {?proto.engineinterface.PB_Query}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getQuery = function() {
  return /** @type{?proto.engineinterface.PB_Query} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Query, 12));
};


/**
 * @param {?proto.engineinterface.PB_Query|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setQuery = function(value) {
  return jspb.Message.setOneofWrapperField(this, 12, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearQuery = function() {
  return this.setQuery(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasQuery = function() {
  return jspb.Message.getField(this, 12) != null;
};


/**
 * optional google.protobuf.Empty sceneStarted = 13;
 * @return {?proto.google.protobuf.Empty}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getScenestarted = function() {
  return /** @type{?proto.google.protobuf.Empty} */ (
    jspb.Message.getWrapperField(this, google_protobuf_empty_pb.Empty, 13));
};


/**
 * @param {?proto.google.protobuf.Empty|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setScenestarted = function(value) {
  return jspb.Message.setOneofWrapperField(this, 13, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearScenestarted = function() {
  return this.setScenestarted(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasScenestarted = function() {
  return jspb.Message.getField(this, 13) != null;
};


/**
 * optional PB_OpenExternalUrl openExternalUrl = 14;
 * @return {?proto.engineinterface.PB_OpenExternalUrl}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getOpenexternalurl = function() {
  return /** @type{?proto.engineinterface.PB_OpenExternalUrl} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_OpenExternalUrl, 14));
};


/**
 * @param {?proto.engineinterface.PB_OpenExternalUrl|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setOpenexternalurl = function(value) {
  return jspb.Message.setOneofWrapperField(this, 14, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearOpenexternalurl = function() {
  return this.setOpenexternalurl(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasOpenexternalurl = function() {
  return jspb.Message.getField(this, 14) != null;
};


/**
 * optional PB_OpenNFTDialog openNFTDialog = 15;
 * @return {?proto.engineinterface.PB_OpenNFTDialog}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.getOpennftdialog = function() {
  return /** @type{?proto.engineinterface.PB_OpenNFTDialog} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_OpenNFTDialog, 15));
};


/**
 * @param {?proto.engineinterface.PB_OpenNFTDialog|undefined} value
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
*/
proto.engineinterface.PB_SendSceneMessage.prototype.setOpennftdialog = function(value) {
  return jspb.Message.setOneofWrapperField(this, 15, proto.engineinterface.PB_SendSceneMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_SendSceneMessage} returns this
 */
proto.engineinterface.PB_SendSceneMessage.prototype.clearOpennftdialog = function() {
  return this.setOpennftdialog(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_SendSceneMessage.prototype.hasOpennftdialog = function() {
  return jspb.Message.getField(this, 15) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_SetPosition.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_SetPosition.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_SetPosition} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_SetPosition.toObject = function(includeInstance, msg) {
  var f, obj = {
    x: jspb.Message.getFloatingPointFieldWithDefault(msg, 1, 0.0),
    y: jspb.Message.getFloatingPointFieldWithDefault(msg, 2, 0.0),
    z: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_SetPosition}
 */
proto.engineinterface.PB_SetPosition.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_SetPosition;
  return proto.engineinterface.PB_SetPosition.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_SetPosition} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_SetPosition}
 */
proto.engineinterface.PB_SetPosition.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setX(value);
      break;
    case 2:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setY(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setZ(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_SetPosition.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_SetPosition.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_SetPosition} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_SetPosition.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getX();
  if (f !== 0.0) {
    writer.writeFloat(
      1,
      f
    );
  }
  f = message.getY();
  if (f !== 0.0) {
    writer.writeFloat(
      2,
      f
    );
  }
  f = message.getZ();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
};


/**
 * optional float x = 1;
 * @return {number}
 */
proto.engineinterface.PB_SetPosition.prototype.getX = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 1, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_SetPosition} returns this
 */
proto.engineinterface.PB_SetPosition.prototype.setX = function(value) {
  return jspb.Message.setProto3FloatField(this, 1, value);
};


/**
 * optional float y = 2;
 * @return {number}
 */
proto.engineinterface.PB_SetPosition.prototype.getY = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 2, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_SetPosition} returns this
 */
proto.engineinterface.PB_SetPosition.prototype.setY = function(value) {
  return jspb.Message.setProto3FloatField(this, 2, value);
};


/**
 * optional float z = 3;
 * @return {number}
 */
proto.engineinterface.PB_SetPosition.prototype.getZ = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_SetPosition} returns this
 */
proto.engineinterface.PB_SetPosition.prototype.setZ = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_ContentMapping.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_ContentMapping.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_ContentMapping} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ContentMapping.toObject = function(includeInstance, msg) {
  var f, obj = {
    file: jspb.Message.getFieldWithDefault(msg, 1, ""),
    hash: jspb.Message.getFieldWithDefault(msg, 2, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_ContentMapping}
 */
proto.engineinterface.PB_ContentMapping.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_ContentMapping;
  return proto.engineinterface.PB_ContentMapping.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_ContentMapping} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_ContentMapping}
 */
proto.engineinterface.PB_ContentMapping.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setFile(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setHash(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_ContentMapping.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_ContentMapping.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_ContentMapping} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ContentMapping.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getFile();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getHash();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
};


/**
 * optional string file = 1;
 * @return {string}
 */
proto.engineinterface.PB_ContentMapping.prototype.getFile = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_ContentMapping} returns this
 */
proto.engineinterface.PB_ContentMapping.prototype.setFile = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string hash = 2;
 * @return {string}
 */
proto.engineinterface.PB_ContentMapping.prototype.getHash = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_ContentMapping} returns this
 */
proto.engineinterface.PB_ContentMapping.prototype.setHash = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Position.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Position.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Position} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Position.toObject = function(includeInstance, msg) {
  var f, obj = {
    x: jspb.Message.getFloatingPointFieldWithDefault(msg, 1, 0.0),
    y: jspb.Message.getFloatingPointFieldWithDefault(msg, 2, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Position}
 */
proto.engineinterface.PB_Position.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Position;
  return proto.engineinterface.PB_Position.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Position} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Position}
 */
proto.engineinterface.PB_Position.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setX(value);
      break;
    case 2:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setY(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Position.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Position.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Position} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Position.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getX();
  if (f !== 0.0) {
    writer.writeFloat(
      1,
      f
    );
  }
  f = message.getY();
  if (f !== 0.0) {
    writer.writeFloat(
      2,
      f
    );
  }
};


/**
 * optional float x = 1;
 * @return {number}
 */
proto.engineinterface.PB_Position.prototype.getX = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 1, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Position} returns this
 */
proto.engineinterface.PB_Position.prototype.setX = function(value) {
  return jspb.Message.setProto3FloatField(this, 1, value);
};


/**
 * optional float y = 2;
 * @return {number}
 */
proto.engineinterface.PB_Position.prototype.getY = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 2, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Position} returns this
 */
proto.engineinterface.PB_Position.prototype.setY = function(value) {
  return jspb.Message.setProto3FloatField(this, 2, value);
};



/**
 * List of repeated fields within this message type.
 * @private {!Array<number>}
 * @const
 */
proto.engineinterface.PB_LoadParcelScenes.repeatedFields_ = [3,4];



if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_LoadParcelScenes.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_LoadParcelScenes} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_LoadParcelScenes.toObject = function(includeInstance, msg) {
  var f, obj = {
    id: jspb.Message.getFieldWithDefault(msg, 1, ""),
    baseposition: (f = msg.getBaseposition()) && proto.engineinterface.PB_Position.toObject(includeInstance, f),
    parcelsList: jspb.Message.toObjectList(msg.getParcelsList(),
    proto.engineinterface.PB_Position.toObject, includeInstance),
    contentsList: jspb.Message.toObjectList(msg.getContentsList(),
    proto.engineinterface.PB_ContentMapping.toObject, includeInstance),
    baseurl: jspb.Message.getFieldWithDefault(msg, 5, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_LoadParcelScenes}
 */
proto.engineinterface.PB_LoadParcelScenes.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_LoadParcelScenes;
  return proto.engineinterface.PB_LoadParcelScenes.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_LoadParcelScenes} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_LoadParcelScenes}
 */
proto.engineinterface.PB_LoadParcelScenes.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setId(value);
      break;
    case 2:
      var value = new proto.engineinterface.PB_Position;
      reader.readMessage(value,proto.engineinterface.PB_Position.deserializeBinaryFromReader);
      msg.setBaseposition(value);
      break;
    case 3:
      var value = new proto.engineinterface.PB_Position;
      reader.readMessage(value,proto.engineinterface.PB_Position.deserializeBinaryFromReader);
      msg.addParcels(value);
      break;
    case 4:
      var value = new proto.engineinterface.PB_ContentMapping;
      reader.readMessage(value,proto.engineinterface.PB_ContentMapping.deserializeBinaryFromReader);
      msg.addContents(value);
      break;
    case 5:
      var value = /** @type {string} */ (reader.readString());
      msg.setBaseurl(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_LoadParcelScenes.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_LoadParcelScenes} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_LoadParcelScenes.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getId();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getBaseposition();
  if (f != null) {
    writer.writeMessage(
      2,
      f,
      proto.engineinterface.PB_Position.serializeBinaryToWriter
    );
  }
  f = message.getParcelsList();
  if (f.length > 0) {
    writer.writeRepeatedMessage(
      3,
      f,
      proto.engineinterface.PB_Position.serializeBinaryToWriter
    );
  }
  f = message.getContentsList();
  if (f.length > 0) {
    writer.writeRepeatedMessage(
      4,
      f,
      proto.engineinterface.PB_ContentMapping.serializeBinaryToWriter
    );
  }
  f = message.getBaseurl();
  if (f.length > 0) {
    writer.writeString(
      5,
      f
    );
  }
};


/**
 * optional string id = 1;
 * @return {string}
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_LoadParcelScenes} returns this
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.setId = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional PB_Position basePosition = 2;
 * @return {?proto.engineinterface.PB_Position}
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.getBaseposition = function() {
  return /** @type{?proto.engineinterface.PB_Position} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Position, 2));
};


/**
 * @param {?proto.engineinterface.PB_Position|undefined} value
 * @return {!proto.engineinterface.PB_LoadParcelScenes} returns this
*/
proto.engineinterface.PB_LoadParcelScenes.prototype.setBaseposition = function(value) {
  return jspb.Message.setWrapperField(this, 2, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_LoadParcelScenes} returns this
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.clearBaseposition = function() {
  return this.setBaseposition(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.hasBaseposition = function() {
  return jspb.Message.getField(this, 2) != null;
};


/**
 * repeated PB_Position parcels = 3;
 * @return {!Array<!proto.engineinterface.PB_Position>}
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.getParcelsList = function() {
  return /** @type{!Array<!proto.engineinterface.PB_Position>} */ (
    jspb.Message.getRepeatedWrapperField(this, proto.engineinterface.PB_Position, 3));
};


/**
 * @param {!Array<!proto.engineinterface.PB_Position>} value
 * @return {!proto.engineinterface.PB_LoadParcelScenes} returns this
*/
proto.engineinterface.PB_LoadParcelScenes.prototype.setParcelsList = function(value) {
  return jspb.Message.setRepeatedWrapperField(this, 3, value);
};


/**
 * @param {!proto.engineinterface.PB_Position=} opt_value
 * @param {number=} opt_index
 * @return {!proto.engineinterface.PB_Position}
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.addParcels = function(opt_value, opt_index) {
  return jspb.Message.addToRepeatedWrapperField(this, 3, opt_value, proto.engineinterface.PB_Position, opt_index);
};


/**
 * Clears the list making it empty but non-null.
 * @return {!proto.engineinterface.PB_LoadParcelScenes} returns this
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.clearParcelsList = function() {
  return this.setParcelsList([]);
};


/**
 * repeated PB_ContentMapping contents = 4;
 * @return {!Array<!proto.engineinterface.PB_ContentMapping>}
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.getContentsList = function() {
  return /** @type{!Array<!proto.engineinterface.PB_ContentMapping>} */ (
    jspb.Message.getRepeatedWrapperField(this, proto.engineinterface.PB_ContentMapping, 4));
};


/**
 * @param {!Array<!proto.engineinterface.PB_ContentMapping>} value
 * @return {!proto.engineinterface.PB_LoadParcelScenes} returns this
*/
proto.engineinterface.PB_LoadParcelScenes.prototype.setContentsList = function(value) {
  return jspb.Message.setRepeatedWrapperField(this, 4, value);
};


/**
 * @param {!proto.engineinterface.PB_ContentMapping=} opt_value
 * @param {number=} opt_index
 * @return {!proto.engineinterface.PB_ContentMapping}
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.addContents = function(opt_value, opt_index) {
  return jspb.Message.addToRepeatedWrapperField(this, 4, opt_value, proto.engineinterface.PB_ContentMapping, opt_index);
};


/**
 * Clears the list making it empty but non-null.
 * @return {!proto.engineinterface.PB_LoadParcelScenes} returns this
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.clearContentsList = function() {
  return this.setContentsList([]);
};


/**
 * optional string baseUrl = 5;
 * @return {string}
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.getBaseurl = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 5, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_LoadParcelScenes} returns this
 */
proto.engineinterface.PB_LoadParcelScenes.prototype.setBaseurl = function(value) {
  return jspb.Message.setProto3StringField(this, 5, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_CreateUIScene.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_CreateUIScene.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_CreateUIScene} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_CreateUIScene.toObject = function(includeInstance, msg) {
  var f, obj = {
    id: jspb.Message.getFieldWithDefault(msg, 1, ""),
    baseurl: jspb.Message.getFieldWithDefault(msg, 2, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_CreateUIScene}
 */
proto.engineinterface.PB_CreateUIScene.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_CreateUIScene;
  return proto.engineinterface.PB_CreateUIScene.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_CreateUIScene} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_CreateUIScene}
 */
proto.engineinterface.PB_CreateUIScene.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setId(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setBaseurl(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_CreateUIScene.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_CreateUIScene.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_CreateUIScene} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_CreateUIScene.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getId();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getBaseurl();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
};


/**
 * optional string id = 1;
 * @return {string}
 */
proto.engineinterface.PB_CreateUIScene.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_CreateUIScene} returns this
 */
proto.engineinterface.PB_CreateUIScene.prototype.setId = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string baseUrl = 2;
 * @return {string}
 */
proto.engineinterface.PB_CreateUIScene.prototype.getBaseurl = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_CreateUIScene} returns this
 */
proto.engineinterface.PB_CreateUIScene.prototype.setBaseurl = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UnloadScene.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UnloadScene.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UnloadScene} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UnloadScene.toObject = function(includeInstance, msg) {
  var f, obj = {
    sceneid: jspb.Message.getFieldWithDefault(msg, 1, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UnloadScene}
 */
proto.engineinterface.PB_UnloadScene.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UnloadScene;
  return proto.engineinterface.PB_UnloadScene.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UnloadScene} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UnloadScene}
 */
proto.engineinterface.PB_UnloadScene.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setSceneid(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UnloadScene.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UnloadScene.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UnloadScene} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UnloadScene.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getSceneid();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
};


/**
 * optional string sceneId = 1;
 * @return {string}
 */
proto.engineinterface.PB_UnloadScene.prototype.getSceneid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UnloadScene} returns this
 */
proto.engineinterface.PB_UnloadScene.prototype.setSceneid = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};



/**
 * Oneof group definitions for this message. Each group defines the field
 * numbers belonging to that group. When of these fields' value is set, all
 * other fields in the group are cleared. During deserialization, if multiple
 * fields are encountered for a group, only the last value seen will be kept.
 * @private {!Array<!Array<number>>}
 * @const
 */
proto.engineinterface.PB_DclMessage.oneofGroups_ = [[1,2,3,4,5,6,7,8,9]];

/**
 * @enum {number}
 */
proto.engineinterface.PB_DclMessage.MessageCase = {
  MESSAGE_NOT_SET: 0,
  SETDEBUG: 1,
  SETSCENEDEBUGPANEL: 2,
  SETENGINEDEBUGPANEL: 3,
  SENDSCENEMESSAGE: 4,
  LOADPARCELSCENES: 5,
  UNLOADSCENE: 6,
  SETPOSITION: 7,
  RESET: 8,
  CREATEUISCENE: 9
};

/**
 * @return {proto.engineinterface.PB_DclMessage.MessageCase}
 */
proto.engineinterface.PB_DclMessage.prototype.getMessageCase = function() {
  return /** @type {proto.engineinterface.PB_DclMessage.MessageCase} */(jspb.Message.computeOneofCase(this, proto.engineinterface.PB_DclMessage.oneofGroups_[0]));
};



if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_DclMessage.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_DclMessage.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_DclMessage} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_DclMessage.toObject = function(includeInstance, msg) {
  var f, obj = {
    setdebug: (f = msg.getSetdebug()) && google_protobuf_empty_pb.Empty.toObject(includeInstance, f),
    setscenedebugpanel: (f = msg.getSetscenedebugpanel()) && google_protobuf_empty_pb.Empty.toObject(includeInstance, f),
    setenginedebugpanel: (f = msg.getSetenginedebugpanel()) && google_protobuf_empty_pb.Empty.toObject(includeInstance, f),
    sendscenemessage: (f = msg.getSendscenemessage()) && proto.engineinterface.PB_SendSceneMessage.toObject(includeInstance, f),
    loadparcelscenes: (f = msg.getLoadparcelscenes()) && proto.engineinterface.PB_LoadParcelScenes.toObject(includeInstance, f),
    unloadscene: (f = msg.getUnloadscene()) && proto.engineinterface.PB_UnloadScene.toObject(includeInstance, f),
    setposition: (f = msg.getSetposition()) && proto.engineinterface.PB_SetPosition.toObject(includeInstance, f),
    reset: (f = msg.getReset()) && google_protobuf_empty_pb.Empty.toObject(includeInstance, f),
    createuiscene: (f = msg.getCreateuiscene()) && proto.engineinterface.PB_CreateUIScene.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_DclMessage}
 */
proto.engineinterface.PB_DclMessage.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_DclMessage;
  return proto.engineinterface.PB_DclMessage.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_DclMessage} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_DclMessage}
 */
proto.engineinterface.PB_DclMessage.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = new google_protobuf_empty_pb.Empty;
      reader.readMessage(value,google_protobuf_empty_pb.Empty.deserializeBinaryFromReader);
      msg.setSetdebug(value);
      break;
    case 2:
      var value = new google_protobuf_empty_pb.Empty;
      reader.readMessage(value,google_protobuf_empty_pb.Empty.deserializeBinaryFromReader);
      msg.setSetscenedebugpanel(value);
      break;
    case 3:
      var value = new google_protobuf_empty_pb.Empty;
      reader.readMessage(value,google_protobuf_empty_pb.Empty.deserializeBinaryFromReader);
      msg.setSetenginedebugpanel(value);
      break;
    case 4:
      var value = new proto.engineinterface.PB_SendSceneMessage;
      reader.readMessage(value,proto.engineinterface.PB_SendSceneMessage.deserializeBinaryFromReader);
      msg.setSendscenemessage(value);
      break;
    case 5:
      var value = new proto.engineinterface.PB_LoadParcelScenes;
      reader.readMessage(value,proto.engineinterface.PB_LoadParcelScenes.deserializeBinaryFromReader);
      msg.setLoadparcelscenes(value);
      break;
    case 6:
      var value = new proto.engineinterface.PB_UnloadScene;
      reader.readMessage(value,proto.engineinterface.PB_UnloadScene.deserializeBinaryFromReader);
      msg.setUnloadscene(value);
      break;
    case 7:
      var value = new proto.engineinterface.PB_SetPosition;
      reader.readMessage(value,proto.engineinterface.PB_SetPosition.deserializeBinaryFromReader);
      msg.setSetposition(value);
      break;
    case 8:
      var value = new google_protobuf_empty_pb.Empty;
      reader.readMessage(value,google_protobuf_empty_pb.Empty.deserializeBinaryFromReader);
      msg.setReset(value);
      break;
    case 9:
      var value = new proto.engineinterface.PB_CreateUIScene;
      reader.readMessage(value,proto.engineinterface.PB_CreateUIScene.deserializeBinaryFromReader);
      msg.setCreateuiscene(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_DclMessage.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_DclMessage.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_DclMessage} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_DclMessage.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getSetdebug();
  if (f != null) {
    writer.writeMessage(
      1,
      f,
      google_protobuf_empty_pb.Empty.serializeBinaryToWriter
    );
  }
  f = message.getSetscenedebugpanel();
  if (f != null) {
    writer.writeMessage(
      2,
      f,
      google_protobuf_empty_pb.Empty.serializeBinaryToWriter
    );
  }
  f = message.getSetenginedebugpanel();
  if (f != null) {
    writer.writeMessage(
      3,
      f,
      google_protobuf_empty_pb.Empty.serializeBinaryToWriter
    );
  }
  f = message.getSendscenemessage();
  if (f != null) {
    writer.writeMessage(
      4,
      f,
      proto.engineinterface.PB_SendSceneMessage.serializeBinaryToWriter
    );
  }
  f = message.getLoadparcelscenes();
  if (f != null) {
    writer.writeMessage(
      5,
      f,
      proto.engineinterface.PB_LoadParcelScenes.serializeBinaryToWriter
    );
  }
  f = message.getUnloadscene();
  if (f != null) {
    writer.writeMessage(
      6,
      f,
      proto.engineinterface.PB_UnloadScene.serializeBinaryToWriter
    );
  }
  f = message.getSetposition();
  if (f != null) {
    writer.writeMessage(
      7,
      f,
      proto.engineinterface.PB_SetPosition.serializeBinaryToWriter
    );
  }
  f = message.getReset();
  if (f != null) {
    writer.writeMessage(
      8,
      f,
      google_protobuf_empty_pb.Empty.serializeBinaryToWriter
    );
  }
  f = message.getCreateuiscene();
  if (f != null) {
    writer.writeMessage(
      9,
      f,
      proto.engineinterface.PB_CreateUIScene.serializeBinaryToWriter
    );
  }
};


/**
 * optional google.protobuf.Empty setDebug = 1;
 * @return {?proto.google.protobuf.Empty}
 */
proto.engineinterface.PB_DclMessage.prototype.getSetdebug = function() {
  return /** @type{?proto.google.protobuf.Empty} */ (
    jspb.Message.getWrapperField(this, google_protobuf_empty_pb.Empty, 1));
};


/**
 * @param {?proto.google.protobuf.Empty|undefined} value
 * @return {!proto.engineinterface.PB_DclMessage} returns this
*/
proto.engineinterface.PB_DclMessage.prototype.setSetdebug = function(value) {
  return jspb.Message.setOneofWrapperField(this, 1, proto.engineinterface.PB_DclMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_DclMessage} returns this
 */
proto.engineinterface.PB_DclMessage.prototype.clearSetdebug = function() {
  return this.setSetdebug(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_DclMessage.prototype.hasSetdebug = function() {
  return jspb.Message.getField(this, 1) != null;
};


/**
 * optional google.protobuf.Empty setSceneDebugPanel = 2;
 * @return {?proto.google.protobuf.Empty}
 */
proto.engineinterface.PB_DclMessage.prototype.getSetscenedebugpanel = function() {
  return /** @type{?proto.google.protobuf.Empty} */ (
    jspb.Message.getWrapperField(this, google_protobuf_empty_pb.Empty, 2));
};


/**
 * @param {?proto.google.protobuf.Empty|undefined} value
 * @return {!proto.engineinterface.PB_DclMessage} returns this
*/
proto.engineinterface.PB_DclMessage.prototype.setSetscenedebugpanel = function(value) {
  return jspb.Message.setOneofWrapperField(this, 2, proto.engineinterface.PB_DclMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_DclMessage} returns this
 */
proto.engineinterface.PB_DclMessage.prototype.clearSetscenedebugpanel = function() {
  return this.setSetscenedebugpanel(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_DclMessage.prototype.hasSetscenedebugpanel = function() {
  return jspb.Message.getField(this, 2) != null;
};


/**
 * optional google.protobuf.Empty setEngineDebugPanel = 3;
 * @return {?proto.google.protobuf.Empty}
 */
proto.engineinterface.PB_DclMessage.prototype.getSetenginedebugpanel = function() {
  return /** @type{?proto.google.protobuf.Empty} */ (
    jspb.Message.getWrapperField(this, google_protobuf_empty_pb.Empty, 3));
};


/**
 * @param {?proto.google.protobuf.Empty|undefined} value
 * @return {!proto.engineinterface.PB_DclMessage} returns this
*/
proto.engineinterface.PB_DclMessage.prototype.setSetenginedebugpanel = function(value) {
  return jspb.Message.setOneofWrapperField(this, 3, proto.engineinterface.PB_DclMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_DclMessage} returns this
 */
proto.engineinterface.PB_DclMessage.prototype.clearSetenginedebugpanel = function() {
  return this.setSetenginedebugpanel(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_DclMessage.prototype.hasSetenginedebugpanel = function() {
  return jspb.Message.getField(this, 3) != null;
};


/**
 * optional PB_SendSceneMessage sendSceneMessage = 4;
 * @return {?proto.engineinterface.PB_SendSceneMessage}
 */
proto.engineinterface.PB_DclMessage.prototype.getSendscenemessage = function() {
  return /** @type{?proto.engineinterface.PB_SendSceneMessage} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_SendSceneMessage, 4));
};


/**
 * @param {?proto.engineinterface.PB_SendSceneMessage|undefined} value
 * @return {!proto.engineinterface.PB_DclMessage} returns this
*/
proto.engineinterface.PB_DclMessage.prototype.setSendscenemessage = function(value) {
  return jspb.Message.setOneofWrapperField(this, 4, proto.engineinterface.PB_DclMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_DclMessage} returns this
 */
proto.engineinterface.PB_DclMessage.prototype.clearSendscenemessage = function() {
  return this.setSendscenemessage(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_DclMessage.prototype.hasSendscenemessage = function() {
  return jspb.Message.getField(this, 4) != null;
};


/**
 * optional PB_LoadParcelScenes loadParcelScenes = 5;
 * @return {?proto.engineinterface.PB_LoadParcelScenes}
 */
proto.engineinterface.PB_DclMessage.prototype.getLoadparcelscenes = function() {
  return /** @type{?proto.engineinterface.PB_LoadParcelScenes} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_LoadParcelScenes, 5));
};


/**
 * @param {?proto.engineinterface.PB_LoadParcelScenes|undefined} value
 * @return {!proto.engineinterface.PB_DclMessage} returns this
*/
proto.engineinterface.PB_DclMessage.prototype.setLoadparcelscenes = function(value) {
  return jspb.Message.setOneofWrapperField(this, 5, proto.engineinterface.PB_DclMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_DclMessage} returns this
 */
proto.engineinterface.PB_DclMessage.prototype.clearLoadparcelscenes = function() {
  return this.setLoadparcelscenes(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_DclMessage.prototype.hasLoadparcelscenes = function() {
  return jspb.Message.getField(this, 5) != null;
};


/**
 * optional PB_UnloadScene unloadScene = 6;
 * @return {?proto.engineinterface.PB_UnloadScene}
 */
proto.engineinterface.PB_DclMessage.prototype.getUnloadscene = function() {
  return /** @type{?proto.engineinterface.PB_UnloadScene} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UnloadScene, 6));
};


/**
 * @param {?proto.engineinterface.PB_UnloadScene|undefined} value
 * @return {!proto.engineinterface.PB_DclMessage} returns this
*/
proto.engineinterface.PB_DclMessage.prototype.setUnloadscene = function(value) {
  return jspb.Message.setOneofWrapperField(this, 6, proto.engineinterface.PB_DclMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_DclMessage} returns this
 */
proto.engineinterface.PB_DclMessage.prototype.clearUnloadscene = function() {
  return this.setUnloadscene(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_DclMessage.prototype.hasUnloadscene = function() {
  return jspb.Message.getField(this, 6) != null;
};


/**
 * optional PB_SetPosition setPosition = 7;
 * @return {?proto.engineinterface.PB_SetPosition}
 */
proto.engineinterface.PB_DclMessage.prototype.getSetposition = function() {
  return /** @type{?proto.engineinterface.PB_SetPosition} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_SetPosition, 7));
};


/**
 * @param {?proto.engineinterface.PB_SetPosition|undefined} value
 * @return {!proto.engineinterface.PB_DclMessage} returns this
*/
proto.engineinterface.PB_DclMessage.prototype.setSetposition = function(value) {
  return jspb.Message.setOneofWrapperField(this, 7, proto.engineinterface.PB_DclMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_DclMessage} returns this
 */
proto.engineinterface.PB_DclMessage.prototype.clearSetposition = function() {
  return this.setSetposition(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_DclMessage.prototype.hasSetposition = function() {
  return jspb.Message.getField(this, 7) != null;
};


/**
 * optional google.protobuf.Empty reset = 8;
 * @return {?proto.google.protobuf.Empty}
 */
proto.engineinterface.PB_DclMessage.prototype.getReset = function() {
  return /** @type{?proto.google.protobuf.Empty} */ (
    jspb.Message.getWrapperField(this, google_protobuf_empty_pb.Empty, 8));
};


/**
 * @param {?proto.google.protobuf.Empty|undefined} value
 * @return {!proto.engineinterface.PB_DclMessage} returns this
*/
proto.engineinterface.PB_DclMessage.prototype.setReset = function(value) {
  return jspb.Message.setOneofWrapperField(this, 8, proto.engineinterface.PB_DclMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_DclMessage} returns this
 */
proto.engineinterface.PB_DclMessage.prototype.clearReset = function() {
  return this.setReset(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_DclMessage.prototype.hasReset = function() {
  return jspb.Message.getField(this, 8) != null;
};


/**
 * optional PB_CreateUIScene createUIScene = 9;
 * @return {?proto.engineinterface.PB_CreateUIScene}
 */
proto.engineinterface.PB_DclMessage.prototype.getCreateuiscene = function() {
  return /** @type{?proto.engineinterface.PB_CreateUIScene} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_CreateUIScene, 9));
};


/**
 * @param {?proto.engineinterface.PB_CreateUIScene|undefined} value
 * @return {!proto.engineinterface.PB_DclMessage} returns this
*/
proto.engineinterface.PB_DclMessage.prototype.setCreateuiscene = function(value) {
  return jspb.Message.setOneofWrapperField(this, 9, proto.engineinterface.PB_DclMessage.oneofGroups_[0], value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_DclMessage} returns this
 */
proto.engineinterface.PB_DclMessage.prototype.clearCreateuiscene = function() {
  return this.setCreateuiscene(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_DclMessage.prototype.hasCreateuiscene = function() {
  return jspb.Message.getField(this, 9) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_AnimationState.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_AnimationState.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_AnimationState} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_AnimationState.toObject = function(includeInstance, msg) {
  var f, obj = {
    clip: jspb.Message.getFieldWithDefault(msg, 1, ""),
    looping: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    weight: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    playing: jspb.Message.getBooleanFieldWithDefault(msg, 4, false),
    shouldreset: jspb.Message.getBooleanFieldWithDefault(msg, 5, false),
    speed: jspb.Message.getFloatingPointFieldWithDefault(msg, 6, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_AnimationState}
 */
proto.engineinterface.PB_AnimationState.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_AnimationState;
  return proto.engineinterface.PB_AnimationState.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_AnimationState} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_AnimationState}
 */
proto.engineinterface.PB_AnimationState.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setClip(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setLooping(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setWeight(value);
      break;
    case 4:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setPlaying(value);
      break;
    case 5:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setShouldreset(value);
      break;
    case 6:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSpeed(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_AnimationState.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_AnimationState.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_AnimationState} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_AnimationState.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getClip();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getLooping();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getWeight();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getPlaying();
  if (f) {
    writer.writeBool(
      4,
      f
    );
  }
  f = message.getShouldreset();
  if (f) {
    writer.writeBool(
      5,
      f
    );
  }
  f = message.getSpeed();
  if (f !== 0.0) {
    writer.writeFloat(
      6,
      f
    );
  }
};


/**
 * optional string clip = 1;
 * @return {string}
 */
proto.engineinterface.PB_AnimationState.prototype.getClip = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_AnimationState} returns this
 */
proto.engineinterface.PB_AnimationState.prototype.setClip = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool looping = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_AnimationState.prototype.getLooping = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_AnimationState} returns this
 */
proto.engineinterface.PB_AnimationState.prototype.setLooping = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float weight = 3;
 * @return {number}
 */
proto.engineinterface.PB_AnimationState.prototype.getWeight = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_AnimationState} returns this
 */
proto.engineinterface.PB_AnimationState.prototype.setWeight = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional bool playing = 4;
 * @return {boolean}
 */
proto.engineinterface.PB_AnimationState.prototype.getPlaying = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 4, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_AnimationState} returns this
 */
proto.engineinterface.PB_AnimationState.prototype.setPlaying = function(value) {
  return jspb.Message.setProto3BooleanField(this, 4, value);
};


/**
 * optional bool shouldReset = 5;
 * @return {boolean}
 */
proto.engineinterface.PB_AnimationState.prototype.getShouldreset = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 5, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_AnimationState} returns this
 */
proto.engineinterface.PB_AnimationState.prototype.setShouldreset = function(value) {
  return jspb.Message.setProto3BooleanField(this, 5, value);
};


/**
 * optional float speed = 6;
 * @return {number}
 */
proto.engineinterface.PB_AnimationState.prototype.getSpeed = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 6, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_AnimationState} returns this
 */
proto.engineinterface.PB_AnimationState.prototype.setSpeed = function(value) {
  return jspb.Message.setProto3FloatField(this, 6, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Animator.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Animator.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Animator} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Animator.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Animator}
 */
proto.engineinterface.PB_Animator.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Animator;
  return proto.engineinterface.PB_Animator.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Animator} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Animator}
 */
proto.engineinterface.PB_Animator.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Animator.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Animator.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Animator} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Animator.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_Animator.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_Animator} returns this
 */
proto.engineinterface.PB_Animator.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_Animator.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_Animator} returns this
 */
proto.engineinterface.PB_Animator.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_AudioClip.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_AudioClip.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_AudioClip} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_AudioClip.toObject = function(includeInstance, msg) {
  var f, obj = {
    url: jspb.Message.getFieldWithDefault(msg, 1, ""),
    loop: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    volume: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_AudioClip}
 */
proto.engineinterface.PB_AudioClip.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_AudioClip;
  return proto.engineinterface.PB_AudioClip.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_AudioClip} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_AudioClip}
 */
proto.engineinterface.PB_AudioClip.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setUrl(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setLoop(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setVolume(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_AudioClip.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_AudioClip.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_AudioClip} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_AudioClip.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getUrl();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getLoop();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getVolume();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
};


/**
 * optional string url = 1;
 * @return {string}
 */
proto.engineinterface.PB_AudioClip.prototype.getUrl = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_AudioClip} returns this
 */
proto.engineinterface.PB_AudioClip.prototype.setUrl = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool loop = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_AudioClip.prototype.getLoop = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_AudioClip} returns this
 */
proto.engineinterface.PB_AudioClip.prototype.setLoop = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float volume = 3;
 * @return {number}
 */
proto.engineinterface.PB_AudioClip.prototype.getVolume = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_AudioClip} returns this
 */
proto.engineinterface.PB_AudioClip.prototype.setVolume = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_AudioSource.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_AudioSource.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_AudioSource} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_AudioSource.toObject = function(includeInstance, msg) {
  var f, obj = {
    audioclip: (f = msg.getAudioclip()) && proto.engineinterface.PB_AudioClip.toObject(includeInstance, f),
    audioclipid: jspb.Message.getFieldWithDefault(msg, 2, ""),
    loop: jspb.Message.getBooleanFieldWithDefault(msg, 3, false),
    volume: jspb.Message.getFloatingPointFieldWithDefault(msg, 4, 0.0),
    playing: jspb.Message.getBooleanFieldWithDefault(msg, 5, false),
    pitch: jspb.Message.getFloatingPointFieldWithDefault(msg, 6, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_AudioSource}
 */
proto.engineinterface.PB_AudioSource.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_AudioSource;
  return proto.engineinterface.PB_AudioSource.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_AudioSource} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_AudioSource}
 */
proto.engineinterface.PB_AudioSource.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = new proto.engineinterface.PB_AudioClip;
      reader.readMessage(value,proto.engineinterface.PB_AudioClip.deserializeBinaryFromReader);
      msg.setAudioclip(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setAudioclipid(value);
      break;
    case 3:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setLoop(value);
      break;
    case 4:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setVolume(value);
      break;
    case 5:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setPlaying(value);
      break;
    case 6:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPitch(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_AudioSource.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_AudioSource.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_AudioSource} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_AudioSource.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getAudioclip();
  if (f != null) {
    writer.writeMessage(
      1,
      f,
      proto.engineinterface.PB_AudioClip.serializeBinaryToWriter
    );
  }
  f = message.getAudioclipid();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
  f = message.getLoop();
  if (f) {
    writer.writeBool(
      3,
      f
    );
  }
  f = message.getVolume();
  if (f !== 0.0) {
    writer.writeFloat(
      4,
      f
    );
  }
  f = message.getPlaying();
  if (f) {
    writer.writeBool(
      5,
      f
    );
  }
  f = message.getPitch();
  if (f !== 0.0) {
    writer.writeFloat(
      6,
      f
    );
  }
};


/**
 * optional PB_AudioClip audioClip = 1;
 * @return {?proto.engineinterface.PB_AudioClip}
 */
proto.engineinterface.PB_AudioSource.prototype.getAudioclip = function() {
  return /** @type{?proto.engineinterface.PB_AudioClip} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_AudioClip, 1));
};


/**
 * @param {?proto.engineinterface.PB_AudioClip|undefined} value
 * @return {!proto.engineinterface.PB_AudioSource} returns this
*/
proto.engineinterface.PB_AudioSource.prototype.setAudioclip = function(value) {
  return jspb.Message.setWrapperField(this, 1, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_AudioSource} returns this
 */
proto.engineinterface.PB_AudioSource.prototype.clearAudioclip = function() {
  return this.setAudioclip(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_AudioSource.prototype.hasAudioclip = function() {
  return jspb.Message.getField(this, 1) != null;
};


/**
 * optional string audioClipId = 2;
 * @return {string}
 */
proto.engineinterface.PB_AudioSource.prototype.getAudioclipid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_AudioSource} returns this
 */
proto.engineinterface.PB_AudioSource.prototype.setAudioclipid = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};


/**
 * optional bool loop = 3;
 * @return {boolean}
 */
proto.engineinterface.PB_AudioSource.prototype.getLoop = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 3, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_AudioSource} returns this
 */
proto.engineinterface.PB_AudioSource.prototype.setLoop = function(value) {
  return jspb.Message.setProto3BooleanField(this, 3, value);
};


/**
 * optional float volume = 4;
 * @return {number}
 */
proto.engineinterface.PB_AudioSource.prototype.getVolume = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 4, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_AudioSource} returns this
 */
proto.engineinterface.PB_AudioSource.prototype.setVolume = function(value) {
  return jspb.Message.setProto3FloatField(this, 4, value);
};


/**
 * optional bool playing = 5;
 * @return {boolean}
 */
proto.engineinterface.PB_AudioSource.prototype.getPlaying = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 5, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_AudioSource} returns this
 */
proto.engineinterface.PB_AudioSource.prototype.setPlaying = function(value) {
  return jspb.Message.setProto3BooleanField(this, 5, value);
};


/**
 * optional float pitch = 6;
 * @return {number}
 */
proto.engineinterface.PB_AudioSource.prototype.getPitch = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 6, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_AudioSource} returns this
 */
proto.engineinterface.PB_AudioSource.prototype.setPitch = function(value) {
  return jspb.Message.setProto3FloatField(this, 6, value);
};



/**
 * List of repeated fields within this message type.
 * @private {!Array<number>}
 * @const
 */
proto.engineinterface.PB_AvatarShape.repeatedFields_ = [5];



if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_AvatarShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_AvatarShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_AvatarShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_AvatarShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    id: jspb.Message.getFieldWithDefault(msg, 1, ""),
    baseurl: jspb.Message.getFieldWithDefault(msg, 2, ""),
    name: jspb.Message.getFieldWithDefault(msg, 3, ""),
    bodyshape: (f = msg.getBodyshape()) && proto.engineinterface.PB_Wearable.toObject(includeInstance, f),
    wearablesList: jspb.Message.toObjectList(msg.getWearablesList(),
    proto.engineinterface.PB_Wearable.toObject, includeInstance),
    skin: (f = msg.getSkin()) && proto.engineinterface.PB_Skin.toObject(includeInstance, f),
    hair: (f = msg.getHair()) && proto.engineinterface.PB_Hair.toObject(includeInstance, f),
    eyes: (f = msg.getEyes()) && proto.engineinterface.PB_Eyes.toObject(includeInstance, f),
    eyebrows: (f = msg.getEyebrows()) && proto.engineinterface.PB_Face.toObject(includeInstance, f),
    mouth: (f = msg.getMouth()) && proto.engineinterface.PB_Face.toObject(includeInstance, f),
    usedummymodel: jspb.Message.getBooleanFieldWithDefault(msg, 11, false),
    expressiontriggerid: jspb.Message.getFieldWithDefault(msg, 12, ""),
    expressiontriggertimestamp: jspb.Message.getFieldWithDefault(msg, 14, 0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_AvatarShape}
 */
proto.engineinterface.PB_AvatarShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_AvatarShape;
  return proto.engineinterface.PB_AvatarShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_AvatarShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_AvatarShape}
 */
proto.engineinterface.PB_AvatarShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setId(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setBaseurl(value);
      break;
    case 3:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 4:
      var value = new proto.engineinterface.PB_Wearable;
      reader.readMessage(value,proto.engineinterface.PB_Wearable.deserializeBinaryFromReader);
      msg.setBodyshape(value);
      break;
    case 5:
      var value = new proto.engineinterface.PB_Wearable;
      reader.readMessage(value,proto.engineinterface.PB_Wearable.deserializeBinaryFromReader);
      msg.addWearables(value);
      break;
    case 6:
      var value = new proto.engineinterface.PB_Skin;
      reader.readMessage(value,proto.engineinterface.PB_Skin.deserializeBinaryFromReader);
      msg.setSkin(value);
      break;
    case 7:
      var value = new proto.engineinterface.PB_Hair;
      reader.readMessage(value,proto.engineinterface.PB_Hair.deserializeBinaryFromReader);
      msg.setHair(value);
      break;
    case 8:
      var value = new proto.engineinterface.PB_Eyes;
      reader.readMessage(value,proto.engineinterface.PB_Eyes.deserializeBinaryFromReader);
      msg.setEyes(value);
      break;
    case 9:
      var value = new proto.engineinterface.PB_Face;
      reader.readMessage(value,proto.engineinterface.PB_Face.deserializeBinaryFromReader);
      msg.setEyebrows(value);
      break;
    case 10:
      var value = new proto.engineinterface.PB_Face;
      reader.readMessage(value,proto.engineinterface.PB_Face.deserializeBinaryFromReader);
      msg.setMouth(value);
      break;
    case 11:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setUsedummymodel(value);
      break;
    case 12:
      var value = /** @type {string} */ (reader.readString());
      msg.setExpressiontriggerid(value);
      break;
    case 14:
      var value = /** @type {number} */ (reader.readUint64());
      msg.setExpressiontriggertimestamp(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_AvatarShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_AvatarShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_AvatarShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_AvatarShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getId();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getBaseurl();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      3,
      f
    );
  }
  f = message.getBodyshape();
  if (f != null) {
    writer.writeMessage(
      4,
      f,
      proto.engineinterface.PB_Wearable.serializeBinaryToWriter
    );
  }
  f = message.getWearablesList();
  if (f.length > 0) {
    writer.writeRepeatedMessage(
      5,
      f,
      proto.engineinterface.PB_Wearable.serializeBinaryToWriter
    );
  }
  f = message.getSkin();
  if (f != null) {
    writer.writeMessage(
      6,
      f,
      proto.engineinterface.PB_Skin.serializeBinaryToWriter
    );
  }
  f = message.getHair();
  if (f != null) {
    writer.writeMessage(
      7,
      f,
      proto.engineinterface.PB_Hair.serializeBinaryToWriter
    );
  }
  f = message.getEyes();
  if (f != null) {
    writer.writeMessage(
      8,
      f,
      proto.engineinterface.PB_Eyes.serializeBinaryToWriter
    );
  }
  f = message.getEyebrows();
  if (f != null) {
    writer.writeMessage(
      9,
      f,
      proto.engineinterface.PB_Face.serializeBinaryToWriter
    );
  }
  f = message.getMouth();
  if (f != null) {
    writer.writeMessage(
      10,
      f,
      proto.engineinterface.PB_Face.serializeBinaryToWriter
    );
  }
  f = message.getUsedummymodel();
  if (f) {
    writer.writeBool(
      11,
      f
    );
  }
  f = message.getExpressiontriggerid();
  if (f.length > 0) {
    writer.writeString(
      12,
      f
    );
  }
  f = message.getExpressiontriggertimestamp();
  if (f !== 0) {
    writer.writeUint64(
      14,
      f
    );
  }
};


/**
 * optional string id = 1;
 * @return {string}
 */
proto.engineinterface.PB_AvatarShape.prototype.getId = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.setId = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string baseUrl = 2;
 * @return {string}
 */
proto.engineinterface.PB_AvatarShape.prototype.getBaseurl = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.setBaseurl = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};


/**
 * optional string name = 3;
 * @return {string}
 */
proto.engineinterface.PB_AvatarShape.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 3, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 3, value);
};


/**
 * optional PB_Wearable bodyShape = 4;
 * @return {?proto.engineinterface.PB_Wearable}
 */
proto.engineinterface.PB_AvatarShape.prototype.getBodyshape = function() {
  return /** @type{?proto.engineinterface.PB_Wearable} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Wearable, 4));
};


/**
 * @param {?proto.engineinterface.PB_Wearable|undefined} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
*/
proto.engineinterface.PB_AvatarShape.prototype.setBodyshape = function(value) {
  return jspb.Message.setWrapperField(this, 4, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.clearBodyshape = function() {
  return this.setBodyshape(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_AvatarShape.prototype.hasBodyshape = function() {
  return jspb.Message.getField(this, 4) != null;
};


/**
 * repeated PB_Wearable wearables = 5;
 * @return {!Array<!proto.engineinterface.PB_Wearable>}
 */
proto.engineinterface.PB_AvatarShape.prototype.getWearablesList = function() {
  return /** @type{!Array<!proto.engineinterface.PB_Wearable>} */ (
    jspb.Message.getRepeatedWrapperField(this, proto.engineinterface.PB_Wearable, 5));
};


/**
 * @param {!Array<!proto.engineinterface.PB_Wearable>} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
*/
proto.engineinterface.PB_AvatarShape.prototype.setWearablesList = function(value) {
  return jspb.Message.setRepeatedWrapperField(this, 5, value);
};


/**
 * @param {!proto.engineinterface.PB_Wearable=} opt_value
 * @param {number=} opt_index
 * @return {!proto.engineinterface.PB_Wearable}
 */
proto.engineinterface.PB_AvatarShape.prototype.addWearables = function(opt_value, opt_index) {
  return jspb.Message.addToRepeatedWrapperField(this, 5, opt_value, proto.engineinterface.PB_Wearable, opt_index);
};


/**
 * Clears the list making it empty but non-null.
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.clearWearablesList = function() {
  return this.setWearablesList([]);
};


/**
 * optional PB_Skin skin = 6;
 * @return {?proto.engineinterface.PB_Skin}
 */
proto.engineinterface.PB_AvatarShape.prototype.getSkin = function() {
  return /** @type{?proto.engineinterface.PB_Skin} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Skin, 6));
};


/**
 * @param {?proto.engineinterface.PB_Skin|undefined} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
*/
proto.engineinterface.PB_AvatarShape.prototype.setSkin = function(value) {
  return jspb.Message.setWrapperField(this, 6, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.clearSkin = function() {
  return this.setSkin(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_AvatarShape.prototype.hasSkin = function() {
  return jspb.Message.getField(this, 6) != null;
};


/**
 * optional PB_Hair hair = 7;
 * @return {?proto.engineinterface.PB_Hair}
 */
proto.engineinterface.PB_AvatarShape.prototype.getHair = function() {
  return /** @type{?proto.engineinterface.PB_Hair} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Hair, 7));
};


/**
 * @param {?proto.engineinterface.PB_Hair|undefined} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
*/
proto.engineinterface.PB_AvatarShape.prototype.setHair = function(value) {
  return jspb.Message.setWrapperField(this, 7, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.clearHair = function() {
  return this.setHair(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_AvatarShape.prototype.hasHair = function() {
  return jspb.Message.getField(this, 7) != null;
};


/**
 * optional PB_Eyes eyes = 8;
 * @return {?proto.engineinterface.PB_Eyes}
 */
proto.engineinterface.PB_AvatarShape.prototype.getEyes = function() {
  return /** @type{?proto.engineinterface.PB_Eyes} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Eyes, 8));
};


/**
 * @param {?proto.engineinterface.PB_Eyes|undefined} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
*/
proto.engineinterface.PB_AvatarShape.prototype.setEyes = function(value) {
  return jspb.Message.setWrapperField(this, 8, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.clearEyes = function() {
  return this.setEyes(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_AvatarShape.prototype.hasEyes = function() {
  return jspb.Message.getField(this, 8) != null;
};


/**
 * optional PB_Face eyebrows = 9;
 * @return {?proto.engineinterface.PB_Face}
 */
proto.engineinterface.PB_AvatarShape.prototype.getEyebrows = function() {
  return /** @type{?proto.engineinterface.PB_Face} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Face, 9));
};


/**
 * @param {?proto.engineinterface.PB_Face|undefined} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
*/
proto.engineinterface.PB_AvatarShape.prototype.setEyebrows = function(value) {
  return jspb.Message.setWrapperField(this, 9, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.clearEyebrows = function() {
  return this.setEyebrows(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_AvatarShape.prototype.hasEyebrows = function() {
  return jspb.Message.getField(this, 9) != null;
};


/**
 * optional PB_Face mouth = 10;
 * @return {?proto.engineinterface.PB_Face}
 */
proto.engineinterface.PB_AvatarShape.prototype.getMouth = function() {
  return /** @type{?proto.engineinterface.PB_Face} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Face, 10));
};


/**
 * @param {?proto.engineinterface.PB_Face|undefined} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
*/
proto.engineinterface.PB_AvatarShape.prototype.setMouth = function(value) {
  return jspb.Message.setWrapperField(this, 10, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.clearMouth = function() {
  return this.setMouth(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_AvatarShape.prototype.hasMouth = function() {
  return jspb.Message.getField(this, 10) != null;
};


/**
 * optional bool useDummyModel = 11;
 * @return {boolean}
 */
proto.engineinterface.PB_AvatarShape.prototype.getUsedummymodel = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 11, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.setUsedummymodel = function(value) {
  return jspb.Message.setProto3BooleanField(this, 11, value);
};


/**
 * optional string expressionTriggerId = 12;
 * @return {string}
 */
proto.engineinterface.PB_AvatarShape.prototype.getExpressiontriggerid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 12, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.setExpressiontriggerid = function(value) {
  return jspb.Message.setProto3StringField(this, 12, value);
};


/**
 * optional uint64 expressionTriggerTimestamp = 14;
 * @return {number}
 */
proto.engineinterface.PB_AvatarShape.prototype.getExpressiontriggertimestamp = function() {
  return /** @type {number} */ (jspb.Message.getFieldWithDefault(this, 14, 0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_AvatarShape} returns this
 */
proto.engineinterface.PB_AvatarShape.prototype.setExpressiontriggertimestamp = function(value) {
  return jspb.Message.setProto3IntField(this, 14, value);
};



/**
 * List of repeated fields within this message type.
 * @private {!Array<number>}
 * @const
 */
proto.engineinterface.PB_Wearable.repeatedFields_ = [3];



if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Wearable.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Wearable.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Wearable} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Wearable.toObject = function(includeInstance, msg) {
  var f, obj = {
    categody: jspb.Message.getFieldWithDefault(msg, 1, ""),
    contentname: jspb.Message.getFieldWithDefault(msg, 2, ""),
    contentsList: jspb.Message.toObjectList(msg.getContentsList(),
    proto.engineinterface.PB_ContentMapping.toObject, includeInstance)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Wearable}
 */
proto.engineinterface.PB_Wearable.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Wearable;
  return proto.engineinterface.PB_Wearable.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Wearable} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Wearable}
 */
proto.engineinterface.PB_Wearable.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setCategody(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setContentname(value);
      break;
    case 3:
      var value = new proto.engineinterface.PB_ContentMapping;
      reader.readMessage(value,proto.engineinterface.PB_ContentMapping.deserializeBinaryFromReader);
      msg.addContents(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Wearable.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Wearable.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Wearable} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Wearable.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getCategody();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getContentname();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
  f = message.getContentsList();
  if (f.length > 0) {
    writer.writeRepeatedMessage(
      3,
      f,
      proto.engineinterface.PB_ContentMapping.serializeBinaryToWriter
    );
  }
};


/**
 * optional string categody = 1;
 * @return {string}
 */
proto.engineinterface.PB_Wearable.prototype.getCategody = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_Wearable} returns this
 */
proto.engineinterface.PB_Wearable.prototype.setCategody = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string contentName = 2;
 * @return {string}
 */
proto.engineinterface.PB_Wearable.prototype.getContentname = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_Wearable} returns this
 */
proto.engineinterface.PB_Wearable.prototype.setContentname = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};


/**
 * repeated PB_ContentMapping contents = 3;
 * @return {!Array<!proto.engineinterface.PB_ContentMapping>}
 */
proto.engineinterface.PB_Wearable.prototype.getContentsList = function() {
  return /** @type{!Array<!proto.engineinterface.PB_ContentMapping>} */ (
    jspb.Message.getRepeatedWrapperField(this, proto.engineinterface.PB_ContentMapping, 3));
};


/**
 * @param {!Array<!proto.engineinterface.PB_ContentMapping>} value
 * @return {!proto.engineinterface.PB_Wearable} returns this
*/
proto.engineinterface.PB_Wearable.prototype.setContentsList = function(value) {
  return jspb.Message.setRepeatedWrapperField(this, 3, value);
};


/**
 * @param {!proto.engineinterface.PB_ContentMapping=} opt_value
 * @param {number=} opt_index
 * @return {!proto.engineinterface.PB_ContentMapping}
 */
proto.engineinterface.PB_Wearable.prototype.addContents = function(opt_value, opt_index) {
  return jspb.Message.addToRepeatedWrapperField(this, 3, opt_value, proto.engineinterface.PB_ContentMapping, opt_index);
};


/**
 * Clears the list making it empty but non-null.
 * @return {!proto.engineinterface.PB_Wearable} returns this
 */
proto.engineinterface.PB_Wearable.prototype.clearContentsList = function() {
  return this.setContentsList([]);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Face.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Face.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Face} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Face.toObject = function(includeInstance, msg) {
  var f, obj = {
    texture: jspb.Message.getFieldWithDefault(msg, 1, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Face}
 */
proto.engineinterface.PB_Face.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Face;
  return proto.engineinterface.PB_Face.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Face} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Face}
 */
proto.engineinterface.PB_Face.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setTexture(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Face.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Face.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Face} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Face.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getTexture();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
};


/**
 * optional string texture = 1;
 * @return {string}
 */
proto.engineinterface.PB_Face.prototype.getTexture = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_Face} returns this
 */
proto.engineinterface.PB_Face.prototype.setTexture = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Eyes.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Eyes.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Eyes} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Eyes.toObject = function(includeInstance, msg) {
  var f, obj = {
    texture: jspb.Message.getFieldWithDefault(msg, 1, ""),
    mask: jspb.Message.getFieldWithDefault(msg, 2, ""),
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Eyes}
 */
proto.engineinterface.PB_Eyes.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Eyes;
  return proto.engineinterface.PB_Eyes.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Eyes} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Eyes}
 */
proto.engineinterface.PB_Eyes.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setTexture(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setMask(value);
      break;
    case 3:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Eyes.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Eyes.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Eyes} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Eyes.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getTexture();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getMask();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      3,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
};


/**
 * optional string texture = 1;
 * @return {string}
 */
proto.engineinterface.PB_Eyes.prototype.getTexture = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_Eyes} returns this
 */
proto.engineinterface.PB_Eyes.prototype.setTexture = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string mask = 2;
 * @return {string}
 */
proto.engineinterface.PB_Eyes.prototype.getMask = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_Eyes} returns this
 */
proto.engineinterface.PB_Eyes.prototype.setMask = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};


/**
 * optional PB_Color4 color = 3;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_Eyes.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 3));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_Eyes} returns this
*/
proto.engineinterface.PB_Eyes.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 3, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Eyes} returns this
 */
proto.engineinterface.PB_Eyes.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Eyes.prototype.hasColor = function() {
  return jspb.Message.getField(this, 3) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Hair.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Hair.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Hair} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Hair.toObject = function(includeInstance, msg) {
  var f, obj = {
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Hair}
 */
proto.engineinterface.PB_Hair.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Hair;
  return proto.engineinterface.PB_Hair.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Hair} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Hair}
 */
proto.engineinterface.PB_Hair.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Hair.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Hair.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Hair} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Hair.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      1,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
};


/**
 * optional PB_Color4 color = 1;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_Hair.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 1));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_Hair} returns this
*/
proto.engineinterface.PB_Hair.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 1, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Hair} returns this
 */
proto.engineinterface.PB_Hair.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Hair.prototype.hasColor = function() {
  return jspb.Message.getField(this, 1) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Skin.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Skin.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Skin} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Skin.toObject = function(includeInstance, msg) {
  var f, obj = {
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Skin}
 */
proto.engineinterface.PB_Skin.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Skin;
  return proto.engineinterface.PB_Skin.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Skin} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Skin}
 */
proto.engineinterface.PB_Skin.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Skin.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Skin.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Skin} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Skin.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      1,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
};


/**
 * optional PB_Color4 color = 1;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_Skin.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 1));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_Skin} returns this
*/
proto.engineinterface.PB_Skin.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 1, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Skin} returns this
 */
proto.engineinterface.PB_Skin.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Skin.prototype.hasColor = function() {
  return jspb.Message.getField(this, 1) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_BasicMaterial.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_BasicMaterial.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_BasicMaterial} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_BasicMaterial.toObject = function(includeInstance, msg) {
  var f, obj = {
    texture: (f = msg.getTexture()) && proto.engineinterface.PB_Texture.toObject(includeInstance, f),
    alphatest: jspb.Message.getFloatingPointFieldWithDefault(msg, 2, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_BasicMaterial}
 */
proto.engineinterface.PB_BasicMaterial.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_BasicMaterial;
  return proto.engineinterface.PB_BasicMaterial.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_BasicMaterial} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_BasicMaterial}
 */
proto.engineinterface.PB_BasicMaterial.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = new proto.engineinterface.PB_Texture;
      reader.readMessage(value,proto.engineinterface.PB_Texture.deserializeBinaryFromReader);
      msg.setTexture(value);
      break;
    case 2:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setAlphatest(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_BasicMaterial.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_BasicMaterial.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_BasicMaterial} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_BasicMaterial.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getTexture();
  if (f != null) {
    writer.writeMessage(
      1,
      f,
      proto.engineinterface.PB_Texture.serializeBinaryToWriter
    );
  }
  f = message.getAlphatest();
  if (f !== 0.0) {
    writer.writeFloat(
      2,
      f
    );
  }
};


/**
 * optional PB_Texture texture = 1;
 * @return {?proto.engineinterface.PB_Texture}
 */
proto.engineinterface.PB_BasicMaterial.prototype.getTexture = function() {
  return /** @type{?proto.engineinterface.PB_Texture} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Texture, 1));
};


/**
 * @param {?proto.engineinterface.PB_Texture|undefined} value
 * @return {!proto.engineinterface.PB_BasicMaterial} returns this
*/
proto.engineinterface.PB_BasicMaterial.prototype.setTexture = function(value) {
  return jspb.Message.setWrapperField(this, 1, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_BasicMaterial} returns this
 */
proto.engineinterface.PB_BasicMaterial.prototype.clearTexture = function() {
  return this.setTexture(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_BasicMaterial.prototype.hasTexture = function() {
  return jspb.Message.getField(this, 1) != null;
};


/**
 * optional float alphaTest = 2;
 * @return {number}
 */
proto.engineinterface.PB_BasicMaterial.prototype.getAlphatest = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 2, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_BasicMaterial} returns this
 */
proto.engineinterface.PB_BasicMaterial.prototype.setAlphatest = function(value) {
  return jspb.Message.setProto3FloatField(this, 2, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Billboard.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Billboard.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Billboard} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Billboard.toObject = function(includeInstance, msg) {
  var f, obj = {
    x: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    y: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    z: jspb.Message.getBooleanFieldWithDefault(msg, 3, false)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Billboard}
 */
proto.engineinterface.PB_Billboard.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Billboard;
  return proto.engineinterface.PB_Billboard.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Billboard} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Billboard}
 */
proto.engineinterface.PB_Billboard.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setX(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setY(value);
      break;
    case 3:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setZ(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Billboard.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Billboard.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Billboard} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Billboard.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getX();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getY();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getZ();
  if (f) {
    writer.writeBool(
      3,
      f
    );
  }
};


/**
 * optional bool x = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_Billboard.prototype.getX = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_Billboard} returns this
 */
proto.engineinterface.PB_Billboard.prototype.setX = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool y = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_Billboard.prototype.getY = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_Billboard} returns this
 */
proto.engineinterface.PB_Billboard.prototype.setY = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional bool z = 3;
 * @return {boolean}
 */
proto.engineinterface.PB_Billboard.prototype.getZ = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 3, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_Billboard} returns this
 */
proto.engineinterface.PB_Billboard.prototype.setZ = function(value) {
  return jspb.Message.setProto3BooleanField(this, 3, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_BoxShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_BoxShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_BoxShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_BoxShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_BoxShape}
 */
proto.engineinterface.PB_BoxShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_BoxShape;
  return proto.engineinterface.PB_BoxShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_BoxShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_BoxShape}
 */
proto.engineinterface.PB_BoxShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_BoxShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_BoxShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_BoxShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_BoxShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_BoxShape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_BoxShape} returns this
 */
proto.engineinterface.PB_BoxShape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_BoxShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_BoxShape} returns this
 */
proto.engineinterface.PB_BoxShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_CircleShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_CircleShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_CircleShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_CircleShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    segments: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    arc: jspb.Message.getFloatingPointFieldWithDefault(msg, 4, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_CircleShape}
 */
proto.engineinterface.PB_CircleShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_CircleShape;
  return proto.engineinterface.PB_CircleShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_CircleShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_CircleShape}
 */
proto.engineinterface.PB_CircleShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSegments(value);
      break;
    case 4:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setArc(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_CircleShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_CircleShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_CircleShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_CircleShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getSegments();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getArc();
  if (f !== 0.0) {
    writer.writeFloat(
      4,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_CircleShape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_CircleShape} returns this
 */
proto.engineinterface.PB_CircleShape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_CircleShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_CircleShape} returns this
 */
proto.engineinterface.PB_CircleShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float segments = 3;
 * @return {number}
 */
proto.engineinterface.PB_CircleShape.prototype.getSegments = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_CircleShape} returns this
 */
proto.engineinterface.PB_CircleShape.prototype.setSegments = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional float arc = 4;
 * @return {number}
 */
proto.engineinterface.PB_CircleShape.prototype.getArc = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 4, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_CircleShape} returns this
 */
proto.engineinterface.PB_CircleShape.prototype.setArc = function(value) {
  return jspb.Message.setProto3FloatField(this, 4, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_ConeShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_ConeShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_ConeShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ConeShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    radiustop: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    radiusbottom: jspb.Message.getFloatingPointFieldWithDefault(msg, 4, 0.0),
    segmentsheight: jspb.Message.getFloatingPointFieldWithDefault(msg, 5, 0.0),
    segmentsradial: jspb.Message.getFloatingPointFieldWithDefault(msg, 6, 0.0),
    openended: jspb.Message.getBooleanFieldWithDefault(msg, 7, false),
    radius: jspb.Message.getFloatingPointFieldWithDefault(msg, 8, 0.0),
    arc: jspb.Message.getFloatingPointFieldWithDefault(msg, 9, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_ConeShape}
 */
proto.engineinterface.PB_ConeShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_ConeShape;
  return proto.engineinterface.PB_ConeShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_ConeShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_ConeShape}
 */
proto.engineinterface.PB_ConeShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setRadiustop(value);
      break;
    case 4:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setRadiusbottom(value);
      break;
    case 5:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSegmentsheight(value);
      break;
    case 6:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSegmentsradial(value);
      break;
    case 7:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setOpenended(value);
      break;
    case 8:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setRadius(value);
      break;
    case 9:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setArc(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_ConeShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_ConeShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_ConeShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_ConeShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getRadiustop();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getRadiusbottom();
  if (f !== 0.0) {
    writer.writeFloat(
      4,
      f
    );
  }
  f = message.getSegmentsheight();
  if (f !== 0.0) {
    writer.writeFloat(
      5,
      f
    );
  }
  f = message.getSegmentsradial();
  if (f !== 0.0) {
    writer.writeFloat(
      6,
      f
    );
  }
  f = message.getOpenended();
  if (f) {
    writer.writeBool(
      7,
      f
    );
  }
  f = message.getRadius();
  if (f !== 0.0) {
    writer.writeFloat(
      8,
      f
    );
  }
  f = message.getArc();
  if (f !== 0.0) {
    writer.writeFloat(
      9,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_ConeShape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_ConeShape} returns this
 */
proto.engineinterface.PB_ConeShape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_ConeShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_ConeShape} returns this
 */
proto.engineinterface.PB_ConeShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float radiusTop = 3;
 * @return {number}
 */
proto.engineinterface.PB_ConeShape.prototype.getRadiustop = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_ConeShape} returns this
 */
proto.engineinterface.PB_ConeShape.prototype.setRadiustop = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional float radiusBottom = 4;
 * @return {number}
 */
proto.engineinterface.PB_ConeShape.prototype.getRadiusbottom = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 4, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_ConeShape} returns this
 */
proto.engineinterface.PB_ConeShape.prototype.setRadiusbottom = function(value) {
  return jspb.Message.setProto3FloatField(this, 4, value);
};


/**
 * optional float segmentsHeight = 5;
 * @return {number}
 */
proto.engineinterface.PB_ConeShape.prototype.getSegmentsheight = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 5, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_ConeShape} returns this
 */
proto.engineinterface.PB_ConeShape.prototype.setSegmentsheight = function(value) {
  return jspb.Message.setProto3FloatField(this, 5, value);
};


/**
 * optional float segmentsRadial = 6;
 * @return {number}
 */
proto.engineinterface.PB_ConeShape.prototype.getSegmentsradial = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 6, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_ConeShape} returns this
 */
proto.engineinterface.PB_ConeShape.prototype.setSegmentsradial = function(value) {
  return jspb.Message.setProto3FloatField(this, 6, value);
};


/**
 * optional bool openEnded = 7;
 * @return {boolean}
 */
proto.engineinterface.PB_ConeShape.prototype.getOpenended = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 7, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_ConeShape} returns this
 */
proto.engineinterface.PB_ConeShape.prototype.setOpenended = function(value) {
  return jspb.Message.setProto3BooleanField(this, 7, value);
};


/**
 * optional float radius = 8;
 * @return {number}
 */
proto.engineinterface.PB_ConeShape.prototype.getRadius = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 8, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_ConeShape} returns this
 */
proto.engineinterface.PB_ConeShape.prototype.setRadius = function(value) {
  return jspb.Message.setProto3FloatField(this, 8, value);
};


/**
 * optional float arc = 9;
 * @return {number}
 */
proto.engineinterface.PB_ConeShape.prototype.getArc = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 9, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_ConeShape} returns this
 */
proto.engineinterface.PB_ConeShape.prototype.setArc = function(value) {
  return jspb.Message.setProto3FloatField(this, 9, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_CylinderShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_CylinderShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_CylinderShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_CylinderShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    radiustop: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    radiusbottom: jspb.Message.getFloatingPointFieldWithDefault(msg, 4, 0.0),
    segmentsheight: jspb.Message.getFloatingPointFieldWithDefault(msg, 5, 0.0),
    segmentsradial: jspb.Message.getFloatingPointFieldWithDefault(msg, 6, 0.0),
    openended: jspb.Message.getBooleanFieldWithDefault(msg, 7, false),
    radius: jspb.Message.getFloatingPointFieldWithDefault(msg, 8, 0.0),
    arc: jspb.Message.getFloatingPointFieldWithDefault(msg, 9, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_CylinderShape}
 */
proto.engineinterface.PB_CylinderShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_CylinderShape;
  return proto.engineinterface.PB_CylinderShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_CylinderShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_CylinderShape}
 */
proto.engineinterface.PB_CylinderShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setRadiustop(value);
      break;
    case 4:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setRadiusbottom(value);
      break;
    case 5:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSegmentsheight(value);
      break;
    case 6:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSegmentsradial(value);
      break;
    case 7:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setOpenended(value);
      break;
    case 8:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setRadius(value);
      break;
    case 9:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setArc(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_CylinderShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_CylinderShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_CylinderShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_CylinderShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getRadiustop();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getRadiusbottom();
  if (f !== 0.0) {
    writer.writeFloat(
      4,
      f
    );
  }
  f = message.getSegmentsheight();
  if (f !== 0.0) {
    writer.writeFloat(
      5,
      f
    );
  }
  f = message.getSegmentsradial();
  if (f !== 0.0) {
    writer.writeFloat(
      6,
      f
    );
  }
  f = message.getOpenended();
  if (f) {
    writer.writeBool(
      7,
      f
    );
  }
  f = message.getRadius();
  if (f !== 0.0) {
    writer.writeFloat(
      8,
      f
    );
  }
  f = message.getArc();
  if (f !== 0.0) {
    writer.writeFloat(
      9,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_CylinderShape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_CylinderShape} returns this
 */
proto.engineinterface.PB_CylinderShape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_CylinderShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_CylinderShape} returns this
 */
proto.engineinterface.PB_CylinderShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float radiusTop = 3;
 * @return {number}
 */
proto.engineinterface.PB_CylinderShape.prototype.getRadiustop = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_CylinderShape} returns this
 */
proto.engineinterface.PB_CylinderShape.prototype.setRadiustop = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional float radiusBottom = 4;
 * @return {number}
 */
proto.engineinterface.PB_CylinderShape.prototype.getRadiusbottom = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 4, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_CylinderShape} returns this
 */
proto.engineinterface.PB_CylinderShape.prototype.setRadiusbottom = function(value) {
  return jspb.Message.setProto3FloatField(this, 4, value);
};


/**
 * optional float segmentsHeight = 5;
 * @return {number}
 */
proto.engineinterface.PB_CylinderShape.prototype.getSegmentsheight = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 5, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_CylinderShape} returns this
 */
proto.engineinterface.PB_CylinderShape.prototype.setSegmentsheight = function(value) {
  return jspb.Message.setProto3FloatField(this, 5, value);
};


/**
 * optional float segmentsRadial = 6;
 * @return {number}
 */
proto.engineinterface.PB_CylinderShape.prototype.getSegmentsradial = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 6, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_CylinderShape} returns this
 */
proto.engineinterface.PB_CylinderShape.prototype.setSegmentsradial = function(value) {
  return jspb.Message.setProto3FloatField(this, 6, value);
};


/**
 * optional bool openEnded = 7;
 * @return {boolean}
 */
proto.engineinterface.PB_CylinderShape.prototype.getOpenended = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 7, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_CylinderShape} returns this
 */
proto.engineinterface.PB_CylinderShape.prototype.setOpenended = function(value) {
  return jspb.Message.setProto3BooleanField(this, 7, value);
};


/**
 * optional float radius = 8;
 * @return {number}
 */
proto.engineinterface.PB_CylinderShape.prototype.getRadius = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 8, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_CylinderShape} returns this
 */
proto.engineinterface.PB_CylinderShape.prototype.setRadius = function(value) {
  return jspb.Message.setProto3FloatField(this, 8, value);
};


/**
 * optional float arc = 9;
 * @return {number}
 */
proto.engineinterface.PB_CylinderShape.prototype.getArc = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 9, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_CylinderShape} returns this
 */
proto.engineinterface.PB_CylinderShape.prototype.setArc = function(value) {
  return jspb.Message.setProto3FloatField(this, 9, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_GlobalPointerDown.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_GlobalPointerDown.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_GlobalPointerDown} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_GlobalPointerDown.toObject = function(includeInstance, msg) {
  var f, obj = {

  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_GlobalPointerDown}
 */
proto.engineinterface.PB_GlobalPointerDown.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_GlobalPointerDown;
  return proto.engineinterface.PB_GlobalPointerDown.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_GlobalPointerDown} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_GlobalPointerDown}
 */
proto.engineinterface.PB_GlobalPointerDown.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_GlobalPointerDown.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_GlobalPointerDown.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_GlobalPointerDown} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_GlobalPointerDown.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_GlobalPointerUp.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_GlobalPointerUp.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_GlobalPointerUp} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_GlobalPointerUp.toObject = function(includeInstance, msg) {
  var f, obj = {

  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_GlobalPointerUp}
 */
proto.engineinterface.PB_GlobalPointerUp.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_GlobalPointerUp;
  return proto.engineinterface.PB_GlobalPointerUp.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_GlobalPointerUp} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_GlobalPointerUp}
 */
proto.engineinterface.PB_GlobalPointerUp.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_GlobalPointerUp.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_GlobalPointerUp.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_GlobalPointerUp} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_GlobalPointerUp.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_GLTFShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_GLTFShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_GLTFShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_GLTFShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    src: jspb.Message.getFieldWithDefault(msg, 3, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_GLTFShape}
 */
proto.engineinterface.PB_GLTFShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_GLTFShape;
  return proto.engineinterface.PB_GLTFShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_GLTFShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_GLTFShape}
 */
proto.engineinterface.PB_GLTFShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {string} */ (reader.readString());
      msg.setSrc(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_GLTFShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_GLTFShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_GLTFShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_GLTFShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getSrc();
  if (f.length > 0) {
    writer.writeString(
      3,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_GLTFShape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_GLTFShape} returns this
 */
proto.engineinterface.PB_GLTFShape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_GLTFShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_GLTFShape} returns this
 */
proto.engineinterface.PB_GLTFShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional string src = 3;
 * @return {string}
 */
proto.engineinterface.PB_GLTFShape.prototype.getSrc = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 3, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_GLTFShape} returns this
 */
proto.engineinterface.PB_GLTFShape.prototype.setSrc = function(value) {
  return jspb.Message.setProto3StringField(this, 3, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Material.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Material.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Material} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Material.toObject = function(includeInstance, msg) {
  var f, obj = {
    alpha: jspb.Message.getFloatingPointFieldWithDefault(msg, 1, 0.0),
    albedocolor: (f = msg.getAlbedocolor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f),
    emissivecolor: (f = msg.getEmissivecolor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f),
    metallic: jspb.Message.getFloatingPointFieldWithDefault(msg, 4, 0.0),
    roughness: jspb.Message.getFloatingPointFieldWithDefault(msg, 5, 0.0),
    ambientcolor: (f = msg.getAmbientcolor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f),
    reflectioncolor: (f = msg.getReflectioncolor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f),
    reflectivitycolor: (f = msg.getReflectivitycolor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f),
    directintensity: jspb.Message.getFloatingPointFieldWithDefault(msg, 9, 0.0),
    microsurface: jspb.Message.getFloatingPointFieldWithDefault(msg, 10, 0.0),
    emissiveintensity: jspb.Message.getFloatingPointFieldWithDefault(msg, 11, 0.0),
    environmentintensity: jspb.Message.getFloatingPointFieldWithDefault(msg, 12, 0.0),
    specularintensity: jspb.Message.getFloatingPointFieldWithDefault(msg, 13, 0.0),
    albedotexture: (f = msg.getAlbedotexture()) && proto.engineinterface.PB_Texture.toObject(includeInstance, f),
    alphatexture: (f = msg.getAlphatexture()) && proto.engineinterface.PB_Texture.toObject(includeInstance, f),
    emissivetexture: (f = msg.getEmissivetexture()) && proto.engineinterface.PB_Texture.toObject(includeInstance, f),
    bumptexture: (f = msg.getBumptexture()) && proto.engineinterface.PB_Texture.toObject(includeInstance, f),
    refractiontexture: (f = msg.getRefractiontexture()) && proto.engineinterface.PB_Texture.toObject(includeInstance, f),
    disablelighting: jspb.Message.getBooleanFieldWithDefault(msg, 19, false),
    transparencymode: jspb.Message.getFloatingPointFieldWithDefault(msg, 20, 0.0),
    hasalpha: jspb.Message.getBooleanFieldWithDefault(msg, 21, false)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Material}
 */
proto.engineinterface.PB_Material.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Material;
  return proto.engineinterface.PB_Material.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Material} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Material}
 */
proto.engineinterface.PB_Material.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setAlpha(value);
      break;
    case 2:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setAlbedocolor(value);
      break;
    case 3:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setEmissivecolor(value);
      break;
    case 4:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setMetallic(value);
      break;
    case 5:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setRoughness(value);
      break;
    case 6:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setAmbientcolor(value);
      break;
    case 7:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setReflectioncolor(value);
      break;
    case 8:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setReflectivitycolor(value);
      break;
    case 9:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setDirectintensity(value);
      break;
    case 10:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setMicrosurface(value);
      break;
    case 11:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setEmissiveintensity(value);
      break;
    case 12:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setEnvironmentintensity(value);
      break;
    case 13:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSpecularintensity(value);
      break;
    case 14:
      var value = new proto.engineinterface.PB_Texture;
      reader.readMessage(value,proto.engineinterface.PB_Texture.deserializeBinaryFromReader);
      msg.setAlbedotexture(value);
      break;
    case 15:
      var value = new proto.engineinterface.PB_Texture;
      reader.readMessage(value,proto.engineinterface.PB_Texture.deserializeBinaryFromReader);
      msg.setAlphatexture(value);
      break;
    case 16:
      var value = new proto.engineinterface.PB_Texture;
      reader.readMessage(value,proto.engineinterface.PB_Texture.deserializeBinaryFromReader);
      msg.setEmissivetexture(value);
      break;
    case 17:
      var value = new proto.engineinterface.PB_Texture;
      reader.readMessage(value,proto.engineinterface.PB_Texture.deserializeBinaryFromReader);
      msg.setBumptexture(value);
      break;
    case 18:
      var value = new proto.engineinterface.PB_Texture;
      reader.readMessage(value,proto.engineinterface.PB_Texture.deserializeBinaryFromReader);
      msg.setRefractiontexture(value);
      break;
    case 19:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setDisablelighting(value);
      break;
    case 20:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setTransparencymode(value);
      break;
    case 21:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setHasalpha(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Material.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Material.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Material} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Material.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getAlpha();
  if (f !== 0.0) {
    writer.writeFloat(
      1,
      f
    );
  }
  f = message.getAlbedocolor();
  if (f != null) {
    writer.writeMessage(
      2,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
  f = message.getEmissivecolor();
  if (f != null) {
    writer.writeMessage(
      3,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
  f = message.getMetallic();
  if (f !== 0.0) {
    writer.writeFloat(
      4,
      f
    );
  }
  f = message.getRoughness();
  if (f !== 0.0) {
    writer.writeFloat(
      5,
      f
    );
  }
  f = message.getAmbientcolor();
  if (f != null) {
    writer.writeMessage(
      6,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
  f = message.getReflectioncolor();
  if (f != null) {
    writer.writeMessage(
      7,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
  f = message.getReflectivitycolor();
  if (f != null) {
    writer.writeMessage(
      8,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
  f = message.getDirectintensity();
  if (f !== 0.0) {
    writer.writeFloat(
      9,
      f
    );
  }
  f = message.getMicrosurface();
  if (f !== 0.0) {
    writer.writeFloat(
      10,
      f
    );
  }
  f = message.getEmissiveintensity();
  if (f !== 0.0) {
    writer.writeFloat(
      11,
      f
    );
  }
  f = message.getEnvironmentintensity();
  if (f !== 0.0) {
    writer.writeFloat(
      12,
      f
    );
  }
  f = message.getSpecularintensity();
  if (f !== 0.0) {
    writer.writeFloat(
      13,
      f
    );
  }
  f = message.getAlbedotexture();
  if (f != null) {
    writer.writeMessage(
      14,
      f,
      proto.engineinterface.PB_Texture.serializeBinaryToWriter
    );
  }
  f = message.getAlphatexture();
  if (f != null) {
    writer.writeMessage(
      15,
      f,
      proto.engineinterface.PB_Texture.serializeBinaryToWriter
    );
  }
  f = message.getEmissivetexture();
  if (f != null) {
    writer.writeMessage(
      16,
      f,
      proto.engineinterface.PB_Texture.serializeBinaryToWriter
    );
  }
  f = message.getBumptexture();
  if (f != null) {
    writer.writeMessage(
      17,
      f,
      proto.engineinterface.PB_Texture.serializeBinaryToWriter
    );
  }
  f = message.getRefractiontexture();
  if (f != null) {
    writer.writeMessage(
      18,
      f,
      proto.engineinterface.PB_Texture.serializeBinaryToWriter
    );
  }
  f = message.getDisablelighting();
  if (f) {
    writer.writeBool(
      19,
      f
    );
  }
  f = message.getTransparencymode();
  if (f !== 0.0) {
    writer.writeFloat(
      20,
      f
    );
  }
  f = message.getHasalpha();
  if (f) {
    writer.writeBool(
      21,
      f
    );
  }
};


/**
 * optional float alpha = 1;
 * @return {number}
 */
proto.engineinterface.PB_Material.prototype.getAlpha = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 1, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setAlpha = function(value) {
  return jspb.Message.setProto3FloatField(this, 1, value);
};


/**
 * optional PB_Color3 albedoColor = 2;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_Material.prototype.getAlbedocolor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 2));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_Material} returns this
*/
proto.engineinterface.PB_Material.prototype.setAlbedocolor = function(value) {
  return jspb.Message.setWrapperField(this, 2, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.clearAlbedocolor = function() {
  return this.setAlbedocolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.hasAlbedocolor = function() {
  return jspb.Message.getField(this, 2) != null;
};


/**
 * optional PB_Color3 emissiveColor = 3;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_Material.prototype.getEmissivecolor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 3));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_Material} returns this
*/
proto.engineinterface.PB_Material.prototype.setEmissivecolor = function(value) {
  return jspb.Message.setWrapperField(this, 3, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.clearEmissivecolor = function() {
  return this.setEmissivecolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.hasEmissivecolor = function() {
  return jspb.Message.getField(this, 3) != null;
};


/**
 * optional float metallic = 4;
 * @return {number}
 */
proto.engineinterface.PB_Material.prototype.getMetallic = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 4, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setMetallic = function(value) {
  return jspb.Message.setProto3FloatField(this, 4, value);
};


/**
 * optional float roughness = 5;
 * @return {number}
 */
proto.engineinterface.PB_Material.prototype.getRoughness = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 5, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setRoughness = function(value) {
  return jspb.Message.setProto3FloatField(this, 5, value);
};


/**
 * optional PB_Color3 ambientColor = 6;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_Material.prototype.getAmbientcolor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 6));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_Material} returns this
*/
proto.engineinterface.PB_Material.prototype.setAmbientcolor = function(value) {
  return jspb.Message.setWrapperField(this, 6, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.clearAmbientcolor = function() {
  return this.setAmbientcolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.hasAmbientcolor = function() {
  return jspb.Message.getField(this, 6) != null;
};


/**
 * optional PB_Color3 reflectionColor = 7;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_Material.prototype.getReflectioncolor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 7));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_Material} returns this
*/
proto.engineinterface.PB_Material.prototype.setReflectioncolor = function(value) {
  return jspb.Message.setWrapperField(this, 7, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.clearReflectioncolor = function() {
  return this.setReflectioncolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.hasReflectioncolor = function() {
  return jspb.Message.getField(this, 7) != null;
};


/**
 * optional PB_Color3 reflectivityColor = 8;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_Material.prototype.getReflectivitycolor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 8));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_Material} returns this
*/
proto.engineinterface.PB_Material.prototype.setReflectivitycolor = function(value) {
  return jspb.Message.setWrapperField(this, 8, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.clearReflectivitycolor = function() {
  return this.setReflectivitycolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.hasReflectivitycolor = function() {
  return jspb.Message.getField(this, 8) != null;
};


/**
 * optional float directIntensity = 9;
 * @return {number}
 */
proto.engineinterface.PB_Material.prototype.getDirectintensity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 9, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setDirectintensity = function(value) {
  return jspb.Message.setProto3FloatField(this, 9, value);
};


/**
 * optional float microSurface = 10;
 * @return {number}
 */
proto.engineinterface.PB_Material.prototype.getMicrosurface = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 10, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setMicrosurface = function(value) {
  return jspb.Message.setProto3FloatField(this, 10, value);
};


/**
 * optional float emissiveIntensity = 11;
 * @return {number}
 */
proto.engineinterface.PB_Material.prototype.getEmissiveintensity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 11, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setEmissiveintensity = function(value) {
  return jspb.Message.setProto3FloatField(this, 11, value);
};


/**
 * optional float environmentIntensity = 12;
 * @return {number}
 */
proto.engineinterface.PB_Material.prototype.getEnvironmentintensity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 12, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setEnvironmentintensity = function(value) {
  return jspb.Message.setProto3FloatField(this, 12, value);
};


/**
 * optional float specularIntensity = 13;
 * @return {number}
 */
proto.engineinterface.PB_Material.prototype.getSpecularintensity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 13, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setSpecularintensity = function(value) {
  return jspb.Message.setProto3FloatField(this, 13, value);
};


/**
 * optional PB_Texture albedoTexture = 14;
 * @return {?proto.engineinterface.PB_Texture}
 */
proto.engineinterface.PB_Material.prototype.getAlbedotexture = function() {
  return /** @type{?proto.engineinterface.PB_Texture} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Texture, 14));
};


/**
 * @param {?proto.engineinterface.PB_Texture|undefined} value
 * @return {!proto.engineinterface.PB_Material} returns this
*/
proto.engineinterface.PB_Material.prototype.setAlbedotexture = function(value) {
  return jspb.Message.setWrapperField(this, 14, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.clearAlbedotexture = function() {
  return this.setAlbedotexture(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.hasAlbedotexture = function() {
  return jspb.Message.getField(this, 14) != null;
};


/**
 * optional PB_Texture alphaTexture = 15;
 * @return {?proto.engineinterface.PB_Texture}
 */
proto.engineinterface.PB_Material.prototype.getAlphatexture = function() {
  return /** @type{?proto.engineinterface.PB_Texture} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Texture, 15));
};


/**
 * @param {?proto.engineinterface.PB_Texture|undefined} value
 * @return {!proto.engineinterface.PB_Material} returns this
*/
proto.engineinterface.PB_Material.prototype.setAlphatexture = function(value) {
  return jspb.Message.setWrapperField(this, 15, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.clearAlphatexture = function() {
  return this.setAlphatexture(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.hasAlphatexture = function() {
  return jspb.Message.getField(this, 15) != null;
};


/**
 * optional PB_Texture emissiveTexture = 16;
 * @return {?proto.engineinterface.PB_Texture}
 */
proto.engineinterface.PB_Material.prototype.getEmissivetexture = function() {
  return /** @type{?proto.engineinterface.PB_Texture} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Texture, 16));
};


/**
 * @param {?proto.engineinterface.PB_Texture|undefined} value
 * @return {!proto.engineinterface.PB_Material} returns this
*/
proto.engineinterface.PB_Material.prototype.setEmissivetexture = function(value) {
  return jspb.Message.setWrapperField(this, 16, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.clearEmissivetexture = function() {
  return this.setEmissivetexture(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.hasEmissivetexture = function() {
  return jspb.Message.getField(this, 16) != null;
};


/**
 * optional PB_Texture bumpTexture = 17;
 * @return {?proto.engineinterface.PB_Texture}
 */
proto.engineinterface.PB_Material.prototype.getBumptexture = function() {
  return /** @type{?proto.engineinterface.PB_Texture} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Texture, 17));
};


/**
 * @param {?proto.engineinterface.PB_Texture|undefined} value
 * @return {!proto.engineinterface.PB_Material} returns this
*/
proto.engineinterface.PB_Material.prototype.setBumptexture = function(value) {
  return jspb.Message.setWrapperField(this, 17, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.clearBumptexture = function() {
  return this.setBumptexture(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.hasBumptexture = function() {
  return jspb.Message.getField(this, 17) != null;
};


/**
 * optional PB_Texture refractionTexture = 18;
 * @return {?proto.engineinterface.PB_Texture}
 */
proto.engineinterface.PB_Material.prototype.getRefractiontexture = function() {
  return /** @type{?proto.engineinterface.PB_Texture} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Texture, 18));
};


/**
 * @param {?proto.engineinterface.PB_Texture|undefined} value
 * @return {!proto.engineinterface.PB_Material} returns this
*/
proto.engineinterface.PB_Material.prototype.setRefractiontexture = function(value) {
  return jspb.Message.setWrapperField(this, 18, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.clearRefractiontexture = function() {
  return this.setRefractiontexture(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.hasRefractiontexture = function() {
  return jspb.Message.getField(this, 18) != null;
};


/**
 * optional bool disableLighting = 19;
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.getDisablelighting = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 19, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setDisablelighting = function(value) {
  return jspb.Message.setProto3BooleanField(this, 19, value);
};


/**
 * optional float transparencyMode = 20;
 * @return {number}
 */
proto.engineinterface.PB_Material.prototype.getTransparencymode = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 20, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setTransparencymode = function(value) {
  return jspb.Message.setProto3FloatField(this, 20, value);
};


/**
 * optional bool hasAlpha = 21;
 * @return {boolean}
 */
proto.engineinterface.PB_Material.prototype.getHasalpha = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 21, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_Material} returns this
 */
proto.engineinterface.PB_Material.prototype.setHasalpha = function(value) {
  return jspb.Message.setProto3BooleanField(this, 21, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_NFTShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_NFTShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_NFTShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_NFTShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    src: jspb.Message.getFieldWithDefault(msg, 3, ""),
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_NFTShape}
 */
proto.engineinterface.PB_NFTShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_NFTShape;
  return proto.engineinterface.PB_NFTShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_NFTShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_NFTShape}
 */
proto.engineinterface.PB_NFTShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {string} */ (reader.readString());
      msg.setSrc(value);
      break;
    case 4:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_NFTShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_NFTShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_NFTShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_NFTShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getSrc();
  if (f.length > 0) {
    writer.writeString(
      3,
      f
    );
  }
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      4,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_NFTShape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_NFTShape} returns this
 */
proto.engineinterface.PB_NFTShape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_NFTShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_NFTShape} returns this
 */
proto.engineinterface.PB_NFTShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional string src = 3;
 * @return {string}
 */
proto.engineinterface.PB_NFTShape.prototype.getSrc = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 3, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_NFTShape} returns this
 */
proto.engineinterface.PB_NFTShape.prototype.setSrc = function(value) {
  return jspb.Message.setProto3StringField(this, 3, value);
};


/**
 * optional PB_Color3 color = 4;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_NFTShape.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 4));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_NFTShape} returns this
*/
proto.engineinterface.PB_NFTShape.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 4, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_NFTShape} returns this
 */
proto.engineinterface.PB_NFTShape.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_NFTShape.prototype.hasColor = function() {
  return jspb.Message.getField(this, 4) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_OBJShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_OBJShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_OBJShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_OBJShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    src: jspb.Message.getFieldWithDefault(msg, 3, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_OBJShape}
 */
proto.engineinterface.PB_OBJShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_OBJShape;
  return proto.engineinterface.PB_OBJShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_OBJShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_OBJShape}
 */
proto.engineinterface.PB_OBJShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {string} */ (reader.readString());
      msg.setSrc(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_OBJShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_OBJShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_OBJShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_OBJShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getSrc();
  if (f.length > 0) {
    writer.writeString(
      3,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_OBJShape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_OBJShape} returns this
 */
proto.engineinterface.PB_OBJShape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_OBJShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_OBJShape} returns this
 */
proto.engineinterface.PB_OBJShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional string src = 3;
 * @return {string}
 */
proto.engineinterface.PB_OBJShape.prototype.getSrc = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 3, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_OBJShape} returns this
 */
proto.engineinterface.PB_OBJShape.prototype.setSrc = function(value) {
  return jspb.Message.setProto3StringField(this, 3, value);
};



/**
 * List of repeated fields within this message type.
 * @private {!Array<number>}
 * @const
 */
proto.engineinterface.PB_PlaneShape.repeatedFields_ = [5];



if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_PlaneShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_PlaneShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_PlaneShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_PlaneShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    width: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    height: jspb.Message.getFloatingPointFieldWithDefault(msg, 4, 0.0),
    uvsList: (f = jspb.Message.getRepeatedFloatingPointField(msg, 5)) == null ? undefined : f
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_PlaneShape}
 */
proto.engineinterface.PB_PlaneShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_PlaneShape;
  return proto.engineinterface.PB_PlaneShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_PlaneShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_PlaneShape}
 */
proto.engineinterface.PB_PlaneShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setWidth(value);
      break;
    case 4:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setHeight(value);
      break;
    case 5:
      var value = /** @type {!Array<number>} */ (reader.readPackedFloat());
      msg.setUvsList(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_PlaneShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_PlaneShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_PlaneShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_PlaneShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getWidth();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHeight();
  if (f !== 0.0) {
    writer.writeFloat(
      4,
      f
    );
  }
  f = message.getUvsList();
  if (f.length > 0) {
    writer.writePackedFloat(
      5,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_PlaneShape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_PlaneShape} returns this
 */
proto.engineinterface.PB_PlaneShape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_PlaneShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_PlaneShape} returns this
 */
proto.engineinterface.PB_PlaneShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float width = 3;
 * @return {number}
 */
proto.engineinterface.PB_PlaneShape.prototype.getWidth = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_PlaneShape} returns this
 */
proto.engineinterface.PB_PlaneShape.prototype.setWidth = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional float height = 4;
 * @return {number}
 */
proto.engineinterface.PB_PlaneShape.prototype.getHeight = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 4, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_PlaneShape} returns this
 */
proto.engineinterface.PB_PlaneShape.prototype.setHeight = function(value) {
  return jspb.Message.setProto3FloatField(this, 4, value);
};


/**
 * repeated float uvs = 5;
 * @return {!Array<number>}
 */
proto.engineinterface.PB_PlaneShape.prototype.getUvsList = function() {
  return /** @type {!Array<number>} */ (jspb.Message.getRepeatedFloatingPointField(this, 5));
};


/**
 * @param {!Array<number>} value
 * @return {!proto.engineinterface.PB_PlaneShape} returns this
 */
proto.engineinterface.PB_PlaneShape.prototype.setUvsList = function(value) {
  return jspb.Message.setField(this, 5, value || []);
};


/**
 * @param {number} value
 * @param {number=} opt_index
 * @return {!proto.engineinterface.PB_PlaneShape} returns this
 */
proto.engineinterface.PB_PlaneShape.prototype.addUvs = function(value, opt_index) {
  return jspb.Message.addToRepeatedField(this, 5, value, opt_index);
};


/**
 * Clears the list making it empty but non-null.
 * @return {!proto.engineinterface.PB_PlaneShape} returns this
 */
proto.engineinterface.PB_PlaneShape.prototype.clearUvsList = function() {
  return this.setUvsList([]);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Shape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Shape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Shape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Shape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Shape}
 */
proto.engineinterface.PB_Shape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Shape;
  return proto.engineinterface.PB_Shape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Shape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Shape}
 */
proto.engineinterface.PB_Shape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Shape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Shape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Shape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Shape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_Shape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_Shape} returns this
 */
proto.engineinterface.PB_Shape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_Shape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_Shape} returns this
 */
proto.engineinterface.PB_Shape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_SphereShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_SphereShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_SphereShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_SphereShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_SphereShape}
 */
proto.engineinterface.PB_SphereShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_SphereShape;
  return proto.engineinterface.PB_SphereShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_SphereShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_SphereShape}
 */
proto.engineinterface.PB_SphereShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_SphereShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_SphereShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_SphereShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_SphereShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_SphereShape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_SphereShape} returns this
 */
proto.engineinterface.PB_SphereShape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_SphereShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_SphereShape} returns this
 */
proto.engineinterface.PB_SphereShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_TextShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_TextShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_TextShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_TextShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    withcollisions: jspb.Message.getBooleanFieldWithDefault(msg, 1, false),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    outlinewidth: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    outlinecolor: (f = msg.getOutlinecolor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f),
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f),
    fontsize: jspb.Message.getFloatingPointFieldWithDefault(msg, 6, 0.0),
    fontweight: jspb.Message.getFieldWithDefault(msg, 7, ""),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 8, 0.0),
    value: jspb.Message.getFieldWithDefault(msg, 9, ""),
    linespacing: jspb.Message.getFieldWithDefault(msg, 10, ""),
    linecount: jspb.Message.getFloatingPointFieldWithDefault(msg, 11, 0.0),
    resizetofit: jspb.Message.getBooleanFieldWithDefault(msg, 12, false),
    textwrapping: jspb.Message.getBooleanFieldWithDefault(msg, 13, false),
    shadowblur: jspb.Message.getFloatingPointFieldWithDefault(msg, 14, 0.0),
    shadowoffsetx: jspb.Message.getFloatingPointFieldWithDefault(msg, 15, 0.0),
    shadowoffsety: jspb.Message.getFloatingPointFieldWithDefault(msg, 16, 0.0),
    shadowcolor: (f = msg.getShadowcolor()) && proto.engineinterface.PB_Color3.toObject(includeInstance, f),
    zindex: jspb.Message.getFloatingPointFieldWithDefault(msg, 18, 0.0),
    htextalign: jspb.Message.getFieldWithDefault(msg, 19, ""),
    vtextalign: jspb.Message.getFieldWithDefault(msg, 20, ""),
    width: jspb.Message.getFloatingPointFieldWithDefault(msg, 21, 0.0),
    height: jspb.Message.getFloatingPointFieldWithDefault(msg, 22, 0.0),
    paddingtop: jspb.Message.getFloatingPointFieldWithDefault(msg, 23, 0.0),
    paddingright: jspb.Message.getFloatingPointFieldWithDefault(msg, 24, 0.0),
    paddingbottom: jspb.Message.getFloatingPointFieldWithDefault(msg, 25, 0.0),
    paddingleft: jspb.Message.getFloatingPointFieldWithDefault(msg, 26, 0.0),
    ispickable: jspb.Message.getBooleanFieldWithDefault(msg, 27, false),
    billboard: jspb.Message.getBooleanFieldWithDefault(msg, 28, false)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_TextShape}
 */
proto.engineinterface.PB_TextShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_TextShape;
  return proto.engineinterface.PB_TextShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_TextShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_TextShape}
 */
proto.engineinterface.PB_TextShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setWithcollisions(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOutlinewidth(value);
      break;
    case 4:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setOutlinecolor(value);
      break;
    case 5:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    case 6:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setFontsize(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setFontweight(value);
      break;
    case 8:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setValue(value);
      break;
    case 10:
      var value = /** @type {string} */ (reader.readString());
      msg.setLinespacing(value);
      break;
    case 11:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setLinecount(value);
      break;
    case 12:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setResizetofit(value);
      break;
    case 13:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setTextwrapping(value);
      break;
    case 14:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowblur(value);
      break;
    case 15:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowoffsetx(value);
      break;
    case 16:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowoffsety(value);
      break;
    case 17:
      var value = new proto.engineinterface.PB_Color3;
      reader.readMessage(value,proto.engineinterface.PB_Color3.deserializeBinaryFromReader);
      msg.setShadowcolor(value);
      break;
    case 18:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setZindex(value);
      break;
    case 19:
      var value = /** @type {string} */ (reader.readString());
      msg.setHtextalign(value);
      break;
    case 20:
      var value = /** @type {string} */ (reader.readString());
      msg.setVtextalign(value);
      break;
    case 21:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setWidth(value);
      break;
    case 22:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setHeight(value);
      break;
    case 23:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingtop(value);
      break;
    case 24:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingright(value);
      break;
    case 25:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingbottom(value);
      break;
    case 26:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingleft(value);
      break;
    case 27:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIspickable(value);
      break;
    case 28:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setBillboard(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_TextShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_TextShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_TextShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_TextShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getWithcollisions();
  if (f) {
    writer.writeBool(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getOutlinewidth();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getOutlinecolor();
  if (f != null) {
    writer.writeMessage(
      4,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      5,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
  f = message.getFontsize();
  if (f !== 0.0) {
    writer.writeFloat(
      6,
      f
    );
  }
  f = message.getFontweight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      8,
      f
    );
  }
  f = message.getValue();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getLinespacing();
  if (f.length > 0) {
    writer.writeString(
      10,
      f
    );
  }
  f = message.getLinecount();
  if (f !== 0.0) {
    writer.writeFloat(
      11,
      f
    );
  }
  f = message.getResizetofit();
  if (f) {
    writer.writeBool(
      12,
      f
    );
  }
  f = message.getTextwrapping();
  if (f) {
    writer.writeBool(
      13,
      f
    );
  }
  f = message.getShadowblur();
  if (f !== 0.0) {
    writer.writeFloat(
      14,
      f
    );
  }
  f = message.getShadowoffsetx();
  if (f !== 0.0) {
    writer.writeFloat(
      15,
      f
    );
  }
  f = message.getShadowoffsety();
  if (f !== 0.0) {
    writer.writeFloat(
      16,
      f
    );
  }
  f = message.getShadowcolor();
  if (f != null) {
    writer.writeMessage(
      17,
      f,
      proto.engineinterface.PB_Color3.serializeBinaryToWriter
    );
  }
  f = message.getZindex();
  if (f !== 0.0) {
    writer.writeFloat(
      18,
      f
    );
  }
  f = message.getHtextalign();
  if (f.length > 0) {
    writer.writeString(
      19,
      f
    );
  }
  f = message.getVtextalign();
  if (f.length > 0) {
    writer.writeString(
      20,
      f
    );
  }
  f = message.getWidth();
  if (f !== 0.0) {
    writer.writeFloat(
      21,
      f
    );
  }
  f = message.getHeight();
  if (f !== 0.0) {
    writer.writeFloat(
      22,
      f
    );
  }
  f = message.getPaddingtop();
  if (f !== 0.0) {
    writer.writeFloat(
      23,
      f
    );
  }
  f = message.getPaddingright();
  if (f !== 0.0) {
    writer.writeFloat(
      24,
      f
    );
  }
  f = message.getPaddingbottom();
  if (f !== 0.0) {
    writer.writeFloat(
      25,
      f
    );
  }
  f = message.getPaddingleft();
  if (f !== 0.0) {
    writer.writeFloat(
      26,
      f
    );
  }
  f = message.getIspickable();
  if (f) {
    writer.writeBool(
      27,
      f
    );
  }
  f = message.getBillboard();
  if (f) {
    writer.writeBool(
      28,
      f
    );
  }
};


/**
 * optional bool withCollisions = 1;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShape.prototype.getWithcollisions = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 1, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setWithcollisions = function(value) {
  return jspb.Message.setProto3BooleanField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float outlineWidth = 3;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getOutlinewidth = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setOutlinewidth = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional PB_Color3 outlineColor = 4;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_TextShape.prototype.getOutlinecolor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 4));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
*/
proto.engineinterface.PB_TextShape.prototype.setOutlinecolor = function(value) {
  return jspb.Message.setWrapperField(this, 4, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.clearOutlinecolor = function() {
  return this.setOutlinecolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_TextShape.prototype.hasOutlinecolor = function() {
  return jspb.Message.getField(this, 4) != null;
};


/**
 * optional PB_Color3 color = 5;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_TextShape.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 5));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
*/
proto.engineinterface.PB_TextShape.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 5, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_TextShape.prototype.hasColor = function() {
  return jspb.Message.getField(this, 5) != null;
};


/**
 * optional float fontSize = 6;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getFontsize = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 6, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setFontsize = function(value) {
  return jspb.Message.setProto3FloatField(this, 6, value);
};


/**
 * optional string fontWeight = 7;
 * @return {string}
 */
proto.engineinterface.PB_TextShape.prototype.getFontweight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setFontweight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional float opacity = 8;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 8, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 8, value);
};


/**
 * optional string value = 9;
 * @return {string}
 */
proto.engineinterface.PB_TextShape.prototype.getValue = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setValue = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional string lineSpacing = 10;
 * @return {string}
 */
proto.engineinterface.PB_TextShape.prototype.getLinespacing = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 10, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setLinespacing = function(value) {
  return jspb.Message.setProto3StringField(this, 10, value);
};


/**
 * optional float lineCount = 11;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getLinecount = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 11, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setLinecount = function(value) {
  return jspb.Message.setProto3FloatField(this, 11, value);
};


/**
 * optional bool resizeToFit = 12;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShape.prototype.getResizetofit = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 12, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setResizetofit = function(value) {
  return jspb.Message.setProto3BooleanField(this, 12, value);
};


/**
 * optional bool textWrapping = 13;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShape.prototype.getTextwrapping = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 13, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setTextwrapping = function(value) {
  return jspb.Message.setProto3BooleanField(this, 13, value);
};


/**
 * optional float shadowBlur = 14;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getShadowblur = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 14, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setShadowblur = function(value) {
  return jspb.Message.setProto3FloatField(this, 14, value);
};


/**
 * optional float shadowOffsetX = 15;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getShadowoffsetx = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 15, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setShadowoffsetx = function(value) {
  return jspb.Message.setProto3FloatField(this, 15, value);
};


/**
 * optional float shadowOffsetY = 16;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getShadowoffsety = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 16, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setShadowoffsety = function(value) {
  return jspb.Message.setProto3FloatField(this, 16, value);
};


/**
 * optional PB_Color3 shadowColor = 17;
 * @return {?proto.engineinterface.PB_Color3}
 */
proto.engineinterface.PB_TextShape.prototype.getShadowcolor = function() {
  return /** @type{?proto.engineinterface.PB_Color3} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color3, 17));
};


/**
 * @param {?proto.engineinterface.PB_Color3|undefined} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
*/
proto.engineinterface.PB_TextShape.prototype.setShadowcolor = function(value) {
  return jspb.Message.setWrapperField(this, 17, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.clearShadowcolor = function() {
  return this.setShadowcolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_TextShape.prototype.hasShadowcolor = function() {
  return jspb.Message.getField(this, 17) != null;
};


/**
 * optional float zIndex = 18;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getZindex = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 18, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setZindex = function(value) {
  return jspb.Message.setProto3FloatField(this, 18, value);
};


/**
 * optional string hTextAlign = 19;
 * @return {string}
 */
proto.engineinterface.PB_TextShape.prototype.getHtextalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 19, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setHtextalign = function(value) {
  return jspb.Message.setProto3StringField(this, 19, value);
};


/**
 * optional string vTextAlign = 20;
 * @return {string}
 */
proto.engineinterface.PB_TextShape.prototype.getVtextalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 20, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setVtextalign = function(value) {
  return jspb.Message.setProto3StringField(this, 20, value);
};


/**
 * optional float width = 21;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getWidth = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 21, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setWidth = function(value) {
  return jspb.Message.setProto3FloatField(this, 21, value);
};


/**
 * optional float height = 22;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getHeight = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 22, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setHeight = function(value) {
  return jspb.Message.setProto3FloatField(this, 22, value);
};


/**
 * optional float paddingTop = 23;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getPaddingtop = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 23, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setPaddingtop = function(value) {
  return jspb.Message.setProto3FloatField(this, 23, value);
};


/**
 * optional float paddingRight = 24;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getPaddingright = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 24, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setPaddingright = function(value) {
  return jspb.Message.setProto3FloatField(this, 24, value);
};


/**
 * optional float paddingBottom = 25;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getPaddingbottom = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 25, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setPaddingbottom = function(value) {
  return jspb.Message.setProto3FloatField(this, 25, value);
};


/**
 * optional float paddingLeft = 26;
 * @return {number}
 */
proto.engineinterface.PB_TextShape.prototype.getPaddingleft = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 26, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setPaddingleft = function(value) {
  return jspb.Message.setProto3FloatField(this, 26, value);
};


/**
 * optional bool isPickable = 27;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShape.prototype.getIspickable = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 27, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setIspickable = function(value) {
  return jspb.Message.setProto3BooleanField(this, 27, value);
};


/**
 * optional bool billboard = 28;
 * @return {boolean}
 */
proto.engineinterface.PB_TextShape.prototype.getBillboard = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 28, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_TextShape} returns this
 */
proto.engineinterface.PB_TextShape.prototype.setBillboard = function(value) {
  return jspb.Message.setProto3BooleanField(this, 28, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_Texture.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_Texture.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_Texture} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Texture.toObject = function(includeInstance, msg) {
  var f, obj = {
    src: jspb.Message.getFieldWithDefault(msg, 1, ""),
    samplingmode: jspb.Message.getFloatingPointFieldWithDefault(msg, 2, 0.0),
    wrap: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    hasalpha: jspb.Message.getBooleanFieldWithDefault(msg, 4, false)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_Texture}
 */
proto.engineinterface.PB_Texture.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_Texture;
  return proto.engineinterface.PB_Texture.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_Texture} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_Texture}
 */
proto.engineinterface.PB_Texture.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setSrc(value);
      break;
    case 2:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSamplingmode(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setWrap(value);
      break;
    case 4:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setHasalpha(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_Texture.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_Texture.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_Texture} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_Texture.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getSrc();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getSamplingmode();
  if (f !== 0.0) {
    writer.writeFloat(
      2,
      f
    );
  }
  f = message.getWrap();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHasalpha();
  if (f) {
    writer.writeBool(
      4,
      f
    );
  }
};


/**
 * optional string src = 1;
 * @return {string}
 */
proto.engineinterface.PB_Texture.prototype.getSrc = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_Texture} returns this
 */
proto.engineinterface.PB_Texture.prototype.setSrc = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional float samplingMode = 2;
 * @return {number}
 */
proto.engineinterface.PB_Texture.prototype.getSamplingmode = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 2, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Texture} returns this
 */
proto.engineinterface.PB_Texture.prototype.setSamplingmode = function(value) {
  return jspb.Message.setProto3FloatField(this, 2, value);
};


/**
 * optional float wrap = 3;
 * @return {number}
 */
proto.engineinterface.PB_Texture.prototype.getWrap = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_Texture} returns this
 */
proto.engineinterface.PB_Texture.prototype.setWrap = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional bool hasAlpha = 4;
 * @return {boolean}
 */
proto.engineinterface.PB_Texture.prototype.getHasalpha = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 4, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_Texture} returns this
 */
proto.engineinterface.PB_Texture.prototype.setHasalpha = function(value) {
  return jspb.Message.setProto3BooleanField(this, 4, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UIButton.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UIButton.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UIButton} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIButton.toObject = function(includeInstance, msg) {
  var f, obj = {
    name: jspb.Message.getFieldWithDefault(msg, 1, ""),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    halign: jspb.Message.getFieldWithDefault(msg, 4, ""),
    valign: jspb.Message.getFieldWithDefault(msg, 5, ""),
    width: jspb.Message.getFieldWithDefault(msg, 6, ""),
    height: jspb.Message.getFieldWithDefault(msg, 7, ""),
    positionx: jspb.Message.getFieldWithDefault(msg, 8, ""),
    positiony: jspb.Message.getFieldWithDefault(msg, 9, ""),
    ispointerblocker: jspb.Message.getBooleanFieldWithDefault(msg, 10, false),
    parent: (f = msg.getParent()) && proto.engineinterface.PB_UIShape.toObject(includeInstance, f),
    fontsize: jspb.Message.getFloatingPointFieldWithDefault(msg, 12, 0.0),
    fontweight: jspb.Message.getFieldWithDefault(msg, 13, ""),
    thickness: jspb.Message.getFloatingPointFieldWithDefault(msg, 14, 0.0),
    cornerradius: jspb.Message.getFloatingPointFieldWithDefault(msg, 15, 0.0),
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    background: (f = msg.getBackground()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    paddingtop: jspb.Message.getFloatingPointFieldWithDefault(msg, 18, 0.0),
    paddingright: jspb.Message.getFloatingPointFieldWithDefault(msg, 19, 0.0),
    paddingbottom: jspb.Message.getFloatingPointFieldWithDefault(msg, 20, 0.0),
    paddingleft: jspb.Message.getFloatingPointFieldWithDefault(msg, 21, 0.0),
    shadowblur: jspb.Message.getFloatingPointFieldWithDefault(msg, 22, 0.0),
    shadowoffsetx: jspb.Message.getFloatingPointFieldWithDefault(msg, 23, 0.0),
    shadowoffsety: jspb.Message.getFloatingPointFieldWithDefault(msg, 24, 0.0),
    shadowcolor: (f = msg.getShadowcolor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    text: jspb.Message.getFieldWithDefault(msg, 26, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UIButton}
 */
proto.engineinterface.PB_UIButton.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UIButton;
  return proto.engineinterface.PB_UIButton.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UIButton} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UIButton}
 */
proto.engineinterface.PB_UIButton.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 4:
      var value = /** @type {string} */ (reader.readString());
      msg.setHalign(value);
      break;
    case 5:
      var value = /** @type {string} */ (reader.readString());
      msg.setValign(value);
      break;
    case 6:
      var value = /** @type {string} */ (reader.readString());
      msg.setWidth(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setHeight(value);
      break;
    case 8:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositionx(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositiony(value);
      break;
    case 10:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIspointerblocker(value);
      break;
    case 11:
      var value = new proto.engineinterface.PB_UIShape;
      reader.readMessage(value,proto.engineinterface.PB_UIShape.deserializeBinaryFromReader);
      msg.setParent(value);
      break;
    case 12:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setFontsize(value);
      break;
    case 13:
      var value = /** @type {string} */ (reader.readString());
      msg.setFontweight(value);
      break;
    case 14:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setThickness(value);
      break;
    case 15:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setCornerradius(value);
      break;
    case 16:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    case 17:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setBackground(value);
      break;
    case 18:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingtop(value);
      break;
    case 19:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingright(value);
      break;
    case 20:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingbottom(value);
      break;
    case 21:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingleft(value);
      break;
    case 22:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowblur(value);
      break;
    case 23:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowoffsetx(value);
      break;
    case 24:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowoffsety(value);
      break;
    case 25:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setShadowcolor(value);
      break;
    case 26:
      var value = /** @type {string} */ (reader.readString());
      msg.setText(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UIButton.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UIButton.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UIButton} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIButton.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHalign();
  if (f.length > 0) {
    writer.writeString(
      4,
      f
    );
  }
  f = message.getValign();
  if (f.length > 0) {
    writer.writeString(
      5,
      f
    );
  }
  f = message.getWidth();
  if (f.length > 0) {
    writer.writeString(
      6,
      f
    );
  }
  f = message.getHeight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getPositionx();
  if (f.length > 0) {
    writer.writeString(
      8,
      f
    );
  }
  f = message.getPositiony();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getIspointerblocker();
  if (f) {
    writer.writeBool(
      10,
      f
    );
  }
  f = message.getParent();
  if (f != null) {
    writer.writeMessage(
      11,
      f,
      proto.engineinterface.PB_UIShape.serializeBinaryToWriter
    );
  }
  f = message.getFontsize();
  if (f !== 0.0) {
    writer.writeFloat(
      12,
      f
    );
  }
  f = message.getFontweight();
  if (f.length > 0) {
    writer.writeString(
      13,
      f
    );
  }
  f = message.getThickness();
  if (f !== 0.0) {
    writer.writeFloat(
      14,
      f
    );
  }
  f = message.getCornerradius();
  if (f !== 0.0) {
    writer.writeFloat(
      15,
      f
    );
  }
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      16,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getBackground();
  if (f != null) {
    writer.writeMessage(
      17,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getPaddingtop();
  if (f !== 0.0) {
    writer.writeFloat(
      18,
      f
    );
  }
  f = message.getPaddingright();
  if (f !== 0.0) {
    writer.writeFloat(
      19,
      f
    );
  }
  f = message.getPaddingbottom();
  if (f !== 0.0) {
    writer.writeFloat(
      20,
      f
    );
  }
  f = message.getPaddingleft();
  if (f !== 0.0) {
    writer.writeFloat(
      21,
      f
    );
  }
  f = message.getShadowblur();
  if (f !== 0.0) {
    writer.writeFloat(
      22,
      f
    );
  }
  f = message.getShadowoffsetx();
  if (f !== 0.0) {
    writer.writeFloat(
      23,
      f
    );
  }
  f = message.getShadowoffsety();
  if (f !== 0.0) {
    writer.writeFloat(
      24,
      f
    );
  }
  f = message.getShadowcolor();
  if (f != null) {
    writer.writeMessage(
      25,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getText();
  if (f.length > 0) {
    writer.writeString(
      26,
      f
    );
  }
};


/**
 * optional string name = 1;
 * @return {string}
 */
proto.engineinterface.PB_UIButton.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_UIButton.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float opacity = 3;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional string hAlign = 4;
 * @return {string}
 */
proto.engineinterface.PB_UIButton.prototype.getHalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 4, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setHalign = function(value) {
  return jspb.Message.setProto3StringField(this, 4, value);
};


/**
 * optional string vAlign = 5;
 * @return {string}
 */
proto.engineinterface.PB_UIButton.prototype.getValign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 5, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setValign = function(value) {
  return jspb.Message.setProto3StringField(this, 5, value);
};


/**
 * optional string width = 6;
 * @return {string}
 */
proto.engineinterface.PB_UIButton.prototype.getWidth = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 6, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setWidth = function(value) {
  return jspb.Message.setProto3StringField(this, 6, value);
};


/**
 * optional string height = 7;
 * @return {string}
 */
proto.engineinterface.PB_UIButton.prototype.getHeight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setHeight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional string positionX = 8;
 * @return {string}
 */
proto.engineinterface.PB_UIButton.prototype.getPositionx = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 8, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setPositionx = function(value) {
  return jspb.Message.setProto3StringField(this, 8, value);
};


/**
 * optional string positionY = 9;
 * @return {string}
 */
proto.engineinterface.PB_UIButton.prototype.getPositiony = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setPositiony = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional bool isPointerBlocker = 10;
 * @return {boolean}
 */
proto.engineinterface.PB_UIButton.prototype.getIspointerblocker = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 10, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setIspointerblocker = function(value) {
  return jspb.Message.setProto3BooleanField(this, 10, value);
};


/**
 * optional PB_UIShape parent = 11;
 * @return {?proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UIButton.prototype.getParent = function() {
  return /** @type{?proto.engineinterface.PB_UIShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIShape, 11));
};


/**
 * @param {?proto.engineinterface.PB_UIShape|undefined} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
*/
proto.engineinterface.PB_UIButton.prototype.setParent = function(value) {
  return jspb.Message.setWrapperField(this, 11, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.clearParent = function() {
  return this.setParent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIButton.prototype.hasParent = function() {
  return jspb.Message.getField(this, 11) != null;
};


/**
 * optional float fontSize = 12;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getFontsize = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 12, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setFontsize = function(value) {
  return jspb.Message.setProto3FloatField(this, 12, value);
};


/**
 * optional string fontWeight = 13;
 * @return {string}
 */
proto.engineinterface.PB_UIButton.prototype.getFontweight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 13, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setFontweight = function(value) {
  return jspb.Message.setProto3StringField(this, 13, value);
};


/**
 * optional float thickness = 14;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getThickness = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 14, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setThickness = function(value) {
  return jspb.Message.setProto3FloatField(this, 14, value);
};


/**
 * optional float cornerRadius = 15;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getCornerradius = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 15, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setCornerradius = function(value) {
  return jspb.Message.setProto3FloatField(this, 15, value);
};


/**
 * optional PB_Color4 color = 16;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIButton.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 16));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
*/
proto.engineinterface.PB_UIButton.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 16, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIButton.prototype.hasColor = function() {
  return jspb.Message.getField(this, 16) != null;
};


/**
 * optional PB_Color4 background = 17;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIButton.prototype.getBackground = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 17));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
*/
proto.engineinterface.PB_UIButton.prototype.setBackground = function(value) {
  return jspb.Message.setWrapperField(this, 17, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.clearBackground = function() {
  return this.setBackground(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIButton.prototype.hasBackground = function() {
  return jspb.Message.getField(this, 17) != null;
};


/**
 * optional float paddingTop = 18;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getPaddingtop = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 18, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setPaddingtop = function(value) {
  return jspb.Message.setProto3FloatField(this, 18, value);
};


/**
 * optional float paddingRight = 19;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getPaddingright = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 19, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setPaddingright = function(value) {
  return jspb.Message.setProto3FloatField(this, 19, value);
};


/**
 * optional float paddingBottom = 20;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getPaddingbottom = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 20, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setPaddingbottom = function(value) {
  return jspb.Message.setProto3FloatField(this, 20, value);
};


/**
 * optional float paddingLeft = 21;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getPaddingleft = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 21, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setPaddingleft = function(value) {
  return jspb.Message.setProto3FloatField(this, 21, value);
};


/**
 * optional float shadowBlur = 22;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getShadowblur = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 22, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setShadowblur = function(value) {
  return jspb.Message.setProto3FloatField(this, 22, value);
};


/**
 * optional float shadowOffsetX = 23;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getShadowoffsetx = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 23, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setShadowoffsetx = function(value) {
  return jspb.Message.setProto3FloatField(this, 23, value);
};


/**
 * optional float shadowOffsetY = 24;
 * @return {number}
 */
proto.engineinterface.PB_UIButton.prototype.getShadowoffsety = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 24, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setShadowoffsety = function(value) {
  return jspb.Message.setProto3FloatField(this, 24, value);
};


/**
 * optional PB_Color4 shadowColor = 25;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIButton.prototype.getShadowcolor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 25));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
*/
proto.engineinterface.PB_UIButton.prototype.setShadowcolor = function(value) {
  return jspb.Message.setWrapperField(this, 25, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.clearShadowcolor = function() {
  return this.setShadowcolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIButton.prototype.hasShadowcolor = function() {
  return jspb.Message.getField(this, 25) != null;
};


/**
 * optional string text = 26;
 * @return {string}
 */
proto.engineinterface.PB_UIButton.prototype.getText = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 26, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIButton} returns this
 */
proto.engineinterface.PB_UIButton.prototype.setText = function(value) {
  return jspb.Message.setProto3StringField(this, 26, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UICanvas.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UICanvas.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UICanvas} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UICanvas.toObject = function(includeInstance, msg) {
  var f, obj = {
    name: jspb.Message.getFieldWithDefault(msg, 1, ""),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    halign: jspb.Message.getFieldWithDefault(msg, 4, ""),
    valign: jspb.Message.getFieldWithDefault(msg, 5, ""),
    width: jspb.Message.getFieldWithDefault(msg, 6, ""),
    height: jspb.Message.getFieldWithDefault(msg, 7, ""),
    positionx: jspb.Message.getFieldWithDefault(msg, 8, ""),
    positiony: jspb.Message.getFieldWithDefault(msg, 9, ""),
    ispointerblocker: jspb.Message.getBooleanFieldWithDefault(msg, 10, false),
    parent: (f = msg.getParent()) && proto.engineinterface.PB_UIShape.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UICanvas}
 */
proto.engineinterface.PB_UICanvas.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UICanvas;
  return proto.engineinterface.PB_UICanvas.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UICanvas} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UICanvas}
 */
proto.engineinterface.PB_UICanvas.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 4:
      var value = /** @type {string} */ (reader.readString());
      msg.setHalign(value);
      break;
    case 5:
      var value = /** @type {string} */ (reader.readString());
      msg.setValign(value);
      break;
    case 6:
      var value = /** @type {string} */ (reader.readString());
      msg.setWidth(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setHeight(value);
      break;
    case 8:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositionx(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositiony(value);
      break;
    case 10:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIspointerblocker(value);
      break;
    case 11:
      var value = new proto.engineinterface.PB_UIShape;
      reader.readMessage(value,proto.engineinterface.PB_UIShape.deserializeBinaryFromReader);
      msg.setParent(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UICanvas.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UICanvas.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UICanvas} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UICanvas.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHalign();
  if (f.length > 0) {
    writer.writeString(
      4,
      f
    );
  }
  f = message.getValign();
  if (f.length > 0) {
    writer.writeString(
      5,
      f
    );
  }
  f = message.getWidth();
  if (f.length > 0) {
    writer.writeString(
      6,
      f
    );
  }
  f = message.getHeight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getPositionx();
  if (f.length > 0) {
    writer.writeString(
      8,
      f
    );
  }
  f = message.getPositiony();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getIspointerblocker();
  if (f) {
    writer.writeBool(
      10,
      f
    );
  }
  f = message.getParent();
  if (f != null) {
    writer.writeMessage(
      11,
      f,
      proto.engineinterface.PB_UIShape.serializeBinaryToWriter
    );
  }
};


/**
 * optional string name = 1;
 * @return {string}
 */
proto.engineinterface.PB_UICanvas.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_UICanvas.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float opacity = 3;
 * @return {number}
 */
proto.engineinterface.PB_UICanvas.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional string hAlign = 4;
 * @return {string}
 */
proto.engineinterface.PB_UICanvas.prototype.getHalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 4, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.setHalign = function(value) {
  return jspb.Message.setProto3StringField(this, 4, value);
};


/**
 * optional string vAlign = 5;
 * @return {string}
 */
proto.engineinterface.PB_UICanvas.prototype.getValign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 5, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.setValign = function(value) {
  return jspb.Message.setProto3StringField(this, 5, value);
};


/**
 * optional string width = 6;
 * @return {string}
 */
proto.engineinterface.PB_UICanvas.prototype.getWidth = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 6, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.setWidth = function(value) {
  return jspb.Message.setProto3StringField(this, 6, value);
};


/**
 * optional string height = 7;
 * @return {string}
 */
proto.engineinterface.PB_UICanvas.prototype.getHeight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.setHeight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional string positionX = 8;
 * @return {string}
 */
proto.engineinterface.PB_UICanvas.prototype.getPositionx = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 8, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.setPositionx = function(value) {
  return jspb.Message.setProto3StringField(this, 8, value);
};


/**
 * optional string positionY = 9;
 * @return {string}
 */
proto.engineinterface.PB_UICanvas.prototype.getPositiony = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.setPositiony = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional bool isPointerBlocker = 10;
 * @return {boolean}
 */
proto.engineinterface.PB_UICanvas.prototype.getIspointerblocker = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 10, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.setIspointerblocker = function(value) {
  return jspb.Message.setProto3BooleanField(this, 10, value);
};


/**
 * optional PB_UIShape parent = 11;
 * @return {?proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UICanvas.prototype.getParent = function() {
  return /** @type{?proto.engineinterface.PB_UIShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIShape, 11));
};


/**
 * @param {?proto.engineinterface.PB_UIShape|undefined} value
 * @return {!proto.engineinterface.PB_UICanvas} returns this
*/
proto.engineinterface.PB_UICanvas.prototype.setParent = function(value) {
  return jspb.Message.setWrapperField(this, 11, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UICanvas} returns this
 */
proto.engineinterface.PB_UICanvas.prototype.clearParent = function() {
  return this.setParent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UICanvas.prototype.hasParent = function() {
  return jspb.Message.getField(this, 11) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UIContainerRect.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UIContainerRect.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UIContainerRect} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIContainerRect.toObject = function(includeInstance, msg) {
  var f, obj = {
    name: jspb.Message.getFieldWithDefault(msg, 1, ""),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    halign: jspb.Message.getFieldWithDefault(msg, 4, ""),
    valign: jspb.Message.getFieldWithDefault(msg, 5, ""),
    width: jspb.Message.getFieldWithDefault(msg, 6, ""),
    height: jspb.Message.getFieldWithDefault(msg, 7, ""),
    positionx: jspb.Message.getFieldWithDefault(msg, 8, ""),
    positiony: jspb.Message.getFieldWithDefault(msg, 9, ""),
    ispointerblocker: jspb.Message.getBooleanFieldWithDefault(msg, 10, false),
    parent: (f = msg.getParent()) && proto.engineinterface.PB_UIShape.toObject(includeInstance, f),
    adaptwidth: jspb.Message.getBooleanFieldWithDefault(msg, 12, false),
    adaptheight: jspb.Message.getBooleanFieldWithDefault(msg, 13, false),
    thickness: jspb.Message.getFloatingPointFieldWithDefault(msg, 14, 0.0),
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    alignmentusessize: jspb.Message.getBooleanFieldWithDefault(msg, 16, false)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UIContainerRect}
 */
proto.engineinterface.PB_UIContainerRect.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UIContainerRect;
  return proto.engineinterface.PB_UIContainerRect.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UIContainerRect} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UIContainerRect}
 */
proto.engineinterface.PB_UIContainerRect.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 4:
      var value = /** @type {string} */ (reader.readString());
      msg.setHalign(value);
      break;
    case 5:
      var value = /** @type {string} */ (reader.readString());
      msg.setValign(value);
      break;
    case 6:
      var value = /** @type {string} */ (reader.readString());
      msg.setWidth(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setHeight(value);
      break;
    case 8:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositionx(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositiony(value);
      break;
    case 10:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIspointerblocker(value);
      break;
    case 11:
      var value = new proto.engineinterface.PB_UIShape;
      reader.readMessage(value,proto.engineinterface.PB_UIShape.deserializeBinaryFromReader);
      msg.setParent(value);
      break;
    case 12:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setAdaptwidth(value);
      break;
    case 13:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setAdaptheight(value);
      break;
    case 14:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setThickness(value);
      break;
    case 15:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    case 16:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setAlignmentusessize(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UIContainerRect.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UIContainerRect.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UIContainerRect} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIContainerRect.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHalign();
  if (f.length > 0) {
    writer.writeString(
      4,
      f
    );
  }
  f = message.getValign();
  if (f.length > 0) {
    writer.writeString(
      5,
      f
    );
  }
  f = message.getWidth();
  if (f.length > 0) {
    writer.writeString(
      6,
      f
    );
  }
  f = message.getHeight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getPositionx();
  if (f.length > 0) {
    writer.writeString(
      8,
      f
    );
  }
  f = message.getPositiony();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getIspointerblocker();
  if (f) {
    writer.writeBool(
      10,
      f
    );
  }
  f = message.getParent();
  if (f != null) {
    writer.writeMessage(
      11,
      f,
      proto.engineinterface.PB_UIShape.serializeBinaryToWriter
    );
  }
  f = message.getAdaptwidth();
  if (f) {
    writer.writeBool(
      12,
      f
    );
  }
  f = message.getAdaptheight();
  if (f) {
    writer.writeBool(
      13,
      f
    );
  }
  f = message.getThickness();
  if (f !== 0.0) {
    writer.writeFloat(
      14,
      f
    );
  }
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      15,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getAlignmentusessize();
  if (f) {
    writer.writeBool(
      16,
      f
    );
  }
};


/**
 * optional string name = 1;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float opacity = 3;
 * @return {number}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional string hAlign = 4;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getHalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 4, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setHalign = function(value) {
  return jspb.Message.setProto3StringField(this, 4, value);
};


/**
 * optional string vAlign = 5;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getValign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 5, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setValign = function(value) {
  return jspb.Message.setProto3StringField(this, 5, value);
};


/**
 * optional string width = 6;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getWidth = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 6, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setWidth = function(value) {
  return jspb.Message.setProto3StringField(this, 6, value);
};


/**
 * optional string height = 7;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getHeight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setHeight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional string positionX = 8;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getPositionx = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 8, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setPositionx = function(value) {
  return jspb.Message.setProto3StringField(this, 8, value);
};


/**
 * optional string positionY = 9;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getPositiony = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setPositiony = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional bool isPointerBlocker = 10;
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getIspointerblocker = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 10, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setIspointerblocker = function(value) {
  return jspb.Message.setProto3BooleanField(this, 10, value);
};


/**
 * optional PB_UIShape parent = 11;
 * @return {?proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getParent = function() {
  return /** @type{?proto.engineinterface.PB_UIShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIShape, 11));
};


/**
 * @param {?proto.engineinterface.PB_UIShape|undefined} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
*/
proto.engineinterface.PB_UIContainerRect.prototype.setParent = function(value) {
  return jspb.Message.setWrapperField(this, 11, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.clearParent = function() {
  return this.setParent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerRect.prototype.hasParent = function() {
  return jspb.Message.getField(this, 11) != null;
};


/**
 * optional bool adaptWidth = 12;
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getAdaptwidth = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 12, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setAdaptwidth = function(value) {
  return jspb.Message.setProto3BooleanField(this, 12, value);
};


/**
 * optional bool adaptHeight = 13;
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getAdaptheight = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 13, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setAdaptheight = function(value) {
  return jspb.Message.setProto3BooleanField(this, 13, value);
};


/**
 * optional float thickness = 14;
 * @return {number}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getThickness = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 14, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setThickness = function(value) {
  return jspb.Message.setProto3FloatField(this, 14, value);
};


/**
 * optional PB_Color4 color = 15;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 15));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
*/
proto.engineinterface.PB_UIContainerRect.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 15, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerRect.prototype.hasColor = function() {
  return jspb.Message.getField(this, 15) != null;
};


/**
 * optional bool alignmentUsesSize = 16;
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerRect.prototype.getAlignmentusessize = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 16, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIContainerRect} returns this
 */
proto.engineinterface.PB_UIContainerRect.prototype.setAlignmentusessize = function(value) {
  return jspb.Message.setProto3BooleanField(this, 16, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UIContainerStack.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UIContainerStack.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UIContainerStack} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIContainerStack.toObject = function(includeInstance, msg) {
  var f, obj = {
    name: jspb.Message.getFieldWithDefault(msg, 1, ""),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    halign: jspb.Message.getFieldWithDefault(msg, 4, ""),
    valign: jspb.Message.getFieldWithDefault(msg, 5, ""),
    width: jspb.Message.getFieldWithDefault(msg, 6, ""),
    height: jspb.Message.getFieldWithDefault(msg, 7, ""),
    positionx: jspb.Message.getFieldWithDefault(msg, 8, ""),
    positiony: jspb.Message.getFieldWithDefault(msg, 9, ""),
    ispointerblocker: jspb.Message.getBooleanFieldWithDefault(msg, 10, false),
    parent: (f = msg.getParent()) && proto.engineinterface.PB_UIShape.toObject(includeInstance, f),
    adaptwidth: jspb.Message.getBooleanFieldWithDefault(msg, 12, false),
    adaptheight: jspb.Message.getBooleanFieldWithDefault(msg, 13, false),
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    stackorientation: jspb.Message.getFieldWithDefault(msg, 15, 0),
    spacing: jspb.Message.getFloatingPointFieldWithDefault(msg, 16, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UIContainerStack}
 */
proto.engineinterface.PB_UIContainerStack.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UIContainerStack;
  return proto.engineinterface.PB_UIContainerStack.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UIContainerStack} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UIContainerStack}
 */
proto.engineinterface.PB_UIContainerStack.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 4:
      var value = /** @type {string} */ (reader.readString());
      msg.setHalign(value);
      break;
    case 5:
      var value = /** @type {string} */ (reader.readString());
      msg.setValign(value);
      break;
    case 6:
      var value = /** @type {string} */ (reader.readString());
      msg.setWidth(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setHeight(value);
      break;
    case 8:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositionx(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositiony(value);
      break;
    case 10:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIspointerblocker(value);
      break;
    case 11:
      var value = new proto.engineinterface.PB_UIShape;
      reader.readMessage(value,proto.engineinterface.PB_UIShape.deserializeBinaryFromReader);
      msg.setParent(value);
      break;
    case 12:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setAdaptwidth(value);
      break;
    case 13:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setAdaptheight(value);
      break;
    case 14:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    case 15:
      var value = /** @type {!proto.engineinterface.PB_UIStackOrientation} */ (reader.readEnum());
      msg.setStackorientation(value);
      break;
    case 16:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSpacing(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UIContainerStack.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UIContainerStack.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UIContainerStack} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIContainerStack.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHalign();
  if (f.length > 0) {
    writer.writeString(
      4,
      f
    );
  }
  f = message.getValign();
  if (f.length > 0) {
    writer.writeString(
      5,
      f
    );
  }
  f = message.getWidth();
  if (f.length > 0) {
    writer.writeString(
      6,
      f
    );
  }
  f = message.getHeight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getPositionx();
  if (f.length > 0) {
    writer.writeString(
      8,
      f
    );
  }
  f = message.getPositiony();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getIspointerblocker();
  if (f) {
    writer.writeBool(
      10,
      f
    );
  }
  f = message.getParent();
  if (f != null) {
    writer.writeMessage(
      11,
      f,
      proto.engineinterface.PB_UIShape.serializeBinaryToWriter
    );
  }
  f = message.getAdaptwidth();
  if (f) {
    writer.writeBool(
      12,
      f
    );
  }
  f = message.getAdaptheight();
  if (f) {
    writer.writeBool(
      13,
      f
    );
  }
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      14,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getStackorientation();
  if (f !== 0.0) {
    writer.writeEnum(
      15,
      f
    );
  }
  f = message.getSpacing();
  if (f !== 0.0) {
    writer.writeFloat(
      16,
      f
    );
  }
};


/**
 * optional string name = 1;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float opacity = 3;
 * @return {number}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional string hAlign = 4;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getHalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 4, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setHalign = function(value) {
  return jspb.Message.setProto3StringField(this, 4, value);
};


/**
 * optional string vAlign = 5;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getValign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 5, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setValign = function(value) {
  return jspb.Message.setProto3StringField(this, 5, value);
};


/**
 * optional string width = 6;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getWidth = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 6, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setWidth = function(value) {
  return jspb.Message.setProto3StringField(this, 6, value);
};


/**
 * optional string height = 7;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getHeight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setHeight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional string positionX = 8;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getPositionx = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 8, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setPositionx = function(value) {
  return jspb.Message.setProto3StringField(this, 8, value);
};


/**
 * optional string positionY = 9;
 * @return {string}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getPositiony = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setPositiony = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional bool isPointerBlocker = 10;
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getIspointerblocker = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 10, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setIspointerblocker = function(value) {
  return jspb.Message.setProto3BooleanField(this, 10, value);
};


/**
 * optional PB_UIShape parent = 11;
 * @return {?proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getParent = function() {
  return /** @type{?proto.engineinterface.PB_UIShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIShape, 11));
};


/**
 * @param {?proto.engineinterface.PB_UIShape|undefined} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
*/
proto.engineinterface.PB_UIContainerStack.prototype.setParent = function(value) {
  return jspb.Message.setWrapperField(this, 11, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.clearParent = function() {
  return this.setParent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerStack.prototype.hasParent = function() {
  return jspb.Message.getField(this, 11) != null;
};


/**
 * optional bool adaptWidth = 12;
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getAdaptwidth = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 12, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setAdaptwidth = function(value) {
  return jspb.Message.setProto3BooleanField(this, 12, value);
};


/**
 * optional bool adaptHeight = 13;
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getAdaptheight = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 13, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setAdaptheight = function(value) {
  return jspb.Message.setProto3BooleanField(this, 13, value);
};


/**
 * optional PB_Color4 color = 14;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 14));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
*/
proto.engineinterface.PB_UIContainerStack.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 14, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIContainerStack.prototype.hasColor = function() {
  return jspb.Message.getField(this, 14) != null;
};


/**
 * optional PB_UIStackOrientation stackOrientation = 15;
 * @return {!proto.engineinterface.PB_UIStackOrientation}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getStackorientation = function() {
  return /** @type {!proto.engineinterface.PB_UIStackOrientation} */ (jspb.Message.getFieldWithDefault(this, 15, 0));
};


/**
 * @param {!proto.engineinterface.PB_UIStackOrientation} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setStackorientation = function(value) {
  return jspb.Message.setProto3EnumField(this, 15, value);
};


/**
 * optional float spacing = 16;
 * @return {number}
 */
proto.engineinterface.PB_UIContainerStack.prototype.getSpacing = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 16, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIContainerStack} returns this
 */
proto.engineinterface.PB_UIContainerStack.prototype.setSpacing = function(value) {
  return jspb.Message.setProto3FloatField(this, 16, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UIImage.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UIImage.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UIImage} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIImage.toObject = function(includeInstance, msg) {
  var f, obj = {
    name: jspb.Message.getFieldWithDefault(msg, 1, ""),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    halign: jspb.Message.getFieldWithDefault(msg, 4, ""),
    valign: jspb.Message.getFieldWithDefault(msg, 5, ""),
    width: jspb.Message.getFieldWithDefault(msg, 6, ""),
    height: jspb.Message.getFieldWithDefault(msg, 7, ""),
    positionx: jspb.Message.getFieldWithDefault(msg, 8, ""),
    positiony: jspb.Message.getFieldWithDefault(msg, 9, ""),
    ispointerblocker: jspb.Message.getBooleanFieldWithDefault(msg, 10, false),
    parent: (f = msg.getParent()) && proto.engineinterface.PB_UIShape.toObject(includeInstance, f),
    sourceleft: jspb.Message.getFloatingPointFieldWithDefault(msg, 12, 0.0),
    sourcetop: jspb.Message.getFloatingPointFieldWithDefault(msg, 13, 0.0),
    sourcewidth: jspb.Message.getFloatingPointFieldWithDefault(msg, 14, 0.0),
    sourceheight: jspb.Message.getFloatingPointFieldWithDefault(msg, 15, 0.0),
    source: (f = msg.getSource()) && proto.engineinterface.PB_Texture.toObject(includeInstance, f),
    paddingtop: jspb.Message.getFloatingPointFieldWithDefault(msg, 17, 0.0),
    paddingright: jspb.Message.getFloatingPointFieldWithDefault(msg, 18, 0.0),
    paddingbottom: jspb.Message.getFloatingPointFieldWithDefault(msg, 19, 0.0),
    paddingleft: jspb.Message.getFloatingPointFieldWithDefault(msg, 20, 0.0),
    sizeinpixels: jspb.Message.getBooleanFieldWithDefault(msg, 21, false),
    onclick: (f = msg.getOnclick()) && proto.engineinterface.PB_UUIDCallback.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UIImage}
 */
proto.engineinterface.PB_UIImage.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UIImage;
  return proto.engineinterface.PB_UIImage.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UIImage} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UIImage}
 */
proto.engineinterface.PB_UIImage.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 4:
      var value = /** @type {string} */ (reader.readString());
      msg.setHalign(value);
      break;
    case 5:
      var value = /** @type {string} */ (reader.readString());
      msg.setValign(value);
      break;
    case 6:
      var value = /** @type {string} */ (reader.readString());
      msg.setWidth(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setHeight(value);
      break;
    case 8:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositionx(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositiony(value);
      break;
    case 10:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIspointerblocker(value);
      break;
    case 11:
      var value = new proto.engineinterface.PB_UIShape;
      reader.readMessage(value,proto.engineinterface.PB_UIShape.deserializeBinaryFromReader);
      msg.setParent(value);
      break;
    case 12:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSourceleft(value);
      break;
    case 13:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSourcetop(value);
      break;
    case 14:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSourcewidth(value);
      break;
    case 15:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setSourceheight(value);
      break;
    case 16:
      var value = new proto.engineinterface.PB_Texture;
      reader.readMessage(value,proto.engineinterface.PB_Texture.deserializeBinaryFromReader);
      msg.setSource(value);
      break;
    case 17:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingtop(value);
      break;
    case 18:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingright(value);
      break;
    case 19:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingbottom(value);
      break;
    case 20:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingleft(value);
      break;
    case 21:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setSizeinpixels(value);
      break;
    case 22:
      var value = new proto.engineinterface.PB_UUIDCallback;
      reader.readMessage(value,proto.engineinterface.PB_UUIDCallback.deserializeBinaryFromReader);
      msg.setOnclick(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UIImage.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UIImage.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UIImage} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIImage.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHalign();
  if (f.length > 0) {
    writer.writeString(
      4,
      f
    );
  }
  f = message.getValign();
  if (f.length > 0) {
    writer.writeString(
      5,
      f
    );
  }
  f = message.getWidth();
  if (f.length > 0) {
    writer.writeString(
      6,
      f
    );
  }
  f = message.getHeight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getPositionx();
  if (f.length > 0) {
    writer.writeString(
      8,
      f
    );
  }
  f = message.getPositiony();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getIspointerblocker();
  if (f) {
    writer.writeBool(
      10,
      f
    );
  }
  f = message.getParent();
  if (f != null) {
    writer.writeMessage(
      11,
      f,
      proto.engineinterface.PB_UIShape.serializeBinaryToWriter
    );
  }
  f = message.getSourceleft();
  if (f !== 0.0) {
    writer.writeFloat(
      12,
      f
    );
  }
  f = message.getSourcetop();
  if (f !== 0.0) {
    writer.writeFloat(
      13,
      f
    );
  }
  f = message.getSourcewidth();
  if (f !== 0.0) {
    writer.writeFloat(
      14,
      f
    );
  }
  f = message.getSourceheight();
  if (f !== 0.0) {
    writer.writeFloat(
      15,
      f
    );
  }
  f = message.getSource();
  if (f != null) {
    writer.writeMessage(
      16,
      f,
      proto.engineinterface.PB_Texture.serializeBinaryToWriter
    );
  }
  f = message.getPaddingtop();
  if (f !== 0.0) {
    writer.writeFloat(
      17,
      f
    );
  }
  f = message.getPaddingright();
  if (f !== 0.0) {
    writer.writeFloat(
      18,
      f
    );
  }
  f = message.getPaddingbottom();
  if (f !== 0.0) {
    writer.writeFloat(
      19,
      f
    );
  }
  f = message.getPaddingleft();
  if (f !== 0.0) {
    writer.writeFloat(
      20,
      f
    );
  }
  f = message.getSizeinpixels();
  if (f) {
    writer.writeBool(
      21,
      f
    );
  }
  f = message.getOnclick();
  if (f != null) {
    writer.writeMessage(
      22,
      f,
      proto.engineinterface.PB_UUIDCallback.serializeBinaryToWriter
    );
  }
};


/**
 * optional string name = 1;
 * @return {string}
 */
proto.engineinterface.PB_UIImage.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_UIImage.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float opacity = 3;
 * @return {number}
 */
proto.engineinterface.PB_UIImage.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional string hAlign = 4;
 * @return {string}
 */
proto.engineinterface.PB_UIImage.prototype.getHalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 4, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setHalign = function(value) {
  return jspb.Message.setProto3StringField(this, 4, value);
};


/**
 * optional string vAlign = 5;
 * @return {string}
 */
proto.engineinterface.PB_UIImage.prototype.getValign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 5, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setValign = function(value) {
  return jspb.Message.setProto3StringField(this, 5, value);
};


/**
 * optional string width = 6;
 * @return {string}
 */
proto.engineinterface.PB_UIImage.prototype.getWidth = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 6, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setWidth = function(value) {
  return jspb.Message.setProto3StringField(this, 6, value);
};


/**
 * optional string height = 7;
 * @return {string}
 */
proto.engineinterface.PB_UIImage.prototype.getHeight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setHeight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional string positionX = 8;
 * @return {string}
 */
proto.engineinterface.PB_UIImage.prototype.getPositionx = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 8, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setPositionx = function(value) {
  return jspb.Message.setProto3StringField(this, 8, value);
};


/**
 * optional string positionY = 9;
 * @return {string}
 */
proto.engineinterface.PB_UIImage.prototype.getPositiony = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setPositiony = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional bool isPointerBlocker = 10;
 * @return {boolean}
 */
proto.engineinterface.PB_UIImage.prototype.getIspointerblocker = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 10, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setIspointerblocker = function(value) {
  return jspb.Message.setProto3BooleanField(this, 10, value);
};


/**
 * optional PB_UIShape parent = 11;
 * @return {?proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UIImage.prototype.getParent = function() {
  return /** @type{?proto.engineinterface.PB_UIShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIShape, 11));
};


/**
 * @param {?proto.engineinterface.PB_UIShape|undefined} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
*/
proto.engineinterface.PB_UIImage.prototype.setParent = function(value) {
  return jspb.Message.setWrapperField(this, 11, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.clearParent = function() {
  return this.setParent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIImage.prototype.hasParent = function() {
  return jspb.Message.getField(this, 11) != null;
};


/**
 * optional float sourceLeft = 12;
 * @return {number}
 */
proto.engineinterface.PB_UIImage.prototype.getSourceleft = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 12, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setSourceleft = function(value) {
  return jspb.Message.setProto3FloatField(this, 12, value);
};


/**
 * optional float sourceTop = 13;
 * @return {number}
 */
proto.engineinterface.PB_UIImage.prototype.getSourcetop = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 13, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setSourcetop = function(value) {
  return jspb.Message.setProto3FloatField(this, 13, value);
};


/**
 * optional float sourceWidth = 14;
 * @return {number}
 */
proto.engineinterface.PB_UIImage.prototype.getSourcewidth = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 14, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setSourcewidth = function(value) {
  return jspb.Message.setProto3FloatField(this, 14, value);
};


/**
 * optional float sourceHeight = 15;
 * @return {number}
 */
proto.engineinterface.PB_UIImage.prototype.getSourceheight = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 15, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setSourceheight = function(value) {
  return jspb.Message.setProto3FloatField(this, 15, value);
};


/**
 * optional PB_Texture source = 16;
 * @return {?proto.engineinterface.PB_Texture}
 */
proto.engineinterface.PB_UIImage.prototype.getSource = function() {
  return /** @type{?proto.engineinterface.PB_Texture} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Texture, 16));
};


/**
 * @param {?proto.engineinterface.PB_Texture|undefined} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
*/
proto.engineinterface.PB_UIImage.prototype.setSource = function(value) {
  return jspb.Message.setWrapperField(this, 16, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.clearSource = function() {
  return this.setSource(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIImage.prototype.hasSource = function() {
  return jspb.Message.getField(this, 16) != null;
};


/**
 * optional float paddingTop = 17;
 * @return {number}
 */
proto.engineinterface.PB_UIImage.prototype.getPaddingtop = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 17, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setPaddingtop = function(value) {
  return jspb.Message.setProto3FloatField(this, 17, value);
};


/**
 * optional float paddingRight = 18;
 * @return {number}
 */
proto.engineinterface.PB_UIImage.prototype.getPaddingright = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 18, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setPaddingright = function(value) {
  return jspb.Message.setProto3FloatField(this, 18, value);
};


/**
 * optional float paddingBottom = 19;
 * @return {number}
 */
proto.engineinterface.PB_UIImage.prototype.getPaddingbottom = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 19, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setPaddingbottom = function(value) {
  return jspb.Message.setProto3FloatField(this, 19, value);
};


/**
 * optional float paddingLeft = 20;
 * @return {number}
 */
proto.engineinterface.PB_UIImage.prototype.getPaddingleft = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 20, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setPaddingleft = function(value) {
  return jspb.Message.setProto3FloatField(this, 20, value);
};


/**
 * optional bool sizeInPixels = 21;
 * @return {boolean}
 */
proto.engineinterface.PB_UIImage.prototype.getSizeinpixels = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 21, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.setSizeinpixels = function(value) {
  return jspb.Message.setProto3BooleanField(this, 21, value);
};


/**
 * optional PB_UUIDCallback onClick = 22;
 * @return {?proto.engineinterface.PB_UUIDCallback}
 */
proto.engineinterface.PB_UIImage.prototype.getOnclick = function() {
  return /** @type{?proto.engineinterface.PB_UUIDCallback} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UUIDCallback, 22));
};


/**
 * @param {?proto.engineinterface.PB_UUIDCallback|undefined} value
 * @return {!proto.engineinterface.PB_UIImage} returns this
*/
proto.engineinterface.PB_UIImage.prototype.setOnclick = function(value) {
  return jspb.Message.setWrapperField(this, 22, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIImage} returns this
 */
proto.engineinterface.PB_UIImage.prototype.clearOnclick = function() {
  return this.setOnclick(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIImage.prototype.hasOnclick = function() {
  return jspb.Message.getField(this, 22) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UUIDCallback.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UUIDCallback.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UUIDCallback} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UUIDCallback.toObject = function(includeInstance, msg) {
  var f, obj = {
    type: jspb.Message.getFieldWithDefault(msg, 1, ""),
    uuid: jspb.Message.getFieldWithDefault(msg, 2, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UUIDCallback}
 */
proto.engineinterface.PB_UUIDCallback.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UUIDCallback;
  return proto.engineinterface.PB_UUIDCallback.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UUIDCallback} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UUIDCallback}
 */
proto.engineinterface.PB_UUIDCallback.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setType(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setUuid(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UUIDCallback.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UUIDCallback.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UUIDCallback} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UUIDCallback.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getType();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getUuid();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
};


/**
 * optional string type = 1;
 * @return {string}
 */
proto.engineinterface.PB_UUIDCallback.prototype.getType = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UUIDCallback} returns this
 */
proto.engineinterface.PB_UUIDCallback.prototype.setType = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string uuid = 2;
 * @return {string}
 */
proto.engineinterface.PB_UUIDCallback.prototype.getUuid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UUIDCallback} returns this
 */
proto.engineinterface.PB_UUIDCallback.prototype.setUuid = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UIInputText.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UIInputText.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UIInputText} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIInputText.toObject = function(includeInstance, msg) {
  var f, obj = {
    name: jspb.Message.getFieldWithDefault(msg, 1, ""),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    halign: jspb.Message.getFieldWithDefault(msg, 4, ""),
    valign: jspb.Message.getFieldWithDefault(msg, 5, ""),
    width: jspb.Message.getFieldWithDefault(msg, 6, ""),
    height: jspb.Message.getFieldWithDefault(msg, 7, ""),
    positionx: jspb.Message.getFieldWithDefault(msg, 8, ""),
    positiony: jspb.Message.getFieldWithDefault(msg, 9, ""),
    ispointerblocker: jspb.Message.getBooleanFieldWithDefault(msg, 10, false),
    parent: (f = msg.getParent()) && proto.engineinterface.PB_UIShape.toObject(includeInstance, f),
    outlinewidth: jspb.Message.getFloatingPointFieldWithDefault(msg, 12, 0.0),
    outlinecolor: (f = msg.getOutlinecolor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    thickness: jspb.Message.getFloatingPointFieldWithDefault(msg, 15, 0.0),
    fontsize: jspb.Message.getFloatingPointFieldWithDefault(msg, 16, 0.0),
    fontweight: jspb.Message.getFieldWithDefault(msg, 17, ""),
    value: jspb.Message.getFieldWithDefault(msg, 18, ""),
    placeholdercolor: (f = msg.getPlaceholdercolor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    placeholder: jspb.Message.getFieldWithDefault(msg, 20, ""),
    margin: jspb.Message.getFloatingPointFieldWithDefault(msg, 21, 0.0),
    maxwidth: jspb.Message.getFloatingPointFieldWithDefault(msg, 22, 0.0),
    htextalign: jspb.Message.getFieldWithDefault(msg, 23, ""),
    vtextalign: jspb.Message.getFieldWithDefault(msg, 24, ""),
    autostretchwidth: jspb.Message.getBooleanFieldWithDefault(msg, 25, false),
    background: (f = msg.getBackground()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    focusedbackground: (f = msg.getFocusedbackground()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    textwrapping: jspb.Message.getBooleanFieldWithDefault(msg, 28, false),
    shadowblur: jspb.Message.getFloatingPointFieldWithDefault(msg, 29, 0.0),
    shadowoffsetx: jspb.Message.getFloatingPointFieldWithDefault(msg, 30, 0.0),
    shadowoffsety: jspb.Message.getFloatingPointFieldWithDefault(msg, 31, 0.0),
    shadowcolor: (f = msg.getShadowcolor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    paddingtop: jspb.Message.getFloatingPointFieldWithDefault(msg, 33, 0.0),
    paddingright: jspb.Message.getFloatingPointFieldWithDefault(msg, 34, 0.0),
    paddingbottom: jspb.Message.getFloatingPointFieldWithDefault(msg, 35, 0.0),
    paddingleft: jspb.Message.getFloatingPointFieldWithDefault(msg, 36, 0.0),
    ontextsubmit: (f = msg.getOntextsubmit()) && proto.engineinterface.PB_UUIDCallback.toObject(includeInstance, f),
    onchanged: (f = msg.getOnchanged()) && proto.engineinterface.PB_UUIDCallback.toObject(includeInstance, f),
    onfocus: (f = msg.getOnfocus()) && proto.engineinterface.PB_UUIDCallback.toObject(includeInstance, f),
    onblur: (f = msg.getOnblur()) && proto.engineinterface.PB_UUIDCallback.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UIInputText}
 */
proto.engineinterface.PB_UIInputText.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UIInputText;
  return proto.engineinterface.PB_UIInputText.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UIInputText} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UIInputText}
 */
proto.engineinterface.PB_UIInputText.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 4:
      var value = /** @type {string} */ (reader.readString());
      msg.setHalign(value);
      break;
    case 5:
      var value = /** @type {string} */ (reader.readString());
      msg.setValign(value);
      break;
    case 6:
      var value = /** @type {string} */ (reader.readString());
      msg.setWidth(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setHeight(value);
      break;
    case 8:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositionx(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositiony(value);
      break;
    case 10:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIspointerblocker(value);
      break;
    case 11:
      var value = new proto.engineinterface.PB_UIShape;
      reader.readMessage(value,proto.engineinterface.PB_UIShape.deserializeBinaryFromReader);
      msg.setParent(value);
      break;
    case 12:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOutlinewidth(value);
      break;
    case 13:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setOutlinecolor(value);
      break;
    case 14:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    case 15:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setThickness(value);
      break;
    case 16:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setFontsize(value);
      break;
    case 17:
      var value = /** @type {string} */ (reader.readString());
      msg.setFontweight(value);
      break;
    case 18:
      var value = /** @type {string} */ (reader.readString());
      msg.setValue(value);
      break;
    case 19:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setPlaceholdercolor(value);
      break;
    case 20:
      var value = /** @type {string} */ (reader.readString());
      msg.setPlaceholder(value);
      break;
    case 21:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setMargin(value);
      break;
    case 22:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setMaxwidth(value);
      break;
    case 23:
      var value = /** @type {string} */ (reader.readString());
      msg.setHtextalign(value);
      break;
    case 24:
      var value = /** @type {string} */ (reader.readString());
      msg.setVtextalign(value);
      break;
    case 25:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setAutostretchwidth(value);
      break;
    case 26:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setBackground(value);
      break;
    case 27:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setFocusedbackground(value);
      break;
    case 28:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setTextwrapping(value);
      break;
    case 29:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowblur(value);
      break;
    case 30:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowoffsetx(value);
      break;
    case 31:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowoffsety(value);
      break;
    case 32:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setShadowcolor(value);
      break;
    case 33:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingtop(value);
      break;
    case 34:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingright(value);
      break;
    case 35:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingbottom(value);
      break;
    case 36:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingleft(value);
      break;
    case 37:
      var value = new proto.engineinterface.PB_UUIDCallback;
      reader.readMessage(value,proto.engineinterface.PB_UUIDCallback.deserializeBinaryFromReader);
      msg.setOntextsubmit(value);
      break;
    case 38:
      var value = new proto.engineinterface.PB_UUIDCallback;
      reader.readMessage(value,proto.engineinterface.PB_UUIDCallback.deserializeBinaryFromReader);
      msg.setOnchanged(value);
      break;
    case 39:
      var value = new proto.engineinterface.PB_UUIDCallback;
      reader.readMessage(value,proto.engineinterface.PB_UUIDCallback.deserializeBinaryFromReader);
      msg.setOnfocus(value);
      break;
    case 40:
      var value = new proto.engineinterface.PB_UUIDCallback;
      reader.readMessage(value,proto.engineinterface.PB_UUIDCallback.deserializeBinaryFromReader);
      msg.setOnblur(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UIInputText.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UIInputText.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UIInputText} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIInputText.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHalign();
  if (f.length > 0) {
    writer.writeString(
      4,
      f
    );
  }
  f = message.getValign();
  if (f.length > 0) {
    writer.writeString(
      5,
      f
    );
  }
  f = message.getWidth();
  if (f.length > 0) {
    writer.writeString(
      6,
      f
    );
  }
  f = message.getHeight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getPositionx();
  if (f.length > 0) {
    writer.writeString(
      8,
      f
    );
  }
  f = message.getPositiony();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getIspointerblocker();
  if (f) {
    writer.writeBool(
      10,
      f
    );
  }
  f = message.getParent();
  if (f != null) {
    writer.writeMessage(
      11,
      f,
      proto.engineinterface.PB_UIShape.serializeBinaryToWriter
    );
  }
  f = message.getOutlinewidth();
  if (f !== 0.0) {
    writer.writeFloat(
      12,
      f
    );
  }
  f = message.getOutlinecolor();
  if (f != null) {
    writer.writeMessage(
      13,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      14,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getThickness();
  if (f !== 0.0) {
    writer.writeFloat(
      15,
      f
    );
  }
  f = message.getFontsize();
  if (f !== 0.0) {
    writer.writeFloat(
      16,
      f
    );
  }
  f = message.getFontweight();
  if (f.length > 0) {
    writer.writeString(
      17,
      f
    );
  }
  f = message.getValue();
  if (f.length > 0) {
    writer.writeString(
      18,
      f
    );
  }
  f = message.getPlaceholdercolor();
  if (f != null) {
    writer.writeMessage(
      19,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getPlaceholder();
  if (f.length > 0) {
    writer.writeString(
      20,
      f
    );
  }
  f = message.getMargin();
  if (f !== 0.0) {
    writer.writeFloat(
      21,
      f
    );
  }
  f = message.getMaxwidth();
  if (f !== 0.0) {
    writer.writeFloat(
      22,
      f
    );
  }
  f = message.getHtextalign();
  if (f.length > 0) {
    writer.writeString(
      23,
      f
    );
  }
  f = message.getVtextalign();
  if (f.length > 0) {
    writer.writeString(
      24,
      f
    );
  }
  f = message.getAutostretchwidth();
  if (f) {
    writer.writeBool(
      25,
      f
    );
  }
  f = message.getBackground();
  if (f != null) {
    writer.writeMessage(
      26,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getFocusedbackground();
  if (f != null) {
    writer.writeMessage(
      27,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getTextwrapping();
  if (f) {
    writer.writeBool(
      28,
      f
    );
  }
  f = message.getShadowblur();
  if (f !== 0.0) {
    writer.writeFloat(
      29,
      f
    );
  }
  f = message.getShadowoffsetx();
  if (f !== 0.0) {
    writer.writeFloat(
      30,
      f
    );
  }
  f = message.getShadowoffsety();
  if (f !== 0.0) {
    writer.writeFloat(
      31,
      f
    );
  }
  f = message.getShadowcolor();
  if (f != null) {
    writer.writeMessage(
      32,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getPaddingtop();
  if (f !== 0.0) {
    writer.writeFloat(
      33,
      f
    );
  }
  f = message.getPaddingright();
  if (f !== 0.0) {
    writer.writeFloat(
      34,
      f
    );
  }
  f = message.getPaddingbottom();
  if (f !== 0.0) {
    writer.writeFloat(
      35,
      f
    );
  }
  f = message.getPaddingleft();
  if (f !== 0.0) {
    writer.writeFloat(
      36,
      f
    );
  }
  f = message.getOntextsubmit();
  if (f != null) {
    writer.writeMessage(
      37,
      f,
      proto.engineinterface.PB_UUIDCallback.serializeBinaryToWriter
    );
  }
  f = message.getOnchanged();
  if (f != null) {
    writer.writeMessage(
      38,
      f,
      proto.engineinterface.PB_UUIDCallback.serializeBinaryToWriter
    );
  }
  f = message.getOnfocus();
  if (f != null) {
    writer.writeMessage(
      39,
      f,
      proto.engineinterface.PB_UUIDCallback.serializeBinaryToWriter
    );
  }
  f = message.getOnblur();
  if (f != null) {
    writer.writeMessage(
      40,
      f,
      proto.engineinterface.PB_UUIDCallback.serializeBinaryToWriter
    );
  }
};


/**
 * optional string name = 1;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float opacity = 3;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional string hAlign = 4;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getHalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 4, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setHalign = function(value) {
  return jspb.Message.setProto3StringField(this, 4, value);
};


/**
 * optional string vAlign = 5;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getValign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 5, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setValign = function(value) {
  return jspb.Message.setProto3StringField(this, 5, value);
};


/**
 * optional string width = 6;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getWidth = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 6, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setWidth = function(value) {
  return jspb.Message.setProto3StringField(this, 6, value);
};


/**
 * optional string height = 7;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getHeight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setHeight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional string positionX = 8;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getPositionx = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 8, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setPositionx = function(value) {
  return jspb.Message.setProto3StringField(this, 8, value);
};


/**
 * optional string positionY = 9;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getPositiony = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setPositiony = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional bool isPointerBlocker = 10;
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.getIspointerblocker = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 10, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setIspointerblocker = function(value) {
  return jspb.Message.setProto3BooleanField(this, 10, value);
};


/**
 * optional PB_UIShape parent = 11;
 * @return {?proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UIInputText.prototype.getParent = function() {
  return /** @type{?proto.engineinterface.PB_UIShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIShape, 11));
};


/**
 * @param {?proto.engineinterface.PB_UIShape|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setParent = function(value) {
  return jspb.Message.setWrapperField(this, 11, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearParent = function() {
  return this.setParent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasParent = function() {
  return jspb.Message.getField(this, 11) != null;
};


/**
 * optional float outlineWidth = 12;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getOutlinewidth = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 12, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setOutlinewidth = function(value) {
  return jspb.Message.setProto3FloatField(this, 12, value);
};


/**
 * optional PB_Color4 outlineColor = 13;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIInputText.prototype.getOutlinecolor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 13));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setOutlinecolor = function(value) {
  return jspb.Message.setWrapperField(this, 13, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearOutlinecolor = function() {
  return this.setOutlinecolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasOutlinecolor = function() {
  return jspb.Message.getField(this, 13) != null;
};


/**
 * optional PB_Color4 color = 14;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIInputText.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 14));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 14, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasColor = function() {
  return jspb.Message.getField(this, 14) != null;
};


/**
 * optional float thickness = 15;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getThickness = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 15, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setThickness = function(value) {
  return jspb.Message.setProto3FloatField(this, 15, value);
};


/**
 * optional float fontSize = 16;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getFontsize = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 16, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setFontsize = function(value) {
  return jspb.Message.setProto3FloatField(this, 16, value);
};


/**
 * optional string fontWeight = 17;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getFontweight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 17, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setFontweight = function(value) {
  return jspb.Message.setProto3StringField(this, 17, value);
};


/**
 * optional string value = 18;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getValue = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 18, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setValue = function(value) {
  return jspb.Message.setProto3StringField(this, 18, value);
};


/**
 * optional PB_Color4 placeholderColor = 19;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIInputText.prototype.getPlaceholdercolor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 19));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setPlaceholdercolor = function(value) {
  return jspb.Message.setWrapperField(this, 19, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearPlaceholdercolor = function() {
  return this.setPlaceholdercolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasPlaceholdercolor = function() {
  return jspb.Message.getField(this, 19) != null;
};


/**
 * optional string placeholder = 20;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getPlaceholder = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 20, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setPlaceholder = function(value) {
  return jspb.Message.setProto3StringField(this, 20, value);
};


/**
 * optional float margin = 21;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getMargin = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 21, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setMargin = function(value) {
  return jspb.Message.setProto3FloatField(this, 21, value);
};


/**
 * optional float maxWidth = 22;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getMaxwidth = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 22, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setMaxwidth = function(value) {
  return jspb.Message.setProto3FloatField(this, 22, value);
};


/**
 * optional string hTextAlign = 23;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getHtextalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 23, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setHtextalign = function(value) {
  return jspb.Message.setProto3StringField(this, 23, value);
};


/**
 * optional string vTextAlign = 24;
 * @return {string}
 */
proto.engineinterface.PB_UIInputText.prototype.getVtextalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 24, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setVtextalign = function(value) {
  return jspb.Message.setProto3StringField(this, 24, value);
};


/**
 * optional bool autoStretchWidth = 25;
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.getAutostretchwidth = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 25, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setAutostretchwidth = function(value) {
  return jspb.Message.setProto3BooleanField(this, 25, value);
};


/**
 * optional PB_Color4 background = 26;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIInputText.prototype.getBackground = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 26));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setBackground = function(value) {
  return jspb.Message.setWrapperField(this, 26, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearBackground = function() {
  return this.setBackground(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasBackground = function() {
  return jspb.Message.getField(this, 26) != null;
};


/**
 * optional PB_Color4 focusedBackground = 27;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIInputText.prototype.getFocusedbackground = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 27));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setFocusedbackground = function(value) {
  return jspb.Message.setWrapperField(this, 27, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearFocusedbackground = function() {
  return this.setFocusedbackground(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasFocusedbackground = function() {
  return jspb.Message.getField(this, 27) != null;
};


/**
 * optional bool textWrapping = 28;
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.getTextwrapping = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 28, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setTextwrapping = function(value) {
  return jspb.Message.setProto3BooleanField(this, 28, value);
};


/**
 * optional float shadowBlur = 29;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getShadowblur = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 29, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setShadowblur = function(value) {
  return jspb.Message.setProto3FloatField(this, 29, value);
};


/**
 * optional float shadowOffsetX = 30;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getShadowoffsetx = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 30, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setShadowoffsetx = function(value) {
  return jspb.Message.setProto3FloatField(this, 30, value);
};


/**
 * optional float shadowOffsetY = 31;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getShadowoffsety = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 31, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setShadowoffsety = function(value) {
  return jspb.Message.setProto3FloatField(this, 31, value);
};


/**
 * optional PB_Color4 shadowColor = 32;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIInputText.prototype.getShadowcolor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 32));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setShadowcolor = function(value) {
  return jspb.Message.setWrapperField(this, 32, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearShadowcolor = function() {
  return this.setShadowcolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasShadowcolor = function() {
  return jspb.Message.getField(this, 32) != null;
};


/**
 * optional float paddingTop = 33;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getPaddingtop = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 33, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setPaddingtop = function(value) {
  return jspb.Message.setProto3FloatField(this, 33, value);
};


/**
 * optional float paddingRight = 34;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getPaddingright = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 34, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setPaddingright = function(value) {
  return jspb.Message.setProto3FloatField(this, 34, value);
};


/**
 * optional float paddingBottom = 35;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getPaddingbottom = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 35, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setPaddingbottom = function(value) {
  return jspb.Message.setProto3FloatField(this, 35, value);
};


/**
 * optional float paddingLeft = 36;
 * @return {number}
 */
proto.engineinterface.PB_UIInputText.prototype.getPaddingleft = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 36, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.setPaddingleft = function(value) {
  return jspb.Message.setProto3FloatField(this, 36, value);
};


/**
 * optional PB_UUIDCallback onTextSubmit = 37;
 * @return {?proto.engineinterface.PB_UUIDCallback}
 */
proto.engineinterface.PB_UIInputText.prototype.getOntextsubmit = function() {
  return /** @type{?proto.engineinterface.PB_UUIDCallback} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UUIDCallback, 37));
};


/**
 * @param {?proto.engineinterface.PB_UUIDCallback|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setOntextsubmit = function(value) {
  return jspb.Message.setWrapperField(this, 37, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearOntextsubmit = function() {
  return this.setOntextsubmit(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasOntextsubmit = function() {
  return jspb.Message.getField(this, 37) != null;
};


/**
 * optional PB_UUIDCallback onChanged = 38;
 * @return {?proto.engineinterface.PB_UUIDCallback}
 */
proto.engineinterface.PB_UIInputText.prototype.getOnchanged = function() {
  return /** @type{?proto.engineinterface.PB_UUIDCallback} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UUIDCallback, 38));
};


/**
 * @param {?proto.engineinterface.PB_UUIDCallback|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setOnchanged = function(value) {
  return jspb.Message.setWrapperField(this, 38, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearOnchanged = function() {
  return this.setOnchanged(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasOnchanged = function() {
  return jspb.Message.getField(this, 38) != null;
};


/**
 * optional PB_UUIDCallback onFocus = 39;
 * @return {?proto.engineinterface.PB_UUIDCallback}
 */
proto.engineinterface.PB_UIInputText.prototype.getOnfocus = function() {
  return /** @type{?proto.engineinterface.PB_UUIDCallback} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UUIDCallback, 39));
};


/**
 * @param {?proto.engineinterface.PB_UUIDCallback|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setOnfocus = function(value) {
  return jspb.Message.setWrapperField(this, 39, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearOnfocus = function() {
  return this.setOnfocus(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasOnfocus = function() {
  return jspb.Message.getField(this, 39) != null;
};


/**
 * optional PB_UUIDCallback onBlur = 40;
 * @return {?proto.engineinterface.PB_UUIDCallback}
 */
proto.engineinterface.PB_UIInputText.prototype.getOnblur = function() {
  return /** @type{?proto.engineinterface.PB_UUIDCallback} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UUIDCallback, 40));
};


/**
 * @param {?proto.engineinterface.PB_UUIDCallback|undefined} value
 * @return {!proto.engineinterface.PB_UIInputText} returns this
*/
proto.engineinterface.PB_UIInputText.prototype.setOnblur = function(value) {
  return jspb.Message.setWrapperField(this, 40, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIInputText} returns this
 */
proto.engineinterface.PB_UIInputText.prototype.clearOnblur = function() {
  return this.setOnblur(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIInputText.prototype.hasOnblur = function() {
  return jspb.Message.getField(this, 40) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UIScrollRect.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UIScrollRect.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UIScrollRect} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIScrollRect.toObject = function(includeInstance, msg) {
  var f, obj = {
    name: jspb.Message.getFieldWithDefault(msg, 1, ""),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    halign: jspb.Message.getFieldWithDefault(msg, 4, ""),
    valign: jspb.Message.getFieldWithDefault(msg, 5, ""),
    width: jspb.Message.getFieldWithDefault(msg, 6, ""),
    height: jspb.Message.getFieldWithDefault(msg, 7, ""),
    positionx: jspb.Message.getFieldWithDefault(msg, 8, ""),
    positiony: jspb.Message.getFieldWithDefault(msg, 9, ""),
    ispointerblocker: jspb.Message.getBooleanFieldWithDefault(msg, 10, false),
    parent: (f = msg.getParent()) && proto.engineinterface.PB_UIShape.toObject(includeInstance, f),
    valuex: jspb.Message.getFloatingPointFieldWithDefault(msg, 12, 0.0),
    valuey: jspb.Message.getFloatingPointFieldWithDefault(msg, 13, 0.0),
    bordercolor: (f = msg.getBordercolor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    backgroundcolor: (f = msg.getBackgroundcolor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    ishorizontal: jspb.Message.getBooleanFieldWithDefault(msg, 16, false),
    isvertical: jspb.Message.getBooleanFieldWithDefault(msg, 17, false),
    paddingtop: jspb.Message.getFloatingPointFieldWithDefault(msg, 18, 0.0),
    paddingright: jspb.Message.getFloatingPointFieldWithDefault(msg, 19, 0.0),
    paddingbottom: jspb.Message.getFloatingPointFieldWithDefault(msg, 20, 0.0),
    paddingleft: jspb.Message.getFloatingPointFieldWithDefault(msg, 21, 0.0),
    onchanged: (f = msg.getOnchanged()) && proto.engineinterface.PB_UUIDCallback.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UIScrollRect}
 */
proto.engineinterface.PB_UIScrollRect.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UIScrollRect;
  return proto.engineinterface.PB_UIScrollRect.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UIScrollRect} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UIScrollRect}
 */
proto.engineinterface.PB_UIScrollRect.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 4:
      var value = /** @type {string} */ (reader.readString());
      msg.setHalign(value);
      break;
    case 5:
      var value = /** @type {string} */ (reader.readString());
      msg.setValign(value);
      break;
    case 6:
      var value = /** @type {string} */ (reader.readString());
      msg.setWidth(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setHeight(value);
      break;
    case 8:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositionx(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositiony(value);
      break;
    case 10:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIspointerblocker(value);
      break;
    case 11:
      var value = new proto.engineinterface.PB_UIShape;
      reader.readMessage(value,proto.engineinterface.PB_UIShape.deserializeBinaryFromReader);
      msg.setParent(value);
      break;
    case 12:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setValuex(value);
      break;
    case 13:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setValuey(value);
      break;
    case 14:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setBordercolor(value);
      break;
    case 15:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setBackgroundcolor(value);
      break;
    case 16:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIshorizontal(value);
      break;
    case 17:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIsvertical(value);
      break;
    case 18:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingtop(value);
      break;
    case 19:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingright(value);
      break;
    case 20:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingbottom(value);
      break;
    case 21:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingleft(value);
      break;
    case 22:
      var value = new proto.engineinterface.PB_UUIDCallback;
      reader.readMessage(value,proto.engineinterface.PB_UUIDCallback.deserializeBinaryFromReader);
      msg.setOnchanged(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UIScrollRect.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UIScrollRect.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UIScrollRect} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIScrollRect.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHalign();
  if (f.length > 0) {
    writer.writeString(
      4,
      f
    );
  }
  f = message.getValign();
  if (f.length > 0) {
    writer.writeString(
      5,
      f
    );
  }
  f = message.getWidth();
  if (f.length > 0) {
    writer.writeString(
      6,
      f
    );
  }
  f = message.getHeight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getPositionx();
  if (f.length > 0) {
    writer.writeString(
      8,
      f
    );
  }
  f = message.getPositiony();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getIspointerblocker();
  if (f) {
    writer.writeBool(
      10,
      f
    );
  }
  f = message.getParent();
  if (f != null) {
    writer.writeMessage(
      11,
      f,
      proto.engineinterface.PB_UIShape.serializeBinaryToWriter
    );
  }
  f = message.getValuex();
  if (f !== 0.0) {
    writer.writeFloat(
      12,
      f
    );
  }
  f = message.getValuey();
  if (f !== 0.0) {
    writer.writeFloat(
      13,
      f
    );
  }
  f = message.getBordercolor();
  if (f != null) {
    writer.writeMessage(
      14,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getBackgroundcolor();
  if (f != null) {
    writer.writeMessage(
      15,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getIshorizontal();
  if (f) {
    writer.writeBool(
      16,
      f
    );
  }
  f = message.getIsvertical();
  if (f) {
    writer.writeBool(
      17,
      f
    );
  }
  f = message.getPaddingtop();
  if (f !== 0.0) {
    writer.writeFloat(
      18,
      f
    );
  }
  f = message.getPaddingright();
  if (f !== 0.0) {
    writer.writeFloat(
      19,
      f
    );
  }
  f = message.getPaddingbottom();
  if (f !== 0.0) {
    writer.writeFloat(
      20,
      f
    );
  }
  f = message.getPaddingleft();
  if (f !== 0.0) {
    writer.writeFloat(
      21,
      f
    );
  }
  f = message.getOnchanged();
  if (f != null) {
    writer.writeMessage(
      22,
      f,
      proto.engineinterface.PB_UUIDCallback.serializeBinaryToWriter
    );
  }
};


/**
 * optional string name = 1;
 * @return {string}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float opacity = 3;
 * @return {number}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional string hAlign = 4;
 * @return {string}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getHalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 4, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setHalign = function(value) {
  return jspb.Message.setProto3StringField(this, 4, value);
};


/**
 * optional string vAlign = 5;
 * @return {string}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getValign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 5, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setValign = function(value) {
  return jspb.Message.setProto3StringField(this, 5, value);
};


/**
 * optional string width = 6;
 * @return {string}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getWidth = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 6, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setWidth = function(value) {
  return jspb.Message.setProto3StringField(this, 6, value);
};


/**
 * optional string height = 7;
 * @return {string}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getHeight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setHeight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional string positionX = 8;
 * @return {string}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getPositionx = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 8, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setPositionx = function(value) {
  return jspb.Message.setProto3StringField(this, 8, value);
};


/**
 * optional string positionY = 9;
 * @return {string}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getPositiony = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setPositiony = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional bool isPointerBlocker = 10;
 * @return {boolean}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getIspointerblocker = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 10, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setIspointerblocker = function(value) {
  return jspb.Message.setProto3BooleanField(this, 10, value);
};


/**
 * optional PB_UIShape parent = 11;
 * @return {?proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getParent = function() {
  return /** @type{?proto.engineinterface.PB_UIShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIShape, 11));
};


/**
 * @param {?proto.engineinterface.PB_UIShape|undefined} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
*/
proto.engineinterface.PB_UIScrollRect.prototype.setParent = function(value) {
  return jspb.Message.setWrapperField(this, 11, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.clearParent = function() {
  return this.setParent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIScrollRect.prototype.hasParent = function() {
  return jspb.Message.getField(this, 11) != null;
};


/**
 * optional float valueX = 12;
 * @return {number}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getValuex = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 12, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setValuex = function(value) {
  return jspb.Message.setProto3FloatField(this, 12, value);
};


/**
 * optional float valueY = 13;
 * @return {number}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getValuey = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 13, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setValuey = function(value) {
  return jspb.Message.setProto3FloatField(this, 13, value);
};


/**
 * optional PB_Color4 borderColor = 14;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getBordercolor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 14));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
*/
proto.engineinterface.PB_UIScrollRect.prototype.setBordercolor = function(value) {
  return jspb.Message.setWrapperField(this, 14, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.clearBordercolor = function() {
  return this.setBordercolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIScrollRect.prototype.hasBordercolor = function() {
  return jspb.Message.getField(this, 14) != null;
};


/**
 * optional PB_Color4 backgroundColor = 15;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getBackgroundcolor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 15));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
*/
proto.engineinterface.PB_UIScrollRect.prototype.setBackgroundcolor = function(value) {
  return jspb.Message.setWrapperField(this, 15, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.clearBackgroundcolor = function() {
  return this.setBackgroundcolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIScrollRect.prototype.hasBackgroundcolor = function() {
  return jspb.Message.getField(this, 15) != null;
};


/**
 * optional bool isHorizontal = 16;
 * @return {boolean}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getIshorizontal = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 16, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setIshorizontal = function(value) {
  return jspb.Message.setProto3BooleanField(this, 16, value);
};


/**
 * optional bool isVertical = 17;
 * @return {boolean}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getIsvertical = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 17, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setIsvertical = function(value) {
  return jspb.Message.setProto3BooleanField(this, 17, value);
};


/**
 * optional float paddingTop = 18;
 * @return {number}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getPaddingtop = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 18, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setPaddingtop = function(value) {
  return jspb.Message.setProto3FloatField(this, 18, value);
};


/**
 * optional float paddingRight = 19;
 * @return {number}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getPaddingright = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 19, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setPaddingright = function(value) {
  return jspb.Message.setProto3FloatField(this, 19, value);
};


/**
 * optional float paddingBottom = 20;
 * @return {number}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getPaddingbottom = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 20, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setPaddingbottom = function(value) {
  return jspb.Message.setProto3FloatField(this, 20, value);
};


/**
 * optional float paddingLeft = 21;
 * @return {number}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getPaddingleft = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 21, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.setPaddingleft = function(value) {
  return jspb.Message.setProto3FloatField(this, 21, value);
};


/**
 * optional PB_UUIDCallback onChanged = 22;
 * @return {?proto.engineinterface.PB_UUIDCallback}
 */
proto.engineinterface.PB_UIScrollRect.prototype.getOnchanged = function() {
  return /** @type{?proto.engineinterface.PB_UUIDCallback} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UUIDCallback, 22));
};


/**
 * @param {?proto.engineinterface.PB_UUIDCallback|undefined} value
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
*/
proto.engineinterface.PB_UIScrollRect.prototype.setOnchanged = function(value) {
  return jspb.Message.setWrapperField(this, 22, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIScrollRect} returns this
 */
proto.engineinterface.PB_UIScrollRect.prototype.clearOnchanged = function() {
  return this.setOnchanged(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIScrollRect.prototype.hasOnchanged = function() {
  return jspb.Message.getField(this, 22) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UIShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UIShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UIShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    name: jspb.Message.getFieldWithDefault(msg, 1, ""),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    halign: jspb.Message.getFieldWithDefault(msg, 4, ""),
    valign: jspb.Message.getFieldWithDefault(msg, 5, ""),
    width: jspb.Message.getFieldWithDefault(msg, 6, ""),
    height: jspb.Message.getFieldWithDefault(msg, 7, ""),
    positionx: jspb.Message.getFieldWithDefault(msg, 8, ""),
    positiony: jspb.Message.getFieldWithDefault(msg, 9, ""),
    ispointerblocker: jspb.Message.getBooleanFieldWithDefault(msg, 10, false),
    parent: (f = msg.getParent()) && proto.engineinterface.PB_UIShape.toObject(includeInstance, f)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UIShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UIShape;
  return proto.engineinterface.PB_UIShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UIShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UIShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 4:
      var value = /** @type {string} */ (reader.readString());
      msg.setHalign(value);
      break;
    case 5:
      var value = /** @type {string} */ (reader.readString());
      msg.setValign(value);
      break;
    case 6:
      var value = /** @type {string} */ (reader.readString());
      msg.setWidth(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setHeight(value);
      break;
    case 8:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositionx(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositiony(value);
      break;
    case 10:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIspointerblocker(value);
      break;
    case 11:
      var value = new proto.engineinterface.PB_UIShape;
      reader.readMessage(value,proto.engineinterface.PB_UIShape.deserializeBinaryFromReader);
      msg.setParent(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UIShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UIShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UIShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UIShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHalign();
  if (f.length > 0) {
    writer.writeString(
      4,
      f
    );
  }
  f = message.getValign();
  if (f.length > 0) {
    writer.writeString(
      5,
      f
    );
  }
  f = message.getWidth();
  if (f.length > 0) {
    writer.writeString(
      6,
      f
    );
  }
  f = message.getHeight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getPositionx();
  if (f.length > 0) {
    writer.writeString(
      8,
      f
    );
  }
  f = message.getPositiony();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getIspointerblocker();
  if (f) {
    writer.writeBool(
      10,
      f
    );
  }
  f = message.getParent();
  if (f != null) {
    writer.writeMessage(
      11,
      f,
      proto.engineinterface.PB_UIShape.serializeBinaryToWriter
    );
  }
};


/**
 * optional string name = 1;
 * @return {string}
 */
proto.engineinterface.PB_UIShape.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_UIShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float opacity = 3;
 * @return {number}
 */
proto.engineinterface.PB_UIShape.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional string hAlign = 4;
 * @return {string}
 */
proto.engineinterface.PB_UIShape.prototype.getHalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 4, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.setHalign = function(value) {
  return jspb.Message.setProto3StringField(this, 4, value);
};


/**
 * optional string vAlign = 5;
 * @return {string}
 */
proto.engineinterface.PB_UIShape.prototype.getValign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 5, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.setValign = function(value) {
  return jspb.Message.setProto3StringField(this, 5, value);
};


/**
 * optional string width = 6;
 * @return {string}
 */
proto.engineinterface.PB_UIShape.prototype.getWidth = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 6, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.setWidth = function(value) {
  return jspb.Message.setProto3StringField(this, 6, value);
};


/**
 * optional string height = 7;
 * @return {string}
 */
proto.engineinterface.PB_UIShape.prototype.getHeight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.setHeight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional string positionX = 8;
 * @return {string}
 */
proto.engineinterface.PB_UIShape.prototype.getPositionx = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 8, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.setPositionx = function(value) {
  return jspb.Message.setProto3StringField(this, 8, value);
};


/**
 * optional string positionY = 9;
 * @return {string}
 */
proto.engineinterface.PB_UIShape.prototype.getPositiony = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.setPositiony = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional bool isPointerBlocker = 10;
 * @return {boolean}
 */
proto.engineinterface.PB_UIShape.prototype.getIspointerblocker = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 10, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.setIspointerblocker = function(value) {
  return jspb.Message.setProto3BooleanField(this, 10, value);
};


/**
 * optional PB_UIShape parent = 11;
 * @return {?proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UIShape.prototype.getParent = function() {
  return /** @type{?proto.engineinterface.PB_UIShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIShape, 11));
};


/**
 * @param {?proto.engineinterface.PB_UIShape|undefined} value
 * @return {!proto.engineinterface.PB_UIShape} returns this
*/
proto.engineinterface.PB_UIShape.prototype.setParent = function(value) {
  return jspb.Message.setWrapperField(this, 11, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UIShape} returns this
 */
proto.engineinterface.PB_UIShape.prototype.clearParent = function() {
  return this.setParent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UIShape.prototype.hasParent = function() {
  return jspb.Message.getField(this, 11) != null;
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_UITextShape.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_UITextShape.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_UITextShape} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UITextShape.toObject = function(includeInstance, msg) {
  var f, obj = {
    name: jspb.Message.getFieldWithDefault(msg, 1, ""),
    visible: jspb.Message.getBooleanFieldWithDefault(msg, 2, false),
    opacity: jspb.Message.getFloatingPointFieldWithDefault(msg, 3, 0.0),
    halign: jspb.Message.getFieldWithDefault(msg, 4, ""),
    valign: jspb.Message.getFieldWithDefault(msg, 5, ""),
    width: jspb.Message.getFieldWithDefault(msg, 6, ""),
    height: jspb.Message.getFieldWithDefault(msg, 7, ""),
    positionx: jspb.Message.getFieldWithDefault(msg, 8, ""),
    positiony: jspb.Message.getFieldWithDefault(msg, 9, ""),
    ispointerblocker: jspb.Message.getBooleanFieldWithDefault(msg, 10, false),
    parent: (f = msg.getParent()) && proto.engineinterface.PB_UIShape.toObject(includeInstance, f),
    outlinewidth: jspb.Message.getFloatingPointFieldWithDefault(msg, 12, 0.0),
    outlinecolor: (f = msg.getOutlinecolor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    color: (f = msg.getColor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    fontsize: jspb.Message.getFloatingPointFieldWithDefault(msg, 15, 0.0),
    fontautosize: jspb.Message.getBooleanFieldWithDefault(msg, 16, false),
    fontweight: jspb.Message.getFieldWithDefault(msg, 17, ""),
    value: jspb.Message.getFieldWithDefault(msg, 18, ""),
    linespacing: jspb.Message.getFloatingPointFieldWithDefault(msg, 19, 0.0),
    linecount: jspb.Message.getFloatingPointFieldWithDefault(msg, 20, 0.0),
    adaptwidth: jspb.Message.getBooleanFieldWithDefault(msg, 21, false),
    adaptheight: jspb.Message.getBooleanFieldWithDefault(msg, 22, false),
    textwrapping: jspb.Message.getBooleanFieldWithDefault(msg, 23, false),
    shadowblur: jspb.Message.getFloatingPointFieldWithDefault(msg, 24, 0.0),
    shadowoffsetx: jspb.Message.getFloatingPointFieldWithDefault(msg, 25, 0.0),
    shadowoffsety: jspb.Message.getFloatingPointFieldWithDefault(msg, 26, 0.0),
    shadowcolor: (f = msg.getShadowcolor()) && proto.engineinterface.PB_Color4.toObject(includeInstance, f),
    htextalign: jspb.Message.getFieldWithDefault(msg, 28, ""),
    vtextalign: jspb.Message.getFieldWithDefault(msg, 29, ""),
    paddingtop: jspb.Message.getFloatingPointFieldWithDefault(msg, 30, 0.0),
    paddingright: jspb.Message.getFloatingPointFieldWithDefault(msg, 31, 0.0),
    paddingbottom: jspb.Message.getFloatingPointFieldWithDefault(msg, 32, 0.0),
    paddingleft: jspb.Message.getFloatingPointFieldWithDefault(msg, 33, 0.0)
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_UITextShape}
 */
proto.engineinterface.PB_UITextShape.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_UITextShape;
  return proto.engineinterface.PB_UITextShape.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_UITextShape} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_UITextShape}
 */
proto.engineinterface.PB_UITextShape.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setName(value);
      break;
    case 2:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setVisible(value);
      break;
    case 3:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOpacity(value);
      break;
    case 4:
      var value = /** @type {string} */ (reader.readString());
      msg.setHalign(value);
      break;
    case 5:
      var value = /** @type {string} */ (reader.readString());
      msg.setValign(value);
      break;
    case 6:
      var value = /** @type {string} */ (reader.readString());
      msg.setWidth(value);
      break;
    case 7:
      var value = /** @type {string} */ (reader.readString());
      msg.setHeight(value);
      break;
    case 8:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositionx(value);
      break;
    case 9:
      var value = /** @type {string} */ (reader.readString());
      msg.setPositiony(value);
      break;
    case 10:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setIspointerblocker(value);
      break;
    case 11:
      var value = new proto.engineinterface.PB_UIShape;
      reader.readMessage(value,proto.engineinterface.PB_UIShape.deserializeBinaryFromReader);
      msg.setParent(value);
      break;
    case 12:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setOutlinewidth(value);
      break;
    case 13:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setOutlinecolor(value);
      break;
    case 14:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setColor(value);
      break;
    case 15:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setFontsize(value);
      break;
    case 16:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setFontautosize(value);
      break;
    case 17:
      var value = /** @type {string} */ (reader.readString());
      msg.setFontweight(value);
      break;
    case 18:
      var value = /** @type {string} */ (reader.readString());
      msg.setValue(value);
      break;
    case 19:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setLinespacing(value);
      break;
    case 20:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setLinecount(value);
      break;
    case 21:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setAdaptwidth(value);
      break;
    case 22:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setAdaptheight(value);
      break;
    case 23:
      var value = /** @type {boolean} */ (reader.readBool());
      msg.setTextwrapping(value);
      break;
    case 24:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowblur(value);
      break;
    case 25:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowoffsetx(value);
      break;
    case 26:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setShadowoffsety(value);
      break;
    case 27:
      var value = new proto.engineinterface.PB_Color4;
      reader.readMessage(value,proto.engineinterface.PB_Color4.deserializeBinaryFromReader);
      msg.setShadowcolor(value);
      break;
    case 28:
      var value = /** @type {string} */ (reader.readString());
      msg.setHtextalign(value);
      break;
    case 29:
      var value = /** @type {string} */ (reader.readString());
      msg.setVtextalign(value);
      break;
    case 30:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingtop(value);
      break;
    case 31:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingright(value);
      break;
    case 32:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingbottom(value);
      break;
    case 33:
      var value = /** @type {number} */ (reader.readFloat());
      msg.setPaddingleft(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_UITextShape.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_UITextShape.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_UITextShape} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_UITextShape.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getName();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getVisible();
  if (f) {
    writer.writeBool(
      2,
      f
    );
  }
  f = message.getOpacity();
  if (f !== 0.0) {
    writer.writeFloat(
      3,
      f
    );
  }
  f = message.getHalign();
  if (f.length > 0) {
    writer.writeString(
      4,
      f
    );
  }
  f = message.getValign();
  if (f.length > 0) {
    writer.writeString(
      5,
      f
    );
  }
  f = message.getWidth();
  if (f.length > 0) {
    writer.writeString(
      6,
      f
    );
  }
  f = message.getHeight();
  if (f.length > 0) {
    writer.writeString(
      7,
      f
    );
  }
  f = message.getPositionx();
  if (f.length > 0) {
    writer.writeString(
      8,
      f
    );
  }
  f = message.getPositiony();
  if (f.length > 0) {
    writer.writeString(
      9,
      f
    );
  }
  f = message.getIspointerblocker();
  if (f) {
    writer.writeBool(
      10,
      f
    );
  }
  f = message.getParent();
  if (f != null) {
    writer.writeMessage(
      11,
      f,
      proto.engineinterface.PB_UIShape.serializeBinaryToWriter
    );
  }
  f = message.getOutlinewidth();
  if (f !== 0.0) {
    writer.writeFloat(
      12,
      f
    );
  }
  f = message.getOutlinecolor();
  if (f != null) {
    writer.writeMessage(
      13,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getColor();
  if (f != null) {
    writer.writeMessage(
      14,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getFontsize();
  if (f !== 0.0) {
    writer.writeFloat(
      15,
      f
    );
  }
  f = message.getFontautosize();
  if (f) {
    writer.writeBool(
      16,
      f
    );
  }
  f = message.getFontweight();
  if (f.length > 0) {
    writer.writeString(
      17,
      f
    );
  }
  f = message.getValue();
  if (f.length > 0) {
    writer.writeString(
      18,
      f
    );
  }
  f = message.getLinespacing();
  if (f !== 0.0) {
    writer.writeFloat(
      19,
      f
    );
  }
  f = message.getLinecount();
  if (f !== 0.0) {
    writer.writeFloat(
      20,
      f
    );
  }
  f = message.getAdaptwidth();
  if (f) {
    writer.writeBool(
      21,
      f
    );
  }
  f = message.getAdaptheight();
  if (f) {
    writer.writeBool(
      22,
      f
    );
  }
  f = message.getTextwrapping();
  if (f) {
    writer.writeBool(
      23,
      f
    );
  }
  f = message.getShadowblur();
  if (f !== 0.0) {
    writer.writeFloat(
      24,
      f
    );
  }
  f = message.getShadowoffsetx();
  if (f !== 0.0) {
    writer.writeFloat(
      25,
      f
    );
  }
  f = message.getShadowoffsety();
  if (f !== 0.0) {
    writer.writeFloat(
      26,
      f
    );
  }
  f = message.getShadowcolor();
  if (f != null) {
    writer.writeMessage(
      27,
      f,
      proto.engineinterface.PB_Color4.serializeBinaryToWriter
    );
  }
  f = message.getHtextalign();
  if (f.length > 0) {
    writer.writeString(
      28,
      f
    );
  }
  f = message.getVtextalign();
  if (f.length > 0) {
    writer.writeString(
      29,
      f
    );
  }
  f = message.getPaddingtop();
  if (f !== 0.0) {
    writer.writeFloat(
      30,
      f
    );
  }
  f = message.getPaddingright();
  if (f !== 0.0) {
    writer.writeFloat(
      31,
      f
    );
  }
  f = message.getPaddingbottom();
  if (f !== 0.0) {
    writer.writeFloat(
      32,
      f
    );
  }
  f = message.getPaddingleft();
  if (f !== 0.0) {
    writer.writeFloat(
      33,
      f
    );
  }
};


/**
 * optional string name = 1;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getName = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setName = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional bool visible = 2;
 * @return {boolean}
 */
proto.engineinterface.PB_UITextShape.prototype.getVisible = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 2, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setVisible = function(value) {
  return jspb.Message.setProto3BooleanField(this, 2, value);
};


/**
 * optional float opacity = 3;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getOpacity = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 3, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setOpacity = function(value) {
  return jspb.Message.setProto3FloatField(this, 3, value);
};


/**
 * optional string hAlign = 4;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getHalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 4, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setHalign = function(value) {
  return jspb.Message.setProto3StringField(this, 4, value);
};


/**
 * optional string vAlign = 5;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getValign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 5, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setValign = function(value) {
  return jspb.Message.setProto3StringField(this, 5, value);
};


/**
 * optional string width = 6;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getWidth = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 6, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setWidth = function(value) {
  return jspb.Message.setProto3StringField(this, 6, value);
};


/**
 * optional string height = 7;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getHeight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 7, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setHeight = function(value) {
  return jspb.Message.setProto3StringField(this, 7, value);
};


/**
 * optional string positionX = 8;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getPositionx = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 8, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setPositionx = function(value) {
  return jspb.Message.setProto3StringField(this, 8, value);
};


/**
 * optional string positionY = 9;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getPositiony = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 9, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setPositiony = function(value) {
  return jspb.Message.setProto3StringField(this, 9, value);
};


/**
 * optional bool isPointerBlocker = 10;
 * @return {boolean}
 */
proto.engineinterface.PB_UITextShape.prototype.getIspointerblocker = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 10, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setIspointerblocker = function(value) {
  return jspb.Message.setProto3BooleanField(this, 10, value);
};


/**
 * optional PB_UIShape parent = 11;
 * @return {?proto.engineinterface.PB_UIShape}
 */
proto.engineinterface.PB_UITextShape.prototype.getParent = function() {
  return /** @type{?proto.engineinterface.PB_UIShape} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_UIShape, 11));
};


/**
 * @param {?proto.engineinterface.PB_UIShape|undefined} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
*/
proto.engineinterface.PB_UITextShape.prototype.setParent = function(value) {
  return jspb.Message.setWrapperField(this, 11, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.clearParent = function() {
  return this.setParent(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UITextShape.prototype.hasParent = function() {
  return jspb.Message.getField(this, 11) != null;
};


/**
 * optional float outlineWidth = 12;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getOutlinewidth = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 12, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setOutlinewidth = function(value) {
  return jspb.Message.setProto3FloatField(this, 12, value);
};


/**
 * optional PB_Color4 outlineColor = 13;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UITextShape.prototype.getOutlinecolor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 13));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
*/
proto.engineinterface.PB_UITextShape.prototype.setOutlinecolor = function(value) {
  return jspb.Message.setWrapperField(this, 13, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.clearOutlinecolor = function() {
  return this.setOutlinecolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UITextShape.prototype.hasOutlinecolor = function() {
  return jspb.Message.getField(this, 13) != null;
};


/**
 * optional PB_Color4 color = 14;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UITextShape.prototype.getColor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 14));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
*/
proto.engineinterface.PB_UITextShape.prototype.setColor = function(value) {
  return jspb.Message.setWrapperField(this, 14, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.clearColor = function() {
  return this.setColor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UITextShape.prototype.hasColor = function() {
  return jspb.Message.getField(this, 14) != null;
};


/**
 * optional float fontSize = 15;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getFontsize = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 15, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setFontsize = function(value) {
  return jspb.Message.setProto3FloatField(this, 15, value);
};


/**
 * optional bool fontAutoSize = 16;
 * @return {boolean}
 */
proto.engineinterface.PB_UITextShape.prototype.getFontautosize = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 16, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setFontautosize = function(value) {
  return jspb.Message.setProto3BooleanField(this, 16, value);
};


/**
 * optional string fontWeight = 17;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getFontweight = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 17, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setFontweight = function(value) {
  return jspb.Message.setProto3StringField(this, 17, value);
};


/**
 * optional string value = 18;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getValue = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 18, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setValue = function(value) {
  return jspb.Message.setProto3StringField(this, 18, value);
};


/**
 * optional float lineSpacing = 19;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getLinespacing = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 19, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setLinespacing = function(value) {
  return jspb.Message.setProto3FloatField(this, 19, value);
};


/**
 * optional float lineCount = 20;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getLinecount = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 20, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setLinecount = function(value) {
  return jspb.Message.setProto3FloatField(this, 20, value);
};


/**
 * optional bool adaptWidth = 21;
 * @return {boolean}
 */
proto.engineinterface.PB_UITextShape.prototype.getAdaptwidth = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 21, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setAdaptwidth = function(value) {
  return jspb.Message.setProto3BooleanField(this, 21, value);
};


/**
 * optional bool adaptHeight = 22;
 * @return {boolean}
 */
proto.engineinterface.PB_UITextShape.prototype.getAdaptheight = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 22, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setAdaptheight = function(value) {
  return jspb.Message.setProto3BooleanField(this, 22, value);
};


/**
 * optional bool textWrapping = 23;
 * @return {boolean}
 */
proto.engineinterface.PB_UITextShape.prototype.getTextwrapping = function() {
  return /** @type {boolean} */ (jspb.Message.getBooleanFieldWithDefault(this, 23, false));
};


/**
 * @param {boolean} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setTextwrapping = function(value) {
  return jspb.Message.setProto3BooleanField(this, 23, value);
};


/**
 * optional float shadowBlur = 24;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getShadowblur = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 24, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setShadowblur = function(value) {
  return jspb.Message.setProto3FloatField(this, 24, value);
};


/**
 * optional float shadowOffsetX = 25;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getShadowoffsetx = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 25, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setShadowoffsetx = function(value) {
  return jspb.Message.setProto3FloatField(this, 25, value);
};


/**
 * optional float shadowOffsetY = 26;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getShadowoffsety = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 26, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setShadowoffsety = function(value) {
  return jspb.Message.setProto3FloatField(this, 26, value);
};


/**
 * optional PB_Color4 shadowColor = 27;
 * @return {?proto.engineinterface.PB_Color4}
 */
proto.engineinterface.PB_UITextShape.prototype.getShadowcolor = function() {
  return /** @type{?proto.engineinterface.PB_Color4} */ (
    jspb.Message.getWrapperField(this, proto.engineinterface.PB_Color4, 27));
};


/**
 * @param {?proto.engineinterface.PB_Color4|undefined} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
*/
proto.engineinterface.PB_UITextShape.prototype.setShadowcolor = function(value) {
  return jspb.Message.setWrapperField(this, 27, value);
};


/**
 * Clears the message field making it undefined.
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.clearShadowcolor = function() {
  return this.setShadowcolor(undefined);
};


/**
 * Returns whether this field is set.
 * @return {boolean}
 */
proto.engineinterface.PB_UITextShape.prototype.hasShadowcolor = function() {
  return jspb.Message.getField(this, 27) != null;
};


/**
 * optional string hTextAlign = 28;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getHtextalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 28, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setHtextalign = function(value) {
  return jspb.Message.setProto3StringField(this, 28, value);
};


/**
 * optional string vTextAlign = 29;
 * @return {string}
 */
proto.engineinterface.PB_UITextShape.prototype.getVtextalign = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 29, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setVtextalign = function(value) {
  return jspb.Message.setProto3StringField(this, 29, value);
};


/**
 * optional float paddingTop = 30;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getPaddingtop = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 30, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setPaddingtop = function(value) {
  return jspb.Message.setProto3FloatField(this, 30, value);
};


/**
 * optional float paddingRight = 31;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getPaddingright = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 31, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setPaddingright = function(value) {
  return jspb.Message.setProto3FloatField(this, 31, value);
};


/**
 * optional float paddingBottom = 32;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getPaddingbottom = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 32, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setPaddingbottom = function(value) {
  return jspb.Message.setProto3FloatField(this, 32, value);
};


/**
 * optional float paddingLeft = 33;
 * @return {number}
 */
proto.engineinterface.PB_UITextShape.prototype.getPaddingleft = function() {
  return /** @type {number} */ (jspb.Message.getFloatingPointFieldWithDefault(this, 33, 0.0));
};


/**
 * @param {number} value
 * @return {!proto.engineinterface.PB_UITextShape} returns this
 */
proto.engineinterface.PB_UITextShape.prototype.setPaddingleft = function(value) {
  return jspb.Message.setProto3FloatField(this, 33, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_OpenExternalUrl.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_OpenExternalUrl.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_OpenExternalUrl} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_OpenExternalUrl.toObject = function(includeInstance, msg) {
  var f, obj = {
    url: jspb.Message.getFieldWithDefault(msg, 1, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_OpenExternalUrl}
 */
proto.engineinterface.PB_OpenExternalUrl.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_OpenExternalUrl;
  return proto.engineinterface.PB_OpenExternalUrl.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_OpenExternalUrl} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_OpenExternalUrl}
 */
proto.engineinterface.PB_OpenExternalUrl.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setUrl(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_OpenExternalUrl.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_OpenExternalUrl.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_OpenExternalUrl} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_OpenExternalUrl.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getUrl();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
};


/**
 * optional string url = 1;
 * @return {string}
 */
proto.engineinterface.PB_OpenExternalUrl.prototype.getUrl = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_OpenExternalUrl} returns this
 */
proto.engineinterface.PB_OpenExternalUrl.prototype.setUrl = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};





if (jspb.Message.GENERATE_TO_OBJECT) {
/**
 * Creates an object representation of this proto.
 * Field names that are reserved in JavaScript and will be renamed to pb_name.
 * Optional fields that are not set will be set to undefined.
 * To access a reserved field use, foo.pb_<name>, eg, foo.pb_default.
 * For the list of reserved names please see:
 *     net/proto2/compiler/js/internal/generator.cc#kKeyword.
 * @param {boolean=} opt_includeInstance Deprecated. whether to include the
 *     JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @return {!Object}
 */
proto.engineinterface.PB_OpenNFTDialog.prototype.toObject = function(opt_includeInstance) {
  return proto.engineinterface.PB_OpenNFTDialog.toObject(opt_includeInstance, this);
};


/**
 * Static version of the {@see toObject} method.
 * @param {boolean|undefined} includeInstance Deprecated. Whether to include
 *     the JSPB instance for transitional soy proto support:
 *     http://goto/soy-param-migration
 * @param {!proto.engineinterface.PB_OpenNFTDialog} msg The msg instance to transform.
 * @return {!Object}
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_OpenNFTDialog.toObject = function(includeInstance, msg) {
  var f, obj = {
    assetcontractaddress: jspb.Message.getFieldWithDefault(msg, 1, ""),
    tokenid: jspb.Message.getFieldWithDefault(msg, 2, ""),
    comment: jspb.Message.getFieldWithDefault(msg, 3, "")
  };

  if (includeInstance) {
    obj.$jspbMessageInstance = msg;
  }
  return obj;
};
}


/**
 * Deserializes binary data (in protobuf wire format).
 * @param {jspb.ByteSource} bytes The bytes to deserialize.
 * @return {!proto.engineinterface.PB_OpenNFTDialog}
 */
proto.engineinterface.PB_OpenNFTDialog.deserializeBinary = function(bytes) {
  var reader = new jspb.BinaryReader(bytes);
  var msg = new proto.engineinterface.PB_OpenNFTDialog;
  return proto.engineinterface.PB_OpenNFTDialog.deserializeBinaryFromReader(msg, reader);
};


/**
 * Deserializes binary data (in protobuf wire format) from the
 * given reader into the given message object.
 * @param {!proto.engineinterface.PB_OpenNFTDialog} msg The message object to deserialize into.
 * @param {!jspb.BinaryReader} reader The BinaryReader to use.
 * @return {!proto.engineinterface.PB_OpenNFTDialog}
 */
proto.engineinterface.PB_OpenNFTDialog.deserializeBinaryFromReader = function(msg, reader) {
  while (reader.nextField()) {
    if (reader.isEndGroup()) {
      break;
    }
    var field = reader.getFieldNumber();
    switch (field) {
    case 1:
      var value = /** @type {string} */ (reader.readString());
      msg.setAssetcontractaddress(value);
      break;
    case 2:
      var value = /** @type {string} */ (reader.readString());
      msg.setTokenid(value);
      break;
    case 3:
      var value = /** @type {string} */ (reader.readString());
      msg.setComment(value);
      break;
    default:
      reader.skipField();
      break;
    }
  }
  return msg;
};


/**
 * Serializes the message to binary data (in protobuf wire format).
 * @return {!Uint8Array}
 */
proto.engineinterface.PB_OpenNFTDialog.prototype.serializeBinary = function() {
  var writer = new jspb.BinaryWriter();
  proto.engineinterface.PB_OpenNFTDialog.serializeBinaryToWriter(this, writer);
  return writer.getResultBuffer();
};


/**
 * Serializes the given message to binary data (in protobuf wire
 * format), writing to the given BinaryWriter.
 * @param {!proto.engineinterface.PB_OpenNFTDialog} message
 * @param {!jspb.BinaryWriter} writer
 * @suppress {unusedLocalVariables} f is only used for nested messages
 */
proto.engineinterface.PB_OpenNFTDialog.serializeBinaryToWriter = function(message, writer) {
  var f = undefined;
  f = message.getAssetcontractaddress();
  if (f.length > 0) {
    writer.writeString(
      1,
      f
    );
  }
  f = message.getTokenid();
  if (f.length > 0) {
    writer.writeString(
      2,
      f
    );
  }
  f = message.getComment();
  if (f.length > 0) {
    writer.writeString(
      3,
      f
    );
  }
};


/**
 * optional string assetContractAddress = 1;
 * @return {string}
 */
proto.engineinterface.PB_OpenNFTDialog.prototype.getAssetcontractaddress = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 1, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_OpenNFTDialog} returns this
 */
proto.engineinterface.PB_OpenNFTDialog.prototype.setAssetcontractaddress = function(value) {
  return jspb.Message.setProto3StringField(this, 1, value);
};


/**
 * optional string tokenId = 2;
 * @return {string}
 */
proto.engineinterface.PB_OpenNFTDialog.prototype.getTokenid = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 2, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_OpenNFTDialog} returns this
 */
proto.engineinterface.PB_OpenNFTDialog.prototype.setTokenid = function(value) {
  return jspb.Message.setProto3StringField(this, 2, value);
};


/**
 * optional string comment = 3;
 * @return {string}
 */
proto.engineinterface.PB_OpenNFTDialog.prototype.getComment = function() {
  return /** @type {string} */ (jspb.Message.getFieldWithDefault(this, 3, ""));
};


/**
 * @param {string} value
 * @return {!proto.engineinterface.PB_OpenNFTDialog} returns this
 */
proto.engineinterface.PB_OpenNFTDialog.prototype.setComment = function(value) {
  return jspb.Message.setProto3StringField(this, 3, value);
};


/**
 * @enum {number}
 */
proto.engineinterface.PB_UIStackOrientation = {
  VERTICAL: 0,
  HORIZONTAL: 1
};

goog.object.extend(exports, proto.engineinterface);
