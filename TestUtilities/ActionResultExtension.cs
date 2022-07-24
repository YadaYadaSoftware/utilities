using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace TestUtilities;

public static class ActionResultExtension
{
    public static TObjectResult UnwrapResult<TObjectResult>(this ActionResult result) where TObjectResult : ObjectResult
    {
        return result.As<TObjectResult>();
    }
    public static TPayload UnwrapResult<TObjectResult, TPayload>(this ActionResult result) where TObjectResult : ObjectResult
    {
        return result.As<TObjectResult>().Value.As<TPayload>();
    }

}