using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Asus.Helpers;
using Asus.Input;
using Asus.Properties;

namespace Asus.USB;

public static class Aura
{
	public static class CustomRGB
	{
		private static class AmbientData
		{
			public enum StretchMode
			{
				STRETCH_ANDSCANS = 1,
				STRETCH_ORSCANS,
				STRETCH_DELETESCANS,
				STRETCH_HALFTONE
			}

			public static Color[] result = new Color[AURA_ZONES];

			public static ColorUtils.SmoothColor[] Colors = (from h in Enumerable.Repeat(0, AURA_ZONES)
				select new ColorUtils.SmoothColor()).ToArray();

			[DllImport("user32.dll")]
			private static extern bool GetGUIThreadInfo(uint idThread, int[] gui);

			public static bool IsMoveSize()
			{
				int[] array = new int[18]
				{
					72, 0, 0, 0, 0, 0, 0, 0, 0, 0,
					0, 0, 0, 0, 0, 0, 0, 0
				};
				if (GetGUIThreadInfo(0u, array))
				{
					return (array[1] & 2) != 0;
				}
				return false;
			}

			[DllImport("user32.dll")]
			private static extern nint GetDesktopWindow();

			[DllImport("user32.dll")]
			private static extern nint GetWindowDC(nint hWnd);

			[DllImport("gdi32.dll")]
			private static extern nint CreateCompatibleDC(nint hDC);

			[DllImport("gdi32.dll")]
			private static extern nint CreateCompatibleBitmap(nint hDC, int nWidth, int nHeight);

			[DllImport("gdi32.dll")]
			private static extern nint SelectObject(nint hDC, nint hObject);

			[DllImport("user32.dll")]
			private static extern bool ReleaseDC(nint hWnd, nint hDC);

			[DllImport("gdi32.dll")]
			private static extern bool DeleteDC(nint hdc);

			[DllImport("gdi32.dll")]
			private static extern bool DeleteObject(nint hObject);

			[DllImport("gdi32.dll")]
			private static extern bool StretchBlt(nint hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest, nint hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, int dwRop);

			[DllImport("gdi32.dll")]
			private static extern bool SetStretchBltMode(nint hdc, StretchMode iStretchMode);

			public static Bitmap CamptureScreen(Rectangle rec, int out_w, int out_h)
			{
				nint desktopWindow = GetDesktopWindow();
				nint windowDC = GetWindowDC(desktopWindow);
				nint num = CreateCompatibleDC(windowDC);
				nint num2 = CreateCompatibleBitmap(windowDC, out_w, out_h);
				nint hObject = SelectObject(num, num2);
				SetStretchBltMode(num, StretchMode.STRETCH_DELETESCANS);
				StretchBlt(num, 0, 0, out_w, out_h, windowDC, rec.X, rec.Y, rec.Width, rec.Height, 13369376);
				SelectObject(num, hObject);
				DeleteDC(num);
				ReleaseDC(desktopWindow, windowDC);
				Bitmap bitmap = Image.FromHbitmap(num2, IntPtr.Zero);
				DeleteObject(num2);
				return bitmap;
			}

			public static Bitmap ResizeImage(Image image, int width, int height)
			{
				Rectangle destRect = new Rectangle(0, 0, width, height);
				Bitmap bitmap = new Bitmap(width, height);
				bitmap.SetResolution(image.HorizontalResolution, image.VerticalResolution);
				using Graphics graphics = Graphics.FromImage(bitmap);
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.Bicubic;
				graphics.SmoothingMode = SmoothingMode.None;
				graphics.PixelOffsetMode = PixelOffsetMode.None;
				using ImageAttributes imageAttributes = new ImageAttributes();
				imageAttributes.SetWrapMode(WrapMode.TileFlipXY);
				graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, imageAttributes);
				return bitmap;
			}

			public static Color GetMostUsedColor(Bitmap bitMap)
			{
				Dictionary<int, int> dictionary = new Dictionary<int, int>();
				for (int i = 0; i < bitMap.Size.Width; i++)
				{
					for (int j = 0; j < bitMap.Size.Height; j++)
					{
						int num = bitMap.GetPixel(i, j).ToArgb();
						if (dictionary.Keys.Contains(num))
						{
							dictionary[num]++;
						}
						else
						{
							dictionary.Add(num, 1);
						}
					}
				}
				return Color.FromArgb(dictionary.OrderByDescending((KeyValuePair<int, int> x) => x.Value).ToDictionary((KeyValuePair<int, int> x) => x.Key, (KeyValuePair<int, int> x) => x.Value).First()
					.Key);
				}
			}

			private static int tempFreeze = AppConfig.Get("temp_freeze", 20);

			private static int tempCold = AppConfig.Get("temp_cold", 40);

			private static int tempWarm = AppConfig.Get("temp_warm", 65);

			private static int tempHot = AppConfig.Get("temp_hot", 90);

			private static Color colorFreeze = ColorTranslator.FromHtml(AppConfig.GetString("color_freeze", "#0000FF"));

			private static Color colorCold = ColorTranslator.FromHtml(AppConfig.GetString("color_cold", "#008000"));

			private static Color colorWarm = ColorTranslator.FromHtml(AppConfig.GetString("color_warm", "#FFFF00"));

			private static Color colorHot = ColorTranslator.FromHtml(AppConfig.GetString("color_hot", "#FF0000"));

			private static float battLow = 20f;

			private static float battMid = 60f;

			private static float battHigh = 100f;

			private static Color colorLow = Color.Red;

			private static Color colorMid = Color.Yellow;

			private static Color colorHigh = Color.Lime;

