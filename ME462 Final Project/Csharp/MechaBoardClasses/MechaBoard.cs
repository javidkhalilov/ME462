using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using USBLibrary;

namespace MechaBoardClasses
{
    /// <summary>
    /// Class for the mechaboard, all usb related variables are also kept here
    /// </summary>
    public class MechaBoard
    {
        #region Constructors

        /// <summary>
        /// The guid and the form that will utilize the mechaboard will be passed as 
        /// arguments to the constructor
        /// In the context of our final project, the form reference that will 
        /// be passed to the constructor is the 
        /// reference for the mainform only. Other forms will subscribe and 
        /// unsubscribe to/from device notifications via
        /// the methods of the MechaBoard class. Note that form reference is passed solely for subscribing 
        /// </summary>
        /// <param name="guid">the unique identifier string for the mechaboard that is to be used</param>
        /// <param name="form_reference">the form that is going to utilize the functionalities 
        /// of the mechaboar</param>
        public MechaBoard(String guid , Form form_reference)
        {
            //notification handle for the main board
            deviceNotificationHandle = IntPtr.Zero;
            //device for usb communication
            device = new WinUsbDevice();
            //manager for the device
            deviceManager = new DeviceManagement();
            //path name of the device used for identification
            devicePathName = "";
            //whether device detected or not
            isDeviceDetected = false;
            //guid of the device
            deviceGUID = guid;
            //form reference used to register the notifications about the board, should be the
            //reference of the mainform
            formReference = form_reference;
            //lock handle to safely provide accesses to the board
            boardLockHandle = new object();

        }

        #endregion

        #region Members, Properties, etc.

        //lock handle is used for critical region code when executing threads
        //is used only when multiple threads are accessing the mechaboard in an asynchronous
        //manner (e.g. data logger threads)
        Object boardLockHandle;

        #region USB Communication Related Members

        private IntPtr deviceNotificationHandle;
        private WinUsbDevice device;
        private DeviceManagement deviceManager;
        private String deviceGUID;
        private String devicePathName;
        private Boolean isDeviceDetected;
        private Form formReference;

        #endregion

        public Boolean IsDeviceDetected
        {
            get { return isDeviceDetected; }
            set { isDeviceDetected = value; }
        }

        public String DeviceGUID
        {
            get { return deviceGUID; }
            set { deviceGUID = value; }
        }

        public String DevicePathName
        {
            get { return devicePathName; }
        }
        
        #endregion

        #region Methods

        /// <summary>
        /// Taken from Jan Axelson's USB Bulk Transfer implementation.
        /// Finds the device if it exists, retrieves the handle to it. Subscribes the main 
        /// form to receive notifications from 
        /// the device. This function should only be called from the main code until the deivice is initially 
        /// </summary>
        /// <returns></returns>
        public bool FindMyDevice()
        {
            Boolean deviceFound = false;
            String devPathName = "";
            Boolean success = false;

           isDeviceDetected = false;
            try
            {
                if ( !( isDeviceDetected ) )
                {
                    //  Convert the device interface GUID String to a GUID object: 
                    System.Guid winusb_device_guid =
                        new System.Guid(deviceGUID);

                    // Fill an array with the device path names of all attached devices with matching GUIDs.
                    deviceFound = deviceManager.FindDeviceFromGuid
                        (winusb_device_guid ,
                        ref devPathName);

                    if ( deviceFound == true )
                    {
                        success = device.GetDeviceHandle(devPathName);

                        if ( success )
                        {
                            isDeviceDetected = true;

                            // Save DevicePathName so OnDeviceChange() knows which name is my device.
                            devicePathName = devPathName;
                        }
                        else
                        {
                            // There was a problem in retrieving the information.
                            isDeviceDetected = false;
                            device.CloseDeviceHandle();
                        }
                    }

                    if ( isDeviceDetected )
                    {
                        // The device was detected.
                        // Register to receive notifications if the device is removed or attached.
                        if ( deviceNotificationHandle == IntPtr.Zero )
                            success = deviceManager.RegisterForDeviceNotifications
                                (devicePathName ,
                                formReference.Handle ,
                                winusb_device_guid ,
                                ref deviceNotificationHandle);

                        if ( success )
                        {
                            device.InitializeDevice();
                        }
                    }
                }

                return isDeviceDetected;
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        /// <summary>
        /// Reads data using bulk transfer mode
        /// </summary>
        /// <param name="buffer">buffer that the data will be read in</param>
        /// <param name="bytesToRead">number of bytes to read</param>
        public void ReadDataViaBulkTransfer(ref Byte[] buffer , UInt32 bytesToRead)
        {
            Boolean success = false;
            UInt32 bytesRead = 0;
            byte[] tempData = new byte[1];
            try
            {
                if ( isDeviceDetected )
                {
                    device.ReadViaBulkTransfer(device.myDevInfo.bulkInPipe , bytesToRead , ref buffer , ref bytesRead , ref success);
                }
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }

        /// <summary>
        /// Send data using bulk transfer
        /// </summary>
        /// <param name="data">buffer from which the data will be read</param>
        /// <param name="bytesToSend">number of bytes to send over bulk transfer</param>
        public void SendDataViaBulkTransfer(Byte[] data , UInt32 bytesToSend)
        {
            try
            {
                Boolean success = false;

                if ( isDeviceDetected )
                {
                    success = device.SendViaBulkTransfer
                        (ref data ,
                        bytesToSend);
                }
            }
            catch ( Exception ex )
            {
                throw ex;
            }
        }


        /// <summary>
        /// Mainform should call this function while closing in order to release usb 
        /// resources and stop the system to send needless notifications
        /// to an already closed form
        /// </summary>
        public void ShutDownDevice()
        {
            device.CloseDeviceHandle();
            deviceManager.StopReceivingDeviceNotifications(deviceNotificationHandle);
        }

        /// <summary>
        /// Checks whether a device name matches, with the name from a notification
        /// </summary>
        /// <param name="m">message from the notification</param>
        /// <returns>true if name matches the name in the message</returns>
        public bool DeviceNameMatchTest(Message m)
        {
            return this.deviceManager.DeviceNameMatch(m , this.devicePathName);
        }

        #endregion
    }
}
