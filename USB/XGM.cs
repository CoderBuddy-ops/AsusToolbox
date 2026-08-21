using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HidSharp;

namespace Asus.USB;

public static class XGM
{
	private const byte XGM_REPORT_ID = 94;

	private const int ASUS_ID = 2821;

	private static readonly int[] deviceIds = new int[5] { 6512, 6810, 7208, 7209, 7105 };

	public static HidDevice? GetDevice()
	{
		try
		{
			return DeviceList.Local.GetHidDevices(2821, null, null).FirstOrDefault((HidDevice device) => deviceIds.Contains(device.ProductID) && device.CanOpen && device.GetMaxFeatureReportLength() >= 300);
		}
		catch (Exception value)
		{
			Logger.WriteLine($"Error getting XGM device: {value}");
			return null;
		}
	}

	public static bool IsConnected()
	{
		return GetDevice() != null;
	}

	public static void Write(byte[] data)
	{
		try
		{
			HidDevice device = GetDevice();
			if (device == null)
			{
				Logger.WriteLine("XGM SUB device not found");
				return;
			}
			using HidStream hidStream = device.Open();
			byte[] array = new byte[300];
			data.CopyTo(array, 0);
			hidStream.SetFeature(array);
			Logger.WriteLine($"XGM-{device.ProductID}|{device.GetMaxFeatureReportLength()}:{BitConverter.ToString(data)}");
		}
		catch (Exception value)
		{
			Logger.WriteLine($"Error accessing XGM device: {value}");
		}
	}

	public static void Init()
	{
		Task.Run(delegate
		{
			if (IsConnected())
			{
				Write(Encoding.ASCII.GetBytes("^ASUS Tech.Inc."));
				Write(new byte[3] { 94, 228, 2 });
				Light(AppConfig.Is("xmg_light"));
			}
		});
	}

	public static void Light(bool status)
	{
		Write(new byte[3]
		{
			94,
			197,
			(byte)(status ? 80 : 0)
		});
		Write(new byte[4]
		{
			94,
			189,
			0,
			status ? ((byte)1) : ((byte)0)
		});
	}

	public static void LightBrightness(int brightness)
	{
		Task.Run(delegate
		{
			if (IsConnected())
			{
				byte[] obj = new byte[5] { 94, 186, 197, 196, 0 };
				obj[4] = (byte)brightness;
				Write(obj);
			}
		});
	}

	public static void LightMode(AuraMode mode, Color color, Color color2, int speed)
	{
		Task.Run(delegate
		{
			if (IsConnected())
			{
				byte[] array = Aura.AuraMessage(mode, color, color2, speed);
				array[0] = 94;
				Write(array);
				Write(new byte[2] { 94, 180 });
				Write(new byte[2] { 94, 181 });
			}
		});
	}

	public static void InitLight()
	{
		Task.Run(delegate
		{
			if (IsConnected())
			{
				Light(AppConfig.Is("xmg_light"));
			}
		});
	}

	public static void NotifyShutdown()
	{
		if (IsConnected())
		{
			Write(new byte[3] { 94, 228, 1 });
		}
	}

	public static void Reset()
	{
		Task.Run(delegate
		{
			if (IsConnected())
			{
				Write(new byte[3] { 94, 209, 2 });
			}
		});
	}

	public static void SetFan(byte[] curve)
	{
		Task.Run(delegate
		{
			if (!AsusACPI.IsInvalidCurve(curve) && IsConnected())
			{
				List<byte> list = new List<byte>();
				list.Add(94);
				list.Add(209);
				list.Add(1);
				list.AddRange(curve);
				Write(list.ToArray());
			}
		});
	}
}
