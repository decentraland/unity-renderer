export const MAX_SPACES = 90
export function spaces(amount: number) {
  if (typeof amount !== 'number' || amount < 0) {
    return ''
  } else if (amount > MAX_SPACES || isNaN(amount)) {
    return new Array(MAX_SPACES).join(' ') + ' '
  } else {
    return new Array(amount).join(' ') + ' '
  }
}
