using Amazon.S3;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using YadaYada.TestUtilities;

namespace TestUtilities.Test;

public class MockPackageTest
{
    public class ClassWithStringInOneCtor
    {
        public ClassWithStringInOneCtor(string myString, string myString2)
        {

        }

        public ClassWithStringInOneCtor(AnotherClassToInject anotherClassToInject)
        {

        }
    }

    public class AnotherClassToInject
    {

    }


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

    public class MyTestClass2
    {
        public string Value { get; }

        public MyTestClass2(string value)
        {
            Value = value;
        }
    }

    [Fact]
    public void FactoryTest()
    {
        const string myValue = "MyValue";

        using var p = new MockPackage<MyTestClass2>(() => new Mock<MyTestClass2>(myValue));

        

        p.Target.Value.Should().Be(myValue);
    }



    [Fact]
    public void StringCtorTest()
    {
        using var p = new MockPackage<ClassWithStringInOneCtor>();
        p.TargetMock.Should().NotBeNull();
    }

    [Fact]
    public void SetupMockTest()
    {
        using var p = new MockPackage<LoggerClass>();
        p.SetupMock<ILogger>(mock => mock.BeginScope("x")).Verifiable();
        p.Target.Log();
    }

    public class LoggerClass
    {
        private readonly ILogger _logger;

        public LoggerClass(ILogger logger)
        {
            _logger = logger;
        }
        public void Log()
        {
            _logger.BeginScope("x");
        }
    }


        
}