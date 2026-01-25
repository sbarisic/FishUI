using System;
using System.IO;

namespace FishUI
{
	/// <summary>
	/// Interface for file system operations. Implement this interface to provide
	/// custom file system behavior (virtual file systems, embedded resources, game engine assets, etc.).
	/// </summary>
	public interface IFishUIFileSystem
	{
		/// <summary>
		/// Determines whether the specified file exists.
		/// </summary>
		/// <param name="path">The file path to check.</param>
		/// <returns>True if the file exists; otherwise, false.</returns>
		bool Exists(string path);

		/// <summary>
		/// Opens a text file, reads all the text, and closes the file.
		/// </summary>
		/// <param name="path">The file path to read.</param>
		/// <returns>The contents of the file as a string.</returns>
		string ReadAllText(string path);

		/// <summary>
		/// Creates a new file, writes the specified string to the file, and closes the file.
		/// If the target file already exists, it is overwritten.
		/// </summary>
		/// <param name="path">The file path to write to.</param>
		/// <param name="contents">The string to write to the file.</param>
		void WriteAllText(string path, string contents);

		/// <summary>
		/// Returns the absolute path for the specified path string.
		/// </summary>
		/// <param name="path">The file or directory path.</param>
		/// <returns>The fully qualified location of path.</returns>
		string GetFullPath(string path);

		/// <summary>
		/// Returns the directory information for the specified path.
		/// </summary>
		/// <param name="path">The path of a file or directory.</param>
		/// <returns>Directory information for path, or null if path is a root directory or is null.</returns>
		string GetDirectoryName(string path);

		/// <summary>
		/// Combines two path strings.
		/// </summary>
		/// <param name="path1">The first path to combine.</param>
		/// <param name="path2">The second path to combine.</param>
		/// <returns>The combined paths.</returns>
		string CombinePath(string path1, string path2);
	}

	/// <summary>
	/// Default file system implementation that uses System.IO.
	/// </summary>
	public class DefaultFishUIFileSystem : IFishUIFileSystem
	{
		/// <inheritdoc/>
		public bool Exists(string path)
		{
			return File.Exists(path);
		}

		/// <inheritdoc/>
		public string ReadAllText(string path)
		{
			return File.ReadAllText(path);
		}

		/// <inheritdoc/>
		public void WriteAllText(string path, string contents)
		{
			File.WriteAllText(path, contents);
		}

		/// <inheritdoc/>
		public string GetFullPath(string path)
		{
			return Path.GetFullPath(path);
		}

		/// <inheritdoc/>
		public string GetDirectoryName(string path)
		{
			return Path.GetDirectoryName(path);
		}

		/// <inheritdoc/>
		public string CombinePath(string path1, string path2)
		{
			return Path.Combine(path1, path2);
		}
	}
}
