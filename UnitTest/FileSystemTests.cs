using System.Numerics;
using FishUI;
using FishUI.Controls;

namespace UnitTest
{
	/// <summary>
	/// Tests for mock file system functionality.
	/// </summary>
	public class FileSystemTests
	{
		[Fact]
		public void MockFileSystem_CanAddAndReadFile()
		{
			using var fixture = new FishUITestFixture();

			fixture.FileSystem.AddFile("test/file.txt", "Hello World");

			var content = fixture.FileSystem.ReadAllText("test/file.txt");

			Assert.Equal("Hello World", content);
		}

		[Fact]
		public void MockFileSystem_Exists_ReturnsTrueForExistingFile()
		{
			using var fixture = new FishUITestFixture();

			fixture.FileSystem.AddFile("test.txt", "content");

			Assert.True(fixture.FileSystem.Exists("test.txt"));
		}

		[Fact]
		public void MockFileSystem_Exists_ReturnsFalseForMissingFile()
		{
			using var fixture = new FishUITestFixture();

			Assert.False(fixture.FileSystem.Exists("nonexistent.txt"));
		}

		[Fact]
		public void MockFileSystem_WriteAllText_CreatesFile()
		{
			using var fixture = new FishUITestFixture();

			fixture.FileSystem.WriteAllText("output.txt", "Test Content");

			Assert.True(fixture.FileSystem.Exists("output.txt"));
			Assert.Equal("Test Content", fixture.FileSystem.ReadAllText("output.txt"));
		}

		[Fact]
		public void MockFileSystem_GetFileName_ExtractsFileName()
		{
			using var fixture = new FishUITestFixture();

			var fileName = fixture.FileSystem.GetFileName("path/to/file.txt");

			Assert.Equal("file.txt", fileName);
		}

		[Fact]
		public void MockFileSystem_GetDirectoryName_ExtractsDirectory()
		{
			using var fixture = new FishUITestFixture();

			var dirName = fixture.FileSystem.GetDirectoryName("path/to/file.txt");

			Assert.Equal("path/to", dirName);
		}

		[Fact]
		public void MockFileSystem_CombinePath_CombinesCorrectly()
		{
			using var fixture = new FishUITestFixture();

			var combined = fixture.FileSystem.CombinePath("path", "file.txt");

			Assert.Equal("path/file.txt", combined);
		}

		[Fact]
		public void MockFileSystem_GetFiles_ReturnsMatchingFiles()
		{
			using var fixture = new FishUITestFixture();

			fixture.FileSystem.AddFile("layouts/main.yaml", "content1");
			fixture.FileSystem.AddFile("layouts/dialog.yaml", "content2");
			fixture.FileSystem.AddFile("layouts/readme.txt", "content3");

			var yamlFiles = fixture.FileSystem.GetFiles("layouts", "*.yaml");

			Assert.Equal(2, yamlFiles.Length);
		}

		[Fact]
		public void MockFileSystem_IsDirectory_IdentifiesDirectories()
		{
			using var fixture = new FishUITestFixture();

			fixture.FileSystem.AddDirectory("mydir");
			fixture.FileSystem.AddFile("myfile.txt", "content");

			Assert.True(fixture.FileSystem.IsDirectory("mydir"));
			Assert.False(fixture.FileSystem.IsDirectory("myfile.txt"));
		}
	}
}
