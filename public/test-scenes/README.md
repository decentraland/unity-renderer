# Naming the folders

`XX.YY.description-with-hypens` i.e. `200.0.tic-tac-toe`

Every folder MUST contain a `scene.json` file. You can initialize it using the CLI.

# File structure

* **0.XX**: Individual features, like color components, audio component, scale, entitites, parcelScene shapes.
* **200.XX**: Scenes with scripts
* **1000.XX**: Benchmarks

# Additional mock data

It is posible to overwrite scene data by defining a `mock-data.json` file that complies with the `ILand` interface. The main use case for this is to provide a way of mocking the `owner` and `hash` values for a test parcel.
