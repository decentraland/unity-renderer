declare module '@decentraland/ChatController' {
  export type MessageEntry = {
    id: string
    sender: string
    message: string
    isCommand?: boolean
  }

  export interface IChatCommand {
    name: string
    description: string
    run: (message: string) => MessageEntry
  }

  /**
   * Send the chat message
   * @param messageEntry
   */
  export function send(messageEntry: MessageEntry): Promise<boolean | MessageEntry>

  /**
   * Return list of chat commands
   */
  export function getChatCommands(): Promise<{ [key: string]: IChatCommand }>

  /**
   * Subscribes to events dispatched by the EngineAPI
   * Use it to listen to events from the scene (like `click`)
   * @param event
   */
  export function subscribe(event: string): Promise<void>

  /**
   * Removes a subscription to an event
   * @param event
   */
  export function unsubscribe(event: string): Promise<void>

  /** Event handler for subscription events */
  export function onSubscribedEvent(fn: any): void
}
