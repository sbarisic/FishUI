using FishUI;

namespace UnitTest.Mocks
{
	/// <summary>
	/// Mock file system backend for unit testing FishUI file operations.
	/// </summary>
	public class MockFishUIFileSystem : IFishUIFileSystem
	{
		private readonly Dictionary<string, string> _files = new(StringComparer.OrdinalIgnoreCase);
		private readonly HashSet<string> _directories = new(StringComparer.OrdinalIgnoreCase);

		/// <summary>
		/// Adds a virtual file to the mock file system.
		/// </summary>
		public void AddFile(string path, string content)
		{
			var normalizedPath = NormalizePath(path);
			_files[normalizedPath] = content;

			// Auto-create parent directories
			var dir = GetDirectoryName(normalizedPath);
			while (!string.IsNullOrEmpty(dir))
			{
				_directories.Add(dir);
				dir = GetDirectoryName(dir);
			}
		}

		/// <summary>
		/// Adds a virtual directory to the mock file system.
		/// </summary>
		public void AddDirectory(string path)
		{
			_directories.Add(NormalizePath(path));
		}

		/// <summary>
		/// Clears all virtual files and directories.
		/// </summary>
		public void Reset()
		{
			_files.Clear();
			_directories.Clear();
		}

		private static string NormalizePath(string path)
		{
			return path?.Replace('\\', '/').TrimEnd('/') ?? "";
		}

		public bool Exists(string path)
		{
			var normalized = NormalizePath(path);
			return _files.ContainsKey(normalized) || _directories.Contains(normalized);
		}

		public string ReadAllText(string path)
		{
			var normalized = NormalizePath(path);
			if (_files.TryGetValue(normalized, out var content))
				return content;
			throw new FileNotFoundException($"File not found: {path}");
		}

		public void WriteAllText(string path, string contents)
		{
			var normalized = NormalizePath(path);
			_files[normalized] = contents;

			// Auto-create parent directories
			var dir = GetDirectoryName(normalized);
			while (!string.IsNullOrEmpty(dir))
			{
				_directories.Add(dir);
				dir = GetDirectoryName(dir);
			}
		}

		public string GetFullPath(string path)
		{
			return NormalizePath(path);
		}

		public string GetDirectoryName(string path)
		{
			var normalized = NormalizePath(path);
			var lastSlash = normalized.LastIndexOf('/');
			return lastSlash > 0 ? normalized[..lastSlash] : null;
		}

		public string CombinePath(string path1, string path2)
		{
			if (string.IsNullOrEmpty(path1)) return NormalizePath(path2);
			if (string.IsNullOrEmpty(path2)) return NormalizePath(path1);
			return NormalizePath(path1) + "/" + NormalizePath(path2);
		}

		public string GetFileName(string path)
		{
			var normalized = NormalizePath(path);
			var lastSlash = normalized.LastIndexOf('/');
			return lastSlash >= 0 ? normalized[(lastSlash + 1)..] : normalized;
		}

		public string[] GetDirectories(string path)
		{
			var normalized = NormalizePath(path);
			return _directories
				.Where(d => GetDirectoryName(d) == normalized)
				.ToArray();
		}

		public string[] GetFiles(string path, string searchPattern = "*")
		{
			var normalized = NormalizePath(path);
			var pattern = searchPattern.Replace("*", "");

			return _files.Keys
				.Where(f => GetDirectoryName(f) == normalized)
				.Where(f => string.IsNullOrEmpty(pattern) || f.EndsWith(pattern, StringComparison.OrdinalIgnoreCase))
				.ToArray();
		}

		public bool IsDirectory(string path)
		{
			return _directories.Contains(NormalizePath(path));
		}

		public string GetParentDirectory(string path)
		{
			return GetDirectoryName(path);
		}
	}
}
