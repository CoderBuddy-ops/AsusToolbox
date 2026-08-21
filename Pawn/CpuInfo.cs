using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using Microsoft.Win32;

namespace PawnIO;

public static class CpuInfo
{
	public static readonly bool IsAMD = DetectAMD();

	private const string CpuRegKey = "HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0";

	private static readonly Lazy<(string Name, string Caption)> _data = new Lazy<(string, string)>(Load, LazyThreadSafetyMode.ExecutionAndPublication);

	public static string Name => _data.Value.Name;

	public static string Caption => _data.Value.Caption;

	public static int MinCPUUV => AppConfig.Get("min_uv", -40);

	public static int MaxCPUUV => AppConfig.Get("max_uv", 0);

	public static int MinIGPUUV => AppConfig.Get("min_igpu_uv", -30);

	public static int MaxIGPUUV => AppConfig.Get("max_igpu_uv", 0);

	public static int MinTemp => AppConfig.Get("min_temp", 75);

	public static int DefaultTemp => AppConfig.Get("max_temp", 96);

	private static bool DetectAMD()
	{
		if (!X86Base.IsSupported)
		{
			return false;
		}
		(int Eax, int Ebx, int Ecx, int Edx) tuple = X86Base.CpuId(0, 0);
		int item = tuple.Ebx;
		int item2 = tuple.Ecx;
		int item3 = tuple.Edx;
		return MemoryMarshal.Cast<uint, byte>(stackalloc uint[3]
		{
			(uint)item,
			(uint)item3,
			(uint)item2
		}).SequenceEqual("AuthenticAMD"u8);
	}

	public static bool IsSupportedUV()
	{
		if (!Name.Contains("RYZEN AI MAX") && !Name.Contains("Ryzen AI 9") && !Name.Contains("Ryzen 9") && !Name.Contains("4900H") && !Name.Contains("4800H"))
		{
			return Name.Contains("4600H");
		}
		return true;
	}

	public static bool IsSupportedUViGPU()
	{
		return Name.Contains("6900H");
	}

	private static (string Name, string Caption) Load()
	{
		try
		{
			using RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("HARDWARE\\DESCRIPTION\\System\\CentralProcessor\\0");
			if (registryKey != null)
			{
				string item = registryKey.GetValue("ProcessorNameString")?.ToString()?.Trim() ?? string.Empty;
				string item2 = registryKey.GetValue("Identifier")?.ToString()?.Trim() ?? string.Empty;
				return (Name: item, Caption: item2);
			}
		}
		catch
		{
		}
		return (Name: string.Empty, Caption: string.Empty);
	}
}
