// taken from https://github.com/aurelia/task-queue (MIT)

const stackSeparator = '\nEnqueued in TaskQueue by:\n'
const microStackSeparator = '\nEnqueued in MicroTaskQueue by:\n'

const defer = Promise.resolve().then.bind(Promise.resolve())

function makeRequestFlushFromTimer(flush: Function) {
  return function requestFlush() {
    // We dispatch a timeout with a specified delay of 0 for engines that
    // can reliably accommodate that request. This will usually be snapped
    // to a 4 milisecond delay, but once we're flushing, there's no delay
    // between events.
    let timeoutHandle = setTimeout(handleFlushTimer, 0)
    // However, since this timer gets frequently dropped in Firefox
    // workers, we enlist an interval handle that will try to fire
    // an event 20 times per second until it succeeds.
    let intervalHandle = setInterval(handleFlushTimer, 50)
    function handleFlushTimer() {
      // Whichever timer succeeds will cancel both timers and request the
      // flush.
      clearTimeout(timeoutHandle)
      clearInterval(intervalHandle)
      flush()
    }
  }
}

function onError(error: Error, task: Task, longStacks: boolean) {
  if (longStacks && task.stack && typeof (error as any) === 'object' && (error as any) !== null) {
    // Note: IE sets error.stack when throwing but does not override a defined .stack.
    error.stack = filterFlushStack(error.stack || '') + task.stack
  }

  if ('onError' in task) {
    task.onError!(error)
  } else {
    setTimeout(() => {
      throw error
    }, 0)
  }
}

/**
 * Either a Function or a class with a call method that will do work when dequeued.
 */
export interface Task {
  stack?: string

  /**
   * Call it.
   */
  call: Function

  onError?(error: any): void
}

/**
 * Implements an asynchronous task queue.
 */
export class TaskQueue {
  /**
   * Whether the queue is in the process of flushing.
   */
  flushing = false

  /**
   * Enables long stack traces for queued tasks.
   */
  longStacks = false

  microTaskQueue: Task[] = []
  taskQueue: Task[] = []

  requestFlushTaskQueue = makeRequestFlushFromTimer(() => this.flushTaskQueue())

  stack?: string

  requestFlushMicroTaskQueue = () => {
    defer(async () => this.flushMicroTaskQueue())
  }

  /**
   * Queues a task on the micro task queue for ASAP execution.
   * @param task The task to queue up for ASAP execution.
   */
  queueMicroTask(task: Task): void {
    if (this.microTaskQueue.length < 1) {
      this.requestFlushMicroTaskQueue()
    }

    if (this.longStacks) {
      task.stack = this.prepareQueueStack(microStackSeparator)
    }

    this.microTaskQueue.push(task)
  }

  /**
   * Queues a task on the macro task queue for turn-based execution.
   * @param task The task to queue up for turn-based execution.
   */
  queueTask(task: Task): void {
    if (this.taskQueue.length < 1) {
      this.requestFlushTaskQueue()
    }

    if (this.longStacks) {
      task.stack = this.prepareQueueStack(stackSeparator)
    }

    this.taskQueue.push(task)
  }

  /**
   * Immediately flushes the task queue.
   */
  flushTaskQueue(): void {
    let queue = this.taskQueue
    this.taskQueue = [] // recursive calls to queueTask should be scheduled after the next cycle
    this.flushQueue(queue)
  }

  /**
   * Immediately flushes the micro task queue.
   */
  flushMicroTaskQueue(): void {
    let queue = this.microTaskQueue
    this.flushQueue(queue)
    queue.length = 0
  }

  prepareQueueStack(separator: string) {
    let stack = separator + filterQueueStack(captureStack())

    if (typeof this.stack === 'string') {
      stack = filterFlushStack(stack) + this.stack
    }

    return stack
  }

  /**
   * Immediately flushes the queue.
   * @param queue The task queue or micro task queue
   */
  private flushQueue(queue: Task[]): void {
    let task: Task | null = null

    try {
      this.flushing = true
      while (queue.length) {
        task = queue.shift() as Task

        if (this.longStacks) {
          this.stack = typeof task.stack === 'string' ? task.stack : undefined
        }

        task.call()
      }
    } catch (error) {
      onError(error, task as any, this.longStacks)
    } finally {
      this.flushing = false
      this.stack = undefined
    }
  }
}

function captureStack(): string {
  let error = new Error()

  // Firefox, Chrome, Edge all have .stack defined by now, IE has not.
  if (error.stack) {
    return error.stack
  }

  try {
    throw error
  } catch (e) {
    return e.stack
  }
}

function filterQueueStack(stack: string) {
  // Remove everything (error message + top stack frames) up to the topmost queueTask or queueMicroTask call
  return stack.replace(/^[\s\S]*?\bqueue(Micro)?Task\b[^\n]*\n/, '')
}

function filterFlushStack(stack: string) {
  // Remove bottom frames starting with the last flushTaskQueue or flushMicroTaskQueue
  let index = stack.lastIndexOf('flushMicroTaskQueue')

  if (index < 0) {
    index = stack.lastIndexOf('flushTaskQueue')
    if (index < 0) {
      return stack
    }
  }

  index = stack.lastIndexOf('\n', index)

  return index < 0 ? stack : stack.substr(0, index)
  // The following would work but without regex support to match from end of string,
  // it's hard to ensure we have the last occurence of "flushTaskQueue".
  // x return stack.replace(/\n[^\n]*?\bflush(Micro)?TaskQueue\b[\s\S]*$/, "");
}
