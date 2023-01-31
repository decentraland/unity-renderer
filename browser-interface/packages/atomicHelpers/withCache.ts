export default function withCache<R>(handler: () => R): () => R {
  const empty = Symbol('@empty')
  let cache: R | symbol = empty
  return () => {
    if (cache === empty) {
      cache = handler()
    }

    return cache as R
  }
}
