using UnityEngine.Assertions;

namespace MainScripts.DCL.Controllers.AssetManager.Font
{
    public readonly struct FontResponse
    {
        public readonly bool IsSuccess;
        public readonly FontSuccessResponse? SuccessResponse;
        public readonly FontFailResponse? FailResponse;

        public FontResponse(FontSuccessResponse successResponse) : this()
        {
            IsSuccess = true;
            SuccessResponse = successResponse;
        }

        public FontResponse(FontFailResponse failResponse) : this()
        {
            IsSuccess = false;
            FailResponse = failResponse;
        }

        public FontSuccessResponse GetSuccessResponse()
        {
            Assert.IsTrue(IsSuccess);
            return SuccessResponse.Value;
        }

        public FontFailResponse GetFailResponse()
        {
            Assert.IsFalse(IsSuccess);
            return FailResponse.Value;
        }
    }
}
