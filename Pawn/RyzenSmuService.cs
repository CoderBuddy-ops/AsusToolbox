using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace PawnIO;

public sealed class RyzenSmuService : IDisposable
{
	private const int MAILBOX_TIMEOUT_MS = 200;

	private readonly PawnIOWrapper _io = new PawnIOWrapper();

	private bool _init;

	private bool _disposed;

	private CpuCodeName _cpu;

	private uint _smuVer;

	private readonly Mutex _smuMutex = new Mutex();

	public bool IsInitialized => _init;

	public CpuCodeName CpuCodeName => _cpu;

	public uint SmuVersion => _smuVer;

	public CpuFamily Family => GetFamily(_cpu);

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
		GetCodeName(out _cpu);
		GetSmuVersion(out _smuVer);
		_init = true;
		return true;
	}

	public static bool IsPawnInstalled()
	{
		using PawnIOWrapper pawnIOWrapper = new PawnIOWrapper();
		return pawnIOWrapper.Connect() != PawnIOWrapper.ConnectResult.NotInstalled;
	}

	public bool Initialize(Assembly assembly)
	{
		string text = assembly.GetName().Name + ".RyzenSMU.bin";
		using Stream stream = assembly.GetManifestResourceStream(text) ?? throw new InvalidOperationException("Embedded resource '" + text + "' not found.");
		using MemoryStream memoryStream = new MemoryStream();
		stream.CopyTo(memoryStream);
		return Initialize(memoryStream.ToArray());
	}

	public bool CanSetTDP()
	{
		return _cpu != CpuCodeName.Undefined;
	}

	public bool CanSetCoAll()
	{
		return _cpu != CpuCodeName.Undefined;
	}

	public bool CanSetThm()
	{
		return _cpu != CpuCodeName.Undefined;
	}

	public bool SetAllLimits(int stapmW, int fastW, int slowW)
	{
		return (SetStapm(stapmW) == SmuStatus.OK) & (SetFast(fastW) == SmuStatus.OK) & (SetSlow(slowW) == SmuStatus.OK);
	}

	public void SetAllLimits(int stapmW, int fastW, int slowW, out SmuStatus stapm, out SmuStatus fast, out SmuStatus slow)
	{
		stapm = SetStapm(stapmW);
		fast = SetFast(fastW);
		slow = SetSlow(slowW);
	}

	public SmuStatus SetCoAll(int value)
	{
		uint arg = EncodeCurve(value);
		switch (Family)
		{
		case CpuFamily.Renoir:
			return SendMp1(85u, arg);
		case CpuFamily.Mobile:
		case CpuFamily.StrixPoint:
			return SendMp1(76u, arg);
		case CpuFamily.StrixHalo:
		{
			SmuStatus smuStatus = SendMp1(76u, arg);
			return (smuStatus == SmuStatus.OK) ? smuStatus : SendPsmu(93u, arg);
		}
		case CpuFamily.Raphael:
			return SendPsmu(7u, arg);
		default:
			return SmuStatus.Failed;
		}
	}

	public SmuStatus SetCoPer(int core, int value)
	{
		uint arg = (uint)((((Family == CpuFamily.Raphael) ? ((core / 8 << 8) | (core % 8)) : core) << 20) | (value & 0xFFFF));
		switch (Family)
		{
		case CpuFamily.Renoir:
			return SendMp1(84u, arg);
		case CpuFamily.Mobile:
		case CpuFamily.StrixPoint:
		case CpuFamily.StrixHalo:
			return SendMp1(75u, arg);
		case CpuFamily.Raphael:
			return SendPsmu(6u, arg);
		default:
			return SmuStatus.Failed;
		}
	}

	public SmuStatus SetCoGfx(int value)
	{
		uint arg = EncodeCurve(value);
		switch (Family)
		{
		case CpuFamily.Renoir:
			return SendMp1(100u, arg);
		case CpuFamily.Mobile:
		case CpuFamily.StrixHalo:
			return SendPsmu(183u, arg);
		default:
			return SmuStatus.Failed;
		}
	}

	public SmuStatus SetThm(int celsius)
	{
		switch (Family)
		{
		case CpuFamily.Raven:
			return SendMp1(31u, (uint)celsius);
		case CpuFamily.Renoir:
		case CpuFamily.Mobile:
		case CpuFamily.StrixPoint:
		case CpuFamily.StrixHalo:
			return SendMp1(25u, (uint)celsius);
		case CpuFamily.Matisse:
			return SendMp1(62u, (uint)celsius);
		case CpuFamily.Raphael:
			return SendMp1(63u, (uint)celsius);
		default:
			return SmuStatus.Failed;
		}
	}

	public bool GetCodeName(out CpuCodeName codeName)
	{
		codeName = CpuCodeName.Undefined;
		ulong[] array = new ulong[1];
		if (_io.Execute("ioctl_get_code_name", null, array))
		{
			codeName = (CpuCodeName)array[0];
			return true;
		}
		return false;
	}

	public bool GetSmuVersion(out uint version)
	{
		version = 0u;
		ulong[] array = new ulong[1];
		if (_io.Execute("ioctl_get_smu_version", null, array))
		{
			version = (uint)array[0];
			return true;
		}
		return false;
	}

	public PowerLimits? GetPowerLimits()
	{
		ulong[] array = new ulong[2];
		if (!_io.Execute("ioctl_resolve_pm_table", null, array))
		{
			return null;
		}
		uint num = (uint)array[0];
		_io.Execute("ioctl_update_pm_table", null, null);
		Thread.Sleep(100);
		if (!_io.Execute("ioctl_update_pm_table", null, null))
		{
			return null;
		}
		Thread.Sleep(200);
		ulong[] array2 = new ulong[64];
		if (!_io.Execute("ioctl_read_pm_table", null, array2))
		{
			return null;
		}
		ReadOnlySpan<float> readOnlySpan = MemoryMarshal.Cast<ulong, float>(array2);
		StringBuilder stringBuilder = new StringBuilder();
		StringBuilder stringBuilder2 = stringBuilder;
		StringBuilder stringBuilder3 = stringBuilder2;
		StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(22, 1, stringBuilder2);
		handler.AppendLiteral("PMTable ver=0x");
		handler.AppendFormatted(num, "X6");
		handler.AppendLiteral(" floats:");
		stringBuilder3.Append(ref handler);
		for (int i = 0; i < readOnlySpan.Length; i++)
		{
			stringBuilder2 = stringBuilder;
			StringBuilder stringBuilder4 = stringBuilder2;
			handler = new StringBuilder.AppendInterpolatedStringHandler(4, 2, stringBuilder2);
			handler.AppendLiteral(" [");
			handler.AppendFormatted(i);
			handler.AppendLiteral("]=");
			handler.AppendFormatted(readOnlySpan[i], "G6");
			stringBuilder4.Append(ref handler);
		}
		Logger.WriteLine(stringBuilder.ToString());
		if (readOnlySpan[0] == 0f)
		{
			return null;
		}
		int tctlIndex = GetTctlIndex(num);
		if (tctlIndex < 0 || readOnlySpan.Length <= tctlIndex)
		{
			return null;
		}
		return new PowerLimits(readOnlySpan[0], readOnlySpan[2], readOnlySpan[4], readOnlySpan[tctlIndex], HasApuSlowField(num) ? new float?(readOnlySpan[6]) : ((float?)null));
	}

	private static int GetTctlIndex(uint tableVersion)
	{
		switch (tableVersion >> 16)
		{
		case 30u:
		case 100u:
			return 22;
		case 55u:
		case 63u:
		case 64u:
		case 69u:
		case 76u:
		case 93u:
		case 101u:
			return 16;
		case 84u:
		case 98u:
			return 10;
		default:
			return 16;
		}
	}

	private static bool HasApuSlowField(uint tableVersion)
	{
		uint num = tableVersion >> 16;
		if (num != 30)
		{
			return num != 84;
		}
		return false;
	}

	public static CpuFamily GetFamily(CpuCodeName cpu)
	{
		switch (cpu)
		{
		case CpuCodeName.Colfax:
		case CpuCodeName.Threadripper:
		case CpuCodeName.CastlePeak:
		case CpuCodeName.SummitRidge:
		case CpuCodeName.PinnacleRidge:
			return CpuFamily.Zen1Desktop;
		case CpuCodeName.Picasso:
		case CpuCodeName.RavenRidge:
		case CpuCodeName.RavenRidge2:
		case CpuCodeName.Dali:
			return CpuFamily.Raven;
		case CpuCodeName.Renoir:
		case CpuCodeName.Cezanne:
		case CpuCodeName.Lucienne:
			return CpuFamily.Renoir;
		case CpuCodeName.Matisse:
		case CpuCodeName.Vermeer:
			return CpuFamily.Matisse;
		case CpuCodeName.Rembrandt:
		case CpuCodeName.Vangogh:
		case CpuCodeName.Phoenix:
		case CpuCodeName.Phoenix2:
		case CpuCodeName.Mendocino:
		case CpuCodeName.HawkPoint:
			return CpuFamily.Mobile;
		case CpuCodeName.Raphael:
		case CpuCodeName.GraniteRidge:
		case CpuCodeName.DragonRange:
			return CpuFamily.Raphael;
		case CpuCodeName.FireFlight:
			return CpuFamily.Mobile;
		case CpuCodeName.StrixPoint:
		case CpuCodeName.KrackanPoint:
		case CpuCodeName.KrackanPoint2:
			return CpuFamily.StrixPoint;
		case CpuCodeName.StrixHalo:
			return CpuFamily.StrixHalo;
		case CpuCodeName.ShimadaPeak:
			return CpuFamily.ShimadaPeak;
		default:
			return CpuFamily.Unknown;
		}
	}

	private static uint EncodeCurve(int steps)
	{
		return (uint)(1048576 - -steps);
	}

	private SmuStatus SetStapm(int watts)
	{
		uint arg = (uint)(watts * 1000);
		switch (Family)
		{
		case CpuFamily.Raven:
			return SendMp1(26u, arg);
		case CpuFamily.Renoir:
		{
			SmuStatus result = SendMp1(20u, arg);
			SendPsmu(49u, arg);
			return result;
		}
		case CpuFamily.Mobile:
		case CpuFamily.StrixPoint:
		case CpuFamily.StrixHalo:
			return SendMp1(20u, arg);
		case CpuFamily.Raphael:
			return SendMp1(79u, arg);
		default:
			return SmuStatus.Failed;
		}
	}

	private SmuStatus SetFast(int watts)
	{
		uint arg = (uint)(watts * 1000);
		switch (Family)
		{
		case CpuFamily.Raven:
			return SendMp1(27u, arg);
		case CpuFamily.Renoir:
		{
			SmuStatus result = SendMp1(21u, arg);
			SendPsmu(50u, arg);
			return result;
		}
		case CpuFamily.Mobile:
		case CpuFamily.StrixPoint:
		case CpuFamily.StrixHalo:
			return SendMp1(21u, arg);
		case CpuFamily.Raphael:
			return SendMp1(62u, arg);
		default:
			return SmuStatus.Failed;
		}
	}

	private SmuStatus SetSlow(int watts)
	{
		uint arg = (uint)(watts * 1000);
		switch (Family)
		{
		case CpuFamily.Raven:
			return SendMp1(28u, arg);
		case CpuFamily.Renoir:
		{
			SmuStatus result = SendMp1(22u, arg);
			SendPsmu(51u, arg);
			SendPsmu(52u, arg);
			return result;
		}
		case CpuFamily.Mobile:
		case CpuFamily.StrixPoint:
		case CpuFamily.StrixHalo:
			return SendMp1(22u, arg);
		case CpuFamily.Raphael:
			return SendMp1(95u, arg);
		default:
			return SmuStatus.Failed;
		}
	}

	private bool ReadReg(uint addr, out uint value)
	{
		value = 0u;
		ulong[] array = new ulong[1];
		if (_io.Execute("ioctl_read_smu_register", new ulong[1] { addr }, array))
		{
			value = (uint)array[0];
			return true;
		}
		return false;
	}

	private bool WriteReg(uint addr, uint value)
	{
		return _io.Execute("ioctl_write_smu_register", new ulong[2] { addr, value }, null);
	}

	private bool WaitForMailboxIdle(uint rspAddr)
	{
		Stopwatch stopwatch = Stopwatch.StartNew();
		int num = 0;
		while (stopwatch.ElapsedMilliseconds < 200)
		{
			if (!ReadReg(rspAddr, out var value))
			{
				return false;
			}
			if (value != 0)
			{
				return true;
			}
			num++;
			if (num > 256)
			{
				Thread.Sleep(1);
			}
			else if (num > 32)
			{
				Thread.Yield();
			}
		}
		return false;
	}

	private SmuStatus SendMp1(uint cmd, uint arg)
	{
		GetMp1Addrs(out var cmd2, out var rsp, out var arg2);
		if (cmd2 == 0)
		{
			return SmuStatus.Failed;
		}
		uint[] response;
		return MailboxRaw(cmd2, rsp, arg2, cmd, new uint[1] { arg }, out response);
	}

	private SmuStatus SendPsmu(uint cmd, uint arg)
	{
		GetPsmuAddrs(out var cmd2, out var rsp, out var arg2);
		if (cmd2 == 0)
		{
			return SmuStatus.Failed;
		}
		uint[] response;
		return MailboxRaw(cmd2, rsp, arg2, cmd, new uint[1] { arg }, out response);
	}

	private SmuStatus MailboxRaw(uint cmdAddr, uint rspAddr, uint argAddr, uint cmd, uint[] args, out uint[] response)
	{
		response = new uint[6];
		if (_disposed)
		{
			return SmuStatus.Failed;
		}
		if (!_smuMutex.WaitOne(5000))
		{
			return SmuStatus.Failed;
		}
		try
		{
			if (_disposed)
			{
				return SmuStatus.Failed;
			}
			if (!WaitForMailboxIdle(rspAddr))
			{
				return SmuStatus.CmdRejectedBusy;
			}
			if (!WriteReg(rspAddr, 0u))
			{
				return SmuStatus.Failed;
			}
			for (int i = 0; i < 6; i++)
			{
				uint value = ((args != null && i < args.Length) ? args[i] : 0u);
				if (!WriteReg(argAddr + (uint)(i * 4), value))
				{
					return SmuStatus.Failed;
				}
			}
			if (!WriteReg(cmdAddr, cmd))
			{
				return SmuStatus.Failed;
			}
			uint value2 = 0u;
			Stopwatch stopwatch = Stopwatch.StartNew();
			int num = 0;
			while (stopwatch.ElapsedMilliseconds < 200)
			{
				if (!ReadReg(rspAddr, out value2))
				{
					return SmuStatus.Failed;
				}
				if (value2 != 0)
				{
					break;
				}
				num++;
				if (num > 256)
				{
					Thread.Sleep(1);
				}
				else if (num > 32)
				{
					Thread.Yield();
				}
			}
			switch (value2)
			{
			case 0u:
				return SmuStatus.Failed;
			default:
				return (SmuStatus)value2;
			case 1u:
			{
				for (int j = 0; j < 6; j++)
				{
					if (!ReadReg(argAddr + (uint)(j * 4), out response[j]))
					{
						return SmuStatus.Failed;
					}
				}
				return SmuStatus.OK;
			}
			}
		}
		finally
		{
			_smuMutex.ReleaseMutex();
		}
	}

	private void GetMp1Addrs(out uint cmd, out uint rsp, out uint arg)
	{
		switch (Family)
		{
		case CpuFamily.Zen1Desktop:
			cmd = 61932840u;
			rsp = 61932900u;
			arg = 61932952u;
			break;
		case CpuFamily.Raven:
		case CpuFamily.Renoir:
			cmd = 61932840u;
			rsp = 61932900u;
			arg = 61933976u;
			break;
		case CpuFamily.Mobile:
			cmd = 61932840u;
			rsp = 61932920u;
			arg = 61933976u;
			break;
		case CpuFamily.StrixPoint:
		case CpuFamily.StrixHalo:
			cmd = 61933864u;
			rsp = 61933944u;
			arg = 61933976u;
			break;
		case CpuFamily.Matisse:
		case CpuFamily.Raphael:
			cmd = 61932848u;
			rsp = 61932924u;
			arg = 61934020u;
			break;
		default:
			cmd = 0u;
			rsp = 0u;
			arg = 0u;
			break;
		}
	}

	private void GetPsmuAddrs(out uint cmd, out uint rsp, out uint arg)
	{
		switch (Family)
		{
		case CpuFamily.Zen1Desktop:
			cmd = 61932828u;
			rsp = 61932904u;
			arg = 61932944u;
			break;
		case CpuFamily.Matisse:
		case CpuFamily.Raphael:
			cmd = 61932836u;
			rsp = 61932912u;
			arg = 61934144u;
			break;
		case CpuFamily.Raven:
		case CpuFamily.Renoir:
		case CpuFamily.Mobile:
		case CpuFamily.StrixPoint:
		case CpuFamily.StrixHalo:
			cmd = 61934112u;
			rsp = 61934208u;
			arg = 61934216u;
			break;
		case CpuFamily.ShimadaPeak:
			cmd = 61933860u;
			rsp = 61933936u;
			arg = 61934144u;
			break;
		default:
			cmd = 0u;
			rsp = 0u;
			arg = 0u;
			break;
		}
	}

	public void Dispose()
	{
		if (!_disposed)
		{
			_io.Dispose();
			_smuMutex.Dispose();
			_disposed = true;
		}
	}
}
