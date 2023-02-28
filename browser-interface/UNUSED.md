98 modules with unused exports
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/config/index.ts: isRunningTest
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/renderer-protocol/transports/webTransport.ts: WebTransportOptions
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/renderer-protocol/transports/webSocketTransportAdapter.ts: defer
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/apis/IEnvironmentAPI.ts: ExplorerConfiguration
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/world/parcelSceneManager.ts: isParcelDenyListed, generateBannedLoadableScene, onLoadParcelScenesObservable, parcelSceneLoadingState, getDesiredParcelScenes, TEST_OBJECT_ObservableAllScenesEvent
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/world/selectors.ts: getCurrentScene
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/world/SceneWorker.ts: SceneLifeCycleStatusType, SceneLifeCycleStatusReport
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/occurences.ts: Counters
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/renderer/actions.ts: SignalRendererInitialized, SignalParcelLoadingStarted
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/portableExperiences/actions.ts: DenyPortableExperiencesAction, KillAllPortableExperiencesAction, ActivateAllPortableExperiencesAction, AddKernelPortableExperience, AddScenePortableExperienceAction, RemoveScenePortableExperienceAction
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/portableExperiences/selectors.ts: getPortableExperienceDenyList, getPortableExperiencesCreatedByScenes, getKernelPortableExperiences
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/dao/index.ts: resolveRealmAboutFromBaseUrl
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/dao/pick-realm-algorithm/utils.ts: memoizedScores, latencyDeductions, scoreUsingLatencyDeductions, penalizeFull
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/dao/pick-realm-algorithm/types.ts: LargeLatencyConfig, ClosePeersScoreConfig, LatencyDeductionsConfig, LoadBalancingConfig, AllPeersScoreConfig
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/dao/actions.ts: WEB3_INITIALIZED, UPDATE_CATALYST_REALM, CATALYST_REALMS_SCAN_REQUESTED, CatalystRealmsScanRequested
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/comms/peers.ts: avatarVersionUpdateObservable, receiveUserVisible
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/comms/sceneSubscriptions.ts: getParcelSceneSubscriptions
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/comms/adapters/LivekitAdapter.ts: LivekitConfig
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/comms/actions.ts: SetCommsIsland
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/comms/sagas.ts: handleNewCommsContext, disconnectRoom
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/comms/interface/types.ts: UserMessage, AvatarExpression, PackageType
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/comms/interface/utils.ts: positionHashRfc4, sameParcel, gridSquareDistance, rotateUsingQuaternion
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/catalogs/sagas.ts: BASE_AVATARS_COLLECTION_ID
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/catalogs/actions.ts: catalogLoaded
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/catalogs/types.ts: Collection, RarityEnum, PartialEmote, BodyShapeRepresentation, EmoteId, ColorString
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/friends/utils.ts: DEFAULT_MAX_CHANNELS_VALUE, DEFAULT_MAX_NUMBER_OF_REQUESTS
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/friends/sagas.ts: initializeStatusUpdateInterval
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/friends/selectors.ts: getConversations
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/loadingScreen/actions.ts: UPDATE_LOADING_SCREEN
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/loadingScreen/selectors.ts: isLoadingScreenVisible, getParcelLoadingStarted
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/loadingScreen/sagas.ts: ACTIONS_FOR_LOADING
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/sceneEvents/sagas.ts: updateLocation
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/atlas/actions.ts: REPORTED_SCENES_FOR_MINIMAP, ReportedScenes
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/atlas/selectors.ts: EMPTY_PARCEL_NAME
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/referral/index.ts: getReferralEndpoint, saveReferral, referUser
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/realm/connections/BFFConnection.ts: TopicData, TopicListener, BffRpcConnection
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/voiceChat/VoiceCommunicator.ts: AudioCommunicatorChannel, StreamPlayingListener, StreamRecordingListener, StreamRecordingErrorListener, VoiceCommunicatorOptions
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/voiceChat/selectors.ts: isVoiceChatRecording, getVoicePolicy, isVoiceAllowedByPolicy
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/voiceChat/actions.ts: JoinVoiceChatAction, LeaveVoiceChatAction, SetVoiceChatHandlerAction, RequestVoiceChatRecordingAction, RequestToggleVoiceChatRecordingAction, setVoiceChatMute, SetVoiceChatPolicyAction
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/meta/types.ts: Ban, POI
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/meta/selectors.ts: getContentWhitelist, getMinCatalystVersion, isLiveKitVoiceChatFeatureFlag
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/meta/actions.ts: MetaConfigurationInitialized
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/profiles/index.ts: takeLatestByUserId, profileSaga, generateRandomUserProfile
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/profiles/selectors.ts: getCurrentUserProfileStatusAndData, getEthereumAddress, filterProfilesByUserNameOrId
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/profiles/types.ts: ProfileType
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/profiles/sagas/content/cachedRequest.ts: cachedRequests, requestCacheKey
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/profiles/sagas/handleDeployProfile.ts: deployAvatar
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/profiles/retrieveProfile.ts: getProfileIfExists
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/profiles/actions.ts: SAVE_PROFILE_SUCCESS, SAVE_PROFILE_FAILURE, DEPLOY_PROFILE_FAILURE, SEND_PROFILE_TO_RENDERER_SUCCESS, SaveProfileFailure, DeployProfileSuccess, addedProfileToCatalog, AddedProfileToCatalog, PROFILE_RECEIVED_OVER_COMMS, profileReceivedOverComms, ProfileReceivedOverComms
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/quests/client.ts: questsClient
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/types.ts: WearableId, Wearable, MessageDict, MessageEntry, IChatCommand, SceneStartedPayload, BillboardModes, TextureSamplingMode, TextureWrapping, TransparencyModes, SceneCommunications, SceneDisplay, SceneParcels, SceneContact, ScenePolicy, SceneSource, SceneSourcePlacement, SceneSpawnPoint, SoundComponent, TransitionValue, TimingFunction, TransitionComponent, SkeletalAnimationValue, SkeletalAnimationComponent, Ray, RayQuery, WelcomeHUDControllerModel, UnseenPrivateMessage, AvatarRendererBasePayload, AvatarRendererRemovedMessage
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/loading/reducer.ts: LoadingState
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/loading/sagas.ts: trackLoadTime, ACTIONS_FOR_LOADING
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/loading/ReportFatalError.ts: ReportFatalErrorWithCatalystPayload, ReportFatalErrorWithCommsPayload
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/loading/types.ts: sceneEntered, unexpectedError, commsCouldNotBeEstablished
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/loading/selectors.ts: getLoadingState, shouldWaitForScenes
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/loading/actions.ts: UpdateStatusMessage
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/session/actions.ts: InitSession, Logout, RedirectToSignUp, SIGNUP_CLEAR_DATA
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/session/getPerformanceInfo.ts: commsPerfObservable
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/session/sagas.ts: observeAccountStateChange
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/selectors.ts: getThumbnailUrlFromJsonData, getThumbnailUrlFromBuilderProjectId
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/analytics/types.ts: PositionTrackEvents, SegmentEvent
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/ethereum/ERC20.ts: IERC20
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/shared/ethereum/ERC721.ts: IERC721, getERC721
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/voice-chat-codec/constants.ts: OUTPUT_NODE_BUFFER_SIZE, OUTPUT_NODE_BUFFER_DURATION, INPUT_NODE_BUFFER_SIZE
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/voice-chat-codec/audioWorkletProcessors.ts: AudioWorkletProcessor
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/voice-chat-codec/VoiceChatCodecWorkerMain.ts: DecodeStream
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/data-structures/index.ts: RingBuffer, OrderedRingBuffer, SortedLimitedQueue
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/decentraland/profiles/names/index.ts: isBadWord
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/decentraland/profiles/transformations/index.ts: analizeColorPart, stripAlpha, convertToRGBObject, rgbToHex, convertSection, isValidBodyShape, fixWearableIds, calculateDisplayName, genericAvatarSnapshots, profileToRendererFormat, defaultProfile, ensureAvatarCompatibilityFormat, buildServerMetadata, AvatarForRenderer, NewProfileForRenderer, AddUserProfilesToCatalogPayload
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/decentraland/profiles/index.ts: AvatarForUserData, generateRandomUserProfile
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/decentraland/index.ts: * -> /lib/decentraland/authentication/signedFetch, * -> /lib/decentraland/authentication/authorizeBuilderHeaders, * -> /lib/decentraland/parcels/encodeParcelPosition, * -> /lib/decentraland/parcels/encodeParcelSceneBoundaries, * -> /lib/decentraland/parcels/getParcelSceneLimits, * -> /lib/decentraland/parcels/gridToWorld, * -> /lib/decentraland/parcels/IParcelSceneLimits, * -> /lib/decentraland/parcels/isAdjacent, * -> /lib/decentraland/parcels/isWorldPositionInsideParcels, * -> /lib/decentraland/parcels/limits, * -> /lib/decentraland/parcels/parseParcelPosition, * -> /lib/decentraland/parcels/worldToGrid, * -> /lib/decentraland/url/resolveURL
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/encoding/base64ToBlob.ts: base64ToBlob
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/encoding/crc32.ts: signedCRC32
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/logger/wrap.ts: METHODS, _console, default
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/logger/index.ts: createGenericLogComponent
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/logger/getStack.ts: getStack
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/logger/perf.ts: reset, tick, report
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/redux/index.ts: waitForAction, takeLatestById
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/math/Vector2.ts: isEqual
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/math/Vector3.ts: isEqual
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/math/Quaternion.ts: Quaternion
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/browser/cache.ts: init, cache, retrieve, store
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/browser/index.ts: asyncLocalStorage, setPersistentStorage, getPersistentStorage, saveToPersistentStorage, getFromPersistentStorage, removeFromPersistentStorage, getKeysFromPersistentStorage, untilNextFrame
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/javascript/spaces.ts: MAX_SPACES
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/javascript/colorString.ts: hexaNumber, colorString
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/lib/javascript/flatFetch.ts: FlatFetchResponse, BodyType
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/unity-interface/portableExperiencesUtils.ts: PortableExperienceHandle, killScenePortableExperience
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/unity-interface/nativeMessagesBridge.ts: NativeMessagesBridge
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/unity-interface/dcl.ts: hudWorkerUrl
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/unity-interface/BrowserInterface.ts: RendererSaveProfile, rendererSaveProfileSchemaV0, rendererSaveProfileSchemaV1, BrowserInterface
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/unity-interface/loader.ts: LoadRendererResult, UnityGame, RendererOptions, DecentralandRendererInstance
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/unity-interface/initializer.ts: InitializeUnityResult
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/unity-interface/protobufMessagesBridge.ts: ProtobufMessagesBridge
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/gif-processor/types.ts: ProcessorMessageType
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/kernel-web-interface/dcl-crypto.ts: Signature, EthAddress, IdentityType, AuthChain, AuthLink, AuthLinkType
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/kernel-web-interface/index.ts: AuthIdentity, KernelTrackingEvent, KernelError, KernelLoadingProgress, KernelAccountState, KernelSignUpEvent, KernelOpenUrlEvent, KernelRendererVisibleEvent, KernelLogoutEvent, KernelShutdownEvent
/home/usr/projects/decentraland/unity-renderer/browser-interface/packages/ui/avatar/avatarSystem.ts: AvatarEntity
