export function getResourcesURL() {
  let pathName = location.pathname.split('/')
  if (pathName[pathName.length - 1].includes('.')) {
    pathName.pop()
  }

  const basePath = origin + pathName.join('/')
  if (basePath.endsWith('/')) return basePath.slice(0, -1)
  return basePath
}
