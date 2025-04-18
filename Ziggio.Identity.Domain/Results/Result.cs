using System.Collections.Immutable;

namespace Ziggio.Identity.Domain.Results;

public readonly struct Result<T> {
  public readonly T Data { get; }
  public readonly ResultType ResultType { get; }
  public readonly ImmutableArray<string> Errors { get; }

  public Result(
    T data,
    ResultType resultType,
    ImmutableArray<string> errors
  ) {
    Data = data;
    ResultType = resultType;
    Errors = errors;
  }

  public static implicit operator Result<T>(Result result) =>
    new Result<T>(default, result.ResultType, result.Errors);
}

public readonly struct Result {
  public static Result Success() => new Result(ResultType.Success, default);
  public static Result<T> Success<T>(T data) => new Result<T>(data, default, default);
  public static Result NotFound(params string[] errors) => new Result(ResultType.NotFound, ImmutableArray.Create<string>(errors));
  public static Result NotFound(ImmutableArray<string> errors) => new Result(ResultType.NotFound, errors);
  public static Result Unauthorized(params string[] errors) => new Result(ResultType.Unauthorized, ImmutableArray.Create<string>(errors));
  public static Result Unauthorized(ImmutableArray<string> errors) => new Result(ResultType.Unauthorized, errors);
  public static Result Error(params string[] errors) => new Result(ResultType.Error, ImmutableArray.Create<string>(errors));
  public static Result Error(ImmutableArray<string> errors) => new Result(ResultType.Error, errors);

  public readonly ResultType ResultType { get; }
  public readonly ImmutableArray<string> Errors { get; }

  public Result(
    ResultType resultType,
    ImmutableArray<string> errors
  ) {
    ResultType = resultType;
    Errors = errors;
  }
}