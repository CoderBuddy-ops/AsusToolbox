using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Asus.Helpers;
using HidSharp;
using HidSharp.Reports;

namespace Asus.USB;

public static class AsusLampArray
{
	private struct Lamp
	{
		public int Zone;

		public double T;
	}

	private const byte FLAG_COMPLETE = 1;

	private const int MULTI_MAX = 8;

	private const byte PURPOSE_CONTROL = 1;

	private static HidDevice? device;

	private static HidStream? stream;

	private static byte ridBase;

	private static int featLen;

	private static volatile bool probed;

	private static bool probing;

	private static bool failLogged;

	private static bool controlled;

	private static Lamp[] lamps = Array.Empty<Lamp>();

	private static byte RidAttr => (byte)(ridBase + 1);

	private static byte RidRequest => (byte)(ridBase + 2);

	private static byte RidResponse => (byte)(ridBase + 3);

	private static byte RidMulti => (byte)(ridBase + 4);

	private static byte RidControl => (byte)(ridBase + 6);

	public static bool Available
	{
		get
		{
			if (probed)
			{
				return device != null;
			}
			if (!AppConfig.IsLampArray())
			{
				probed = true;
				return false;
			}
			if (!probing)
			{
				probing = true;
				Task.Run((Action)Probe);
			}
			return false;
		}
	}

	public static bool Probing
	{
		get
		{
			if (probing)
			{
				return !probed;
			}
			return false;
		}
	}

	private static void Probe()
	{
		device = FindDevice();
		if (device != null && Reopen())
		{
			featLen = device.GetMaxFeatureReportLength();
			ReadLamps(ReadLampCount());
		}
		if (lamps.Length == 0)
		{
			stream?.Dispose();
			stream = null;
			device = null;
			Logger.WriteLine("LampArray: not available");
		}
		else
		{
			Logger.WriteLine($"LampArray: rid=0x{ridBase:X2} feat={featLen} lamps={lamps.Length}");
		}
		probed = true;
		Aura.ApplyAura();
	}

	private static bool Reopen()
	{
		if (stream != null)
		{
			return true;
		}
		try
		{
			stream = device.Open();
			failLogged = false;
			return true;
		}
		catch (Exception ex)
		{
			if (!failLogged)
			{
				Logger.WriteLine("LampArray: open failed " + ex.Message);
			}
			failLogged = true;
			return false;
		}
	}

	private static HidDevice? FindDevice()
	{
		byte[] array = new byte[2] { 0, 64 };
		foreach (byte b in array)
		{
			foreach (HidDevice item in AsusHid.FindDevices((byte)(b + 4), AsusHid.MAIN_AURA_PIDS))
			{
				if (item.GetReportDescriptor().TryGetReport(ReportType.Feature, (byte)(b + 6), out var _))
				{
					ridBase = b;
					return item;
				}
			}
		}
		return null;
	}

	private static int ReadLampCount()
	{
		try
		{
			byte[] array = new byte[featLen];
			array[0] = RidAttr;
			lock (AsusHid.hidLock)
			{
				stream.GetFeature(array);
			}
			int num = array[1] | (array[2] << 8);
			if (num > 0 && num <= 512)
			{
				return num;
			}
		}
		catch (Exception ex)
		{
			Logger.WriteLine("LampArray: attr read error " + ex.Message);
		}
		return 0;
	}

	private static void ReadLamps(int count)
	{
		int[] array = new int[count];
		bool[] array2 = new bool[count];
		int num = int.MaxValue;
		int num2 = int.MinValue;
		int num3 = int.MaxValue;
		int num4 = int.MinValue;
		for (int i = 0; i < count; i++)
		{
			try
			{
				byte[] array3 = new byte[featLen];
				array3[0] = RidRequest;
				array3[1] = (byte)i;
				array3[2] = (byte)(i >> 8);
				byte[] array4 = new byte[featLen];
				array4[0] = RidResponse;
				lock (AsusHid.hidLock)
				{
					stream.SetFeature(array3);
					stream.GetFeature(array4);
				}
				array[i] = BitConverter.ToInt32(array4, 3);
				array2[i] = (BitConverter.ToUInt32(array4, 19) & 1) != 0;
			}
			catch
			{
			}
			if (array2[i])
			{
				num = Math.Min(num, array[i]);
				num2 = Math.Max(num2, array[i]);
			}
			else
			{
				num3 = Math.Min(num3, array[i]);
				num4 = Math.Max(num4, array[i]);
			}
		}
		lamps = new Lamp[count];
		for (int j = 0; j < count; j++)
		{
			int num5 = (array2[j] ? num : num3);
			int num6 = Math.Max(1, (array2[j] ? num2 : num4) - num5);
			lamps[j] = new Lamp
			{
				Zone = ((!array2[j]) ? 4 : 0),
				T = (double)(array[j] - num5) / (double)num6
			};
		}
	}

