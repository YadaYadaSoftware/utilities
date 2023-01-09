using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Amazon.S3;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Language.Flow;
using IServiceProvider = System.IServiceProvider;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]


namespace YadaYada.TestUtilities;

public class MockPackage<TTarget> : IServiceProvider, IDisposable, IServiceCollection, ISupportRequiredService where TTarget : class
{

    public object activateLock = new object();

    public MockPackage(Func<Mock<TTarget>?> targetMock)
    {
        this.AddMock(typeof(TTarget), targetMock.Invoke());
    }

    public MockPackage(params KeyValuePair<Type, object>[]? fakes)
    {
        var serviceScopeFactory = this.GetMock<IServiceScopeFactory>();
        var scope = this.GetMock<IServiceScope>();
        scope.Setup(z => z.ServiceProvider).Returns(this);
        serviceScopeFactory.Setup(z => z.CreateScope()).Returns(scope.Object);
        var loggingProvider = this.GetMock<ILoggerProvider>();
        var mockLogger = new Mock<ILogger>();
        mockLogger.Setup(_ => _.BeginScope(It.IsAny<string>())).Returns(new Mock<IDisposable>().Object);
        loggingProvider.Setup(_ => _.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        var factory = this.GetMock<ILoggerFactory>();
        factory.Setup(_ => _.CreateLogger(It.IsAny<string>())).Returns(mockLogger.Object);
        if (fakes == null) return;

        foreach (var (key, value) in fakes)
        {
            this._descriptors.Add(new ServiceDescriptor(key, value));
        }
    }

    private Mock? CreateMock(Type mockOfType)
    {

        Mock? returnValue = null;

        var mockType = typeof(Mock<>).MakeGenericType(mockOfType);

        if (mockOfType.IsInterface)
        {
            lock (activateLock)
            {
                returnValue = (Mock) Activator.CreateInstance(mockType);
            }
        }
        else
        {
            var constructorInfos = mockOfType.GetConstructors(BindingFlags.CreateInstance|BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public).OrderByDescending(z => z.GetParameters().Length).ThenByDescending(_=>_.GetCustomAttributes<PreferredMockConstructorAttribute>().Count());


            // ReSharper disable once PossibleMultipleEnumeration
            if (!constructorInfos.Any())
            {
                lock (activateLock)
                {
                    return (Mock) Activator.CreateInstance(mockType)!;
                }
            }

            var i = 0;

            foreach (var constructorInfo in constructorInfos)
            {
                i++;
                if (constructorInfo.GetParameters().Any(p=>p.ParameterType.IsPrimitive || p.ParameterType == typeof(string))) continue;
                returnValue = this.ConstructFrom(constructorInfo, mockType);
                if (returnValue is { }) break;
                // ReSharper disable once PossibleMultipleEnumeration
                if (i==constructorInfos.Count()) throw new InvalidOperationException($"Cannot find a ctor for {mockOfType}");
            }
        }

        ArgumentNullException.ThrowIfNull(returnValue,nameof(returnValue));

        foreach (var propertyInfo in mockOfType.GetProperties().Where(_=>_.GetCustomAttribute<InjectAttribute>() is {}))
        {
            var mock = this.GetMock(propertyInfo.PropertyType);
            ArgumentNullException.ThrowIfNull(mock);
            propertyInfo.SetValue(returnValue.Object, mock.Object);
        }

        return returnValue;

    }

    private Mock? ConstructFrom(ConstructorInfo constructorInfo, Type mockType)
    {
        Mock? returnValue = null;
        var parameterInfos = constructorInfo.GetParameters();

        var arguments = new List<object>();

        try
        {
            foreach (var parameterInfo in parameterInfos)
            {
                if (parameterInfo.ParameterType == typeof(IServiceProvider) || parameterInfo.ParameterType == typeof(IServiceCollection))
                {
                    arguments.Add(this);

                }
                else
                {
                    arguments.Add(this.GetFake(parameterInfo.ParameterType));

                }
            }

            returnValue = (Mock)Activator.CreateInstance(mockType, (object[])arguments.ToArray());


            //switch (parameterInfos.Length)
            //{
            //    case 0:
            //        lock (this.activateLock)
            //        {
            //            returnValue = (Mock)Activator.CreateInstance(mockType);
            //        }

            //        returnValue.CallBase = true;
            //        break;
            //    case 1:
            //        lock (this.activateLock)
            //        {
            //            returnValue = (Mock)Activator.CreateInstance(mockType, arguments[0]);
            //        }

            //        returnValue.CallBase = true;

            //        break;
            //    case 2:
            //        lock (this.activateLock)
            //        {
            //            returnValue = (Mock)Activator.CreateInstance(mockType, arguments[0], arguments[1]);
            //        }

            //        returnValue.CallBase = true;
            //        break;
            //    case 3:
            //        lock (this.activateLock)
            //        {
            //            returnValue =
            //                (Mock)Activator.CreateInstance(mockType, arguments[0], arguments[1],
            //                    arguments[2]);
            //        }

            //        returnValue.CallBase = true;
            //        break;
            //    case 4:
            //        lock (this.activateLock)
            //        {
            //            returnValue = (Mock)Activator.CreateInstance(mockType, arguments[0],
            //                arguments[1],
            //                arguments[2], arguments[3]);
            //        }

            //        returnValue.CallBase = true;
            //        break;
            //    case 5:
            //        lock (this.activateLock)
            //        {
            //            returnValue = (Mock)Activator.CreateInstance(mockType, arguments[0], arguments[1],
            //                arguments[2], arguments[3], arguments[4]);
            //        }

            //        returnValue.CallBase = true;
            //        break;
            //    case 6:
            //        lock (this.activateLock)
            //        {
            //            returnValue = (Mock)Activator.CreateInstance(mockType, arguments[0],
            //                arguments[1],
            //                arguments[2], arguments[3], arguments[4], arguments[5]);
            //        }

            //        returnValue.CallBase = true;
            //        break;
            //    case 7:
            //        lock (this.activateLock)
            //        {
            //            returnValue = (Mock)Activator.CreateInstance(mockType, arguments[0],
            //                arguments[1],
            //                arguments[2], arguments[3], arguments[4], arguments[5], arguments[6]);
            //        }

            //        returnValue.CallBase = true;
            //        break;
            //    default:
            //        throw new NotSupportedException("Too many parameters for ctor");
            //}

            return returnValue;
        }
        catch (Exception e)
        {
            // ignore
        }

        return returnValue;
    }

    private Mock<TMockOf> CreateMock<TMockOf>() where TMockOf : class
    {
        return (Mock<TMockOf>)this.CreateMock(typeof(TMockOf));
    }

    public virtual Mock<TTarget> TargetMock
    {
        get
        {
            var targetMock = this.GetMock<TTarget>();
            if (targetMock == null)
            {
                targetMock = this.CreateMock<TTarget>();
                this.AddMock(typeof(TTarget), targetMock);
            }

            return targetMock;
        }
    }

    public TTarget Target => this.TargetMock.Object;

    protected readonly IServiceCollection _descriptors = new ServiceCollection();
    private readonly Dictionary<Type, Mock?> _mocks = new Dictionary<Type, Mock?>();

    public void AddMock(Type mockOf, Mock? mock)
    {
        this._descriptors.Add(new ServiceDescriptor(mockOf, mock.Object));
        this._mocks.Add(mockOf, mock);
    }

    public Mock<T> AddMock<T>() where T : class
    {
        return this.GetMock<T>();
    }




    public Mock? GetMock([NotNull] Type mockType)
    {
        if (mockType == null) throw new ArgumentNullException(nameof(mockType));

        ServiceDescriptor descriptor = this._descriptors.SingleOrDefault(d => d.ServiceType == mockType);


        if (descriptor == null)
        {
            var mock = this.CreateMock(mockType);
            mock.CallBase = true;
            this.AddMock(mockType, mock);
        }


        return this._mocks[mockType];

    }

    public Mock<TMock> GetMock<TMock>() where TMock : class
    {
        return (Mock<TMock>)this.GetMock(typeof(TMock));

    }

    public object GetFake(Type fakeType)
    {
        if (fakeType == typeof(IServiceProvider)) return this;
        var returnValue = this._descriptors.SingleOrDefault(d => d.ServiceType == fakeType)?.ImplementationInstance;
        if (returnValue == null)
        {
            returnValue = this.GetMock(fakeType).Object;
        }

        return returnValue;
    }

    public T GetFake<T>()
    {
        return (T)this.GetFake(typeof(T));
    }

    public T AddFake<T>(T fake) where T : class
    {
        if (this._descriptors.SingleOrDefault(d => d.ServiceType == typeof(T)) is { } removeMe)
        {
            this._descriptors.Remove(removeMe);

        }
        this._descriptors.Add(new ServiceDescriptor(typeof(T), fake));
        return fake;
    }

    public IAmazonS3 S3 => this.GetService<IAmazonS3>();

    public Mock<IAmazonS3> S3Mock => Mock.Get(this.S3);

    public IConfiguration Configuration => this.ConfigurationMock.Object;

    public Mock<IConfiguration> ConfigurationMock => this.GetMock<IConfiguration>();

    public void Verify()
    {
        foreach (var mock in this._mocks.Values)
        {
            mock.Verify();
        }
    }


    public void Dispose()
    {
        if (Marshal.GetExceptionCode() != 0) return;
        this.Verify();
    }


    public object GetService(Type serviceType)
    {
        var descriptor = this._descriptors.SingleOrDefault(d => d.ServiceType == serviceType);
        if (descriptor == null)
        {
            //Mock mock = this.GetMock(serviceType);

            //this.AddMock(serviceType, mock);
            this.GetMock(serviceType);

        }

        return this._descriptors.Single(d => d.ServiceType == serviceType).ImplementationInstance;
    }

    public object GetRequiredService(Type serviceType)
    {
        return GetService(serviceType);
    }

    public IEnumerator<ServiceDescriptor> GetEnumerator()
    {
        return this._descriptors.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return this._descriptors.GetEnumerator();
    }

    public void Add(ServiceDescriptor item)
    {
        this._descriptors.Add(item);
    }

    public void Clear()
    {
        this._descriptors.Clear();
    }

    public bool Contains(ServiceDescriptor item)
    {
        return this._descriptors.Contains(item);
    }

    public void CopyTo(ServiceDescriptor[] array, int arrayIndex)
    {
        this._descriptors.CopyTo(array, arrayIndex);
    }

    public bool Remove(ServiceDescriptor item)
    {
        return this._descriptors.Remove(item);
    }

    public int Count => this._descriptors.Count;
    public bool IsReadOnly => this._descriptors.IsReadOnly;
    public int IndexOf(ServiceDescriptor item)
    {
        return this._descriptors.IndexOf(item);
    }

    public void Insert(int index, ServiceDescriptor item)
    {
        this._descriptors.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        this._descriptors.RemoveAt(index);
    }

    public ServiceDescriptor this[int index]
    {
        get => this._descriptors[index];
        set => this._descriptors[index] = value;
    }

    public ISetup<TTarget, TResult> Setup<TResult>(Expression<Func<TTarget, TResult>> expression)
    {
        return this.TargetMock.Setup(expression);
    }

    public ISetup<TTarget, Task<TResult>> SetupAsync<TResult>(Expression<Func<TTarget, TResult>> expression)
    {
        return (ISetup<TTarget, Task<TResult>>)this.TargetMock.Setup(expression);
    }
    public ISetup<TTarget, ValueTask<TResult>> SetupValueAsync<TResult>(Expression<Func<TTarget, ValueTask<TResult>>> expression)
    {
        return this.TargetMock.Setup(expression);
    }

    public ISetup<TTarget> Setup(Expression<Action<TTarget>> expression)
    {
        return this.TargetMock.Setup(expression);
    }

    public ISetup<TMock, TResult> Setup<TMock, TResult>(Expression<Func<TMock, TResult>> expression) where TMock : class
    {
        var mock = this.GetMock<TMock>();
        return mock.Setup(expression);
    }

}

[AttributeUsage(AttributeTargets.Constructor)]
public class PreferredMockConstructorAttribute : Attribute
{
}

public class MockPackage<TTarget, TMock0> : MockPackage<TTarget> where TMock0 : class where TTarget : class
{
    public MockPackage(TMock0 mock0) : base(new KeyValuePair<Type, object>[] { new KeyValuePair<Type, object>(typeof(TMock0), mock0) })
    {

    }
}


public class MockPackage<TTarget, TMock0, TMock1> : MockPackage<TTarget, TMock0> where TTarget : class where TMock0 : class where TMock1 : class
{
    public MockPackage(TMock0 mock0, TMock1 mock1) : base(mock0)
    {
        this._descriptors.Add(new ServiceDescriptor(typeof(TMock1), mock1));
    }
}
public class MockPackage<TTarget, TMock0, TMock1, TMock2> : MockPackage<TTarget, TMock0, TMock1> where TTarget : class where TMock0 : class where TMock1 : class where TMock2 : class
{
    public MockPackage(TMock0 mock0, TMock1 mock1, TMock2 mock2) : base(mock0, mock1)
    {
        this._descriptors.Add(new ServiceDescriptor(typeof(TMock2), mock2));

    }
}
public class MockPackage<TTarget, TMock0, TMock1, TMock2, TMock3> : MockPackage<TTarget, TMock0, TMock1, TMock2> where TTarget : class where TMock0 : class where TMock1 : class where TMock2 : class where TMock3 : class
{
    public MockPackage(TMock0 mock0, TMock1 mock1, TMock2 mock2, TMock3 mock3) : base(mock0, mock1, mock2)
    {
        this._descriptors.Add(new ServiceDescriptor(typeof(TMock3), mock3));

    }
}
public class MockPackage<TTarget, TMock0, TMock1, TMock2, TMock3, TMock4> : MockPackage<TTarget, TMock0, TMock1, TMock2, TMock3> where TTarget : class where TMock0 : class where TMock1 : class where TMock2 : class where TMock3 : class where TMock4 : class
{
    public MockPackage(TMock0 mock0, TMock1 mock1, TMock2 mock2, TMock3 mock3, TMock4 mock4) : base(mock0, mock1, mock2, mock3)
    {
        this._descriptors.Add(new ServiceDescriptor(typeof(TMock4), mock4));

    }
}

public class MockPackage<TTarget, TMock0, TMock1, TMock2, TMock3, TMock4, TMock5> : MockPackage<TTarget, TMock0, TMock1, TMock2, TMock3, TMock4> where TTarget : class where TMock0 : class where TMock1 : class where TMock2 : class where TMock3 : class where TMock4 : class where TMock5 : class
{
    public MockPackage(TMock0 mock0, TMock1 mock1, TMock2 mock2, TMock3 mock3, TMock4 mock4, TMock5 mock5) : base(mock0, mock1, mock2, mock3, mock4)
    {
        this._descriptors.Add(new ServiceDescriptor(typeof(TMock5), mock5));

    }
}
public class MockPackage<TTarget, TMock0, TMock1, TMock2, TMock3, TMock4, TMock5, TMock6> : MockPackage<TTarget, TMock0, TMock1, TMock2, TMock3, TMock4, TMock5> where TTarget : class where TMock0 : class where TMock1 : class where TMock2 : class where TMock3 : class where TMock4 : class where TMock5 : class where TMock6 : class
{
    public MockPackage(TMock0 mock0, TMock1 mock1, TMock2 mock2, TMock3 mock3, TMock4 mock4, TMock5 mock5, TMock6 mock6) : base(mock0, mock1, mock2, mock3, mock4, mock5)
    {
        this._descriptors.Add(new ServiceDescriptor(typeof(TMock6), mock6));

    }
}