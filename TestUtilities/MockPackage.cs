using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Amazon.S3;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
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

    public MockPackage(params KeyValuePair<Type, object>[] fakes)
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
        if (fakes != null)
        {
            foreach (var (key, value) in fakes)
            {
                if (value == default)
                {
                    //var mockType = typeof(Mock<>).MakeGenericType(key);
                    //Mock mock = (Mock) Activator.CreateInstance(mockType);
                    //this._mocks.Add(key, mock);
                    ////var x = (Activator.CreateInstance(mockType) as Mock).Object;

                    this.GetMock(key);

                }
                else
                {
                    this._descriptors.Add(new ServiceDescriptor(key, value));

                }
            }
        }
    }

    private Mock CreateMock(Type mockOfType)
    {

        Mock returnValue = null;

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

            if (constructorInfos.Any())
            {
                foreach (var constructorInfo in constructorInfos)
                {
                    returnValue = this.ConstructFrom(constructorInfo, mockType);
                    if (returnValue is {}) break;
                }

            }
            else
            {
                lock (activateLock)
                {
                    return (Mock) Activator.CreateInstance(mockType);
                }
            }

        }

        foreach (var propertyInfo in mockOfType.GetProperties().Where(_=>_.GetCustomAttribute<InjectAttribute>() is {}))
        {
            propertyInfo.SetValue(returnValue.Object, this.GetMock(propertyInfo.PropertyType).Object);
        }

        return returnValue;

    }

    private Mock ConstructFrom(ConstructorInfo constructorInfo, Type mockType)
    {
        Mock returnValue = null;
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
    private readonly Dictionary<Type, Mock> _mocks = new Dictionary<Type, Mock>();

    public void AddMock(Type mockOf, Mock mock)
    {
        this._descriptors.Add(new ServiceDescriptor(mockOf, mock.Object));
        this._mocks.Add(mockOf, mock);
    }

    public Mock<T> AddMock<T>() where T : class
    {
        return this.GetMock<T>();
    }




    public Mock GetMock([NotNull] Type mockType)
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

        //ServiceDescriptor descriptor = this._descriptors.SingleOrDefault(d => d.ServiceType == typeof(TMock));


        //if (descriptor == null)
        //{
        //    var mock = this.CreateMock(typeof(TMock));
        //    mock.CallBase = true;
        //    this.AddMock(typeof(TMock), mock);
        //}

        //descriptor = this._descriptors.SingleOrDefault(d => d.ServiceType == typeof(TMock));

        ////return Mock<IServiceScopeFactory>.Get((IServiceScopeFactory)descriptor.ImplementationInstance);

        //return Mock<TMock>.Get((TMock) descriptor.ImplementationInstance);

        ////Type mockedType = typeof(Mock<>);
        ////var makeGenericType = mockedType.MakeGenericType(mockedType);

        ////Mock returnValue = (Mock)makeGenericType.InvokeMember("Get", BindingFlags.Public | BindingFlags.Static | BindingFlags.InvokeMethod, null, null, new object[] { descriptor.ImplementationInstance });



        ////return returnValue;
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

    public ISetup<T, TResult> Setup<T, TResult>(Expression<Func<T, TResult>> expression) where T : class
    {
        return this.GetMock<T>().Setup(expression);
    }

    public ISetup<T, Task<TResult>> SetupAsync<T, TResult>(Expression<Func<T, TResult>> expression) where T : class
    {
        return (ISetup<T, Task<TResult>>)this.GetMock<T>().Setup(expression);
    }
    public ISetup<T, ValueTask<TResult>> SetupValueAsync<T, TResult>(Expression<Func<T, ValueTask<TResult>>> expression) where T : class
    {
        return (ISetup<T, ValueTask<TResult>>)this.GetMock<T>().Setup(expression);
    }

}

[AttributeUsage(AttributeTargets.Constructor)]
public class PreferredMockConstructorAttribute : Attribute
{
}

public class MockPackage<TTarget, TMock0> : MockPackage<TTarget> where TMock0 : class where TTarget : class
{

    public Mock<TMock0> ContextMock => this.GetMock<TMock0>();
    public TMock0 Context
    {
        get
        {
            if (this._descriptors.SingleOrDefault(_ => _.ServiceType == typeof(TMock0)) is { } serviceDescriptor)
            {
                return (TMock0)serviceDescriptor.ImplementationInstance;

            }
            return this.ContextMock.Object;
        }
    }

    public MockPackage(TMock0 context = null) : base(new KeyValuePair<Type, object>[] { new KeyValuePair<Type, object>(typeof(TMock0), context) })
    {

    }

}


public class MockPackage<TTarget, TContext, TMock1> : MockPackage<TTarget, TContext> where TTarget : class where TContext : DbContext
{
    public MockPackage(TContext context = null, TMock1 mock1 = default) : base(context)
    {
        this._descriptors.Add(new ServiceDescriptor(typeof(TMock1), mock1));
    }
}
public class MockPackage<TTarget, TContext, TMock1, TMock2> : MockPackage<TTarget, TContext, TMock1> where TTarget : class where TContext : DbContext
{
    public MockPackage(TContext context = null, TMock1 mock1 = default, TMock2 mock2 = default) : base(context, mock1)
    {
        this._descriptors.Add(new ServiceDescriptor(typeof(TMock2), mock2));

    }
}
public class MockPackage<TTarget, TContext, TMock1, TMock2, TMock3> : MockPackage<TTarget, TContext, TMock1, TMock2> where TTarget : class where TContext : DbContext
{
    public MockPackage(TContext context = null, TMock1 mock1 = default, TMock2 mock2 = default, TMock3 mock3 = default) : base(context, mock1, mock2)
    {
        this._descriptors.Add(new ServiceDescriptor(typeof(TMock3), mock3));

    }
}
public class MockPackage<TTarget, TContext, TMock1, TMock2, TMock3, TMock4> : MockPackage<TTarget, TContext, TMock1, TMock2, TMock3> where TTarget : class where TContext : DbContext
{
    public MockPackage(TContext context = null, TMock1 mock1 = default, TMock2 mock2 = default, TMock3 mock3 = default, TMock4 mock4 = default) : base(context, mock1, mock2, mock3)
    {
        this._descriptors.Add(new ServiceDescriptor(typeof(TMock4), mock4));

    }
}