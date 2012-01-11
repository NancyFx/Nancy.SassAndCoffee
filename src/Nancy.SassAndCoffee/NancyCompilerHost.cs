namespace Nancy.SassAndCoffee
{
    using System;
    using System.IO;

    using global::SassAndCoffee.Core;

    public class NancyCompilerHost
    {
        private readonly IRootPathProvider rootPathProvider;

        public NancyCompilerHost(IRootPathProvider rootPathProvider)
        {
            this.rootPathProvider = rootPathProvider;
        }

        public string MapPath(string path)
        {
            return Path.Combine(this.rootPathProvider.GetRootPath(), GetRelativePath(path));
        }

        private string GetRelativePath(string path)
        {
            return path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        }

        public string ApplicationBasePath
        {
            get
            {
                return this.rootPathProvider.GetRootPath();
            }
        }
    }
}