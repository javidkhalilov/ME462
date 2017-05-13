using System;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace USBLibrary
{
	/// <summary>
	///  These declarations are translated from the C declarations in various files
	///  in the Windows DDK. The files are:
	///  
	///  winddk\6001\inc\api\usb.h
	///  winddk\6001\inc\api\usb100.h
	///  winddk\6001\inc\api\winusbio.h
	///  
	///  (your home directory and release number may vary)
	/// <summary>

	sealed public partial class WinUsbDevice
	{
		public const UInt32 DEVICE_SPEED = ((UInt32)(1));
		public const Byte USB_ENDPOINT_DIRECTION_MASK = ((Byte)(0X80));

		public enum POLICY_TYPE
		{
			SHORT_PACKET_TERMINATE = 1,
			AUTO_CLEAR_STALL,
			PIPE_TRANSFER_TIMEOUT,
			IGNORE_SHORT_PACKETS,
			ALLOW_PARTIAL_READS,
			AUTO_FLUSH,
			RAW_IO,
		}

		public enum USBD_PIPE_TYPE
		{
			UsbdPipeTypeControl,
			UsbdPipeTypeIsochronous,
			UsbdPipeTypeBulk,
			UsbdPipeTypeInterrupt,
		}

		public enum USB_DEVICE_SPEED
		{
			UsbLowSpeed = 1,
			UsbFullSpeed,
			UsbHighSpeed,
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct USB_CONFIGURATION_DESCRIPTOR
		{
			public Byte bLength;
			public Byte bDescriptorType;
			public ushort wTotalLength;
			public Byte bNumInterfaces;
			public Byte bConfigurationValue;
			public Byte iConfiguration;
			public Byte bmAttributes;
			public Byte MaxPower;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct USB_INTERFACE_DESCRIPTOR
		{
			public Byte bLength;
			public Byte bDescriptorType;
			public Byte bInterfaceNumber;
			public Byte bAlternateSetting;
			public Byte bNumEndpoints;
			public Byte bInterfaceClass;
			public Byte bInterfaceSubClass;
			public Byte bInterfaceProtocol;
			public Byte iInterface;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct WINUSB_PIPE_INFORMATION
		{
			public USBD_PIPE_TYPE PipeType;
			public Byte PipeId;
			public ushort MaximumPacketSize;
			public Byte Interval;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct WINUSB_SETUP_PACKET
		{
			public Byte RequestType;
			public Byte Request;
			public ushort Value;
			public ushort Index;
			public ushort Length;
		}

		[DllImport("winusb.dll", SetLastError = true)]
		public static extern Boolean WinUsb_ControlTransfer(IntPtr InterfaceHandle, WINUSB_SETUP_PACKET SetupPacket, Byte[] Buffer, UInt32 BufferLength, ref UInt32 LengthTransferred, IntPtr Overlapped);

		[DllImport("winusb.dll", SetLastError = true)]
		public static extern Boolean WinUsb_Free(IntPtr InterfaceHandle);

		[DllImport("winusb.dll", SetLastError = true)]
		public static extern Boolean WinUsb_Initialize(SafeFileHandle DeviceHandle, ref IntPtr InterfaceHandle);

		//  Use this declaration to retrieve DEVICE_SPEED (the only currently defined InformationType).

        [DllImport("winusb.dll" , SetLastError = true)]
        public static extern Boolean WinUsb_FlushPipe(IntPtr InterfaceHandle , Byte PipeID);

		[DllImport("winusb.dll", SetLastError = true)]
		public static extern Boolean WinUsb_QueryDeviceInformation(IntPtr InterfaceHandle, UInt32 InformationType, ref UInt32 BufferLength, ref Byte Buffer);

		[DllImport("winusb.dll", SetLastError = true)]
		public static extern Boolean WinUsb_QueryInterfaceSettings(IntPtr InterfaceHandle, Byte AlternateInterfaceNumber, ref USB_INTERFACE_DESCRIPTOR UsbAltInterfaceDescriptor);

		[DllImport("winusb.dll", SetLastError = true)]
		public static extern Boolean WinUsb_QueryPipe(IntPtr InterfaceHandle, Byte AlternateInterfaceNumber, Byte PipeIndex, ref WINUSB_PIPE_INFORMATION PipeInformation);

		[DllImport("winusb.dll", SetLastError = true)]
		public static extern Boolean WinUsb_ReadPipe(IntPtr InterfaceHandle, Byte PipeID, Byte[] Buffer, UInt32 BufferLength, ref UInt32 LengthTransferred, IntPtr Overlapped);

		//  Two declarations for WinUsb_SetPipePolicy. 
		//  Use this one when the returned Value is a Byte (all except PIPE_TRANSFER_TIMEOUT):

		[DllImport("winusb.dll", SetLastError = true)]
		public static extern Boolean WinUsb_SetPipePolicy(IntPtr InterfaceHandle, Byte PipeID, UInt32 PolicyType, UInt32 ValueLength, ref Byte Value);

		//  Use this alias when the returned Value is a UInt32 (PIPE_TRANSFER_TIMEOUT only):

		[DllImport("winusb.dll", SetLastError = true, EntryPoint = "WinUsb_SetPipePolicy")]
		public static extern Boolean WinUsb_SetPipePolicy1(IntPtr InterfaceHandle, Byte PipeID, UInt32 PolicyType, UInt32 ValueLength, ref UInt32 Value);

		[DllImport("winusb.dll", SetLastError = true)]
		public static extern Boolean WinUsb_WritePipe(IntPtr InterfaceHandle, Byte PipeID, Byte[] Buffer, UInt32 BufferLength, ref UInt32 LengthTransferred, IntPtr Overlapped);
	}
}
