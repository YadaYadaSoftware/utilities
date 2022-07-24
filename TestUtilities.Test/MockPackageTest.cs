using Amazon.S3;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Xunit;

namespace TestUtilities.Test;

public class MockPackageTest
{
    [Fact]
    public void ReplacementTest()
    {
        using var p = new MockPackage<MyTestClass>();
        p.AddFake<IAmazonS3>(new FakeS3Client(new DirectoryInfo("x")));
        p.S3.Should().BeOfType<FakeS3Client>();
        p.GetRequiredService<IAmazonS3>().Should().NotBeNull();
    }

    public class MyTestClass
    { }

        
}