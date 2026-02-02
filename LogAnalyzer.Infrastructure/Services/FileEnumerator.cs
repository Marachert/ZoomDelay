using System.Collections.Generic;
using System.IO;
using System.Threading;
using LogAnalyzer.Core.Interfaces;

namespace LogAnalyzer.Infrastructure.Services;

public sealed class FileEnumerator : IFileEnumerator
{
    public IEnumerable<string> EnumerateLogFiles(string rootFolder, CancellationToken cancellationToken)
    {
        Stack<string> pending = new Stack<string>();
        pending.Push(rootFolder);

        while (pending.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string current = pending.Pop();
            IEnumerable<string> directories;
            IEnumerable<string> files;

            try
            {
                directories = Directory.EnumerateDirectories(current);
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (string directory in directories)
            {
                pending.Push(directory);
            }

            try
            {
                files = Directory.EnumerateFiles(current, "*.log");
            }
            catch (IOException)
            {
                continue;
            }
            catch (UnauthorizedAccessException)
            {
                continue;
            }

            foreach (string file in files)
            {
                yield return file;
            }
        }
    }
}
