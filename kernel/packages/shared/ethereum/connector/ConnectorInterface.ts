export interface ConnectorInterface {
  getProvider(): any

  login(): Promise<any>

  logout(): Promise<boolean>

  isAvailable(): boolean
}
