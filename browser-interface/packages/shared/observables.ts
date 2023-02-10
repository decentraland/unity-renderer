import mitt from 'mitt'
import type { NamedEvents } from 'kernel-web-interface'
export const globalObservable = mitt<NamedEvents>()
