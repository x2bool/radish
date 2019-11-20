namespace Rbmk.Utils.Apis
{
    public class ApiResult<T>
    {
        public T Data { get; set; }
        
        public ApiError[] Errors { get; set; }
    }
}