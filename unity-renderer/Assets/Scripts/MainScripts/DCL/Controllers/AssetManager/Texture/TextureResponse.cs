using UnityEngine;
using UnityEngine.Assertions;

namespace MainScripts.DCL.Controllers.AssetManager.Texture
{
    public readonly struct TextureResponse
    {
        public readonly bool IsSuccess;
        public readonly TextureSuccessResponse? SuccessResponse;
        public readonly TextureFailResponse? FailResponse;

        public TextureResponse(TextureSuccessResponse successResponse) : this()
        {
            IsSuccess = true;
            SuccessResponse = successResponse;
        }

        public TextureResponse(TextureFailResponse failResponse) : this()
        {
            IsSuccess = false;
            FailResponse = failResponse;
        }

        public TextureSuccessResponse GetSuccessResponse()
        {
            Assert.IsTrue(IsSuccess);
            return SuccessResponse.Value;
        }

        public TextureFailResponse GetFailResponse()
        {
            Assert.IsFalse(IsSuccess);
            return FailResponse.Value;
        }
    }
}
