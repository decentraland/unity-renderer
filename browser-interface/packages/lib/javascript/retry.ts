export async function retry<T>(
  operation: () => Promise<T>,
  attempts: number = 5,
  onEachFailure: (e: any) => void = (_e) => {
    // Nothing
  }
): Promise<T> {
  let error: any = undefined
  let attempt = 0

  while (attempt < attempts) {
    attempt++
    try {
      return await operation()
    } catch (e) {
      onEachFailure(e)
      error = e
    }
  }

  throw error
}
