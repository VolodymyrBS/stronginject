using FluentAssertions;
using Microsoft.CodeAnalysis;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace StrongInject.Generator.Tests.Unit
{
    public class GeneratorTests : TestBase
    {
        public GeneratorTests(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        [Fact]
        public void InstancePerResolutionDependencies()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
[Register(typeof(C))]
[Register(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, C c){}
}
public class B 
{
    public B(C c, D d){}
}
public class C {}
public class D 
{
    public D(C c){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_2;
        global::D d_0_3;
        global::B b_0_1;
        global::A a_0_0;
        c_0_2 = new global::C();
        d_0_3 = new global::D(c: c_0_2);
        b_0_1 = new global::B(c: c_0_2, d: d_0_3);
        a_0_0 = new global::A(b: b_0_1, c: c_0_2);
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_2;
        global::D d_0_3;
        global::B b_0_1;
        global::A a_0_0;
        c_0_2 = new global::C();
        d_0_3 = new global::D(c: c_0_2);
        b_0_1 = new global::B(c: c_0_2, d: d_0_3);
        a_0_0 = new global::A(b: b_0_1, c: c_0_2);
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void InstancePerResolutionDependenciesWithCasts()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
[Register(typeof(C), typeof(C), typeof(IC))]
[Register(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, IC c){}
}
public class B 
{
    public B(IC c, D d){}
}
public class C : IC {}
public class D 
{
    public D(C c){}
}
public interface IC {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_3;
        global::IC iC_0_2;
        global::D d_0_4;
        global::B b_0_1;
        global::A a_0_0;
        c_0_3 = new global::C();
        iC_0_2 = (global::IC)c_0_3;
        d_0_4 = new global::D(c: c_0_3);
        b_0_1 = new global::B(c: iC_0_2, d: d_0_4);
        a_0_0 = new global::A(b: b_0_1, c: iC_0_2);
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_3;
        global::IC iC_0_2;
        global::D d_0_4;
        global::B b_0_1;
        global::A a_0_0;
        c_0_3 = new global::C();
        iC_0_2 = (global::IC)c_0_3;
        d_0_4 = new global::D(c: c_0_3);
        b_0_1 = new global::B(c: iC_0_2, d: d_0_4);
        a_0_0 = new global::A(b: b_0_1, c: iC_0_2);
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void InstancePerResolutionDependenciesWithRequiresInitialization()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A))]
[Register(typeof(B))]
[Register(typeof(C))]
[Register(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A : IRequiresAsyncInitialization
{
    public A(B b, C c){}

    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
public class B 
{
    public B(C c, D d){}
}
public class C : IRequiresAsyncInitialization { public ValueTask InitializeAsync()  => new ValueTask();  }
public class D : E
{
    public D(C c){}
}

public class E : IRequiresAsyncInitialization
{
    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_2;
        global::System.Threading.Tasks.ValueTask c_0_3;
        var hasAwaitStarted_c_0_3 = false;
        global::D d_0_4;
        global::System.Threading.Tasks.ValueTask d_0_5;
        var hasAwaitStarted_d_0_5 = false;
        global::B b_0_1;
        global::A a_0_0;
        global::System.Threading.Tasks.ValueTask a_0_6;
        var hasAwaitStarted_a_0_6 = false;
        c_0_2 = new global::C();
        c_0_3 = ((global::StrongInject.IRequiresAsyncInitialization)c_0_2).InitializeAsync();
        try
        {
            hasAwaitStarted_c_0_3 = true;
            await c_0_3;
            d_0_4 = new global::D(c: c_0_2);
            d_0_5 = ((global::StrongInject.IRequiresAsyncInitialization)d_0_4).InitializeAsync();
            try
            {
                hasAwaitStarted_d_0_5 = true;
                await d_0_5;
                b_0_1 = new global::B(c: c_0_2, d: d_0_4);
                a_0_0 = new global::A(b: b_0_1, c: c_0_2);
                a_0_6 = ((global::StrongInject.IRequiresAsyncInitialization)a_0_0).InitializeAsync();
                try
                {
                    hasAwaitStarted_a_0_6 = true;
                    await a_0_6;
                }
                catch
                {
                    if (!hasAwaitStarted_a_0_6)
                    {
                        _ = a_0_6.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                    }

                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_d_0_5)
                {
                    _ = d_0_5.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_c_0_3)
            {
                _ = c_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_2;
        global::System.Threading.Tasks.ValueTask c_0_3;
        var hasAwaitStarted_c_0_3 = false;
        global::D d_0_4;
        global::System.Threading.Tasks.ValueTask d_0_5;
        var hasAwaitStarted_d_0_5 = false;
        global::B b_0_1;
        global::A a_0_0;
        global::System.Threading.Tasks.ValueTask a_0_6;
        var hasAwaitStarted_a_0_6 = false;
        c_0_2 = new global::C();
        c_0_3 = ((global::StrongInject.IRequiresAsyncInitialization)c_0_2).InitializeAsync();
        try
        {
            hasAwaitStarted_c_0_3 = true;
            await c_0_3;
            d_0_4 = new global::D(c: c_0_2);
            d_0_5 = ((global::StrongInject.IRequiresAsyncInitialization)d_0_4).InitializeAsync();
            try
            {
                hasAwaitStarted_d_0_5 = true;
                await d_0_5;
                b_0_1 = new global::B(c: c_0_2, d: d_0_4);
                a_0_0 = new global::A(b: b_0_1, c: c_0_2);
                a_0_6 = ((global::StrongInject.IRequiresAsyncInitialization)a_0_0).InitializeAsync();
                try
                {
                    hasAwaitStarted_a_0_6 = true;
                    await a_0_6;
                }
                catch
                {
                    if (!hasAwaitStarted_a_0_6)
                    {
                        _ = a_0_6.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                    }

                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_d_0_5)
                {
                    _ = d_0_5.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_c_0_3)
            {
                _ = c_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void InstancePerResolutionDependenciesWithFactories()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[RegisterFactory(typeof(A))]
[RegisterFactory(typeof(B))]
[RegisterFactory(typeof(C))]
[RegisterFactory(typeof(D))]
[Register(typeof(C))]
public partial class Container : IAsyncContainer<AFactoryTarget>
{
}

public class A : IAsyncFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c){}
    ValueTask<AFactoryTarget> IAsyncFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IAsyncFactory<BFactoryTarget>
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IAsyncFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
}
public class BFactoryTarget {}
public class C : IAsyncFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IAsyncFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IAsyncFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IAsyncFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (9,2): Warning SI1001: 'C' implements 'StrongInject.IAsyncFactory<CFactoryTarget>'. Did you mean to use FactoryRegistration instead?
                // Register(typeof(C))
                new DiagnosticResult("SI1001", @"Register(typeof(C))", DiagnosticSeverity.Warning).WithLocation(9, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_6;
        global::StrongInject.IAsyncFactory<global::CFactoryTarget> iAsyncFactory_0_11;
        global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_12;
        var hasAwaitStarted_cFactoryTarget_0_12 = false;
        var cFactoryTarget_0_10 = default(global::CFactoryTarget);
        var hasAwaitCompleted_cFactoryTarget_0_12 = false;
        global::D d_0_9;
        global::StrongInject.IAsyncFactory<global::DFactoryTarget> iAsyncFactory_0_8;
        global::System.Threading.Tasks.ValueTask<global::DFactoryTarget> dFactoryTarget_0_13;
        var hasAwaitStarted_dFactoryTarget_0_13 = false;
        var dFactoryTarget_0_7 = default(global::DFactoryTarget);
        var hasAwaitCompleted_dFactoryTarget_0_13 = false;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::BFactoryTarget> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> bFactoryTarget_0_14;
        var hasAwaitStarted_bFactoryTarget_0_14 = false;
        var bFactoryTarget_0_3 = default(global::BFactoryTarget);
        var hasAwaitCompleted_bFactoryTarget_0_14 = false;
        global::A a_0_2;
        global::StrongInject.IAsyncFactory<global::AFactoryTarget> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::AFactoryTarget> aFactoryTarget_0_15;
        var hasAwaitStarted_aFactoryTarget_0_15 = false;
        var aFactoryTarget_0_0 = default(global::AFactoryTarget);
        var hasAwaitCompleted_aFactoryTarget_0_15 = false;
        c_0_6 = new global::C();
        iAsyncFactory_0_11 = (global::StrongInject.IAsyncFactory<global::CFactoryTarget>)c_0_6;
        cFactoryTarget_0_12 = iAsyncFactory_0_11.CreateAsync();
        try
        {
            hasAwaitStarted_cFactoryTarget_0_12 = true;
            cFactoryTarget_0_10 = await cFactoryTarget_0_12;
            hasAwaitCompleted_cFactoryTarget_0_12 = true;
            d_0_9 = new global::D(c: cFactoryTarget_0_10);
            iAsyncFactory_0_8 = (global::StrongInject.IAsyncFactory<global::DFactoryTarget>)d_0_9;
            dFactoryTarget_0_13 = iAsyncFactory_0_8.CreateAsync();
            try
            {
                hasAwaitStarted_dFactoryTarget_0_13 = true;
                dFactoryTarget_0_7 = await dFactoryTarget_0_13;
                hasAwaitCompleted_dFactoryTarget_0_13 = true;
                b_0_5 = new global::B(c: c_0_6, d: dFactoryTarget_0_7);
                iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::BFactoryTarget>)b_0_5;
                bFactoryTarget_0_14 = iAsyncFactory_0_4.CreateAsync();
                try
                {
                    hasAwaitStarted_bFactoryTarget_0_14 = true;
                    bFactoryTarget_0_3 = await bFactoryTarget_0_14;
                    hasAwaitCompleted_bFactoryTarget_0_14 = true;
                    a_0_2 = new global::A(b: bFactoryTarget_0_3, c: cFactoryTarget_0_10);
                    iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::AFactoryTarget>)a_0_2;
                    aFactoryTarget_0_15 = iAsyncFactory_0_1.CreateAsync();
                    try
                    {
                        hasAwaitStarted_aFactoryTarget_0_15 = true;
                        aFactoryTarget_0_0 = await aFactoryTarget_0_15;
                        hasAwaitCompleted_aFactoryTarget_0_15 = true;
                    }
                    catch
                    {
                        if (!hasAwaitStarted_aFactoryTarget_0_15)
                        {
                            aFactoryTarget_0_0 = await aFactoryTarget_0_15;
                        }
                        else if (!hasAwaitCompleted_aFactoryTarget_0_15)
                        {
                            throw;
                        }

                        await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
                        throw;
                    }
                }
                catch
                {
                    if (!hasAwaitStarted_bFactoryTarget_0_14)
                    {
                        bFactoryTarget_0_3 = await bFactoryTarget_0_14;
                    }
                    else if (!hasAwaitCompleted_bFactoryTarget_0_14)
                    {
                        throw;
                    }

                    await iAsyncFactory_0_4.ReleaseAsync(bFactoryTarget_0_3);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_dFactoryTarget_0_13)
                {
                    dFactoryTarget_0_7 = await dFactoryTarget_0_13;
                }
                else if (!hasAwaitCompleted_dFactoryTarget_0_13)
                {
                    throw;
                }

                await iAsyncFactory_0_8.ReleaseAsync(dFactoryTarget_0_7);
                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_cFactoryTarget_0_12)
            {
                cFactoryTarget_0_10 = await cFactoryTarget_0_12;
            }
            else if (!hasAwaitCompleted_cFactoryTarget_0_12)
            {
                throw;
            }

            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_10);
            throw;
        }

        TResult result;
        try
        {
            result = await func(aFactoryTarget_0_0, param);
        }
        finally
        {
            await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
            await iAsyncFactory_0_4.ReleaseAsync(bFactoryTarget_0_3);
            await iAsyncFactory_0_8.ReleaseAsync(dFactoryTarget_0_7);
            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_10);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::AFactoryTarget>> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_6;
        global::StrongInject.IAsyncFactory<global::CFactoryTarget> iAsyncFactory_0_11;
        global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_12;
        var hasAwaitStarted_cFactoryTarget_0_12 = false;
        var cFactoryTarget_0_10 = default(global::CFactoryTarget);
        var hasAwaitCompleted_cFactoryTarget_0_12 = false;
        global::D d_0_9;
        global::StrongInject.IAsyncFactory<global::DFactoryTarget> iAsyncFactory_0_8;
        global::System.Threading.Tasks.ValueTask<global::DFactoryTarget> dFactoryTarget_0_13;
        var hasAwaitStarted_dFactoryTarget_0_13 = false;
        var dFactoryTarget_0_7 = default(global::DFactoryTarget);
        var hasAwaitCompleted_dFactoryTarget_0_13 = false;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::BFactoryTarget> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> bFactoryTarget_0_14;
        var hasAwaitStarted_bFactoryTarget_0_14 = false;
        var bFactoryTarget_0_3 = default(global::BFactoryTarget);
        var hasAwaitCompleted_bFactoryTarget_0_14 = false;
        global::A a_0_2;
        global::StrongInject.IAsyncFactory<global::AFactoryTarget> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::AFactoryTarget> aFactoryTarget_0_15;
        var hasAwaitStarted_aFactoryTarget_0_15 = false;
        var aFactoryTarget_0_0 = default(global::AFactoryTarget);
        var hasAwaitCompleted_aFactoryTarget_0_15 = false;
        c_0_6 = new global::C();
        iAsyncFactory_0_11 = (global::StrongInject.IAsyncFactory<global::CFactoryTarget>)c_0_6;
        cFactoryTarget_0_12 = iAsyncFactory_0_11.CreateAsync();
        try
        {
            hasAwaitStarted_cFactoryTarget_0_12 = true;
            cFactoryTarget_0_10 = await cFactoryTarget_0_12;
            hasAwaitCompleted_cFactoryTarget_0_12 = true;
            d_0_9 = new global::D(c: cFactoryTarget_0_10);
            iAsyncFactory_0_8 = (global::StrongInject.IAsyncFactory<global::DFactoryTarget>)d_0_9;
            dFactoryTarget_0_13 = iAsyncFactory_0_8.CreateAsync();
            try
            {
                hasAwaitStarted_dFactoryTarget_0_13 = true;
                dFactoryTarget_0_7 = await dFactoryTarget_0_13;
                hasAwaitCompleted_dFactoryTarget_0_13 = true;
                b_0_5 = new global::B(c: c_0_6, d: dFactoryTarget_0_7);
                iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::BFactoryTarget>)b_0_5;
                bFactoryTarget_0_14 = iAsyncFactory_0_4.CreateAsync();
                try
                {
                    hasAwaitStarted_bFactoryTarget_0_14 = true;
                    bFactoryTarget_0_3 = await bFactoryTarget_0_14;
                    hasAwaitCompleted_bFactoryTarget_0_14 = true;
                    a_0_2 = new global::A(b: bFactoryTarget_0_3, c: cFactoryTarget_0_10);
                    iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::AFactoryTarget>)a_0_2;
                    aFactoryTarget_0_15 = iAsyncFactory_0_1.CreateAsync();
                    try
                    {
                        hasAwaitStarted_aFactoryTarget_0_15 = true;
                        aFactoryTarget_0_0 = await aFactoryTarget_0_15;
                        hasAwaitCompleted_aFactoryTarget_0_15 = true;
                    }
                    catch
                    {
                        if (!hasAwaitStarted_aFactoryTarget_0_15)
                        {
                            aFactoryTarget_0_0 = await aFactoryTarget_0_15;
                        }
                        else if (!hasAwaitCompleted_aFactoryTarget_0_15)
                        {
                            throw;
                        }

                        await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
                        throw;
                    }
                }
                catch
                {
                    if (!hasAwaitStarted_bFactoryTarget_0_14)
                    {
                        bFactoryTarget_0_3 = await bFactoryTarget_0_14;
                    }
                    else if (!hasAwaitCompleted_bFactoryTarget_0_14)
                    {
                        throw;
                    }

                    await iAsyncFactory_0_4.ReleaseAsync(bFactoryTarget_0_3);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_dFactoryTarget_0_13)
                {
                    dFactoryTarget_0_7 = await dFactoryTarget_0_13;
                }
                else if (!hasAwaitCompleted_dFactoryTarget_0_13)
                {
                    throw;
                }

                await iAsyncFactory_0_8.ReleaseAsync(dFactoryTarget_0_7);
                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_cFactoryTarget_0_12)
            {
                cFactoryTarget_0_10 = await cFactoryTarget_0_12;
            }
            else if (!hasAwaitCompleted_cFactoryTarget_0_12)
            {
                throw;
            }

            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_10);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::AFactoryTarget>(aFactoryTarget_0_0, async () =>
        {
            await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
            await iAsyncFactory_0_4.ReleaseAsync(bFactoryTarget_0_3);
            await iAsyncFactory_0_8.ReleaseAsync(dFactoryTarget_0_7);
            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_10);
        });
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependencies()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A), Scope.InstancePerDependency)]
[Register(typeof(B))]
[Register(typeof(C), Scope.InstancePerDependency)]
[Register(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, C c){}
}
public class B 
{
    public B(C c, D d){}
}
public class C {}
public class D 
{
    public D(C c){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_2;
        global::C c_0_4;
        global::D d_0_3;
        global::B b_0_1;
        global::C c_0_5;
        global::A a_0_0;
        c_0_2 = new global::C();
        c_0_4 = new global::C();
        d_0_3 = new global::D(c: c_0_4);
        b_0_1 = new global::B(c: c_0_2, d: d_0_3);
        c_0_5 = new global::C();
        a_0_0 = new global::A(b: b_0_1, c: c_0_5);
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_2;
        global::C c_0_4;
        global::D d_0_3;
        global::B b_0_1;
        global::C c_0_5;
        global::A a_0_0;
        c_0_2 = new global::C();
        c_0_4 = new global::C();
        d_0_3 = new global::D(c: c_0_4);
        b_0_1 = new global::B(c: c_0_2, d: d_0_3);
        c_0_5 = new global::C();
        a_0_0 = new global::A(b: b_0_1, c: c_0_5);
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependenciesWithCasts()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
[Register(typeof(C), Scope.InstancePerDependency, typeof(C), typeof(IC))]
[Register(typeof(D), Scope.InstancePerDependency)]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, IC c){}
}
public class B 
{
    public B(IC c, D d){}
}
public class C : IC {}
public class D 
{
    public D(C c){}
}
public interface IC {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_3;
        global::IC iC_0_2;
        global::C c_0_5;
        global::D d_0_4;
        global::B b_0_1;
        global::C c_0_7;
        global::IC iC_0_6;
        global::A a_0_0;
        c_0_3 = new global::C();
        iC_0_2 = (global::IC)c_0_3;
        c_0_5 = new global::C();
        d_0_4 = new global::D(c: c_0_5);
        b_0_1 = new global::B(c: iC_0_2, d: d_0_4);
        c_0_7 = new global::C();
        iC_0_6 = (global::IC)c_0_7;
        a_0_0 = new global::A(b: b_0_1, c: iC_0_6);
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_3;
        global::IC iC_0_2;
        global::C c_0_5;
        global::D d_0_4;
        global::B b_0_1;
        global::C c_0_7;
        global::IC iC_0_6;
        global::A a_0_0;
        c_0_3 = new global::C();
        iC_0_2 = (global::IC)c_0_3;
        c_0_5 = new global::C();
        d_0_4 = new global::D(c: c_0_5);
        b_0_1 = new global::B(c: iC_0_2, d: d_0_4);
        c_0_7 = new global::C();
        iC_0_6 = (global::IC)c_0_7;
        a_0_0 = new global::A(b: b_0_1, c: iC_0_6);
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependenciesWithRequiresInitialization()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A), Scope.InstancePerDependency)]
[Register(typeof(B), Scope.InstancePerDependency)]
[Register(typeof(C))]
[Register(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A : IRequiresAsyncInitialization
{
    public A(B b, C c, B b1){}

    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
public class B 
{
    public B(C c, D d){}
}
public class C : IRequiresAsyncInitialization { public ValueTask InitializeAsync()  => new ValueTask();  }
public class D : E
{
    public D(C c){}
}

public class E : IRequiresAsyncInitialization
{
    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_2;
        global::System.Threading.Tasks.ValueTask c_0_3;
        var hasAwaitStarted_c_0_3 = false;
        global::D d_0_4;
        global::System.Threading.Tasks.ValueTask d_0_5;
        var hasAwaitStarted_d_0_5 = false;
        global::B b_0_1;
        global::B b_0_6;
        global::A a_0_0;
        global::System.Threading.Tasks.ValueTask a_0_7;
        var hasAwaitStarted_a_0_7 = false;
        c_0_2 = new global::C();
        c_0_3 = ((global::StrongInject.IRequiresAsyncInitialization)c_0_2).InitializeAsync();
        try
        {
            hasAwaitStarted_c_0_3 = true;
            await c_0_3;
            d_0_4 = new global::D(c: c_0_2);
            d_0_5 = ((global::StrongInject.IRequiresAsyncInitialization)d_0_4).InitializeAsync();
            try
            {
                hasAwaitStarted_d_0_5 = true;
                await d_0_5;
                b_0_1 = new global::B(c: c_0_2, d: d_0_4);
                b_0_6 = new global::B(c: c_0_2, d: d_0_4);
                a_0_0 = new global::A(b: b_0_1, c: c_0_2, b1: b_0_6);
                a_0_7 = ((global::StrongInject.IRequiresAsyncInitialization)a_0_0).InitializeAsync();
                try
                {
                    hasAwaitStarted_a_0_7 = true;
                    await a_0_7;
                }
                catch
                {
                    if (!hasAwaitStarted_a_0_7)
                    {
                        _ = a_0_7.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                    }

                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_d_0_5)
                {
                    _ = d_0_5.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_c_0_3)
            {
                _ = c_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_2;
        global::System.Threading.Tasks.ValueTask c_0_3;
        var hasAwaitStarted_c_0_3 = false;
        global::D d_0_4;
        global::System.Threading.Tasks.ValueTask d_0_5;
        var hasAwaitStarted_d_0_5 = false;
        global::B b_0_1;
        global::B b_0_6;
        global::A a_0_0;
        global::System.Threading.Tasks.ValueTask a_0_7;
        var hasAwaitStarted_a_0_7 = false;
        c_0_2 = new global::C();
        c_0_3 = ((global::StrongInject.IRequiresAsyncInitialization)c_0_2).InitializeAsync();
        try
        {
            hasAwaitStarted_c_0_3 = true;
            await c_0_3;
            d_0_4 = new global::D(c: c_0_2);
            d_0_5 = ((global::StrongInject.IRequiresAsyncInitialization)d_0_4).InitializeAsync();
            try
            {
                hasAwaitStarted_d_0_5 = true;
                await d_0_5;
                b_0_1 = new global::B(c: c_0_2, d: d_0_4);
                b_0_6 = new global::B(c: c_0_2, d: d_0_4);
                a_0_0 = new global::A(b: b_0_1, c: c_0_2, b1: b_0_6);
                a_0_7 = ((global::StrongInject.IRequiresAsyncInitialization)a_0_0).InitializeAsync();
                try
                {
                    hasAwaitStarted_a_0_7 = true;
                    await a_0_7;
                }
                catch
                {
                    if (!hasAwaitStarted_a_0_7)
                    {
                        _ = a_0_7.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                    }

                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_d_0_5)
                {
                    _ = d_0_5.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_c_0_3)
            {
                _ = c_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void InstancePerDependencyDependenciesWithFactories()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[RegisterFactory(typeof(A))]
[RegisterFactory(typeof(B), Scope.InstancePerDependency)]
[RegisterFactory(typeof(C), Scope.InstancePerResolution, Scope.InstancePerDependency)]
[RegisterFactory(typeof(D), Scope.InstancePerDependency, Scope.InstancePerDependency)]
[Register(typeof(C))]
public partial class Container : IAsyncContainer<AFactoryTarget>
{
}

public class A : IAsyncFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c, DFactoryTarget d){}
    ValueTask<AFactoryTarget> IAsyncFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IAsyncFactory<BFactoryTarget>
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IAsyncFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
}
public class BFactoryTarget {}
public class C : IAsyncFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IAsyncFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IAsyncFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IAsyncFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (9,2): Warning SI1001: 'C' implements 'StrongInject.IAsyncFactory<CFactoryTarget>'. Did you mean to use FactoryRegistration instead?
                // Register(typeof(C))
                new DiagnosticResult("SI1001", @"Register(typeof(C))", DiagnosticSeverity.Warning).WithLocation(9, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_6;
        global::StrongInject.IAsyncFactory<global::CFactoryTarget> iAsyncFactory_0_11;
        global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_12;
        global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_16;
        global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_21;
        var hasAwaitStarted_cFactoryTarget_0_12 = false;
        var cFactoryTarget_0_10 = default(global::CFactoryTarget);
        var hasAwaitCompleted_cFactoryTarget_0_12 = false;
        global::D d_0_9;
        global::StrongInject.IAsyncFactory<global::DFactoryTarget> iAsyncFactory_0_8;
        global::System.Threading.Tasks.ValueTask<global::DFactoryTarget> dFactoryTarget_0_13;
        var hasAwaitStarted_dFactoryTarget_0_13 = false;
        var dFactoryTarget_0_7 = default(global::DFactoryTarget);
        var hasAwaitCompleted_dFactoryTarget_0_13 = false;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::BFactoryTarget> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> bFactoryTarget_0_14;
        var hasAwaitStarted_cFactoryTarget_0_21 = false;
        var cFactoryTarget_0_20 = default(global::CFactoryTarget);
        var hasAwaitCompleted_cFactoryTarget_0_21 = false;
        global::D d_0_19;
        global::StrongInject.IAsyncFactory<global::DFactoryTarget> iAsyncFactory_0_18;
        global::System.Threading.Tasks.ValueTask<global::DFactoryTarget> dFactoryTarget_0_22;
        var hasAwaitStarted_bFactoryTarget_0_14 = false;
        var bFactoryTarget_0_3 = default(global::BFactoryTarget);
        var hasAwaitCompleted_bFactoryTarget_0_14 = false;
        var hasAwaitStarted_cFactoryTarget_0_16 = false;
        var cFactoryTarget_0_15 = default(global::CFactoryTarget);
        var hasAwaitCompleted_cFactoryTarget_0_16 = false;
        var hasAwaitStarted_dFactoryTarget_0_22 = false;
        var dFactoryTarget_0_17 = default(global::DFactoryTarget);
        var hasAwaitCompleted_dFactoryTarget_0_22 = false;
        global::A a_0_2;
        global::StrongInject.IAsyncFactory<global::AFactoryTarget> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::AFactoryTarget> aFactoryTarget_0_23;
        var hasAwaitStarted_aFactoryTarget_0_23 = false;
        var aFactoryTarget_0_0 = default(global::AFactoryTarget);
        var hasAwaitCompleted_aFactoryTarget_0_23 = false;
        c_0_6 = new global::C();
        iAsyncFactory_0_11 = (global::StrongInject.IAsyncFactory<global::CFactoryTarget>)c_0_6;
        cFactoryTarget_0_12 = iAsyncFactory_0_11.CreateAsync();
        try
        {
            cFactoryTarget_0_16 = iAsyncFactory_0_11.CreateAsync();
            try
            {
                cFactoryTarget_0_21 = iAsyncFactory_0_11.CreateAsync();
                try
                {
                    hasAwaitStarted_cFactoryTarget_0_12 = true;
                    cFactoryTarget_0_10 = await cFactoryTarget_0_12;
                    hasAwaitCompleted_cFactoryTarget_0_12 = true;
                    d_0_9 = new global::D(c: cFactoryTarget_0_10);
                    iAsyncFactory_0_8 = (global::StrongInject.IAsyncFactory<global::DFactoryTarget>)d_0_9;
                    dFactoryTarget_0_13 = iAsyncFactory_0_8.CreateAsync();
                    try
                    {
                        hasAwaitStarted_dFactoryTarget_0_13 = true;
                        dFactoryTarget_0_7 = await dFactoryTarget_0_13;
                        hasAwaitCompleted_dFactoryTarget_0_13 = true;
                        b_0_5 = new global::B(c: c_0_6, d: dFactoryTarget_0_7);
                        iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::BFactoryTarget>)b_0_5;
                        bFactoryTarget_0_14 = iAsyncFactory_0_4.CreateAsync();
                        try
                        {
                            hasAwaitStarted_cFactoryTarget_0_21 = true;
                            cFactoryTarget_0_20 = await cFactoryTarget_0_21;
                            hasAwaitCompleted_cFactoryTarget_0_21 = true;
                            d_0_19 = new global::D(c: cFactoryTarget_0_20);
                            iAsyncFactory_0_18 = (global::StrongInject.IAsyncFactory<global::DFactoryTarget>)d_0_19;
                            dFactoryTarget_0_22 = iAsyncFactory_0_18.CreateAsync();
                            try
                            {
                                hasAwaitStarted_bFactoryTarget_0_14 = true;
                                bFactoryTarget_0_3 = await bFactoryTarget_0_14;
                                hasAwaitCompleted_bFactoryTarget_0_14 = true;
                                hasAwaitStarted_cFactoryTarget_0_16 = true;
                                cFactoryTarget_0_15 = await cFactoryTarget_0_16;
                                hasAwaitCompleted_cFactoryTarget_0_16 = true;
                                hasAwaitStarted_dFactoryTarget_0_22 = true;
                                dFactoryTarget_0_17 = await dFactoryTarget_0_22;
                                hasAwaitCompleted_dFactoryTarget_0_22 = true;
                                a_0_2 = new global::A(b: bFactoryTarget_0_3, c: cFactoryTarget_0_15, d: dFactoryTarget_0_17);
                                iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::AFactoryTarget>)a_0_2;
                                aFactoryTarget_0_23 = iAsyncFactory_0_1.CreateAsync();
                                try
                                {
                                    hasAwaitStarted_aFactoryTarget_0_23 = true;
                                    aFactoryTarget_0_0 = await aFactoryTarget_0_23;
                                    hasAwaitCompleted_aFactoryTarget_0_23 = true;
                                }
                                catch
                                {
                                    if (!hasAwaitStarted_aFactoryTarget_0_23)
                                    {
                                        aFactoryTarget_0_0 = await aFactoryTarget_0_23;
                                    }
                                    else if (!hasAwaitCompleted_aFactoryTarget_0_23)
                                    {
                                        throw;
                                    }

                                    await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
                                    throw;
                                }
                            }
                            catch
                            {
                                if (!hasAwaitStarted_dFactoryTarget_0_22)
                                {
                                    dFactoryTarget_0_17 = await dFactoryTarget_0_22;
                                }
                                else if (!hasAwaitCompleted_dFactoryTarget_0_22)
                                {
                                    throw;
                                }

                                await iAsyncFactory_0_18.ReleaseAsync(dFactoryTarget_0_17);
                                throw;
                            }
                        }
                        catch
                        {
                            if (!hasAwaitStarted_bFactoryTarget_0_14)
                            {
                                bFactoryTarget_0_3 = await bFactoryTarget_0_14;
                            }
                            else if (!hasAwaitCompleted_bFactoryTarget_0_14)
                            {
                                throw;
                            }

                            await iAsyncFactory_0_4.ReleaseAsync(bFactoryTarget_0_3);
                            throw;
                        }
                    }
                    catch
                    {
                        if (!hasAwaitStarted_dFactoryTarget_0_13)
                        {
                            dFactoryTarget_0_7 = await dFactoryTarget_0_13;
                        }
                        else if (!hasAwaitCompleted_dFactoryTarget_0_13)
                        {
                            throw;
                        }

                        await iAsyncFactory_0_8.ReleaseAsync(dFactoryTarget_0_7);
                        throw;
                    }
                }
                catch
                {
                    if (!hasAwaitStarted_cFactoryTarget_0_21)
                    {
                        cFactoryTarget_0_20 = await cFactoryTarget_0_21;
                    }
                    else if (!hasAwaitCompleted_cFactoryTarget_0_21)
                    {
                        throw;
                    }

                    await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_20);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_cFactoryTarget_0_16)
                {
                    cFactoryTarget_0_15 = await cFactoryTarget_0_16;
                }
                else if (!hasAwaitCompleted_cFactoryTarget_0_16)
                {
                    throw;
                }

                await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_15);
                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_cFactoryTarget_0_12)
            {
                cFactoryTarget_0_10 = await cFactoryTarget_0_12;
            }
            else if (!hasAwaitCompleted_cFactoryTarget_0_12)
            {
                throw;
            }

            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_10);
            throw;
        }

        TResult result;
        try
        {
            result = await func(aFactoryTarget_0_0, param);
        }
        finally
        {
            await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
            await iAsyncFactory_0_18.ReleaseAsync(dFactoryTarget_0_17);
            await iAsyncFactory_0_4.ReleaseAsync(bFactoryTarget_0_3);
            await iAsyncFactory_0_8.ReleaseAsync(dFactoryTarget_0_7);
            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_20);
            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_15);
            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_10);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::AFactoryTarget>> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_6;
        global::StrongInject.IAsyncFactory<global::CFactoryTarget> iAsyncFactory_0_11;
        global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_12;
        global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_16;
        global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_21;
        var hasAwaitStarted_cFactoryTarget_0_12 = false;
        var cFactoryTarget_0_10 = default(global::CFactoryTarget);
        var hasAwaitCompleted_cFactoryTarget_0_12 = false;
        global::D d_0_9;
        global::StrongInject.IAsyncFactory<global::DFactoryTarget> iAsyncFactory_0_8;
        global::System.Threading.Tasks.ValueTask<global::DFactoryTarget> dFactoryTarget_0_13;
        var hasAwaitStarted_dFactoryTarget_0_13 = false;
        var dFactoryTarget_0_7 = default(global::DFactoryTarget);
        var hasAwaitCompleted_dFactoryTarget_0_13 = false;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::BFactoryTarget> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> bFactoryTarget_0_14;
        var hasAwaitStarted_cFactoryTarget_0_21 = false;
        var cFactoryTarget_0_20 = default(global::CFactoryTarget);
        var hasAwaitCompleted_cFactoryTarget_0_21 = false;
        global::D d_0_19;
        global::StrongInject.IAsyncFactory<global::DFactoryTarget> iAsyncFactory_0_18;
        global::System.Threading.Tasks.ValueTask<global::DFactoryTarget> dFactoryTarget_0_22;
        var hasAwaitStarted_bFactoryTarget_0_14 = false;
        var bFactoryTarget_0_3 = default(global::BFactoryTarget);
        var hasAwaitCompleted_bFactoryTarget_0_14 = false;
        var hasAwaitStarted_cFactoryTarget_0_16 = false;
        var cFactoryTarget_0_15 = default(global::CFactoryTarget);
        var hasAwaitCompleted_cFactoryTarget_0_16 = false;
        var hasAwaitStarted_dFactoryTarget_0_22 = false;
        var dFactoryTarget_0_17 = default(global::DFactoryTarget);
        var hasAwaitCompleted_dFactoryTarget_0_22 = false;
        global::A a_0_2;
        global::StrongInject.IAsyncFactory<global::AFactoryTarget> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::AFactoryTarget> aFactoryTarget_0_23;
        var hasAwaitStarted_aFactoryTarget_0_23 = false;
        var aFactoryTarget_0_0 = default(global::AFactoryTarget);
        var hasAwaitCompleted_aFactoryTarget_0_23 = false;
        c_0_6 = new global::C();
        iAsyncFactory_0_11 = (global::StrongInject.IAsyncFactory<global::CFactoryTarget>)c_0_6;
        cFactoryTarget_0_12 = iAsyncFactory_0_11.CreateAsync();
        try
        {
            cFactoryTarget_0_16 = iAsyncFactory_0_11.CreateAsync();
            try
            {
                cFactoryTarget_0_21 = iAsyncFactory_0_11.CreateAsync();
                try
                {
                    hasAwaitStarted_cFactoryTarget_0_12 = true;
                    cFactoryTarget_0_10 = await cFactoryTarget_0_12;
                    hasAwaitCompleted_cFactoryTarget_0_12 = true;
                    d_0_9 = new global::D(c: cFactoryTarget_0_10);
                    iAsyncFactory_0_8 = (global::StrongInject.IAsyncFactory<global::DFactoryTarget>)d_0_9;
                    dFactoryTarget_0_13 = iAsyncFactory_0_8.CreateAsync();
                    try
                    {
                        hasAwaitStarted_dFactoryTarget_0_13 = true;
                        dFactoryTarget_0_7 = await dFactoryTarget_0_13;
                        hasAwaitCompleted_dFactoryTarget_0_13 = true;
                        b_0_5 = new global::B(c: c_0_6, d: dFactoryTarget_0_7);
                        iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::BFactoryTarget>)b_0_5;
                        bFactoryTarget_0_14 = iAsyncFactory_0_4.CreateAsync();
                        try
                        {
                            hasAwaitStarted_cFactoryTarget_0_21 = true;
                            cFactoryTarget_0_20 = await cFactoryTarget_0_21;
                            hasAwaitCompleted_cFactoryTarget_0_21 = true;
                            d_0_19 = new global::D(c: cFactoryTarget_0_20);
                            iAsyncFactory_0_18 = (global::StrongInject.IAsyncFactory<global::DFactoryTarget>)d_0_19;
                            dFactoryTarget_0_22 = iAsyncFactory_0_18.CreateAsync();
                            try
                            {
                                hasAwaitStarted_bFactoryTarget_0_14 = true;
                                bFactoryTarget_0_3 = await bFactoryTarget_0_14;
                                hasAwaitCompleted_bFactoryTarget_0_14 = true;
                                hasAwaitStarted_cFactoryTarget_0_16 = true;
                                cFactoryTarget_0_15 = await cFactoryTarget_0_16;
                                hasAwaitCompleted_cFactoryTarget_0_16 = true;
                                hasAwaitStarted_dFactoryTarget_0_22 = true;
                                dFactoryTarget_0_17 = await dFactoryTarget_0_22;
                                hasAwaitCompleted_dFactoryTarget_0_22 = true;
                                a_0_2 = new global::A(b: bFactoryTarget_0_3, c: cFactoryTarget_0_15, d: dFactoryTarget_0_17);
                                iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::AFactoryTarget>)a_0_2;
                                aFactoryTarget_0_23 = iAsyncFactory_0_1.CreateAsync();
                                try
                                {
                                    hasAwaitStarted_aFactoryTarget_0_23 = true;
                                    aFactoryTarget_0_0 = await aFactoryTarget_0_23;
                                    hasAwaitCompleted_aFactoryTarget_0_23 = true;
                                }
                                catch
                                {
                                    if (!hasAwaitStarted_aFactoryTarget_0_23)
                                    {
                                        aFactoryTarget_0_0 = await aFactoryTarget_0_23;
                                    }
                                    else if (!hasAwaitCompleted_aFactoryTarget_0_23)
                                    {
                                        throw;
                                    }

                                    await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
                                    throw;
                                }
                            }
                            catch
                            {
                                if (!hasAwaitStarted_dFactoryTarget_0_22)
                                {
                                    dFactoryTarget_0_17 = await dFactoryTarget_0_22;
                                }
                                else if (!hasAwaitCompleted_dFactoryTarget_0_22)
                                {
                                    throw;
                                }

                                await iAsyncFactory_0_18.ReleaseAsync(dFactoryTarget_0_17);
                                throw;
                            }
                        }
                        catch
                        {
                            if (!hasAwaitStarted_bFactoryTarget_0_14)
                            {
                                bFactoryTarget_0_3 = await bFactoryTarget_0_14;
                            }
                            else if (!hasAwaitCompleted_bFactoryTarget_0_14)
                            {
                                throw;
                            }

                            await iAsyncFactory_0_4.ReleaseAsync(bFactoryTarget_0_3);
                            throw;
                        }
                    }
                    catch
                    {
                        if (!hasAwaitStarted_dFactoryTarget_0_13)
                        {
                            dFactoryTarget_0_7 = await dFactoryTarget_0_13;
                        }
                        else if (!hasAwaitCompleted_dFactoryTarget_0_13)
                        {
                            throw;
                        }

                        await iAsyncFactory_0_8.ReleaseAsync(dFactoryTarget_0_7);
                        throw;
                    }
                }
                catch
                {
                    if (!hasAwaitStarted_cFactoryTarget_0_21)
                    {
                        cFactoryTarget_0_20 = await cFactoryTarget_0_21;
                    }
                    else if (!hasAwaitCompleted_cFactoryTarget_0_21)
                    {
                        throw;
                    }

                    await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_20);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_cFactoryTarget_0_16)
                {
                    cFactoryTarget_0_15 = await cFactoryTarget_0_16;
                }
                else if (!hasAwaitCompleted_cFactoryTarget_0_16)
                {
                    throw;
                }

                await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_15);
                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_cFactoryTarget_0_12)
            {
                cFactoryTarget_0_10 = await cFactoryTarget_0_12;
            }
            else if (!hasAwaitCompleted_cFactoryTarget_0_12)
            {
                throw;
            }

            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_10);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::AFactoryTarget>(aFactoryTarget_0_0, async () =>
        {
            await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
            await iAsyncFactory_0_18.ReleaseAsync(dFactoryTarget_0_17);
            await iAsyncFactory_0_4.ReleaseAsync(bFactoryTarget_0_3);
            await iAsyncFactory_0_8.ReleaseAsync(dFactoryTarget_0_7);
            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_20);
            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_15);
            await iAsyncFactory_0_11.ReleaseAsync(cFactoryTarget_0_10);
        });
    }
}");
        }

        [Fact]
        public void SingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A), Scope.SingleInstance)]
[Register(typeof(B))]
[Register(typeof(C))]
[Register(typeof(D), Scope.SingleInstance)]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, C c){}
}
public class B 
{
    public B(C c, D d){}
}
public class C {}
public class D 
{
    public D(C c){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }
    }

    private global::A _aField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::D _dField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private global::D GetDField1()
    {
        if (!object.ReferenceEquals(_dField1, null))
            return _dField1;
        this._lock1.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::C c_0_1;
            global::D d_0_0;
            c_0_1 = new global::C();
            d_0_0 = new global::D(c: c_0_1);
            this._dField1 = d_0_0;
            this._disposeAction1 = async () =>
            {
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _dField1;
    }

    private global::A GetAField0()
    {
        if (!object.ReferenceEquals(_aField0, null))
            return _aField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::C c_0_2;
            global::D d_0_3;
            global::B b_0_1;
            global::A a_0_0;
            c_0_2 = new global::C();
            d_0_3 = GetDField1();
            b_0_1 = new global::B(c: c_0_2, d: d_0_3);
            a_0_0 = new global::A(b: b_0_1, c: c_0_2);
            this._aField0 = a_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _aField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = GetAField0();
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = GetAField0();
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void SingleInstanceDependenciesWihCasts()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
[Register(typeof(C), Scope.SingleInstance, typeof(C), typeof(IC))]
[Register(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A 
{
    public A(B b, IC c){}
}
public class B 
{
    public B(IC c, D d){}
}
public class C : IC {}
public class D 
{
    public D(C c){}
}
public interface IC {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::C _cField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::C GetCField0()
    {
        if (!object.ReferenceEquals(_cField0, null))
            return _cField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::C c_0_0;
            c_0_0 = new global::C();
            this._cField0 = c_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _cField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_3;
        global::IC iC_0_2;
        global::D d_0_4;
        global::B b_0_1;
        global::A a_0_0;
        c_0_3 = GetCField0();
        iC_0_2 = (global::IC)c_0_3;
        d_0_4 = new global::D(c: c_0_3);
        b_0_1 = new global::B(c: iC_0_2, d: d_0_4);
        a_0_0 = new global::A(b: b_0_1, c: iC_0_2);
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_3;
        global::IC iC_0_2;
        global::D d_0_4;
        global::B b_0_1;
        global::A a_0_0;
        c_0_3 = GetCField0();
        iC_0_2 = (global::IC)c_0_3;
        d_0_4 = new global::D(c: c_0_3);
        b_0_1 = new global::B(c: iC_0_2, d: d_0_4);
        a_0_0 = new global::A(b: b_0_1, c: iC_0_2);
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void SingleInstanceDependenciesWithRequiresInitialization()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A), Scope.SingleInstance)]
[Register(typeof(B))]
[Register(typeof(C), Scope.SingleInstance)]
[Register(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
}

public class A : IRequiresAsyncInitialization
{
    public A(B b, C c){}

    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
public class B 
{
    public B(C c, D d){}
}
public class C : IRequiresAsyncInitialization { public ValueTask InitializeAsync()  => new ValueTask();  }
public class D : E
{
    public D(C c){}
}

public class E : IRequiresAsyncInitialization
{
    ValueTask IRequiresAsyncInitialization.InitializeAsync() => new ValueTask();
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }
    }

    private global::A _aField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::C _cField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private async global::System.Threading.Tasks.ValueTask<global::C> GetCField1()
    {
        if (!object.ReferenceEquals(_cField1, null))
            return _cField1;
        await this._lock1.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::C c_0_0;
            global::System.Threading.Tasks.ValueTask c_0_1;
            var hasAwaitStarted_c_0_1 = false;
            c_0_0 = new global::C();
            c_0_1 = ((global::StrongInject.IRequiresAsyncInitialization)c_0_0).InitializeAsync();
            try
            {
                hasAwaitStarted_c_0_1 = true;
                await c_0_1;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_1)
                {
                    _ = c_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._cField1 = c_0_0;
            this._disposeAction1 = async () =>
            {
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _cField1;
    }

    private async global::System.Threading.Tasks.ValueTask<global::A> GetAField0()
    {
        if (!object.ReferenceEquals(_aField0, null))
            return _aField0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::C> c_0_2;
            var hasAwaitStarted_c_0_2 = false;
            var c_0_3 = default(global::C);
            global::D d_0_4;
            global::System.Threading.Tasks.ValueTask d_0_5;
            var hasAwaitStarted_d_0_5 = false;
            global::B b_0_1;
            global::A a_0_0;
            global::System.Threading.Tasks.ValueTask a_0_6;
            var hasAwaitStarted_a_0_6 = false;
            c_0_2 = GetCField1();
            try
            {
                hasAwaitStarted_c_0_2 = true;
                c_0_3 = await c_0_2;
                d_0_4 = new global::D(c: c_0_3);
                d_0_5 = ((global::StrongInject.IRequiresAsyncInitialization)d_0_4).InitializeAsync();
                try
                {
                    hasAwaitStarted_d_0_5 = true;
                    await d_0_5;
                    b_0_1 = new global::B(c: c_0_3, d: d_0_4);
                    a_0_0 = new global::A(b: b_0_1, c: c_0_3);
                    a_0_6 = ((global::StrongInject.IRequiresAsyncInitialization)a_0_0).InitializeAsync();
                    try
                    {
                        hasAwaitStarted_a_0_6 = true;
                        await a_0_6;
                    }
                    catch
                    {
                        if (!hasAwaitStarted_a_0_6)
                        {
                            _ = a_0_6.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                        }

                        throw;
                    }
                }
                catch
                {
                    if (!hasAwaitStarted_d_0_5)
                    {
                        _ = d_0_5.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                    }

                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_2)
                {
                    _ = c_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._aField0 = a_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _aField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::A> a_0_0;
        var hasAwaitStarted_a_0_0 = false;
        var a_0_1 = default(global::A);
        a_0_0 = GetAField0();
        try
        {
            hasAwaitStarted_a_0_0 = true;
            a_0_1 = await a_0_0;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_0)
            {
                _ = a_0_0.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(a_0_1, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::A> a_0_0;
        var hasAwaitStarted_a_0_0 = false;
        var a_0_1 = default(global::A);
        a_0_0 = GetAField0();
        try
        {
            hasAwaitStarted_a_0_0 = true;
            a_0_1 = await a_0_0;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_0)
            {
                _ = a_0_0.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::A>(a_0_1, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void SingleInstanceDependenciesWithFactories()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[RegisterFactory(typeof(A), Scope.SingleInstance, Scope.InstancePerResolution)]
[RegisterFactory(typeof(B), Scope.SingleInstance, Scope.SingleInstance)]
[RegisterFactory(typeof(C), Scope.InstancePerResolution, Scope.SingleInstance)]
[RegisterFactory(typeof(D), Scope.InstancePerResolution, Scope.InstancePerResolution)]
[Register(typeof(C), Scope.InstancePerResolution, typeof(C))]
public partial class Container : IAsyncContainer<AFactoryTarget>
{
}

public class A : IAsyncFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c){}
    ValueTask<AFactoryTarget> IAsyncFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IAsyncFactory<BFactoryTarget>
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IAsyncFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
}
public class BFactoryTarget {}
public class C : IAsyncFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IAsyncFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IAsyncFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IAsyncFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (9,2): Warning SI1001: 'C' implements 'StrongInject.IAsyncFactory<CFactoryTarget>'. Did you mean to use FactoryRegistration instead?
                // Register(typeof(C), Scope.InstancePerResolution, typeof(C))
                new DiagnosticResult("SI1001", @"Register(typeof(C), Scope.InstancePerResolution, typeof(C))", DiagnosticSeverity.Warning).WithLocation(9, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }

        await this._lock2.WaitAsync();
        try
        {
            await (this._disposeAction2?.Invoke() ?? default);
        }
        finally
        {
            this._lock2.Release();
        }

        await this._lock3.WaitAsync();
        try
        {
            await (this._disposeAction3?.Invoke() ?? default);
        }
        finally
        {
            this._lock3.Release();
        }
    }

    private global::A _aField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::BFactoryTarget _bFactoryTargetField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private global::B _bField2;
    private global::System.Threading.SemaphoreSlim _lock2 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction2;
    private global::CFactoryTarget _cFactoryTargetField3;
    private global::System.Threading.SemaphoreSlim _lock3 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction3;
    private async global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> GetCFactoryTargetField3()
    {
        if (!object.ReferenceEquals(_cFactoryTargetField3, null))
            return _cFactoryTargetField3;
        await this._lock3.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::C c_0_2;
            global::StrongInject.IAsyncFactory<global::CFactoryTarget> iAsyncFactory_0_1;
            global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_3;
            var hasAwaitStarted_cFactoryTarget_0_3 = false;
            var cFactoryTarget_0_0 = default(global::CFactoryTarget);
            var hasAwaitCompleted_cFactoryTarget_0_3 = false;
            c_0_2 = new global::C();
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::CFactoryTarget>)c_0_2;
            cFactoryTarget_0_3 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_cFactoryTarget_0_3 = true;
                cFactoryTarget_0_0 = await cFactoryTarget_0_3;
                hasAwaitCompleted_cFactoryTarget_0_3 = true;
            }
            catch
            {
                if (!hasAwaitStarted_cFactoryTarget_0_3)
                {
                    cFactoryTarget_0_0 = await cFactoryTarget_0_3;
                }
                else if (!hasAwaitCompleted_cFactoryTarget_0_3)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(cFactoryTarget_0_0);
                throw;
            }

            this._cFactoryTargetField3 = cFactoryTarget_0_0;
            this._disposeAction3 = async () =>
            {
                await iAsyncFactory_0_1.ReleaseAsync(cFactoryTarget_0_0);
            };
        }
        finally
        {
            this._lock3.Release();
        }

        return _cFactoryTargetField3;
    }

    private async global::System.Threading.Tasks.ValueTask<global::B> GetBField2()
    {
        if (!object.ReferenceEquals(_bField2, null))
            return _bField2;
        await this._lock2.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_5;
            global::C c_0_1;
            var hasAwaitStarted_cFactoryTarget_0_5 = false;
            var cFactoryTarget_0_6 = default(global::CFactoryTarget);
            global::D d_0_4;
            global::StrongInject.IAsyncFactory<global::DFactoryTarget> iAsyncFactory_0_3;
            global::System.Threading.Tasks.ValueTask<global::DFactoryTarget> dFactoryTarget_0_7;
            var hasAwaitStarted_dFactoryTarget_0_7 = false;
            var dFactoryTarget_0_2 = default(global::DFactoryTarget);
            var hasAwaitCompleted_dFactoryTarget_0_7 = false;
            global::B b_0_0;
            cFactoryTarget_0_5 = GetCFactoryTargetField3();
            try
            {
                c_0_1 = new global::C();
                hasAwaitStarted_cFactoryTarget_0_5 = true;
                cFactoryTarget_0_6 = await cFactoryTarget_0_5;
                d_0_4 = new global::D(c: cFactoryTarget_0_6);
                iAsyncFactory_0_3 = (global::StrongInject.IAsyncFactory<global::DFactoryTarget>)d_0_4;
                dFactoryTarget_0_7 = iAsyncFactory_0_3.CreateAsync();
                try
                {
                    hasAwaitStarted_dFactoryTarget_0_7 = true;
                    dFactoryTarget_0_2 = await dFactoryTarget_0_7;
                    hasAwaitCompleted_dFactoryTarget_0_7 = true;
                    b_0_0 = new global::B(c: c_0_1, d: dFactoryTarget_0_2);
                }
                catch
                {
                    if (!hasAwaitStarted_dFactoryTarget_0_7)
                    {
                        dFactoryTarget_0_2 = await dFactoryTarget_0_7;
                    }
                    else if (!hasAwaitCompleted_dFactoryTarget_0_7)
                    {
                        throw;
                    }

                    await iAsyncFactory_0_3.ReleaseAsync(dFactoryTarget_0_2);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_cFactoryTarget_0_5)
                {
                    _ = cFactoryTarget_0_5.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._bField2 = b_0_0;
            this._disposeAction2 = async () =>
            {
                await iAsyncFactory_0_3.ReleaseAsync(dFactoryTarget_0_2);
            };
        }
        finally
        {
            this._lock2.Release();
        }

        return _bField2;
    }

    private async global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> GetBFactoryTargetField1()
    {
        if (!object.ReferenceEquals(_bFactoryTargetField1, null))
            return _bFactoryTargetField1;
        await this._lock1.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::B> b_0_2;
            var hasAwaitStarted_b_0_2 = false;
            var b_0_3 = default(global::B);
            global::StrongInject.IAsyncFactory<global::BFactoryTarget> iAsyncFactory_0_1;
            global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> bFactoryTarget_0_4;
            var hasAwaitStarted_bFactoryTarget_0_4 = false;
            var bFactoryTarget_0_0 = default(global::BFactoryTarget);
            var hasAwaitCompleted_bFactoryTarget_0_4 = false;
            b_0_2 = GetBField2();
            try
            {
                hasAwaitStarted_b_0_2 = true;
                b_0_3 = await b_0_2;
                iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::BFactoryTarget>)b_0_3;
                bFactoryTarget_0_4 = iAsyncFactory_0_1.CreateAsync();
                try
                {
                    hasAwaitStarted_bFactoryTarget_0_4 = true;
                    bFactoryTarget_0_0 = await bFactoryTarget_0_4;
                    hasAwaitCompleted_bFactoryTarget_0_4 = true;
                }
                catch
                {
                    if (!hasAwaitStarted_bFactoryTarget_0_4)
                    {
                        bFactoryTarget_0_0 = await bFactoryTarget_0_4;
                    }
                    else if (!hasAwaitCompleted_bFactoryTarget_0_4)
                    {
                        throw;
                    }

                    await iAsyncFactory_0_1.ReleaseAsync(bFactoryTarget_0_0);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_b_0_2)
                {
                    _ = b_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._bFactoryTargetField1 = bFactoryTarget_0_0;
            this._disposeAction1 = async () =>
            {
                await iAsyncFactory_0_1.ReleaseAsync(bFactoryTarget_0_0);
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _bFactoryTargetField1;
    }

    private async global::System.Threading.Tasks.ValueTask<global::A> GetAField0()
    {
        if (!object.ReferenceEquals(_aField0, null))
            return _aField0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> bFactoryTarget_0_1;
            global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_3;
            var hasAwaitStarted_bFactoryTarget_0_1 = false;
            var bFactoryTarget_0_2 = default(global::BFactoryTarget);
            var hasAwaitStarted_cFactoryTarget_0_3 = false;
            var cFactoryTarget_0_4 = default(global::CFactoryTarget);
            global::A a_0_0;
            bFactoryTarget_0_1 = GetBFactoryTargetField1();
            try
            {
                cFactoryTarget_0_3 = GetCFactoryTargetField3();
                try
                {
                    hasAwaitStarted_bFactoryTarget_0_1 = true;
                    bFactoryTarget_0_2 = await bFactoryTarget_0_1;
                    hasAwaitStarted_cFactoryTarget_0_3 = true;
                    cFactoryTarget_0_4 = await cFactoryTarget_0_3;
                    a_0_0 = new global::A(b: bFactoryTarget_0_2, c: cFactoryTarget_0_4);
                }
                catch
                {
                    if (!hasAwaitStarted_cFactoryTarget_0_3)
                    {
                        _ = cFactoryTarget_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                    }

                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_bFactoryTarget_0_1)
                {
                    _ = bFactoryTarget_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._aField0 = a_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _aField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::A> a_0_2;
        var hasAwaitStarted_a_0_2 = false;
        var a_0_3 = default(global::A);
        global::StrongInject.IAsyncFactory<global::AFactoryTarget> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::AFactoryTarget> aFactoryTarget_0_4;
        var hasAwaitStarted_aFactoryTarget_0_4 = false;
        var aFactoryTarget_0_0 = default(global::AFactoryTarget);
        var hasAwaitCompleted_aFactoryTarget_0_4 = false;
        a_0_2 = GetAField0();
        try
        {
            hasAwaitStarted_a_0_2 = true;
            a_0_3 = await a_0_2;
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::AFactoryTarget>)a_0_3;
            aFactoryTarget_0_4 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_aFactoryTarget_0_4 = true;
                aFactoryTarget_0_0 = await aFactoryTarget_0_4;
                hasAwaitCompleted_aFactoryTarget_0_4 = true;
            }
            catch
            {
                if (!hasAwaitStarted_aFactoryTarget_0_4)
                {
                    aFactoryTarget_0_0 = await aFactoryTarget_0_4;
                }
                else if (!hasAwaitCompleted_aFactoryTarget_0_4)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_a_0_2)
            {
                _ = a_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(aFactoryTarget_0_0, param);
        }
        finally
        {
            await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::AFactoryTarget>> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::A> a_0_2;
        var hasAwaitStarted_a_0_2 = false;
        var a_0_3 = default(global::A);
        global::StrongInject.IAsyncFactory<global::AFactoryTarget> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::AFactoryTarget> aFactoryTarget_0_4;
        var hasAwaitStarted_aFactoryTarget_0_4 = false;
        var aFactoryTarget_0_0 = default(global::AFactoryTarget);
        var hasAwaitCompleted_aFactoryTarget_0_4 = false;
        a_0_2 = GetAField0();
        try
        {
            hasAwaitStarted_a_0_2 = true;
            a_0_3 = await a_0_2;
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::AFactoryTarget>)a_0_3;
            aFactoryTarget_0_4 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_aFactoryTarget_0_4 = true;
                aFactoryTarget_0_0 = await aFactoryTarget_0_4;
                hasAwaitCompleted_aFactoryTarget_0_4 = true;
            }
            catch
            {
                if (!hasAwaitStarted_aFactoryTarget_0_4)
                {
                    aFactoryTarget_0_0 = await aFactoryTarget_0_4;
                }
                else if (!hasAwaitCompleted_aFactoryTarget_0_4)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_a_0_2)
            {
                _ = a_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::AFactoryTarget>(aFactoryTarget_0_0, async () =>
        {
            await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
        });
    }
}");
        }

        [Fact]
        public void MultipleResolvesShareSingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
[Register(typeof(C), Scope.SingleInstance, typeof(C), typeof(IC))]
[Register(typeof(D))]
public partial class Container : IAsyncContainer<A>, IAsyncContainer<B>
{
}

public class A 
{
    public A(IC c){}
}
public class B 
{
    public B(C c, D d){}
}
public class C : IC {}
public class D 
{
    public D(C c){}
}
public interface IC {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::C _cField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::C GetCField0()
    {
        if (!object.ReferenceEquals(_cField0, null))
            return _cField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::C c_0_0;
            c_0_0 = new global::C();
            this._cField0 = c_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _cField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_2;
        global::IC iC_0_1;
        global::A a_0_0;
        c_0_2 = GetCField0();
        iC_0_1 = (global::IC)c_0_2;
        a_0_0 = new global::A(c: iC_0_1);
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_2;
        global::IC iC_0_1;
        global::A a_0_0;
        c_0_2 = GetCField0();
        iC_0_1 = (global::IC)c_0_2;
        a_0_0 = new global::A(c: iC_0_1);
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::B>.RunAsync<TResult, TParam>(global::System.Func<global::B, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::D d_0_2;
        global::B b_0_0;
        c_0_1 = GetCField0();
        d_0_2 = new global::D(c: c_0_1);
        b_0_0 = new global::B(c: c_0_1, d: d_0_2);
        TResult result;
        try
        {
            result = await func(b_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::B>> global::StrongInject.IAsyncContainer<global::B>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::D d_0_2;
        global::B b_0_0;
        c_0_1 = GetCField0();
        d_0_2 = new global::D(c: c_0_1);
        b_0_0 = new global::B(c: c_0_1, d: d_0_2);
        return new global::StrongInject.AsyncOwned<global::B>(b_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ReportMissingTypes()
        {
            string userSource = @"";
            var comp = RunGenerator(userSource, out var generatorDiagnostics, out _);
            generatorDiagnostics.Verify(
                // (1,1): Error SI0201: Missing Type 'StrongInject.IContainer`1[T]'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.IAsyncContainer`1'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.IFactory`1[T]'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.IAsyncFactory`1[T]'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.IRequiresInitialization'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.IRequiresAsyncInitialization'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.Owned`1[T]'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.AsyncOwned`1'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.RegisterAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.RegisterModuleAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.RegisterFactoryAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.RegisterDecoratorAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.FactoryAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.DecoratorFactoryAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.FactoryOfAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.InstanceAttribute'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1),
                // (1,1): Error SI0201: Missing Type 'StrongInject.Helpers'. Are you missing an assembly reference?
                // Missing Type.SI0201
                new DiagnosticResult("SI0201", @"<UNKNOWN>", DiagnosticSeverity.Error).WithLocation(1, 1));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void ErrorIfInstanceUsedAsFactoryDuplicatesContainerRegistration()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A))]
[Register(typeof(B))]
[Register(typeof(C))]
[Register(typeof(D))]
public partial class Container : IAsyncContainer<A>
{
    [Instance(Options.AsImplementedInterfacesAndUseAsFactory)] public InstanceFactory _instanceFactory;
}

public class A
{
    public A(B b, IC c){}
}
public class B 
{
    public B(C c, D d){}
}
public class C : IC {}
public interface IC {}
public class D
{
    public D(C c){}
}

public class InstanceFactory : IAsyncFactory<IC>, IAsyncFactory<D>
{
    public ValueTask<IC> CreateAsync() => throw null;
    ValueTask<D> IAsyncFactory<D>.CreateAsync() => throw null;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (9,22): Error SI0106: Error while resolving dependencies for 'A': We have multiple sources for instance of type 'D' and no best source. Try adding a single registration for 'D' directly to the container, and moving any existing registrations for 'D' on the container to an imported module.
                // Container
                new DiagnosticResult("SI0106", @"Container", DiagnosticSeverity.Error).WithLocation(9, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void CorrectDisposal()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;
using System;

[RegisterFactory(typeof(A))]
[RegisterFactory(typeof(B), Scope.SingleInstance, Scope.SingleInstance)]
[RegisterFactory(typeof(C), Scope.InstancePerResolution, Scope.SingleInstance)]
[RegisterFactory(typeof(D), Scope.InstancePerResolution, Scope.InstancePerResolution)]
[Register(typeof(C))]
[Register(typeof(E))]
[Register(typeof(F))]
[Register(typeof(G))]
[Register(typeof(H))]
[Register(typeof(I), Scope.SingleInstance)]
public partial class Container : IAsyncContainer<AFactoryTarget>
{
    [Instance(Options.UseAsFactory)] IAsyncFactory<int> _factory;
}

public class A : IAsyncFactory<AFactoryTarget>
{
    public A(BFactoryTarget b, CFactoryTarget c, E e, int i){}
    ValueTask<AFactoryTarget> IAsyncFactory<AFactoryTarget>.CreateAsync() => new ValueTask<AFactoryTarget>(new AFactoryTarget());
}
public class AFactoryTarget {}
public class B : IAsyncFactory<BFactoryTarget>, IDisposable
{
    public B(C c, DFactoryTarget d){}
    ValueTask<BFactoryTarget> IAsyncFactory<BFactoryTarget>.CreateAsync() => new ValueTask<BFactoryTarget>(new BFactoryTarget());
    public void Dispose() {}
}
public class BFactoryTarget {}
public class C : IAsyncFactory<CFactoryTarget> 
{
    ValueTask<CFactoryTarget> IAsyncFactory<CFactoryTarget>.CreateAsync() => new ValueTask<CFactoryTarget>(new CFactoryTarget());
}
public class CFactoryTarget {}
public class D : IAsyncFactory<DFactoryTarget>
{
    public D(CFactoryTarget c){}
    ValueTask<DFactoryTarget> IAsyncFactory<DFactoryTarget>.CreateAsync() => new ValueTask<DFactoryTarget>(new DFactoryTarget());
}
public class DFactoryTarget {}
public class E : IDisposable { public E(F f) {} public void Dispose() {} }
public class F : IAsyncDisposable { public F(G g) {} ValueTask IAsyncDisposable.DisposeAsync() => default; }
public class G : IDisposable, IAsyncDisposable { public G(H h) {} void IDisposable.Dispose() {} public ValueTask DisposeAsync() => default; }
public class H { public H(I i) {} }
public class I : IDisposable { public I(int i) {} public void Dispose() {} }
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (10,2): Warning SI1001: 'C' implements 'StrongInject.IAsyncFactory<CFactoryTarget>'. Did you mean to use FactoryRegistration instead?
                // Register(typeof(C))
                new DiagnosticResult("SI1001", @"Register(typeof(C))", DiagnosticSeverity.Warning).WithLocation(10, 2));
            comp.GetDiagnostics().Verify(
                // (18,57): Warning CS0649: Field 'Container._factory' is never assigned to, and will always have its default value null
                // _factory
                new DiagnosticResult("CS0649", @"_factory", DiagnosticSeverity.Warning).WithLocation(18, 57));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock3.WaitAsync();
        try
        {
            await (this._disposeAction3?.Invoke() ?? default);
        }
        finally
        {
            this._lock3.Release();
        }

        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }

        await this._lock2.WaitAsync();
        try
        {
            await (this._disposeAction2?.Invoke() ?? default);
        }
        finally
        {
            this._lock2.Release();
        }
    }

    private global::BFactoryTarget _bFactoryTargetField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::B _bField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private global::CFactoryTarget _cFactoryTargetField2;
    private global::System.Threading.SemaphoreSlim _lock2 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction2;
    private async global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> GetCFactoryTargetField2()
    {
        if (!object.ReferenceEquals(_cFactoryTargetField2, null))
            return _cFactoryTargetField2;
        await this._lock2.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::C c_0_2;
            global::StrongInject.IAsyncFactory<global::CFactoryTarget> iAsyncFactory_0_1;
            global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_3;
            var hasAwaitStarted_cFactoryTarget_0_3 = false;
            var cFactoryTarget_0_0 = default(global::CFactoryTarget);
            var hasAwaitCompleted_cFactoryTarget_0_3 = false;
            c_0_2 = new global::C();
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::CFactoryTarget>)c_0_2;
            cFactoryTarget_0_3 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_cFactoryTarget_0_3 = true;
                cFactoryTarget_0_0 = await cFactoryTarget_0_3;
                hasAwaitCompleted_cFactoryTarget_0_3 = true;
            }
            catch
            {
                if (!hasAwaitStarted_cFactoryTarget_0_3)
                {
                    cFactoryTarget_0_0 = await cFactoryTarget_0_3;
                }
                else if (!hasAwaitCompleted_cFactoryTarget_0_3)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(cFactoryTarget_0_0);
                throw;
            }

            this._cFactoryTargetField2 = cFactoryTarget_0_0;
            this._disposeAction2 = async () =>
            {
                await iAsyncFactory_0_1.ReleaseAsync(cFactoryTarget_0_0);
            };
        }
        finally
        {
            this._lock2.Release();
        }

        return _cFactoryTargetField2;
    }

    private async global::System.Threading.Tasks.ValueTask<global::B> GetBField1()
    {
        if (!object.ReferenceEquals(_bField1, null))
            return _bField1;
        await this._lock1.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_5;
            global::C c_0_1;
            var hasAwaitStarted_cFactoryTarget_0_5 = false;
            var cFactoryTarget_0_6 = default(global::CFactoryTarget);
            global::D d_0_4;
            global::StrongInject.IAsyncFactory<global::DFactoryTarget> iAsyncFactory_0_3;
            global::System.Threading.Tasks.ValueTask<global::DFactoryTarget> dFactoryTarget_0_7;
            var hasAwaitStarted_dFactoryTarget_0_7 = false;
            var dFactoryTarget_0_2 = default(global::DFactoryTarget);
            var hasAwaitCompleted_dFactoryTarget_0_7 = false;
            global::B b_0_0;
            cFactoryTarget_0_5 = GetCFactoryTargetField2();
            try
            {
                c_0_1 = new global::C();
                hasAwaitStarted_cFactoryTarget_0_5 = true;
                cFactoryTarget_0_6 = await cFactoryTarget_0_5;
                d_0_4 = new global::D(c: cFactoryTarget_0_6);
                iAsyncFactory_0_3 = (global::StrongInject.IAsyncFactory<global::DFactoryTarget>)d_0_4;
                dFactoryTarget_0_7 = iAsyncFactory_0_3.CreateAsync();
                try
                {
                    hasAwaitStarted_dFactoryTarget_0_7 = true;
                    dFactoryTarget_0_2 = await dFactoryTarget_0_7;
                    hasAwaitCompleted_dFactoryTarget_0_7 = true;
                    b_0_0 = new global::B(c: c_0_1, d: dFactoryTarget_0_2);
                }
                catch
                {
                    if (!hasAwaitStarted_dFactoryTarget_0_7)
                    {
                        dFactoryTarget_0_2 = await dFactoryTarget_0_7;
                    }
                    else if (!hasAwaitCompleted_dFactoryTarget_0_7)
                    {
                        throw;
                    }

                    await iAsyncFactory_0_3.ReleaseAsync(dFactoryTarget_0_2);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_cFactoryTarget_0_5)
                {
                    _ = cFactoryTarget_0_5.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._bField1 = b_0_0;
            this._disposeAction1 = async () =>
            {
                ((global::System.IDisposable)b_0_0).Dispose();
                await iAsyncFactory_0_3.ReleaseAsync(dFactoryTarget_0_2);
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _bField1;
    }

    private async global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> GetBFactoryTargetField0()
    {
        if (!object.ReferenceEquals(_bFactoryTargetField0, null))
            return _bFactoryTargetField0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::B> b_0_2;
            var hasAwaitStarted_b_0_2 = false;
            var b_0_3 = default(global::B);
            global::StrongInject.IAsyncFactory<global::BFactoryTarget> iAsyncFactory_0_1;
            global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> bFactoryTarget_0_4;
            var hasAwaitStarted_bFactoryTarget_0_4 = false;
            var bFactoryTarget_0_0 = default(global::BFactoryTarget);
            var hasAwaitCompleted_bFactoryTarget_0_4 = false;
            b_0_2 = GetBField1();
            try
            {
                hasAwaitStarted_b_0_2 = true;
                b_0_3 = await b_0_2;
                iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::BFactoryTarget>)b_0_3;
                bFactoryTarget_0_4 = iAsyncFactory_0_1.CreateAsync();
                try
                {
                    hasAwaitStarted_bFactoryTarget_0_4 = true;
                    bFactoryTarget_0_0 = await bFactoryTarget_0_4;
                    hasAwaitCompleted_bFactoryTarget_0_4 = true;
                }
                catch
                {
                    if (!hasAwaitStarted_bFactoryTarget_0_4)
                    {
                        bFactoryTarget_0_0 = await bFactoryTarget_0_4;
                    }
                    else if (!hasAwaitCompleted_bFactoryTarget_0_4)
                    {
                        throw;
                    }

                    await iAsyncFactory_0_1.ReleaseAsync(bFactoryTarget_0_0);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_b_0_2)
                {
                    _ = b_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._bFactoryTargetField0 = bFactoryTarget_0_0;
            this._disposeAction0 = async () =>
            {
                await iAsyncFactory_0_1.ReleaseAsync(bFactoryTarget_0_0);
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _bFactoryTargetField0;
    }

    private global::I _iField3;
    private global::System.Threading.SemaphoreSlim _lock3 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction3;
    private async global::System.Threading.Tasks.ValueTask<global::I> GetIField3()
    {
        if (!object.ReferenceEquals(_iField3, null))
            return _iField3;
        await this._lock3.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::StrongInject.IAsyncFactory<global::System.Int32> iAsyncFactory_0_2;
            global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_3;
            var hasAwaitStarted_int32_0_3 = false;
            var int32_0_1 = default(global::System.Int32);
            var hasAwaitCompleted_int32_0_3 = false;
            global::I i_0_0;
            iAsyncFactory_0_2 = this._factory;
            int32_0_3 = iAsyncFactory_0_2.CreateAsync();
            try
            {
                hasAwaitStarted_int32_0_3 = true;
                int32_0_1 = await int32_0_3;
                hasAwaitCompleted_int32_0_3 = true;
                i_0_0 = new global::I(i: int32_0_1);
            }
            catch
            {
                if (!hasAwaitStarted_int32_0_3)
                {
                    int32_0_1 = await int32_0_3;
                }
                else if (!hasAwaitCompleted_int32_0_3)
                {
                    throw;
                }

                await iAsyncFactory_0_2.ReleaseAsync(int32_0_1);
                throw;
            }

            this._iField3 = i_0_0;
            this._disposeAction3 = async () =>
            {
                ((global::System.IDisposable)i_0_0).Dispose();
                await iAsyncFactory_0_2.ReleaseAsync(int32_0_1);
            };
        }
        finally
        {
            this._lock3.Release();
        }

        return _iField3;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.RunAsync<TResult, TParam>(global::System.Func<global::AFactoryTarget, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> bFactoryTarget_0_3;
        global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_5;
        global::System.Threading.Tasks.ValueTask<global::I> i_0_11;
        global::StrongInject.IAsyncFactory<global::System.Int32> iAsyncFactory_0_14;
        global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_15;
        var hasAwaitStarted_bFactoryTarget_0_3 = false;
        var bFactoryTarget_0_4 = default(global::BFactoryTarget);
        var hasAwaitStarted_cFactoryTarget_0_5 = false;
        var cFactoryTarget_0_6 = default(global::CFactoryTarget);
        var hasAwaitStarted_i_0_11 = false;
        var i_0_12 = default(global::I);
        global::H h_0_10;
        global::G g_0_9;
        global::F f_0_8;
        global::E e_0_7;
        var hasAwaitStarted_int32_0_15 = false;
        var int32_0_13 = default(global::System.Int32);
        var hasAwaitCompleted_int32_0_15 = false;
        global::A a_0_2;
        global::StrongInject.IAsyncFactory<global::AFactoryTarget> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::AFactoryTarget> aFactoryTarget_0_16;
        var hasAwaitStarted_aFactoryTarget_0_16 = false;
        var aFactoryTarget_0_0 = default(global::AFactoryTarget);
        var hasAwaitCompleted_aFactoryTarget_0_16 = false;
        bFactoryTarget_0_3 = GetBFactoryTargetField0();
        try
        {
            cFactoryTarget_0_5 = GetCFactoryTargetField2();
            try
            {
                i_0_11 = GetIField3();
                try
                {
                    iAsyncFactory_0_14 = this._factory;
                    int32_0_15 = iAsyncFactory_0_14.CreateAsync();
                    try
                    {
                        hasAwaitStarted_bFactoryTarget_0_3 = true;
                        bFactoryTarget_0_4 = await bFactoryTarget_0_3;
                        hasAwaitStarted_cFactoryTarget_0_5 = true;
                        cFactoryTarget_0_6 = await cFactoryTarget_0_5;
                        hasAwaitStarted_i_0_11 = true;
                        i_0_12 = await i_0_11;
                        h_0_10 = new global::H(i: i_0_12);
                        g_0_9 = new global::G(h: h_0_10);
                        try
                        {
                            f_0_8 = new global::F(g: g_0_9);
                            try
                            {
                                e_0_7 = new global::E(f: f_0_8);
                                try
                                {
                                    hasAwaitStarted_int32_0_15 = true;
                                    int32_0_13 = await int32_0_15;
                                    hasAwaitCompleted_int32_0_15 = true;
                                    a_0_2 = new global::A(b: bFactoryTarget_0_4, c: cFactoryTarget_0_6, e: e_0_7, i: int32_0_13);
                                    iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::AFactoryTarget>)a_0_2;
                                    aFactoryTarget_0_16 = iAsyncFactory_0_1.CreateAsync();
                                    try
                                    {
                                        hasAwaitStarted_aFactoryTarget_0_16 = true;
                                        aFactoryTarget_0_0 = await aFactoryTarget_0_16;
                                        hasAwaitCompleted_aFactoryTarget_0_16 = true;
                                    }
                                    catch
                                    {
                                        if (!hasAwaitStarted_aFactoryTarget_0_16)
                                        {
                                            aFactoryTarget_0_0 = await aFactoryTarget_0_16;
                                        }
                                        else if (!hasAwaitCompleted_aFactoryTarget_0_16)
                                        {
                                            throw;
                                        }

                                        await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
                                        throw;
                                    }
                                }
                                catch
                                {
                                    ((global::System.IDisposable)e_0_7).Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                await ((global::System.IAsyncDisposable)f_0_8).DisposeAsync();
                                throw;
                            }
                        }
                        catch
                        {
                            await ((global::System.IAsyncDisposable)g_0_9).DisposeAsync();
                            throw;
                        }
                    }
                    catch
                    {
                        if (!hasAwaitStarted_int32_0_15)
                        {
                            int32_0_13 = await int32_0_15;
                        }
                        else if (!hasAwaitCompleted_int32_0_15)
                        {
                            throw;
                        }

                        await iAsyncFactory_0_14.ReleaseAsync(int32_0_13);
                        throw;
                    }
                }
                catch
                {
                    if (!hasAwaitStarted_i_0_11)
                    {
                        _ = i_0_11.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                    }

                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_cFactoryTarget_0_5)
                {
                    _ = cFactoryTarget_0_5.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_bFactoryTarget_0_3)
            {
                _ = bFactoryTarget_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(aFactoryTarget_0_0, param);
        }
        finally
        {
            await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
            ((global::System.IDisposable)e_0_7).Dispose();
            await ((global::System.IAsyncDisposable)f_0_8).DisposeAsync();
            await ((global::System.IAsyncDisposable)g_0_9).DisposeAsync();
            await iAsyncFactory_0_14.ReleaseAsync(int32_0_13);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::AFactoryTarget>> global::StrongInject.IAsyncContainer<global::AFactoryTarget>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::BFactoryTarget> bFactoryTarget_0_3;
        global::System.Threading.Tasks.ValueTask<global::CFactoryTarget> cFactoryTarget_0_5;
        global::System.Threading.Tasks.ValueTask<global::I> i_0_11;
        global::StrongInject.IAsyncFactory<global::System.Int32> iAsyncFactory_0_14;
        global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_15;
        var hasAwaitStarted_bFactoryTarget_0_3 = false;
        var bFactoryTarget_0_4 = default(global::BFactoryTarget);
        var hasAwaitStarted_cFactoryTarget_0_5 = false;
        var cFactoryTarget_0_6 = default(global::CFactoryTarget);
        var hasAwaitStarted_i_0_11 = false;
        var i_0_12 = default(global::I);
        global::H h_0_10;
        global::G g_0_9;
        global::F f_0_8;
        global::E e_0_7;
        var hasAwaitStarted_int32_0_15 = false;
        var int32_0_13 = default(global::System.Int32);
        var hasAwaitCompleted_int32_0_15 = false;
        global::A a_0_2;
        global::StrongInject.IAsyncFactory<global::AFactoryTarget> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::AFactoryTarget> aFactoryTarget_0_16;
        var hasAwaitStarted_aFactoryTarget_0_16 = false;
        var aFactoryTarget_0_0 = default(global::AFactoryTarget);
        var hasAwaitCompleted_aFactoryTarget_0_16 = false;
        bFactoryTarget_0_3 = GetBFactoryTargetField0();
        try
        {
            cFactoryTarget_0_5 = GetCFactoryTargetField2();
            try
            {
                i_0_11 = GetIField3();
                try
                {
                    iAsyncFactory_0_14 = this._factory;
                    int32_0_15 = iAsyncFactory_0_14.CreateAsync();
                    try
                    {
                        hasAwaitStarted_bFactoryTarget_0_3 = true;
                        bFactoryTarget_0_4 = await bFactoryTarget_0_3;
                        hasAwaitStarted_cFactoryTarget_0_5 = true;
                        cFactoryTarget_0_6 = await cFactoryTarget_0_5;
                        hasAwaitStarted_i_0_11 = true;
                        i_0_12 = await i_0_11;
                        h_0_10 = new global::H(i: i_0_12);
                        g_0_9 = new global::G(h: h_0_10);
                        try
                        {
                            f_0_8 = new global::F(g: g_0_9);
                            try
                            {
                                e_0_7 = new global::E(f: f_0_8);
                                try
                                {
                                    hasAwaitStarted_int32_0_15 = true;
                                    int32_0_13 = await int32_0_15;
                                    hasAwaitCompleted_int32_0_15 = true;
                                    a_0_2 = new global::A(b: bFactoryTarget_0_4, c: cFactoryTarget_0_6, e: e_0_7, i: int32_0_13);
                                    iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::AFactoryTarget>)a_0_2;
                                    aFactoryTarget_0_16 = iAsyncFactory_0_1.CreateAsync();
                                    try
                                    {
                                        hasAwaitStarted_aFactoryTarget_0_16 = true;
                                        aFactoryTarget_0_0 = await aFactoryTarget_0_16;
                                        hasAwaitCompleted_aFactoryTarget_0_16 = true;
                                    }
                                    catch
                                    {
                                        if (!hasAwaitStarted_aFactoryTarget_0_16)
                                        {
                                            aFactoryTarget_0_0 = await aFactoryTarget_0_16;
                                        }
                                        else if (!hasAwaitCompleted_aFactoryTarget_0_16)
                                        {
                                            throw;
                                        }

                                        await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
                                        throw;
                                    }
                                }
                                catch
                                {
                                    ((global::System.IDisposable)e_0_7).Dispose();
                                    throw;
                                }
                            }
                            catch
                            {
                                await ((global::System.IAsyncDisposable)f_0_8).DisposeAsync();
                                throw;
                            }
                        }
                        catch
                        {
                            await ((global::System.IAsyncDisposable)g_0_9).DisposeAsync();
                            throw;
                        }
                    }
                    catch
                    {
                        if (!hasAwaitStarted_int32_0_15)
                        {
                            int32_0_13 = await int32_0_15;
                        }
                        else if (!hasAwaitCompleted_int32_0_15)
                        {
                            throw;
                        }

                        await iAsyncFactory_0_14.ReleaseAsync(int32_0_13);
                        throw;
                    }
                }
                catch
                {
                    if (!hasAwaitStarted_i_0_11)
                    {
                        _ = i_0_11.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                    }

                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_cFactoryTarget_0_5)
                {
                    _ = cFactoryTarget_0_5.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_bFactoryTarget_0_3)
            {
                _ = bFactoryTarget_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::AFactoryTarget>(aFactoryTarget_0_0, async () =>
        {
            await iAsyncFactory_0_1.ReleaseAsync(aFactoryTarget_0_0);
            ((global::System.IDisposable)e_0_7).Dispose();
            await ((global::System.IAsyncDisposable)f_0_8).DisposeAsync();
            await ((global::System.IAsyncDisposable)g_0_9).DisposeAsync();
            await iAsyncFactory_0_14.ReleaseAsync(int32_0_13);
        });
    }
}");
        }

        [Fact]
        public void DisposeOfClassImplementingIDisposableAndIRequiresAsyncInitialization()
        {
            string userSource = @"
using StrongInject;
using System;
using System.Threading.Tasks;

class A : IDisposable, IRequiresAsyncInitialization
{
    public void Dispose() {}
    public ValueTask InitializeAsync() => default;
}

[Register(typeof(A))]
partial class Container : IAsyncContainer<A>
{
    
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        global::System.Threading.Tasks.ValueTask a_0_1;
        var hasAwaitStarted_a_0_1 = false;
        a_0_0 = new global::A();
        try
        {
            a_0_1 = ((global::StrongInject.IRequiresAsyncInitialization)a_0_0).InitializeAsync();
            try
            {
                hasAwaitStarted_a_0_1 = true;
                await a_0_1;
            }
            catch
            {
                if (!hasAwaitStarted_a_0_1)
                {
                    _ = a_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }
        }
        catch
        {
            ((global::System.IDisposable)a_0_0).Dispose();
            throw;
        }

        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
            ((global::System.IDisposable)a_0_0).Dispose();
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        global::System.Threading.Tasks.ValueTask a_0_1;
        var hasAwaitStarted_a_0_1 = false;
        a_0_0 = new global::A();
        try
        {
            a_0_1 = ((global::StrongInject.IRequiresAsyncInitialization)a_0_0).InitializeAsync();
            try
            {
                hasAwaitStarted_a_0_1 = true;
                await a_0_1;
            }
            catch
            {
                if (!hasAwaitStarted_a_0_1)
                {
                    _ = a_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }
        }
        catch
        {
            ((global::System.IDisposable)a_0_0).Dispose();
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
            ((global::System.IDisposable)a_0_0).Dispose();
        });
    }
}");
        }

        [Fact]
        public void GeneratesContainerInNamespace()
        {
            string userSource = @"
using StrongInject;

namespace N.O.P
{
    [Register(typeof(A))]
    public partial class Container : IAsyncContainer<A>
    {
    }

    public class A 
    {
    }
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
namespace N.O.P
{
    partial class Container
    {
        private int _disposed = 0;
        private bool Disposed => _disposed != 0;
        public async global::System.Threading.Tasks.ValueTask DisposeAsync()
        {
            var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
            if (disposed != 0)
                return;
        }

        async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::N.O.P.A>.RunAsync<TResult, TParam>(global::System.Func<global::N.O.P.A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
        {
            if (Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::N.O.P.A a_0_0;
            a_0_0 = new global::N.O.P.A();
            TResult result;
            try
            {
                result = await func(a_0_0, param);
            }
            finally
            {
            }

            return result;
        }

        async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::N.O.P.A>> global::StrongInject.IAsyncContainer<global::N.O.P.A>.ResolveAsync()
        {
            if (Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::N.O.P.A a_0_0;
            a_0_0 = new global::N.O.P.A();
            return new global::StrongInject.AsyncOwned<global::N.O.P.A>(a_0_0, async () =>
            {
            });
        }
    }
}");
        }

        [Fact]
        public void GeneratesContainerInNestedType()
        {
            string userSource = @"
using StrongInject;

namespace N.O.P
{
    public partial class Outer1
    {
        public partial class Outer2
        {
            [Register(typeof(A))]
            public partial class Container : IAsyncContainer<A>
            {
            }

            public class A 
            {
            }
        }
    }
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
namespace N.O.P
{
    partial class Outer1
    {
        partial class Outer2
        {
            partial class Container
            {
                private int _disposed = 0;
                private bool Disposed => _disposed != 0;
                public async global::System.Threading.Tasks.ValueTask DisposeAsync()
                {
                    var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
                    if (disposed != 0)
                        return;
                }

                async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::N.O.P.Outer1.Outer2.A>.RunAsync<TResult, TParam>(global::System.Func<global::N.O.P.Outer1.Outer2.A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
                {
                    if (Disposed)
                        throw new global::System.ObjectDisposedException(nameof(Container));
                    global::N.O.P.Outer1.Outer2.A a_0_0;
                    a_0_0 = new global::N.O.P.Outer1.Outer2.A();
                    TResult result;
                    try
                    {
                        result = await func(a_0_0, param);
                    }
                    finally
                    {
                    }

                    return result;
                }

                async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::N.O.P.Outer1.Outer2.A>> global::StrongInject.IAsyncContainer<global::N.O.P.Outer1.Outer2.A>.ResolveAsync()
                {
                    if (Disposed)
                        throw new global::System.ObjectDisposedException(nameof(Container));
                    global::N.O.P.Outer1.Outer2.A a_0_0;
                    a_0_0 = new global::N.O.P.Outer1.Outer2.A();
                    return new global::StrongInject.AsyncOwned<global::N.O.P.Outer1.Outer2.A>(a_0_0, async () =>
                    {
                    });
                }
            }
        }
    }
}");
        }

        [Fact]
        public void GeneratesContainerInGenericNestedType()
        {
            string userSource = @"
using StrongInject;

namespace N.O.P
{
    public partial class Outer1<T>
    {
        public partial class Outer2<T1, T2>
        {
            [Register(typeof(A))]
            public partial class Container : IAsyncContainer<A>
            {
            }
        }
    }

    public class A 
    {
    }
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
namespace N.O.P
{
    partial class Outer1<T>
    {
        partial class Outer2<T1, T2>
        {
            partial class Container
            {
                private int _disposed = 0;
                private bool Disposed => _disposed != 0;
                public async global::System.Threading.Tasks.ValueTask DisposeAsync()
                {
                    var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
                    if (disposed != 0)
                        return;
                }

                async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::N.O.P.A>.RunAsync<TResult, TParam>(global::System.Func<global::N.O.P.A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
                {
                    if (Disposed)
                        throw new global::System.ObjectDisposedException(nameof(Container));
                    global::N.O.P.A a_0_0;
                    a_0_0 = new global::N.O.P.A();
                    TResult result;
                    try
                    {
                        result = await func(a_0_0, param);
                    }
                    finally
                    {
                    }

                    return result;
                }

                async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::N.O.P.A>> global::StrongInject.IAsyncContainer<global::N.O.P.A>.ResolveAsync()
                {
                    if (Disposed)
                        throw new global::System.ObjectDisposedException(nameof(Container));
                    global::N.O.P.A a_0_0;
                    a_0_0 = new global::N.O.P.A();
                    return new global::StrongInject.AsyncOwned<global::N.O.P.A>(a_0_0, async () =>
                    {
                    });
                }
            }
        }
    }
}");
        }

        [Fact]
        public void GeneratesThrowingImplementationForContainerWithMissingDependencies()
        {
            string userSource = @"
using StrongInject;

public class A {}

partial class Container : IAsyncContainer<A>
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,15): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(6, 15));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfConstructorParameterPassedByRef()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
public partial class Container : IContainer<A>
{
}

public class A
{
    public A(ref B b){}
}
public class B{}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,2): Error SI0019: parameter 'ref B' of constructor 'A.A(ref B)' is passed as 'Ref'.
                // Register(typeof(A))
                new DiagnosticResult("SI0019", @"Register(typeof(A))", DiagnosticSeverity.Error).WithLocation(4, 2),
                // (6,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfFactoryConstructorParameterPassedByRef()
        {
            string userSource = @"
using StrongInject;

[RegisterFactory(typeof(A))]
[Register(typeof(B))]
public partial class Container : IContainer<int>
{
}

public class A : IFactory<int>
{
    public A(ref B b){}
    public int Create() => 42;
}
public class B{}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,2): Error SI0019: parameter 'ref B' of constructor 'A.A(ref B)' is passed as 'Ref'.
                // RegisterFactory(typeof(A))
                new DiagnosticResult("SI0019", @"RegisterFactory(typeof(A))", DiagnosticSeverity.Error).WithLocation(4, 2),
                // (6,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer1()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A))]
public partial class Container : IContainer<A>
{
}

public class A : IRequiresAsyncInitialization
{
    public ValueTask InitializeAsync() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0102: Error while resolving dependencies for 'A': 'A' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer2()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A))]
[Register(typeof(B))]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b){} }
public class B : IRequiresAsyncInitialization
{
    public ValueTask InitializeAsync() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (7,22): Error SI0103: Error while resolving dependencies for 'A': 'B' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(7, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer3()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[RegisterFactory(typeof(A))]
public partial class Container : IContainer<int>
{
}

public class A : IAsyncFactory<int>
{
    public ValueTask<int> CreateAsync() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0103: Error while resolving dependencies for 'int': 'int' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer4()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[RegisterFactory(typeof(A))]
public partial class Container : IContainer<int>
{
}

public class A : IFactory<int>, IRequiresAsyncInitialization
{
    public int Create() => default;
    public ValueTask InitializeAsync() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0103: Error while resolving dependencies for 'int': 'StrongInject.IFactory<int>' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer5()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<int>
{
    [Instance(Options.UseAsFactory)] public IAsyncFactory<int> _instanceProvider;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,22): Error SI0103: Error while resolving dependencies for 'int': 'int' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer6()
        {
            string userSource = @"
using StrongInject;

[RegisterFactory(typeof(A))]
public partial class Container : IContainer<int>
{
    [Instance(Options.AsEverythingPossible)] public IAsyncFactory<IFactory<B>> _instanceProvider;
}

public class A : IFactory<int>
{
    public A(B b) {}
    public int Create() => default;
}
public class B {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (5,22): Error SI0103: Error while resolving dependencies for 'int': 'B' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void ErrorIfAsyncTypeRequiredByContainer7()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[RegisterFactory(typeof(C))]
[Register(typeof(A))]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b) {} }
public class B {}
public class C : IAsyncFactory<B>
{
    public ValueTask<B> CreateAsync() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (7,22): Error SI0103: Error while resolving dependencies for 'A': 'B' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(7, 22));
            comp.GetDiagnostics().Verify();
        }

        [Fact]
        public void CanGenerateSynchronousContainer()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b){} }
public class B {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        a_0_0 = new global::A(b: b_0_1);
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        a_0_0 = new global::A(b: b_0_1);
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanGenerateSynchronousContainerWithRequiresInitialization()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
public partial class Container : IContainer<A>
{
}

public class A : IRequiresInitialization { public A(B b){} public void Initialize() {}}
public class B : IRequiresInitialization { public void Initialize() {} }
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        ((global::StrongInject.IRequiresInitialization)b_0_1).Initialize();
        a_0_0 = new global::A(b: b_0_1);
        ((global::StrongInject.IRequiresInitialization)a_0_0).Initialize();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        ((global::StrongInject.IRequiresInitialization)b_0_1).Initialize();
        a_0_0 = new global::A(b: b_0_1);
        ((global::StrongInject.IRequiresInitialization)a_0_0).Initialize();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanGenerateSynchronousContainerWithFactories()
        {
            string userSource = @"
using StrongInject;

[RegisterFactory(typeof(A))]
[Register(typeof(B))]
public partial class Container : IContainer<int>
{
}

public class A : IFactory<int> { public A(B b){} public int Create() => default; }
public class B {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_3;
        global::A a_0_2;
        global::StrongInject.IFactory<global::System.Int32> iFactory_0_1;
        global::System.Int32 int32_0_0;
        b_0_3 = new global::B();
        a_0_2 = new global::A(b: b_0_3);
        iFactory_0_1 = (global::StrongInject.IFactory<global::System.Int32>)a_0_2;
        int32_0_0 = iFactory_0_1.Create();
        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(int32_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_3;
        global::A a_0_2;
        global::StrongInject.IFactory<global::System.Int32> iFactory_0_1;
        global::System.Int32 int32_0_0;
        b_0_3 = new global::B();
        a_0_2 = new global::A(b: b_0_3);
        iFactory_0_1 = (global::StrongInject.IFactory<global::System.Int32>)a_0_2;
        int32_0_0 = iFactory_0_1.Create();
        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
            iFactory_0_1.Release(int32_0_0);
        });
    }
}");
        }

        [Fact]
        public void CanGenerateSynchronousContainerWithInstanceProviders()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
public partial class Container : IContainer<A>
{
    [Instance(Options.UseAsFactory)] IFactory<int> _instanceProvider;
}

public class A { public A(B b, int i){} }
public class B {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify(
                // (8,52): Warning CS0649: Field 'Container._instanceProvider' is never assigned to, and will always have its default value null
                // _instanceProvider
                new DiagnosticResult("CS0649", @"_instanceProvider", DiagnosticSeverity.Warning).WithLocation(8, 52));
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::StrongInject.IFactory<global::System.Int32> iFactory_0_3;
        global::System.Int32 int32_0_2;
        global::A a_0_0;
        b_0_1 = new global::B();
        iFactory_0_3 = this._instanceProvider;
        int32_0_2 = iFactory_0_3.Create();
        try
        {
            a_0_0 = new global::A(b: b_0_1, i: int32_0_2);
        }
        catch
        {
            iFactory_0_3.Release(int32_0_2);
            throw;
        }

        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
            iFactory_0_3.Release(int32_0_2);
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::StrongInject.IFactory<global::System.Int32> iFactory_0_3;
        global::System.Int32 int32_0_2;
        global::A a_0_0;
        b_0_1 = new global::B();
        iFactory_0_3 = this._instanceProvider;
        int32_0_2 = iFactory_0_3.Create();
        try
        {
            a_0_0 = new global::A(b: b_0_1, i: int32_0_2);
        }
        catch
        {
            iFactory_0_3.Release(int32_0_2);
            throw;
        }

        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
            iFactory_0_3.Release(int32_0_2);
        });
    }
}");
        }

        [Fact]
        public void CanGenerateSynchronousContainerWithSingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B), Scope.SingleInstance)]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b){} }
public class B {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::B _bField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::B GetBField0()
    {
        if (!object.ReferenceEquals(_bField0, null))
            return _bField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::B b_0_0;
            b_0_0 = new global::B();
            this._bField0 = b_0_0;
            this._disposeAction0 = () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _bField0;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = GetBField0();
        a_0_0 = new global::A(b: b_0_1);
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = GetBField0();
        a_0_0 = new global::A(b: b_0_1);
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void SynchronousAndAsynchronousResolvesCanShareSingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A))]
[Register(typeof(B), Scope.SingleInstance)]
[Register(typeof(C))]
[Register(typeof(D), Scope.SingleInstance)]
public partial class Container : IContainer<A>, IAsyncContainer<C>
{
}

public class A { public A(B b){} }
public class B {}
public class C : IRequiresAsyncInitialization { public C(B b, D d) {} public ValueTask InitializeAsync() => default; }
public class D : IRequiresAsyncInitialization { public ValueTask InitializeAsync() => default; }
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }

        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    void global::System.IDisposable.Dispose()
    {
        throw new global::StrongInject.StrongInjectException(""This container requires async disposal"");
    }

    private global::B _bField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::B GetBField0()
    {
        if (!object.ReferenceEquals(_bField0, null))
            return _bField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::B b_0_0;
            b_0_0 = new global::B();
            this._bField0 = b_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _bField0;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = GetBField0();
        a_0_0 = new global::A(b: b_0_1);
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = GetBField0();
        a_0_0 = new global::A(b: b_0_1);
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    private global::D _dField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private async global::System.Threading.Tasks.ValueTask<global::D> GetDField1()
    {
        if (!object.ReferenceEquals(_dField1, null))
            return _dField1;
        await this._lock1.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::D d_0_0;
            global::System.Threading.Tasks.ValueTask d_0_1;
            var hasAwaitStarted_d_0_1 = false;
            d_0_0 = new global::D();
            d_0_1 = ((global::StrongInject.IRequiresAsyncInitialization)d_0_0).InitializeAsync();
            try
            {
                hasAwaitStarted_d_0_1 = true;
                await d_0_1;
            }
            catch
            {
                if (!hasAwaitStarted_d_0_1)
                {
                    _ = d_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._dField1 = d_0_0;
            this._disposeAction1 = async () =>
            {
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _dField1;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::C>.RunAsync<TResult, TParam>(global::System.Func<global::C, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::D> d_0_2;
        global::B b_0_1;
        var hasAwaitStarted_d_0_2 = false;
        var d_0_3 = default(global::D);
        global::C c_0_0;
        global::System.Threading.Tasks.ValueTask c_0_4;
        var hasAwaitStarted_c_0_4 = false;
        d_0_2 = GetDField1();
        try
        {
            b_0_1 = GetBField0();
            hasAwaitStarted_d_0_2 = true;
            d_0_3 = await d_0_2;
            c_0_0 = new global::C(b: b_0_1, d: d_0_3);
            c_0_4 = ((global::StrongInject.IRequiresAsyncInitialization)c_0_0).InitializeAsync();
            try
            {
                hasAwaitStarted_c_0_4 = true;
                await c_0_4;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_4)
                {
                    _ = c_0_4.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_d_0_2)
            {
                _ = d_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(c_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::C>> global::StrongInject.IAsyncContainer<global::C>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::D> d_0_2;
        global::B b_0_1;
        var hasAwaitStarted_d_0_2 = false;
        var d_0_3 = default(global::D);
        global::C c_0_0;
        global::System.Threading.Tasks.ValueTask c_0_4;
        var hasAwaitStarted_c_0_4 = false;
        d_0_2 = GetDField1();
        try
        {
            b_0_1 = GetBField0();
            hasAwaitStarted_d_0_2 = true;
            d_0_3 = await d_0_2;
            c_0_0 = new global::C(b: b_0_1, d: d_0_3);
            c_0_4 = ((global::StrongInject.IRequiresAsyncInitialization)c_0_0).InitializeAsync();
            try
            {
                hasAwaitStarted_c_0_4 = true;
                await c_0_4;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_4)
                {
                    _ = c_0_4.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }
        }
        catch
        {
            if (!hasAwaitStarted_d_0_2)
            {
                _ = d_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::C>(c_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DisposalOfSingleInstanceDependency()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B), Scope.SingleInstance)]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b){} }
public class B : IDisposable { public void Dispose(){} }
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::B _bField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::B GetBField0()
    {
        if (!object.ReferenceEquals(_bField0, null))
            return _bField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::B b_0_0;
            b_0_0 = new global::B();
            this._bField0 = b_0_0;
            this._disposeAction0 = () =>
            {
                ((global::System.IDisposable)b_0_0).Dispose();
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _bField0;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = GetBField0();
        a_0_0 = new global::A(b: b_0_1);
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = GetBField0();
        a_0_0 = new global::A(b: b_0_1);
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DisposalOfMultipleSingleInstanceDependencies()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B), Scope.SingleInstance)]
[Register(typeof(C), Scope.SingleInstance)]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b){} }
public class B : IDisposable { public B(C c){} public void Dispose(){} }
public class C : IDisposable { public void Dispose(){} }
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }

        this._lock1.Wait();
        try
        {
            this._disposeAction1?.Invoke();
        }
        finally
        {
            this._lock1.Release();
        }
    }

    private global::B _bField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::C _cField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction1;
    private global::C GetCField1()
    {
        if (!object.ReferenceEquals(_cField1, null))
            return _cField1;
        this._lock1.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::C c_0_0;
            c_0_0 = new global::C();
            this._cField1 = c_0_0;
            this._disposeAction1 = () =>
            {
                ((global::System.IDisposable)c_0_0).Dispose();
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _cField1;
    }

    private global::B GetBField0()
    {
        if (!object.ReferenceEquals(_bField0, null))
            return _bField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::C c_0_1;
            global::B b_0_0;
            c_0_1 = GetCField1();
            b_0_0 = new global::B(c: c_0_1);
            this._bField0 = b_0_0;
            this._disposeAction0 = () =>
            {
                ((global::System.IDisposable)b_0_0).Dispose();
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _bField0;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = GetBField0();
        a_0_0 = new global::A(b: b_0_1);
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = GetBField0();
        a_0_0 = new global::A(b: b_0_1);
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DoesNotDisposeUnusedSingleInstanceDependencies()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A), Scope.SingleInstance)]
[Register(typeof(B), Scope.SingleInstance)]
[Register(typeof(C), Scope.SingleInstance)]
public partial class Container : IContainer<C>
{
}

public class A : IDisposable { public A(A a){} public void Dispose(){} }
public class B : IDisposable { public void Dispose(){} }
public class C : IDisposable { public void Dispose(){} }
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::C _cField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::C GetCField0()
    {
        if (!object.ReferenceEquals(_cField0, null))
            return _cField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::C c_0_0;
            c_0_0 = new global::C();
            this._cField0 = c_0_0;
            this._disposeAction0 = () =>
            {
                ((global::System.IDisposable)c_0_0).Dispose();
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _cField0;
    }

    TResult global::StrongInject.IContainer<global::C>.Run<TResult, TParam>(global::System.Func<global::C, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_0;
        c_0_0 = GetCField0();
        TResult result;
        try
        {
            result = func(c_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::C> global::StrongInject.IContainer<global::C>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_0;
        c_0_0 = GetCField0();
        return new global::StrongInject.Owned<global::C>(c_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanResolveFuncWithoutParameters()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
public partial class Container : IAsyncContainer<Func<A>>
{
}

public class A 
{
    public A(B b){}
}
public class B {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Func<global::A>>.RunAsync<TResult, TParam>(global::System.Func<global::System.Func<global::A>, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::A> func_0_0;
        func_0_0 = () =>
        {
            global::B b_1_1;
            global::A a_1_0;
            b_1_1 = new global::B();
            a_1_0 = new global::A(b: b_1_1);
            return a_1_0;
        };
        TResult result;
        try
        {
            result = await func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Func<global::A>>> global::StrongInject.IAsyncContainer<global::System.Func<global::A>>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::A> func_0_0;
        func_0_0 = () =>
        {
            global::B b_1_1;
            global::A a_1_0;
            b_1_1 = new global::B();
            a_1_0 = new global::A(b: b_1_1);
            return a_1_0;
        };
        return new global::StrongInject.AsyncOwned<global::System.Func<global::A>>(func_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanResolveFuncWithParametersWhereParameterTypeIsRegistered()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
public partial class Container : IContainer<Func<B, A>>
{
}

public class A 
{
    public A(B b){}
}
public class B {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::B, global::A>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::B, global::A>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::B, global::A> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::A a_1_0;
            a_1_0 = new global::A(b: param0_0);
            return a_1_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::B, global::A>> global::StrongInject.IContainer<global::System.Func<global::B, global::A>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::B, global::A> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::A a_1_0;
            a_1_0 = new global::A(b: param0_0);
            return a_1_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::B, global::A>>(func_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanResolveFuncWithParametersWhereParameterTypeIsNotRegistered()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
public partial class Container : IContainer<Func<B, A>>
{
}

public class A 
{
    public A(B b){}
}
public class B {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::B, global::A>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::B, global::A>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::B, global::A> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::A a_1_0;
            a_1_0 = new global::A(b: param0_0);
            return a_1_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::B, global::A>> global::StrongInject.IContainer<global::System.Func<global::B, global::A>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::B, global::A> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::A a_1_0;
            a_1_0 = new global::A(b: param0_0);
            return a_1_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::B, global::A>>(func_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanResolveFuncUsedAsParameter()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
public partial class Container : IContainer<A>
{
}

public class A 
{
    public A(Func<int, string, B> b){}
}
public class B { public B(int i, string s, int i1){} }
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Int32, global::System.String, global::B> func_0_1;
        global::A a_0_0;
        func_0_1 = (param0_0, param0_1) =>
        {
            global::B b_1_0;
            b_1_0 = new global::B(i: param0_0, s: param0_1, i1: param0_0);
            return b_1_0;
        };
        a_0_0 = new global::A(b: func_0_1);
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Int32, global::System.String, global::B> func_0_1;
        global::A a_0_0;
        func_0_1 = (param0_0, param0_1) =>
        {
            global::B b_1_0;
            b_1_0 = new global::B(i: param0_0, s: param0_1, i1: param0_0);
            return b_1_0;
        };
        a_0_0 = new global::A(b: func_0_1);
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanResolveFuncUsedInsideFuncResolution()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
[Register(typeof(B))]
public partial class Container : IContainer<Func<int, A>>
{
}

public class A 
{
    public A(int a, Func<string, B> func){}
}
public class B { public B(int i, string s){} }
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::System.Int32, global::A>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::System.Int32, global::A>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Int32, global::A> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::System.Func<global::System.String, global::B> func_1_1;
            global::A a_1_0;
            func_1_1 = (param1_0) =>
            {
                global::B b_2_0;
                b_2_0 = new global::B(i: param0_0, s: param1_0);
                return b_2_0;
            };
            a_1_0 = new global::A(a: param0_0, func: func_1_1);
            return a_1_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::System.Int32, global::A>> global::StrongInject.IContainer<global::System.Func<global::System.Int32, global::A>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Int32, global::A> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::System.Func<global::System.String, global::B> func_1_1;
            global::A a_1_0;
            func_1_1 = (param1_0) =>
            {
                global::B b_2_0;
                b_2_0 = new global::B(i: param0_0, s: param1_0);
                return b_2_0;
            };
            a_1_0 = new global::A(a: param0_0, func: func_1_1);
            return a_1_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::System.Int32, global::A>>(func_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanResolveFuncOfFuncOfFunc()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
public partial class Container : IContainer<Func<bool, Func<string, Func<int, A>>>>
{
}

public class A 
{
    public A(int a, string b, bool c){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::System.Boolean, global::System.Func<global::System.String, global::System.Func<global::System.Int32, global::A>>>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::System.Boolean, global::System.Func<global::System.String, global::System.Func<global::System.Int32, global::A>>>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Boolean, global::System.Func<global::System.String, global::System.Func<global::System.Int32, global::A>>> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::System.Func<global::System.String, global::System.Func<global::System.Int32, global::A>> func_1_0;
            func_1_0 = (param1_0) =>
            {
                global::System.Func<global::System.Int32, global::A> func_2_0;
                func_2_0 = (param2_0) =>
                {
                    global::A a_3_0;
                    a_3_0 = new global::A(a: param2_0, b: param1_0, c: param0_0);
                    return a_3_0;
                };
                return func_2_0;
            };
            return func_1_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::System.Boolean, global::System.Func<global::System.String, global::System.Func<global::System.Int32, global::A>>>> global::StrongInject.IContainer<global::System.Func<global::System.Boolean, global::System.Func<global::System.String, global::System.Func<global::System.Int32, global::A>>>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Boolean, global::System.Func<global::System.String, global::System.Func<global::System.Int32, global::A>>> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::System.Func<global::System.String, global::System.Func<global::System.Int32, global::A>> func_1_0;
            func_1_0 = (param1_0) =>
            {
                global::System.Func<global::System.Int32, global::A> func_2_0;
                func_2_0 = (param2_0) =>
                {
                    global::A a_3_0;
                    a_3_0 = new global::A(a: param2_0, b: param1_0, c: param0_0);
                    return a_3_0;
                };
                return func_2_0;
            };
            return func_1_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::System.Boolean, global::System.Func<global::System.String, global::System.Func<global::System.Int32, global::A>>>>(func_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DisposesOfFuncDependencies()
        {
            string userSource = @"
using System;
using StrongInject;
using StrongInject.Modules;

[Register(typeof(A))]
[Register(typeof(B))]
[RegisterModule(typeof(ValueTupleModule))]
public partial class Container : IContainer<(B, Func<A>)>
{
}

public class A : IDisposable
{
    public A(B b){}
    public void Dispose(){}
}
public class B : IDisposable
{
    public void Dispose(){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::B, global::System.Func<global::A>)>.Run<TResult, TParam>(global::System.Func<(global::B, global::System.Func<global::A>), TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::System.Collections.Concurrent.ConcurrentBag<global::System.Action> disposeActions_func_0_2;
        global::System.Func<global::A> func_0_2;
        (global::B, global::System.Func<global::A>) valueTuple_0_0;
        b_0_1 = new global::B();
        try
        {
            disposeActions_func_0_2 = new global::System.Collections.Concurrent.ConcurrentBag<global::System.Action>();
            func_0_2 = () =>
            {
                global::B b_1_1;
                global::A a_1_0;
                b_1_1 = new global::B();
                try
                {
                    a_1_0 = new global::A(b: b_1_1);
                }
                catch
                {
                    ((global::System.IDisposable)b_1_1).Dispose();
                    throw;
                }

                disposeActions_func_0_2.Add(() =>
                {
                    ((global::System.IDisposable)a_1_0).Dispose();
                    ((global::System.IDisposable)b_1_1).Dispose();
                });
                return a_1_0;
            };
            try
            {
                valueTuple_0_0 = global::StrongInject.Modules.ValueTupleModule.CreateValueTuple<global::B, global::System.Func<global::A>>(a: b_0_1, b: func_0_2);
            }
            catch
            {
                foreach (var disposeAction in disposeActions_func_0_2)
                    disposeAction();
                throw;
            }
        }
        catch
        {
            ((global::System.IDisposable)b_0_1).Dispose();
            throw;
        }

        TResult result;
        try
        {
            result = func(valueTuple_0_0, param);
        }
        finally
        {
            foreach (var disposeAction in disposeActions_func_0_2)
                disposeAction();
            ((global::System.IDisposable)b_0_1).Dispose();
        }

        return result;
    }

    global::StrongInject.Owned<(global::B, global::System.Func<global::A>)> global::StrongInject.IContainer<(global::B, global::System.Func<global::A>)>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::System.Collections.Concurrent.ConcurrentBag<global::System.Action> disposeActions_func_0_2;
        global::System.Func<global::A> func_0_2;
        (global::B, global::System.Func<global::A>) valueTuple_0_0;
        b_0_1 = new global::B();
        try
        {
            disposeActions_func_0_2 = new global::System.Collections.Concurrent.ConcurrentBag<global::System.Action>();
            func_0_2 = () =>
            {
                global::B b_1_1;
                global::A a_1_0;
                b_1_1 = new global::B();
                try
                {
                    a_1_0 = new global::A(b: b_1_1);
                }
                catch
                {
                    ((global::System.IDisposable)b_1_1).Dispose();
                    throw;
                }

                disposeActions_func_0_2.Add(() =>
                {
                    ((global::System.IDisposable)a_1_0).Dispose();
                    ((global::System.IDisposable)b_1_1).Dispose();
                });
                return a_1_0;
            };
            try
            {
                valueTuple_0_0 = global::StrongInject.Modules.ValueTupleModule.CreateValueTuple<global::B, global::System.Func<global::A>>(a: b_0_1, b: func_0_2);
            }
            catch
            {
                foreach (var disposeAction in disposeActions_func_0_2)
                    disposeAction();
                throw;
            }
        }
        catch
        {
            ((global::System.IDisposable)b_0_1).Dispose();
            throw;
        }

        return new global::StrongInject.Owned<(global::B, global::System.Func<global::A>)>(valueTuple_0_0, () =>
        {
            foreach (var disposeAction in disposeActions_func_0_2)
                disposeAction();
            ((global::System.IDisposable)b_0_1).Dispose();
        });
    }
}");
        }

        [Fact]
        public void DisposesOfFuncDependenciesAsynchronously()
        {
            string userSource = @"
using System;
using StrongInject;
using StrongInject.Modules;
using System.Threading.Tasks;

[Register(typeof(A))]
[Register(typeof(B))]
[RegisterModule(typeof(ValueTupleModule))]
public partial class Container : IAsyncContainer<(B, Func<A>)>
{
}

public class A : IAsyncDisposable
{
    public A(B b){}
    public ValueTask DisposeAsync() => default;
}
public class B : IAsyncDisposable
{
    public ValueTask DisposeAsync() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<(global::B, global::System.Func<global::A>)>.RunAsync<TResult, TParam>(global::System.Func<(global::B, global::System.Func<global::A>), TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::System.Collections.Concurrent.ConcurrentBag<global::System.Func<global::System.Threading.Tasks.ValueTask>> disposeActions_func_0_2;
        global::System.Func<global::A> func_0_2;
        (global::B, global::System.Func<global::A>) valueTuple_0_0;
        b_0_1 = new global::B();
        try
        {
            disposeActions_func_0_2 = new global::System.Collections.Concurrent.ConcurrentBag<global::System.Func<global::System.Threading.Tasks.ValueTask>>();
            func_0_2 = () =>
            {
                global::B b_1_1;
                global::A a_1_0;
                b_1_1 = new global::B();
                a_1_0 = new global::A(b: b_1_1);
                disposeActions_func_0_2.Add(async () =>
                {
                    await ((global::System.IAsyncDisposable)a_1_0).DisposeAsync();
                    await ((global::System.IAsyncDisposable)b_1_1).DisposeAsync();
                });
                return a_1_0;
            };
            try
            {
                valueTuple_0_0 = global::StrongInject.Modules.ValueTupleModule.CreateValueTuple<global::B, global::System.Func<global::A>>(a: b_0_1, b: func_0_2);
            }
            catch
            {
                foreach (var disposeAction in disposeActions_func_0_2)
                    await disposeAction();
                throw;
            }
        }
        catch
        {
            await ((global::System.IAsyncDisposable)b_0_1).DisposeAsync();
            throw;
        }

        TResult result;
        try
        {
            result = await func(valueTuple_0_0, param);
        }
        finally
        {
            foreach (var disposeAction in disposeActions_func_0_2)
                await disposeAction();
            await ((global::System.IAsyncDisposable)b_0_1).DisposeAsync();
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<(global::B, global::System.Func<global::A>)>> global::StrongInject.IAsyncContainer<(global::B, global::System.Func<global::A>)>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::System.Collections.Concurrent.ConcurrentBag<global::System.Func<global::System.Threading.Tasks.ValueTask>> disposeActions_func_0_2;
        global::System.Func<global::A> func_0_2;
        (global::B, global::System.Func<global::A>) valueTuple_0_0;
        b_0_1 = new global::B();
        try
        {
            disposeActions_func_0_2 = new global::System.Collections.Concurrent.ConcurrentBag<global::System.Func<global::System.Threading.Tasks.ValueTask>>();
            func_0_2 = () =>
            {
                global::B b_1_1;
                global::A a_1_0;
                b_1_1 = new global::B();
                a_1_0 = new global::A(b: b_1_1);
                disposeActions_func_0_2.Add(async () =>
                {
                    await ((global::System.IAsyncDisposable)a_1_0).DisposeAsync();
                    await ((global::System.IAsyncDisposable)b_1_1).DisposeAsync();
                });
                return a_1_0;
            };
            try
            {
                valueTuple_0_0 = global::StrongInject.Modules.ValueTupleModule.CreateValueTuple<global::B, global::System.Func<global::A>>(a: b_0_1, b: func_0_2);
            }
            catch
            {
                foreach (var disposeAction in disposeActions_func_0_2)
                    await disposeAction();
                throw;
            }
        }
        catch
        {
            await ((global::System.IAsyncDisposable)b_0_1).DisposeAsync();
            throw;
        }

        return new global::StrongInject.AsyncOwned<(global::B, global::System.Func<global::A>)>(valueTuple_0_0, async () =>
        {
            foreach (var disposeAction in disposeActions_func_0_2)
                await disposeAction();
            await ((global::System.IAsyncDisposable)b_0_1).DisposeAsync();
        });
    }
}");
        }

        [Fact]
        public void DisposesOfFuncDependenciesButNotParameters()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
[Register(typeof(C))]
public partial class Container : IContainer<Func<B, A>>
{
}

public class A 
{
    public A(B b, C c){}
}
public class B : IDisposable
{
    public void Dispose(){}
}
public class C : IDisposable
{
    public void Dispose(){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::B, global::A>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::B, global::A>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Concurrent.ConcurrentBag<global::System.Action> disposeActions_func_0_0;
        global::System.Func<global::B, global::A> func_0_0;
        disposeActions_func_0_0 = new global::System.Collections.Concurrent.ConcurrentBag<global::System.Action>();
        func_0_0 = (param0_0) =>
        {
            global::C c_1_1;
            global::A a_1_0;
            c_1_1 = new global::C();
            try
            {
                a_1_0 = new global::A(b: param0_0, c: c_1_1);
            }
            catch
            {
                ((global::System.IDisposable)c_1_1).Dispose();
                throw;
            }

            disposeActions_func_0_0.Add(() =>
            {
                ((global::System.IDisposable)c_1_1).Dispose();
            });
            return a_1_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
            foreach (var disposeAction in disposeActions_func_0_0)
                disposeAction();
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::B, global::A>> global::StrongInject.IContainer<global::System.Func<global::B, global::A>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Concurrent.ConcurrentBag<global::System.Action> disposeActions_func_0_0;
        global::System.Func<global::B, global::A> func_0_0;
        disposeActions_func_0_0 = new global::System.Collections.Concurrent.ConcurrentBag<global::System.Action>();
        func_0_0 = (param0_0) =>
        {
            global::C c_1_1;
            global::A a_1_0;
            c_1_1 = new global::C();
            try
            {
                a_1_0 = new global::A(b: param0_0, c: c_1_1);
            }
            catch
            {
                ((global::System.IDisposable)c_1_1).Dispose();
                throw;
            }

            disposeActions_func_0_0.Add(() =>
            {
                ((global::System.IDisposable)c_1_1).Dispose();
            });
            return a_1_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::B, global::A>>(func_0_0, () =>
        {
            foreach (var disposeAction in disposeActions_func_0_0)
                disposeAction();
        });
    }
}");
        }

        [Fact]
        public void WarningOnUnusedParameters1()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
public partial class Container : IContainer<Func<int, string, A>>
{
}

public class A 
{
    public A(string s1, string s2){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Warning SI1101: Warning while resolving dependencies for 'System.Func<int, string, A>': Parameter 'int' of delegate 'System.Func<int, string, A>' is not used in resolution of 'A'.
                // Container
                new DiagnosticResult("SI1101", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::System.Int32, global::System.String, global::A>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::System.Int32, global::System.String, global::A>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Int32, global::System.String, global::A> func_0_0;
        func_0_0 = (param0_0, param0_1) =>
        {
            global::A a_1_0;
            a_1_0 = new global::A(s1: param0_1, s2: param0_1);
            return a_1_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::System.Int32, global::System.String, global::A>> global::StrongInject.IContainer<global::System.Func<global::System.Int32, global::System.String, global::A>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Int32, global::System.String, global::A> func_0_0;
        func_0_0 = (param0_0, param0_1) =>
        {
            global::A a_1_0;
            a_1_0 = new global::A(s1: param0_1, s2: param0_1);
            return a_1_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::System.Int32, global::System.String, global::A>>(func_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WarningOnUnusedParameters2()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
public partial class Container : IContainer<Func<int, Func<int, A>>>
{
}

public class A 
{
    public A(int a1, int a2){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Warning SI1101: Warning while resolving dependencies for 'System.Func<int, System.Func<int, A>>': Parameter 'int' of delegate 'System.Func<int, System.Func<int, A>>' is not used in resolution of 'System.Func<int, A>'.
                // Container
                new DiagnosticResult("SI1101", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::System.Int32, global::System.Func<global::System.Int32, global::A>>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::System.Int32, global::System.Func<global::System.Int32, global::A>>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Int32, global::System.Func<global::System.Int32, global::A>> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::System.Func<global::System.Int32, global::A> func_1_0;
            func_1_0 = (param1_0) =>
            {
                global::A a_2_0;
                a_2_0 = new global::A(a1: param1_0, a2: param1_0);
                return a_2_0;
            };
            return func_1_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::System.Int32, global::System.Func<global::System.Int32, global::A>>> global::StrongInject.IContainer<global::System.Func<global::System.Int32, global::System.Func<global::System.Int32, global::A>>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Int32, global::System.Func<global::System.Int32, global::A>> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::System.Func<global::System.Int32, global::A> func_1_0;
            func_1_0 = (param1_0) =>
            {
                global::A a_2_0;
                a_2_0 = new global::A(a1: param1_0, a2: param1_0);
                return a_2_0;
            };
            return func_1_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::System.Int32, global::System.Func<global::System.Int32, global::A>>>(func_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WarningOnSingleInstanceReturnType()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A), Scope.SingleInstance)]
[Register(typeof(B))]
public partial class Container : IContainer<Func<B, A>>
{
}

public class A 
{
    public A(B b){}
}

public class B
{
    public B(){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (7,22): Warning SI1103: Warning while resolving dependencies for 'System.Func<B, A>': Return type 'A' of delegate 'System.Func<B, A>' has a single instance scope and so will always have the same value.
                // Container
                new DiagnosticResult("SI1103", @"Container", DiagnosticSeverity.Warning).WithLocation(7, 22),
                // (7,22): Warning SI1101: Warning while resolving dependencies for 'System.Func<B, A>': Parameter 'B' of delegate 'System.Func<B, A>' is not used in resolution of 'A'.
                // Container
                new DiagnosticResult("SI1101", @"Container", DiagnosticSeverity.Warning).WithLocation(7, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::A _aField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::A GetAField0()
    {
        if (!object.ReferenceEquals(_aField0, null))
            return _aField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::B b_0_1;
            global::A a_0_0;
            b_0_1 = new global::B();
            a_0_0 = new global::A(b: b_0_1);
            this._aField0 = a_0_0;
            this._disposeAction0 = () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _aField0;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::B, global::A>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::B, global::A>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::B, global::A> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::A a_1_0;
            a_1_0 = GetAField0();
            return a_1_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::B, global::A>> global::StrongInject.IContainer<global::System.Func<global::B, global::A>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::B, global::A> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::A a_1_0;
            a_1_0 = GetAField0();
            return a_1_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::B, global::A>>(func_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WarningOnDirectDelegateParameterReturnType()
        {
            string userSource = @"
using System;
using StrongInject;

public partial class Container : IContainer<Func<A, A>>
{
}

public class A 
{
    public A(){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (5,22): Warning SI1104: Warning while resolving dependencies for 'System.Func<A, A>': Return type 'A' of delegate 'System.Func<A, A>' is provided as a parameter to the delegate and so will be returned unchanged.
                // Container
                new DiagnosticResult("SI1104", @"Container", DiagnosticSeverity.Warning).WithLocation(5, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::A, global::A>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::A, global::A>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::A, global::A> func_0_0;
        func_0_0 = (param0_0) =>
        {
            return param0_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::A, global::A>> global::StrongInject.IContainer<global::System.Func<global::A, global::A>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::A, global::A> func_0_0;
        func_0_0 = (param0_0) =>
        {
            return param0_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::A, global::A>>(func_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WarningOnIndirectDelegateParameterReturnType()
        {
            string userSource = @"
using System;
using StrongInject;

public partial class Container : IContainer<Func<A, Func<A>>>
{
}

public class A 
{
    public A(){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (5,22): Warning SI1102: Warning while resolving dependencies for 'System.Func<A, System.Func<A>>': Return type 'A' of delegate 'System.Func<A>' is provided as a parameter to another delegate and so will always have the same value.
                // Container
                new DiagnosticResult("SI1102", @"Container", DiagnosticSeverity.Warning).WithLocation(5, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::A, global::System.Func<global::A>>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::A, global::System.Func<global::A>>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::A, global::System.Func<global::A>> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::System.Func<global::A> func_1_0;
            func_1_0 = () =>
            {
                return param0_0;
            };
            return func_1_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::A, global::System.Func<global::A>>> global::StrongInject.IContainer<global::System.Func<global::A, global::System.Func<global::A>>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::A, global::System.Func<global::A>> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::System.Func<global::A> func_1_0;
            func_1_0 = () =>
            {
                return param0_0;
            };
            return func_1_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::A, global::System.Func<global::A>>>(func_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorOnMultipleParametersWithSameType()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
public partial class Container : IContainer<Func<int, int, A>>
{
}

public class A 
{
    public A(int a){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0104: Error while resolving dependencies for 'System.Func<int, int, A>': delegate 'System.Func<int, int, A>' has multiple parameters of type 'int'.
                // Container
                new DiagnosticResult("SI0104", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22),
                // (6,22): Warning SI1101: Warning while resolving dependencies for 'System.Func<int, int, A>': Parameter 'int' of delegate 'System.Func<int, int, A>' is not used in resolution of 'A'.
                // Container
                new DiagnosticResult("SI1101", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::System.Int32, global::System.Int32, global::A>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::System.Int32, global::System.Int32, global::A>, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Func<global::System.Int32, global::System.Int32, global::A>> global::StrongInject.IContainer<global::System.Func<global::System.Int32, global::System.Int32, global::A>>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorOnRecursiveFuncCall()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
public partial class Container : IContainer<A>
{
}

public class A 
{
    public A(Func<A> a){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0101: Error while resolving dependencies for 'A': 'A' has a circular dependency
                // Container
                new DiagnosticResult("SI0101", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfSyncFuncRequiresAsyncResolution()
        {
            string userSource = @"
using System;
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A))]
public partial class Container : IAsyncContainer<Func<A>>
{
}

public class A : IRequiresAsyncInitialization
{
    public A(){}
    public ValueTask InitializeAsync() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (7,22): Error SI0103: Error while resolving dependencies for 'System.Func<A>': 'A' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(7, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Func<global::A>>.RunAsync<TResult, TParam>(global::System.Func<global::System.Func<global::A>, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Func<global::A>>> global::StrongInject.IAsyncContainer<global::System.Func<global::A>>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorOnParameterPassedAsRef()
        {
            string userSource = @"
using StrongInject;

public delegate A Del(ref int i);
[Register(typeof(A))]
public partial class Container : IContainer<Del>
{
}

public class A 
{
    public A(int a){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0105: Error while resolving dependencies for 'Del': parameter 'ref int' of delegate 'Del' is passed as 'Ref'.
                // Container
                new DiagnosticResult("SI0105", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::Del>.Run<TResult, TParam>(global::System.Func<global::Del, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::Del> global::StrongInject.IContainer<global::Del>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorOnParameterPassedAsIn()
        {
            string userSource = @"
using StrongInject;

public delegate A Del(in int i);
[Register(typeof(A))]
public partial class Container : IContainer<Del>
{
}

public class A 
{
    public A(int a){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0105: Error while resolving dependencies for 'Del': parameter 'in int' of delegate 'Del' is passed as 'In'.
                // Container
                new DiagnosticResult("SI0105", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::Del>.Run<TResult, TParam>(global::System.Func<global::Del, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::Del> global::StrongInject.IContainer<global::Del>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorOnParameterPassedAsOut()
        {
            string userSource = @"
using StrongInject;

public delegate A Del(out int i);
[Register(typeof(A))]
public partial class Container : IContainer<Del>
{
}

public class A 
{
    public A(int a){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0105: Error while resolving dependencies for 'Del': parameter 'out int' of delegate 'Del' is passed as 'Out'.
                // Container
                new DiagnosticResult("SI0105", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::Del>.Run<TResult, TParam>(global::System.Func<global::Del, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::Del> global::StrongInject.IContainer<global::Del>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void DelegateReturningTaskCanResolveDependenciesAsynchronously()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public delegate Task<A> Del(int i);
[Register(typeof(A))]
public partial class Container : IContainer<Del>
{
}

public class A : IRequiresAsyncInitialization
{
    public A(int a){}
    public ValueTask InitializeAsync() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::Del>.Run<TResult, TParam>(global::System.Func<global::Del, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::Del del_0_0;
        del_0_0 = async (param0_0) =>
        {
            global::A a_1_0;
            global::System.Threading.Tasks.ValueTask a_1_1;
            var hasAwaitStarted_a_1_1 = false;
            a_1_0 = new global::A(a: param0_0);
            a_1_1 = ((global::StrongInject.IRequiresAsyncInitialization)a_1_0).InitializeAsync();
            try
            {
                hasAwaitStarted_a_1_1 = true;
                await a_1_1;
            }
            catch
            {
                if (!hasAwaitStarted_a_1_1)
                {
                    _ = a_1_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            return a_1_0;
        };
        TResult result;
        try
        {
            result = func(del_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::Del> global::StrongInject.IContainer<global::Del>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::Del del_0_0;
        del_0_0 = async (param0_0) =>
        {
            global::A a_1_0;
            global::System.Threading.Tasks.ValueTask a_1_1;
            var hasAwaitStarted_a_1_1 = false;
            a_1_0 = new global::A(a: param0_0);
            a_1_1 = ((global::StrongInject.IRequiresAsyncInitialization)a_1_0).InitializeAsync();
            try
            {
                hasAwaitStarted_a_1_1 = true;
                await a_1_1;
            }
            catch
            {
                if (!hasAwaitStarted_a_1_1)
                {
                    _ = a_1_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            return a_1_0;
        };
        return new global::StrongInject.Owned<global::Del>(del_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DelegateReturningValueTaskCanResolveDependenciesAsynchronously()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public delegate ValueTask<A> Del(int i);
[Register(typeof(A))]
public partial class Container : IContainer<Del>
{
}

public class A : IRequiresAsyncInitialization
{
    public A(int a){}
    public ValueTask InitializeAsync() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::Del>.Run<TResult, TParam>(global::System.Func<global::Del, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::Del del_0_0;
        del_0_0 = async (param0_0) =>
        {
            global::A a_1_0;
            global::System.Threading.Tasks.ValueTask a_1_1;
            var hasAwaitStarted_a_1_1 = false;
            a_1_0 = new global::A(a: param0_0);
            a_1_1 = ((global::StrongInject.IRequiresAsyncInitialization)a_1_0).InitializeAsync();
            try
            {
                hasAwaitStarted_a_1_1 = true;
                await a_1_1;
            }
            catch
            {
                if (!hasAwaitStarted_a_1_1)
                {
                    _ = a_1_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            return a_1_0;
        };
        TResult result;
        try
        {
            result = func(del_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::Del> global::StrongInject.IContainer<global::Del>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::Del del_0_0;
        del_0_0 = async (param0_0) =>
        {
            global::A a_1_0;
            global::System.Threading.Tasks.ValueTask a_1_1;
            var hasAwaitStarted_a_1_1 = false;
            a_1_0 = new global::A(a: param0_0);
            a_1_1 = ((global::StrongInject.IRequiresAsyncInitialization)a_1_0).InitializeAsync();
            try
            {
                hasAwaitStarted_a_1_1 = true;
                await a_1_1;
            }
            catch
            {
                if (!hasAwaitStarted_a_1_1)
                {
                    _ = a_1_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            return a_1_0;
        };
        return new global::StrongInject.Owned<global::Del>(del_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void SingleInstanceDependencyCanDependOnDelegate()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public delegate ValueTask<A> Del(int i);
[Register(typeof(A))]
[Register(typeof(B), Scope.SingleInstance)]
public partial class Container : IContainer<B>
{
}

public class A : IRequiresAsyncInitialization
{
    public A(int a){}
    public ValueTask InitializeAsync() => default;
}
public class B
{
    public B(Del d){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::B _bField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::B GetBField0()
    {
        if (!object.ReferenceEquals(_bField0, null))
            return _bField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::Del del_0_1;
            global::B b_0_0;
            del_0_1 = async (param0_0) =>
            {
                global::A a_1_0;
                global::System.Threading.Tasks.ValueTask a_1_1;
                var hasAwaitStarted_a_1_1 = false;
                a_1_0 = new global::A(a: param0_0);
                a_1_1 = ((global::StrongInject.IRequiresAsyncInitialization)a_1_0).InitializeAsync();
                try
                {
                    hasAwaitStarted_a_1_1 = true;
                    await a_1_1;
                }
                catch
                {
                    if (!hasAwaitStarted_a_1_1)
                    {
                        _ = a_1_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                    }

                    throw;
                }

                return a_1_0;
            };
            b_0_0 = new global::B(d: del_0_1);
            this._bField0 = b_0_0;
            this._disposeAction0 = () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _bField0;
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_0;
        b_0_0 = GetBField0();
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_0;
        b_0_0 = GetBField0();
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void PreferInstanceUsedAsFactoryToProvideDelegate()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
public partial class Container : IAsyncContainer<Func<A>>
{
    [Instance(Options.UseAsFactory)] private IFactory<Func<A>> _instanceProvider;
}

public class A
{
    public A(){}
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (8,64): Warning CS0649: Field 'Container._instanceProvider' is never assigned to, and will always have its default value null
                // _instanceProvider
                new DiagnosticResult("CS0649", @"_instanceProvider", DiagnosticSeverity.Warning).WithLocation(8, 64));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Func<global::A>>.RunAsync<TResult, TParam>(global::System.Func<global::System.Func<global::A>, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::System.Func<global::A>> iFactory_0_1;
        global::System.Func<global::A> func_0_0;
        iFactory_0_1 = this._instanceProvider;
        func_0_0 = iFactory_0_1.Create();
        TResult result;
        try
        {
            result = await func(func_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(func_0_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Func<global::A>>> global::StrongInject.IAsyncContainer<global::System.Func<global::A>>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::System.Func<global::A>> iFactory_0_1;
        global::System.Func<global::A> func_0_0;
        iFactory_0_1 = this._instanceProvider;
        func_0_0 = iFactory_0_1.Create();
        return new global::StrongInject.AsyncOwned<global::System.Func<global::A>>(func_0_0, async () =>
        {
            iFactory_0_1.Release(func_0_0);
        });
    }
}");
        }

        [Fact]
        public void PreferFactoryToProvideDelegate()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
[RegisterFactory(typeof(B))]
public partial class Container : IAsyncContainer<Func<A>>
{
}

public class A
{
}
public class B : IFactory<Func<A>>
{
    public Func<A> Create() => null;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Func<global::A>>.RunAsync<TResult, TParam>(global::System.Func<global::System.Func<global::A>, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_2;
        global::StrongInject.IFactory<global::System.Func<global::A>> iFactory_0_1;
        global::System.Func<global::A> func_0_0;
        b_0_2 = new global::B();
        iFactory_0_1 = (global::StrongInject.IFactory<global::System.Func<global::A>>)b_0_2;
        func_0_0 = iFactory_0_1.Create();
        TResult result;
        try
        {
            result = await func(func_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(func_0_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Func<global::A>>> global::StrongInject.IAsyncContainer<global::System.Func<global::A>>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_2;
        global::StrongInject.IFactory<global::System.Func<global::A>> iFactory_0_1;
        global::System.Func<global::A> func_0_0;
        b_0_2 = new global::B();
        iFactory_0_1 = (global::StrongInject.IFactory<global::System.Func<global::A>>)b_0_2;
        func_0_0 = iFactory_0_1.Create();
        return new global::StrongInject.AsyncOwned<global::System.Func<global::A>>(func_0_0, async () =>
        {
            iFactory_0_1.Release(func_0_0);
        });
    }
}");
        }

        [Fact]
        public void PreferDelegateParameterToProvideDelegate()
        {
            string userSource = @"
using System;
using StrongInject;

[Register(typeof(A))]
public partial class Container : IAsyncContainer<Func<Func<A>, Func<A>>>
{
}

public class A
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Warning SI1104: Warning while resolving dependencies for 'System.Func<System.Func<A>, System.Func<A>>': Return type 'System.Func<A>' of delegate 'System.Func<System.Func<A>, System.Func<A>>' is provided as a parameter to the delegate and so will be returned unchanged.
                // Container
                new DiagnosticResult("SI1104", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Func<global::System.Func<global::A>, global::System.Func<global::A>>>.RunAsync<TResult, TParam>(global::System.Func<global::System.Func<global::System.Func<global::A>, global::System.Func<global::A>>, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Func<global::A>, global::System.Func<global::A>> func_0_0;
        func_0_0 = (param0_0) =>
        {
            return param0_0;
        };
        TResult result;
        try
        {
            result = await func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Func<global::System.Func<global::A>, global::System.Func<global::A>>>> global::StrongInject.IAsyncContainer<global::System.Func<global::System.Func<global::A>, global::System.Func<global::A>>>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Func<global::A>, global::System.Func<global::A>> func_0_0;
        func_0_0 = (param0_0) =>
        {
            return param0_0;
        };
        return new global::StrongInject.AsyncOwned<global::System.Func<global::System.Func<global::A>, global::System.Func<global::A>>>(func_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CannotResolveVoidReturningDelegate()
        {
            string userSource = @"
using StrongInject;
using System;

public partial class Container : IContainer<Action<int>>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (5,22): Error SI0102: Error while resolving dependencies for 'Action<int>': We have no source for instance of type 'Action<int>'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Action<global::System.Int32>>.Run<TResult, TParam>(global::System.Func<global::System.Action<global::System.Int32>, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Action<global::System.Int32>> global::StrongInject.IContainer<global::System.Action<global::System.Int32>>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void CanImportFactoryMethodFromModule()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Factory]
    public static A M(B b) => null;
}

[Register(typeof(B))]
[RegisterModule(typeof(Module))]
public partial class Container : IAsyncContainer<A>
{
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        a_0_0 = global::Module.M(b: b_0_1);
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        a_0_0 = global::Module.M(b: b_0_1);
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        });
    }
}");
        }

        [Fact]
        public void FactoryMethodCanBeSingleInstance()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Factory(Scope.SingleInstance)]
    public static A M(B b) => null;
}

[Register(typeof(B))]
[RegisterModule(typeof(Module))]
public partial class Container : IAsyncContainer<A>
{
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::A _aField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::A GetAField0()
    {
        if (!object.ReferenceEquals(_aField0, null))
            return _aField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::B b_0_1;
            global::A a_0_0;
            b_0_1 = new global::B();
            a_0_0 = global::Module.M(b: b_0_1);
            this._aField0 = a_0_0;
            this._disposeAction0 = async () =>
            {
                await global::StrongInject.Helpers.DisposeAsync(a_0_0);
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _aField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = GetAField0();
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = GetAField0();
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void PublicFactoryInInternalModuleUsed()
        {
            string userSource = @"
using StrongInject;

internal class Module
{
    [Factory]
    public static A M(B b) => null;
}

[Register(typeof(B))]
[RegisterModule(typeof(Module))]
public partial class Container : IAsyncContainer<A>
{
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (11,2): Warning SI1006: 'Module' is not public, but is imported by public module 'Container'. If 'Container' is imported outside this assembly this may result in errors. Try making 'Container' internal.
                // RegisterModule(typeof(Module))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(Module))", DiagnosticSeverity.Warning).WithLocation(11, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        a_0_0 = global::Module.M(b: b_0_1);
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        a_0_0 = global::Module.M(b: b_0_1);
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        });
    }
}");
        }

        [Fact]
        public void NonPublicFactoryMethodIgnored()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Factory]
    internal static A M(B b) => null;
}

[Register(typeof(B))]
[RegisterModule(typeof(Module))]
public partial class Container : IAsyncContainer<A>
{
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Warning SI1002: Factory method 'Module.M(B)' is not either public and static, or protected, and containing module 'Module' is not a container, so will be ignored.
                // Factory
                new DiagnosticResult("SI1002", @"Factory", DiagnosticSeverity.Warning).WithLocation(6, 6),
                // (12,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(12, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void NonStaticFactoryMethodIgnored()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Factory]
    public A M(B b) => null;
}

[Register(typeof(B))]
[RegisterModule(typeof(Module))]
public partial class Container : IAsyncContainer<A>
{
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Warning SI1002: Factory method 'Module.M(B)' is not static, and containing module 'Module' is not a container, so will be ignored.
                // Factory
                new DiagnosticResult("SI1002", @"Factory", DiagnosticSeverity.Warning).WithLocation(6, 6),
                // (12,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(12, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void CanUseNonPublicStaticFactoryMethodDefinedInContainer()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(B))]
public partial class Container : IAsyncContainer<A>
{
    [Factory]
    A M(B b) => null;
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        a_0_0 = this.M(b: b_0_1);
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        a_0_0 = this.M(b: b_0_1);
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        });
    }
}");
        }

        [Fact]
        public void ErrorIfPrivateInstanceFactoryMethodDefinedInContainerDuplicatesExistingRegistration()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(B))]
[Register(typeof(A))]
public partial class Container : IAsyncContainer<A>
{
    [Factory]
    A M(B b) => null;
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0106: Error while resolving dependencies for 'A': We have multiple sources for instance of type 'A' and no best source. Try adding a single registration for 'A' directly to the container, and moving any existing registrations for 'A' on the container to an imported module.
                // Container
                new DiagnosticResult("SI0106", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfPublicStaticFactoryMethodDefinedInContainerOverridesExistingRegistration()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(B))]
[Register(typeof(A))]
public partial class Container : IAsyncContainer<A>
{
    [Factory]
    public static A M(B b) => null;
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0106: Error while resolving dependencies for 'A': We have multiple sources for instance of type 'A' and no best source. Try adding a single registration for 'A' directly to the container, and moving any existing registrations for 'A' on the container to an imported module.
                // Container
                new DiagnosticResult("SI0106", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfMultipleFactoryMethodsDefinedByContainerForSameType()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(B))]
[Register(typeof(A))]
public partial class Container : IAsyncContainer<A>
{
    [Factory]
    A M(B b) => null;
    [Factory]
    A M1() => null;
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0106: Error while resolving dependencies for 'A': We have multiple sources for instance of type 'A' and no best source. Try adding a single registration for 'A' directly to the container, and moving any existing registrations for 'A' on the container to an imported module.
                // Container
                new DiagnosticResult("SI0106", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfInstanceUsedAsFactoryAndFactoryMethodDefinedByContainerForSameType()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(B))]
[Register(typeof(A))]
public partial class Container : IAsyncContainer<A>
{
    [Factory]
    A M(B b) => null;
    [Instance(Options.UseAsFactory)]
    public IFactory<A> _instanceProvider;
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0106: Error while resolving dependencies for 'A': We have multiple sources for instance of type 'A' and no best source. Try adding a single registration for 'A' directly to the container, and moving any existing registrations for 'A' on the container to an imported module.
                // Container
                new DiagnosticResult("SI0106", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfFactoryMethodReturnsVoid()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(B))]
[Register(typeof(A))]
public partial class Container : IAsyncContainer<A>
{
    [Factory]
    void M(B b){}
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (8,6): Error SI0014: Factory method 'Container.M(B)' returns void.
                // Factory
                new DiagnosticResult("SI0014", @"Factory", DiagnosticSeverity.Error).WithLocation(8, 6));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorIfPublicStaticFactoryMethodInContainerReturnsVoid()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(B))]
[Register(typeof(A))]
public partial class Container : IAsyncContainer<A>
{
    [Factory]
    public static void M(B b){}
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (8,6): Error SI0014: Factory method 'Container.M(B)' returns void.
                // Factory
                new DiagnosticResult("SI0014", @"Factory", DiagnosticSeverity.Error).WithLocation(8, 6));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorIfFactoryMethodFromModuleOverridesExisingRegistration()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
public class Module
{
    [Factory]
    public static A M(B b) => null;
}

public class A{}
public class B{}

[Register(typeof(B))]
[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (16,22): Error SI0106: Error while resolving dependencies for 'A': We have multiple sources for instance of type 'A' and no best source. Try adding a single registration for 'A' directly to the container, and moving any existing registrations for 'A' on the container to an imported module.
                // Container
                new DiagnosticResult("SI0106", @"Container", DiagnosticSeverity.Error).WithLocation(16, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfFactoryMethodTakesParameterByRef()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Factory]
    public static A M(ref B b) => null;
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (7,23): Error SI0018: parameter 'ref B' of factory method 'Module.M(ref B)' is passed as 'Ref'.
                // ref B b
                new DiagnosticResult("SI0018", @"ref B b", DiagnosticSeverity.Error).WithLocation(7, 23));
            comp.GetDiagnostics().Verify();
            Assert.Empty(generated);
        }

        [Fact]
        public void CanResolveAsyncFactoryMethod()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public class Module
{
    [Factory]
    public static Task<A> M(B b) => null;
}

[Register(typeof(B))]
[RegisterModule(typeof(Module))]
public partial class Container : IAsyncContainer<A>
{
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::System.Threading.Tasks.Task<global::A> a_0_2;
        var hasAwaitStarted_a_0_2 = false;
        var a_0_0 = default(global::A);
        var hasAwaitCompleted_a_0_2 = false;
        b_0_1 = new global::B();
        a_0_2 = global::Module.M(b: b_0_1);
        try
        {
            hasAwaitStarted_a_0_2 = true;
            a_0_0 = await a_0_2;
            hasAwaitCompleted_a_0_2 = true;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_2)
            {
                a_0_0 = await a_0_2;
            }
            else if (!hasAwaitCompleted_a_0_2)
            {
                throw;
            }

            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
            throw;
        }

        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::System.Threading.Tasks.Task<global::A> a_0_2;
        var hasAwaitStarted_a_0_2 = false;
        var a_0_0 = default(global::A);
        var hasAwaitCompleted_a_0_2 = false;
        b_0_1 = new global::B();
        a_0_2 = global::Module.M(b: b_0_1);
        try
        {
            hasAwaitStarted_a_0_2 = true;
            a_0_0 = await a_0_2;
            hasAwaitCompleted_a_0_2 = true;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_2)
            {
                a_0_0 = await a_0_2;
            }
            else if (!hasAwaitCompleted_a_0_2)
            {
                throw;
            }

            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        });
    }
}");
        }

        [Fact]
        public void ErrorIfAsyncFactoryMethodUsedInSyncContainer()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public class Module
{
    [Factory]
    public static Task<A> M(B b) => null;
}

[Register(typeof(B))]
[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (13,22): Error SI0103: Error while resolving dependencies for 'A': 'A' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(13, 22));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfNotAllGenericFactoryMethodTypeParametersUsedInReturnType()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Factory]
    public static A M<T>(B b) => null;
}

[Register(typeof(B))]
[RegisterModule(typeof(Module))]
public partial class Container : IAsyncContainer<A>
{
}

public class A{}
public class B{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Error SI0020: All type parameters must be used in return type of generic factory method 'Module.M<T>(B)'
                // Factory
                new DiagnosticResult("SI0020", @"Factory", DiagnosticSeverity.Error).WithLocation(6, 6),
                // (12,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(12, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfFactoryMethodIsRecursive()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IAsyncContainer<A>
{
    [Factory]
    A M(A a) => null;
}

public class A{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,22): Error SI0101: Error while resolving dependencies for 'A': 'A' has a circular dependency
                // Container
                new DiagnosticResult("SI0101", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfFactoryMethodRequiresAsyncResolutionInSyncContainer()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(B))]
public partial class Container : IContainer<A>
{
    [Factory]
    A M(B b) => null;
}

public class A{}
public class B : IRequiresAsyncInitialization
{
    public ValueTask InitializeAsync() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Error SI0103: Error while resolving dependencies for 'A': 'B' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void FactoryMethodRequiringAsyncResolutionCanBeSingleInstance()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(B))]
public partial class Container : IAsyncContainer<A>
{
    [Factory(Scope.SingleInstance)]
    A M(B b) => null;
}

public class A{}
public class B : IRequiresAsyncInitialization
{
    public ValueTask InitializeAsync() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::A _aField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private async global::System.Threading.Tasks.ValueTask<global::A> GetAField0()
    {
        if (!object.ReferenceEquals(_aField0, null))
            return _aField0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::B b_0_1;
            global::System.Threading.Tasks.ValueTask b_0_2;
            var hasAwaitStarted_b_0_2 = false;
            global::A a_0_0;
            b_0_1 = new global::B();
            b_0_2 = ((global::StrongInject.IRequiresAsyncInitialization)b_0_1).InitializeAsync();
            try
            {
                hasAwaitStarted_b_0_2 = true;
                await b_0_2;
                a_0_0 = this.M(b: b_0_1);
            }
            catch
            {
                if (!hasAwaitStarted_b_0_2)
                {
                    _ = b_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._aField0 = a_0_0;
            this._disposeAction0 = async () =>
            {
                await global::StrongInject.Helpers.DisposeAsync(a_0_0);
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _aField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::A> a_0_0;
        var hasAwaitStarted_a_0_0 = false;
        var a_0_1 = default(global::A);
        a_0_0 = GetAField0();
        try
        {
            hasAwaitStarted_a_0_0 = true;
            a_0_1 = await a_0_0;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_0)
            {
                _ = a_0_0.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(a_0_1, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::A> a_0_0;
        var hasAwaitStarted_a_0_0 = false;
        var a_0_1 = default(global::A);
        a_0_0 = GetAField0();
        try
        {
            hasAwaitStarted_a_0_0 = true;
            a_0_1 = await a_0_0;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_0)
            {
                _ = a_0_0.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::A>(a_0_1, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void TestAsyncFactory()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<A>
{
    [Factory] ValueTask<A> M() => default;
}

public class A{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::A> a_0_1;
        var hasAwaitStarted_a_0_1 = false;
        var a_0_0 = default(global::A);
        var hasAwaitCompleted_a_0_1 = false;
        a_0_1 = this.M();
        try
        {
            hasAwaitStarted_a_0_1 = true;
            a_0_0 = await a_0_1;
            hasAwaitCompleted_a_0_1 = true;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_1)
            {
                a_0_0 = await a_0_1;
            }
            else if (!hasAwaitCompleted_a_0_1)
            {
                throw;
            }

            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
            throw;
        }

        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::A> a_0_1;
        var hasAwaitStarted_a_0_1 = false;
        var a_0_0 = default(global::A);
        var hasAwaitCompleted_a_0_1 = false;
        a_0_1 = this.M();
        try
        {
            hasAwaitStarted_a_0_1 = true;
            a_0_0 = await a_0_1;
            hasAwaitCompleted_a_0_1 = true;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_1)
            {
                a_0_0 = await a_0_1;
            }
            else if (!hasAwaitCompleted_a_0_1)
            {
                throw;
            }

            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        });
    }
}");
        }

        [Fact]
        public void TestAsyncGenericFactory()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<A>
{
    [Factory] ValueTask<T> M<T>() => default;
}

public class A{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::A> a_0_1;
        var hasAwaitStarted_a_0_1 = false;
        var a_0_0 = default(global::A);
        var hasAwaitCompleted_a_0_1 = false;
        a_0_1 = this.M<global::A>();
        try
        {
            hasAwaitStarted_a_0_1 = true;
            a_0_0 = await a_0_1;
            hasAwaitCompleted_a_0_1 = true;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_1)
            {
                a_0_0 = await a_0_1;
            }
            else if (!hasAwaitCompleted_a_0_1)
            {
                throw;
            }

            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
            throw;
        }

        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::A> a_0_1;
        var hasAwaitStarted_a_0_1 = false;
        var a_0_0 = default(global::A);
        var hasAwaitCompleted_a_0_1 = false;
        a_0_1 = this.M<global::A>();
        try
        {
            hasAwaitStarted_a_0_1 = true;
            a_0_0 = await a_0_1;
            hasAwaitCompleted_a_0_1 = true;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_1)
            {
                a_0_0 = await a_0_1;
            }
            else if (!hasAwaitCompleted_a_0_1)
            {
                throw;
            }

            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_0);
        });
    }
}");
        }

        [Fact]
        public void WarnOnInstanceRequiringAsyncDisposalInSyncResolution()
        {
            string userSource = @"
using StrongInject;
using System;
using System.Threading.Tasks;

[Register(typeof(A))]
public partial class Container : IContainer<A>
{
}

public class A : IAsyncDisposable
{
    public ValueTask DisposeAsync() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (7,22): Warning SI1301: Cannot call asynchronous dispose for 'A' in implementation of synchronous container
                // Container
                new DiagnosticResult("SI1301", @"Container", DiagnosticSeverity.Warning).WithLocation(7, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void NoErrorIfMultipleDependenciesRegisteredForATypeButNoneUsed()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A), typeof(A), typeof(IInterface))]
[Register(typeof(B), typeof(B), typeof(IInterface))]
public partial class Container : IContainer<A>, IContainer<B>
{
}

public interface IInterface {}
public class A : IInterface {}
public class B : IInterface {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_0;
        b_0_0 = new global::B();
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_0;
        b_0_0 = new global::B();
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanImportInstanceFieldFromModule()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Instance]
    public static readonly A Instance;
}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = global::Module.Instance;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = global::Module.Instance;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanImportInstancePropertyFromModule()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Instance]
    public static A Instance { get; }
}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = global::Module.Instance;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = global::Module.Instance;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WarningIfInstanceFieldIsNotStatic()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Instance]
    public readonly A Instance;
}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Warning SI1004: Instance field 'Module.Instance' is not static, and containing module 'Module' is not a container, so will be ignored.
                // Instance
                new DiagnosticResult("SI1004", @"Instance", DiagnosticSeverity.Warning).WithLocation(6, 6),
                // (11,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(11, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void WarningIfInstancePropertyIsNotStatic()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Instance]
    public A Instance { get; }
}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Warning SI1004: Instance property 'Module.Instance' is not static, and containing module 'Module' is not a container, so will be ignored.
                // Instance
                new DiagnosticResult("SI1004", @"Instance", DiagnosticSeverity.Warning).WithLocation(6, 6),
                // (11,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(11, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void WarningIfInstanceFieldIsNotPublic()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Instance]
    internal static readonly A Instance = null;
}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Warning SI1004: Instance field 'Module.Instance' is not either public and static, or protected, and containing module 'Module' is not a container, so will be ignored.
                // Instance
                new DiagnosticResult("SI1004", @"Instance", DiagnosticSeverity.Warning).WithLocation(6, 6),
                // (11,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(11, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void PublicInstancePropertyOnInternalModuleIsUsed()
        {
            string userSource = @"
using StrongInject;

internal class Module
{
    [Instance]
    public static A Instance { get; }
}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (10,2): Warning SI1006: 'Module' is not public, but is imported by public module 'Container'. If 'Container' is imported outside this assembly this may result in errors. Try making 'Container' internal.
                // RegisterModule(typeof(Module))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(Module))", DiagnosticSeverity.Warning).WithLocation(10, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = global::Module.Instance;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = global::Module.Instance;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WarningIfInstancePropertyIsNotPublic()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Instance]
    internal static A Instance { get; }
}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Warning SI1004: Instance property 'Module.Instance' is not either public and static, or protected, and containing module 'Module' is not a container, so will be ignored.
                // Instance
                new DiagnosticResult("SI1004", @"Instance", DiagnosticSeverity.Warning).WithLocation(6, 6),
                // (11,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(11, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfInstancePropertyIsWriteOnly()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Instance]
    public static A Instance { set {} }
}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Error SI0021: Instance property 'Module.Instance' is write only.
                // Instance
                new DiagnosticResult("SI0021", @"Instance", DiagnosticSeverity.Error).WithLocation(6, 6),
                // (11,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(11, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void WarningIfInstancePropertyGetMethodIsNotPublic()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Instance]
    public static A Instance { internal get; set; }
}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Warning SI1004: Instance property 'Module.Instance' is not either public and static, or protected, and containing module 'Module' is not a container, so will be ignored.
                // Instance
                new DiagnosticResult("SI1004", @"Instance", DiagnosticSeverity.Warning).WithLocation(6, 6),
                // (11,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(11, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void CanUsePrivateInstanceFieldOnContainer()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<A>
{
    [Instance] private A AInstance = null;
}

public class A {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this.AInstance;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this.AInstance;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DoesNotCallDisposeOnInstanceField()
        {
            string userSource = @"
using StrongInject;
using System;

public partial class Container : IContainer<IDisposable>
{
    [Instance] private IDisposable DisposableInstance = null;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.IDisposable>.Run<TResult, TParam>(global::System.Func<global::System.IDisposable, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.IDisposable iDisposable_0_0;
        iDisposable_0_0 = this.DisposableInstance;
        TResult result;
        try
        {
            result = func(iDisposable_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.IDisposable> global::StrongInject.IContainer<global::System.IDisposable>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.IDisposable iDisposable_0_0;
        iDisposable_0_0 = this.DisposableInstance;
        return new global::StrongInject.Owned<global::System.IDisposable>(iDisposable_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ArrayResolvesAllRegistrationsForType()
        {
            string userSource = @"
using StrongInject;

public class A : IA {}
public class B : IA {}
public class C : IA {}
public class IAFactory : IFactory<IA>
{
    public IA Create() => null;
}

[Register(typeof(B), Scope.SingleInstance, typeof(IA))]
[Register(typeof(C))]
[RegisterFactory(typeof(IAFactory))]
public class Module
{
    [Factory] public static IA FactoryOfA() => null; 
}

public interface IA {}

[Register(typeof(A), typeof(IA))]
[RegisterModule(typeof(Module))]
public partial class Container : IContainer<IA[]>
{
    [Instance] private IA AInstance = null;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::B _bField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::B GetBField0()
    {
        if (!object.ReferenceEquals(_bField0, null))
            return _bField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::B b_0_0;
            b_0_0 = new global::B();
            this._bField0 = b_0_0;
            this._disposeAction0 = () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _bField0;
    }

    TResult global::StrongInject.IContainer<global::IA[]>.Run<TResult, TParam>(global::System.Func<global::IA[], TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::IAFactory iAFactory_0_3;
        global::StrongInject.IFactory<global::IA> iFactory_0_2;
        global::IA iA_0_1;
        global::B b_0_5;
        global::IA iA_0_4;
        global::IA iA_0_6;
        global::IA iA_0_7;
        global::A a_0_9;
        global::IA iA_0_8;
        global::IA[] _0_0;
        iAFactory_0_3 = new global::IAFactory();
        iFactory_0_2 = (global::StrongInject.IFactory<global::IA>)iAFactory_0_3;
        iA_0_1 = iFactory_0_2.Create();
        try
        {
            b_0_5 = GetBField0();
            iA_0_4 = (global::IA)b_0_5;
            iA_0_6 = global::Module.FactoryOfA();
            try
            {
                iA_0_7 = this.AInstance;
                a_0_9 = new global::A();
                iA_0_8 = (global::IA)a_0_9;
                _0_0 = new global::IA[]{(global::IA)iA_0_1, (global::IA)iA_0_4, (global::IA)iA_0_6, (global::IA)iA_0_7, (global::IA)iA_0_8, };
            }
            catch
            {
                global::StrongInject.Helpers.Dispose(iA_0_6);
                throw;
            }
        }
        catch
        {
            iFactory_0_2.Release(iA_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(iA_0_6);
            iFactory_0_2.Release(iA_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::IA[]> global::StrongInject.IContainer<global::IA[]>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::IAFactory iAFactory_0_3;
        global::StrongInject.IFactory<global::IA> iFactory_0_2;
        global::IA iA_0_1;
        global::B b_0_5;
        global::IA iA_0_4;
        global::IA iA_0_6;
        global::IA iA_0_7;
        global::A a_0_9;
        global::IA iA_0_8;
        global::IA[] _0_0;
        iAFactory_0_3 = new global::IAFactory();
        iFactory_0_2 = (global::StrongInject.IFactory<global::IA>)iAFactory_0_3;
        iA_0_1 = iFactory_0_2.Create();
        try
        {
            b_0_5 = GetBField0();
            iA_0_4 = (global::IA)b_0_5;
            iA_0_6 = global::Module.FactoryOfA();
            try
            {
                iA_0_7 = this.AInstance;
                a_0_9 = new global::A();
                iA_0_8 = (global::IA)a_0_9;
                _0_0 = new global::IA[]{(global::IA)iA_0_1, (global::IA)iA_0_4, (global::IA)iA_0_6, (global::IA)iA_0_7, (global::IA)iA_0_8, };
            }
            catch
            {
                global::StrongInject.Helpers.Dispose(iA_0_6);
                throw;
            }
        }
        catch
        {
            iFactory_0_2.Release(iA_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::IA[]>(_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(iA_0_6);
            iFactory_0_2.Release(iA_0_1);
        });
    }
}");
        }

        [Fact]
        public void ArrayIgnoresDuplicateRegistrationForType1()
        {
            string userSource = @"
using StrongInject;

public class A : IA {}
public class B : IA {}
public interface IA {}

[Register(typeof(B), typeof(IA))]
[Register(typeof(A), typeof(IA))]
public class Module
{
}

[Register(typeof(A), typeof(IA))]
[RegisterModule(typeof(Module))]
public partial class Container : IContainer<IA[]>
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::IA[]>.Run<TResult, TParam>(global::System.Func<global::IA[], TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::IA iA_0_1;
        global::B b_0_4;
        global::IA iA_0_3;
        global::IA[] _0_0;
        a_0_2 = new global::A();
        iA_0_1 = (global::IA)a_0_2;
        b_0_4 = new global::B();
        iA_0_3 = (global::IA)b_0_4;
        _0_0 = new global::IA[]{(global::IA)iA_0_1, (global::IA)iA_0_3, };
        TResult result;
        try
        {
            result = func(_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::IA[]> global::StrongInject.IContainer<global::IA[]>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::IA iA_0_1;
        global::B b_0_4;
        global::IA iA_0_3;
        global::IA[] _0_0;
        a_0_2 = new global::A();
        iA_0_1 = (global::IA)a_0_2;
        b_0_4 = new global::B();
        iA_0_3 = (global::IA)b_0_4;
        _0_0 = new global::IA[]{(global::IA)iA_0_1, (global::IA)iA_0_3, };
        return new global::StrongInject.Owned<global::IA[]>(_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ArrayIgnoresDuplicateRegistrationForType2()
        {
            string userSource = @"
using StrongInject;

public class A : IA {}
public class B : IA {}
public interface IA {}

[Register(typeof(B), typeof(IA))]
[Register(typeof(A), typeof(IA))]
public class Module1
{
}

[Register(typeof(A), typeof(IA))]
public class Module2
{
}

[RegisterModule(typeof(Module1))]
[RegisterModule(typeof(Module2))]
public partial class Container : IContainer<IA[]>
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::IA[]>.Run<TResult, TParam>(global::System.Func<global::IA[], TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_2;
        global::IA iA_0_1;
        global::A a_0_4;
        global::IA iA_0_3;
        global::IA[] _0_0;
        b_0_2 = new global::B();
        iA_0_1 = (global::IA)b_0_2;
        a_0_4 = new global::A();
        iA_0_3 = (global::IA)a_0_4;
        _0_0 = new global::IA[]{(global::IA)iA_0_1, (global::IA)iA_0_3, };
        TResult result;
        try
        {
            result = func(_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::IA[]> global::StrongInject.IContainer<global::IA[]>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_2;
        global::IA iA_0_1;
        global::A a_0_4;
        global::IA iA_0_3;
        global::IA[] _0_0;
        b_0_2 = new global::B();
        iA_0_1 = (global::IA)b_0_2;
        a_0_4 = new global::A();
        iA_0_3 = (global::IA)a_0_4;
        _0_0 = new global::IA[]{(global::IA)iA_0_1, (global::IA)iA_0_3, };
        return new global::StrongInject.Owned<global::IA[]>(_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ArrayIgnoresExludedRegistrations()
        {
            string userSource = @"
using StrongInject;

public class A : IA {}
public class B : IA {}
public interface IA {}

[Register(typeof(A), typeof(IA))]
public class Module
{
}

[RegisterModule(typeof(Module), typeof(IA))]
public partial class Container : IContainer<IA[]>
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (14,22): Warning SI1105: Warning while resolving dependencies for 'IA[]': Resolving all registration of type 'IA', but there are no such registrations.
                // Container
                new DiagnosticResult("SI1105", @"Container", DiagnosticSeverity.Warning).WithLocation(14, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::IA[]>.Run<TResult, TParam>(global::System.Func<global::IA[], TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::IA[] _0_0;
        _0_0 = new global::IA[]{};
        TResult result;
        try
        {
            result = func(_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::IA[]> global::StrongInject.IContainer<global::IA[]>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::IA[] _0_0;
        _0_0 = new global::IA[]{};
        return new global::StrongInject.Owned<global::IA[]>(_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorIfArrayDependenciesAreRecursive()
        {
            string userSource = @"
using StrongInject;

public class A : IA { public A(IA[] ia){} }
public class B : IA {}
public interface IA {}

[Register(typeof(A), typeof(IA))]
public class Module
{
}

[Register(typeof(B), typeof(IA))]
[RegisterModule(typeof(Module))]
public partial class Container : IContainer<IA[]>
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (15,22): Error SI0101: Error while resolving dependencies for 'IA[]': 'IA[]' has a circular dependency
                // Container
                new DiagnosticResult("SI0101", @"Container", DiagnosticSeverity.Error).WithLocation(15, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::IA[]>.Run<TResult, TParam>(global::System.Func<global::IA[], TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::IA[]> global::StrongInject.IContainer<global::IA[]>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ErrorIfArrayDependenciesRequireAsyncResolutionInSyncContainer()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public class A : IA, IRequiresAsyncInitialization { public ValueTask InitializeAsync() => default; }
public class B : IA {}
public interface IA {}

[Register(typeof(A), typeof(IA))]
public class Module
{
}

[Register(typeof(B), typeof(IA))]
[RegisterModule(typeof(Module))]
public partial class Container : IContainer<IA[]>
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (16,22): Error SI0103: Error while resolving dependencies for 'IA[]': 'IA' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(16, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::IA[]>.Run<TResult, TParam>(global::System.Func<global::IA[], TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::IA[]> global::StrongInject.IContainer<global::IA[]>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ArrayDependenciesDontIncludeDelegateParameters()
        {
            string userSource = @"
using StrongInject;
using System;

public class A : IA {}
public class B : IA {}
public interface IA {}

[Register(typeof(A), typeof(IA))]
public class Module
{
}

[Register(typeof(B), typeof(IA))]
[RegisterModule(typeof(Module))]
public partial class Container : IContainer<Func<IA, IA[]>>
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (16,22): Warning SI1101: Warning while resolving dependencies for 'System.Func<IA, IA[]>': Parameter 'IA' of delegate 'IA[]' is not used in resolution of 'IA[]'.
                // Container
                new DiagnosticResult("SI1101", @"Container", DiagnosticSeverity.Warning).WithLocation(16, 22));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Func<global::IA, global::IA[]>>.Run<TResult, TParam>(global::System.Func<global::System.Func<global::IA, global::IA[]>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::IA, global::IA[]> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::B b_1_2;
            global::IA iA_1_1;
            global::A a_1_4;
            global::IA iA_1_3;
            global::IA[] _1_0;
            b_1_2 = new global::B();
            iA_1_1 = (global::IA)b_1_2;
            a_1_4 = new global::A();
            iA_1_3 = (global::IA)a_1_4;
            _1_0 = new global::IA[]{(global::IA)iA_1_1, (global::IA)iA_1_3, };
            return _1_0;
        };
        TResult result;
        try
        {
            result = func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Func<global::IA, global::IA[]>> global::StrongInject.IContainer<global::System.Func<global::IA, global::IA[]>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::IA, global::IA[]> func_0_0;
        func_0_0 = (param0_0) =>
        {
            global::B b_1_2;
            global::IA iA_1_1;
            global::A a_1_4;
            global::IA iA_1_3;
            global::IA[] _1_0;
            b_1_2 = new global::B();
            iA_1_1 = (global::IA)b_1_2;
            a_1_4 = new global::A();
            iA_1_3 = (global::IA)a_1_4;
            _1_0 = new global::IA[]{(global::IA)iA_1_1, (global::IA)iA_1_3, };
            return _1_0;
        };
        return new global::StrongInject.Owned<global::System.Func<global::IA, global::IA[]>>(func_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanResolveSimpleTypeFromGenericFactoryMethod()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<string>
{
    [Factory] T Resolve<T>() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.String>.Run<TResult, TParam>(global::System.Func<global::System.String, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.String string_0_0;
        string_0_0 = this.Resolve<global::System.String>();
        TResult result;
        try
        {
            result = func(string_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.String> global::StrongInject.IContainer<global::System.String>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.String string_0_0;
        string_0_0 = this.Resolve<global::System.String>();
        return new global::StrongInject.Owned<global::System.String>(string_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanResolveNamedTypeFromGenericFactoryMethod1()
        {
            string userSource = @"
using StrongInject;
using System.Collections.Generic;

public partial class Container : IContainer<List<string>>
{
    [Factory] List<T> Resolve<T>() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.List<global::System.String>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.List<global::System.String>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.List<global::System.String> list_0_0;
        list_0_0 = this.Resolve<global::System.String>();
        TResult result;
        try
        {
            result = func(list_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(list_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Collections.Generic.List<global::System.String>> global::StrongInject.IContainer<global::System.Collections.Generic.List<global::System.String>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.List<global::System.String> list_0_0;
        list_0_0 = this.Resolve<global::System.String>();
        return new global::StrongInject.Owned<global::System.Collections.Generic.List<global::System.String>>(list_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(list_0_0);
        });
    }
}");
        }

        [Fact]
        public void CanResolveNamedTypeFromGenericFactoryMethod2()
        {
            string userSource = @"
using StrongInject;
using System.Collections.Generic;

public partial class Container : IContainer<List<string[]>>
{
    [Factory] List<T> Resolve<T>() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.List<global::System.String[]>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.List<global::System.String[]>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.List<global::System.String[]> list_0_0;
        list_0_0 = this.Resolve<global::System.String[]>();
        TResult result;
        try
        {
            result = func(list_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(list_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Collections.Generic.List<global::System.String[]>> global::StrongInject.IContainer<global::System.Collections.Generic.List<global::System.String[]>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.List<global::System.String[]> list_0_0;
        list_0_0 = this.Resolve<global::System.String[]>();
        return new global::StrongInject.Owned<global::System.Collections.Generic.List<global::System.String[]>>(list_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(list_0_0);
        });
    }
}");
        }

        [Fact]
        public void CanResolveNamedTypeFromGenericFactoryMethod3()
        {
            string userSource = @"
using StrongInject;
using System.Collections.Generic;

public partial class Container : IContainer<List<string[]>>
{
    [Factory] List<T[]> Resolve<T>() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.List<global::System.String[]>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.List<global::System.String[]>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.List<global::System.String[]> list_0_0;
        list_0_0 = this.Resolve<global::System.String>();
        TResult result;
        try
        {
            result = func(list_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(list_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Collections.Generic.List<global::System.String[]>> global::StrongInject.IContainer<global::System.Collections.Generic.List<global::System.String[]>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.List<global::System.String[]> list_0_0;
        list_0_0 = this.Resolve<global::System.String>();
        return new global::StrongInject.Owned<global::System.Collections.Generic.List<global::System.String[]>>(list_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(list_0_0);
        });
    }
}");
        }

        [Fact]
        public void CanResolveNamedTypeFromGenericFactoryMethod4()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<(int, object, int, int)>
{
    [Factory] (T1, T2, T1, T3) Resolve<T1, T2, T3>() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Object, global::System.Int32, global::System.Int32)>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Object, global::System.Int32, global::System.Int32), TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Object, global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = this.Resolve<global::System.Int32, global::System.Object, global::System.Int32>();
        TResult result;
        try
        {
            result = func(valueTuple_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Object, global::System.Int32, global::System.Int32)> global::StrongInject.IContainer<(global::System.Int32, global::System.Object, global::System.Int32, global::System.Int32)>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Object, global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = this.Resolve<global::System.Int32, global::System.Object, global::System.Int32>();
        return new global::StrongInject.Owned<(global::System.Int32, global::System.Object, global::System.Int32, global::System.Int32)>(valueTuple_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanResolveArrayTypeFromGenericFactoryMethod()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<(int, object, int, string)[]>
{
    [Factory] (T1, T2, T1, T3)[] Resolve<T1, T2, T3>() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Object, global::System.Int32, global::System.String)[]>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Object, global::System.Int32, global::System.String)[], TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Object, global::System.Int32, global::System.String)[] _0_0;
        _0_0 = this.Resolve<global::System.Int32, global::System.Object, global::System.String>();
        TResult result;
        try
        {
            result = func(_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Object, global::System.Int32, global::System.String)[]> global::StrongInject.IContainer<(global::System.Int32, global::System.Object, global::System.Int32, global::System.String)[]>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Object, global::System.Int32, global::System.String)[] _0_0;
        _0_0 = this.Resolve<global::System.Int32, global::System.Object, global::System.String>();
        return new global::StrongInject.Owned<(global::System.Int32, global::System.Object, global::System.Int32, global::System.String)[]>(_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(_0_0);
        });
    }
}");
        }

        [Fact]
        public void CanResolveTypeIncludingClassTypeParameterFromGenericFactoryMethod()
        {
            string userSource = @"
using StrongInject;

public partial class Container<T> : IContainer<(T, int)>
{
    [Factory] (T, T1) Resolve<T1>() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container<T>
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(T, global::System.Int32)>.Run<TResult, TParam>(global::System.Func<(T, global::System.Int32), TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container<T>));
        (T, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = this.Resolve<global::System.Int32>();
        TResult result;
        try
        {
            result = func(valueTuple_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<(T, global::System.Int32)> global::StrongInject.IContainer<(T, global::System.Int32)>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container<T>));
        (T, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = this.Resolve<global::System.Int32>();
        return new global::StrongInject.Owned<(T, global::System.Int32)>(valueTuple_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorIfNotAllTypeParametersUsedInReturnType()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<(int, object, int)>
{
    [Factory] (T1, T2, T1) Resolve<T1, T2, T3>() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (4,22): Error SI0102: Error while resolving dependencies for '(int, object, int)': We have no source for instance of type '(int, object, int)'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (6,6): Error SI0020: All type parameters must be used in return type of generic factory method 'Container.Resolve<T1, T2, T3>()'
                // Factory
                new DiagnosticResult("SI0020", @"Factory", DiagnosticSeverity.Error).WithLocation(6, 6));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Object, global::System.Int32)>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Object, global::System.Int32), TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Object, global::System.Int32)> global::StrongInject.IContainer<(global::System.Int32, global::System.Object, global::System.Int32)>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void LooksForRegisteredInstancesOfArgumentsOfConstructedType()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A<int>))]
public partial class Container : IContainer<int>
{
    [Factory] T Resolve<T>(A<T> a) => default;
}

public class A<T>
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_1;
        global::System.Int32 int32_0_0;
        a_0_1 = new global::A<global::System.Int32>();
        int32_0_0 = this.Resolve<global::System.Int32>(a: a_0_1);
        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_1;
        global::System.Int32 int32_0_0;
        a_0_1 = new global::A<global::System.Int32>();
        int32_0_0 = this.Resolve<global::System.Int32>(a: a_0_1);
        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorOnRecursiveGenericMethodFactoryDependencies()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<int>
{
    [Factory] T Resolve<T>(A<T> a) => default;
}

public class A<T>
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (4,22): Error SI0107: Error while resolving dependencies for 'int': The Dependency tree is deeper than the maximum depth of 200.
                // Container
                new DiagnosticResult("SI0107", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void FollowsRecursiveGenericMethodFactoryDependenciesToPossibleResolution()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A<A<A<A<A<A<A<A<A<A<int>>>>>>>>>>))]
public partial class Container : IContainer<int>
{
    [Factory] T Resolve<T>(A<T> a) => default;
}

public class A<T>
{
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>>> a_0_10;
        global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>> a_0_9;
        global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>> a_0_8;
        global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>> a_0_7;
        global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>> a_0_6;
        global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>> a_0_5;
        global::A<global::A<global::A<global::A<global::System.Int32>>>> a_0_4;
        global::A<global::A<global::A<global::System.Int32>>> a_0_3;
        global::A<global::A<global::System.Int32>> a_0_2;
        global::A<global::System.Int32> a_0_1;
        global::System.Int32 int32_0_0;
        a_0_10 = new global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>>>();
        a_0_9 = this.Resolve<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>>>(a: a_0_10);
        try
        {
            a_0_8 = this.Resolve<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>>(a: a_0_9);
            try
            {
                a_0_7 = this.Resolve<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>(a: a_0_8);
                try
                {
                    a_0_6 = this.Resolve<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>(a: a_0_7);
                    try
                    {
                        a_0_5 = this.Resolve<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>(a: a_0_6);
                        try
                        {
                            a_0_4 = this.Resolve<global::A<global::A<global::A<global::A<global::System.Int32>>>>>(a: a_0_5);
                            try
                            {
                                a_0_3 = this.Resolve<global::A<global::A<global::A<global::System.Int32>>>>(a: a_0_4);
                                try
                                {
                                    a_0_2 = this.Resolve<global::A<global::A<global::System.Int32>>>(a: a_0_3);
                                    try
                                    {
                                        a_0_1 = this.Resolve<global::A<global::System.Int32>>(a: a_0_2);
                                        try
                                        {
                                            int32_0_0 = this.Resolve<global::System.Int32>(a: a_0_1);
                                        }
                                        catch
                                        {
                                            global::StrongInject.Helpers.Dispose(a_0_1);
                                            throw;
                                        }
                                    }
                                    catch
                                    {
                                        global::StrongInject.Helpers.Dispose(a_0_2);
                                        throw;
                                    }
                                }
                                catch
                                {
                                    global::StrongInject.Helpers.Dispose(a_0_3);
                                    throw;
                                }
                            }
                            catch
                            {
                                global::StrongInject.Helpers.Dispose(a_0_4);
                                throw;
                            }
                        }
                        catch
                        {
                            global::StrongInject.Helpers.Dispose(a_0_5);
                            throw;
                        }
                    }
                    catch
                    {
                        global::StrongInject.Helpers.Dispose(a_0_6);
                        throw;
                    }
                }
                catch
                {
                    global::StrongInject.Helpers.Dispose(a_0_7);
                    throw;
                }
            }
            catch
            {
                global::StrongInject.Helpers.Dispose(a_0_8);
                throw;
            }
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(a_0_9);
            throw;
        }

        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(a_0_1);
            global::StrongInject.Helpers.Dispose(a_0_2);
            global::StrongInject.Helpers.Dispose(a_0_3);
            global::StrongInject.Helpers.Dispose(a_0_4);
            global::StrongInject.Helpers.Dispose(a_0_5);
            global::StrongInject.Helpers.Dispose(a_0_6);
            global::StrongInject.Helpers.Dispose(a_0_7);
            global::StrongInject.Helpers.Dispose(a_0_8);
            global::StrongInject.Helpers.Dispose(a_0_9);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>>> a_0_10;
        global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>> a_0_9;
        global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>> a_0_8;
        global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>> a_0_7;
        global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>> a_0_6;
        global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>> a_0_5;
        global::A<global::A<global::A<global::A<global::System.Int32>>>> a_0_4;
        global::A<global::A<global::A<global::System.Int32>>> a_0_3;
        global::A<global::A<global::System.Int32>> a_0_2;
        global::A<global::System.Int32> a_0_1;
        global::System.Int32 int32_0_0;
        a_0_10 = new global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>>>();
        a_0_9 = this.Resolve<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>>>(a: a_0_10);
        try
        {
            a_0_8 = this.Resolve<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>>(a: a_0_9);
            try
            {
                a_0_7 = this.Resolve<global::A<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>>(a: a_0_8);
                try
                {
                    a_0_6 = this.Resolve<global::A<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>>(a: a_0_7);
                    try
                    {
                        a_0_5 = this.Resolve<global::A<global::A<global::A<global::A<global::A<global::System.Int32>>>>>>(a: a_0_6);
                        try
                        {
                            a_0_4 = this.Resolve<global::A<global::A<global::A<global::A<global::System.Int32>>>>>(a: a_0_5);
                            try
                            {
                                a_0_3 = this.Resolve<global::A<global::A<global::A<global::System.Int32>>>>(a: a_0_4);
                                try
                                {
                                    a_0_2 = this.Resolve<global::A<global::A<global::System.Int32>>>(a: a_0_3);
                                    try
                                    {
                                        a_0_1 = this.Resolve<global::A<global::System.Int32>>(a: a_0_2);
                                        try
                                        {
                                            int32_0_0 = this.Resolve<global::System.Int32>(a: a_0_1);
                                        }
                                        catch
                                        {
                                            global::StrongInject.Helpers.Dispose(a_0_1);
                                            throw;
                                        }
                                    }
                                    catch
                                    {
                                        global::StrongInject.Helpers.Dispose(a_0_2);
                                        throw;
                                    }
                                }
                                catch
                                {
                                    global::StrongInject.Helpers.Dispose(a_0_3);
                                    throw;
                                }
                            }
                            catch
                            {
                                global::StrongInject.Helpers.Dispose(a_0_4);
                                throw;
                            }
                        }
                        catch
                        {
                            global::StrongInject.Helpers.Dispose(a_0_5);
                            throw;
                        }
                    }
                    catch
                    {
                        global::StrongInject.Helpers.Dispose(a_0_6);
                        throw;
                    }
                }
                catch
                {
                    global::StrongInject.Helpers.Dispose(a_0_7);
                    throw;
                }
            }
            catch
            {
                global::StrongInject.Helpers.Dispose(a_0_8);
                throw;
            }
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(a_0_9);
            throw;
        }

        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(a_0_1);
            global::StrongInject.Helpers.Dispose(a_0_2);
            global::StrongInject.Helpers.Dispose(a_0_3);
            global::StrongInject.Helpers.Dispose(a_0_4);
            global::StrongInject.Helpers.Dispose(a_0_5);
            global::StrongInject.Helpers.Dispose(a_0_6);
            global::StrongInject.Helpers.Dispose(a_0_7);
            global::StrongInject.Helpers.Dispose(a_0_8);
            global::StrongInject.Helpers.Dispose(a_0_9);
        });
    }
}");
        }

        [Fact]
        public void ErrorIfTypeParametersCantMatch()
        {
            string userSource = @"
using StrongInject;
using System.Collections.Generic;

public partial class Container<T> : IContainer<List<(string, int, object)>>
{
    [Factory] List<(T1, T2, T1)> Resolve<T1, T2>() => default;
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (5,22): Error SI0102: Error while resolving dependencies for 'System.Collections.Generic.List<(string, int, object)>': We have no source for instance of type 'System.Collections.Generic.List<(string, int, object)>'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container<T>
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.List<(global::System.String, global::System.Int32, global::System.Object)>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.List<(global::System.String, global::System.Int32, global::System.Object)>, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Collections.Generic.List<(global::System.String, global::System.Int32, global::System.Object)>> global::StrongInject.IContainer<global::System.Collections.Generic.List<(global::System.String, global::System.Int32, global::System.Object)>>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void TestNewConstraint()
        {
            string userSource = @"
using StrongInject;

public partial class Container1 : IContainer<A>, IContainer<A?>, IContainer<B>, IContainer<C>, IContainer<D>, IContainer<E>, IContainer<F>, IContainer<int>, IContainer<string>
{
    [Factory] T Resolve<T>() where T : new() => default;
}

public partial class Container2<T1> : IContainer<T1> where T1 : new()
{
    [Factory] T Resolve<T>() where T : new() => default;
}

public partial class Container3<T1> : IContainer<T1>
{
    [Factory] T Resolve<T>() where T : new() => default;
}

public struct A { public A(int i){} }
public class B {}
public abstract class C {}
public class D { public D() {} }
public class E { internal E() {} }
public class F { public F(int i) {} }
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (4,22): Error SI0102: Error while resolving dependencies for 'C': We have no source for instance of type 'C'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'C': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'C' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'E': We have no source for instance of type 'E'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'E': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'E' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'F': We have no source for instance of type 'F'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'F': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'F' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'string': We have no source for instance of type 'string'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'string': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'string' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (14,22): Error SI0102: Error while resolving dependencies for 'T1': We have no source for instance of type 'T1'
                // Container3
                new DiagnosticResult("SI0102", @"Container3", DiagnosticSeverity.Error).WithLocation(14, 22),
                // (14,22): Warning SI1106: Warning while resolving dependencies for 'T1': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'T1' as the required type arguments do not satisfy the generic constraints.
                // Container3
                new DiagnosticResult("SI1106", @"Container3", DiagnosticSeverity.Warning).WithLocation(14, 22));
            Assert.Equal(3, generated.Length);
            var ordered = generated.OrderBy(x => x).ToArray();
            ordered[0].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container1
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::A a_0_0;
        a_0_0 = this.Resolve<global::A>();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::A a_0_0;
        a_0_0 = this.Resolve<global::A>();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::A?>.Run<TResult, TParam>(global::System.Func<global::A?, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::A? nullable_0_0;
        nullable_0_0 = this.Resolve<global::A?>();
        TResult result;
        try
        {
            result = func(nullable_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A?> global::StrongInject.IContainer<global::A?>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::A? nullable_0_0;
        nullable_0_0 = this.Resolve<global::A?>();
        return new global::StrongInject.Owned<global::A?>(nullable_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::B b_0_0;
        b_0_0 = this.Resolve<global::B>();
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(b_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::B b_0_0;
        b_0_0 = this.Resolve<global::B>();
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(b_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::C>.Run<TResult, TParam>(global::System.Func<global::C, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::C> global::StrongInject.IContainer<global::C>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::D>.Run<TResult, TParam>(global::System.Func<global::D, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::D d_0_0;
        d_0_0 = this.Resolve<global::D>();
        TResult result;
        try
        {
            result = func(d_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(d_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::D> global::StrongInject.IContainer<global::D>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::D d_0_0;
        d_0_0 = this.Resolve<global::D>();
        return new global::StrongInject.Owned<global::D>(d_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(d_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::E>.Run<TResult, TParam>(global::System.Func<global::E, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::E> global::StrongInject.IContainer<global::E>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::F>.Run<TResult, TParam>(global::System.Func<global::F, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::F> global::StrongInject.IContainer<global::F>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::System.Int32 int32_0_0;
        int32_0_0 = this.Resolve<global::System.Int32>();
        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::System.Int32 int32_0_0;
        int32_0_0 = this.Resolve<global::System.Int32>();
        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::System.String>.Run<TResult, TParam>(global::System.Func<global::System.String, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.String> global::StrongInject.IContainer<global::System.String>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
            ordered[1].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container2<T1>
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<T1>.Run<TResult, TParam>(global::System.Func<T1, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container2<T1>));
        T1 t1_0_0;
        t1_0_0 = this.Resolve<T1>();
        TResult result;
        try
        {
            result = func(t1_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(t1_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<T1> global::StrongInject.IContainer<T1>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container2<T1>));
        T1 t1_0_0;
        t1_0_0 = this.Resolve<T1>();
        return new global::StrongInject.Owned<T1>(t1_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(t1_0_0);
        });
    }
}");
            ordered[2].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container3<T1>
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<T1>.Run<TResult, TParam>(global::System.Func<T1, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<T1> global::StrongInject.IContainer<T1>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void TestStructConstraint()
        {
            string userSource = @"
using StrongInject;

public partial class Container1 : IContainer<A>, IContainer<A?>, IContainer<B>, IContainer<C>, IContainer<System.ValueType>
{
    [Factory] T Resolve<T>() where T : struct => default;
}

public partial class Container2<T1> : IContainer<T1> where T1 : struct
{
    [Factory] T Resolve<T>() where T : struct => default;
}

public partial class Container3<T1> : IContainer<T1>
{
    [Factory] T Resolve<T>() where T : struct => default;
}

public struct A {}
public class B {}
public enum C {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (4,22): Error SI0102: Error while resolving dependencies for 'A?': We have no source for instance of type 'A?'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'A?': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'A?' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'B': We have no source for instance of type 'B'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'B': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'B' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'System.ValueType': We have no source for instance of type 'System.ValueType'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'System.ValueType': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'System.ValueType' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (14,22): Error SI0102: Error while resolving dependencies for 'T1': We have no source for instance of type 'T1'
                // Container3
                new DiagnosticResult("SI0102", @"Container3", DiagnosticSeverity.Error).WithLocation(14, 22),
                // (14,22): Warning SI1106: Warning while resolving dependencies for 'T1': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'T1' as the required type arguments do not satisfy the generic constraints.
                // Container3
                new DiagnosticResult("SI1106", @"Container3", DiagnosticSeverity.Warning).WithLocation(14, 22));
            Assert.Equal(3, generated.Length);
            var ordered = generated.OrderBy(x => x).ToArray();
            ordered[0].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container1
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::A a_0_0;
        a_0_0 = this.Resolve<global::A>();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::A a_0_0;
        a_0_0 = this.Resolve<global::A>();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::A?>.Run<TResult, TParam>(global::System.Func<global::A?, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A?> global::StrongInject.IContainer<global::A?>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::C>.Run<TResult, TParam>(global::System.Func<global::C, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::C c_0_0;
        c_0_0 = this.Resolve<global::C>();
        TResult result;
        try
        {
            result = func(c_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::C> global::StrongInject.IContainer<global::C>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::C c_0_0;
        c_0_0 = this.Resolve<global::C>();
        return new global::StrongInject.Owned<global::C>(c_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::System.ValueType>.Run<TResult, TParam>(global::System.Func<global::System.ValueType, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.ValueType> global::StrongInject.IContainer<global::System.ValueType>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
            ordered[1].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container2<T1>
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<T1>.Run<TResult, TParam>(global::System.Func<T1, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container2<T1>));
        T1 t1_0_0;
        t1_0_0 = this.Resolve<T1>();
        TResult result;
        try
        {
            result = func(t1_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(t1_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<T1> global::StrongInject.IContainer<T1>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container2<T1>));
        T1 t1_0_0;
        t1_0_0 = this.Resolve<T1>();
        return new global::StrongInject.Owned<T1>(t1_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(t1_0_0);
        });
    }
}");
            ordered[2].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container3<T1>
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<T1>.Run<TResult, TParam>(global::System.Func<T1, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<T1> global::StrongInject.IContainer<T1>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void TestReferenceConstraint()
        {
            string userSource = @"
using StrongInject;

public partial class Container1 : IContainer<A>, IContainer<A?>, IContainer<B>, IContainer<C>, IContainer<I>, IContainer<System.ValueType>
{
    [Factory] T Resolve<T>() where T : class => default;
}

public partial class Container2<T1> : IContainer<T1> where T1 : class
{
    [Factory] T Resolve<T>() where T : class => default;
}

public partial class Container3<T1> : IContainer<T1>
{
    [Factory] T Resolve<T>() where T : class => default;
}

public struct A {}
public class B {}
public enum C {}
public interface I {}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (4,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'A': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'A' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'A?': We have no source for instance of type 'A?'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'A?': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'A?' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'C': We have no source for instance of type 'C'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'C': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'C' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (14,22): Error SI0102: Error while resolving dependencies for 'T1': We have no source for instance of type 'T1'
                // Container3
                new DiagnosticResult("SI0102", @"Container3", DiagnosticSeverity.Error).WithLocation(14, 22),
                // (14,22): Warning SI1106: Warning while resolving dependencies for 'T1': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'T1' as the required type arguments do not satisfy the generic constraints.
                // Container3
                new DiagnosticResult("SI1106", @"Container3", DiagnosticSeverity.Warning).WithLocation(14, 22));
            Assert.Equal(3, generated.Length);
            var ordered = generated.OrderBy(x => x).ToArray();
            ordered[0].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container1
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::A?>.Run<TResult, TParam>(global::System.Func<global::A?, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A?> global::StrongInject.IContainer<global::A?>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::B b_0_0;
        b_0_0 = this.Resolve<global::B>();
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(b_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::B b_0_0;
        b_0_0 = this.Resolve<global::B>();
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(b_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::C>.Run<TResult, TParam>(global::System.Func<global::C, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::C> global::StrongInject.IContainer<global::C>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::I>.Run<TResult, TParam>(global::System.Func<global::I, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::I i_0_0;
        i_0_0 = this.Resolve<global::I>();
        TResult result;
        try
        {
            result = func(i_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(i_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::I> global::StrongInject.IContainer<global::I>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::I i_0_0;
        i_0_0 = this.Resolve<global::I>();
        return new global::StrongInject.Owned<global::I>(i_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(i_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::System.ValueType>.Run<TResult, TParam>(global::System.Func<global::System.ValueType, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::System.ValueType valueType_0_0;
        valueType_0_0 = this.Resolve<global::System.ValueType>();
        TResult result;
        try
        {
            result = func(valueType_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(valueType_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.ValueType> global::StrongInject.IContainer<global::System.ValueType>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::System.ValueType valueType_0_0;
        valueType_0_0 = this.Resolve<global::System.ValueType>();
        return new global::StrongInject.Owned<global::System.ValueType>(valueType_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(valueType_0_0);
        });
    }
}");
            ordered[1].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container2<T1>
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<T1>.Run<TResult, TParam>(global::System.Func<T1, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container2<T1>));
        T1 t1_0_0;
        t1_0_0 = this.Resolve<T1>();
        TResult result;
        try
        {
            result = func(t1_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(t1_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<T1> global::StrongInject.IContainer<T1>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container2<T1>));
        T1 t1_0_0;
        t1_0_0 = this.Resolve<T1>();
        return new global::StrongInject.Owned<T1>(t1_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(t1_0_0);
        });
    }
}");
            ordered[2].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container3<T1>
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<T1>.Run<TResult, TParam>(global::System.Func<T1, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<T1> global::StrongInject.IContainer<T1>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void TestUnmanagedConstraint()
        {
            string userSource = @"
using StrongInject;

public partial class Container1 : IContainer<A>, IContainer<A?>, IContainer<B>, IContainer<C>, IContainer<D>, IContainer<E>, IContainer<F<int>>, IContainer<G<int>>, IContainer<G<D>>, IContainer<System.ValueType>
{
    [Factory] T Resolve<T>() where T : unmanaged => default;
}

public partial class Container2<T1> : IContainer<T1> where T1 : unmanaged
{
    [Factory] T Resolve<T>() where T : unmanaged => default;
}

public partial class Container3<T1> : IContainer<T1>
{
    [Factory] T Resolve<T>() where T : unmanaged => default;
}

public struct A {}
public class B {}
public enum C {}
public struct D { string _s; }
public struct E { int _e; }
public struct F<T> where T : unmanaged { T _t; }
public struct G<T> where T : struct { T _t; }
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify(
                // (22,26): Warning CS0169: The field 'D._s' is never used
                // _s
                new DiagnosticResult("CS0169", @"_s", DiagnosticSeverity.Warning).WithLocation(22, 26),
                // (23,23): Warning CS0169: The field 'E._e' is never used
                // _e
                new DiagnosticResult("CS0169", @"_e", DiagnosticSeverity.Warning).WithLocation(23, 23),
                // (24,44): Warning CS0169: The field 'F<T>._t' is never used
                // _t
                new DiagnosticResult("CS0169", @"_t", DiagnosticSeverity.Warning).WithLocation(24, 44),
                // (25,41): Warning CS0169: The field 'G<T>._t' is never used
                // _t
                new DiagnosticResult("CS0169", @"_t", DiagnosticSeverity.Warning).WithLocation(25, 41));
            generatorDiagnostics.Verify(
                // (4,22): Error SI0102: Error while resolving dependencies for 'A?': We have no source for instance of type 'A?'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'A?': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'A?' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'B': We have no source for instance of type 'B'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'B': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'B' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'D': We have no source for instance of type 'D'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'D': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'D' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'G<D>': We have no source for instance of type 'G<D>'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'G<D>': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'G<D>' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'System.ValueType': We have no source for instance of type 'System.ValueType'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'System.ValueType': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'System.ValueType' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (14,22): Error SI0102: Error while resolving dependencies for 'T1': We have no source for instance of type 'T1'
                // Container3
                new DiagnosticResult("SI0102", @"Container3", DiagnosticSeverity.Error).WithLocation(14, 22),
                // (14,22): Warning SI1106: Warning while resolving dependencies for 'T1': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'T1' as the required type arguments do not satisfy the generic constraints.
                // Container3
                new DiagnosticResult("SI1106", @"Container3", DiagnosticSeverity.Warning).WithLocation(14, 22));
            Assert.Equal(3, generated.Length);
            var ordered = generated.OrderBy(x => x).ToArray();
            ordered[0].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container1
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::A a_0_0;
        a_0_0 = this.Resolve<global::A>();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::A a_0_0;
        a_0_0 = this.Resolve<global::A>();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::A?>.Run<TResult, TParam>(global::System.Func<global::A?, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A?> global::StrongInject.IContainer<global::A?>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::C>.Run<TResult, TParam>(global::System.Func<global::C, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::C c_0_0;
        c_0_0 = this.Resolve<global::C>();
        TResult result;
        try
        {
            result = func(c_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::C> global::StrongInject.IContainer<global::C>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::C c_0_0;
        c_0_0 = this.Resolve<global::C>();
        return new global::StrongInject.Owned<global::C>(c_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::D>.Run<TResult, TParam>(global::System.Func<global::D, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::D> global::StrongInject.IContainer<global::D>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::E>.Run<TResult, TParam>(global::System.Func<global::E, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::E e_0_0;
        e_0_0 = this.Resolve<global::E>();
        TResult result;
        try
        {
            result = func(e_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::E> global::StrongInject.IContainer<global::E>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::E e_0_0;
        e_0_0 = this.Resolve<global::E>();
        return new global::StrongInject.Owned<global::E>(e_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::F<global::System.Int32>>.Run<TResult, TParam>(global::System.Func<global::F<global::System.Int32>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::F<global::System.Int32> f_0_0;
        f_0_0 = this.Resolve<global::F<global::System.Int32>>();
        TResult result;
        try
        {
            result = func(f_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::F<global::System.Int32>> global::StrongInject.IContainer<global::F<global::System.Int32>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::F<global::System.Int32> f_0_0;
        f_0_0 = this.Resolve<global::F<global::System.Int32>>();
        return new global::StrongInject.Owned<global::F<global::System.Int32>>(f_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::G<global::System.Int32>>.Run<TResult, TParam>(global::System.Func<global::G<global::System.Int32>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::G<global::System.Int32> g_0_0;
        g_0_0 = this.Resolve<global::G<global::System.Int32>>();
        TResult result;
        try
        {
            result = func(g_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::G<global::System.Int32>> global::StrongInject.IContainer<global::G<global::System.Int32>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container1));
        global::G<global::System.Int32> g_0_0;
        g_0_0 = this.Resolve<global::G<global::System.Int32>>();
        return new global::StrongInject.Owned<global::G<global::System.Int32>>(g_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::G<global::D>>.Run<TResult, TParam>(global::System.Func<global::G<global::D>, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::G<global::D>> global::StrongInject.IContainer<global::G<global::D>>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::System.ValueType>.Run<TResult, TParam>(global::System.Func<global::System.ValueType, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.ValueType> global::StrongInject.IContainer<global::System.ValueType>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
            ordered[1].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container2<T1>
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<T1>.Run<TResult, TParam>(global::System.Func<T1, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container2<T1>));
        T1 t1_0_0;
        t1_0_0 = this.Resolve<T1>();
        TResult result;
        try
        {
            result = func(t1_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(t1_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<T1> global::StrongInject.IContainer<T1>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container2<T1>));
        T1 t1_0_0;
        t1_0_0 = this.Resolve<T1>();
        return new global::StrongInject.Owned<T1>(t1_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(t1_0_0);
        });
    }
}");
            ordered[2].Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container3<T1>
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<T1>.Run<TResult, TParam>(global::System.Func<T1, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<T1> global::StrongInject.IContainer<T1>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void TestTypeConstraints1()
        {
            string userSource = @"
using StrongInject;

public partial class Container1<T1> : IContainer<T1> where T1 : A 
{
    [Factory] T Resolve<T>() where T : A => default;
}

public partial class Container2<T1> : IContainer<T1> where T1 : B 
{
    [Factory] T Resolve<T>() where T : A => default;
}

public partial class Container3<T1, T2> : IContainer<T2> where T1 : A where T2 : T1 
{
    [Factory] T Resolve<T>() where T : A => default;
}

public partial class Container4<T1, T2> : IContainer<T2> where T1 : B where T2 : T1
{
    [Factory] T Resolve<T>() where T : A => default;
}

public partial class Container5<T1> : IContainer<T1> where T1 : C 
{
    [Factory] T Resolve<T>() where T : A => default;
}

public partial class Container6<T1, T2> : IContainer<T2> where T1 : C where T2 : T1 
{
    [Factory] T Resolve<T>() where T : A => default;
}

public partial class Container7 : IContainer<A>, IContainer<B>, IContainer<C>
{
    [Factory] T Resolve<T>() where T : A => default;
}

public class A {}
public class B : A {}
public class C {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (24,22): Error SI0102: Error while resolving dependencies for 'T1': We have no source for instance of type 'T1'
                // Container5
                new DiagnosticResult("SI0102", @"Container5", DiagnosticSeverity.Error).WithLocation(24, 22),
                // (24,22): Warning SI1106: Warning while resolving dependencies for 'T1': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'T1' as the required type arguments do not satisfy the generic constraints.
                // Container5
                new DiagnosticResult("SI1106", @"Container5", DiagnosticSeverity.Warning).WithLocation(24, 22),
                // (29,22): Error SI0102: Error while resolving dependencies for 'T2': We have no source for instance of type 'T2'
                // Container6
                new DiagnosticResult("SI0102", @"Container6", DiagnosticSeverity.Error).WithLocation(29, 22),
                // (29,22): Warning SI1106: Warning while resolving dependencies for 'T2': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'T2' as the required type arguments do not satisfy the generic constraints.
                // Container6
                new DiagnosticResult("SI1106", @"Container6", DiagnosticSeverity.Warning).WithLocation(29, 22),
                // (34,22): Error SI0102: Error while resolving dependencies for 'C': We have no source for instance of type 'C'
                // Container7
                new DiagnosticResult("SI0102", @"Container7", DiagnosticSeverity.Error).WithLocation(34, 22),
                // (34,22): Warning SI1106: Warning while resolving dependencies for 'C': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'C' as the required type arguments do not satisfy the generic constraints.
                // Container7
                new DiagnosticResult("SI1106", @"Container7", DiagnosticSeverity.Warning).WithLocation(34, 22));
            Assert.Equal(7, generated.Length);
        }

        [Fact]
        public void TestTypeConstraints2()
        {
            string userSource = @"
using StrongInject;
using System;

public partial class Container : IContainer<Enum>, IContainer<E>, IContainer<E?>, IContainer<S>
{
    [Factory] T Resolve<T>() where T : Enum => default;
}

public enum E {}
public struct S {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (5,22): Error SI0102: Error while resolving dependencies for 'E?': We have no source for instance of type 'E?'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22),
                // (5,22): Warning SI1106: Warning while resolving dependencies for 'E?': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'E?' as the required type arguments do not satisfy the generic constraints.
                // Container
                new DiagnosticResult("SI1106", @"Container", DiagnosticSeverity.Warning).WithLocation(5, 22),
                // (5,22): Error SI0102: Error while resolving dependencies for 'S': We have no source for instance of type 'S'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22),
                // (5,22): Warning SI1106: Warning while resolving dependencies for 'S': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'S' as the required type arguments do not satisfy the generic constraints.
                // Container
                new DiagnosticResult("SI1106", @"Container", DiagnosticSeverity.Warning).WithLocation(5, 22));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Enum>.Run<TResult, TParam>(global::System.Func<global::System.Enum, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Enum enum_0_0;
        enum_0_0 = this.Resolve<global::System.Enum>();
        TResult result;
        try
        {
            result = func(enum_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(enum_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Enum> global::StrongInject.IContainer<global::System.Enum>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Enum enum_0_0;
        enum_0_0 = this.Resolve<global::System.Enum>();
        return new global::StrongInject.Owned<global::System.Enum>(enum_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(enum_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::E>.Run<TResult, TParam>(global::System.Func<global::E, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::E e_0_0;
        e_0_0 = this.Resolve<global::E>();
        TResult result;
        try
        {
            result = func(e_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::E> global::StrongInject.IContainer<global::E>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::E e_0_0;
        e_0_0 = this.Resolve<global::E>();
        return new global::StrongInject.Owned<global::E>(e_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::E?>.Run<TResult, TParam>(global::System.Func<global::E?, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::E?> global::StrongInject.IContainer<global::E?>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::S>.Run<TResult, TParam>(global::System.Func<global::S, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::S> global::StrongInject.IContainer<global::S>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void TestTypeConstraints3()
        {
            string userSource = @"
using StrongInject;

public partial class Container1 : IContainer<I>, IContainer<C>, IContainer<C2>, IContainer<S>, IContainer<S?>, IContainer<S2>
{
    [Factory] T Resolve<T>() where T : I => default;
}

public partial class Container2<T1, T2> : IContainer<T2> where T1 : I where T2 : T1
{
    [Factory] T Resolve<T>() where T : I => default;
}

public partial class Container3<T1, T2> : IContainer<T2> where T1 : C where T2 : T1
{
    [Factory] T Resolve<T>() where T : I => default;
}

public interface I {}
public class C : I {}
public class C2 {}
public struct S : I {}
public struct S2 {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (4,22): Error SI0102: Error while resolving dependencies for 'C2': We have no source for instance of type 'C2'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'C2': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'C2' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'S?': We have no source for instance of type 'S?'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'S?': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'S?' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'S2': We have no source for instance of type 'S2'
                // Container1
                new DiagnosticResult("SI0102", @"Container1", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Warning SI1106: Warning while resolving dependencies for 'S2': factory method 'StrongInject.Generator.FactoryMethod' cannot be used to resolve instance of type 'S2' as the required type arguments do not satisfy the generic constraints.
                // Container1
                new DiagnosticResult("SI1106", @"Container1", DiagnosticSeverity.Warning).WithLocation(4, 22));
        }

        [Fact]
        public void TestTypeConstraints4()
        {
            string userSource = @"
using StrongInject;

public partial class Container<T1> : IContainer<T1>
{
    [Factory] T Resolve<T>() where T : T1 => default;
}

public partial class Container<T1, T2> : IContainer<T2> where T2 : T1
{
    [Factory] T Resolve<T>() where T : T1 => default;
}

public partial class Container<T1, T2, T3> : IContainer<T3> where T2 : T1 where T3 : T2
{
    [Factory] T Resolve<T>() where T : T1 => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
        }

        [Fact]
        public void TestTypeConstraints5()
        {
            string userSource = @"
using StrongInject;

public interface A<out T> {}

public partial class Container1 : IContainer<(A<int>, int)>
{
    [Factory] (T1, T2) Resolve<T1, T2>() where T1 : A<T2> => default;
}

public partial class Container2 : IContainer<(A<int>, string)>
{
    [Factory] (T1, T2) Resolve<T1, T2>() where T1 : A<T2> => default;
}

public partial class Container3 : IContainer<(A<string>, object)>
{
    [Factory] (T1, T2) Resolve<T1, T2>() where T1 : A<T2> => default;
}

public partial class Container4 : IContainer<(A<object>, string)>
{
    [Factory] (T1, T2) Resolve<T1, T2>() where T1 : A<T2> => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (11,22): Error SI0102: Error while resolving dependencies for '(A<int>, string)': We have no source for instance of type '(A<int>, string)'
                // Container2
                new DiagnosticResult("SI0102", @"Container2", DiagnosticSeverity.Error).WithLocation(11, 22),
                // (11,22): Warning SI1106: Warning while resolving dependencies for '(A<int>, string)': factory method 'Container2.Resolve<T1, T2>()' cannot be used to resolve instance of type '(A<int>, string)' as the required type arguments do not satisfy the generic constraints.
                // Container2
                new DiagnosticResult("SI1106", @"Container2", DiagnosticSeverity.Warning).WithLocation(11, 22),
                // (21,22): Error SI0102: Error while resolving dependencies for '(A<object>, string)': We have no source for instance of type '(A<object>, string)'
                // Container4
                new DiagnosticResult("SI0102", @"Container4", DiagnosticSeverity.Error).WithLocation(21, 22),
                // (21,22): Warning SI1106: Warning while resolving dependencies for '(A<object>, string)': factory method 'Container4.Resolve<T1, T2>()' cannot be used to resolve instance of type '(A<object>, string)' as the required type arguments do not satisfy the generic constraints.
                // Container4
                new DiagnosticResult("SI1106", @"Container4", DiagnosticSeverity.Warning).WithLocation(21, 22));
        }

        [Fact]
        public void TestTypeConstraints6()
        {
            string userSource = @"
using StrongInject;

public class A<T1, T2> {}

public partial class Container1<T> : IContainer<(A<T, A<int, string>[]>, string)>
{
    [Factory] (T1, T2) Resolve<T1, T2>() where T1 : A<T, A<int, T2>[]> => default;
}

public partial class Container2<T> : IContainer<(A<T, A<int, string>[]>, int)>
{
    [Factory] (T1, T2) Resolve<T1, T2>() where T1 : A<T, A<int, T2>[]> => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (11,22): Error SI0102: Error while resolving dependencies for '(A<T, A<int, string>[]>, int)': We have no source for instance of type '(A<T, A<int, string>[]>, int)'
                // Container2
                new DiagnosticResult("SI0102", @"Container2", DiagnosticSeverity.Error).WithLocation(11, 22),
                // (11,22): Warning SI1106: Warning while resolving dependencies for '(A<T, A<int, string>[]>, int)': factory method 'Container2<T>.Resolve<T1, T2>()' cannot be used to resolve instance of type '(A<T, A<int, string>[]>, int)' as the required type arguments do not satisfy the generic constraints.
                // Container2
                new DiagnosticResult("SI1106", @"Container2", DiagnosticSeverity.Warning).WithLocation(11, 22));
        }

        [Fact]
        public void CanImportGenericFactoryMethod()
        {
            string userSource = @"
using StrongInject;

public class Module
{
    [Factory] public static (T, T) M<T>() => default; 
}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<(int, int)>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Int32), TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = global::Module.M<global::System.Int32>();
        TResult result;
        try
        {
            result = func(valueTuple_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Int32)> global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = global::Module.M<global::System.Int32>();
        return new global::StrongInject.Owned<(global::System.Int32, global::System.Int32)>(valueTuple_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorIfGenericFactoryMethodsImportedFromMultipleModules()
        {
            string userSource = @"
using StrongInject;

public class Module1
{
    [Factory] public static (T, T) M<T>() => default; 
}

public class Module2
{
    [Factory] public static (T1, T2) M<T1, T2>() => default; 
}

[RegisterModule(typeof(Module1))]
[RegisterModule(typeof(Module2))]
public partial class Container : IContainer<(int, int)>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (16,22): Error SI0106: Error while resolving dependencies for '(int, int)': We have multiple sources for instance of type '(int, int)' and no best source. Try adding a single registration for '(int, int)' directly to the container, and moving any existing registrations for '(int, int)' on the container to an imported module.
                // Container
                new DiagnosticResult("SI0106", @"Container", DiagnosticSeverity.Error).WithLocation(16, 22));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Int32), TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Int32)> global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void IgnoresGenericFactoryMethodsWhereConstraintsDontMatch()
        {
            string userSource = @"
using StrongInject;

public class Module1
{
    [Factory] public static (T, T) M<T>() where T : class => default; 
}

public class Module2
{
    [Factory] public static (T1, T2) M<T1, T2>() => default; 
}

[RegisterModule(typeof(Module1))]
[RegisterModule(typeof(Module2))]
public partial class Container : IContainer<(int, int)>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Int32), TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = global::Module2.M<global::System.Int32, global::System.Int32>();
        TResult result;
        try
        {
            result = func(valueTuple_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Int32)> global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = global::Module2.M<global::System.Int32, global::System.Int32>();
        return new global::StrongInject.Owned<(global::System.Int32, global::System.Int32)>(valueTuple_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ModuleOverridesRegistrationsItImports()
        {
            string userSource = @"
using StrongInject;

public class Module1
{
    [Factory] public static (T, T) M<T>() => default; 
}

[RegisterModule(typeof(Module1))]
public class Module2
{
    [Factory] public static (T1, T2) M<T1, T2>() => default; 
}

[RegisterModule(typeof(Module2))]
public partial class Container : IContainer<(int, int)>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Int32), TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = global::Module2.M<global::System.Int32, global::System.Int32>();
        TResult result;
        try
        {
            result = func(valueTuple_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Int32)> global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = global::Module2.M<global::System.Int32, global::System.Int32>();
        return new global::StrongInject.Owned<(global::System.Int32, global::System.Int32)>(valueTuple_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ModuleDoesNotOverrideRegistrationsItImportsIfConstraintsDontMatch()
        {
            string userSource = @"
using StrongInject;

public class Module1
{
    [Factory] public static (T, T) M<T>() => default; 
}

[RegisterModule(typeof(Module1))]
public class Module2
{
    [Factory] public static (T1, T2) M<T1, T2>() where T1 : class => default; 
}

[RegisterModule(typeof(Module2))]
public partial class Container : IContainer<(int, int)>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Int32), TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = global::Module1.M<global::System.Int32>();
        TResult result;
        try
        {
            result = func(valueTuple_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Int32)> global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = global::Module1.M<global::System.Int32>();
        return new global::StrongInject.Owned<(global::System.Int32, global::System.Int32)>(valueTuple_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ModuleDoesNotOverrideModuleItDoesNotImport()
        {
            string userSource = @"
using StrongInject;

public class Module1
{
    [Factory] public static (T, T) M<T>() => default; 
}

[RegisterModule(typeof(Module1))]
public class Module2
{
    [Factory] public static (T1, T2) M<T1, T2>() where T1 : class => default; 
}

public class Module3
{
    [Factory] public static (T1, T2) M<T1, T2>() => default; 
}

[RegisterModule(typeof(Module2))]
[RegisterModule(typeof(Module3))]
public partial class Container : IContainer<(int, int)>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify(
                // (22,22): Error SI0106: Error while resolving dependencies for '(int, int)': We have multiple sources for instance of type '(int, int)' and no best source. Try adding a single registration for '(int, int)' directly to the container, and moving any existing registrations for '(int, int)' on the container to an imported module.
                // Container
                new DiagnosticResult("SI0106", @"Container", DiagnosticSeverity.Error).WithLocation(22, 22));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Int32), TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Int32)> global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void NoErrorIfSameModuleImportedTwice()
        {
            string userSource = @"
using StrongInject;

public class Module1
{
    [Factory] public static (T, T) M<T>() => default; 
}

[RegisterModule(typeof(Module1))]
public class Module2
{
    [Factory] public static (T1, T2) M<T1, T2>() where T1 : class => default; 
}

[RegisterModule(typeof(Module1))]
public class Module3
{
    [Factory] public static (T1, T2) M<T1, T2>() where T1 : class => default; 
}

[RegisterModule(typeof(Module2))]
[RegisterModule(typeof(Module3))]
public partial class Container : IContainer<(int, int)>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Int32), TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = global::Module1.M<global::System.Int32>();
        TResult result;
        try
        {
            result = func(valueTuple_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Int32)> global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_0;
        valueTuple_0_0 = global::Module1.M<global::System.Int32>();
        return new global::StrongInject.Owned<(global::System.Int32, global::System.Int32)>(valueTuple_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ResolveAllDeduplicatesIfSameModuleImportedTwice()
        {
            string userSource = @"
using StrongInject;

public class Module1
{
    [Factory] public static (T, T) M<T>() => default; 
}

[RegisterModule(typeof(Module1))]
public class Module2
{
    [Factory] public static (T1, T2) M<T1, T2>() => default; 
}

[RegisterModule(typeof(Module1))]
public class Module3
{
    [Factory] public static (T1, T2) M<T1, T2>() => default; 
}

[RegisterModule(typeof(Module2))]
[RegisterModule(typeof(Module3))]
public partial class Container : IContainer<(int, int)[]>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)[]>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Int32)[], TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_1;
        (global::System.Int32, global::System.Int32) valueTuple_0_2;
        (global::System.Int32, global::System.Int32) valueTuple_0_3;
        (global::System.Int32, global::System.Int32)[] _0_0;
        valueTuple_0_1 = global::Module2.M<global::System.Int32, global::System.Int32>();
        valueTuple_0_2 = global::Module1.M<global::System.Int32>();
        valueTuple_0_3 = global::Module3.M<global::System.Int32, global::System.Int32>();
        _0_0 = new (global::System.Int32, global::System.Int32)[]{((global::System.Int32, global::System.Int32))valueTuple_0_1, ((global::System.Int32, global::System.Int32))valueTuple_0_2, ((global::System.Int32, global::System.Int32))valueTuple_0_3, };
        TResult result;
        try
        {
            result = func(_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Int32)[]> global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)[]>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_1;
        (global::System.Int32, global::System.Int32) valueTuple_0_2;
        (global::System.Int32, global::System.Int32) valueTuple_0_3;
        (global::System.Int32, global::System.Int32)[] _0_0;
        valueTuple_0_1 = global::Module2.M<global::System.Int32, global::System.Int32>();
        valueTuple_0_2 = global::Module1.M<global::System.Int32>();
        valueTuple_0_3 = global::Module3.M<global::System.Int32, global::System.Int32>();
        _0_0 = new (global::System.Int32, global::System.Int32)[]{((global::System.Int32, global::System.Int32))valueTuple_0_1, ((global::System.Int32, global::System.Int32))valueTuple_0_2, ((global::System.Int32, global::System.Int32))valueTuple_0_3, };
        return new global::StrongInject.Owned<(global::System.Int32, global::System.Int32)[]>(_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void AppendGenericAndNonGenericResolutions()
        {
            string userSource = @"
using StrongInject;

public class Module1
{
    [Factory] public static (T, T) M<T>() => default; 
}

[RegisterModule(typeof(Module1))]
public class Module2
{
    [Factory] public static (T1, T2) M<T1, T2>() => default; 
}

[RegisterModule(typeof(Module1))]
public class Module3
{
    [Factory] public static (T1, T2) M<T1, T2>() => default; 
}

[RegisterModule(typeof(Module2))]
[RegisterModule(typeof(Module3))]
public partial class Container : IContainer<(int, int)[]>
{
    [Factory] public (int, int) M() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            comp.GetDiagnostics().Verify();
            generatorDiagnostics.Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)[]>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.Int32)[], TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_1;
        (global::System.Int32, global::System.Int32) valueTuple_0_2;
        (global::System.Int32, global::System.Int32) valueTuple_0_3;
        (global::System.Int32, global::System.Int32) valueTuple_0_4;
        (global::System.Int32, global::System.Int32)[] _0_0;
        valueTuple_0_1 = this.M();
        valueTuple_0_2 = global::Module2.M<global::System.Int32, global::System.Int32>();
        valueTuple_0_3 = global::Module1.M<global::System.Int32>();
        valueTuple_0_4 = global::Module3.M<global::System.Int32, global::System.Int32>();
        _0_0 = new (global::System.Int32, global::System.Int32)[]{((global::System.Int32, global::System.Int32))valueTuple_0_1, ((global::System.Int32, global::System.Int32))valueTuple_0_2, ((global::System.Int32, global::System.Int32))valueTuple_0_3, ((global::System.Int32, global::System.Int32))valueTuple_0_4, };
        TResult result;
        try
        {
            result = func(_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.Int32)[]> global::StrongInject.IContainer<(global::System.Int32, global::System.Int32)[]>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        (global::System.Int32, global::System.Int32) valueTuple_0_1;
        (global::System.Int32, global::System.Int32) valueTuple_0_2;
        (global::System.Int32, global::System.Int32) valueTuple_0_3;
        (global::System.Int32, global::System.Int32) valueTuple_0_4;
        (global::System.Int32, global::System.Int32)[] _0_0;
        valueTuple_0_1 = this.M();
        valueTuple_0_2 = global::Module2.M<global::System.Int32, global::System.Int32>();
        valueTuple_0_3 = global::Module1.M<global::System.Int32>();
        valueTuple_0_4 = global::Module3.M<global::System.Int32, global::System.Int32>();
        _0_0 = new (global::System.Int32, global::System.Int32)[]{((global::System.Int32, global::System.Int32))valueTuple_0_1, ((global::System.Int32, global::System.Int32))valueTuple_0_2, ((global::System.Int32, global::System.Int32))valueTuple_0_3, ((global::System.Int32, global::System.Int32))valueTuple_0_4, };
        return new global::StrongInject.Owned<(global::System.Int32, global::System.Int32)[]>(_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WrapsTypesInDecoratorsRegisteredByAttributes()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A), typeof(IA))]
[Register(typeof(B))]
[RegisterDecorator(typeof(Decorator1), typeof(IA))]
[RegisterDecorator(typeof(Decorator2), typeof(IA))]
public partial class Container : IAsyncContainer<IA>
{
}

public interface IA {}
public class A : IA {}
public class B {}
public class Decorator1 : IA
{
    public Decorator1(IA a){} 
}
public class Decorator2 : IA
{
    public Decorator2(IA a, B b){} 
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::IA iA_0_2;
        global::IA iA_0_1;
        global::B b_0_4;
        global::IA iA_0_0;
        a_0_3 = new global::A();
        iA_0_2 = (global::IA)a_0_3;
        iA_0_1 = new global::Decorator1(a: iA_0_2);
        b_0_4 = new global::B();
        iA_0_0 = new global::Decorator2(a: iA_0_1, b: b_0_4);
        TResult result;
        try
        {
            result = await func(iA_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::IA iA_0_2;
        global::IA iA_0_1;
        global::B b_0_4;
        global::IA iA_0_0;
        a_0_3 = new global::A();
        iA_0_2 = (global::IA)a_0_3;
        iA_0_1 = new global::Decorator1(a: iA_0_2);
        b_0_4 = new global::B();
        iA_0_0 = new global::Decorator2(a: iA_0_1, b: b_0_4);
        return new global::StrongInject.AsyncOwned<global::IA>(iA_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WrapsTypesInDecoratorsRegisteredByDecoratorFactory()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A), typeof(IA))]
[Register(typeof(B))]
public partial class Container : IAsyncContainer<IA>
{
    [DecoratorFactory]
    public IA Decorator1(IA a) => default;

    [DecoratorFactory]
    public IA Decorator2(IA a, B b) => default;
}

public interface IA {}
public class A : IA {}
public class B {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::IA iA_0_2;
        global::IA iA_0_1;
        global::B b_0_4;
        global::IA iA_0_0;
        a_0_3 = new global::A();
        iA_0_2 = (global::IA)a_0_3;
        iA_0_1 = this.Decorator1(a: iA_0_2);
        b_0_4 = new global::B();
        iA_0_0 = this.Decorator2(a: iA_0_1, b: b_0_4);
        TResult result;
        try
        {
            result = await func(iA_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::IA iA_0_2;
        global::IA iA_0_1;
        global::B b_0_4;
        global::IA iA_0_0;
        a_0_3 = new global::A();
        iA_0_2 = (global::IA)a_0_3;
        iA_0_1 = this.Decorator1(a: iA_0_2);
        b_0_4 = new global::B();
        iA_0_0 = this.Decorator2(a: iA_0_1, b: b_0_4);
        return new global::StrongInject.AsyncOwned<global::IA>(iA_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorIfDecoratorsHaveNoParametersOfDecoratedType()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A), typeof(IA))]
[Register(typeof(B))]
[RegisterDecorator(typeof(Decorator), typeof(IA))]
public partial class Container : IAsyncContainer<IA>
{
    [DecoratorFactory] IA Decorator(A a) => a;
}

public interface IA {}
public class A : IA {}
public class B {}
public class Decorator : IA
{
    public Decorator(Decorator d){} 
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,2): Error SI0022: Decorator 'Decorator' does not have a constructor parameter of decorated type 'IA'.
                // RegisterDecorator(typeof(Decorator), typeof(IA))
                new DiagnosticResult("SI0022", @"RegisterDecorator(typeof(Decorator), typeof(IA))", DiagnosticSeverity.Error).WithLocation(6, 2),
                // (9,6): Error SI0024: Decorator Factory 'Container.Decorator(A)' does not have a parameter of decorated type 'IA'.
                // DecoratorFactory
                new DiagnosticResult("SI0024", @"DecoratorFactory", DiagnosticSeverity.Error).WithLocation(9, 6));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_1;
        global::IA iA_0_0;
        a_0_1 = new global::A();
        iA_0_0 = (global::IA)a_0_1;
        TResult result;
        try
        {
            result = await func(iA_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_1;
        global::IA iA_0_0;
        a_0_1 = new global::A();
        iA_0_0 = (global::IA)a_0_1;
        return new global::StrongInject.AsyncOwned<global::IA>(iA_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorIfDecoratorsHaveMultipleParametersOfDecoratedType()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A), typeof(IA))]
[Register(typeof(B))]
[RegisterDecorator(typeof(Decorator), typeof(IA))]
public partial class Container : IAsyncContainer<IA>
{
    [DecoratorFactory] IA Decorator(IA a, IA b) => a;
}

public interface IA {}
public class A : IA {}
public class B {}
public class Decorator : IA
{
    public Decorator(IA a, B b, IA c){} 
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,2): Error SI0023: Decorator 'Decorator' has multiple constructor parameters of decorated type 'IA'.
                // RegisterDecorator(typeof(Decorator), typeof(IA))
                new DiagnosticResult("SI0023", @"RegisterDecorator(typeof(Decorator), typeof(IA))", DiagnosticSeverity.Error).WithLocation(6, 2),
                // (9,6): Error SI0025: Decorator Factory 'Container.Decorator(IA, IA)' has multiple constructor parameters of decorated type 'IA'.
                // DecoratorFactory
                new DiagnosticResult("SI0025", @"DecoratorFactory", DiagnosticSeverity.Error).WithLocation(9, 6));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_1;
        global::IA iA_0_0;
        a_0_1 = new global::A();
        iA_0_0 = (global::IA)a_0_1;
        TResult result;
        try
        {
            result = await func(iA_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_1;
        global::IA iA_0_0;
        a_0_1 = new global::A();
        iA_0_0 = (global::IA)a_0_1;
        return new global::StrongInject.AsyncOwned<global::IA>(iA_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WrapsTypesInDecoratorsRegisteredByGenericDecoratorFactory()
        {
            string userSource = @"
using StrongInject;
using System.Collections.Generic;

[Register(typeof(A), typeof(IA))]
[Register(typeof(B))]
public partial class Container : IAsyncContainer<List<IA>>
{
    [DecoratorFactory]
    public T Decorator1<T>(T t) => t;

    [DecoratorFactory]
    public List<T> Decorator2<T>(List<T> a, B b) => default;

    [DecoratorFactory]
    public T[] Decorator3<T>(T[] a) => default;

    [Factory]
    public List<T> ListFactory<T>(T[] a) => default;
}

public interface IA {}
public class A : IA {}
public class B {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Collections.Generic.List<global::IA>>.RunAsync<TResult, TParam>(global::System.Func<global::System.Collections.Generic.List<global::IA>, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_9;
        global::A a_0_8;
        global::IA iA_0_7;
        global::IA iA_0_6;
        global::IA[] _0_5;
        global::IA[] _0_4;
        global::IA[] _0_3;
        global::System.Collections.Generic.List<global::IA> list_0_2;
        global::B b_0_11;
        global::B b_0_10;
        global::System.Collections.Generic.List<global::IA> list_0_1;
        global::System.Collections.Generic.List<global::IA> list_0_0;
        a_0_9 = new global::A();
        a_0_8 = this.Decorator1<global::A>(t: a_0_9);
        iA_0_7 = (global::IA)a_0_8;
        iA_0_6 = this.Decorator1<global::IA>(t: iA_0_7);
        _0_5 = new global::IA[]{(global::IA)iA_0_6, };
        _0_4 = this.Decorator3<global::IA>(a: _0_5);
        _0_3 = this.Decorator1<global::IA[]>(t: _0_4);
        list_0_2 = this.ListFactory<global::IA>(a: _0_3);
        try
        {
            b_0_11 = new global::B();
            b_0_10 = this.Decorator1<global::B>(t: b_0_11);
            list_0_1 = this.Decorator2<global::IA>(a: list_0_2, b: b_0_10);
            list_0_0 = this.Decorator1<global::System.Collections.Generic.List<global::IA>>(t: list_0_1);
        }
        catch
        {
            await global::StrongInject.Helpers.DisposeAsync(list_0_2);
            throw;
        }

        TResult result;
        try
        {
            result = await func(list_0_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(list_0_2);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Collections.Generic.List<global::IA>>> global::StrongInject.IAsyncContainer<global::System.Collections.Generic.List<global::IA>>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_9;
        global::A a_0_8;
        global::IA iA_0_7;
        global::IA iA_0_6;
        global::IA[] _0_5;
        global::IA[] _0_4;
        global::IA[] _0_3;
        global::System.Collections.Generic.List<global::IA> list_0_2;
        global::B b_0_11;
        global::B b_0_10;
        global::System.Collections.Generic.List<global::IA> list_0_1;
        global::System.Collections.Generic.List<global::IA> list_0_0;
        a_0_9 = new global::A();
        a_0_8 = this.Decorator1<global::A>(t: a_0_9);
        iA_0_7 = (global::IA)a_0_8;
        iA_0_6 = this.Decorator1<global::IA>(t: iA_0_7);
        _0_5 = new global::IA[]{(global::IA)iA_0_6, };
        _0_4 = this.Decorator3<global::IA>(a: _0_5);
        _0_3 = this.Decorator1<global::IA[]>(t: _0_4);
        list_0_2 = this.ListFactory<global::IA>(a: _0_3);
        try
        {
            b_0_11 = new global::B();
            b_0_10 = this.Decorator1<global::B>(t: b_0_11);
            list_0_1 = this.Decorator2<global::IA>(a: list_0_2, b: b_0_10);
            list_0_0 = this.Decorator1<global::System.Collections.Generic.List<global::IA>>(t: list_0_1);
        }
        catch
        {
            await global::StrongInject.Helpers.DisposeAsync(list_0_2);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::System.Collections.Generic.List<global::IA>>(list_0_0, async () =>
        {
            await global::StrongInject.Helpers.DisposeAsync(list_0_2);
        });
    }
}");
        }

        [Fact]
        public void DoesNotDecorateDelegateParameters()
        {
            string userSource = @"
using StrongInject;
using System;

[RegisterDecorator(typeof(Decorator), typeof(IA))]
public partial class Container : IAsyncContainer<Func<IA, IA>>
{
}

public interface IA {}
public class Decorator : IA
{
    public Decorator(IA a){} 
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Warning SI1104: Warning while resolving dependencies for 'System.Func<IA, IA>': Return type 'IA' of delegate 'System.Func<IA, IA>' is provided as a parameter to the delegate and so will be returned unchanged.
                // Container
                new DiagnosticResult("SI1104", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Func<global::IA, global::IA>>.RunAsync<TResult, TParam>(global::System.Func<global::System.Func<global::IA, global::IA>, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::IA, global::IA> func_0_0;
        func_0_0 = (param0_0) =>
        {
            return param0_0;
        };
        TResult result;
        try
        {
            result = await func(func_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Func<global::IA, global::IA>>> global::StrongInject.IAsyncContainer<global::System.Func<global::IA, global::IA>>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::IA, global::IA> func_0_0;
        func_0_0 = (param0_0) =>
        {
            return param0_0;
        };
        return new global::StrongInject.AsyncOwned<global::System.Func<global::IA, global::IA>>(func_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DecoratesInstanceFieldOrProperties()
        {
            string userSource = @"
using StrongInject;

[RegisterDecorator(typeof(Decorator), typeof(IA))]
public partial class Container : IAsyncContainer<IA>
{
    [Instance] IA _ia;
}

public interface IA {}
public class Decorator : IA
{
    public Decorator(IA a){} 
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (7,19): Warning CS0649: Field 'Container._ia' is never assigned to, and will always have its default value null
                // _ia
                new DiagnosticResult("CS0649", @"_ia", DiagnosticSeverity.Warning).WithLocation(7, 19));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::IA _iAField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::IA GetIAField0()
    {
        if (!object.ReferenceEquals(_iAField0, null))
            return _iAField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::IA iA_0_1;
            global::IA iA_0_0;
            iA_0_1 = this._ia;
            iA_0_0 = new global::Decorator(a: iA_0_1);
            this._iAField0 = iA_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _iAField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::IA iA_0_0;
        iA_0_0 = GetIAField0();
        TResult result;
        try
        {
            result = await func(iA_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::IA iA_0_0;
        iA_0_0 = GetIAField0();
        return new global::StrongInject.AsyncOwned<global::IA>(iA_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DecoratesSingleInstanceDependencies()
        {
            string userSource = @"
using StrongInject;

[RegisterDecorator(typeof(Decorator), typeof(IA))]
public partial class Container : IAsyncContainer<IA>
{
    [Factory(Scope.SingleInstance)] IA GetIA() => default;
}

public interface IA {}
public class Decorator : IA
{
    public Decorator(IA a){} 
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }
    }

    private global::IA _iAField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::IA _iAField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private global::IA GetIAField1()
    {
        if (!object.ReferenceEquals(_iAField1, null))
            return _iAField1;
        this._lock1.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::IA iA_0_0;
            iA_0_0 = this.GetIA();
            this._iAField1 = iA_0_0;
            this._disposeAction1 = async () =>
            {
                await global::StrongInject.Helpers.DisposeAsync(iA_0_0);
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _iAField1;
    }

    private global::IA GetIAField0()
    {
        if (!object.ReferenceEquals(_iAField0, null))
            return _iAField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::IA iA_0_1;
            global::IA iA_0_0;
            iA_0_1 = GetIAField1();
            iA_0_0 = new global::Decorator(a: iA_0_1);
            this._iAField0 = iA_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _iAField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::IA iA_0_0;
        iA_0_0 = GetIAField0();
        TResult result;
        try
        {
            result = await func(iA_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::IA iA_0_0;
        iA_0_0 = GetIAField0();
        return new global::StrongInject.AsyncOwned<global::IA>(iA_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DeduplicatesMultipleRegistrationsOfSameDecorator()
        {
            string userSource = @"
using StrongInject;

public class Module1
{
    [DecoratorFactory] public static IA Decorator(IA a) => a;
    [DecoratorFactory] public static T Decorator<T>(T a) => a;
}

[RegisterModule(typeof(Module1))]
public class Module2
{
}

[RegisterModule(typeof(Module1))]
[RegisterModule(typeof(Module2))]
[Register(typeof(A), typeof(IA))]
[RegisterDecorator(typeof(Decorator), typeof(IA))]
public partial class Container : IAsyncContainer<IA>
{
}

public interface IA {}
public class A : IA {}
public class Decorator : IA
{
    public Decorator(IA a){} 
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_5;
        global::A a_0_4;
        global::IA iA_0_3;
        global::IA iA_0_2;
        global::IA iA_0_1;
        global::IA iA_0_0;
        a_0_5 = new global::A();
        a_0_4 = global::Module1.Decorator<global::A>(a: a_0_5);
        iA_0_3 = (global::IA)a_0_4;
        iA_0_2 = global::Module1.Decorator(a: iA_0_3);
        iA_0_1 = new global::Decorator(a: iA_0_2);
        iA_0_0 = global::Module1.Decorator<global::IA>(a: iA_0_1);
        TResult result;
        try
        {
            result = await func(iA_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_5;
        global::A a_0_4;
        global::IA iA_0_3;
        global::IA iA_0_2;
        global::IA iA_0_1;
        global::IA iA_0_0;
        a_0_5 = new global::A();
        a_0_4 = global::Module1.Decorator<global::A>(a: a_0_5);
        iA_0_3 = (global::IA)a_0_4;
        iA_0_2 = global::Module1.Decorator(a: iA_0_3);
        iA_0_1 = new global::Decorator(a: iA_0_2);
        iA_0_0 = global::Module1.Decorator<global::IA>(a: iA_0_1);
        return new global::StrongInject.AsyncOwned<global::IA>(iA_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WarnOnNonStaticPublidDecoratorInModule()
        {
            string userSource = @"
using StrongInject;

public class Module1
{
    [DecoratorFactory] static int Decorator(int a) => a;
    [DecoratorFactory] public T Decorator<T>(T a) => a;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Warning SI1002: Factory method 'Module1.Decorator(int)' is not either public and static, or protected, and containing module 'Module1' is not a container, so will be ignored.
                // DecoratorFactory
                new DiagnosticResult("SI1002", @"DecoratorFactory", DiagnosticSeverity.Warning).WithLocation(6, 6),
                // (7,6): Warning SI1002: Factory method 'Module1.Decorator<T>(T)' is not static, and containing module 'Module1' is not a container, so will be ignored.
                // DecoratorFactory
                new DiagnosticResult("SI1002", @"DecoratorFactory", DiagnosticSeverity.Warning).WithLocation(7, 6));
            comp.GetDiagnostics().Verify();
            Assert.Empty(generated);
        }

        [Fact]
        public void ErrorIfDecoratorParametersAreNotAvailable()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A), typeof(IA))]
public partial class Container : IAsyncContainer<IA>
{
    [DecoratorFactory] IA Decorator(IA a, B b) => a;
}

public interface IA {}
public class A : IA {}
public class B {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (5,22): Error SI0102: Error while resolving dependencies for 'IA': We have no source for instance of type 'B'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void DisposesDecoratorsRegisteredByAttributes()
        {
            string userSource = @"
using StrongInject;
using System;
using System.Threading.Tasks;

[Register(typeof(A), typeof(IA))]
[Register(typeof(B), typeof(IB))]
[RegisterDecorator(typeof(DecoratorA), typeof(IA))]
[RegisterDecorator(typeof(DecoratorB), typeof(IB))]
public partial class Container : IAsyncContainer<IA>, IContainer<IB>
{
}

public interface IA {}
public interface IB : IDisposable {}
public class A : IA {}
public class B : IB { public void Dispose(){} }
public class DecoratorA : IA, IAsyncDisposable
{
    public DecoratorA(IA a){}
    public ValueTask DisposeAsync() => default;
}
public class DecoratorB : IB
{
    public DecoratorB(IB b){}
    public void Dispose(){}
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    void global::System.IDisposable.Dispose()
    {
        throw new global::StrongInject.StrongInjectException(""This container requires async disposal"");
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::IA iA_0_1;
        global::IA iA_0_0;
        a_0_2 = new global::A();
        iA_0_1 = (global::IA)a_0_2;
        iA_0_0 = new global::DecoratorA(a: iA_0_1);
        TResult result;
        try
        {
            result = await func(iA_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::IA iA_0_1;
        global::IA iA_0_0;
        a_0_2 = new global::A();
        iA_0_1 = (global::IA)a_0_2;
        iA_0_0 = new global::DecoratorA(a: iA_0_1);
        return new global::StrongInject.AsyncOwned<global::IA>(iA_0_0, async () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::IB>.Run<TResult, TParam>(global::System.Func<global::IB, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_2;
        global::IB iB_0_1;
        global::IB iB_0_0;
        b_0_2 = new global::B();
        try
        {
            iB_0_1 = (global::IB)b_0_2;
            iB_0_0 = new global::DecoratorB(b: iB_0_1);
        }
        catch
        {
            ((global::System.IDisposable)b_0_2).Dispose();
            throw;
        }

        TResult result;
        try
        {
            result = func(iB_0_0, param);
        }
        finally
        {
            ((global::System.IDisposable)b_0_2).Dispose();
        }

        return result;
    }

    global::StrongInject.Owned<global::IB> global::StrongInject.IContainer<global::IB>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_2;
        global::IB iB_0_1;
        global::IB iB_0_0;
        b_0_2 = new global::B();
        try
        {
            iB_0_1 = (global::IB)b_0_2;
            iB_0_0 = new global::DecoratorB(b: iB_0_1);
        }
        catch
        {
            ((global::System.IDisposable)b_0_2).Dispose();
            throw;
        }

        return new global::StrongInject.Owned<global::IB>(iB_0_0, () =>
        {
            ((global::System.IDisposable)b_0_2).Dispose();
        });
    }
}");
        }

        [Fact]
        public void InitializeDecoratorsRegisteredByAttributes()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A), typeof(IA))]
[Register(typeof(B), typeof(IB))]
[RegisterDecorator(typeof(DecoratorA), typeof(IA))]
[RegisterDecorator(typeof(DecoratorB), typeof(IB))]
public partial class Container : IAsyncContainer<IA>, IContainer<IB>
{
}

public interface IA {}
public interface IB {}
public class A : IA {}
public class B : IB {}
public class DecoratorA : IA, IRequiresAsyncInitialization
{
    public DecoratorA(IA a){}
    public ValueTask InitializeAsync() => default;
}
public class DecoratorB : IB, IRequiresInitialization
{
    public DecoratorB(IB b){}
    public void Initialize(){}
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    void global::System.IDisposable.Dispose()
    {
        throw new global::StrongInject.StrongInjectException(""This container requires async disposal"");
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::IA iA_0_1;
        global::IA iA_0_0;
        global::System.Threading.Tasks.ValueTask iA_0_3;
        var hasAwaitStarted_iA_0_3 = false;
        a_0_2 = new global::A();
        iA_0_1 = (global::IA)a_0_2;
        iA_0_0 = new global::DecoratorA(a: iA_0_1);
        iA_0_3 = ((global::StrongInject.IRequiresAsyncInitialization)iA_0_0).InitializeAsync();
        try
        {
            hasAwaitStarted_iA_0_3 = true;
            await iA_0_3;
        }
        catch
        {
            if (!hasAwaitStarted_iA_0_3)
            {
                _ = iA_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(iA_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::IA iA_0_1;
        global::IA iA_0_0;
        global::System.Threading.Tasks.ValueTask iA_0_3;
        var hasAwaitStarted_iA_0_3 = false;
        a_0_2 = new global::A();
        iA_0_1 = (global::IA)a_0_2;
        iA_0_0 = new global::DecoratorA(a: iA_0_1);
        iA_0_3 = ((global::StrongInject.IRequiresAsyncInitialization)iA_0_0).InitializeAsync();
        try
        {
            hasAwaitStarted_iA_0_3 = true;
            await iA_0_3;
        }
        catch
        {
            if (!hasAwaitStarted_iA_0_3)
            {
                _ = iA_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::IA>(iA_0_0, async () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::IB>.Run<TResult, TParam>(global::System.Func<global::IB, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_2;
        global::IB iB_0_1;
        global::IB iB_0_0;
        b_0_2 = new global::B();
        iB_0_1 = (global::IB)b_0_2;
        iB_0_0 = new global::DecoratorB(b: iB_0_1);
        ((global::StrongInject.IRequiresInitialization)iB_0_0).Initialize();
        TResult result;
        try
        {
            result = func(iB_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::IB> global::StrongInject.IContainer<global::IB>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_2;
        global::IB iB_0_1;
        global::IB iB_0_0;
        b_0_2 = new global::B();
        iB_0_1 = (global::IB)b_0_2;
        iB_0_0 = new global::DecoratorB(b: iB_0_1);
        ((global::StrongInject.IRequiresInitialization)iB_0_0).Initialize();
        return new global::StrongInject.Owned<global::IB>(iB_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void TestAsyncDecorators()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A))]
public partial class Container : IAsyncContainer<A>
{
    [DecoratorFactory]
    public ValueTask<A> Decorator(A a) => default;
}

public class A{}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_1;
        global::System.Threading.Tasks.ValueTask<global::A> a_0_2;
        var hasAwaitStarted_a_0_2 = false;
        var a_0_0 = default(global::A);
        a_0_1 = new global::A();
        a_0_2 = this.Decorator(a: a_0_1);
        try
        {
            hasAwaitStarted_a_0_2 = true;
            a_0_0 = await a_0_2;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_2)
            {
                _ = a_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_1;
        global::System.Threading.Tasks.ValueTask<global::A> a_0_2;
        var hasAwaitStarted_a_0_2 = false;
        var a_0_0 = default(global::A);
        a_0_1 = new global::A();
        a_0_2 = this.Decorator(a: a_0_1);
        try
        {
            hasAwaitStarted_a_0_2 = true;
            a_0_0 = await a_0_2;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_2)
            {
                _ = a_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void TestAsyncGenericDecorators()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

[Register(typeof(A))]
public partial class Container : IAsyncContainer<A>
{
    [DecoratorFactory]
    public async ValueTask<T> Decorator<T>(T t) where T : INeedsInitialization { await t.Initialize(); return t; }
}

public interface INeedsInitialization { ValueTask Initialize(); }
public class A : INeedsInitialization { public ValueTask Initialize() => default; }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::A>.RunAsync<TResult, TParam>(global::System.Func<global::A, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_1;
        global::System.Threading.Tasks.ValueTask<global::A> a_0_2;
        var hasAwaitStarted_a_0_2 = false;
        var a_0_0 = default(global::A);
        a_0_1 = new global::A();
        a_0_2 = this.Decorator<global::A>(t: a_0_1);
        try
        {
            hasAwaitStarted_a_0_2 = true;
            a_0_0 = await a_0_2;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_2)
            {
                _ = a_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::A>> global::StrongInject.IAsyncContainer<global::A>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_1;
        global::System.Threading.Tasks.ValueTask<global::A> a_0_2;
        var hasAwaitStarted_a_0_2 = false;
        var a_0_0 = default(global::A);
        a_0_1 = new global::A();
        a_0_2 = this.Decorator<global::A>(t: a_0_1);
        try
        {
            hasAwaitStarted_a_0_2 = true;
            a_0_0 = await a_0_2;
        }
        catch
        {
            if (!hasAwaitStarted_a_0_2)
            {
                _ = a_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::A>(a_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DisposesDecoratorsWithDisposeOptionsButNotThoseWithDefaultOptions()
        {
            string userSource = @"
using StrongInject;
using System;

[Register(typeof(A), typeof(IA))]
[RegisterDecorator(typeof(Decorator1), typeof(IA), DecoratorOptions.Dispose)]
[RegisterDecorator(typeof(Decorator2), typeof(IA), DecoratorOptions.Default)]
public partial class Container : IAsyncContainer<IA>
{
    [DecoratorFactory(DecoratorOptions.Dispose)] IA Decorator1(IA a) => a;
    [DecoratorFactory(DecoratorOptions.Dispose)] T Decorator1<T>(T t) => t;
    [DecoratorFactory(DecoratorOptions.Default)] IA Decorator2(IA a) => a;
    [DecoratorFactory(DecoratorOptions.Default)] T Decorator2<T>(T t) => t;
}

public interface IA  {}
public class A : IA {}
public class Decorator1 : IA, IDisposable
{
    public Decorator1(IA a){} 
    public void Dispose(){} 
}
public class Decorator2 : IA, IDisposable
{
    public Decorator2(IA a){} 
    public void Dispose(){} 
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::IA>.RunAsync<TResult, TParam>(global::System.Func<global::IA, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_9;
        global::A a_0_8;
        global::A a_0_7;
        global::IA iA_0_6;
        global::IA iA_0_5;
        global::IA iA_0_4;
        global::IA iA_0_3;
        global::IA iA_0_2;
        global::IA iA_0_1;
        global::IA iA_0_0;
        a_0_9 = new global::A();
        a_0_8 = this.Decorator1<global::A>(t: a_0_9);
        try
        {
            a_0_7 = this.Decorator2<global::A>(t: a_0_8);
            iA_0_6 = (global::IA)a_0_7;
            iA_0_5 = new global::Decorator1(a: iA_0_6);
            try
            {
                iA_0_4 = new global::Decorator2(a: iA_0_5);
                iA_0_3 = this.Decorator1(a: iA_0_4);
                try
                {
                    iA_0_2 = this.Decorator2(a: iA_0_3);
                    iA_0_1 = this.Decorator1<global::IA>(t: iA_0_2);
                    try
                    {
                        iA_0_0 = this.Decorator2<global::IA>(t: iA_0_1);
                    }
                    catch
                    {
                        await global::StrongInject.Helpers.DisposeAsync(iA_0_1);
                        throw;
                    }
                }
                catch
                {
                    await global::StrongInject.Helpers.DisposeAsync(iA_0_3);
                    throw;
                }
            }
            catch
            {
                ((global::System.IDisposable)iA_0_5).Dispose();
                throw;
            }
        }
        catch
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_8);
            throw;
        }

        TResult result;
        try
        {
            result = await func(iA_0_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(iA_0_1);
            await global::StrongInject.Helpers.DisposeAsync(iA_0_3);
            ((global::System.IDisposable)iA_0_5).Dispose();
            await global::StrongInject.Helpers.DisposeAsync(a_0_8);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::IA>> global::StrongInject.IAsyncContainer<global::IA>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_9;
        global::A a_0_8;
        global::A a_0_7;
        global::IA iA_0_6;
        global::IA iA_0_5;
        global::IA iA_0_4;
        global::IA iA_0_3;
        global::IA iA_0_2;
        global::IA iA_0_1;
        global::IA iA_0_0;
        a_0_9 = new global::A();
        a_0_8 = this.Decorator1<global::A>(t: a_0_9);
        try
        {
            a_0_7 = this.Decorator2<global::A>(t: a_0_8);
            iA_0_6 = (global::IA)a_0_7;
            iA_0_5 = new global::Decorator1(a: iA_0_6);
            try
            {
                iA_0_4 = new global::Decorator2(a: iA_0_5);
                iA_0_3 = this.Decorator1(a: iA_0_4);
                try
                {
                    iA_0_2 = this.Decorator2(a: iA_0_3);
                    iA_0_1 = this.Decorator1<global::IA>(t: iA_0_2);
                    try
                    {
                        iA_0_0 = this.Decorator2<global::IA>(t: iA_0_1);
                    }
                    catch
                    {
                        await global::StrongInject.Helpers.DisposeAsync(iA_0_1);
                        throw;
                    }
                }
                catch
                {
                    await global::StrongInject.Helpers.DisposeAsync(iA_0_3);
                    throw;
                }
            }
            catch
            {
                ((global::System.IDisposable)iA_0_5).Dispose();
                throw;
            }
        }
        catch
        {
            await global::StrongInject.Helpers.DisposeAsync(a_0_8);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::IA>(iA_0_0, async () =>
        {
            await global::StrongInject.Helpers.DisposeAsync(iA_0_1);
            await global::StrongInject.Helpers.DisposeAsync(iA_0_3);
            ((global::System.IDisposable)iA_0_5).Dispose();
            await global::StrongInject.Helpers.DisposeAsync(a_0_8);
        });
    }
}");
        }

        [Fact]
        public void InstanceWithAsImplementedInterfacesIsRegisteredAsImplementedInterfacesButNotAsFactoriesOrBaseClasses()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<B>, IContainer<A>, IContainer<I3>, IContainer<I2>, IContainer<I1>, IContainer<IFactory<int>>, IContainer<int>
{
    [Instance(Options.AsImplementedInterfaces)] B _b;
}

interface I1 {}
interface I2 {}
interface I3 : I2 {}
public class A : I1 {}
public class B : A, I3, IFactory<int> {
    public int Create() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,22): Error SI0102: Error while resolving dependencies for 'A': We have no source for instance of type 'A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'int': We have no source for instance of type 'int'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22));
            comp.GetDiagnostics().Verify(
                // (6,51): Warning CS0649: Field 'Container._b' is never assigned to, and will always have its default value null
                // _b
                new DiagnosticResult("CS0649", @"_b", DiagnosticSeverity.Warning).WithLocation(6, 51));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_0;
        b_0_0 = this._b;
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_0;
        b_0_0 = this._b;
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::I3>.Run<TResult, TParam>(global::System.Func<global::I3, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::I3 i3_0_0;
        b_0_1 = this._b;
        i3_0_0 = (global::I3)b_0_1;
        TResult result;
        try
        {
            result = func(i3_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::I3> global::StrongInject.IContainer<global::I3>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::I3 i3_0_0;
        b_0_1 = this._b;
        i3_0_0 = (global::I3)b_0_1;
        return new global::StrongInject.Owned<global::I3>(i3_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::I2>.Run<TResult, TParam>(global::System.Func<global::I2, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::I2 i2_0_0;
        b_0_1 = this._b;
        i2_0_0 = (global::I2)b_0_1;
        TResult result;
        try
        {
            result = func(i2_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::I2> global::StrongInject.IContainer<global::I2>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::I2 i2_0_0;
        b_0_1 = this._b;
        i2_0_0 = (global::I2)b_0_1;
        return new global::StrongInject.Owned<global::I2>(i2_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::I1>.Run<TResult, TParam>(global::System.Func<global::I1, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::I1 i1_0_0;
        b_0_1 = this._b;
        i1_0_0 = (global::I1)b_0_1;
        TResult result;
        try
        {
            result = func(i1_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::I1> global::StrongInject.IContainer<global::I1>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::I1 i1_0_0;
        b_0_1 = this._b;
        i1_0_0 = (global::I1)b_0_1;
        return new global::StrongInject.Owned<global::I1>(i1_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IFactory<global::System.Int32>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IFactory<global::System.Int32>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::StrongInject.IFactory<global::System.Int32> iFactory_0_0;
        b_0_1 = this._b;
        iFactory_0_0 = (global::StrongInject.IFactory<global::System.Int32>)b_0_1;
        TResult result;
        try
        {
            result = func(iFactory_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IFactory<global::System.Int32>> global::StrongInject.IContainer<global::StrongInject.IFactory<global::System.Int32>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::StrongInject.IFactory<global::System.Int32> iFactory_0_0;
        b_0_1 = this._b;
        iFactory_0_0 = (global::StrongInject.IFactory<global::System.Int32>)b_0_1;
        return new global::StrongInject.Owned<global::StrongInject.IFactory<global::System.Int32>>(iFactory_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void InstanceWithAsBaseClassesIsRegisteredAsBaseClassesButNotAsImplementedInterfacesOrFactories()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<C>, IContainer<B>, IContainer<A>, IContainer<IFactory<int>>, IContainer<int>
{
    [Instance(Options.AsBaseClasses)] C _c;
}

public class A {}
public class B : A {}
public class C : B, IFactory<int> {
    public int Create() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,22): Error SI0102: Error while resolving dependencies for 'StrongInject.IFactory<int>': We have no source for instance of type 'StrongInject.IFactory<int>'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22),
                // (4,22): Error SI0102: Error while resolving dependencies for 'int': We have no source for instance of type 'int'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22));
            comp.GetDiagnostics().Verify(
                // (6,41): Warning CS0649: Field 'Container._c' is never assigned to, and will always have its default value null
                // _c
                new DiagnosticResult("CS0649", @"_c", DiagnosticSeverity.Warning).WithLocation(6, 41));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::C>.Run<TResult, TParam>(global::System.Func<global::C, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_0;
        c_0_0 = this._c;
        TResult result;
        try
        {
            result = func(c_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::C> global::StrongInject.IContainer<global::C>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_0;
        c_0_0 = this._c;
        return new global::StrongInject.Owned<global::C>(c_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::B b_0_0;
        c_0_1 = this._c;
        b_0_0 = (global::B)c_0_1;
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::B b_0_0;
        c_0_1 = this._c;
        b_0_0 = (global::B)c_0_1;
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::A a_0_0;
        c_0_1 = this._c;
        a_0_0 = (global::A)c_0_1;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::A a_0_0;
        c_0_1 = this._c;
        a_0_0 = (global::A)c_0_1;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IFactory<global::System.Int32>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IFactory<global::System.Int32>, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::StrongInject.IFactory<global::System.Int32>> global::StrongInject.IContainer<global::StrongInject.IFactory<global::System.Int32>>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void InstanceWithAsBaseClassesIsNotRegisteredAsObject()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<object>
{
    [Instance(Options.AsBaseClasses)] A _a;
}

public class A {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,22): Error SI0102: Error while resolving dependencies for 'object': We have no source for instance of type 'object'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22));
            comp.GetDiagnostics().Verify(
                // (6,41): Warning CS0169: The field 'Container._a' is never used
                // _a
                new DiagnosticResult("CS0169", @"_a", DiagnosticSeverity.Warning).WithLocation(6, 41));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Object>.Run<TResult, TParam>(global::System.Func<global::System.Object, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Object> global::StrongInject.IContainer<global::System.Object>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void InstanceWithUseAsFactoryIsRegisteredAsFactoriesButFactoryTargetIsntUsedAsFactory()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<IFactory<IFactory<int>>>, IContainer<IFactory<int>>, IContainer<int>
{
    [Instance(Options.UseAsFactory)] IFactory<IFactory<int>> _fac;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,22): Error SI0102: Error while resolving dependencies for 'int': We have no source for instance of type 'int'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(4, 22));
            comp.GetDiagnostics().Verify(
                // (6,62): Warning CS0649: Field 'Container._fac' is never assigned to, and will always have its default value null
                // _fac
                new DiagnosticResult("CS0649", @"_fac", DiagnosticSeverity.Warning).WithLocation(6, 62));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IFactory<global::StrongInject.IFactory<global::System.Int32>>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IFactory<global::StrongInject.IFactory<global::System.Int32>>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::StrongInject.IFactory<global::System.Int32>> iFactory_0_0;
        iFactory_0_0 = this._fac;
        TResult result;
        try
        {
            result = func(iFactory_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IFactory<global::StrongInject.IFactory<global::System.Int32>>> global::StrongInject.IContainer<global::StrongInject.IFactory<global::StrongInject.IFactory<global::System.Int32>>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::StrongInject.IFactory<global::System.Int32>> iFactory_0_0;
        iFactory_0_0 = this._fac;
        return new global::StrongInject.Owned<global::StrongInject.IFactory<global::StrongInject.IFactory<global::System.Int32>>>(iFactory_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IFactory<global::System.Int32>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IFactory<global::System.Int32>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::StrongInject.IFactory<global::System.Int32>> iFactory_0_1;
        global::StrongInject.IFactory<global::System.Int32> iFactory_0_0;
        iFactory_0_1 = this._fac;
        iFactory_0_0 = iFactory_0_1.Create();
        TResult result;
        try
        {
            result = func(iFactory_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(iFactory_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IFactory<global::System.Int32>> global::StrongInject.IContainer<global::StrongInject.IFactory<global::System.Int32>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::StrongInject.IFactory<global::System.Int32>> iFactory_0_1;
        global::StrongInject.IFactory<global::System.Int32> iFactory_0_0;
        iFactory_0_1 = this._fac;
        iFactory_0_0 = iFactory_0_1.Create();
        return new global::StrongInject.Owned<global::StrongInject.IFactory<global::System.Int32>>(iFactory_0_0, () =>
        {
            iFactory_0_1.Release(iFactory_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void InstanceWithAsEverythingPossibleRegistersEverythingForAllFactoryTargetsRecursively()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IContainer<A>, IContainer<B>, IAsyncContainer<C>, IContainer<D>, IAsyncContainer<E>, IAsyncContainer<I>, IContainer<IAsyncFactory<C>>
{
    [Instance(Options.AsEverythingPossible)] A _a;
}

public class A : IFactory<B> { public B Create() => default; }
public class B : IAsyncFactory<C> { public ValueTask<C> CreateAsync() => default; }
public class C : IFactory<D> { public D Create() => default; }
public class D : E {}
public class E : I {}
public interface I {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (5,22): Error SI0103: Error while resolving dependencies for 'D': 'C' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22));
            comp.GetDiagnostics().Verify(
                // (7,48): Warning CS0649: Field 'Container._a' is never assigned to, and will always have its default value null
                // _a
                new DiagnosticResult("CS0649", @"_a", DiagnosticSeverity.Warning).WithLocation(7, 48));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    void global::System.IDisposable.Dispose()
    {
        throw new global::StrongInject.StrongInjectException(""This container requires async disposal"");
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::StrongInject.IFactory<global::B> iFactory_0_1;
        global::B b_0_0;
        a_0_2 = this._a;
        iFactory_0_1 = (global::StrongInject.IFactory<global::B>)a_0_2;
        b_0_0 = iFactory_0_1.Create();
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(b_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::StrongInject.IFactory<global::B> iFactory_0_1;
        global::B b_0_0;
        a_0_2 = this._a;
        iFactory_0_1 = (global::StrongInject.IFactory<global::B>)a_0_2;
        b_0_0 = iFactory_0_1.Create();
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
            iFactory_0_1.Release(b_0_0);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::C>.RunAsync<TResult, TParam>(global::System.Func<global::C, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_4;
        global::StrongInject.IFactory<global::B> iFactory_0_3;
        global::B b_0_2;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_5;
        var hasAwaitStarted_c_0_5 = false;
        var c_0_0 = default(global::C);
        var hasAwaitCompleted_c_0_5 = false;
        a_0_4 = this._a;
        iFactory_0_3 = (global::StrongInject.IFactory<global::B>)a_0_4;
        b_0_2 = iFactory_0_3.Create();
        try
        {
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            c_0_5 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_5 = true;
                c_0_0 = await c_0_5;
                hasAwaitCompleted_c_0_5 = true;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_5)
                {
                    c_0_0 = await c_0_5;
                }
                else if (!hasAwaitCompleted_c_0_5)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
                throw;
            }
        }
        catch
        {
            iFactory_0_3.Release(b_0_2);
            throw;
        }

        TResult result;
        try
        {
            result = await func(c_0_0, param);
        }
        finally
        {
            await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
            iFactory_0_3.Release(b_0_2);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::C>> global::StrongInject.IAsyncContainer<global::C>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_4;
        global::StrongInject.IFactory<global::B> iFactory_0_3;
        global::B b_0_2;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_5;
        var hasAwaitStarted_c_0_5 = false;
        var c_0_0 = default(global::C);
        var hasAwaitCompleted_c_0_5 = false;
        a_0_4 = this._a;
        iFactory_0_3 = (global::StrongInject.IFactory<global::B>)a_0_4;
        b_0_2 = iFactory_0_3.Create();
        try
        {
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            c_0_5 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_5 = true;
                c_0_0 = await c_0_5;
                hasAwaitCompleted_c_0_5 = true;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_5)
                {
                    c_0_0 = await c_0_5;
                }
                else if (!hasAwaitCompleted_c_0_5)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
                throw;
            }
        }
        catch
        {
            iFactory_0_3.Release(b_0_2);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::C>(c_0_0, async () =>
        {
            await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
            iFactory_0_3.Release(b_0_2);
        });
    }

    TResult global::StrongInject.IContainer<global::D>.Run<TResult, TParam>(global::System.Func<global::D, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::D> global::StrongInject.IContainer<global::D>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::E>.RunAsync<TResult, TParam>(global::System.Func<global::E, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::E e_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    e_0_0 = (global::E)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        TResult result;
        try
        {
            result = await func(e_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::E>> global::StrongInject.IAsyncContainer<global::E>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::E e_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    e_0_0 = (global::E)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::E>(e_0_0, async () =>
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::I>.RunAsync<TResult, TParam>(global::System.Func<global::I, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::I i_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    i_0_0 = (global::I)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        TResult result;
        try
        {
            result = await func(i_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::I>> global::StrongInject.IAsyncContainer<global::I>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::I i_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    i_0_0 = (global::I)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::I>(i_0_0, async () =>
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IAsyncFactory<global::C>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::StrongInject.IFactory<global::B> iFactory_0_2;
        global::B b_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        a_0_3 = this._a;
        iFactory_0_2 = (global::StrongInject.IFactory<global::B>)a_0_3;
        b_0_1 = iFactory_0_2.Create();
        try
        {
            iAsyncFactory_0_0 = (global::StrongInject.IAsyncFactory<global::C>)b_0_1;
        }
        catch
        {
            iFactory_0_2.Release(b_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(iAsyncFactory_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(b_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>> global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::StrongInject.IFactory<global::B> iFactory_0_2;
        global::B b_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        a_0_3 = this._a;
        iFactory_0_2 = (global::StrongInject.IFactory<global::B>)a_0_3;
        b_0_1 = iFactory_0_2.Create();
        try
        {
            iAsyncFactory_0_0 = (global::StrongInject.IAsyncFactory<global::C>)b_0_1;
        }
        catch
        {
            iFactory_0_2.Release(b_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>>(iAsyncFactory_0_0, () =>
        {
            iFactory_0_2.Release(b_0_1);
        });
    }
}");
        }

        [Fact]
        public void InstanceWithAsEverythingPossibleDoesNotStackOverflowOnRecursion1()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<A>, IContainer<IFactory<A>>
{
    [Instance(Options.AsEverythingPossible)] A _a;
}

public class A : IFactory<A> { public A Create() => default; }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (6,48): Warning CS0649: Field 'Container._a' is never assigned to, and will always have its default value null
                // _a
                new DiagnosticResult("CS0649", @"_a", DiagnosticSeverity.Warning).WithLocation(6, 48));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IFactory<global::A>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IFactory<global::A>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_1;
        global::StrongInject.IFactory<global::A> iFactory_0_0;
        a_0_1 = this._a;
        iFactory_0_0 = (global::StrongInject.IFactory<global::A>)a_0_1;
        TResult result;
        try
        {
            result = func(iFactory_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IFactory<global::A>> global::StrongInject.IContainer<global::StrongInject.IFactory<global::A>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_1;
        global::StrongInject.IFactory<global::A> iFactory_0_0;
        a_0_1 = this._a;
        iFactory_0_0 = (global::StrongInject.IFactory<global::A>)a_0_1;
        return new global::StrongInject.Owned<global::StrongInject.IFactory<global::A>>(iFactory_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void InstanceWithAsEverythingPossibleDoesNotStackOverflowOnRecursion2()
        {
            string userSource = @"
using StrongInject;

public partial class Container : IContainer<A<int>>, IContainer<IFactory<A<A<int>>>>, IContainer<A<A<int>>>, IContainer<IFactory<A<A<A<int>>>>>, IContainer<A<A<A<int>>>>
{
    [Instance(Options.AsEverythingPossible)] A<int> _a;
}

public class A<T> : IFactory<A<A<T>>> { public A<A<T>> Create() => default; }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (6,53): Warning CS0649: Field 'Container._a' is never assigned to, and will always have its default value null
                // _a
                new DiagnosticResult("CS0649", @"_a", DiagnosticSeverity.Warning).WithLocation(6, 53));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A<global::System.Int32>>.Run<TResult, TParam>(global::System.Func<global::A<global::System.Int32>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_0;
        a_0_0 = this._a;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A<global::System.Int32>> global::StrongInject.IContainer<global::A<global::System.Int32>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_0;
        a_0_0 = this._a;
        return new global::StrongInject.Owned<global::A<global::System.Int32>>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_1;
        global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>> iFactory_0_0;
        a_0_1 = this._a;
        iFactory_0_0 = (global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>)a_0_1;
        TResult result;
        try
        {
            result = func(iFactory_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>> global::StrongInject.IContainer<global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_1;
        global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>> iFactory_0_0;
        a_0_1 = this._a;
        iFactory_0_0 = (global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>)a_0_1;
        return new global::StrongInject.Owned<global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>>(iFactory_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::A<global::A<global::System.Int32>>>.Run<TResult, TParam>(global::System.Func<global::A<global::A<global::System.Int32>>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_2;
        global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>> iFactory_0_1;
        global::A<global::A<global::System.Int32>> a_0_0;
        a_0_2 = this._a;
        iFactory_0_1 = (global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>)a_0_2;
        a_0_0 = iFactory_0_1.Create();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(a_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::A<global::A<global::System.Int32>>> global::StrongInject.IContainer<global::A<global::A<global::System.Int32>>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_2;
        global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>> iFactory_0_1;
        global::A<global::A<global::System.Int32>> a_0_0;
        a_0_2 = this._a;
        iFactory_0_1 = (global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>)a_0_2;
        a_0_0 = iFactory_0_1.Create();
        return new global::StrongInject.Owned<global::A<global::A<global::System.Int32>>>(a_0_0, () =>
        {
            iFactory_0_1.Release(a_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_3;
        global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>> iFactory_0_2;
        global::A<global::A<global::System.Int32>> a_0_1;
        global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>> iFactory_0_0;
        a_0_3 = this._a;
        iFactory_0_2 = (global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>)a_0_3;
        a_0_1 = iFactory_0_2.Create();
        try
        {
            iFactory_0_0 = (global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>>)a_0_1;
        }
        catch
        {
            iFactory_0_2.Release(a_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(iFactory_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(a_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>>> global::StrongInject.IContainer<global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_3;
        global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>> iFactory_0_2;
        global::A<global::A<global::System.Int32>> a_0_1;
        global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>> iFactory_0_0;
        a_0_3 = this._a;
        iFactory_0_2 = (global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>)a_0_3;
        a_0_1 = iFactory_0_2.Create();
        try
        {
            iFactory_0_0 = (global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>>)a_0_1;
        }
        catch
        {
            iFactory_0_2.Release(a_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>>>(iFactory_0_0, () =>
        {
            iFactory_0_2.Release(a_0_1);
        });
    }

    TResult global::StrongInject.IContainer<global::A<global::A<global::A<global::System.Int32>>>>.Run<TResult, TParam>(global::System.Func<global::A<global::A<global::A<global::System.Int32>>>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_4;
        global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>> iFactory_0_3;
        global::A<global::A<global::System.Int32>> a_0_2;
        global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>> iFactory_0_1;
        global::A<global::A<global::A<global::System.Int32>>> a_0_0;
        a_0_4 = this._a;
        iFactory_0_3 = (global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>)a_0_4;
        a_0_2 = iFactory_0_3.Create();
        try
        {
            iFactory_0_1 = (global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>>)a_0_2;
            a_0_0 = iFactory_0_1.Create();
        }
        catch
        {
            iFactory_0_3.Release(a_0_2);
            throw;
        }

        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(a_0_0);
            iFactory_0_3.Release(a_0_2);
        }

        return result;
    }

    global::StrongInject.Owned<global::A<global::A<global::A<global::System.Int32>>>> global::StrongInject.IContainer<global::A<global::A<global::A<global::System.Int32>>>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A<global::System.Int32> a_0_4;
        global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>> iFactory_0_3;
        global::A<global::A<global::System.Int32>> a_0_2;
        global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>> iFactory_0_1;
        global::A<global::A<global::A<global::System.Int32>>> a_0_0;
        a_0_4 = this._a;
        iFactory_0_3 = (global::StrongInject.IFactory<global::A<global::A<global::System.Int32>>>)a_0_4;
        a_0_2 = iFactory_0_3.Create();
        try
        {
            iFactory_0_1 = (global::StrongInject.IFactory<global::A<global::A<global::A<global::System.Int32>>>>)a_0_2;
            a_0_0 = iFactory_0_1.Create();
        }
        catch
        {
            iFactory_0_3.Release(a_0_2);
            throw;
        }

        return new global::StrongInject.Owned<global::A<global::A<global::A<global::System.Int32>>>>(a_0_0, () =>
        {
            iFactory_0_1.Release(a_0_0);
            iFactory_0_3.Release(a_0_2);
        });
    }
}");
        }

        [Fact]
        public void InstanceWithAsEverythingPossibleCanBeDecorated()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IContainer<A>, IContainer<B>, IAsyncContainer<C>, IContainer<D>, IAsyncContainer<E>, IAsyncContainer<I>, IContainer<IAsyncFactory<C>>
{
    [Instance(Options.AsEverythingPossible)] A _a;
    [DecoratorFactory] T M<T>(T t) => t;
}

public class A : IFactory<B> { public B Create() => default; }
public class B : IAsyncFactory<C> { public ValueTask<C> CreateAsync() => default; }
public class C : IFactory<D> { public D Create() => default; }
public class D : E {}
public class E : I {}
public interface I {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (5,22): Error SI0103: Error while resolving dependencies for 'D': 'C' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22));
            comp.GetDiagnostics().Verify(
                // (7,48): Warning CS0649: Field 'Container._a' is never assigned to, and will always have its default value null
                // _a
                new DiagnosticResult("CS0649", @"_a", DiagnosticSeverity.Warning).WithLocation(7, 48));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }

        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    void global::System.IDisposable.Dispose()
    {
        throw new global::StrongInject.StrongInjectException(""This container requires async disposal"");
    }

    private global::A _aField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::A GetAField0()
    {
        if (!object.ReferenceEquals(_aField0, null))
            return _aField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::A a_0_1;
            global::A a_0_0;
            a_0_1 = this._a;
            a_0_0 = this.M<global::A>(t: a_0_1);
            this._aField0 = a_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _aField0;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = GetAField0();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = GetAField0();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    private global::StrongInject.IFactory<global::B> _iFactoryField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private global::StrongInject.IFactory<global::B> GetIFactoryField1()
    {
        if (!object.ReferenceEquals(_iFactoryField1, null))
            return _iFactoryField1;
        this._lock1.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::A a_0_2;
            global::StrongInject.IFactory<global::B> iFactory_0_1;
            global::StrongInject.IFactory<global::B> iFactory_0_0;
            a_0_2 = GetAField0();
            iFactory_0_1 = (global::StrongInject.IFactory<global::B>)a_0_2;
            iFactory_0_0 = this.M<global::StrongInject.IFactory<global::B>>(t: iFactory_0_1);
            this._iFactoryField1 = iFactory_0_0;
            this._disposeAction1 = async () =>
            {
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _iFactoryField1;
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::B> iFactory_0_2;
        global::B b_0_1;
        global::B b_0_0;
        iFactory_0_2 = GetIFactoryField1();
        b_0_1 = iFactory_0_2.Create();
        try
        {
            b_0_0 = this.M<global::B>(t: b_0_1);
        }
        catch
        {
            iFactory_0_2.Release(b_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(b_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::B> iFactory_0_2;
        global::B b_0_1;
        global::B b_0_0;
        iFactory_0_2 = GetIFactoryField1();
        b_0_1 = iFactory_0_2.Create();
        try
        {
            b_0_0 = this.M<global::B>(t: b_0_1);
        }
        catch
        {
            iFactory_0_2.Release(b_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
            iFactory_0_2.Release(b_0_1);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::C>.RunAsync<TResult, TParam>(global::System.Func<global::C, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::B b_0_4;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_3;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_2;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_7;
        var hasAwaitStarted_c_0_7 = false;
        var c_0_1 = default(global::C);
        var hasAwaitCompleted_c_0_7 = false;
        global::C c_0_0;
        iFactory_0_6 = GetIFactoryField1();
        b_0_5 = iFactory_0_6.Create();
        try
        {
            b_0_4 = this.M<global::B>(t: b_0_5);
            iAsyncFactory_0_3 = (global::StrongInject.IAsyncFactory<global::C>)b_0_4;
            iAsyncFactory_0_2 = this.M<global::StrongInject.IAsyncFactory<global::C>>(t: iAsyncFactory_0_3);
            c_0_7 = iAsyncFactory_0_2.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_7 = true;
                c_0_1 = await c_0_7;
                hasAwaitCompleted_c_0_7 = true;
                c_0_0 = this.M<global::C>(t: c_0_1);
            }
            catch
            {
                if (!hasAwaitStarted_c_0_7)
                {
                    c_0_1 = await c_0_7;
                }
                else if (!hasAwaitCompleted_c_0_7)
                {
                    throw;
                }

                await iAsyncFactory_0_2.ReleaseAsync(c_0_1);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        TResult result;
        try
        {
            result = await func(c_0_0, param);
        }
        finally
        {
            await iAsyncFactory_0_2.ReleaseAsync(c_0_1);
            iFactory_0_6.Release(b_0_5);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::C>> global::StrongInject.IAsyncContainer<global::C>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::B b_0_4;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_3;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_2;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_7;
        var hasAwaitStarted_c_0_7 = false;
        var c_0_1 = default(global::C);
        var hasAwaitCompleted_c_0_7 = false;
        global::C c_0_0;
        iFactory_0_6 = GetIFactoryField1();
        b_0_5 = iFactory_0_6.Create();
        try
        {
            b_0_4 = this.M<global::B>(t: b_0_5);
            iAsyncFactory_0_3 = (global::StrongInject.IAsyncFactory<global::C>)b_0_4;
            iAsyncFactory_0_2 = this.M<global::StrongInject.IAsyncFactory<global::C>>(t: iAsyncFactory_0_3);
            c_0_7 = iAsyncFactory_0_2.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_7 = true;
                c_0_1 = await c_0_7;
                hasAwaitCompleted_c_0_7 = true;
                c_0_0 = this.M<global::C>(t: c_0_1);
            }
            catch
            {
                if (!hasAwaitStarted_c_0_7)
                {
                    c_0_1 = await c_0_7;
                }
                else if (!hasAwaitCompleted_c_0_7)
                {
                    throw;
                }

                await iAsyncFactory_0_2.ReleaseAsync(c_0_1);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::C>(c_0_0, async () =>
        {
            await iAsyncFactory_0_2.ReleaseAsync(c_0_1);
            iFactory_0_6.Release(b_0_5);
        });
    }

    TResult global::StrongInject.IContainer<global::D>.Run<TResult, TParam>(global::System.Func<global::D, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::D> global::StrongInject.IContainer<global::D>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::E>.RunAsync<TResult, TParam>(global::System.Func<global::E, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::B> iFactory_0_12;
        global::B b_0_11;
        global::B b_0_10;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_9;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_8;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_13;
        var hasAwaitStarted_c_0_13 = false;
        var c_0_7 = default(global::C);
        var hasAwaitCompleted_c_0_13 = false;
        global::C c_0_6;
        global::StrongInject.IFactory<global::D> iFactory_0_5;
        global::StrongInject.IFactory<global::D> iFactory_0_4;
        global::D d_0_3;
        global::D d_0_2;
        global::E e_0_1;
        global::E e_0_0;
        iFactory_0_12 = GetIFactoryField1();
        b_0_11 = iFactory_0_12.Create();
        try
        {
            b_0_10 = this.M<global::B>(t: b_0_11);
            iAsyncFactory_0_9 = (global::StrongInject.IAsyncFactory<global::C>)b_0_10;
            iAsyncFactory_0_8 = this.M<global::StrongInject.IAsyncFactory<global::C>>(t: iAsyncFactory_0_9);
            c_0_13 = iAsyncFactory_0_8.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_13 = true;
                c_0_7 = await c_0_13;
                hasAwaitCompleted_c_0_13 = true;
                c_0_6 = this.M<global::C>(t: c_0_7);
                iFactory_0_5 = (global::StrongInject.IFactory<global::D>)c_0_6;
                iFactory_0_4 = this.M<global::StrongInject.IFactory<global::D>>(t: iFactory_0_5);
                d_0_3 = iFactory_0_4.Create();
                try
                {
                    d_0_2 = this.M<global::D>(t: d_0_3);
                    e_0_1 = (global::E)d_0_2;
                    e_0_0 = this.M<global::E>(t: e_0_1);
                }
                catch
                {
                    iFactory_0_4.Release(d_0_3);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_13)
                {
                    c_0_7 = await c_0_13;
                }
                else if (!hasAwaitCompleted_c_0_13)
                {
                    throw;
                }

                await iAsyncFactory_0_8.ReleaseAsync(c_0_7);
                throw;
            }
        }
        catch
        {
            iFactory_0_12.Release(b_0_11);
            throw;
        }

        TResult result;
        try
        {
            result = await func(e_0_0, param);
        }
        finally
        {
            iFactory_0_4.Release(d_0_3);
            await iAsyncFactory_0_8.ReleaseAsync(c_0_7);
            iFactory_0_12.Release(b_0_11);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::E>> global::StrongInject.IAsyncContainer<global::E>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::B> iFactory_0_12;
        global::B b_0_11;
        global::B b_0_10;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_9;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_8;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_13;
        var hasAwaitStarted_c_0_13 = false;
        var c_0_7 = default(global::C);
        var hasAwaitCompleted_c_0_13 = false;
        global::C c_0_6;
        global::StrongInject.IFactory<global::D> iFactory_0_5;
        global::StrongInject.IFactory<global::D> iFactory_0_4;
        global::D d_0_3;
        global::D d_0_2;
        global::E e_0_1;
        global::E e_0_0;
        iFactory_0_12 = GetIFactoryField1();
        b_0_11 = iFactory_0_12.Create();
        try
        {
            b_0_10 = this.M<global::B>(t: b_0_11);
            iAsyncFactory_0_9 = (global::StrongInject.IAsyncFactory<global::C>)b_0_10;
            iAsyncFactory_0_8 = this.M<global::StrongInject.IAsyncFactory<global::C>>(t: iAsyncFactory_0_9);
            c_0_13 = iAsyncFactory_0_8.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_13 = true;
                c_0_7 = await c_0_13;
                hasAwaitCompleted_c_0_13 = true;
                c_0_6 = this.M<global::C>(t: c_0_7);
                iFactory_0_5 = (global::StrongInject.IFactory<global::D>)c_0_6;
                iFactory_0_4 = this.M<global::StrongInject.IFactory<global::D>>(t: iFactory_0_5);
                d_0_3 = iFactory_0_4.Create();
                try
                {
                    d_0_2 = this.M<global::D>(t: d_0_3);
                    e_0_1 = (global::E)d_0_2;
                    e_0_0 = this.M<global::E>(t: e_0_1);
                }
                catch
                {
                    iFactory_0_4.Release(d_0_3);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_13)
                {
                    c_0_7 = await c_0_13;
                }
                else if (!hasAwaitCompleted_c_0_13)
                {
                    throw;
                }

                await iAsyncFactory_0_8.ReleaseAsync(c_0_7);
                throw;
            }
        }
        catch
        {
            iFactory_0_12.Release(b_0_11);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::E>(e_0_0, async () =>
        {
            iFactory_0_4.Release(d_0_3);
            await iAsyncFactory_0_8.ReleaseAsync(c_0_7);
            iFactory_0_12.Release(b_0_11);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::I>.RunAsync<TResult, TParam>(global::System.Func<global::I, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::B> iFactory_0_12;
        global::B b_0_11;
        global::B b_0_10;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_9;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_8;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_13;
        var hasAwaitStarted_c_0_13 = false;
        var c_0_7 = default(global::C);
        var hasAwaitCompleted_c_0_13 = false;
        global::C c_0_6;
        global::StrongInject.IFactory<global::D> iFactory_0_5;
        global::StrongInject.IFactory<global::D> iFactory_0_4;
        global::D d_0_3;
        global::D d_0_2;
        global::I i_0_1;
        global::I i_0_0;
        iFactory_0_12 = GetIFactoryField1();
        b_0_11 = iFactory_0_12.Create();
        try
        {
            b_0_10 = this.M<global::B>(t: b_0_11);
            iAsyncFactory_0_9 = (global::StrongInject.IAsyncFactory<global::C>)b_0_10;
            iAsyncFactory_0_8 = this.M<global::StrongInject.IAsyncFactory<global::C>>(t: iAsyncFactory_0_9);
            c_0_13 = iAsyncFactory_0_8.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_13 = true;
                c_0_7 = await c_0_13;
                hasAwaitCompleted_c_0_13 = true;
                c_0_6 = this.M<global::C>(t: c_0_7);
                iFactory_0_5 = (global::StrongInject.IFactory<global::D>)c_0_6;
                iFactory_0_4 = this.M<global::StrongInject.IFactory<global::D>>(t: iFactory_0_5);
                d_0_3 = iFactory_0_4.Create();
                try
                {
                    d_0_2 = this.M<global::D>(t: d_0_3);
                    i_0_1 = (global::I)d_0_2;
                    i_0_0 = this.M<global::I>(t: i_0_1);
                }
                catch
                {
                    iFactory_0_4.Release(d_0_3);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_13)
                {
                    c_0_7 = await c_0_13;
                }
                else if (!hasAwaitCompleted_c_0_13)
                {
                    throw;
                }

                await iAsyncFactory_0_8.ReleaseAsync(c_0_7);
                throw;
            }
        }
        catch
        {
            iFactory_0_12.Release(b_0_11);
            throw;
        }

        TResult result;
        try
        {
            result = await func(i_0_0, param);
        }
        finally
        {
            iFactory_0_4.Release(d_0_3);
            await iAsyncFactory_0_8.ReleaseAsync(c_0_7);
            iFactory_0_12.Release(b_0_11);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::I>> global::StrongInject.IAsyncContainer<global::I>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::B> iFactory_0_12;
        global::B b_0_11;
        global::B b_0_10;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_9;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_8;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_13;
        var hasAwaitStarted_c_0_13 = false;
        var c_0_7 = default(global::C);
        var hasAwaitCompleted_c_0_13 = false;
        global::C c_0_6;
        global::StrongInject.IFactory<global::D> iFactory_0_5;
        global::StrongInject.IFactory<global::D> iFactory_0_4;
        global::D d_0_3;
        global::D d_0_2;
        global::I i_0_1;
        global::I i_0_0;
        iFactory_0_12 = GetIFactoryField1();
        b_0_11 = iFactory_0_12.Create();
        try
        {
            b_0_10 = this.M<global::B>(t: b_0_11);
            iAsyncFactory_0_9 = (global::StrongInject.IAsyncFactory<global::C>)b_0_10;
            iAsyncFactory_0_8 = this.M<global::StrongInject.IAsyncFactory<global::C>>(t: iAsyncFactory_0_9);
            c_0_13 = iAsyncFactory_0_8.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_13 = true;
                c_0_7 = await c_0_13;
                hasAwaitCompleted_c_0_13 = true;
                c_0_6 = this.M<global::C>(t: c_0_7);
                iFactory_0_5 = (global::StrongInject.IFactory<global::D>)c_0_6;
                iFactory_0_4 = this.M<global::StrongInject.IFactory<global::D>>(t: iFactory_0_5);
                d_0_3 = iFactory_0_4.Create();
                try
                {
                    d_0_2 = this.M<global::D>(t: d_0_3);
                    i_0_1 = (global::I)d_0_2;
                    i_0_0 = this.M<global::I>(t: i_0_1);
                }
                catch
                {
                    iFactory_0_4.Release(d_0_3);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_13)
                {
                    c_0_7 = await c_0_13;
                }
                else if (!hasAwaitCompleted_c_0_13)
                {
                    throw;
                }

                await iAsyncFactory_0_8.ReleaseAsync(c_0_7);
                throw;
            }
        }
        catch
        {
            iFactory_0_12.Release(b_0_11);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::I>(i_0_0, async () =>
        {
            iFactory_0_4.Release(d_0_3);
            await iAsyncFactory_0_8.ReleaseAsync(c_0_7);
            iFactory_0_12.Release(b_0_11);
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IAsyncFactory<global::C>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::B> iFactory_0_4;
        global::B b_0_3;
        global::B b_0_2;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        iFactory_0_4 = GetIFactoryField1();
        b_0_3 = iFactory_0_4.Create();
        try
        {
            b_0_2 = this.M<global::B>(t: b_0_3);
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            iAsyncFactory_0_0 = this.M<global::StrongInject.IAsyncFactory<global::C>>(t: iAsyncFactory_0_1);
        }
        catch
        {
            iFactory_0_4.Release(b_0_3);
            throw;
        }

        TResult result;
        try
        {
            result = func(iAsyncFactory_0_0, param);
        }
        finally
        {
            iFactory_0_4.Release(b_0_3);
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>> global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::StrongInject.IFactory<global::B> iFactory_0_4;
        global::B b_0_3;
        global::B b_0_2;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        iFactory_0_4 = GetIFactoryField1();
        b_0_3 = iFactory_0_4.Create();
        try
        {
            b_0_2 = this.M<global::B>(t: b_0_3);
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            iAsyncFactory_0_0 = this.M<global::StrongInject.IAsyncFactory<global::C>>(t: iAsyncFactory_0_1);
        }
        catch
        {
            iFactory_0_4.Release(b_0_3);
            throw;
        }

        return new global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>>(iAsyncFactory_0_0, () =>
        {
            iFactory_0_4.Release(b_0_3);
        });
    }
}");
        }

        [Fact]
        public void InstanceWithAsEverythingPossibleAndDoNotDecorateIsNotDecorated()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IContainer<A>, IContainer<B>, IAsyncContainer<C>, IAsyncContainer<D>, IAsyncContainer<E>, IAsyncContainer<I>, IContainer<IAsyncFactory<C>>
{
    [Instance(Options.AsEverythingPossible | Options.DoNotDecorate)] A _a;
    [DecoratorFactory] T M<T>(T t) => t;
}

public class A : IFactory<B> { public B Create() => default; }
public class B : IAsyncFactory<C> { public ValueTask<C> CreateAsync() => default; }
public class C : IFactory<D> { public D Create() => default; }
public class D : E {}
public class E : I {}
public interface I {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (7,72): Warning CS0649: Field 'Container._a' is never assigned to, and will always have its default value null
                // _a
                new DiagnosticResult("CS0649", @"_a", DiagnosticSeverity.Warning).WithLocation(7, 72));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    void global::System.IDisposable.Dispose()
    {
        throw new global::StrongInject.StrongInjectException(""This container requires async disposal"");
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::StrongInject.IFactory<global::B> iFactory_0_1;
        global::B b_0_0;
        a_0_2 = this._a;
        iFactory_0_1 = (global::StrongInject.IFactory<global::B>)a_0_2;
        b_0_0 = iFactory_0_1.Create();
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(b_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::StrongInject.IFactory<global::B> iFactory_0_1;
        global::B b_0_0;
        a_0_2 = this._a;
        iFactory_0_1 = (global::StrongInject.IFactory<global::B>)a_0_2;
        b_0_0 = iFactory_0_1.Create();
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
            iFactory_0_1.Release(b_0_0);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::C>.RunAsync<TResult, TParam>(global::System.Func<global::C, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_4;
        global::StrongInject.IFactory<global::B> iFactory_0_3;
        global::B b_0_2;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_5;
        var hasAwaitStarted_c_0_5 = false;
        var c_0_0 = default(global::C);
        var hasAwaitCompleted_c_0_5 = false;
        a_0_4 = this._a;
        iFactory_0_3 = (global::StrongInject.IFactory<global::B>)a_0_4;
        b_0_2 = iFactory_0_3.Create();
        try
        {
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            c_0_5 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_5 = true;
                c_0_0 = await c_0_5;
                hasAwaitCompleted_c_0_5 = true;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_5)
                {
                    c_0_0 = await c_0_5;
                }
                else if (!hasAwaitCompleted_c_0_5)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
                throw;
            }
        }
        catch
        {
            iFactory_0_3.Release(b_0_2);
            throw;
        }

        TResult result;
        try
        {
            result = await func(c_0_0, param);
        }
        finally
        {
            await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
            iFactory_0_3.Release(b_0_2);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::C>> global::StrongInject.IAsyncContainer<global::C>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_4;
        global::StrongInject.IFactory<global::B> iFactory_0_3;
        global::B b_0_2;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_5;
        var hasAwaitStarted_c_0_5 = false;
        var c_0_0 = default(global::C);
        var hasAwaitCompleted_c_0_5 = false;
        a_0_4 = this._a;
        iFactory_0_3 = (global::StrongInject.IFactory<global::B>)a_0_4;
        b_0_2 = iFactory_0_3.Create();
        try
        {
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            c_0_5 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_5 = true;
                c_0_0 = await c_0_5;
                hasAwaitCompleted_c_0_5 = true;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_5)
                {
                    c_0_0 = await c_0_5;
                }
                else if (!hasAwaitCompleted_c_0_5)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
                throw;
            }
        }
        catch
        {
            iFactory_0_3.Release(b_0_2);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::C>(c_0_0, async () =>
        {
            await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
            iFactory_0_3.Release(b_0_2);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::D>.RunAsync<TResult, TParam>(global::System.Func<global::D, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_6;
        global::StrongInject.IFactory<global::B> iFactory_0_5;
        global::B b_0_4;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_3;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_7;
        var hasAwaitStarted_c_0_7 = false;
        var c_0_2 = default(global::C);
        var hasAwaitCompleted_c_0_7 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_1;
        global::D d_0_0;
        a_0_6 = this._a;
        iFactory_0_5 = (global::StrongInject.IFactory<global::B>)a_0_6;
        b_0_4 = iFactory_0_5.Create();
        try
        {
            iAsyncFactory_0_3 = (global::StrongInject.IAsyncFactory<global::C>)b_0_4;
            c_0_7 = iAsyncFactory_0_3.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_7 = true;
                c_0_2 = await c_0_7;
                hasAwaitCompleted_c_0_7 = true;
                iFactory_0_1 = (global::StrongInject.IFactory<global::D>)c_0_2;
                d_0_0 = iFactory_0_1.Create();
            }
            catch
            {
                if (!hasAwaitStarted_c_0_7)
                {
                    c_0_2 = await c_0_7;
                }
                else if (!hasAwaitCompleted_c_0_7)
                {
                    throw;
                }

                await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
                throw;
            }
        }
        catch
        {
            iFactory_0_5.Release(b_0_4);
            throw;
        }

        TResult result;
        try
        {
            result = await func(d_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(d_0_0);
            await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
            iFactory_0_5.Release(b_0_4);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::D>> global::StrongInject.IAsyncContainer<global::D>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_6;
        global::StrongInject.IFactory<global::B> iFactory_0_5;
        global::B b_0_4;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_3;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_7;
        var hasAwaitStarted_c_0_7 = false;
        var c_0_2 = default(global::C);
        var hasAwaitCompleted_c_0_7 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_1;
        global::D d_0_0;
        a_0_6 = this._a;
        iFactory_0_5 = (global::StrongInject.IFactory<global::B>)a_0_6;
        b_0_4 = iFactory_0_5.Create();
        try
        {
            iAsyncFactory_0_3 = (global::StrongInject.IAsyncFactory<global::C>)b_0_4;
            c_0_7 = iAsyncFactory_0_3.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_7 = true;
                c_0_2 = await c_0_7;
                hasAwaitCompleted_c_0_7 = true;
                iFactory_0_1 = (global::StrongInject.IFactory<global::D>)c_0_2;
                d_0_0 = iFactory_0_1.Create();
            }
            catch
            {
                if (!hasAwaitStarted_c_0_7)
                {
                    c_0_2 = await c_0_7;
                }
                else if (!hasAwaitCompleted_c_0_7)
                {
                    throw;
                }

                await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
                throw;
            }
        }
        catch
        {
            iFactory_0_5.Release(b_0_4);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::D>(d_0_0, async () =>
        {
            iFactory_0_1.Release(d_0_0);
            await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
            iFactory_0_5.Release(b_0_4);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::E>.RunAsync<TResult, TParam>(global::System.Func<global::E, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::E e_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    e_0_0 = (global::E)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        TResult result;
        try
        {
            result = await func(e_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::E>> global::StrongInject.IAsyncContainer<global::E>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::E e_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    e_0_0 = (global::E)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::E>(e_0_0, async () =>
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::I>.RunAsync<TResult, TParam>(global::System.Func<global::I, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::I i_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    i_0_0 = (global::I)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        TResult result;
        try
        {
            result = await func(i_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::I>> global::StrongInject.IAsyncContainer<global::I>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::I i_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    i_0_0 = (global::I)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::I>(i_0_0, async () =>
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IAsyncFactory<global::C>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::StrongInject.IFactory<global::B> iFactory_0_2;
        global::B b_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        a_0_3 = this._a;
        iFactory_0_2 = (global::StrongInject.IFactory<global::B>)a_0_3;
        b_0_1 = iFactory_0_2.Create();
        try
        {
            iAsyncFactory_0_0 = (global::StrongInject.IAsyncFactory<global::C>)b_0_1;
        }
        catch
        {
            iFactory_0_2.Release(b_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(iAsyncFactory_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(b_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>> global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::StrongInject.IFactory<global::B> iFactory_0_2;
        global::B b_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        a_0_3 = this._a;
        iFactory_0_2 = (global::StrongInject.IFactory<global::B>)a_0_3;
        b_0_1 = iFactory_0_2.Create();
        try
        {
            iAsyncFactory_0_0 = (global::StrongInject.IAsyncFactory<global::C>)b_0_1;
        }
        catch
        {
            iFactory_0_2.Release(b_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>>(iAsyncFactory_0_0, () =>
        {
            iFactory_0_2.Release(b_0_1);
        });
    }
}");
        }

        [Fact]
        public void InstanceWithAsEverythingPossibleAndFactoryTargetScopeShouldBeInstancePerResolutionUsesCorrectScope()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IContainer<A>, IContainer<B>, IAsyncContainer<C>, IAsyncContainer<D>, IAsyncContainer<E>, IAsyncContainer<I>, IContainer<IAsyncFactory<C>>
{
    [Instance(Options.AsEverythingPossible | Options.FactoryTargetScopeShouldBeInstancePerResolution)] A _a;
}

public class A : IFactory<B> { public B Create() => default; }
public class B : IAsyncFactory<C> { public ValueTask<C> CreateAsync() => default; }
public class C : IFactory<D> { public D Create() => default; }
public class D : E {}
public class E : I {}
public interface I {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (7,106): Warning CS0649: Field 'Container._a' is never assigned to, and will always have its default value null
                // _a
                new DiagnosticResult("CS0649", @"_a", DiagnosticSeverity.Warning).WithLocation(7, 106));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    void global::System.IDisposable.Dispose()
    {
        throw new global::StrongInject.StrongInjectException(""This container requires async disposal"");
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::StrongInject.IFactory<global::B> iFactory_0_1;
        global::B b_0_0;
        a_0_2 = this._a;
        iFactory_0_1 = (global::StrongInject.IFactory<global::B>)a_0_2;
        b_0_0 = iFactory_0_1.Create();
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(b_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::StrongInject.IFactory<global::B> iFactory_0_1;
        global::B b_0_0;
        a_0_2 = this._a;
        iFactory_0_1 = (global::StrongInject.IFactory<global::B>)a_0_2;
        b_0_0 = iFactory_0_1.Create();
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
            iFactory_0_1.Release(b_0_0);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::C>.RunAsync<TResult, TParam>(global::System.Func<global::C, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_4;
        global::StrongInject.IFactory<global::B> iFactory_0_3;
        global::B b_0_2;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_5;
        var hasAwaitStarted_c_0_5 = false;
        var c_0_0 = default(global::C);
        var hasAwaitCompleted_c_0_5 = false;
        a_0_4 = this._a;
        iFactory_0_3 = (global::StrongInject.IFactory<global::B>)a_0_4;
        b_0_2 = iFactory_0_3.Create();
        try
        {
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            c_0_5 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_5 = true;
                c_0_0 = await c_0_5;
                hasAwaitCompleted_c_0_5 = true;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_5)
                {
                    c_0_0 = await c_0_5;
                }
                else if (!hasAwaitCompleted_c_0_5)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
                throw;
            }
        }
        catch
        {
            iFactory_0_3.Release(b_0_2);
            throw;
        }

        TResult result;
        try
        {
            result = await func(c_0_0, param);
        }
        finally
        {
            await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
            iFactory_0_3.Release(b_0_2);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::C>> global::StrongInject.IAsyncContainer<global::C>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_4;
        global::StrongInject.IFactory<global::B> iFactory_0_3;
        global::B b_0_2;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_5;
        var hasAwaitStarted_c_0_5 = false;
        var c_0_0 = default(global::C);
        var hasAwaitCompleted_c_0_5 = false;
        a_0_4 = this._a;
        iFactory_0_3 = (global::StrongInject.IFactory<global::B>)a_0_4;
        b_0_2 = iFactory_0_3.Create();
        try
        {
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            c_0_5 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_5 = true;
                c_0_0 = await c_0_5;
                hasAwaitCompleted_c_0_5 = true;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_5)
                {
                    c_0_0 = await c_0_5;
                }
                else if (!hasAwaitCompleted_c_0_5)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
                throw;
            }
        }
        catch
        {
            iFactory_0_3.Release(b_0_2);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::C>(c_0_0, async () =>
        {
            await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
            iFactory_0_3.Release(b_0_2);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::D>.RunAsync<TResult, TParam>(global::System.Func<global::D, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_6;
        global::StrongInject.IFactory<global::B> iFactory_0_5;
        global::B b_0_4;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_3;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_7;
        var hasAwaitStarted_c_0_7 = false;
        var c_0_2 = default(global::C);
        var hasAwaitCompleted_c_0_7 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_1;
        global::D d_0_0;
        a_0_6 = this._a;
        iFactory_0_5 = (global::StrongInject.IFactory<global::B>)a_0_6;
        b_0_4 = iFactory_0_5.Create();
        try
        {
            iAsyncFactory_0_3 = (global::StrongInject.IAsyncFactory<global::C>)b_0_4;
            c_0_7 = iAsyncFactory_0_3.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_7 = true;
                c_0_2 = await c_0_7;
                hasAwaitCompleted_c_0_7 = true;
                iFactory_0_1 = (global::StrongInject.IFactory<global::D>)c_0_2;
                d_0_0 = iFactory_0_1.Create();
            }
            catch
            {
                if (!hasAwaitStarted_c_0_7)
                {
                    c_0_2 = await c_0_7;
                }
                else if (!hasAwaitCompleted_c_0_7)
                {
                    throw;
                }

                await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
                throw;
            }
        }
        catch
        {
            iFactory_0_5.Release(b_0_4);
            throw;
        }

        TResult result;
        try
        {
            result = await func(d_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(d_0_0);
            await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
            iFactory_0_5.Release(b_0_4);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::D>> global::StrongInject.IAsyncContainer<global::D>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_6;
        global::StrongInject.IFactory<global::B> iFactory_0_5;
        global::B b_0_4;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_3;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_7;
        var hasAwaitStarted_c_0_7 = false;
        var c_0_2 = default(global::C);
        var hasAwaitCompleted_c_0_7 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_1;
        global::D d_0_0;
        a_0_6 = this._a;
        iFactory_0_5 = (global::StrongInject.IFactory<global::B>)a_0_6;
        b_0_4 = iFactory_0_5.Create();
        try
        {
            iAsyncFactory_0_3 = (global::StrongInject.IAsyncFactory<global::C>)b_0_4;
            c_0_7 = iAsyncFactory_0_3.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_7 = true;
                c_0_2 = await c_0_7;
                hasAwaitCompleted_c_0_7 = true;
                iFactory_0_1 = (global::StrongInject.IFactory<global::D>)c_0_2;
                d_0_0 = iFactory_0_1.Create();
            }
            catch
            {
                if (!hasAwaitStarted_c_0_7)
                {
                    c_0_2 = await c_0_7;
                }
                else if (!hasAwaitCompleted_c_0_7)
                {
                    throw;
                }

                await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
                throw;
            }
        }
        catch
        {
            iFactory_0_5.Release(b_0_4);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::D>(d_0_0, async () =>
        {
            iFactory_0_1.Release(d_0_0);
            await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
            iFactory_0_5.Release(b_0_4);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::E>.RunAsync<TResult, TParam>(global::System.Func<global::E, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::E e_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    e_0_0 = (global::E)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        TResult result;
        try
        {
            result = await func(e_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::E>> global::StrongInject.IAsyncContainer<global::E>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::E e_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    e_0_0 = (global::E)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::E>(e_0_0, async () =>
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::I>.RunAsync<TResult, TParam>(global::System.Func<global::I, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::I i_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    i_0_0 = (global::I)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        TResult result;
        try
        {
            result = await func(i_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::I>> global::StrongInject.IAsyncContainer<global::I>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::I i_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    i_0_0 = (global::I)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::I>(i_0_0, async () =>
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IAsyncFactory<global::C>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::StrongInject.IFactory<global::B> iFactory_0_2;
        global::B b_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        a_0_3 = this._a;
        iFactory_0_2 = (global::StrongInject.IFactory<global::B>)a_0_3;
        b_0_1 = iFactory_0_2.Create();
        try
        {
            iAsyncFactory_0_0 = (global::StrongInject.IAsyncFactory<global::C>)b_0_1;
        }
        catch
        {
            iFactory_0_2.Release(b_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(iAsyncFactory_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(b_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>> global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::StrongInject.IFactory<global::B> iFactory_0_2;
        global::B b_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        a_0_3 = this._a;
        iFactory_0_2 = (global::StrongInject.IFactory<global::B>)a_0_3;
        b_0_1 = iFactory_0_2.Create();
        try
        {
            iAsyncFactory_0_0 = (global::StrongInject.IAsyncFactory<global::C>)b_0_1;
        }
        catch
        {
            iFactory_0_2.Release(b_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>>(iAsyncFactory_0_0, () =>
        {
            iFactory_0_2.Release(b_0_1);
        });
    }
}");
        }

        [Fact]
        public void InstanceWithAsEverythingPossibleAndFactoryTargetScopeShouldBeSingleInstanceUsesCorrectScope()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IContainer<A>, IContainer<B>, IAsyncContainer<C>, IAsyncContainer<D>, IAsyncContainer<E>, IAsyncContainer<I>, IContainer<IAsyncFactory<C>>
{
    [Instance(Options.AsEverythingPossible | Options.FactoryTargetScopeShouldBeSingleInstance)] A _a;
}

public class A : IFactory<B> { public B Create() => default; }
public class B : IAsyncFactory<C> { public ValueTask<C> CreateAsync() => default; }
public class C : IFactory<D> { public D Create() => default; }
public class D : E {}
public class E : I {}
public interface I {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (7,99): Warning CS0649: Field 'Container._a' is never assigned to, and will always have its default value null
                // _a
                new DiagnosticResult("CS0649", @"_a", DiagnosticSeverity.Warning).WithLocation(7, 99));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock2.WaitAsync();
        try
        {
            await (this._disposeAction2?.Invoke() ?? default);
        }
        finally
        {
            this._lock2.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }

        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    void global::System.IDisposable.Dispose()
    {
        throw new global::StrongInject.StrongInjectException(""This container requires async disposal"");
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    private global::B _bField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::B GetBField0()
    {
        if (!object.ReferenceEquals(_bField0, null))
            return _bField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::A a_0_2;
            global::StrongInject.IFactory<global::B> iFactory_0_1;
            global::B b_0_0;
            a_0_2 = this._a;
            iFactory_0_1 = (global::StrongInject.IFactory<global::B>)a_0_2;
            b_0_0 = iFactory_0_1.Create();
            this._bField0 = b_0_0;
            this._disposeAction0 = async () =>
            {
                iFactory_0_1.Release(b_0_0);
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _bField0;
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_0;
        b_0_0 = GetBField0();
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_0;
        b_0_0 = GetBField0();
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
        });
    }

    private global::C _cField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private async global::System.Threading.Tasks.ValueTask<global::C> GetCField1()
    {
        if (!object.ReferenceEquals(_cField1, null))
            return _cField1;
        await this._lock1.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::B b_0_2;
            global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
            global::System.Threading.Tasks.ValueTask<global::C> c_0_3;
            var hasAwaitStarted_c_0_3 = false;
            var c_0_0 = default(global::C);
            var hasAwaitCompleted_c_0_3 = false;
            b_0_2 = GetBField0();
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            c_0_3 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_3 = true;
                c_0_0 = await c_0_3;
                hasAwaitCompleted_c_0_3 = true;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_3)
                {
                    c_0_0 = await c_0_3;
                }
                else if (!hasAwaitCompleted_c_0_3)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
                throw;
            }

            this._cField1 = c_0_0;
            this._disposeAction1 = async () =>
            {
                await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _cField1;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::C>.RunAsync<TResult, TParam>(global::System.Func<global::C, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::C> c_0_0;
        var hasAwaitStarted_c_0_0 = false;
        var c_0_1 = default(global::C);
        c_0_0 = GetCField1();
        try
        {
            hasAwaitStarted_c_0_0 = true;
            c_0_1 = await c_0_0;
        }
        catch
        {
            if (!hasAwaitStarted_c_0_0)
            {
                _ = c_0_0.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(c_0_1, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::C>> global::StrongInject.IAsyncContainer<global::C>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::C> c_0_0;
        var hasAwaitStarted_c_0_0 = false;
        var c_0_1 = default(global::C);
        c_0_0 = GetCField1();
        try
        {
            hasAwaitStarted_c_0_0 = true;
            c_0_1 = await c_0_0;
        }
        catch
        {
            if (!hasAwaitStarted_c_0_0)
            {
                _ = c_0_0.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::C>(c_0_1, async () =>
        {
        });
    }

    private global::D _dField2;
    private global::System.Threading.SemaphoreSlim _lock2 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction2;
    private async global::System.Threading.Tasks.ValueTask<global::D> GetDField2()
    {
        if (!object.ReferenceEquals(_dField2, null))
            return _dField2;
        await this._lock2.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::C> c_0_2;
            var hasAwaitStarted_c_0_2 = false;
            var c_0_3 = default(global::C);
            global::StrongInject.IFactory<global::D> iFactory_0_1;
            global::D d_0_0;
            c_0_2 = GetCField1();
            try
            {
                hasAwaitStarted_c_0_2 = true;
                c_0_3 = await c_0_2;
                iFactory_0_1 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_0 = iFactory_0_1.Create();
            }
            catch
            {
                if (!hasAwaitStarted_c_0_2)
                {
                    _ = c_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._dField2 = d_0_0;
            this._disposeAction2 = async () =>
            {
                iFactory_0_1.Release(d_0_0);
            };
        }
        finally
        {
            this._lock2.Release();
        }

        return _dField2;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::D>.RunAsync<TResult, TParam>(global::System.Func<global::D, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::D> d_0_0;
        var hasAwaitStarted_d_0_0 = false;
        var d_0_1 = default(global::D);
        d_0_0 = GetDField2();
        try
        {
            hasAwaitStarted_d_0_0 = true;
            d_0_1 = await d_0_0;
        }
        catch
        {
            if (!hasAwaitStarted_d_0_0)
            {
                _ = d_0_0.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(d_0_1, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::D>> global::StrongInject.IAsyncContainer<global::D>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::D> d_0_0;
        var hasAwaitStarted_d_0_0 = false;
        var d_0_1 = default(global::D);
        d_0_0 = GetDField2();
        try
        {
            hasAwaitStarted_d_0_0 = true;
            d_0_1 = await d_0_0;
        }
        catch
        {
            if (!hasAwaitStarted_d_0_0)
            {
                _ = d_0_0.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::D>(d_0_1, async () =>
        {
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::E>.RunAsync<TResult, TParam>(global::System.Func<global::E, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::D> d_0_1;
        var hasAwaitStarted_d_0_1 = false;
        var d_0_2 = default(global::D);
        global::E e_0_0;
        d_0_1 = GetDField2();
        try
        {
            hasAwaitStarted_d_0_1 = true;
            d_0_2 = await d_0_1;
            e_0_0 = (global::E)d_0_2;
        }
        catch
        {
            if (!hasAwaitStarted_d_0_1)
            {
                _ = d_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(e_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::E>> global::StrongInject.IAsyncContainer<global::E>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::D> d_0_1;
        var hasAwaitStarted_d_0_1 = false;
        var d_0_2 = default(global::D);
        global::E e_0_0;
        d_0_1 = GetDField2();
        try
        {
            hasAwaitStarted_d_0_1 = true;
            d_0_2 = await d_0_1;
            e_0_0 = (global::E)d_0_2;
        }
        catch
        {
            if (!hasAwaitStarted_d_0_1)
            {
                _ = d_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::E>(e_0_0, async () =>
        {
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::I>.RunAsync<TResult, TParam>(global::System.Func<global::I, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::D> d_0_1;
        var hasAwaitStarted_d_0_1 = false;
        var d_0_2 = default(global::D);
        global::I i_0_0;
        d_0_1 = GetDField2();
        try
        {
            hasAwaitStarted_d_0_1 = true;
            d_0_2 = await d_0_1;
            i_0_0 = (global::I)d_0_2;
        }
        catch
        {
            if (!hasAwaitStarted_d_0_1)
            {
                _ = d_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(i_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::I>> global::StrongInject.IAsyncContainer<global::I>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::D> d_0_1;
        var hasAwaitStarted_d_0_1 = false;
        var d_0_2 = default(global::D);
        global::I i_0_0;
        d_0_1 = GetDField2();
        try
        {
            hasAwaitStarted_d_0_1 = true;
            d_0_2 = await d_0_1;
            i_0_0 = (global::I)d_0_2;
        }
        catch
        {
            if (!hasAwaitStarted_d_0_1)
            {
                _ = d_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::I>(i_0_0, async () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IAsyncFactory<global::C>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        b_0_1 = GetBField0();
        iAsyncFactory_0_0 = (global::StrongInject.IAsyncFactory<global::C>)b_0_1;
        TResult result;
        try
        {
            result = func(iAsyncFactory_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>> global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        b_0_1 = GetBField0();
        iAsyncFactory_0_0 = (global::StrongInject.IAsyncFactory<global::C>)b_0_1;
        return new global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>>(iAsyncFactory_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void InstanceWithAsEverythingPossibleAndFactoryTargetScopeShouldBeInstancePerDependencyUsesCorrectScope()
        {
            string userSource = @"
using StrongInject;
using System.Threading.Tasks;

public partial class Container : IContainer<A>, IContainer<B>, IAsyncContainer<C>, IAsyncContainer<D>, IAsyncContainer<E>, IAsyncContainer<I>, IContainer<IAsyncFactory<C>>, IAsyncContainer<int>
{
    [Instance(Options.AsEverythingPossible | Options.FactoryTargetScopeShouldBeInstancePerDependency)] A _a;
    [Factory] int  M(D d1, D d2) => 42;
}

public class A : IFactory<B> { public B Create() => default; }
public class B : IAsyncFactory<C> { public ValueTask<C> CreateAsync() => default; }
public class C : IFactory<D> { public D Create() => default; }
public class D : E {}
public class E : I {}
public interface I {}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify(
                // (7,106): Warning CS0649: Field 'Container._a' is never assigned to, and will always have its default value null
                // _a
                new DiagnosticResult("CS0649", @"_a", DiagnosticSeverity.Warning).WithLocation(7, 106));
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    void global::System.IDisposable.Dispose()
    {
        throw new global::StrongInject.StrongInjectException(""This container requires async disposal"");
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = this._a;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }

    TResult global::StrongInject.IContainer<global::B>.Run<TResult, TParam>(global::System.Func<global::B, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::StrongInject.IFactory<global::B> iFactory_0_1;
        global::B b_0_0;
        a_0_2 = this._a;
        iFactory_0_1 = (global::StrongInject.IFactory<global::B>)a_0_2;
        b_0_0 = iFactory_0_1.Create();
        TResult result;
        try
        {
            result = func(b_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(b_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::B> global::StrongInject.IContainer<global::B>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::StrongInject.IFactory<global::B> iFactory_0_1;
        global::B b_0_0;
        a_0_2 = this._a;
        iFactory_0_1 = (global::StrongInject.IFactory<global::B>)a_0_2;
        b_0_0 = iFactory_0_1.Create();
        return new global::StrongInject.Owned<global::B>(b_0_0, () =>
        {
            iFactory_0_1.Release(b_0_0);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::C>.RunAsync<TResult, TParam>(global::System.Func<global::C, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_4;
        global::StrongInject.IFactory<global::B> iFactory_0_3;
        global::B b_0_2;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_5;
        var hasAwaitStarted_c_0_5 = false;
        var c_0_0 = default(global::C);
        var hasAwaitCompleted_c_0_5 = false;
        a_0_4 = this._a;
        iFactory_0_3 = (global::StrongInject.IFactory<global::B>)a_0_4;
        b_0_2 = iFactory_0_3.Create();
        try
        {
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            c_0_5 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_5 = true;
                c_0_0 = await c_0_5;
                hasAwaitCompleted_c_0_5 = true;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_5)
                {
                    c_0_0 = await c_0_5;
                }
                else if (!hasAwaitCompleted_c_0_5)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
                throw;
            }
        }
        catch
        {
            iFactory_0_3.Release(b_0_2);
            throw;
        }

        TResult result;
        try
        {
            result = await func(c_0_0, param);
        }
        finally
        {
            await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
            iFactory_0_3.Release(b_0_2);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::C>> global::StrongInject.IAsyncContainer<global::C>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_4;
        global::StrongInject.IFactory<global::B> iFactory_0_3;
        global::B b_0_2;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_1;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_5;
        var hasAwaitStarted_c_0_5 = false;
        var c_0_0 = default(global::C);
        var hasAwaitCompleted_c_0_5 = false;
        a_0_4 = this._a;
        iFactory_0_3 = (global::StrongInject.IFactory<global::B>)a_0_4;
        b_0_2 = iFactory_0_3.Create();
        try
        {
            iAsyncFactory_0_1 = (global::StrongInject.IAsyncFactory<global::C>)b_0_2;
            c_0_5 = iAsyncFactory_0_1.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_5 = true;
                c_0_0 = await c_0_5;
                hasAwaitCompleted_c_0_5 = true;
            }
            catch
            {
                if (!hasAwaitStarted_c_0_5)
                {
                    c_0_0 = await c_0_5;
                }
                else if (!hasAwaitCompleted_c_0_5)
                {
                    throw;
                }

                await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
                throw;
            }
        }
        catch
        {
            iFactory_0_3.Release(b_0_2);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::C>(c_0_0, async () =>
        {
            await iAsyncFactory_0_1.ReleaseAsync(c_0_0);
            iFactory_0_3.Release(b_0_2);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::D>.RunAsync<TResult, TParam>(global::System.Func<global::D, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_6;
        global::StrongInject.IFactory<global::B> iFactory_0_5;
        global::B b_0_4;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_3;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_7;
        var hasAwaitStarted_c_0_7 = false;
        var c_0_2 = default(global::C);
        var hasAwaitCompleted_c_0_7 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_1;
        global::D d_0_0;
        a_0_6 = this._a;
        iFactory_0_5 = (global::StrongInject.IFactory<global::B>)a_0_6;
        b_0_4 = iFactory_0_5.Create();
        try
        {
            iAsyncFactory_0_3 = (global::StrongInject.IAsyncFactory<global::C>)b_0_4;
            c_0_7 = iAsyncFactory_0_3.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_7 = true;
                c_0_2 = await c_0_7;
                hasAwaitCompleted_c_0_7 = true;
                iFactory_0_1 = (global::StrongInject.IFactory<global::D>)c_0_2;
                d_0_0 = iFactory_0_1.Create();
            }
            catch
            {
                if (!hasAwaitStarted_c_0_7)
                {
                    c_0_2 = await c_0_7;
                }
                else if (!hasAwaitCompleted_c_0_7)
                {
                    throw;
                }

                await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
                throw;
            }
        }
        catch
        {
            iFactory_0_5.Release(b_0_4);
            throw;
        }

        TResult result;
        try
        {
            result = await func(d_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(d_0_0);
            await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
            iFactory_0_5.Release(b_0_4);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::D>> global::StrongInject.IAsyncContainer<global::D>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_6;
        global::StrongInject.IFactory<global::B> iFactory_0_5;
        global::B b_0_4;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_3;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_7;
        var hasAwaitStarted_c_0_7 = false;
        var c_0_2 = default(global::C);
        var hasAwaitCompleted_c_0_7 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_1;
        global::D d_0_0;
        a_0_6 = this._a;
        iFactory_0_5 = (global::StrongInject.IFactory<global::B>)a_0_6;
        b_0_4 = iFactory_0_5.Create();
        try
        {
            iAsyncFactory_0_3 = (global::StrongInject.IAsyncFactory<global::C>)b_0_4;
            c_0_7 = iAsyncFactory_0_3.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_7 = true;
                c_0_2 = await c_0_7;
                hasAwaitCompleted_c_0_7 = true;
                iFactory_0_1 = (global::StrongInject.IFactory<global::D>)c_0_2;
                d_0_0 = iFactory_0_1.Create();
            }
            catch
            {
                if (!hasAwaitStarted_c_0_7)
                {
                    c_0_2 = await c_0_7;
                }
                else if (!hasAwaitCompleted_c_0_7)
                {
                    throw;
                }

                await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
                throw;
            }
        }
        catch
        {
            iFactory_0_5.Release(b_0_4);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::D>(d_0_0, async () =>
        {
            iFactory_0_1.Release(d_0_0);
            await iAsyncFactory_0_3.ReleaseAsync(c_0_2);
            iFactory_0_5.Release(b_0_4);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::E>.RunAsync<TResult, TParam>(global::System.Func<global::E, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::E e_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    e_0_0 = (global::E)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        TResult result;
        try
        {
            result = await func(e_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::E>> global::StrongInject.IAsyncContainer<global::E>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::E e_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    e_0_0 = (global::E)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::E>(e_0_0, async () =>
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::I>.RunAsync<TResult, TParam>(global::System.Func<global::I, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::I i_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    i_0_0 = (global::I)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        TResult result;
        try
        {
            result = await func(i_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::I>> global::StrongInject.IAsyncContainer<global::I>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        global::I i_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                hasAwaitStarted_c_0_8 = true;
                c_0_3 = await c_0_8;
                hasAwaitCompleted_c_0_8 = true;
                iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                d_0_1 = iFactory_0_2.Create();
                try
                {
                    i_0_0 = (global::I)d_0_1;
                }
                catch
                {
                    iFactory_0_2.Release(d_0_1);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::I>(i_0_0, async () =>
        {
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        });
    }

    TResult global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Run<TResult, TParam>(global::System.Func<global::StrongInject.IAsyncFactory<global::C>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::StrongInject.IFactory<global::B> iFactory_0_2;
        global::B b_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        a_0_3 = this._a;
        iFactory_0_2 = (global::StrongInject.IFactory<global::B>)a_0_3;
        b_0_1 = iFactory_0_2.Create();
        try
        {
            iAsyncFactory_0_0 = (global::StrongInject.IAsyncFactory<global::C>)b_0_1;
        }
        catch
        {
            iFactory_0_2.Release(b_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(iAsyncFactory_0_0, param);
        }
        finally
        {
            iFactory_0_2.Release(b_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>> global::StrongInject.IContainer<global::StrongInject.IAsyncFactory<global::C>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_3;
        global::StrongInject.IFactory<global::B> iFactory_0_2;
        global::B b_0_1;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_0;
        a_0_3 = this._a;
        iFactory_0_2 = (global::StrongInject.IFactory<global::B>)a_0_3;
        b_0_1 = iFactory_0_2.Create();
        try
        {
            iAsyncFactory_0_0 = (global::StrongInject.IAsyncFactory<global::C>)b_0_1;
        }
        catch
        {
            iFactory_0_2.Release(b_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::StrongInject.IAsyncFactory<global::C>>(iAsyncFactory_0_0, () =>
        {
            iFactory_0_2.Release(b_0_1);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Int32>.RunAsync<TResult, TParam>(global::System.Func<global::System.Int32, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        global::B b_0_13;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_12;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_14;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        var hasAwaitStarted_c_0_14 = false;
        var c_0_11 = default(global::C);
        var hasAwaitCompleted_c_0_14 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_10;
        global::D d_0_9;
        global::System.Int32 int32_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                b_0_13 = iFactory_0_6.Create();
                try
                {
                    iAsyncFactory_0_12 = (global::StrongInject.IAsyncFactory<global::C>)b_0_13;
                    c_0_14 = iAsyncFactory_0_12.CreateAsync();
                    try
                    {
                        hasAwaitStarted_c_0_8 = true;
                        c_0_3 = await c_0_8;
                        hasAwaitCompleted_c_0_8 = true;
                        iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                        d_0_1 = iFactory_0_2.Create();
                        try
                        {
                            hasAwaitStarted_c_0_14 = true;
                            c_0_11 = await c_0_14;
                            hasAwaitCompleted_c_0_14 = true;
                            iFactory_0_10 = (global::StrongInject.IFactory<global::D>)c_0_11;
                            d_0_9 = iFactory_0_10.Create();
                            try
                            {
                                int32_0_0 = this.M(d1: d_0_1, d2: d_0_9);
                            }
                            catch
                            {
                                iFactory_0_10.Release(d_0_9);
                                throw;
                            }
                        }
                        catch
                        {
                            iFactory_0_2.Release(d_0_1);
                            throw;
                        }
                    }
                    catch
                    {
                        if (!hasAwaitStarted_c_0_14)
                        {
                            c_0_11 = await c_0_14;
                        }
                        else if (!hasAwaitCompleted_c_0_14)
                        {
                            throw;
                        }

                        await iAsyncFactory_0_12.ReleaseAsync(c_0_11);
                        throw;
                    }
                }
                catch
                {
                    iFactory_0_6.Release(b_0_13);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        TResult result;
        try
        {
            result = await func(int32_0_0, param);
        }
        finally
        {
            iFactory_0_10.Release(d_0_9);
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_12.ReleaseAsync(c_0_11);
            iFactory_0_6.Release(b_0_13);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Int32>> global::StrongInject.IAsyncContainer<global::System.Int32>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_7;
        global::StrongInject.IFactory<global::B> iFactory_0_6;
        global::B b_0_5;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_4;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_8;
        global::B b_0_13;
        global::StrongInject.IAsyncFactory<global::C> iAsyncFactory_0_12;
        global::System.Threading.Tasks.ValueTask<global::C> c_0_14;
        var hasAwaitStarted_c_0_8 = false;
        var c_0_3 = default(global::C);
        var hasAwaitCompleted_c_0_8 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_2;
        global::D d_0_1;
        var hasAwaitStarted_c_0_14 = false;
        var c_0_11 = default(global::C);
        var hasAwaitCompleted_c_0_14 = false;
        global::StrongInject.IFactory<global::D> iFactory_0_10;
        global::D d_0_9;
        global::System.Int32 int32_0_0;
        a_0_7 = this._a;
        iFactory_0_6 = (global::StrongInject.IFactory<global::B>)a_0_7;
        b_0_5 = iFactory_0_6.Create();
        try
        {
            iAsyncFactory_0_4 = (global::StrongInject.IAsyncFactory<global::C>)b_0_5;
            c_0_8 = iAsyncFactory_0_4.CreateAsync();
            try
            {
                b_0_13 = iFactory_0_6.Create();
                try
                {
                    iAsyncFactory_0_12 = (global::StrongInject.IAsyncFactory<global::C>)b_0_13;
                    c_0_14 = iAsyncFactory_0_12.CreateAsync();
                    try
                    {
                        hasAwaitStarted_c_0_8 = true;
                        c_0_3 = await c_0_8;
                        hasAwaitCompleted_c_0_8 = true;
                        iFactory_0_2 = (global::StrongInject.IFactory<global::D>)c_0_3;
                        d_0_1 = iFactory_0_2.Create();
                        try
                        {
                            hasAwaitStarted_c_0_14 = true;
                            c_0_11 = await c_0_14;
                            hasAwaitCompleted_c_0_14 = true;
                            iFactory_0_10 = (global::StrongInject.IFactory<global::D>)c_0_11;
                            d_0_9 = iFactory_0_10.Create();
                            try
                            {
                                int32_0_0 = this.M(d1: d_0_1, d2: d_0_9);
                            }
                            catch
                            {
                                iFactory_0_10.Release(d_0_9);
                                throw;
                            }
                        }
                        catch
                        {
                            iFactory_0_2.Release(d_0_1);
                            throw;
                        }
                    }
                    catch
                    {
                        if (!hasAwaitStarted_c_0_14)
                        {
                            c_0_11 = await c_0_14;
                        }
                        else if (!hasAwaitCompleted_c_0_14)
                        {
                            throw;
                        }

                        await iAsyncFactory_0_12.ReleaseAsync(c_0_11);
                        throw;
                    }
                }
                catch
                {
                    iFactory_0_6.Release(b_0_13);
                    throw;
                }
            }
            catch
            {
                if (!hasAwaitStarted_c_0_8)
                {
                    c_0_3 = await c_0_8;
                }
                else if (!hasAwaitCompleted_c_0_8)
                {
                    throw;
                }

                await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
                throw;
            }
        }
        catch
        {
            iFactory_0_6.Release(b_0_5);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::System.Int32>(int32_0_0, async () =>
        {
            iFactory_0_10.Release(d_0_9);
            iFactory_0_2.Release(d_0_1);
            await iAsyncFactory_0_12.ReleaseAsync(c_0_11);
            iFactory_0_6.Release(b_0_13);
            await iAsyncFactory_0_4.ReleaseAsync(c_0_3);
            iFactory_0_6.Release(b_0_5);
        });
    }
}");
        }

        [Fact]
        public void ImportsRegistrationsFromBaseClass()
        {
            string userSource = @"
using StrongInject;

public class A {}

[Register(typeof(A))]
public class Module {}

public partial class Container : Module, IContainer<A>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ImportsRegistrationsFromBaseBaseClass()
        {
            string userSource = @"
using StrongInject;

public class A {}

[Register(typeof(A))]
public class Module {}

public class InBetween : Module {}

public partial class Container : InBetween, IContainer<A>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void CanOverrideRegistrationImportedFromBaseClass()
        {
            string userSource = @"
using StrongInject;

public class A {}
public class B : A {}
[Register(typeof(A))]
public class Module {}

[Register(typeof(B), typeof(A))]
public partial class Container : Module, IContainer<A>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        a_0_0 = (global::A)b_0_1;
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::A a_0_0;
        b_0_1 = new global::B();
        a_0_0 = (global::A)b_0_1;
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorIfRegistrationFromBaseClassConflictsWithThatFromImportedModule()
        {
            string userSource = @"
using StrongInject;

public class A {}
public class B : A {}

[Register(typeof(A))]
public class ModuleA {}

[Register(typeof(B), typeof(A))]
public class ModuleB{}

[RegisterModule(typeof(ModuleB))]
public partial class Container : ModuleA, IContainer<A>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (14,22): Error SI0106: Error while resolving dependencies for 'A': We have multiple sources for instance of type 'A' and no best source. Try adding a single registration for 'A' directly to the container, and moving any existing registrations for 'A' on the container to an imported module.
                // Container
                new DiagnosticResult("SI0106", @"Container", DiagnosticSeverity.Error).WithLocation(14, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ImportsProtectedInstanceFieldInstancePropertyFactoryAndDecoratorFromBaseClass()
        {
            string userSource = @"
using StrongInject;

public class A {}
public class B {}
public class C {}

public class Module
{
    [Instance] protected A A = new A();
    [Instance] protected internal B B => new B();
    [Factory] protected C CreateC(A a, B b) => new C();
    [DecoratorFactory] protected C DecorateC(C c) => c;
}

public partial class Container : Module, IContainer<C>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::C>.Run<TResult, TParam>(global::System.Func<global::C, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::B b_0_3;
        global::C c_0_1;
        global::C c_0_0;
        a_0_2 = this.A;
        b_0_3 = this.B;
        c_0_1 = this.CreateC(a: a_0_2, b: b_0_3);
        try
        {
            c_0_0 = this.DecorateC(c: c_0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(c_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::C> global::StrongInject.IContainer<global::C>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::B b_0_3;
        global::C c_0_1;
        global::C c_0_0;
        a_0_2 = this.A;
        b_0_3 = this.B;
        c_0_1 = this.CreateC(a: a_0_2, b: b_0_3);
        try
        {
            c_0_0 = this.DecorateC(c: c_0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::C>(c_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
        });
    }
}");
        }

        [Fact]
        public void ImportsPublicStaticInstanceFieldInstancePropertyFactoryAndDecoratorFromBaseClass()
        {
            string userSource = @"
using StrongInject;

public class A {}
public class B {}
public class C {}

public class Module
{
    [Instance] public static A A = new A();
    [Instance] public static B B => new B();
    [Factory] public static C CreateC(A a, B b) => new C();
    [DecoratorFactory] public static C DecorateC(C c) => c;
}

public partial class Container : Module, IContainer<C>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::C>.Run<TResult, TParam>(global::System.Func<global::C, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::B b_0_3;
        global::C c_0_1;
        global::C c_0_0;
        a_0_2 = global::Module.A;
        b_0_3 = global::Module.B;
        c_0_1 = global::Module.CreateC(a: a_0_2, b: b_0_3);
        try
        {
            c_0_0 = global::Module.DecorateC(c: c_0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(c_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::C> global::StrongInject.IContainer<global::C>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::B b_0_3;
        global::C c_0_1;
        global::C c_0_0;
        a_0_2 = global::Module.A;
        b_0_3 = global::Module.B;
        c_0_1 = global::Module.CreateC(a: a_0_2, b: b_0_3);
        try
        {
            c_0_0 = global::Module.DecorateC(c: c_0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::C>(c_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
        });
    }
}");
        }

        [Fact]
        public void ImportsProtectedStaticInstanceFieldInstancePropertyFactoryAndDecoratorFromBaseClass()
        {
            string userSource = @"
using StrongInject;

public class A {}
public class B {}
public class C {}

public class Module
{
    [Instance] protected internal static A A = new A();
    [Instance] protected static B B => new B();
    [Factory] protected internal static C CreateC(A a, B b) => new C();
    [DecoratorFactory] protected static C DecorateC(C c) => c;
}

public partial class Container : Module, IContainer<C>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::C>.Run<TResult, TParam>(global::System.Func<global::C, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::B b_0_3;
        global::C c_0_1;
        global::C c_0_0;
        a_0_2 = global::Module.A;
        b_0_3 = global::Module.B;
        c_0_1 = global::Module.CreateC(a: a_0_2, b: b_0_3);
        try
        {
            c_0_0 = global::Module.DecorateC(c: c_0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(c_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::C> global::StrongInject.IContainer<global::C>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::B b_0_3;
        global::C c_0_1;
        global::C c_0_0;
        a_0_2 = global::Module.A;
        b_0_3 = global::Module.B;
        c_0_1 = global::Module.CreateC(a: a_0_2, b: b_0_3);
        try
        {
            c_0_0 = global::Module.DecorateC(c: c_0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::C>(c_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(c_0_1);
        });
    }
}");
        }

        [Fact]
        public void WarningIfInstanceFieldInstancePropertyFactoryAndDecoratorAreNotPublicStaticOrProtected()
        {
            string userSource = @"
using StrongInject;

public class A {}

public class Module
{
    [Instance] public A A1 = new A();
    [Instance] static A A2 => new A();
    [Factory] private protected A CreateA() => new A();
    [DecoratorFactory] internal A DecorateA(A a) => a;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (8,6): Warning SI1004: Instance field 'Module.A1' is not either public and static, or protected, and containing module 'Module' is not a container, so will be ignored.
                // Instance
                new DiagnosticResult("SI1004", @"Instance", DiagnosticSeverity.Warning).WithLocation(8, 6),
                // (9,6): Warning SI1004: Instance property 'Module.A2' is not either public and static, or protected, and containing module 'Module' is not a container, so will be ignored.
                // Instance
                new DiagnosticResult("SI1004", @"Instance", DiagnosticSeverity.Warning).WithLocation(9, 6),
                // (10,6): Warning SI1002: Factory method 'Module.CreateA()' is not either public and static, or protected, and containing module 'Module' is not a container, so will be ignored.
                // Factory
                new DiagnosticResult("SI1002", @"Factory", DiagnosticSeverity.Warning).WithLocation(10, 6),
                // (11,6): Warning SI1002: Factory method 'Module.DecorateA(A)' is not either public and static, or protected, and containing module 'Module' is not a container, so will be ignored.
                // DecoratorFactory
                new DiagnosticResult("SI1002", @"DecoratorFactory", DiagnosticSeverity.Warning).WithLocation(11, 6));
            comp.GetDiagnostics().Verify();
            Assert.Empty(generated);
        }

        [Fact]
        public void OptionalParametersInTypeConstructor()
        {
            string userSource = @"
using StrongInject;

public class A { public A(B b = null, C c = null, string s  = """", D d = null,  int i = 5){} }
public class B {}
public class C {}
public class D {}

[Register(typeof(A))]
[Register(typeof(C))]
[Register(typeof(D))]
public partial class Container : IContainer<A>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (12,22): Info SI2100: Info about resolving dependencies for 'A': We have no source for instance of type 'B' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(12, 22),
                // (12,22): Info SI2100: Info about resolving dependencies for 'A': We have no source for instance of type 'string' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(12, 22),
                // (12,22): Info SI2100: Info about resolving dependencies for 'A': We have no source for instance of type 'int' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(12, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::D d_0_2;
        global::A a_0_0;
        c_0_1 = new global::C();
        d_0_2 = new global::D();
        a_0_0 = new global::A(c: c_0_1, d: d_0_2);
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::D d_0_2;
        global::A a_0_0;
        c_0_1 = new global::C();
        d_0_2 = new global::D();
        a_0_0 = new global::A(c: c_0_1, d: d_0_2);
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void OptionalParametersInDecoratorTypeConstructor()
        {
            string userSource = @"
using StrongInject;

public interface IA {}
public class Impl : IA {}
public class A : IA { public A(IA a, B b = null, C c = null, string s  = """", D d = null,  int i = 5){} }
public class B {}
public class C {}
public class D {}

[Register(typeof(Impl), typeof(IA))]
[RegisterDecorator(typeof(A), typeof(IA))]
[Register(typeof(C))]
[Register(typeof(D))]
public partial class Container : IContainer<IA>
{
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (15,22): Info SI2100: Info about resolving dependencies for 'IA': We have no source for instance of type 'B' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(15, 22),
                // (15,22): Info SI2100: Info about resolving dependencies for 'IA': We have no source for instance of type 'string' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(15, 22),
                // (15,22): Info SI2100: Info about resolving dependencies for 'IA': We have no source for instance of type 'int' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(15, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::IA>.Run<TResult, TParam>(global::System.Func<global::IA, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::Impl impl_0_2;
        global::IA iA_0_1;
        global::C c_0_3;
        global::D d_0_4;
        global::IA iA_0_0;
        impl_0_2 = new global::Impl();
        iA_0_1 = (global::IA)impl_0_2;
        c_0_3 = new global::C();
        d_0_4 = new global::D();
        iA_0_0 = new global::A(a: iA_0_1, c: c_0_3, d: d_0_4);
        TResult result;
        try
        {
            result = func(iA_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::IA> global::StrongInject.IContainer<global::IA>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::Impl impl_0_2;
        global::IA iA_0_1;
        global::C c_0_3;
        global::D d_0_4;
        global::IA iA_0_0;
        impl_0_2 = new global::Impl();
        iA_0_1 = (global::IA)impl_0_2;
        c_0_3 = new global::C();
        d_0_4 = new global::D();
        iA_0_0 = new global::A(a: iA_0_1, c: c_0_3, d: d_0_4);
        return new global::StrongInject.Owned<global::IA>(iA_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void OptionalParametersInFactoryMethod()
        {
            string userSource = @"
using StrongInject;

public class A {}
public class B {}
public class C {}
public class D {}

[Register(typeof(C))]
[Register(typeof(D))]
public partial class Container : IContainer<A>
{
    [Factory] public A CreateA(B b = null, C c = null, string s  = """", D d = null,  int i = 5) => null;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (11,22): Info SI2100: Info about resolving dependencies for 'A': We have no source for instance of type 'B' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(11, 22),
                // (11,22): Info SI2100: Info about resolving dependencies for 'A': We have no source for instance of type 'string' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(11, 22),
                // (11,22): Info SI2100: Info about resolving dependencies for 'A': We have no source for instance of type 'int' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(11, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::D d_0_2;
        global::A a_0_0;
        c_0_1 = new global::C();
        d_0_2 = new global::D();
        a_0_0 = this.CreateA(c: c_0_1, d: d_0_2);
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(a_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::D d_0_2;
        global::A a_0_0;
        c_0_1 = new global::C();
        d_0_2 = new global::D();
        a_0_0 = this.CreateA(c: c_0_1, d: d_0_2);
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(a_0_0);
        });
    }
}");
        }

        [Fact]
        public void OptionalParametersInDecoratorFactoryMethod()
        {
            string userSource = @"
using StrongInject;

public class A {}
public class B {}
public class C {}
public class D {}

[Register(typeof(A))]
[Register(typeof(C))]
[Register(typeof(D))]
public partial class Container : IContainer<A>
{
    [DecoratorFactory] public A CreateA(B b = null, C c = null, string s  = """", D d = null,  int i = 5, A a = null) => null;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (12,22): Info SI2100: Info about resolving dependencies for 'A': We have no source for instance of type 'B' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(12, 22),
                // (12,22): Info SI2100: Info about resolving dependencies for 'A': We have no source for instance of type 'string' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(12, 22),
                // (12,22): Info SI2100: Info about resolving dependencies for 'A': We have no source for instance of type 'int' used in an optional parameter. Using The default value instead.
                // Container
                new DiagnosticResult("SI2100", @"Container", DiagnosticSeverity.Info).WithLocation(12, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::D d_0_2;
        global::A a_0_3;
        global::A a_0_0;
        c_0_1 = new global::C();
        d_0_2 = new global::D();
        a_0_3 = new global::A();
        a_0_0 = this.CreateA(c: c_0_1, d: d_0_2, a: a_0_3);
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::C c_0_1;
        global::D d_0_2;
        global::A a_0_3;
        global::A a_0_0;
        c_0_1 = new global::C();
        d_0_2 = new global::D();
        a_0_3 = new global::A();
        a_0_0 = this.CreateA(c: c_0_1, d: d_0_2, a: a_0_3);
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }
        
        [Fact]
        public void AsyncSingleInstanceCanBeResolvedFromNonAsyncFunc1()
        {
            string userSource = @"
using StrongInject;
using System;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<bool>
{
    [Factory(Scope.SingleInstance)] ValueTask<int> Create() => default;
    [Factory] string Create(int i) => default;
    [Factory] long Create(Func<string> func) => default;
    [Factory] bool Create(int i, long l) => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::System.Int32 _int32Field0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private async global::System.Threading.Tasks.ValueTask<global::System.Int32> GetInt32Field0()
    {
        if (!object.ReferenceEquals(_int32Field0, null))
            return _int32Field0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_1;
            var hasAwaitStarted_int32_0_1 = false;
            var int32_0_0 = default(global::System.Int32);
            int32_0_1 = this.Create();
            try
            {
                hasAwaitStarted_int32_0_1 = true;
                int32_0_0 = await int32_0_1;
            }
            catch
            {
                if (!hasAwaitStarted_int32_0_1)
                {
                    _ = int32_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._int32Field0 = int32_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _int32Field0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Boolean>.RunAsync<TResult, TParam>(global::System.Func<global::System.Boolean, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_1;
        var hasAwaitStarted_int32_0_1 = false;
        var int32_0_2 = default(global::System.Int32);
        global::System.Func<global::System.String> func_0_4;
        global::System.Int64 int64_0_3;
        global::System.Boolean boolean_0_0;
        int32_0_1 = GetInt32Field0();
        try
        {
            hasAwaitStarted_int32_0_1 = true;
            int32_0_2 = await int32_0_1;
            func_0_4 = () =>
            {
                global::System.String string_1_0;
                string_1_0 = this.Create(i: int32_0_2);
                return string_1_0;
            };
            int64_0_3 = this.Create(func: func_0_4);
            boolean_0_0 = this.Create(i: int32_0_2, l: int64_0_3);
        }
        catch
        {
            if (!hasAwaitStarted_int32_0_1)
            {
                _ = int32_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(boolean_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Boolean>> global::StrongInject.IAsyncContainer<global::System.Boolean>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_1;
        var hasAwaitStarted_int32_0_1 = false;
        var int32_0_2 = default(global::System.Int32);
        global::System.Func<global::System.String> func_0_4;
        global::System.Int64 int64_0_3;
        global::System.Boolean boolean_0_0;
        int32_0_1 = GetInt32Field0();
        try
        {
            hasAwaitStarted_int32_0_1 = true;
            int32_0_2 = await int32_0_1;
            func_0_4 = () =>
            {
                global::System.String string_1_0;
                string_1_0 = this.Create(i: int32_0_2);
                return string_1_0;
            };
            int64_0_3 = this.Create(func: func_0_4);
            boolean_0_0 = this.Create(i: int32_0_2, l: int64_0_3);
        }
        catch
        {
            if (!hasAwaitStarted_int32_0_1)
            {
                _ = int32_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::System.Boolean>(boolean_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void AsyncSingleInstanceCanBeResolvedFromNonAsyncFunc2()
        {
            string userSource = @"
using StrongInject;
using System;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<bool>
{
    [Factory(Scope.SingleInstance)] ValueTask<int> Create() => default;
    [Factory] string Create(int i) => default;
    [Factory] long Create(Func<string> func) => default;
    [Factory] bool Create(long l) => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::System.Int32 _int32Field0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private async global::System.Threading.Tasks.ValueTask<global::System.Int32> GetInt32Field0()
    {
        if (!object.ReferenceEquals(_int32Field0, null))
            return _int32Field0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_1;
            var hasAwaitStarted_int32_0_1 = false;
            var int32_0_0 = default(global::System.Int32);
            int32_0_1 = this.Create();
            try
            {
                hasAwaitStarted_int32_0_1 = true;
                int32_0_0 = await int32_0_1;
            }
            catch
            {
                if (!hasAwaitStarted_int32_0_1)
                {
                    _ = int32_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._int32Field0 = int32_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _int32Field0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Boolean>.RunAsync<TResult, TParam>(global::System.Func<global::System.Boolean, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_3;
        var hasAwaitStarted_int32_0_3 = false;
        var int32_0_4 = default(global::System.Int32);
        global::System.Func<global::System.String> func_0_2;
        global::System.Int64 int64_0_1;
        global::System.Boolean boolean_0_0;
        int32_0_3 = GetInt32Field0();
        try
        {
            hasAwaitStarted_int32_0_3 = true;
            int32_0_4 = await int32_0_3;
            func_0_2 = () =>
            {
                global::System.String string_1_0;
                string_1_0 = this.Create(i: int32_0_4);
                return string_1_0;
            };
            int64_0_1 = this.Create(func: func_0_2);
            boolean_0_0 = this.Create(l: int64_0_1);
        }
        catch
        {
            if (!hasAwaitStarted_int32_0_3)
            {
                _ = int32_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(boolean_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Boolean>> global::StrongInject.IAsyncContainer<global::System.Boolean>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_3;
        var hasAwaitStarted_int32_0_3 = false;
        var int32_0_4 = default(global::System.Int32);
        global::System.Func<global::System.String> func_0_2;
        global::System.Int64 int64_0_1;
        global::System.Boolean boolean_0_0;
        int32_0_3 = GetInt32Field0();
        try
        {
            hasAwaitStarted_int32_0_3 = true;
            int32_0_4 = await int32_0_3;
            func_0_2 = () =>
            {
                global::System.String string_1_0;
                string_1_0 = this.Create(i: int32_0_4);
                return string_1_0;
            };
            int64_0_1 = this.Create(func: func_0_2);
            boolean_0_0 = this.Create(l: int64_0_1);
        }
        catch
        {
            if (!hasAwaitStarted_int32_0_3)
            {
                _ = int32_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::System.Boolean>(boolean_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void AsyncSingleInstanceCanBeResolvedFromNonAsyncFunc3()
        {
            string userSource = @"
using StrongInject;
using System;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<bool>
{
    [Factory(Scope.SingleInstance)] ValueTask<int> Create() => default;
    [Factory] string Create(Func<int> i) => default;
    [Factory] long Create(Func<string> func) => default;
    [Factory] bool Create(long l) => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Warning SI1103: Warning while resolving dependencies for 'bool': Return type 'int' of delegate 'System.Func<int>' has a single instance scope and so will always have the same value.
                // Container
                new DiagnosticResult("SI1103", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::System.Int32 _int32Field0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private async global::System.Threading.Tasks.ValueTask<global::System.Int32> GetInt32Field0()
    {
        if (!object.ReferenceEquals(_int32Field0, null))
            return _int32Field0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_1;
            var hasAwaitStarted_int32_0_1 = false;
            var int32_0_0 = default(global::System.Int32);
            int32_0_1 = this.Create();
            try
            {
                hasAwaitStarted_int32_0_1 = true;
                int32_0_0 = await int32_0_1;
            }
            catch
            {
                if (!hasAwaitStarted_int32_0_1)
                {
                    _ = int32_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._int32Field0 = int32_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _int32Field0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Boolean>.RunAsync<TResult, TParam>(global::System.Func<global::System.Boolean, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_3;
        var hasAwaitStarted_int32_0_3 = false;
        var int32_0_4 = default(global::System.Int32);
        global::System.Func<global::System.String> func_0_2;
        global::System.Int64 int64_0_1;
        global::System.Boolean boolean_0_0;
        int32_0_3 = GetInt32Field0();
        try
        {
            hasAwaitStarted_int32_0_3 = true;
            int32_0_4 = await int32_0_3;
            func_0_2 = () =>
            {
                global::System.Func<global::System.Int32> func_1_1;
                global::System.String string_1_0;
                func_1_1 = () =>
                {
                    return int32_0_4;
                };
                string_1_0 = this.Create(i: func_1_1);
                return string_1_0;
            };
            int64_0_1 = this.Create(func: func_0_2);
            boolean_0_0 = this.Create(l: int64_0_1);
        }
        catch
        {
            if (!hasAwaitStarted_int32_0_3)
            {
                _ = int32_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(boolean_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Boolean>> global::StrongInject.IAsyncContainer<global::System.Boolean>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_3;
        var hasAwaitStarted_int32_0_3 = false;
        var int32_0_4 = default(global::System.Int32);
        global::System.Func<global::System.String> func_0_2;
        global::System.Int64 int64_0_1;
        global::System.Boolean boolean_0_0;
        int32_0_3 = GetInt32Field0();
        try
        {
            hasAwaitStarted_int32_0_3 = true;
            int32_0_4 = await int32_0_3;
            func_0_2 = () =>
            {
                global::System.Func<global::System.Int32> func_1_1;
                global::System.String string_1_0;
                func_1_1 = () =>
                {
                    return int32_0_4;
                };
                string_1_0 = this.Create(i: func_1_1);
                return string_1_0;
            };
            int64_0_1 = this.Create(func: func_0_2);
            boolean_0_0 = this.Create(l: int64_0_1);
        }
        catch
        {
            if (!hasAwaitStarted_int32_0_3)
            {
                _ = int32_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::System.Boolean>(boolean_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void AsyncSingleInstanceCanBeResolvedFromNonAsyncFunc4()
        {
            string userSource = @"
using StrongInject;
using System;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<bool>
{
    [Factory(Scope.SingleInstance)] ValueTask<int> Create() => default;
    [Factory(Scope.SingleInstance)] string Create(Func<int> i) => default;
    [Factory] long Create(Func<string> func) => default;
    [Factory] bool Create(long l) => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Warning SI1103: Warning while resolving dependencies for 'bool': Return type 'string' of delegate 'System.Func<string>' has a single instance scope and so will always have the same value.
                // Container
                new DiagnosticResult("SI1103", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 22),
                // (6,22): Warning SI1103: Warning while resolving dependencies for 'bool': Return type 'int' of delegate 'System.Func<int>' has a single instance scope and so will always have the same value.
                // Container
                new DiagnosticResult("SI1103", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }

        await this._lock1.WaitAsync();
        try
        {
            await (this._disposeAction1?.Invoke() ?? default);
        }
        finally
        {
            this._lock1.Release();
        }
    }

    private global::System.String _stringField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private global::System.Int32 _int32Field1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction1;
    private async global::System.Threading.Tasks.ValueTask<global::System.Int32> GetInt32Field1()
    {
        if (!object.ReferenceEquals(_int32Field1, null))
            return _int32Field1;
        await this._lock1.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_1;
            var hasAwaitStarted_int32_0_1 = false;
            var int32_0_0 = default(global::System.Int32);
            int32_0_1 = this.Create();
            try
            {
                hasAwaitStarted_int32_0_1 = true;
                int32_0_0 = await int32_0_1;
            }
            catch
            {
                if (!hasAwaitStarted_int32_0_1)
                {
                    _ = int32_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._int32Field1 = int32_0_0;
            this._disposeAction1 = async () =>
            {
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _int32Field1;
    }

    private async global::System.Threading.Tasks.ValueTask<global::System.String> GetStringField0()
    {
        if (!object.ReferenceEquals(_stringField0, null))
            return _stringField0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_2;
            var hasAwaitStarted_int32_0_2 = false;
            var int32_0_3 = default(global::System.Int32);
            global::System.Func<global::System.Int32> func_0_1;
            global::System.String string_0_0;
            int32_0_2 = GetInt32Field1();
            try
            {
                hasAwaitStarted_int32_0_2 = true;
                int32_0_3 = await int32_0_2;
                func_0_1 = () =>
                {
                    return int32_0_3;
                };
                string_0_0 = this.Create(i: func_0_1);
            }
            catch
            {
                if (!hasAwaitStarted_int32_0_2)
                {
                    _ = int32_0_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._stringField0 = string_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _stringField0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Boolean>.RunAsync<TResult, TParam>(global::System.Func<global::System.Boolean, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::System.String> string_0_3;
        var hasAwaitStarted_string_0_3 = false;
        var string_0_4 = default(global::System.String);
        global::System.Func<global::System.String> func_0_2;
        global::System.Int64 int64_0_1;
        global::System.Boolean boolean_0_0;
        string_0_3 = GetStringField0();
        try
        {
            hasAwaitStarted_string_0_3 = true;
            string_0_4 = await string_0_3;
            func_0_2 = () =>
            {
                return string_0_4;
            };
            int64_0_1 = this.Create(func: func_0_2);
            boolean_0_0 = this.Create(l: int64_0_1);
        }
        catch
        {
            if (!hasAwaitStarted_string_0_3)
            {
                _ = string_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(boolean_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Boolean>> global::StrongInject.IAsyncContainer<global::System.Boolean>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.ValueTask<global::System.String> string_0_3;
        var hasAwaitStarted_string_0_3 = false;
        var string_0_4 = default(global::System.String);
        global::System.Func<global::System.String> func_0_2;
        global::System.Int64 int64_0_1;
        global::System.Boolean boolean_0_0;
        string_0_3 = GetStringField0();
        try
        {
            hasAwaitStarted_string_0_3 = true;
            string_0_4 = await string_0_3;
            func_0_2 = () =>
            {
                return string_0_4;
            };
            int64_0_1 = this.Create(func: func_0_2);
            boolean_0_0 = this.Create(l: int64_0_1);
        }
        catch
        {
            if (!hasAwaitStarted_string_0_3)
            {
                _ = string_0_3.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::System.Boolean>(boolean_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void AsyncSingleInstanceCanBeResolvedFromNonAsyncFunc5()
        {
            string userSource = @"
using StrongInject;
using System;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<bool>
{
    [Factory(Scope.SingleInstance)] ValueTask<int> Create() => default;
    [Factory] string Create(Func<int> i) => default;
    [Factory] long Create(Func<string> func) => default;
    [Factory] bool Create(Func<ValueTask<long>> l) => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Warning SI1103: Warning while resolving dependencies for 'bool': Return type 'int' of delegate 'System.Func<int>' has a single instance scope and so will always have the same value.
                // Container
                new DiagnosticResult("SI1103", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        await this._lock0.WaitAsync();
        try
        {
            await (this._disposeAction0?.Invoke() ?? default);
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::System.Int32 _int32Field0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Func<global::System.Threading.Tasks.ValueTask> _disposeAction0;
    private async global::System.Threading.Tasks.ValueTask<global::System.Int32> GetInt32Field0()
    {
        if (!object.ReferenceEquals(_int32Field0, null))
            return _int32Field0;
        await this._lock0.WaitAsync();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_0_1;
            var hasAwaitStarted_int32_0_1 = false;
            var int32_0_0 = default(global::System.Int32);
            int32_0_1 = this.Create();
            try
            {
                hasAwaitStarted_int32_0_1 = true;
                int32_0_0 = await int32_0_1;
            }
            catch
            {
                if (!hasAwaitStarted_int32_0_1)
                {
                    _ = int32_0_1.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            this._int32Field0 = int32_0_0;
            this._disposeAction0 = async () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _int32Field0;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Boolean>.RunAsync<TResult, TParam>(global::System.Func<global::System.Boolean, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Threading.Tasks.ValueTask<global::System.Int64>> func_0_1;
        global::System.Boolean boolean_0_0;
        func_0_1 = async () =>
        {
            global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_1_2;
            var hasAwaitStarted_int32_1_2 = false;
            var int32_1_3 = default(global::System.Int32);
            global::System.Func<global::System.String> func_1_1;
            global::System.Int64 int64_1_0;
            int32_1_2 = GetInt32Field0();
            try
            {
                hasAwaitStarted_int32_1_2 = true;
                int32_1_3 = await int32_1_2;
                func_1_1 = () =>
                {
                    global::System.Func<global::System.Int32> func_2_1;
                    global::System.String string_2_0;
                    func_2_1 = () =>
                    {
                        return int32_1_3;
                    };
                    string_2_0 = this.Create(i: func_2_1);
                    return string_2_0;
                };
                int64_1_0 = this.Create(func: func_1_1);
            }
            catch
            {
                if (!hasAwaitStarted_int32_1_2)
                {
                    _ = int32_1_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            return int64_1_0;
        };
        boolean_0_0 = this.Create(l: func_0_1);
        TResult result;
        try
        {
            result = await func(boolean_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Boolean>> global::StrongInject.IAsyncContainer<global::System.Boolean>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Func<global::System.Threading.Tasks.ValueTask<global::System.Int64>> func_0_1;
        global::System.Boolean boolean_0_0;
        func_0_1 = async () =>
        {
            global::System.Threading.Tasks.ValueTask<global::System.Int32> int32_1_2;
            var hasAwaitStarted_int32_1_2 = false;
            var int32_1_3 = default(global::System.Int32);
            global::System.Func<global::System.String> func_1_1;
            global::System.Int64 int64_1_0;
            int32_1_2 = GetInt32Field0();
            try
            {
                hasAwaitStarted_int32_1_2 = true;
                int32_1_3 = await int32_1_2;
                func_1_1 = () =>
                {
                    global::System.Func<global::System.Int32> func_2_1;
                    global::System.String string_2_0;
                    func_2_1 = () =>
                    {
                        return int32_1_3;
                    };
                    string_2_0 = this.Create(i: func_2_1);
                    return string_2_0;
                };
                int64_1_0 = this.Create(func: func_1_1);
            }
            catch
            {
                if (!hasAwaitStarted_int32_1_2)
                {
                    _ = int32_1_2.AsTask().ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
                }

                throw;
            }

            return int64_1_0;
        };
        boolean_0_0 = this.Create(l: func_0_1);
        return new global::StrongInject.AsyncOwned<global::System.Boolean>(boolean_0_0, async () =>
        {
        });
    }
}");
        }

        [Fact]
        public void AsyncSingleInstanceCannotBeResolvedFromAsyncFuncIfContainerIsNonAsync()
        {
            string userSource = @"
using StrongInject;
using System;
using System.Threading.Tasks;

public partial class Container : IContainer<bool>
{
    [Factory(Scope.SingleInstance)] ValueTask<int> Create() => default;
    [Factory] string Create(Func<int> i) => default;
    [Factory] long Create(string func) => default;
    [Factory] bool Create(long l) => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,22): Warning SI1103: Warning while resolving dependencies for 'bool': Return type 'int' of delegate 'System.Func<int>' has a single instance scope and so will always have the same value.
                // Container
                new DiagnosticResult("SI1103", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 22),
                // (6,22): Error SI0103: Error while resolving dependencies for 'bool': 'int' can only be resolved asynchronously.
                // Container
                new DiagnosticResult("SI0103", @"Container", DiagnosticSeverity.Error).WithLocation(6, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Boolean>.Run<TResult, TParam>(global::System.Func<global::System.Boolean, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Boolean> global::StrongInject.IContainer<global::System.Boolean>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void UseDelegateParameterBugInV_1_0_2()
        {
            string userSource = @"
using StrongInject;
using System;

public class BaseViewModel { }
public class ItemsViewModel : BaseViewModel
{
    public ItemsViewModel(
        INavigationService<ItemDetailViewModel> itemDetailNavigationService,
        INavigationService<NewItemViewModel> newItemNavigationService,
        Func<Item, ItemDetailViewModel> createItemDetailViewModel,
        Func<NewItemViewModel> createNewItemViewModel,
        IDataStore<Item> dataStore)
    { }
}

public interface INavigationService
{
}

public interface INavigationService<T> : INavigationService where T : BaseViewModel
{
}

public class NavigationService : INavigationService
{
    public NavigationService(INavigation navigation) { }
}

public class NavigationService<T> : NavigationService, INavigationService<T> where T : BaseViewModel
{
    public NavigationService(INavigation navigation, Func<T, IViewOf<T>> createView) : base(navigation) { }
}

public class ItemDetailViewModel : BaseViewModel
{
    public ItemDetailViewModel(Item item) { }
}

public interface IViewOf<T> where T : BaseViewModel { }

public class ItemDetailPage : IViewOf<ItemDetailViewModel>
{
    public ItemDetailPage(ItemDetailViewModel itemDetailViewModel) { }
}

public class NewItemViewModel : BaseViewModel
{
    public NewItemViewModel(IDataStore<Item> dataStore, INavigationService navigationService) { }
}

public class NewItemPage : IViewOf<NewItemViewModel>
{
    public NewItemPage(NewItemViewModel newItemViewModel) { }
}

public class MockDataStore : IDataStore<Item> { }

public interface IDataStore<T> { }

public class Item { }

public interface INavigation
{
}

[Register(typeof(ItemsViewModel))]
[Register(typeof(NavigationService), Scope.SingleInstance, typeof(INavigationService))]
[Register(typeof(ItemDetailViewModel))]
[Register(typeof(ItemDetailPage), typeof(IViewOf<ItemDetailViewModel>))]
[Register(typeof(NewItemViewModel))]
[Register(typeof(NewItemPage), typeof(IViewOf<NewItemViewModel>))]
[Register(typeof(MockDataStore), Scope.SingleInstance, typeof(IDataStore<Item>))]
public partial class Container : IContainer<ItemsViewModel>
{
    [Factory(Scope.SingleInstance)]
    INavigationService<T> CreateNavigationService<T>(INavigation navigation, Func<T, IViewOf<T>> createView) where T : BaseViewModel
        => new NavigationService<T>(navigation, createView);
    [Instance] INavigation Navigation => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock3.Wait();
        try
        {
            this._disposeAction3?.Invoke();
        }
        finally
        {
            this._lock3.Release();
        }

        this._lock2.Wait();
        try
        {
            this._disposeAction2?.Invoke();
        }
        finally
        {
            this._lock2.Release();
        }

        this._lock1.Wait();
        try
        {
            this._disposeAction1?.Invoke();
        }
        finally
        {
            this._lock1.Release();
        }

        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::INavigationService<global::ItemDetailViewModel> _iNavigationServiceField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::INavigationService<global::ItemDetailViewModel> GetINavigationServiceField0()
    {
        if (!object.ReferenceEquals(_iNavigationServiceField0, null))
            return _iNavigationServiceField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::INavigation iNavigation_0_1;
            global::System.Func<global::ItemDetailViewModel, global::IViewOf<global::ItemDetailViewModel>> func_0_2;
            global::INavigationService<global::ItemDetailViewModel> iNavigationService_0_0;
            iNavigation_0_1 = this.Navigation;
            func_0_2 = (param0_0) =>
            {
                global::ItemDetailPage itemDetailPage_1_1;
                global::IViewOf<global::ItemDetailViewModel> iViewOf_1_0;
                itemDetailPage_1_1 = new global::ItemDetailPage(itemDetailViewModel: param0_0);
                iViewOf_1_0 = (global::IViewOf<global::ItemDetailViewModel>)itemDetailPage_1_1;
                return iViewOf_1_0;
            };
            iNavigationService_0_0 = this.CreateNavigationService<global::ItemDetailViewModel>(navigation: iNavigation_0_1, createView: func_0_2);
            this._iNavigationServiceField0 = iNavigationService_0_0;
            this._disposeAction0 = () =>
            {
                global::StrongInject.Helpers.Dispose(iNavigationService_0_0);
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _iNavigationServiceField0;
    }

    private global::INavigationService<global::NewItemViewModel> _iNavigationServiceField1;
    private global::System.Threading.SemaphoreSlim _lock1 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction1;
    private global::INavigationService<global::NewItemViewModel> GetINavigationServiceField1()
    {
        if (!object.ReferenceEquals(_iNavigationServiceField1, null))
            return _iNavigationServiceField1;
        this._lock1.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::INavigation iNavigation_0_1;
            global::System.Func<global::NewItemViewModel, global::IViewOf<global::NewItemViewModel>> func_0_2;
            global::INavigationService<global::NewItemViewModel> iNavigationService_0_0;
            iNavigation_0_1 = this.Navigation;
            func_0_2 = (param0_0) =>
            {
                global::NewItemPage newItemPage_1_1;
                global::IViewOf<global::NewItemViewModel> iViewOf_1_0;
                newItemPage_1_1 = new global::NewItemPage(newItemViewModel: param0_0);
                iViewOf_1_0 = (global::IViewOf<global::NewItemViewModel>)newItemPage_1_1;
                return iViewOf_1_0;
            };
            iNavigationService_0_0 = this.CreateNavigationService<global::NewItemViewModel>(navigation: iNavigation_0_1, createView: func_0_2);
            this._iNavigationServiceField1 = iNavigationService_0_0;
            this._disposeAction1 = () =>
            {
                global::StrongInject.Helpers.Dispose(iNavigationService_0_0);
            };
        }
        finally
        {
            this._lock1.Release();
        }

        return _iNavigationServiceField1;
    }

    private global::MockDataStore _mockDataStoreField2;
    private global::System.Threading.SemaphoreSlim _lock2 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction2;
    private global::MockDataStore GetMockDataStoreField2()
    {
        if (!object.ReferenceEquals(_mockDataStoreField2, null))
            return _mockDataStoreField2;
        this._lock2.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::MockDataStore mockDataStore_0_0;
            mockDataStore_0_0 = new global::MockDataStore();
            this._mockDataStoreField2 = mockDataStore_0_0;
            this._disposeAction2 = () =>
            {
            };
        }
        finally
        {
            this._lock2.Release();
        }

        return _mockDataStoreField2;
    }

    private global::NavigationService _navigationServiceField3;
    private global::System.Threading.SemaphoreSlim _lock3 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction3;
    private global::NavigationService GetNavigationServiceField3()
    {
        if (!object.ReferenceEquals(_navigationServiceField3, null))
            return _navigationServiceField3;
        this._lock3.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::INavigation iNavigation_0_1;
            global::NavigationService navigationService_0_0;
            iNavigation_0_1 = this.Navigation;
            navigationService_0_0 = new global::NavigationService(navigation: iNavigation_0_1);
            this._navigationServiceField3 = navigationService_0_0;
            this._disposeAction3 = () =>
            {
            };
        }
        finally
        {
            this._lock3.Release();
        }

        return _navigationServiceField3;
    }

    TResult global::StrongInject.IContainer<global::ItemsViewModel>.Run<TResult, TParam>(global::System.Func<global::ItemsViewModel, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::INavigationService<global::ItemDetailViewModel> iNavigationService_0_1;
        global::INavigationService<global::NewItemViewModel> iNavigationService_0_2;
        global::System.Func<global::Item, global::ItemDetailViewModel> func_0_3;
        global::System.Func<global::NewItemViewModel> func_0_4;
        global::MockDataStore mockDataStore_0_6;
        global::IDataStore<global::Item> iDataStore_0_5;
        global::ItemsViewModel itemsViewModel_0_0;
        iNavigationService_0_1 = GetINavigationServiceField0();
        iNavigationService_0_2 = GetINavigationServiceField1();
        func_0_3 = (param0_0) =>
        {
            global::ItemDetailViewModel itemDetailViewModel_1_0;
            itemDetailViewModel_1_0 = new global::ItemDetailViewModel(item: param0_0);
            return itemDetailViewModel_1_0;
        };
        func_0_4 = () =>
        {
            global::MockDataStore mockDataStore_0_2;
            global::IDataStore<global::Item> iDataStore_1_1;
            global::NavigationService navigationService_0_4;
            global::INavigationService iNavigationService_1_3;
            global::NewItemViewModel newItemViewModel_1_0;
            mockDataStore_0_2 = GetMockDataStoreField2();
            iDataStore_1_1 = (global::IDataStore<global::Item>)mockDataStore_0_2;
            navigationService_0_4 = GetNavigationServiceField3();
            iNavigationService_1_3 = (global::INavigationService)navigationService_0_4;
            newItemViewModel_1_0 = new global::NewItemViewModel(dataStore: iDataStore_1_1, navigationService: iNavigationService_1_3);
            return newItemViewModel_1_0;
        };
        mockDataStore_0_6 = GetMockDataStoreField2();
        iDataStore_0_5 = (global::IDataStore<global::Item>)mockDataStore_0_6;
        itemsViewModel_0_0 = new global::ItemsViewModel(itemDetailNavigationService: iNavigationService_0_1, newItemNavigationService: iNavigationService_0_2, createItemDetailViewModel: func_0_3, createNewItemViewModel: func_0_4, dataStore: iDataStore_0_5);
        TResult result;
        try
        {
            result = func(itemsViewModel_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::ItemsViewModel> global::StrongInject.IContainer<global::ItemsViewModel>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::INavigationService<global::ItemDetailViewModel> iNavigationService_0_1;
        global::INavigationService<global::NewItemViewModel> iNavigationService_0_2;
        global::System.Func<global::Item, global::ItemDetailViewModel> func_0_3;
        global::System.Func<global::NewItemViewModel> func_0_4;
        global::MockDataStore mockDataStore_0_6;
        global::IDataStore<global::Item> iDataStore_0_5;
        global::ItemsViewModel itemsViewModel_0_0;
        iNavigationService_0_1 = GetINavigationServiceField0();
        iNavigationService_0_2 = GetINavigationServiceField1();
        func_0_3 = (param0_0) =>
        {
            global::ItemDetailViewModel itemDetailViewModel_1_0;
            itemDetailViewModel_1_0 = new global::ItemDetailViewModel(item: param0_0);
            return itemDetailViewModel_1_0;
        };
        func_0_4 = () =>
        {
            global::MockDataStore mockDataStore_0_2;
            global::IDataStore<global::Item> iDataStore_1_1;
            global::NavigationService navigationService_0_4;
            global::INavigationService iNavigationService_1_3;
            global::NewItemViewModel newItemViewModel_1_0;
            mockDataStore_0_2 = GetMockDataStoreField2();
            iDataStore_1_1 = (global::IDataStore<global::Item>)mockDataStore_0_2;
            navigationService_0_4 = GetNavigationServiceField3();
            iNavigationService_1_3 = (global::INavigationService)navigationService_0_4;
            newItemViewModel_1_0 = new global::NewItemViewModel(dataStore: iDataStore_1_1, navigationService: iNavigationService_1_3);
            return newItemViewModel_1_0;
        };
        mockDataStore_0_6 = GetMockDataStoreField2();
        iDataStore_0_5 = (global::IDataStore<global::Item>)mockDataStore_0_6;
        itemsViewModel_0_0 = new global::ItemsViewModel(itemDetailNavigationService: iNavigationService_0_1, newItemNavigationService: iNavigationService_0_2, createItemDetailViewModel: func_0_3, createNewItemViewModel: func_0_4, dataStore: iDataStore_0_5);
        return new global::StrongInject.Owned<global::ItemsViewModel>(itemsViewModel_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void RegressionTest_FalseCircularDependencyBug()
        {
            string userSource = @"
using StrongInject;
using System;

[Register(typeof(A))]
[Register(typeof(B), Scope.SingleInstance)]
[Register(typeof(C))]
public partial class Container : IContainer<A>
{
}

public class A { public A(B b, Func<C> c){} }
public class B { }
public class C { public C(B b1, B b2){} }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::B _bField0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::B GetBField0()
    {
        if (!object.ReferenceEquals(_bField0, null))
            return _bField0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::B b_0_0;
            b_0_0 = new global::B();
            this._bField0 = b_0_0;
            this._disposeAction0 = () =>
            {
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _bField0;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::System.Func<global::C> func_0_2;
        global::A a_0_0;
        b_0_1 = GetBField0();
        func_0_2 = () =>
        {
            global::C c_1_0;
            c_1_0 = new global::C(b1: b_0_1, b2: b_0_1);
            return c_1_0;
        };
        a_0_0 = new global::A(b: b_0_1, c: func_0_2);
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::B b_0_1;
        global::System.Func<global::C> func_0_2;
        global::A a_0_0;
        b_0_1 = GetBField0();
        func_0_2 = () =>
        {
            global::C c_1_0;
            c_1_0 = new global::C(b1: b_0_1, b2: b_0_1);
            return c_1_0;
        };
        a_0_0 = new global::A(b: b_0_1, c: func_0_2);
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void InternalTypeCanBeUsedByInternalContainer()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
internal partial class Container : IContainer<A>
{
}

internal class A { }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void InternalTypeCanBeUsedByInternalModuleWhichCanBeUsedByInternalContainer()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
internal class Module {}

[RegisterModule(typeof(Module))]
internal partial class Container : IContainer<A>
{
}

internal class A { }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WarnWhenInternalTypeUsedByPublicContainer1()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
public partial class Container : IContainer<A>
{
}

internal class A { }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,2): Warning SI1005: 'A' is not public, but is registered with public module 'Container'. If 'Container' is imported outside this assembly this may result in errors. Try making 'Container' internal.
                // Register(typeof(A))
                new DiagnosticResult("SI1005", @"Register(typeof(A))", DiagnosticSeverity.Warning).WithLocation(4, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WarnWhenInternalTypeUsedByPublicContainer2()
        {
            string userSource = @"
using StrongInject;

[RegisterFactory(typeof(A))]
public partial class Container : IContainer<int>
{
}

internal class A : IFactory<int> { public int Create() => 42; }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,2): Warning SI1005: 'A' is not public, but is registered with public module 'Container'. If 'Container' is imported outside this assembly this may result in errors. Try making 'Container' internal.
                // RegisterFactory(typeof(A))
                new DiagnosticResult("SI1005", @"RegisterFactory(typeof(A))", DiagnosticSeverity.Warning).WithLocation(4, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::StrongInject.IFactory<global::System.Int32> iFactory_0_1;
        global::System.Int32 int32_0_0;
        a_0_2 = new global::A();
        iFactory_0_1 = (global::StrongInject.IFactory<global::System.Int32>)a_0_2;
        int32_0_0 = iFactory_0_1.Create();
        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
            iFactory_0_1.Release(int32_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_2;
        global::StrongInject.IFactory<global::System.Int32> iFactory_0_1;
        global::System.Int32 int32_0_0;
        a_0_2 = new global::A();
        iFactory_0_1 = (global::StrongInject.IFactory<global::System.Int32>)a_0_2;
        int32_0_0 = iFactory_0_1.Create();
        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
            iFactory_0_1.Release(int32_0_0);
        });
    }
}");
        }

        [Fact]
        public void WarnWhenInternalTypeUsedByPublicModule()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
public class Module {}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

internal class A { }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,2): Warning SI1005: 'A' is not public, but is registered with public module 'Module'. If 'Module' is imported outside this assembly this may result in errors. Try making 'Module' internal.
                // Register(typeof(A))
                new DiagnosticResult("SI1005", @"Register(typeof(A))", DiagnosticSeverity.Warning).WithLocation(4, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void WarnWhenInternalModuleUsedByPublicModule()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
internal class Module {}

[RegisterModule(typeof(Module))]
public partial class Container : IContainer<A>
{
}

public class A { }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (7,2): Warning SI1006: 'Module' is not public, but is imported by public module 'Container'. If 'Container' is imported outside this assembly this may result in errors. Try making 'Container' internal.
                // RegisterModule(typeof(Module))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(Module))", DiagnosticSeverity.Warning).WithLocation(7, 2));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::A>.Run<TResult, TParam>(global::System.Func<global::A, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        TResult result;
        try
        {
            result = func(a_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::A> global::StrongInject.IContainer<global::A>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::A a_0_0;
        a_0_0 = new global::A();
        return new global::StrongInject.Owned<global::A>(a_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ErrorWhenLessThanInternallyVisibleTypeUsedByContainer()
        {
            string userSource = @"
using StrongInject;

public partial class Outer
{
    protected class A { }
    private class B { }
    private protected class C { }

    private class Inner
    {
        internal class D { }
    }

    [Register(typeof(A))]
    [Register(typeof(B))]
    [Register(typeof(C))]
    [Register(typeof(Inner.D))]
    internal partial class Container : IContainer<A>, IContainer<B>, IContainer<C>, IContainer<Inner.D>
    {
    }
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (15,6): Error SI0026: 'Outer.A' must have at least internal Accessibility. Try making it internal or public.
                // Register(typeof(A))
                new DiagnosticResult("SI0026", @"Register(typeof(A))", DiagnosticSeverity.Error).WithLocation(15, 6),
                // (16,6): Error SI0026: 'Outer.B' must have at least internal Accessibility. Try making it internal or public.
                // Register(typeof(B))
                new DiagnosticResult("SI0026", @"Register(typeof(B))", DiagnosticSeverity.Error).WithLocation(16, 6),
                // (17,6): Error SI0026: 'Outer.C' must have at least internal Accessibility. Try making it internal or public.
                // Register(typeof(C))
                new DiagnosticResult("SI0026", @"Register(typeof(C))", DiagnosticSeverity.Error).WithLocation(17, 6),
                // (18,6): Error SI0026: 'Outer.Inner.D' must have at least internal Accessibility. Try making it internal or public.
                // Register(typeof(Inner.D))
                new DiagnosticResult("SI0026", @"Register(typeof(Inner.D))", DiagnosticSeverity.Error).WithLocation(18, 6),
                // (19,28): Error SI0102: Error while resolving dependencies for 'Outer.A': We have no source for instance of type 'Outer.A'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(19, 28),
                // (19,28): Error SI0102: Error while resolving dependencies for 'Outer.B': We have no source for instance of type 'Outer.B'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(19, 28),
                // (19,28): Error SI0102: Error while resolving dependencies for 'Outer.C': We have no source for instance of type 'Outer.C'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(19, 28),
                // (19,28): Error SI0102: Error while resolving dependencies for 'Outer.Inner.D': We have no source for instance of type 'Outer.Inner.D'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(19, 28));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Outer
{
    partial class Container
    {
        private int _disposed = 0;
        private bool Disposed => _disposed != 0;
        public void Dispose()
        {
            var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
            if (disposed != 0)
                return;
        }

        TResult global::StrongInject.IContainer<global::Outer.A>.Run<TResult, TParam>(global::System.Func<global::Outer.A, TParam, TResult> func, TParam param)
        {
            throw new global::System.NotImplementedException();
        }

        global::StrongInject.Owned<global::Outer.A> global::StrongInject.IContainer<global::Outer.A>.Resolve()
        {
            throw new global::System.NotImplementedException();
        }

        TResult global::StrongInject.IContainer<global::Outer.B>.Run<TResult, TParam>(global::System.Func<global::Outer.B, TParam, TResult> func, TParam param)
        {
            throw new global::System.NotImplementedException();
        }

        global::StrongInject.Owned<global::Outer.B> global::StrongInject.IContainer<global::Outer.B>.Resolve()
        {
            throw new global::System.NotImplementedException();
        }

        TResult global::StrongInject.IContainer<global::Outer.C>.Run<TResult, TParam>(global::System.Func<global::Outer.C, TParam, TResult> func, TParam param)
        {
            throw new global::System.NotImplementedException();
        }

        global::StrongInject.Owned<global::Outer.C> global::StrongInject.IContainer<global::Outer.C>.Resolve()
        {
            throw new global::System.NotImplementedException();
        }

        TResult global::StrongInject.IContainer<global::Outer.Inner.D>.Run<TResult, TParam>(global::System.Func<global::Outer.Inner.D, TParam, TResult> func, TParam param)
        {
            throw new global::System.NotImplementedException();
        }

        global::StrongInject.Owned<global::Outer.Inner.D> global::StrongInject.IContainer<global::Outer.Inner.D>.Resolve()
        {
            throw new global::System.NotImplementedException();
        }
    }
}");
        }

        [Fact]
        public void ErrorOnPrivateModule()
        {
            string userSource = @"
using StrongInject;

internal partial class Outer
{
    internal class A { }

    [Register(typeof(A))]
    private class Module { }

    [RegisterModule(typeof(Module))]
    public partial class Container : IContainer<A>
    {
    }
}
";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (9,19): Error SI0401: Module 'Outer.Module' must be public or internal.
                // Module
                new DiagnosticResult("SI0401", @"Module", DiagnosticSeverity.Error).WithLocation(9, 19));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Outer
{
    partial class Container
    {
        private int _disposed = 0;
        private bool Disposed => _disposed != 0;
        public void Dispose()
        {
            var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
            if (disposed != 0)
                return;
        }

        TResult global::StrongInject.IContainer<global::Outer.A>.Run<TResult, TParam>(global::System.Func<global::Outer.A, TParam, TResult> func, TParam param)
        {
            if (Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::Outer.A a_0_0;
            a_0_0 = new global::Outer.A();
            TResult result;
            try
            {
                result = func(a_0_0, param);
            }
            finally
            {
            }

            return result;
        }

        global::StrongInject.Owned<global::Outer.A> global::StrongInject.IContainer<global::Outer.A>.Resolve()
        {
            if (Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::Outer.A a_0_0;
            a_0_0 = new global::Outer.A();
            return new global::StrongInject.Owned<global::Outer.A>(a_0_0, () =>
            {
            });
        }
    }
}");
        }

        [Fact]
        public void WarnWhenInternalTypeUsedByMoreThanInternallyVisibleModule()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
public class Module1 {}

public class Outer1 {

    [Register(typeof(A))]
    protected class Module2 {}

    [Register(typeof(A))]
    protected internal class Module3 {}

    protected class Inner
    {
        [Register(typeof(A))]
        protected internal class Module4 {}
    }
}

internal class A { }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (4,2): Warning SI1005: 'A' is not public, but is registered with public module 'Module1'. If 'Module1' is imported outside this assembly this may result in errors. Try making 'Module1' internal.
                // Register(typeof(A))
                new DiagnosticResult("SI1005", @"Register(typeof(A))", DiagnosticSeverity.Warning).WithLocation(4, 2),
                // (9,6): Warning SI1005: 'A' is not public, but is registered with public module 'Outer1.Module2'. If 'Outer1.Module2' is imported outside this assembly this may result in errors. Try making 'Outer1.Module2' internal.
                // Register(typeof(A))
                new DiagnosticResult("SI1005", @"Register(typeof(A))", DiagnosticSeverity.Warning).WithLocation(9, 6),
                // (10,21): Error SI0401: Module 'Outer1.Module2' must be public or internal.
                // Module2
                new DiagnosticResult("SI0401", @"Module2", DiagnosticSeverity.Error).WithLocation(10, 21),
                // (12,6): Warning SI1005: 'A' is not public, but is registered with public module 'Outer1.Module3'. If 'Outer1.Module3' is imported outside this assembly this may result in errors. Try making 'Outer1.Module3' internal.
                // Register(typeof(A))
                new DiagnosticResult("SI1005", @"Register(typeof(A))", DiagnosticSeverity.Warning).WithLocation(12, 6),
                // (13,30): Error SI0401: Module 'Outer1.Module3' must be public or internal.
                // Module3
                new DiagnosticResult("SI0401", @"Module3", DiagnosticSeverity.Error).WithLocation(13, 30),
                // (17,10): Warning SI1005: 'A' is not public, but is registered with public module 'Outer1.Inner.Module4'. If 'Outer1.Inner.Module4' is imported outside this assembly this may result in errors. Try making 'Outer1.Inner.Module4' internal.
                // Register(typeof(A))
                new DiagnosticResult("SI1005", @"Register(typeof(A))", DiagnosticSeverity.Warning).WithLocation(17, 10),
                // (18,34): Error SI0401: Module 'Outer1.Inner.Module4' must be public or internal.
                // Module4
                new DiagnosticResult("SI0401", @"Module4", DiagnosticSeverity.Error).WithLocation(18, 34));
            comp.GetDiagnostics().Verify();
            Assert.Empty(generated);
        }

        [Fact]
        public void NoWarningWhenInternalTypeUsedByAtMostInternallyVisibleModule()
        {
            string userSource = @"
using StrongInject;

[Register(typeof(A))]
internal class Module1 {}

internal class Outer1 {

    [Register(typeof(A))]
    public class Module2 {}

    public class Inner
    {
        [Register(typeof(A))]
        public class Module3 {}
    }
}

internal class A { }";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            Assert.Empty(generated);
        }

        [Fact]
        public void WarnWhenAtMostInternallyVisibleModuleImportedByMoreThanInternallyVisibleModule()
        {
            string userSource = @"
using StrongInject;

internal class InternalModule1 {}

internal class InternalOuter {

    public class InternalModule2 {}

    public class Inner
    {
        public class InternalModule3 {}
    }
}

[RegisterModule(typeof(InternalModule1))]
[RegisterModule(typeof(InternalOuter.InternalModule2))]
[RegisterModule(typeof(InternalOuter.Inner.InternalModule3))]
public class PublicModule1 {}

public class PublicOuter {

    [RegisterModule(typeof(InternalModule1))]
    [RegisterModule(typeof(InternalOuter.InternalModule2))]
    [RegisterModule(typeof(InternalOuter.Inner.InternalModule3))]
    [RegisterModule(typeof(InternalModule4))]
    protected class PublicModule2 {}

    [RegisterModule(typeof(InternalModule1))]
    [RegisterModule(typeof(InternalOuter.InternalModule2))]
    [RegisterModule(typeof(InternalOuter.Inner.InternalModule3))]
    [RegisterModule(typeof(InternalModule4))]
    protected internal class PublicModule3 {}

    protected class Inner
    {
        [RegisterModule(typeof(InternalModule1))]
        [RegisterModule(typeof(InternalOuter.InternalModule2))]
        [RegisterModule(typeof(InternalOuter.Inner.InternalModule3))]
        [RegisterModule(typeof(InternalModule4))]
        protected internal class PublicModule4 {}
    }

    private class InternalModule4 {}
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (16,2): Warning SI1006: 'InternalModule1' is not public, but is imported by public module 'PublicModule1'. If 'PublicModule1' is imported outside this assembly this may result in errors. Try making 'PublicModule1' internal.
                // RegisterModule(typeof(InternalModule1))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalModule1))", DiagnosticSeverity.Warning).WithLocation(16, 2),
                // (17,2): Warning SI1006: 'InternalOuter.InternalModule2' is not public, but is imported by public module 'PublicModule1'. If 'PublicModule1' is imported outside this assembly this may result in errors. Try making 'PublicModule1' internal.
                // RegisterModule(typeof(InternalOuter.InternalModule2))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalOuter.InternalModule2))", DiagnosticSeverity.Warning).WithLocation(17, 2),
                // (18,2): Warning SI1006: 'InternalOuter.Inner.InternalModule3' is not public, but is imported by public module 'PublicModule1'. If 'PublicModule1' is imported outside this assembly this may result in errors. Try making 'PublicModule1' internal.
                // RegisterModule(typeof(InternalOuter.Inner.InternalModule3))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalOuter.Inner.InternalModule3))", DiagnosticSeverity.Warning).WithLocation(18, 2),
                // (23,6): Warning SI1006: 'InternalModule1' is not public, but is imported by public module 'PublicOuter.PublicModule2'. If 'PublicOuter.PublicModule2' is imported outside this assembly this may result in errors. Try making 'PublicOuter.PublicModule2' internal.
                // RegisterModule(typeof(InternalModule1))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalModule1))", DiagnosticSeverity.Warning).WithLocation(23, 6),
                // (24,6): Warning SI1006: 'InternalOuter.InternalModule2' is not public, but is imported by public module 'PublicOuter.PublicModule2'. If 'PublicOuter.PublicModule2' is imported outside this assembly this may result in errors. Try making 'PublicOuter.PublicModule2' internal.
                // RegisterModule(typeof(InternalOuter.InternalModule2))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalOuter.InternalModule2))", DiagnosticSeverity.Warning).WithLocation(24, 6),
                // (25,6): Warning SI1006: 'InternalOuter.Inner.InternalModule3' is not public, but is imported by public module 'PublicOuter.PublicModule2'. If 'PublicOuter.PublicModule2' is imported outside this assembly this may result in errors. Try making 'PublicOuter.PublicModule2' internal.
                // RegisterModule(typeof(InternalOuter.Inner.InternalModule3))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalOuter.Inner.InternalModule3))", DiagnosticSeverity.Warning).WithLocation(25, 6),
                // (26,6): Warning SI1006: 'PublicOuter.InternalModule4' is not public, but is imported by public module 'PublicOuter.PublicModule2'. If 'PublicOuter.PublicModule2' is imported outside this assembly this may result in errors. Try making 'PublicOuter.PublicModule2' internal.
                // RegisterModule(typeof(InternalModule4))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalModule4))", DiagnosticSeverity.Warning).WithLocation(26, 6),
                // (27,21): Error SI0401: Module 'PublicOuter.PublicModule2' must be public or internal.
                // PublicModule2
                new DiagnosticResult("SI0401", @"PublicModule2", DiagnosticSeverity.Error).WithLocation(27, 21),
                // (29,6): Warning SI1006: 'InternalModule1' is not public, but is imported by public module 'PublicOuter.PublicModule3'. If 'PublicOuter.PublicModule3' is imported outside this assembly this may result in errors. Try making 'PublicOuter.PublicModule3' internal.
                // RegisterModule(typeof(InternalModule1))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalModule1))", DiagnosticSeverity.Warning).WithLocation(29, 6),
                // (30,6): Warning SI1006: 'InternalOuter.InternalModule2' is not public, but is imported by public module 'PublicOuter.PublicModule3'. If 'PublicOuter.PublicModule3' is imported outside this assembly this may result in errors. Try making 'PublicOuter.PublicModule3' internal.
                // RegisterModule(typeof(InternalOuter.InternalModule2))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalOuter.InternalModule2))", DiagnosticSeverity.Warning).WithLocation(30, 6),
                // (31,6): Warning SI1006: 'InternalOuter.Inner.InternalModule3' is not public, but is imported by public module 'PublicOuter.PublicModule3'. If 'PublicOuter.PublicModule3' is imported outside this assembly this may result in errors. Try making 'PublicOuter.PublicModule3' internal.
                // RegisterModule(typeof(InternalOuter.Inner.InternalModule3))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalOuter.Inner.InternalModule3))", DiagnosticSeverity.Warning).WithLocation(31, 6),
                // (32,6): Warning SI1006: 'PublicOuter.InternalModule4' is not public, but is imported by public module 'PublicOuter.PublicModule3'. If 'PublicOuter.PublicModule3' is imported outside this assembly this may result in errors. Try making 'PublicOuter.PublicModule3' internal.
                // RegisterModule(typeof(InternalModule4))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalModule4))", DiagnosticSeverity.Warning).WithLocation(32, 6),
                // (33,30): Error SI0401: Module 'PublicOuter.PublicModule3' must be public or internal.
                // PublicModule3
                new DiagnosticResult("SI0401", @"PublicModule3", DiagnosticSeverity.Error).WithLocation(33, 30),
                // (37,10): Warning SI1006: 'InternalModule1' is not public, but is imported by public module 'PublicOuter.Inner.PublicModule4'. If 'PublicOuter.Inner.PublicModule4' is imported outside this assembly this may result in errors. Try making 'PublicOuter.Inner.PublicModule4' internal.
                // RegisterModule(typeof(InternalModule1))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalModule1))", DiagnosticSeverity.Warning).WithLocation(37, 10),
                // (38,10): Warning SI1006: 'InternalOuter.InternalModule2' is not public, but is imported by public module 'PublicOuter.Inner.PublicModule4'. If 'PublicOuter.Inner.PublicModule4' is imported outside this assembly this may result in errors. Try making 'PublicOuter.Inner.PublicModule4' internal.
                // RegisterModule(typeof(InternalOuter.InternalModule2))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalOuter.InternalModule2))", DiagnosticSeverity.Warning).WithLocation(38, 10),
                // (39,10): Warning SI1006: 'InternalOuter.Inner.InternalModule3' is not public, but is imported by public module 'PublicOuter.Inner.PublicModule4'. If 'PublicOuter.Inner.PublicModule4' is imported outside this assembly this may result in errors. Try making 'PublicOuter.Inner.PublicModule4' internal.
                // RegisterModule(typeof(InternalOuter.Inner.InternalModule3))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalOuter.Inner.InternalModule3))", DiagnosticSeverity.Warning).WithLocation(39, 10),
                // (40,10): Warning SI1006: 'PublicOuter.InternalModule4' is not public, but is imported by public module 'PublicOuter.Inner.PublicModule4'. If 'PublicOuter.Inner.PublicModule4' is imported outside this assembly this may result in errors. Try making 'PublicOuter.Inner.PublicModule4' internal.
                // RegisterModule(typeof(InternalModule4))
                new DiagnosticResult("SI1006", @"RegisterModule(typeof(InternalModule4))", DiagnosticSeverity.Warning).WithLocation(40, 10),
                // (41,34): Error SI0401: Module 'PublicOuter.Inner.PublicModule4' must be public or internal.
                // PublicModule4
                new DiagnosticResult("SI0401", @"PublicModule4", DiagnosticSeverity.Error).WithLocation(41, 34));
            comp.GetDiagnostics().Verify();
            Assert.Empty(generated);
        }

        [Fact]
        public void NoWarningWhenAtMostInternallyVisibleModuleImportedByAtMostInternallyVisibleModule()
        {
            string userSource = @"
using StrongInject;

internal class ImportedModule1 {}

[RegisterModule(typeof(ImportedModule1))]
[RegisterModule(typeof(Outer.ImportedModule2))]
[RegisterModule(typeof(Outer.Inner.ImportedModule3))]
internal class Module1 {}

internal class Outer {

    public class ImportedModule2 {}

    [RegisterModule(typeof(ImportedModule1))]
    [RegisterModule(typeof(Outer.ImportedModule2))]
    [RegisterModule(typeof(Outer.Inner.ImportedModule3))]
    [RegisterModule(typeof(Outer.ImportedModule4))]
    public class Module2 {}

    public class Inner
    {
        public class ImportedModule3 {}

        [RegisterModule(typeof(ImportedModule1))]
        [RegisterModule(typeof(Outer.ImportedModule2))]
        [RegisterModule(typeof(Outer.Inner.ImportedModule3))]
        [RegisterModule(typeof(Outer.ImportedModule4))]
        public class Module3 {}
    }

    private class ImportedModule4 {}

    [RegisterModule(typeof(ImportedModule1))]
    [RegisterModule(typeof(Outer.ImportedModule2))]
    [RegisterModule(typeof(Outer.Inner.ImportedModule3))]
    [RegisterModule(typeof(Outer.ImportedModule4))]
    private class Module4 {}
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (38,19): Error SI0401: Module 'Outer.Module4' must be public or internal.
                // Module4
                new DiagnosticResult("SI0401", @"Module4", DiagnosticSeverity.Error).WithLocation(38, 19));
            comp.GetDiagnostics().Verify();
            Assert.Empty(generated);
        }

        [Fact]
        public void RegisterDynamicWithFactoryMethod()
        {

            string userSource = @"
using StrongInject;

public partial class Container : IContainer<int> {
    [Factory] int M(dynamic a) => default;
    [Factory] dynamic M() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        dynamic dynamic_0_1;
        global::System.Int32 int32_0_0;
        dynamic_0_1 = this.M();
        try
        {
            int32_0_0 = this.M(a: dynamic_0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(dynamic_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(dynamic_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        dynamic dynamic_0_1;
        global::System.Int32 int32_0_0;
        dynamic_0_1 = this.M();
        try
        {
            int32_0_0 = this.M(a: dynamic_0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(dynamic_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(dynamic_0_1);
        });
    }
}");
        }

        [Fact]
        public void ResolveDynamicWithGenericFactoryMethod()
        {

            string userSource = @"
using StrongInject;

public partial class Container : IContainer<int> {
    [Factory] int M(dynamic a) => default;
    [Factory] T M<T>() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        dynamic dynamic_0_1;
        global::System.Int32 int32_0_0;
        dynamic_0_1 = this.M<dynamic>();
        try
        {
            int32_0_0 = this.M(a: dynamic_0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(dynamic_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(dynamic_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        dynamic dynamic_0_1;
        global::System.Int32 int32_0_0;
        dynamic_0_1 = this.M<dynamic>();
        try
        {
            int32_0_0 = this.M(a: dynamic_0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(dynamic_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(dynamic_0_1);
        });
    }
}");
        }

        [Fact]
        public void RegisterFuncOfDynamicWithFactoryMethod()
        {

            string userSource = @"
using StrongInject;
using System;

public partial class Container : IContainer<int> {
    [Factory(Scope.SingleInstance)] int M(Func<dynamic> a) => default;
    [Factory] dynamic M() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
        this._lock0.Wait();
        try
        {
            this._disposeAction0?.Invoke();
        }
        finally
        {
            this._lock0.Release();
        }
    }

    private global::System.Int32 _int32Field0;
    private global::System.Threading.SemaphoreSlim _lock0 = new global::System.Threading.SemaphoreSlim(1);
    private global::System.Action _disposeAction0;
    private global::System.Int32 GetInt32Field0()
    {
        if (!object.ReferenceEquals(_int32Field0, null))
            return _int32Field0;
        this._lock0.Wait();
        try
        {
            if (this.Disposed)
                throw new global::System.ObjectDisposedException(nameof(Container));
            global::System.Collections.Concurrent.ConcurrentBag<global::System.Action> disposeActions_func_0_1;
            global::System.Func<dynamic> func_0_1;
            global::System.Int32 int32_0_0;
            disposeActions_func_0_1 = new global::System.Collections.Concurrent.ConcurrentBag<global::System.Action>();
            func_0_1 = () =>
            {
                dynamic dynamic_1_0;
                dynamic_1_0 = this.M();
                disposeActions_func_0_1.Add(() =>
                {
                    global::StrongInject.Helpers.Dispose(dynamic_1_0);
                });
                return dynamic_1_0;
            };
            try
            {
                int32_0_0 = this.M(a: func_0_1);
            }
            catch
            {
                foreach (var disposeAction in disposeActions_func_0_1)
                    disposeAction();
                throw;
            }

            this._int32Field0 = int32_0_0;
            this._disposeAction0 = () =>
            {
                foreach (var disposeAction in disposeActions_func_0_1)
                    disposeAction();
            };
        }
        finally
        {
            this._lock0.Release();
        }

        return _int32Field0;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Int32 int32_0_0;
        int32_0_0 = GetInt32Field0();
        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Int32 int32_0_0;
        int32_0_0 = GetInt32Field0();
        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ResolveFunctionPointerWithGenericFactoryMethod()
        {

            string userSource = @"
using StrongInject;
using StrongInject.Modules;

[RegisterModule(typeof(ValueTupleModule))]
public unsafe partial class Container : IContainer<(int, string, bool, short, ushort)> {
    [Factory] int M(delegate*<int, string> a) => default;
    [Factory] delegate*<T1, T2> M<T1, T2>() => default;

    [Factory] string M1(delegate*<ref int, string> a) => default;
    [Factory] delegate*<ref T1, T2> M1<T1, T2>() => default;

    [Factory] bool M2(delegate*<int, ref string> a) => default;
    [Factory] delegate*<T1, ref T2> M2<T1, T2>() => default;

    [Factory] short M3(delegate* unmanaged[Cdecl]<int, string> a) => default;
    [Factory] delegate* unmanaged[Cdecl]<T1, T2> M3<T1, T2>() => default;

    [Factory] ushort M4(delegate* unmanaged[Fastcall]<int, string> a) => default;
    [Factory] delegate* unmanaged[Fastcall]<T1, T2> M4<T1, T2>() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
unsafe partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<(global::System.Int32, global::System.String, global::System.Boolean, global::System.Int16, global::System.UInt16)>.Run<TResult, TParam>(global::System.Func<(global::System.Int32, global::System.String, global::System.Boolean, global::System.Int16, global::System.UInt16), TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        delegate *<global::System.Int32, global::System.String>_0_2;
        global::System.Int32 int32_0_1;
        delegate *<ref global::System.Int32, global::System.String>_0_4;
        global::System.String string_0_3;
        delegate *<global::System.Int32, ref global::System.String>_0_6;
        global::System.Boolean boolean_0_5;
        delegate *unmanaged[Cdecl]<global::System.Int32, global::System.String>_0_8;
        global::System.Int16 int16_0_7;
        delegate *unmanaged[Fastcall]<global::System.Int32, global::System.String>_0_10;
        global::System.UInt16 uInt16_0_9;
        (global::System.Int32, global::System.String, global::System.Boolean, global::System.Int16, global::System.UInt16) valueTuple_0_0;
        _0_2 = this.M<global::System.Int32, global::System.String>();
        int32_0_1 = this.M(a: _0_2);
        _0_4 = this.M1<global::System.Int32, global::System.String>();
        string_0_3 = this.M1(a: _0_4);
        _0_6 = this.M2<global::System.Int32, global::System.String>();
        boolean_0_5 = this.M2(a: _0_6);
        _0_8 = this.M3<global::System.Int32, global::System.String>();
        int16_0_7 = this.M3(a: _0_8);
        _0_10 = this.M4<global::System.Int32, global::System.String>();
        uInt16_0_9 = this.M4(a: _0_10);
        valueTuple_0_0 = global::StrongInject.Modules.ValueTupleModule.CreateValueTuple<global::System.Int32, global::System.String, global::System.Boolean, global::System.Int16, global::System.UInt16>(a: int32_0_1, b: string_0_3, c: boolean_0_5, d: int16_0_7, e: uInt16_0_9);
        TResult result;
        try
        {
            result = func(valueTuple_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<(global::System.Int32, global::System.String, global::System.Boolean, global::System.Int16, global::System.UInt16)> global::StrongInject.IContainer<(global::System.Int32, global::System.String, global::System.Boolean, global::System.Int16, global::System.UInt16)>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        delegate *<global::System.Int32, global::System.String>_0_2;
        global::System.Int32 int32_0_1;
        delegate *<ref global::System.Int32, global::System.String>_0_4;
        global::System.String string_0_3;
        delegate *<global::System.Int32, ref global::System.String>_0_6;
        global::System.Boolean boolean_0_5;
        delegate *unmanaged[Cdecl]<global::System.Int32, global::System.String>_0_8;
        global::System.Int16 int16_0_7;
        delegate *unmanaged[Fastcall]<global::System.Int32, global::System.String>_0_10;
        global::System.UInt16 uInt16_0_9;
        (global::System.Int32, global::System.String, global::System.Boolean, global::System.Int16, global::System.UInt16) valueTuple_0_0;
        _0_2 = this.M<global::System.Int32, global::System.String>();
        int32_0_1 = this.M(a: _0_2);
        _0_4 = this.M1<global::System.Int32, global::System.String>();
        string_0_3 = this.M1(a: _0_4);
        _0_6 = this.M2<global::System.Int32, global::System.String>();
        boolean_0_5 = this.M2(a: _0_6);
        _0_8 = this.M3<global::System.Int32, global::System.String>();
        int16_0_7 = this.M3(a: _0_8);
        _0_10 = this.M4<global::System.Int32, global::System.String>();
        uInt16_0_9 = this.M4(a: _0_10);
        valueTuple_0_0 = global::StrongInject.Modules.ValueTupleModule.CreateValueTuple<global::System.Int32, global::System.String, global::System.Boolean, global::System.Int16, global::System.UInt16>(a: int32_0_1, b: string_0_3, c: boolean_0_5, d: int16_0_7, e: uInt16_0_9);
        return new global::StrongInject.Owned<(global::System.Int32, global::System.String, global::System.Boolean, global::System.Int16, global::System.UInt16)>(valueTuple_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void ResolvePointerWithGenericFactoryMethod()
        {

            string userSource = @"
using StrongInject;
using StrongInject.Modules;

[RegisterModule(typeof(ValueTupleModule))]
public unsafe partial class Container : IContainer<int> {
    [Factory] int M(int* a) => default;
    [Factory] T* M<T>() where T : unmanaged => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
unsafe partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Int32*_0_1;
        global::System.Int32 int32_0_0;
        _0_1 = this.M<global::System.Int32>();
        int32_0_0 = this.M(a: _0_1);
        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Int32*_0_1;
        global::System.Int32 int32_0_0;
        _0_1 = this.M<global::System.Int32>();
        int32_0_0 = this.M(a: _0_1);
        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void DontResolvePointerWithGenericFactoryMethodReturningNonPointer()
        {

            string userSource = @"
using StrongInject;
using StrongInject.Modules;

[RegisterModule(typeof(ValueTupleModule))]
public unsafe partial class Container : IContainer<int> {
    [Factory] int M(int* a) => default;
    [Factory] T M<T>() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,29): Error SI0102: Error while resolving dependencies for 'int': We have no source for instance of type 'int*'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(6, 29),
                // (6,29): Warning SI1106: Warning while resolving dependencies for 'int': factory method 'Container.M<T>()' cannot be used to resolve instance of type 'int*' as the required type arguments do not satisfy the generic constraints.
                // Container
                new DiagnosticResult("SI1106", @"Container", DiagnosticSeverity.Warning).WithLocation(6, 29));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }
}");
        }

        [Fact]
        public void ResolveArrayOfPointersWithGenericFactoryMethod()
        {

            string userSource = @"
using StrongInject;
using StrongInject.Modules;

[RegisterModule(typeof(ValueTupleModule))]
public unsafe partial class Container : IContainer<int> {
    [Factory] int M(int*[] a) => default;
    [Factory] T M<T>() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
unsafe partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Int32*[] _0_1;
        global::System.Int32 int32_0_0;
        _0_1 = this.M<global::System.Int32*[]>();
        try
        {
            int32_0_0 = this.M(a: _0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(_0_1);
            throw;
        }

        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(_0_1);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Int32*[] _0_1;
        global::System.Int32 int32_0_0;
        _0_1 = this.M<global::System.Int32*[]>();
        try
        {
            int32_0_0 = this.M(a: _0_1);
        }
        catch
        {
            global::StrongInject.Helpers.Dispose(_0_1);
            throw;
        }

        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(_0_1);
        });
    }
}");
        }

        [Fact]
        public void TestFactoryOfNonGeneric()
        {

            string userSource = @"
using StrongInject;
using System.Collections.Generic;

public partial class Container : IContainer<Dictionary<string, int>>, IContainer<Dictionary<string, object>>, IContainer<int> {
    [FactoryOf(typeof(Dictionary<string, int>))] [FactoryOf(typeof(Dictionary<string, object>))] Dictionary<T1, T2> M1<T1, T2>() => default;
    [FactoryOf(typeof(int))] T M2<T>() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32> dictionary_0_0;
        dictionary_0_0 = this.M1<global::System.String, global::System.Int32>();
        TResult result;
        try
        {
            result = func(dictionary_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(dictionary_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>> global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32> dictionary_0_0;
        dictionary_0_0 = this.M1<global::System.String, global::System.Int32>();
        return new global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>(dictionary_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(dictionary_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object> dictionary_0_0;
        dictionary_0_0 = this.M1<global::System.String, global::System.Object>();
        TResult result;
        try
        {
            result = func(dictionary_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(dictionary_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>> global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object> dictionary_0_0;
        dictionary_0_0 = this.M1<global::System.String, global::System.Object>();
        return new global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>(dictionary_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(dictionary_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::System.Int32>.Run<TResult, TParam>(global::System.Func<global::System.Int32, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Int32 int32_0_0;
        int32_0_0 = this.M2<global::System.Int32>();
        TResult result;
        try
        {
            result = func(int32_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Int32> global::StrongInject.IContainer<global::System.Int32>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Int32 int32_0_0;
        int32_0_0 = this.M2<global::System.Int32>();
        return new global::StrongInject.Owned<global::System.Int32>(int32_0_0, () =>
        {
        });
    }
}");
        }

        [Fact]
        public void TestFactoryOfGeneric()
        {

            string userSource = @"
using StrongInject;
using System.Collections.Generic;

public partial class Container : IContainer<Dictionary<string, int>>, IContainer<Dictionary<string, object>> {
    [FactoryOf(typeof(Dictionary<,>))] T M1<T>() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32> dictionary_0_0;
        dictionary_0_0 = this.M1<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>();
        TResult result;
        try
        {
            result = func(dictionary_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(dictionary_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>> global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32> dictionary_0_0;
        dictionary_0_0 = this.M1<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>();
        return new global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>(dictionary_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(dictionary_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object> dictionary_0_0;
        dictionary_0_0 = this.M1<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>();
        TResult result;
        try
        {
            result = func(dictionary_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(dictionary_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>> global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object> dictionary_0_0;
        dictionary_0_0 = this.M1<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>();
        return new global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>(dictionary_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(dictionary_0_0);
        });
    }
}");
        }

        [Fact]
        public void CannotResolveFactoryOfWhichDoesntMatch()
        {

            string userSource = @"
using StrongInject;
using System.Collections.Generic;

public partial class Container : IContainer<Dictionary<string, int>>, IContainer<Dictionary<string, object>>, IContainer<IEnumerable<KeyValuePair<string, int>>> {
    [FactoryOf(typeof(Dictionary<string, object>))] [FactoryOf(typeof(IEnumerable<>))] T M1<T>() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (5,22): Error SI0102: Error while resolving dependencies for 'System.Collections.Generic.Dictionary<string, int>': We have no source for instance of type 'System.Collections.Generic.Dictionary<string, int>'
                // Container
                new DiagnosticResult("SI0102", @"Container", DiagnosticSeverity.Error).WithLocation(5, 22));
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public void Dispose()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>, TParam, TResult> func, TParam param)
    {
        throw new global::System.NotImplementedException();
    }

    global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>> global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Int32>>.Resolve()
    {
        throw new global::System.NotImplementedException();
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object> dictionary_0_0;
        dictionary_0_0 = this.M1<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>();
        TResult result;
        try
        {
            result = func(dictionary_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(dictionary_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>> global::StrongInject.IContainer<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object> dictionary_0_0;
        dictionary_0_0 = this.M1<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>();
        return new global::StrongInject.Owned<global::System.Collections.Generic.Dictionary<global::System.String, global::System.Object>>(dictionary_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(dictionary_0_0);
        });
    }

    TResult global::StrongInject.IContainer<global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Int32>>>.Run<TResult, TParam>(global::System.Func<global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Int32>>, TParam, TResult> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Int32>> iEnumerable_0_0;
        iEnumerable_0_0 = this.M1<global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Int32>>>();
        TResult result;
        try
        {
            result = func(iEnumerable_0_0, param);
        }
        finally
        {
            global::StrongInject.Helpers.Dispose(iEnumerable_0_0);
        }

        return result;
    }

    global::StrongInject.Owned<global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Int32>>> global::StrongInject.IContainer<global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Int32>>>.Resolve()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Int32>> iEnumerable_0_0;
        iEnumerable_0_0 = this.M1<global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Int32>>>();
        return new global::StrongInject.Owned<global::System.Collections.Generic.IEnumerable<global::System.Collections.Generic.KeyValuePair<global::System.String, global::System.Int32>>>(iEnumerable_0_0, () =>
        {
            global::StrongInject.Helpers.Dispose(iEnumerable_0_0);
        });
    }
}");
        }

        [Fact]
        public void FactoryOfNonGenericMustBeResolvableFromMethod()
        {

            string userSource = @"
using StrongInject;
using System.Collections.Generic;

public class Module {
    [FactoryOf(typeof(Dictionary<string, object>))] [FactoryOf(typeof(Dictionary<string, List<int>>))] public static Dictionary<T1, List<T2>> M1<T1, T2>() where T2 : class => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Error SI0029: FactoryOfAttribute Type 'System.Collections.Generic.Dictionary<string, object>' cannot be constructed from the return type 'System.Collections.Generic.Dictionary<T1, System.Collections.Generic.List<T2>>' of method 'Container.M1<T1, T2>()'.
                // FactoryOf(typeof(Dictionary<string, object>))
                new DiagnosticResult("SI0029", @"FactoryOf(typeof(Dictionary<string, object>))", DiagnosticSeverity.Error).WithLocation(6, 6),
                // (6,54): Error SI0030: FactoryOfAttribute Type 'System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<int>>' cannot be constructed from the return type 'System.Collections.Generic.Dictionary<T1, System.Collections.Generic.List<T2>>' of method 'Container.M1<T1, T2>()'  as constraints do not match.
                // FactoryOf(typeof(Dictionary<string, List<int>>))
                new DiagnosticResult("SI0030", @"FactoryOf(typeof(Dictionary<string, List<int>>))", DiagnosticSeverity.Error).WithLocation(6, 54));
            comp.GetDiagnostics().Verify();
            Assert.Empty(generated);
        }

        [Fact]
        public void FactoryOfGenericMustHaveMethodReturnTypeParameter()
        {

            string userSource = @"
using StrongInject;
using System.Collections.Generic;

public class Module {
    [FactoryOf(typeof(Dictionary<,>))] public static Dictionary<T1, T2> M1<T1, T2>() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify(
                // (6,6): Error SI0028: Method 'Module.M1<T1, T2>()' marked with FactoryOfAttribute of open generic type 'System.Collections.Generic.Dictionary<,>' must have a single type parameter, and return that type parameter.
                // FactoryOf(typeof(Dictionary<,>))
                new DiagnosticResult("SI0028", @"FactoryOf(typeof(Dictionary<,>))", DiagnosticSeverity.Error).WithLocation(6, 6));
            comp.GetDiagnostics().Verify();
            Assert.Empty(generated);
        }

        [Fact]
        public void FactoryOfMethodCanReturnTask()
        {

            string userSource = @"
using StrongInject;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class Container : IAsyncContainer<Dictionary<int, string>>, IAsyncContainer<int> {
    [FactoryOf(typeof(Dictionary<,>))] [FactoryOf(typeof(int))] public static Task<T> M<T>() => default;
}";
            var comp = RunGeneratorWithStrongInjectReference(userSource, out var generatorDiagnostics, out var generated);
            generatorDiagnostics.Verify();
            comp.GetDiagnostics().Verify();
            var file = Assert.Single(generated);
            file.Should().BeIgnoringLineEndings(@"#pragma warning disable CS1998
partial class Container
{
    private int _disposed = 0;
    private bool Disposed => _disposed != 0;
    public async global::System.Threading.Tasks.ValueTask DisposeAsync()
    {
        var disposed = global::System.Threading.Interlocked.Exchange(ref this._disposed, 1);
        if (disposed != 0)
            return;
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>>.RunAsync<TResult, TParam>(global::System.Func<global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.Task<global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>> dictionary_0_1;
        var hasAwaitStarted_dictionary_0_1 = false;
        var dictionary_0_0 = default(global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>);
        var hasAwaitCompleted_dictionary_0_1 = false;
        dictionary_0_1 = global::Container.M<global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>>();
        try
        {
            hasAwaitStarted_dictionary_0_1 = true;
            dictionary_0_0 = await dictionary_0_1;
            hasAwaitCompleted_dictionary_0_1 = true;
        }
        catch
        {
            if (!hasAwaitStarted_dictionary_0_1)
            {
                dictionary_0_0 = await dictionary_0_1;
            }
            else if (!hasAwaitCompleted_dictionary_0_1)
            {
                throw;
            }

            await global::StrongInject.Helpers.DisposeAsync(dictionary_0_0);
            throw;
        }

        TResult result;
        try
        {
            result = await func(dictionary_0_0, param);
        }
        finally
        {
            await global::StrongInject.Helpers.DisposeAsync(dictionary_0_0);
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>>> global::StrongInject.IAsyncContainer<global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.Task<global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>> dictionary_0_1;
        var hasAwaitStarted_dictionary_0_1 = false;
        var dictionary_0_0 = default(global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>);
        var hasAwaitCompleted_dictionary_0_1 = false;
        dictionary_0_1 = global::Container.M<global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>>();
        try
        {
            hasAwaitStarted_dictionary_0_1 = true;
            dictionary_0_0 = await dictionary_0_1;
            hasAwaitCompleted_dictionary_0_1 = true;
        }
        catch
        {
            if (!hasAwaitStarted_dictionary_0_1)
            {
                dictionary_0_0 = await dictionary_0_1;
            }
            else if (!hasAwaitCompleted_dictionary_0_1)
            {
                throw;
            }

            await global::StrongInject.Helpers.DisposeAsync(dictionary_0_0);
            throw;
        }

        return new global::StrongInject.AsyncOwned<global::System.Collections.Generic.Dictionary<global::System.Int32, global::System.String>>(dictionary_0_0, async () =>
        {
            await global::StrongInject.Helpers.DisposeAsync(dictionary_0_0);
        });
    }

    async global::System.Threading.Tasks.ValueTask<TResult> global::StrongInject.IAsyncContainer<global::System.Int32>.RunAsync<TResult, TParam>(global::System.Func<global::System.Int32, TParam, global::System.Threading.Tasks.ValueTask<TResult>> func, TParam param)
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.Task<global::System.Int32> int32_0_1;
        var hasAwaitStarted_int32_0_1 = false;
        var int32_0_0 = default(global::System.Int32);
        int32_0_1 = global::Container.M<global::System.Int32>();
        try
        {
            hasAwaitStarted_int32_0_1 = true;
            int32_0_0 = await int32_0_1;
        }
        catch
        {
            if (!hasAwaitStarted_int32_0_1)
            {
                _ = int32_0_1.ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        TResult result;
        try
        {
            result = await func(int32_0_0, param);
        }
        finally
        {
        }

        return result;
    }

    async global::System.Threading.Tasks.ValueTask<global::StrongInject.AsyncOwned<global::System.Int32>> global::StrongInject.IAsyncContainer<global::System.Int32>.ResolveAsync()
    {
        if (Disposed)
            throw new global::System.ObjectDisposedException(nameof(Container));
        global::System.Threading.Tasks.Task<global::System.Int32> int32_0_1;
        var hasAwaitStarted_int32_0_1 = false;
        var int32_0_0 = default(global::System.Int32);
        int32_0_1 = global::Container.M<global::System.Int32>();
        try
        {
            hasAwaitStarted_int32_0_1 = true;
            int32_0_0 = await int32_0_1;
        }
        catch
        {
            if (!hasAwaitStarted_int32_0_1)
            {
                _ = int32_0_1.ContinueWith(failedTask => _ = failedTask.Exception, global::System.Threading.Tasks.TaskContinuationOptions.OnlyOnFaulted);
            }

            throw;
        }

        return new global::StrongInject.AsyncOwned<global::System.Int32>(int32_0_0, async () =>
        {
        });
    }
}");
        }
    }
}
