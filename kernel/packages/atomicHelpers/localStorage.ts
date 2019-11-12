declare var window: any

export function saveToLocalStorage(key: string, data: any) {
  if (!window.localStorage) {
    throw new Error('Storage not supported')
  }
  window.localStorage.setItem(key, JSON.stringify(data))
}

export function getFromLocalStorage(key: string) {
  if (!window.localStorage) {
    throw new Error('Storage not supported')
  }
  const data = window.localStorage.getItem(key)
  return (data && JSON.parse(data)) || null
}

export function removeFromLocalStorage(key: string) {
  if (!window.localStorage) {
    throw new Error('Storage not supported')
  }
  window.localStorage.removeItem(key)
}