	private static void Send(byte[] data)
	{
		HidStream hidStream = stream;
		if (hidStream == null)
		{
			return;
		}
		byte[] array = new byte[featLen];
		Array.Copy(data, array, Math.Min(data.Length, featLen));
		try
		{
			lock (AsusHid.hidLock)
			{
				hidStream.SetFeature(array);
			}
		}
		catch (Exception ex)
		{
			Logger.WriteLine("LampArray: write error " + ex.Message);
			stream = null;
			controlled = false;
			hidStream.Dispose();
		}
	}

	private static void Control()
	{
		lock (AsusHid.hidLock)
		{
			AsusHid.SetFeatureAura(new byte[4] { 93, 192, 3, 1 });
			Send(new byte[2] { RidControl, 1 });
			Send(new byte[2] { RidControl, 0 });
		}
		controlled = stream != null;
	}

	public static void SetMode(AuraMode mode)
	{
		if (Available)
		{
			bool flag;
			switch (mode)
			{
			case AuraMode.HEATMAP:
			case AuraMode.AMBIENT:
			case AuraMode.GRADIENT:
			case AuraMode.ZONETEST:
			case AuraMode.AUDIO:
			case AuraMode.AUDIOPULSE:
				flag = true;
				break;
			default:
				flag = false;
				break;
			}
			if (flag)
			{
				controlled = false;
			}
			else
			{
				Reset();
			}
		}
	}

	private static void Reset()
	{
		lock (AsusHid.hidLock)
		{
			if (Reopen())
			{
				Send(new byte[2] { RidControl, 1 });
			}
			AsusHid.SetFeatureAura(new byte[5] { 93, 192, 4, 1, 1 });
		}
		controlled = false;
	}

	public static void Release()
	{
		if (controlled)
		{
			Reset();
		}
	}

	private static Color Blend(Color[] zones, int off, double t)
	{
		double num = Math.Clamp(t, 0.0, 1.0) * 3.0;
		int num2 = (int)num;
		return ColorUtils.GetWeightedAverage(zones[off + num2], zones[off + Math.Min(3, num2 + 1)], (float)(num - (double)num2));
	}

	public static void SetColor(Color c)
	{
		SetColors(Enumerable.Repeat(c, 8).ToArray());
	}

	public static void SetColors(Color[] zones)
	{
		if (Available && Reopen() && zones.Length >= 8)
		{
			if (!controlled)
			{
				Control();
			}
			Color[] array = new Color[lamps.Length];
			for (int i = 0; i < lamps.Length; i++)
			{
				array[i] = Blend(zones, lamps[i].Zone, lamps[i].T);
			}
			SendMulti(array);
		}
	}

	private static void SendMulti(Color[] arr)
	{
		for (int i = 0; i < arr.Length; i += 8)
		{
			int num = Math.Min(8, arr.Length - i);
			byte[] array = new byte[51];
			array[0] = RidMulti;
			array[1] = (byte)num;
			array[2] = ((i + num >= arr.Length) ? ((byte)1) : ((byte)0));
			int num2 = 3;
			int num3 = 19;
			for (int j = 0; j < num; j++)
			{
				int num4 = i + j;
				array[num2 + j * 2] = (byte)(num4 & 0xFF);
				array[num2 + j * 2 + 1] = (byte)(num4 >> 8);
				Color color = arr[num4];
				array[num3 + j * 4] = color.R;
				array[num3 + j * 4 + 1] = color.G;
				array[num3 + j * 4 + 2] = color.B;
				array[num3 + j * 4 + 3] = byte.MaxValue;
			}
			Send(array);
		}
	}
}
