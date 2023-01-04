namespace DCLServices.Lambdas
{
    public static class LambdaPaginatedResponseHelper
    {
        private const string PAGE_SIZE_PARAM_NAME = "pageSize";
        private const string PAGE_NUM_PARAM_NAME = "pageNum";

        public static (string, string) GetPageSizeParam(int size) =>
            (PAGE_SIZE_PARAM_NAME, size.ToString());

        public static (string, string) GetPageNumParam(int pageNum) =>
            (PAGE_NUM_PARAM_NAME, pageNum.ToString());
    }
}
