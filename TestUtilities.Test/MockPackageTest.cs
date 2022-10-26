using Amazon.S3;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using YadaYada.TestUtilities;

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

    [Fact]
    public void FactoryTest()
    {
        const string myValue = "MyValue";

        using var p = new MockPackage<MyTestClass2>(() => new Mock<MyTestClass2>(myValue));

        

        p.Target.Value.Should().Be(myValue);
    }


    private void Factory()
    {
        throw new NotImplementedException();
    }


    public class MyTestClass2
    {
        public string Value { get; }

        public MyTestClass2(string value)
        {
            Value = value;
        }
    }

        
}