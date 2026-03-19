namespace DotnetEnterpriseApi.Application.Common.Models
{
    public class Result
    {
        public bool IsSuccess { get; }
        public string Message { get; }
        public string[] Errors { get; }

        protected Result(bool isSuccess, string message, string[] errors)
        {
            IsSuccess = isSuccess;
            Message = message;
            Errors = errors ?? Array.Empty<string>();
        }

        public static Result Success(string message = "Operation completed successfully")
        {
            return new Result(true, message, Array.Empty<string>());
        }

        public static Result Failure(string message, params string[] errors)
        {
            return new Result(false, message, errors);
        }

        public static Result Failure(string[] errors)
        {
            return new Result(false, "Operation failed", errors);
        }
    }

    public class Result<T> : Result
    {
        public T? Data { get; }

        protected Result(bool isSuccess, T? data, string message, string[] errors)
            : base(isSuccess, message, errors)
        {
            Data = data;
        }

        public static Result<T> Success(T data, string message = "Operation completed successfully")
        {
            return new Result<T>(true, data, message, Array.Empty<string>());
        }

        public new static Result<T> Failure(string message, params string[] errors)
        {
            return new Result<T>(false, default, message, errors);
        }

        public new static Result<T> Failure(string[] errors)
        {
            return new Result<T>(false, default, "Operation failed", errors);
        }
    }
}
