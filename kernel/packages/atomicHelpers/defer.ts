export const defer = Promise.prototype.then.bind(Promise.resolve())
