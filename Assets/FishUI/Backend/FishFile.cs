using FishUI;
using System.IO;
using UnityEngine;

public class FishFS : IFishUIFileSystem
{
	private string rootPath;

	public FishFS()
	{
		rootPath = Application.dataPath;
		Debug.Log($"[FishFS] Initialized with root path: {rootPath}");
	}

	public string ResolvePath(string path)
	{
		if (path.StartsWith("Assets"))
			return path;

		if (Path.IsPathRooted(path))
			return path;

		return Path.Combine(rootPath, path);
	}

	public string CombinePath(string path1, string path2)
	{
		string result = Path.Combine(path1, path2);
		Debug.Log($"[FishFS] CombinePath: {path1} + {path2} = {result}");
		return result;
	}

	public bool Exists(string path)
	{
		string resolved = ResolvePath(path);
		bool exists = File.Exists(resolved) || Directory.Exists(resolved);
		Debug.Log($"[FishFS] Exists: {resolved} = {exists}");
		return exists;
	}

	public string[] GetDirectories(string path)
	{
		string resolved = ResolvePath(path);
		string[] dirs = Directory.GetDirectories(resolved);
		Debug.Log($"[FishFS] GetDirectories: {resolved} ({dirs.Length} directories)");
		return dirs;
	}

	public string GetDirectoryName(string path)
	{
		string result = Path.GetDirectoryName(path);
		Debug.Log($"[FishFS] GetDirectoryName: {path} = {result}");
		return result;
	}

	public string GetFileName(string path)
	{
		string result = Path.GetFileName(path);
		Debug.Log($"[FishFS] GetFileName: {path} = {result}");
		return result;
	}

	public string[] GetFiles(string path, string searchPattern = "*")
	{
		string resolved = ResolvePath(path);
		string[] files = Directory.GetFiles(resolved, searchPattern);
		Debug.Log($"[FishFS] GetFiles: {resolved} (pattern: {searchPattern}, {files.Length} files)");
		return files;
	}

	public string GetFullPath(string path)
	{
		string result = Path.GetFullPath(ResolvePath(path));
		Debug.Log($"[FishFS] GetFullPath: {path} = {result}");
		return result;
	}

	public string GetParentDirectory(string path)
	{
		string resolved = ResolvePath(path);
		DirectoryInfo parent = Directory.GetParent(resolved);
		string result = parent?.FullName;
		Debug.Log($"[FishFS] GetParentDirectory: {resolved} = {result}");
		return result;
	}

	public bool IsDirectory(string path)
	{
		string resolved = ResolvePath(path);
		bool isDir = Directory.Exists(resolved);
		Debug.Log($"[FishFS] IsDirectory: {resolved} = {isDir}");
		return isDir;
	}

	public string ReadAllText(string path)
	{
		string resolved = ResolvePath(path);
		Debug.Log($"[FishFS] Reading file: {resolved}");
		return File.ReadAllText(resolved);
	}

	public void WriteAllText(string path, string contents)
	{
		string resolved = ResolvePath(path);
		Debug.Log($"[FishFS] Writing file: {resolved} ({contents.Length} chars)");
		File.WriteAllText(resolved, contents);
	}
}