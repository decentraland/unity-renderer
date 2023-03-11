import { trackEvent } from 'shared/analytics/trackEvent'
import { getPerformanceInfo } from 'shared/session/getPerformanceInfo'
import { getUnityInstance } from 'unity-interface/IUnityInterface'

export function handlePerformanceReport(data: Record<string, unknown>) {
  let estimatedAllocatedMemory = 0
  let estimatedTotalMemory = 0
  if (getUnityInstance()?.Module?.asmLibraryArg?._GetDynamicMemorySize) {
    estimatedAllocatedMemory = getUnityInstance().Module.asmLibraryArg._GetDynamicMemorySize()
    estimatedTotalMemory = getUnityInstance().Module.asmLibraryArg._GetTotalMemorySize()
  }
  const perfReport = getPerformanceInfo({ ...(data as any), estimatedAllocatedMemory, estimatedTotalMemory })
  trackEvent('performance report', perfReport)
}
