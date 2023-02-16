export function arrayCleanup<T>(array: T[] | null | undefined): T[] | undefined {
  return !array || array.length === 0 ? undefined : array
}
