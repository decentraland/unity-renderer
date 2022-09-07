#include <stdint.h>
#include "emscripten.h"
#include "kernelNativeBridge.h"

#define EXTERNAL_CALLBACK_V(NAME_PARAM)\
callback_v cb_ ## NAME_PARAM;\
void SetCallback_ ## NAME_PARAM(callback_v NAME_PARAM) {\
cb_ ## NAME_PARAM = NAME_PARAM;\
}\
void EMSCRIPTEN_KEEPALIVE call_ ## NAME_PARAM() {\
	cb_ ## NAME_PARAM();\
}\

#define EXTERNAL_CALLBACK_VS(NAME_PARAM)\
callback_vs cb_ ## NAME_PARAM;\
\
void SetCallback_ ## NAME_PARAM(callback_vs NAME_PARAM) {\
cb_ ## NAME_PARAM = NAME_PARAM;\
}\
void EMSCRIPTEN_KEEPALIVE call_ ## NAME_PARAM(char* a) {\
	cb_ ## NAME_PARAM(a);\
}\

#define EXTERNAL_CALLBACK_VSS(NAME_PARAM)\
callback_vss cb_ ## NAME_PARAM;\
void SetCallback_ ## NAME_PARAM(callback_vss NAME_PARAM) {\
cb_ ## NAME_PARAM = NAME_PARAM;\
}\
void EMSCRIPTEN_KEEPALIVE call_ ## NAME_PARAM(char* a, char* b) {\
	cb_ ## NAME_PARAM(a, b);\
}\

#define EXTERNAL_CALLBACK_VSSS(NAME_PARAM)\
callback_vsss cb_ ## NAME_PARAM;\
void SetCallback_ ## NAME_PARAM(callback_vsss NAME_PARAM) {\
cb_ ## NAME_PARAM = NAME_PARAM;\
}\
void EMSCRIPTEN_KEEPALIVE call_ ## NAME_PARAM(char* a, char* b, char* c) {\
	cb_ ## NAME_PARAM(a, b, c);\
}\

#define EXTERNAL_CALLBACK_VIS(NAME_PARAM)\
callback_vis cb_ ## NAME_PARAM;\
void SetCallback_ ## NAME_PARAM(callback_vis NAME_PARAM) {\
cb_ ## NAME_PARAM = NAME_PARAM;\
}\
void EMSCRIPTEN_KEEPALIVE call_ ## NAME_PARAM(int32_t a, char* b) {\
	cb_ ## NAME_PARAM(a, b);\
}\

#define EXTERNAL_CALLBACK_QUERY(NAME_PARAM)\
callback_query cb_ ## NAME_PARAM;\
void SetCallback_ ## NAME_PARAM(callback_query NAME_PARAM) {\
cb_ ## NAME_PARAM = NAME_PARAM;\
}\
void EMSCRIPTEN_KEEPALIVE call_ ## NAME_PARAM(QueryPayload query) {\
	cb_ ## NAME_PARAM(query);\
}\

#define EXTERNAL_CALLBACK_VII(NAME_PARAM)\
callback_vii cb_ ## NAME_PARAM;\
void SetCallback_ ## NAME_PARAM(callback_vii NAME_PARAM) {\
cb_ ## NAME_PARAM = NAME_PARAM;\
}\
void EMSCRIPTEN_KEEPALIVE call_ ## NAME_PARAM(int32_t a, int32_t b) {\
	cb_ ## NAME_PARAM(a, b);\
}\


EXTERNAL_CALLBACK_V(CreateEntity)
EXTERNAL_CALLBACK_V(RemoveEntity)
EXTERNAL_CALLBACK_V(SceneReady)
EXTERNAL_CALLBACK_VS(SetEntityId)
EXTERNAL_CALLBACK_VS(SetSceneId)
EXTERNAL_CALLBACK_VS(SetSceneNumber)
EXTERNAL_CALLBACK_VS(SetTag)
EXTERNAL_CALLBACK_VS(SetEntityParent)
EXTERNAL_CALLBACK_VIS(EntityComponentCreateOrUpdate)
EXTERNAL_CALLBACK_VS(EntityComponentDestroy)
EXTERNAL_CALLBACK_VIS(SharedComponentCreate)
EXTERNAL_CALLBACK_VSS(SharedComponentAttach)
EXTERNAL_CALLBACK_VSS(SharedComponentUpdate)
EXTERNAL_CALLBACK_VS(SharedComponentDispose)
EXTERNAL_CALLBACK_VSSS(OpenNftDialog)
EXTERNAL_CALLBACK_VS(OpenExternalUrl)
EXTERNAL_CALLBACK_QUERY(Query)
EXTERNAL_CALLBACK_VII(BinaryMessage)