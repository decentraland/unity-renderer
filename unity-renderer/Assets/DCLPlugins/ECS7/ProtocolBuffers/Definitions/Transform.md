
# Transform

Transform is not a protocol-buffer definition, because it uses the native byte-buffer to write directly on the wire. This allows the consumer to copy the message directly on memory and the component be serialized a little bit faster.

```cpp
// Transform length = 44 
struct Transform {
    float positionX;
    float positionY;
    float positionZ;

    float rotationX;
    float rotationY;
    float rotationZ;
    float rotationW;

    float scaleX;
    float scaleY;
    float scaleZ;

    uint32_t parentEntity;
};
```
- Serialized in big-endian