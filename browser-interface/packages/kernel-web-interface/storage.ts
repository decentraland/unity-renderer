/**
 * @public
 */
export interface PersistentAsyncStorage {
  /**
   * Empties the list associated with the object of all key/value pairs, if there are any.
   */
  clear(): Promise<void>

  /**
   * Returns the current value associated with the given key, or null if the given key does not exist in the list associated with the object.
   */
  getItem(key: string): Promise<string | null>

  /**
   * Returns the string array of keys
   */
  keys(): Promise<string[]>
  /**
   * Removes the key/value pair with the given key from the list associated with the object, if a key/value pair with the given key exists.
   */
  removeItem(key: string): Promise<void>

  /**
   * Sets the value of the pair identified by key to value, creating a new key/value pair if none existed for key previously.
   *
   * Throws a "QuotaExceededError" DOMException exception if the new value couldn't be set. (Setting could fail if, e.g., the user has disabled storage for the site, or if the quota has been exceeded.)
   */
  setItem(key: string, value: string): Promise<void>
}
