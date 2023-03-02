# Things to test

## Scene loader

* [ ] Given a scene described in HTML with scripts and HTML garbage, it should parse and ignore non \* nodes
* [ ] Given a scene described in XML, it should parse and ignore non \* nodes

## Parcel manager

* [ ] Given a position XY, If I am standing in XY it should load a circle of parcels around XY. Every loaded parcel should be stored in the parcels map.
* [ ] Given a position XY, If I am standing in XY it should load a circle of parcels around XY. Every loaded parcel should be stored in the parcels map. Then if I move to XY + Radius, I should have the same ammount of parcels in memory because the parsels out of the radius should unload
* [ ] Test the hole detection

## Scene loader from simplified nodes

* [ ] Given tons of models in a scene, I want the same quantity of elements in the scene
* [ ] Given invalid simplified nodes (malformed objects, i.e. without `tag`) the rest of the scene should load
* [ ] Given an element that falls outside a scene, that element should not be rendered, the rest of the scene yes
* [ ] Given a scene with that doubles the ammount of allowed elements, the allowed elements (the first half) should be rendered

## Component schema validation

* [ ] Given a single property schema, validations should pass and values should be casted
* [ ] Given a single property schema, validations should fail and a semantic error should be logged
* [ ] Given a multi-property schema, validations should pass and values should be casted
* [ ] Given a multi-property schema, validations should fail and a semantic error should be logged

## Event bubbling

* [ ] Given a event triggered in an entity inside a system, the element should bubble until the system and stop the propagation there.
* [ ] Given a event emited inside a system (a) entity, which is also inside another system (b). The event should reach only the system B and not the system A

## Parcel loading manager

* [ ] Function `getExpectedParcels`

## Entity loading and unloading

* [ ] Given a tree of entities. If we remove the root (like scene). All the children should call the lifecycle method `entityWillUnload`. That will be used to dispose resources. All entities are disposables
