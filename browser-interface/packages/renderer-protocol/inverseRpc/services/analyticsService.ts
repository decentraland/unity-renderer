import { RpcServerPort } from '@dcl/rpc'
import { RendererProtocolContext } from '../context'
import * as codegen from '@dcl/rpc/dist/codegen'
import { AnalyticsKernelServiceDefinition } from 'shared/protocol/decentraland/renderer/kernel_services/analytics.gen'
import { getPerformanceInfo } from 'shared/session/getPerformanceInfo'
import { getUnityInstance } from 'unity-interface/IUnityInterface'
import { trackEvent } from 'shared/analytics/trackEvent'
import { setDelightedSurveyEnabled } from 'unity-interface/delightedSurvey'
import { browserInterface } from 'unity-interface/BrowserInterface'

type UnityEvent = any

export function registerAnalyticsKernelService(port: RpcServerPort<RendererProtocolContext>) {
  codegen.registerService(port, AnalyticsKernelServiceDefinition, async () => ({
    async analyticsEvent(req, _) {
      const properties: Record<string, string> = {}
      if (req.properties) {
        for (const property of req.properties) {
          properties[property.key] = property.value
        }
      }

      trackEvent(req.eventName as UnityEvent, { context: properties.context || 'unity-event', ...properties })
      return {}
    },
    async performanceReport(req, _) {
      let estimatedAllocatedMemory = 0
      let estimatedTotalMemory = 0
      if (getUnityInstance()?.Module?.asmLibraryArg?._GetDynamicMemorySize) {
        estimatedAllocatedMemory = getUnityInstance().Module.asmLibraryArg._GetDynamicMemorySize()
        estimatedTotalMemory = getUnityInstance().Module.asmLibraryArg._GetTotalMemorySize()
      }
      const perfReport = getPerformanceInfo({ ...(req as any), estimatedAllocatedMemory, estimatedTotalMemory })
      trackEvent('performance report', perfReport)
      return {}
    },
    async setDelightedSurveyEnabled(req, _) {
      setDelightedSurveyEnabled(req.enabled)
      return {}
    },
    async systemInfoReport(req, _) {
      trackEvent('system info report', req)

      // @deprecated
      browserInterface.startedFuture.resolve()
      return {}
    }
  }))
}
