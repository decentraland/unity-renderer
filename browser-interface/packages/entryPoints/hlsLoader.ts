// ensure that Hls exist in the global context. To enable http live streaming in the browsers
import _hls from 'hls.js'
declare const globalThis: any
if (typeof globalThis.Hls === 'undefined') {
  // HLS is required to make video texture and streaming work in Unity
  globalThis.Hls = _hls
}
export default _hls
