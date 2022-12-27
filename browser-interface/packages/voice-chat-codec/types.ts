export enum RequestTopic {
  ENCODE = 'ENCODE',
  DECODE = 'DECODE',
  DESTROY_ENCODER = 'DESTROY_ENCODER',
  DESTROY_DECODER = 'DESTROY_ENCODER'
}

export enum InputWorkletRequestTopic {
  ENCODE = 'ENCODE',
  PAUSE = 'PAUSE',
  RESUME = 'RESUME',
  ON_PAUSED = 'ON_PAUSED',
  ON_RECORDING = 'ON_RECORDING',
  CHECK_STATUS = 'CHECK_STATUS',
  TIMEOUT = 'TIMEOUT'
}

export enum OutputWorkletRequestTopic {
  STREAM_PLAYING = 'STREAM_PLAYING',
  WRITE_SAMPLES = 'WRITE_SAMPLES'
}

export enum ResponseTopic {
  ENCODE = 'ENCODE_OUTPUT',
  DECODE = 'DECODE_OUTPUT'
}

export type VoiceChatWorkerRequest = { topic: RequestTopic } & any
export type VoiceChatWorkerResponse = { topic: ResponseTopic } & any
