const hasSymbol = typeof Symbol === 'function' && Symbol.for

export function hasOwnSymbol(object: any, symbol: any) {
  return hasSymbol
    ? Object.getOwnPropertySymbols(object).includes(symbol)
    : Object.getOwnPropertyNames(object).includes(symbol)
}
