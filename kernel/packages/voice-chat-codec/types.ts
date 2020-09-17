export enum RequestTopic {
  ENCODE = 'ENCODE',
  DECODE = 'DECODE',
  DESTROY_ENCODER = 'DESTROY_ENCODER',
  DESTROY_DECODER = 'DESTROY_ENCODER'
}

export enum ResponseTopic {
  ENCODE = 'ENCODE_OUTPUT',
  DECODE = 'DECODE_OUTPUT'
}

export type VoiceChatWorkerRequest = { topic: RequestTopic } & any
export type VoiceChatWorkerResponse = { topic: ResponseTopic } & any
