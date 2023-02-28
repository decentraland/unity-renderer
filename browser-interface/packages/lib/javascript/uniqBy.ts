export function uniqBy(arr: any[], key: string) {
  return Array.from(new Map(arr.map((item) => [item[key], item])).values())
}
