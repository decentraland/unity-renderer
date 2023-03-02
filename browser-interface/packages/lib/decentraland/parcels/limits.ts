// Maximum numbers for parcelScenes to prevent performance problems
// Note that more limitations may be added to this with time
// And we may also measure individual parcelScene performance (as
// in webgl draw time) and disable parcelScenes based on that too,
// Performance / anti-ddos work is a fluid area.

// number of entities
export const entities = 200

// Number of faces (per parcel)
export const triangles = 10000
export const bodies = 300
export const textures = 10
export const materials = 20
export const height = 20
export const geometries = 200

export const parcelSize = 16 /* meters */
export const halfParcelSize = parcelSize / 2 /* meters */
export const centimeter = 0.01

export const maxParcelX = 150
export const maxParcelZ = 150
