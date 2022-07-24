using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace YadaYada.TestUtilities;

public static class ActionResultExtensions
{
    public static TObjectResult UnwrapResult<TObjectResult>(this ActionResult result) where TObjectResult : ObjectResult
    {
        return result.As<TObjectResult>();
    }
    public static TPayload UnwrapResult<TObjectResult, TPayload>(this ActionResult result) where TObjectResult : ObjectResult
    {
        return result.As<TObjectResult>().Value.As<TPayload>();
    }

    public static TActionResult Unwrap<TResult, TActionResult>(this ActionResult<TActionResult> result) where TResult : ObjectResult
    {
        return result.Should().BeOfType<ActionResult<TActionResult>>().Subject.Result.Should().BeOfType<TResult>().Subject.Value.Should().BeAssignableTo<TActionResult>().Subject;
    }
}