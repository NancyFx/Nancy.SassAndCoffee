namespace Nancy.SassAndCoffee.Demo
{
    using global::SassAndCoffee.Core.Caching;

    public class Bootstrapper : DefaultNancyBootstrapper
    {
        protected override void ApplicationStartup(TinyIoC.TinyIoCContainer container, Nancy.Bootstrapper.IPipelines pipelines)
        {
            base.ApplicationStartup(container, pipelines);

            StaticConfiguration.DisableErrorTraces = false;

            SassAndCoffee.Hooks.Enable(pipelines, new InMemoryCache(), container.Resolve<IRootPathProvider>());
        }
    }
}