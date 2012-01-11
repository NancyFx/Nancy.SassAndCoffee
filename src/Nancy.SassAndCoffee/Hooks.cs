namespace Nancy.SassAndCoffee
{
    using System;
    using System.IO;

    using global::SassAndCoffee.Core;
    using global::SassAndCoffee.Core.Caching;

    using Nancy.Bootstrapper;

    using global::SassAndCoffee.JavaScript;
    using global::SassAndCoffee.JavaScript.CoffeeScript;
    using global::SassAndCoffee.JavaScript.Uglify;
    using global::SassAndCoffee.Ruby.Sass;

    public static class Hooks
    {
        /// <summary>
        /// <para>
        /// Enable SassAndCoffee support in the application.
        /// </para>
        /// <para>
        /// SassAndCoffee supports on the fly compilation and caching of CoffeeScript and Sass,
        /// along with concatenation and minification.
        /// </para>
        /// </summary>
        /// <param name="pipelines">Application pipelines to hook into</param>
        /// <param name="cache">Cache provider to use</param>
        /// <param name="rootPathProvider">Root path provider</param>
        public static void Enable(IPipelines pipelines, ICompiledCache cache, IRootPathProvider rootPathProvider)
        {
            var host = new NancyCompilerHost(rootPathProvider);

            var compiler = new ContentPipeline(new IContentTransform[]
                {
                    new FileSourceContentTransform("text/javascript", ".js"),
                    new JavaScriptCombineContentTransform(),
                    new FileSourceContentTransform("text/coffeescript", ".coffee"),
                    new CoffeeScriptCompilerContentTransform(),
                    new UglifyCompilerContentTransform(),
                    new SassCompilerContentTransform(),
                });

            pipelines.BeforeRequest.AddItemToStartOfPipeline(GetPipelineHook(compiler, host));
        }

        private static Func<NancyContext, Response> GetPipelineHook(IContentPipeline compiler, NancyCompilerHost host)
        {
            return ctx =>
                {
                    if (ctx.Request == null)
                    {
                        return null;
                    }

                    var result = compiler.ProcessRequest(host.MapPath(ctx.Request.Path));

                    if (result == null)
                    {
                        return null;
                    }

                    return GetResponse(result);
                };
        }

        private static Response GetResponse(ContentResult content)
        {
            var response = new Response();

            response.StatusCode = HttpStatusCode.OK;
            response.ContentType = content.MimeType;
            //response.Headers["ETag"] = content.SourceLastModifiedUtc.Ticks.ToString("x");
            //response.Headers["Content-Disposition"] = "inline";
            //response.Headers["Last-Modified"] = content.SourceLastModifiedUtc.ToString("R");
            response.Contents = s =>
                {
                    using (var writer = new StreamWriter(s))
                    {
                        writer.Write(content.Content);
                        writer.Flush();
                    }
                };

            return response;
        }
    }
}