import { doDaThing } from './printer'

if (typeof log === 'undefined') {
  throw new Error('log function not defined')
}

doDaThing()
