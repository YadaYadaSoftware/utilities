using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace YadaYada.TestUtilities;

public static class ActionResultExtensions
{
    public static TActionResult Unwrap<TResult, TActionResult>(this ActionResult<TActionResult> result) where TResult : ObjectResult
    {
        return result.Should().BeOfType<ActionResult<TActionResult>>().Subject.Result.Should().BeOfType<TResult>().Subject.Value.Should().BeAssignableTo<TActionResult>().Subject;
    }
}