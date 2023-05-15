import mitt from 'mitt'
import type { NamedEvents } from '@dcl/kernel-interface'
export const globalObservable = mitt<NamedEvents>()
