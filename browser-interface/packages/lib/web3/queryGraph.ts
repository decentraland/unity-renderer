export async function queryGraph(url: string, query: string, variables: any, _totalAttempts: number = 5) {
  const ret = await fetch(url, {
    method: 'POST',
    body: JSON.stringify({ query, variables }),
    headers: { 'Content-Type': 'application/json' }
  })
  const response = await ret.json()
  if (response.errors) {
    throw new Error(`Error querying graph. Reasons: ${JSON.stringify(response.errors)}`)
  }
  return response.data
}
