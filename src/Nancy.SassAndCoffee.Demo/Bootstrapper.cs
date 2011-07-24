namespace Nancy.SassAndCoffee.Demo
{
    using global::SassAndCoffee.Core.Caching;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void InitialiseInternal(TinyIoC.TinyIoCContainer container)
        {
            base.InitialiseInternal(container);

            SassAndCoffee.Hooks.Enable(this, new InMemoryCache(), container.Resolve<IRootPathProvider>());
        }
    }
}