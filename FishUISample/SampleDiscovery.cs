using FishUIDemos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FishUISample
{
	/// <summary>
	/// Automatically discovers and instantiates all ISample implementations using reflection.
	/// </summary>
	public static class SampleDiscovery
	{
		/// <summary>
		/// Discovers all non-abstract classes that implement ISample in loaded assemblies.
		/// </summary>
		/// <returns>Array of ISample instances sorted by name.</returns>
		public static ISample[] DiscoverSamples()
		{
			List<ISample> samples = new List<ISample>();

			// Get all loaded assemblies
			Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

			foreach (Assembly assembly in assemblies)
			{
				try
				{
					// Skip system assemblies for performance
					string assemblyName = assembly.GetName().Name ?? "";
					if (assemblyName.StartsWith("System") ||
						assemblyName.StartsWith("Microsoft") ||
						assemblyName.StartsWith("Raylib") ||
						assemblyName.StartsWith("YamlDotNet") ||
						assemblyName == "FishUI")
					{
						continue;
					}

					// Find all types that implement ISample
					Type[] types = assembly.GetTypes()
						.Where(t => typeof(ISample).IsAssignableFrom(t) &&
								   !t.IsInterface &&
								   !t.IsAbstract &&
								   t.GetConstructor(Type.EmptyTypes) != null)
						.ToArray();

					foreach (Type type in types)
					{
						try
						{
							ISample instance = (ISample)Activator.CreateInstance(type)!;
							samples.Add(instance);
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Warning: Could not instantiate sample {type.Name}: {ex.Message}");
						}
					}
				}
				catch (ReflectionTypeLoadException)
				{
					// Some assemblies may not be loadable, skip them
				}
			}

			// Sort by name for consistent ordering
			return samples.OrderBy(s => s.Name).ToArray();
		}
	}
}
