using Ziggio.Identity.Domain.Results;

namespace Ziggio.Identity.Infrastructure.Extensions;

public static class ResultExtensions {
  public static bool IsSuccessful(this in Result result) => result.ResultType == ResultType.Success;
  public static bool IsSuccessful<T>(this in Result<T> result) => result.ResultType == ResultType.Success;
  public static bool IsNotFound(this in Result result) => result.ResultType == ResultType.NotFound;
  public static bool IsNotFound<T>(this in Result<T> result) => result.ResultType == ResultType.NotFound;
  public static bool IsUnauthorized(this in Result result) => result.ResultType == ResultType.Unauthorized;
  public static bool IsUnauthorized<T>(this in Result<T> result) => result.ResultType == ResultType.Unauthorized;
  public static bool HasErrors(this in Result result) => result.ResultType == ResultType.Error;
  public static bool HasErrors<T>(this in Result<T> result) => result.ResultType == ResultType.Error;
}