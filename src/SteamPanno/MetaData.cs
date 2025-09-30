using System;
using System.Collections.Generic;
using System.Reflection;
using SteamPanno.panno;

namespace SteamPanno
{
	public static class MetaData
	{
		public static IReadOnlyDictionary<string, Type> GenerationTypes { get; private set; }
		public static IReadOnlyDictionary<string, Type> OutpaintingTypes { get; private set; }
		public static string Version { get; private set; }

		static MetaData()
		{
			var generationTypes = new Dictionary<string, Type>();
			var outpaintingTypes = new Dictionary<string, Type>();

			var assembly = Assembly.GetAssembly(typeof(MetaData));
			foreach (var type in assembly.GetTypes())
			{
				if (type.IsAssignableTo(typeof(PannoGameLayoutGenerator)) &&
					type.IsAbstract == false)
				{
					generationTypes.Add(type.Name.Replace(nameof(PannoGameLayoutGenerator), ""), type);
				}
				else if (type.IsAssignableTo(typeof(PannoDrawer)) &&
					type.IsAbstract == false)
				{
					outpaintingTypes.Add(type.Name.Replace(nameof(PannoDrawer), ""), type);
				}
			}

			GenerationTypes = generationTypes;
			OutpaintingTypes = outpaintingTypes;

			Version = typeof(MetaData).Assembly
				.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
				.InformationalVersion;
		}
	}
}
