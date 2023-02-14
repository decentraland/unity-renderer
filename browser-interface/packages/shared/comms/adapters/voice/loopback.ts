const offerOptions = {
  offerVideo: false,
  offerAudio: true,
  offerToReceiveAudio: false,
  offerToReceiveVideo: false
}

export async function startLoopback(stream: MediaStream) {
  const loopbackStream = new MediaStream()
  const rtcConnection = new RTCPeerConnection()
  const rtcLoopbackConnection = new RTCPeerConnection()

  rtcConnection.onicecandidate = (e) =>
    e.candidate && rtcLoopbackConnection.addIceCandidate(new RTCIceCandidate(e.candidate))

  rtcLoopbackConnection.onicecandidate = (e) =>
    e.candidate && rtcConnection.addIceCandidate(new RTCIceCandidate(e.candidate))

  rtcLoopbackConnection.ontrack = (e) => e.streams[0].getTracks().forEach((track) => loopbackStream.addTrack(track))

  // setup the loopback
  stream.getTracks().forEach((track) => rtcConnection.addTrack(track, stream))

  const offer = await rtcConnection.createOffer(offerOptions)
  await rtcConnection.setLocalDescription(offer)

  await rtcLoopbackConnection.setRemoteDescription(offer)
  const answer = await rtcLoopbackConnection.createAnswer()
  await rtcLoopbackConnection.setLocalDescription(answer)

  await rtcConnection.setRemoteDescription(answer)

  return loopbackStream
}