			private static Color colorUltimate = ColorTranslator.FromHtml(AppConfig.GetString("color_ultimate", "#FF0000"));

			private static Color colorStandard = ColorTranslator.FromHtml(AppConfig.GetString("color_standard", "#FFFF00"));

			private static Color colorEco = ColorTranslator.FromHtml(AppConfig.GetString("color_eco", "#008000"));

			public static void ApplyGradient()
			{
				if (!isStrix && !isStrix4Zone)
				{
					ApplyDirect(Color1, init: true);
					return;
				}
				Color[] array = new Color[AURA_ZONES];
				for (int i = 0; i < 4; i++)
				{
					float weight = (float)i / 3f;
					array[i] = ColorUtils.GetWeightedAverage(Color2, Color1, weight);
				}
				int[] array2 = ((!AsusLampArray.Available) ? new int[4] { 7, 6, 4, 5 } : new int[4] { 4, 5, 6, 7 });
				for (int j = 0; j < array2.Length; j++)
				{
					float weight2 = (float)j / 3f;
					array[array2[j]] = ColorUtils.GetWeightedAverage(Color2, Color1, weight2);
				}
				ApplyDirect(array, init: true);
				ApplyDirect(array);
			}

			public static void ApplyZoneTest()
			{
				Color[] color = new Color[8]
				{
					Color.FromArgb(255, 0, 0),
					Color.FromArgb(255, 128, 0),
					Color.FromArgb(255, 255, 0),
					Color.FromArgb(0, 255, 0),
					Color.FromArgb(0, 255, 255),
					Color.FromArgb(0, 0, 255),
					Color.FromArgb(255, 0, 255),
					Color.FromArgb(255, 255, 255)
				};
				ApplyDirect(color, init: true);
				ApplyDirectLightbar(color);
			}

			public static void ApplyGPUColor(int gpuMode = -1)
			{
				if (AppConfig.Get("aura_mode") == 21)
				{
					if (gpuMode < 0)
					{
						gpuMode = 0; // Ultralight: GPU mode control removed
					}
					Color color = gpuMode switch
					{
						2 => colorUltimate, 
						0 => colorEco, 
						_ => colorStandard, 
					};
					if (isACPI)
					{
						Program.acpi.TUFKeyboardRGB(AuraMode.AuraStatic, color, 235, $"TUF RGB GPU {gpuMode}");
					}
					AsusHid.Write(new List<byte[]>
					{
						AuraMessage(AuraMode.AuraStatic, color, color, 235),
						MESSAGE_APPLY,
						MESSAGE_SET
					});
				}
			}

			public static void ApplyHeatmap(bool init = false)
			{
				float num = HardwareControl.GetCPUTemp().Value;
				Color color = colorFreeze;
				color = ((num < (float)tempCold) ? ColorUtils.GetWeightedAverage(colorFreeze, colorCold, (num - (float)tempFreeze) / (float)(tempCold - tempFreeze)) : ((num < (float)tempWarm) ? ColorUtils.GetWeightedAverage(colorCold, colorWarm, (num - (float)tempCold) / (float)(tempWarm - tempCold)) : ((!(num < (float)tempHot)) ? colorHot : ColorUtils.GetWeightedAverage(colorWarm, colorHot, (num - (float)tempWarm) / (float)(tempHot - tempWarm)))));
				ApplyDirect(color, init);
			}

			public static void ApplyBattery()
			{
				float num = (float)HardwareControl.GetBatteryChargePercentage();
				Color color = colorLow;
				if (num < battLow)
				{
					color = colorLow;
				}
				else if (num < battMid)
				{
					float weight = (num - battLow) / (battMid - battLow);
					color = ColorUtils.GetWeightedAverage(colorLow, colorMid, weight);
				}
				else if (num < battHigh)
				{
					float weight2 = (num - battMid) / (battHigh - battMid);
					color = ColorUtils.GetWeightedAverage(colorMid, colorHigh, weight2);
				}
				else
				{
					color = colorHigh;
				}
				AsusHid.Write(new List<byte[]>
				{
					AuraMessage(AuraMode.AuraStatic, color, color, 235),
					MESSAGE_APPLY,
					MESSAGE_SET
				});
				if (isACPI)
				{
					Program.acpi.TUFKeyboardRGB(AuraMode.AuraStatic, color, 235);
				}
			}

