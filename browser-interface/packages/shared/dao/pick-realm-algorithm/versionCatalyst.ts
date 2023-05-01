import semver from 'semver'
import { Candidate } from '../types'
import { AlgorithmContext, AlgorithmLink, AlgorithmLinkTypes, VersionCatalystParameters } from './types'
import { isValidCommsProtocol } from './utils'

interface ServiceVersionValidator {
  isValid: (version: string | undefined) => boolean
  isMet: (requiredVersion: string, candidate: Candidate) => boolean
}

const serviceVersionChecker = {
  content: {
    isValid: semver.valid,
    isMet: (requiredVersion: string, candidate: Candidate) =>
      !semver.valid(candidate.version.content) || semver.gte(candidate.version.content, requiredVersion)
  },
  lambdas: {
    isValid: semver.valid,
    isMet: (requiredVersion: string, candidate: Candidate) =>
      !semver.valid(candidate.version.lambdas) || semver.gte(candidate.version.lambdas, requiredVersion)
  },
  bff: {
    isValid: semver.valid,
    isMet: (requiredVersion: string, candidate: Candidate) =>
      !semver.valid(candidate.version.bff) || semver.gte(candidate.version.bff, requiredVersion)
  },
  comms: {
    isValid: isValidCommsProtocol,
    isMet: (requiredVersion: string, candidate: Candidate) => {
      const protocolPattern = /^v(\d+)$/
      const currentVersionMatch = candidate.version.comms.match(protocolPattern)

      if (currentVersionMatch) {
        return Number(requiredVersion.match(protocolPattern)![1]) <= Number(currentVersionMatch[1])
      } else {
        return true
      }
    }
  }
}

export function versionCatalystLink(minimumVersions?: VersionCatalystParameters): AlgorithmLink {
  return {
    name: AlgorithmLinkTypes.VERSION_CATALYST,
    pick: (context: AlgorithmContext) => {
      let picked = context.picked

      minimumVersions &&
        Object.entries(minimumVersions).forEach(([serviceName, requiredVersion]) => {
          const serviceVersionValidator: ServiceVersionValidator = serviceVersionChecker[serviceName]

          if (serviceVersionValidator.isValid(requiredVersion)) {
            picked = picked.filter((candidate) => serviceVersionValidator.isMet(requiredVersion, candidate))
          }
        })

      if (picked.length === 1) {
        context.selected = picked[0]
      }

      context.picked = picked
      return context
    }
  }
}
