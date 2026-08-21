using System;
using System.IO;
using System.Reflection;

namespace PawnIO;

public sealed class IntelMsr : IDisposable
{
	private const uint MSR_RAPL_POWER_UNIT = 1542u;

	private const uint MSR_PKG_ENERGY_STATUS = 1553u;

	private readonly PawnIOWrapper _io = new PawnIOWrapper();

	private bool _init;

	private double _energyUnit;

	private uint _lastEnergy;

	private long _lastTick;

	public bool IsInitialized => _init;

	public bool Initialize(Assembly assembly)
	{
		string text = assembly.GetName().Name + ".IntelMSR.bin";
		using Stream stream = assembly.GetManifestResourceStream(text) ?? throw new InvalidOperationException("Embedded resource '" + text + "' not found.");
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return Initialize(memoryStream.ToArray());
	}

	public bool Initialize(byte[] moduleData)
	{
		if (_init)
		{
			return true;
		}
		if (_io.Connect() != 0 || !_io.LoadModule(moduleData))
		{
			return false;
		}
		if (!ReadMsr(1542u, out var value))
		{
			return false;
		}
		int num = (int)((value >> 8) & 0x1F);
		_energyUnit = 1.0 / (double)(ulong)(1L << num);
		_init = true;
		return true;
	}

	public float? GetPackagePower()
	{
		if (!_init || !ReadMsr(1553u, out var value))
		{
			return null;
		}
		uint num = (uint)value;
		long tickCount = Environment.TickCount64;
		if (_lastTick == 0L)
		{
			_lastEnergy = num;
			_lastTick = tickCount;
			return null;
		}
		double num2 = (double)(tickCount - _lastTick) / 1000.0;
		if (num2 < 0.05)
		{
			return null;
		}
		double num3 = (double)(num - _lastEnergy) * _energyUnit;
		_lastEnergy = num;
		_lastTick = tickCount;
		return (float)(num3 / num2);
	}

	private bool ReadMsr(uint msr, out ulong value)
	{
		value = 0uL;
		ulong[] array = new ulong[1];
		if (!_io.Execute("ioctl_read_msr", new ulong[1] { msr }, array))
		{
			return false;
		}
		value = array[0];
		return true;
	}

	public void Dispose()
	{
		_io.Dispose();
	}
}