			public static void ApplyAmbient(bool init = false)
			{
				if (!backlight || sessionLock || AmbientData.IsMoveSize())
				{
					return;
				}
				Rectangle bounds = Screen.GetBounds(Point.Empty);
				bounds.Y += bounds.Height / 3;
				bounds.Height -= (int)Math.Round((float)bounds.Height * 0.352f);
				Bitmap bitmap = AmbientData.CamptureScreen(bounds, 512, 288);
				Bitmap bitmap2 = AmbientData.ResizeImage(bitmap, 4, 2);
				int num = AURA_ZONES;
				if (isStrix)
				{
					Color midColor = ColorUtils.GetMidColor(bitmap2.GetPixel(0, 1), bitmap2.GetPixel(1, 1));
					Color midColor2 = ColorUtils.GetMidColor(bitmap2.GetPixel(2, 1), bitmap2.GetPixel(3, 1));
					AmbientData.Colors[4].RGB = ColorUtils.HSV.UpSaturation(bitmap2.GetPixel(1, 1));
					AmbientData.Colors[5].RGB = ColorUtils.HSV.UpSaturation(midColor);
					AmbientData.Colors[6].RGB = ColorUtils.HSV.UpSaturation(midColor2);
					AmbientData.Colors[7].RGB = ColorUtils.HSV.UpSaturation(bitmap2.GetPixel(3, 1));
					for (int i = 0; i < 4; i++)
					{
						AmbientData.Colors[i].RGB = ColorUtils.HSV.UpSaturation(bitmap2.GetPixel(i, 0));
					}
				}
				else
				{
					num = 1;
					AmbientData.Colors[0].RGB = ColorUtils.HSV.UpSaturation(ColorUtils.GetDominantColor(bitmap2), 0.3f);
				}
				bitmap.Dispose();
				bitmap2.Dispose();
				bool flag = init;
				for (int j = 0; j < num; j++)
				{
					if (AmbientData.result[j].ToArgb() != AmbientData.Colors[j].RGB.ToArgb())
					{
						flag = true;
					}
					AmbientData.result[j] = AmbientData.Colors[j].RGB;
				}
				if (flag)
				{
					if (isStrix)
					{
						ApplyDirect(AmbientData.result, init);
					}
					else
					{
						ApplyDirect(AmbientData.result[0], init);
					}
				}
			}
		}

		private static byte[] MESSAGE_APPLY;

		private static byte[] MESSAGE_SET;

		private static readonly int AURA_ZONES;

		private static AuraMode mode;

		private static AuraSpeed speed;

		private static bool backlight;

		private static bool initDirect;

		public static bool sessionLock;

		public static Color Color1;

		public static Color Color2;

		public static Color RearColor;

		private static AuraMode rearMode;

		private static bool isACPI;

		private static bool isStrix4Zone;

		public static bool isWhite;

		private static System.Timers.Timer timer;

		private static readonly List<double> audioMaxes;

		private static long lastAudioPresent;

		private static double envBrightness;

		private static double smoothedHue;

		private static readonly double audioDecay;

		private static Dictionary<AuraMode, string> _modesRear;

		private static byte[] packetMap;

		private static byte[] packetZone;

		private static byte[] packetZoneNumpad;

		private static byte[] packet4Zone;

		private static byte[] packet4ZoneFlipped;

		public static AuraMode RearMode
		{
			get
			{
				return rearMode;
			}
			set
			{
				rearMode = (GetRearModes().ContainsKey(value) ? value : AuraMode.AuraStatic);
			}
		}

		private static bool isStrix
		{
			get
			{
				if (BacklightType != AuraBacklightType.MultiZone)
				{
					return BacklightType == AuraBacklightType.PerKey;
				}
				return true;
			}
		}

		public static bool IsBacklightDetected => BacklightType != AuraBacklightType.Unknown;

		public static AuraBacklightType BacklightType { get; private set; }

		public static bool HasLogo { get; private set; }

		public static bool HasLightbar { get; private set; }

		public static bool HasRearglow { get; private set; }

		public static bool IsOldStrix { get; private set; }

		public static AuraMode Mode
		{
			get
			{
				return mode;
			}
			set
			{
				mode = (GetModes().ContainsKey(value) ? value : AuraMode.AuraStatic);
			}
		}

		public static AuraSpeed Speed
		{
			get
			{
				return speed;
			}
			set
			{
				speed = ((!GetSpeeds().ContainsKey(value)) ? AuraSpeed.Normal : value);
			}
		}

		static Aura()
		{
			MESSAGE_APPLY = new byte[2] { 93, 180 };
			MESSAGE_SET = new byte[5] { 93, 181, 0, 0, 0 };
			AURA_ZONES = 8;
			mode = AuraMode.AuraStatic;
			speed = AuraSpeed.Normal;
			backlight = false;
			initDirect = false;
			sessionLock = false;
			Color1 = Color.White;
			Color2 = Color.Black;
			RearColor = Color.White;
			rearMode = AuraMode.AuraStatic;
			isACPI = AppConfig.IsVivoZenPro();
			isStrix4Zone = false;
			isWhite = false;
			BacklightType = AuraBacklightType.Unknown;
			timer = new System.Timers.Timer(1000.0);
			audioMaxes = new List<double>();
			audioDecay = (double)AppConfig.Get("audio_decay", 70) / 100.0;
			_modesRear = new Dictionary<AuraMode, string>
			{
				{
					AuraMode.AuraStatic,
					Strings.AuraStatic
				},
				{
					AuraMode.AuraBreathe,
					Strings.AuraBreathe
				},
				{
					AuraMode.AuraColorCycle,
					Strings.AuraColorCycle
				},
				{
					AuraMode.AuraRainbow,
					Strings.AuraRainbow
				},
				{
					AuraMode.AuraStrobe,
					Strings.AuraStrobe
				}
			};
			packetMap = new byte[134]
			{
				2, 3, 4, 5, 6, 21, 23, 24, 25, 26,
				28, 29, 30, 31, 33, 34, 35, 36, 37, 38,
				39, 40, 41, 42, 43, 44, 45, 46, 47, 48,
				49, 50, 51, 52, 53, 54, 55, 56, 57, 58,
				59, 60, 61, 62, 63, 64, 65, 66, 67, 68,
				69, 70, 71, 72, 73, 74, 75, 76, 79, 80,
				81, 82, 83, 84, 85, 86, 87, 88, 89, 90,
				91, 92, 93, 94, 95, 96, 97, 98, 99, 100,
				101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
				111, 112, 113, 114, 115, 116, 117, 118, 119, 139,
				121, 122, 123, 124, 125, 126, 127, 128, 129, 131,
				135, 136, 137, 159, 160, 161, 142, 144, 145, 146,
				174, 173, 172, 120, 140, 141, 143, 171, 170, 169,
				0, 167, 176, 177
			};
			packetZone = new byte[134]
			{
				0, 0, 1, 1, 1, 0, 0, 0, 1, 1,
				1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
				3, 3, 3, 0, 0, 0, 0, 1, 1, 1,
				1, 2, 2, 2, 2, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 0, 0, 0, 0, 1, 1,
				1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
				3, 3, 3, 0, 0, 0, 0, 1, 1, 1,
				1, 2, 2, 2, 2, 3, 3, 3, 3, 3,
				3, 3, 3, 3, 0, 0, 0, 0, 1, 1,
				1, 1, 2, 2, 2, 2, 3, 3, 3, 3,
				3, 3, 3, 3, 3, 0, 0, 0, 0, 1,
				2, 2, 2, 3, 3, 3, 3, 3, 3, 3,
				5, 5, 4, 3, 3, 3, 3, 6, 7, 7,
				3, 0, 0, 3
			};
			packetZoneNumpad = new byte[134]
			{
				0, 0, 0, 1, 1, 0, 0, 0, 0, 1,
				1, 1, 1, 1, 2, 2, 2, 2, 3, 3,
				3, 3, 3, 0, 0, 0, 0, 0, 1, 1,
				1, 1, 1, 2, 2, 2, 2, 2, 2, 3,
				3, 3, 3, 3, 0, 0, 0, 0, 0, 1,
				1, 1, 1, 1, 2, 2, 2, 2, 3, 3,
				3, 3, 3, 0, 0, 0, 0, 0, 1, 1,
				1, 1, 1, 2, 2, 2, 2, 2, 2, 3,
				3, 3, 3, 3, 0, 0, 0, 0, 0, 1,
				1, 1, 1, 1, 2, 2, 2, 2, 2, 2,
				3, 3, 3, 3, 3, 0, 0, 0, 0, 1,
				1, 2, 2, 2, 2, 2, 3, 3, 3, 3,
				5, 5, 4, 2, 2, 2, 3, 6, 7, 7,
				3, 0, 0, 3
			};
			packet4Zone = new byte[12]
			{
				0, 1, 2, 3, 0, 0, 7, 7, 6, 5,
				4, 4
			};
			packet4ZoneFlipped = new byte[12]
			{
				0, 1, 2, 3, 0, 0, 4, 4, 5, 6,
				7, 7
			};
			timer.Elapsed += Timer_Elapsed;
		}

		public static Dictionary<AuraSpeed, string> GetSpeeds()
		{
			return new Dictionary<AuraSpeed, string>
			{
				{
					AuraSpeed.Slow,
					Strings.AuraSlow
				},
				{
					AuraSpeed.Normal,
					Strings.AuraNormal
				},
				{
					AuraSpeed.Fast,
					Strings.AuraFast
				}
			};
		}

		public static Dictionary<AuraMode, string> GetModes()
		{
			Dictionary<AuraMode, string> dictionary = new Dictionary<AuraMode, string>();
			if (isWhite)
			{
				dictionary[AuraMode.AuraStatic] = Strings.AuraStatic;
				dictionary[AuraMode.AuraBreathe] = Strings.AuraBreathe;
				dictionary[AuraMode.AuraStrobe] = Strings.AuraStrobe;
				return dictionary;
			}
			if (AppConfig.IsDynamicLightingOnly())
			{
				dictionary[AuraMode.AuraStatic] = Strings.AuraStatic;
				dictionary[AuraMode.AuraBreathe] = Strings.AuraColorCycle;
				dictionary[AuraMode.AuraRainbow] = Strings.AuraRainbow;
				dictionary[AuraMode.AuraStrobe] = Strings.AuraStrobe;
				return dictionary;
			}
			bool num = BacklightType == AuraBacklightType.PerKey;
			bool flag = BacklightType == AuraBacklightType.MultiZone;
			bool flag2 = num || flag;
			dictionary[AuraMode.AuraStatic] = Strings.AuraStatic;
			dictionary[AuraMode.AuraBreathe] = Strings.AuraBreathe;
			dictionary[AuraMode.AuraColorCycle] = Strings.AuraColorCycle;
			if (flag2)
			{
				dictionary[AuraMode.AuraRainbow] = Strings.AuraRainbow;
			}
			if (num)
			{
				dictionary[AuraMode.Star] = "Star";
				dictionary[AuraMode.Rain] = "Rain";
				dictionary[AuraMode.Highlight] = "Highlight";
				dictionary[AuraMode.Laser] = "Laser";
				dictionary[AuraMode.Ripple] = "Ripple";
			}
			dictionary[AuraMode.AuraStrobe] = Strings.AuraStrobe;
			if (num)
			{
				dictionary[AuraMode.Comet] = "Comet";
				dictionary[AuraMode.Flash] = "Flash";
			}
			dictionary[AuraMode.HEATMAP] = "Heatmap";
			dictionary[AuraMode.GPUMODE] = "GPU Mode";
			dictionary[AuraMode.AMBIENT] = "Ambient";
			dictionary[AuraMode.BATTERY] = "Battery";
			dictionary[AuraMode.AUDIO] = "Audio Spectrum";
			dictionary[AuraMode.AUDIOPULSE] = "Audio Pulse";
			if (flag2)
			{
				dictionary[AuraMode.GRADIENT] = "Gradient";
				dictionary[AuraMode.ZONETEST] = "Zone Test";
			}
			return dictionary;
		}

		public static Dictionary<AuraMode, string> GetRearModes()
		{
			return _modesRear;
		}

		public static void SetColor(int colorCode)
		{
			Color1 = Color.FromArgb(colorCode);
		}

		public static void SetColor2(int colorCode)
		{
			Color2 = Color.FromArgb(colorCode);
		}

		public static void SetRearColor(int colorCode)
		{
			RearColor = Color.FromArgb(colorCode);
		}

		public static bool HasSecondColor()
		{
			if (mode == AuraMode.AuraBreathe || mode == AuraMode.GRADIENT)
			{
				if (isACPI)
				{
					return AppConfig.IsDynamicLightingOnly();
				}
				return true;
			}
			return false;
		}

		public static bool HasRandomColor()
		{
			if (mode != AuraMode.Star && mode != AuraMode.Highlight && mode != AuraMode.Laser)
			{
				return mode == AuraMode.Ripple;
			}
			return true;
		}

		private static void Timer_Elapsed(object? sender, ElapsedEventArgs e)
		{
			if (InputDispatcher.backlightActivity)
			{
				if (Mode == AuraMode.HEATMAP)
				{
					CustomRGB.ApplyHeatmap();
				}
				else if (Mode == AuraMode.BATTERY)
				{
					CustomRGB.ApplyBattery();
				}
				else if (Mode == AuraMode.AMBIENT)
				{
					CustomRGB.ApplyAmbient();
				}
			}
		}

		public static byte[] AuraMessage(AuraMode mode, Color color, Color color2, int speed)
		{
			return new byte[17]
			{
				93,
				179,
				0,
				(byte)mode,
				color.R,
				(byte)((!isWhite) ? color.G : 0),
				(byte)((!isWhite) ? color.B : 0),
				(byte)speed,
				0,
				(color.R == 0 && color.G == 0 && color.B == 0) ? ((byte)255) : ((mode == AuraMode.AuraBreathe) ? ((byte)1) : ((byte)0)),
				color2.R,
				(byte)((!isWhite) ? color2.G : 0),
				(byte)((!isWhite) ? color2.B : 0),
				0,
				0,
				0,
				0
			};
		}

		private static void DetectBacklightType()
		{
			if (isACPI)
			{
				return;
			}
			if (IsBacklightDetected)
			{
				AsusHid.AuraProbe(query: false);
				return;
			}
			byte[] array = AsusHid.AuraProbe(query: true);
			if (array == null || array.Length < 18)
			{
				return;
			}
			byte b = array[9];
			byte b2 = array[10];
			byte value = array[12];
			byte b3 = array[13];
			byte b4 = array[14];
			byte b5 = (byte)((b2 >= 35) ? array[17] : 0);
			string value2 = b5 switch
			{
				1 => "Strix", 
				2 => "Flow", 
				4 => "Zephyrus", 
				8 => "TUF", 
				16 => "NR2301", 
				32 => "Desktop", 
				0 => "(pre-2023)", 
				_ => $"unknown(0x{b5:X2})", 
			};
			Logger.WriteLine($"Aura Probe: Type=0x{b:X2} Year=0x{b2:X2} Layout=0x{value:X2} Feat1=0x{b3:X2} Feat2=0x{b4:X2} Family=0x{b5:X2} ({value2})");
			Logger.WriteLine($"Aura Probe Feat1 regions: Logo={(b3 & 1) != 0} Lightbar={(b3 & 2) != 0} Vcut={(b3 & 0x10) != 0} Aero={(b3 & 0x20) != 0} Bump={(b3 & 0x40) != 0} Rearglow={(b3 & 0x80) != 0}");
			Logger.WriteLine($"Aura Probe Feat2 features: DefaultColor={(b4 & 4) != 0} RGBWheel={(b4 & 8) != 0} OneZoneRedEffect={(b4 & 0x10) != 0} PerKeyMap={(b4 & 0x40) != 0}");
			BacklightType = b switch
			{
				2 => AuraBacklightType.MultiZone, 
				3 => AuraBacklightType.PerKey, 
				4 => AuraBacklightType.SingleZone, 
				0 => AuraBacklightType.SingleZone, 
				_ => AuraBacklightType.Unknown, 
			};
			if (IsBacklightDetected)
			{
				AppConfig.Set("backlight_type", b);
				IsOldStrix = false;
				HasLogo = (b3 & 1) != 0;
				HasLightbar = (b3 & 2) != 0;
				HasRearglow = (b3 & 0x90) != 0;
				isStrix4Zone = BacklightType == AuraBacklightType.MultiZone;
				if (b != 0 && (b4 & 0x10) != 0)
				{
					isWhite = true;
				}
			}
		}

		public static void Init()
		{
			DetectBacklightType();
			InputDispatcher.InitFNLock();
		}

		public static void SleepBrightness()
		{
			if (!AppConfig.Is("keyboard_sleep"))
			{
				ApplyBrightness(0, "Sleep");
			}
		}

		public static void ApplyBrightness(int brightness, string log = "Backlight")
		{
			if (brightness == 0)
			{
				backlight = false;
			}
			DirectBrightness(brightness, log);
			if (brightness > 0)
			{
				if (!backlight)
				{
					initDirect = true;
				}
				backlight = true;
			}
		}

		public static void DirectBrightness(int brightness, string log)
		{
			if (isACPI)
			{
				Program.acpi.TUFKeyboardBrightness(brightness, log);
			}
			byte[] obj = new byte[5] { 90, 186, 197, 196, 0 };
			obj[4] = (byte)brightness;
			AsusHid.WriteInput(obj, log);
		}

		private static byte[] AuraPowerMessage(AuraPower flags)
		{
			byte b = 0;
			byte b2 = 0;
			byte b3 = 0;
			byte b4 = 0;
			if (flags.BootLogo)
			{
				b |= 1;
			}
			if (flags.BootKeyb)
			{
				b |= 2;
			}
			if (flags.AwakeLogo)
			{
				b |= 4;
			}
			if (flags.AwakeKeyb)
			{
				b |= 8;
			}
			if (flags.SleepLogo)
			{
				b |= 0x10;
			}
			if (flags.SleepKeyb)
			{
				b |= 0x20;
			}
			if (flags.ShutdownLogo)
			{
				b |= 0x40;
			}
			if (flags.ShutdownKeyb)
			{
				b |= 0x80;
			}
			if (flags.AwakeBar)
			{
				b2 |= 1;
			}
			if (flags.BootBar)
			{
				b2 |= 2;
			}
			if (flags.AwakeBar)
			{
				b2 |= 4;
			}
			if (flags.SleepBar)
			{
				b2 |= 8;
			}
			if (flags.ShutdownBar)
			{
				b2 |= 0x10;
			}
			if (flags.BootLid)
			{
				b3 |= 1;
			}
			if (flags.AwakeLid)
			{
				b3 |= 2;
			}
			if (flags.SleepLid)
			{
				b3 |= 4;
			}
			if (flags.ShutdownLid)
			{
				b3 |= 8;
			}
			if (flags.BootLid)
			{
				b3 |= 0x10;
			}
			if (flags.AwakeLid)
			{
				b3 |= 0x20;
			}
			if (flags.SleepLid)
			{
				b3 |= 0x40;
			}
			if (flags.ShutdownLid)
			{
				b3 |= 0x80;
			}
			if (flags.BootRear)
			{
				b4 |= 1;
			}
			if (flags.AwakeRear)
			{
				b4 |= 2;
			}
			if (flags.SleepRear)
			{
				b4 |= 4;
			}
			if (flags.ShutdownRear)
			{
				b4 |= 8;
			}
			if (flags.BootRear)
			{
				b4 |= 0x10;
			}
			if (flags.AwakeRear)
			{
				b4 |= 0x20;
			}
			if (flags.SleepRear)
			{
				b4 |= 0x40;
			}
			if (flags.ShutdownRear)
			{
				b4 |= 0x80;
			}
			byte[] obj = new byte[8] { 93, 189, 1, 0, 0, 0, 0, 255 };
			obj[3] = b;
			obj[4] = b2;
			obj[5] = b3;
			obj[6] = b4;
			return obj;
		}

		public static void ApplyPowerOff()
		{
			AsusHid.Write(AuraPowerMessage(new AuraPower()));
		}

		public static void ApplyPower()
		{
			bool flag = false;
			AuraPower auraPower = new AuraPower
			{
				AwakeKeyb = (flag ? AppConfig.IsOnBattery("keyboard_awake") : AppConfig.IsNotFalse("keyboard_awake")),
				BootKeyb = AppConfig.IsNotFalse("keyboard_boot"),
				SleepKeyb = AppConfig.IsNotFalse("keyboard_sleep"),
				ShutdownKeyb = AppConfig.IsNotFalse("keyboard_shutdown"),
				AwakeLogo = (flag ? AppConfig.IsOnBattery("keyboard_awake_logo") : AppConfig.IsNotFalse("keyboard_awake_logo")),
				BootLogo = AppConfig.IsNotFalse("keyboard_boot_logo"),
				SleepLogo = AppConfig.IsNotFalse("keyboard_sleep_logo"),
				ShutdownLogo = AppConfig.IsNotFalse("keyboard_shutdown_logo"),
				AwakeBar = (flag ? AppConfig.IsOnBattery("keyboard_awake_bar") : AppConfig.IsNotFalse("keyboard_awake_bar")),
				BootBar = AppConfig.IsNotFalse("keyboard_boot_bar"),
				SleepBar = AppConfig.IsNotFalse("keyboard_sleep_bar"),
				ShutdownBar = AppConfig.IsNotFalse("keyboard_shutdown_bar"),
				AwakeLid = (flag ? AppConfig.IsOnBattery("keyboard_awake_lid") : AppConfig.IsNotFalse("keyboard_awake_lid")),
				BootLid = AppConfig.IsNotFalse("keyboard_boot_lid"),
				SleepLid = AppConfig.IsNotFalse("keyboard_sleep_lid"),
				ShutdownLid = AppConfig.IsNotFalse("keyboard_shutdown_lid"),
				AwakeRear = (flag ? AppConfig.IsOnBattery("keyboard_awake_lid") : AppConfig.IsNotFalse("keyboard_awake_lid")),
				BootRear = AppConfig.IsNotFalse("keyboard_boot_lid"),
				SleepRear = AppConfig.IsNotFalse("keyboard_sleep_lid"),
				ShutdownRear = AppConfig.IsNotFalse("keyboard_shutdown_lid")
			};
			AsusHid.Write(AuraPowerMessage(auraPower));
			if (isACPI)
			{
				Program.acpi.TUFKeyboardPower(auraPower.AwakeKeyb, auraPower.BootKeyb, auraPower.SleepKeyb, auraPower.ShutdownKeyb);
			}
		}

		public static void ApplyDirect(Color[] color, bool init = false)
		{
			if (!backlight)
			{
				return;
			}
			if (AsusLampArray.Available)
			{
				AsusLampArray.SetColors(color);
				return;
			}
			byte[] array = new byte[64];
			byte[] array2 = new byte[534];
			array[0] = 93;
			array[1] = 188;
			array[2] = 0;
			array[3] = 1;
			array[4] = 1;
			array[5] = 1;
			array[6] = 0;
			array[7] = 16;
			if (init || initDirect)
			{
				initDirect = false;
				AsusHid.SetFeatureAura(new byte[3]
				{
					93,
					188,
					(!IsOldStrix) ? ((byte)1) : ((byte)0)
				});
				Thread.Sleep(50);
			}
			Array.Clear(array2, 0, array2.Length);
			if (!isStrix4Zone)
			{
				for (int i = 0; i < packetMap.Count(); i++)
				{
					ushort num = (ushort)(3 * packetMap[i]);
					byte b = packetZone[i];
					array2[num] = color[b].R;
					array2[num + 1] = color[b].G;
					array2[num + 2] = color[b].B;
				}
				for (int j = 0; j < 167; j += 16)
				{
					byte b2 = (byte)(167 - j);
					if (b2 < 16)
					{
						array[7] = b2;
					}
					array[6] = (byte)j;
					Buffer.BlockCopy(array2, 3 * j, array, 9, 3 * array[7]);
					AsusHid.SetFeatureAura(array);
					Thread.Sleep(1);
				}
			}
			array[4] = 4;
			array[5] = 0;
			array[6] = 0;
			array[7] = 0;
			if (isStrix4Zone)
			{
				byte[] array3 = packet4Zone;
				int num2 = array3.Count();
				for (int k = 0; k < num2; k++)
				{
					byte b3 = array3[k];
					array2[k * 3] = color[b3].R;
					array2[k * 3 + 1] = color[b3].G;
					array2[k * 3 + 2] = color[b3].B;
				}
				Buffer.BlockCopy(array2, 0, array, 9, 3 * num2);
				AsusHid.SetFeatureAura(array);
				Thread.Sleep(1);
			}
			else
			{
				Buffer.BlockCopy(array2, 501, array, 9, 33);
				AsusHid.SetFeatureAura(array);
			}
		}

		public static void ApplyDirectLightbar(Color[] color)
		{
			if (!AsusLampArray.Available)
			{
				byte[] array = packet4Zone;
				byte[] array2 = new byte[64];
				array2[0] = 93;
				array2[1] = 188;
				array2[2] = 0;
				array2[3] = 1;
				array2[4] = 4;
				for (int i = 0; i < array.Length; i++)
				{
					byte b = array[i];
					int num = 9 + i * 3;
					array2[num] = color[b].R;
					array2[num + 1] = color[b].G;
					array2[num + 2] = color[b].B;
				}
				AsusHid.SetFeatureAura(array2);
			}
		}

		public static void ApplyDirect(Color color, bool init = false)
		{
			if (!backlight)
			{
				return;
			}
			if (isACPI)
			{
				Program.acpi.TUFKeyboardRGB(AuraMode.AuraStatic, color, 0, null);
				return;
			}
			if (AsusLampArray.Available)
			{
				AsusLampArray.SetColor(color);
				return;
			}
			if (AppConfig.IsNoDirectRGB())
			{
				AsusHid.SetFeatureAura(AuraMessage(AuraMode.AuraStatic, color, color, 235));
				AsusHid.SetFeatureAura(MESSAGE_SET);
				return;
			}
			if (isStrix)
			{
				ApplyDirect(Enumerable.Repeat(color, AURA_ZONES).ToArray(), init);
				return;
			}
			if (init || initDirect)
			{
				initDirect = false;
				AsusHid.SetFeatureAura(new byte[3] { 93, 188, 1 });
				Thread.Sleep(50);
			}
			AsusHid.SetFeatureAura(new byte[12]
			{
				93, 188, 1, 1, 0, 0, 0, 0, 0, color.R,
				color.G, color.B
			});
		}

		public static Color ColorDim(Color Color, double colorDim = 1.0)
		{
			switch (InputDispatcher.GetBacklight())
			{
			case 1:
				colorDim = 0.1;
				break;
			case 2:
				colorDim = 0.3;
				break;
			}
			return Color.FromArgb((int)((double)(int)Color.R * colorDim), (int)((double)(int)Color.G * colorDim), (int)((double)(int)Color.B * colorDim));
		}

		public static void ApplyAura()
		{
			Mode = (AuraMode)AppConfig.Get("aura_mode");
			Speed = (AuraSpeed)AppConfig.Get("aura_speed");
			SetColor(AppConfig.Get("aura_color"));
			SetColor2(AppConfig.Get("aura_color2"));
			Color color = Color1;
			Color color2 = Color2;
			timer.Stop();
			if (Mode != AuraMode.AUDIO && Mode != AuraMode.AUDIOPULSE)
			{
				StopAudio();
			}
			Logger.WriteLine($"AuraMode: {Mode}");
			AsusLampArray.SetMode(Mode);
			if (AsusLampArray.Probing)
			{
				return;
			}
			if (Mode == AuraMode.AUDIO || Mode == AuraMode.AUDIOPULSE)
			{
				StartAudio();
			}
			else if (Mode == AuraMode.HEATMAP)
			{
				CustomRGB.ApplyHeatmap(init: true);
				timer.Interval = 2000.0;
				timer.Start();
			}
			else if (Mode == AuraMode.BATTERY)
			{
				CustomRGB.ApplyBattery();
				timer.Interval = 30000.0;
				timer.Start();
			}
			else if (Mode == AuraMode.AMBIENT)
			{
				CustomRGB.ApplyAmbient(init: true);
				timer.Interval = AppConfig.Get("aura_refresh", 300);
				timer.Start();
			}
			else if (Mode == AuraMode.GRADIENT)
			{
				CustomRGB.ApplyGradient();
			}
			else if (Mode == AuraMode.ZONETEST)
			{
				CustomRGB.ApplyZoneTest();
			}
			else if (Mode == AuraMode.GPUMODE)
			{
				CustomRGB.ApplyGPUColor();
			}
			else if (AppConfig.IsDynamicLightingOnly())
			{
				switch (mode)
				{
				case AuraMode.AuraBreathe:
				{
					Color? color4 = color;
					Color? color5 = color2;
					int? num = (int)Speed * 5;
					DynamicLightingHelper.SetEffect(DynamicLightingHelper.DynamicLightingEffect.Wave, color4, color5, null, num);
					break;
				}
				case AuraMode.AuraColorCycle:
				case AuraMode.AuraRainbow:
				{
					int? num = (int)Speed * 5;
					DynamicLightingHelper.SetEffect(DynamicLightingHelper.DynamicLightingEffect.Rainbow, null, null, null, num);
					break;
				}
				case AuraMode.AuraStrobe:
				{
					Color? color3 = color;
					int? num = 10;
					DynamicLightingHelper.SetEffect(DynamicLightingHelper.DynamicLightingEffect.Breathing, color3, null, null, num);
					break;
				}
				default:
					DynamicLightingHelper.SetEffect(DynamicLightingHelper.DynamicLightingEffect.Solid, color, null, null, null);
					break;
				}
			}
			else
			{
				int num2 = Speed switch
				{
					AuraSpeed.Fast => 245, 
					AuraSpeed.Normal => 235, 
					_ => 225, 
				};
				AsusHid.Write(new List<byte[]>
				{
					AuraMessage(Mode, color, color2, num2),
					MESSAGE_SET,
					MESSAGE_APPLY
				}, "Aura", AsusHid.MAIN_AURA_PIDS);
				XGM.LightMode(Mode, color, color2, num2);
				if (isACPI)
				{
					Program.acpi.TUFKeyboardRGB(Mode, Color1, num2);
				}
			}
		}

		public static void StopAudio()
		{
			// Ultralight: audio visualizer removed
		}

		public static void StartAudio()
		{
			if (backlight)
			{
				initDirect = true;
				audioMaxes.Clear();
				lastAudioPresent = 0L;
				envBrightness = 0.0;
				smoothedHue = 0.0;
				// Ultralight: audio visualizer removed
			}
		}

		private static void OnAudioSpectrum(double[] fftMag)
		{
			if (!backlight || sessionLock || (Mode != AuraMode.AUDIO && Mode != AuraMode.AUDIOPULSE))
			{
				return;
			}
			long num = DateTimeOffset.Now.ToUnixTimeMilliseconds();
			if (Math.Abs(num - lastAudioPresent) < 50)
			{
				return;
			}
			lastAudioPresent = num;
			int aURA_ZONES = AURA_ZONES;
			if (fftMag.Length < aURA_ZONES)
			{
				return;
			}
			double[] array = new double[aURA_ZONES];
			double num2 = 0.0;
			for (int i = 0; i < aURA_ZONES; i++)
			{
				array[i] = Math.Sqrt(fftMag[i] * 10000.0);
				if (array[i] > num2)
				{
					num2 = array[i];
				}
			}
			audioMaxes.Add(num2);
			if (audioMaxes.Count > 100)
			{
				audioMaxes.RemoveAt(0);
			}
			double num3 = audioMaxes.OrderByDescending((double x) => x).ElementAt(audioMaxes.Count / 10);
			if (num3 < 1.0)
			{
				num3 = 1.0;
			}
			envBrightness = Math.Max(envBrightness * audioDecay, num2);
			double num4 = Math.Min(1.0, envBrightness / num3);
			Color color = Color1;
			double num5 = num4 * num4 * num4;
			try
			{
				if (Mode == AuraMode.AUDIOPULSE)
				{
					Color color2 = Color.FromArgb((byte)((double)(int)color.R * num5), (byte)((double)(int)color.G * num5), (byte)((double)(int)color.B * num5));
					if (isStrix)
					{
						ApplyDirect(Enumerable.Repeat(color2, AURA_ZONES).ToArray());
					}
					else
					{
						ApplyDirect(color2);
					}
					return;
				}
				double hue = ColorUtils.HSV.ToHSV(color).Hue;
				if (isStrix)
				{
					Color[] array2 = new Color[AURA_ZONES];
					for (int j = 0; j < AURA_ZONES; j++)
					{
						double hue2 = (hue + (double)j / (double)(AURA_ZONES - 1) * (2.0 / 3.0)) % 1.0;
						double num6 = Math.Min(1.0, array[j] / num3);
						double value = num6 * num6 * num6;
						array2[j] = new ColorUtils.HSV
						{
							Hue = hue2,
							Saturation = 1.0,
							Value = value
						}.ToRGB();
					}
					ApplyDirect(array2);
					return;
				}
				int num7 = 1;
				double num8 = array[1];
				for (int k = 2; k < aURA_ZONES; k++)
				{
					double num9 = array[k] * (1.0 + (double)(k - 1) * 0.15);
					if (num9 > num8)
					{
						num8 = num9;
						num7 = k;
					}
				}
				if (num2 > num3 * 0.3)
				{
					double num10 = (hue + (double)(num7 - 1) / (double)(aURA_ZONES - 2) * (2.0 / 3.0)) % 1.0;
					smoothedHue = smoothedHue * 0.6 + num10 * 0.4;
				}
				ApplyDirect(new ColorUtils.HSV
				{
					Hue = smoothedHue,
					Saturation = 1.0,
					Value = num5
				}.ToRGB());
			}
			catch (Exception ex)
			{
				Logger.WriteLine("Aura audio: " + ex.Message);
			}
		}
	}
