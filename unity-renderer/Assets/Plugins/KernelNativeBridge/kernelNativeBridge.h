typedef struct QueryPayload QueryPayload;
typedef struct RayQuery RayQuery;
typedef struct Vector3 Vector3;
typedef struct Ray Ray;

struct Vector3
{
    float x;
    float y;
    float z;
};

struct Ray
{
    Vector3 origin;
    Vector3 direction;
    float distance;
};

struct RayQuery
{
    int32_t queryType;
    Ray ray;
};

struct QueryPayload
{
    int32_t queryType;
    int32_t queryId;
    RayQuery payload;
};

typedef void (*callback_v)();
typedef void (*callback_vi)(int32_t a);
typedef void (*callback_vf)(float a);
typedef void (*callback_vs)(const char *a);
typedef void (*callback_vis)(int32_t, const char *b);
typedef void (*callback_vss)(const char *a, const char *b);
typedef void (*callback_vsss)(const char *a, const char *b, const char *c);
typedef void (*callback_vv3)(struct Vector3);
typedef void (*callback_vx)(int32_t,int32_t,struct Vector3,struct Vector3);
typedef void (*callback_query)(struct QueryPayload);
typedef void (*callback_vii)(int32_t a, int32_t b);

typedef int32_t (*callback_I)();
typedef float (*callback_F)();
typedef const char * (*callback_S)();






