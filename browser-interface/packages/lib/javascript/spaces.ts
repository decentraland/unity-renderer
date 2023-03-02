export const MAX_SPACES = 90
export function spaces(amount: number) {
  if (!Number.isFinite(amount) || amount < 0) {
    return ''
  }
  return ' '.repeat(Math.min(MAX_SPACES, amount))
}
