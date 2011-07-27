namespace Nancy.SassAndCoffee
{
    using System;
    using System.IO;

    using global::SassAndCoffee.Core;
    using global::SassAndCoffee.Core.Caching;

    using Nancy.Bootstrapper;

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
        public static void Enable(IApplicationPipelines pipelines, ICompiledCache cache, IRootPathProvider rootPathProvider)
        {
            var host = new NancyCompilerHost(rootPathProvider);

            var compiler = new ContentCompiler(host, cache);

            pipelines.BeforeRequest.AddItemToStartOfPipeline(GetPipelineHook(compiler));
        }

        private static Func<NancyContext, Response> GetPipelineHook(ContentCompiler compiler)
        {
            return ctx =>
                {
                    if (ctx.Request == null)
                    {
                        return null;
                    }

                    if (!compiler.CanCompile(ctx.Request.Url.Path))
                    {
                        return null;
                    }

                    var content = compiler.GetCompiledContent(ctx.Request.Url.Path);

                    return content.Compiled ? GetResponse(content) : null;
                };
        }

        private static Response GetResponse(CompilationResult content)
        {
            var response = new Response();

            response.StatusCode = HttpStatusCode.OK;
            response.ContentType = content.MimeType;
            response.Headers["ETag"] = content.SourceLastModifiedUtc.Ticks.ToString("x");
            response.Headers["Content-Disposition"] = "inline";
            response.Headers["Last-Modified"] = content.SourceLastModifiedUtc.ToString("R");
            response.Contents = s =>
                {
                    using (var writer = new StreamWriter(s))
                    {
                        writer.Write(content.Contents);
                        writer.Flush();
                    }
                };

            return response;
        }
    }
}