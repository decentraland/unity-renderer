export function zip<T, R>(data: T[], transform: (e: T) => [string, R]) {
  const result: Record<string, R> = {}
  for (let i of data) {
    const elem = transform(i)
    result[elem[0]] = elem[1]
  }
  return result
}
