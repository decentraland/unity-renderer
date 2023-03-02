export function pickProperty<T>(arr: T[], key: keyof T) {
  return arr.map((_) => _[key])
}
