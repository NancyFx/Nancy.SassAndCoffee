namespace Nancy.SassAndCoffee
{
    using System;
    using System.IO;
    using System.Text;

    using global::SassAndCoffee.Core;
    using global::SassAndCoffee.Core.Caching;

    using Nancy.Bootstrapper;

    public static class Hooks
    {
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

                    if (!compiler.CanCompile(ctx.Request.Uri))
                    {
                        return null;
                    }

                    var content = compiler.GetCompiledContent(ctx.Request.Uri);

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