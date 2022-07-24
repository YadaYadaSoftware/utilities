
using System;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace TestUtilities.Test;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var p = new MockPackage<MyClassToMock, MyDbContext>();
        const string value = "Xyz";
        p.ContextMock.Setup(z => z.DoSomethingElse()).Returns(value);
        var r = p.Target.DoSomething();
        r.Should().Be(value);
    }

    public class MyDbContext : DbContext
    {
        public virtual string DoSomethingElse()
        {
            return string.Empty;
        }
    }

    public class MyClassToMock
    {
        public MyDbContext Context { get; }

        public MyClassToMock(MyDbContext context)
        {
            Context = context;
        }

        public string DoSomething()
        {
            return this.Context.DoSomethingElse();
        }

    }
}