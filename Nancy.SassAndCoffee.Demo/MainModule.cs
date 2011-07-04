namespace Nancy.SassAndCoffee.Demo
{
    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get["/"] = _ => View["Index"];
        }
    }
}