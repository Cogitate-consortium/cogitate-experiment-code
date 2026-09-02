using System;
using System.Runtime.InteropServices;

namespace Peripherals.LPT.Core
{
    /*
     * Useful Link
     * http://sandeep-aparajit.blogspot.com/2008/08/io-how-to-program-readwrite-parallel.html
     */

    /* 
     * Got code from
     * https://stackoverflow.com/questions/4607935/c-sharp-lpt-inpout32-dll
     */
    /// <summary>
    /// [RENAME] LPTWrapper
    /// </summary>
    public class LPTAccess
    {
        #region Import DLL

        [DllImport("inpoutx64", EntryPoint = "IsInpOutDriverOpen")]
        private static extern UInt32 IsInpOutDriverOpen_x64();

        [DllImport("inpoutx64", EntryPoint = "Out32")]
        private static extern void Out32_x64(short PortAddress, short Data);

        [DllImport("inpoutx64", EntryPoint = "Inp32")]
        private static extern char Inp32_x64(short PortAddress);

        [DllImport("inpoutx64", EntryPoint = "DlPortWritePortUshort")]
        private static extern void DlPortWritePortUshort_x64(short PortAddress, ushort Data);

        [DllImport("inpoutx64", EntryPoint = "DlPortReadPortUshort")]
        private static extern ushort DlPortReadPortUshort_x64(short PortAddress);

        [DllImport("inpoutx64", EntryPoint = "DlPortWritePortUlong")]
        private static extern void DlPortWritePortUlong_x64(int PortAddress, uint Data);

        [DllImport("inpoutx64", EntryPoint = "DlPortReadPortUlong")]
        private static extern uint DlPortReadPortUlong_x64(int PortAddress);

        [DllImport("inpoutx64", EntryPoint = "GetPhysLong")]
        private static extern bool GetPhysLong_x64(ref int PortAddress, ref uint Data);

        [DllImport("inpoutx64", EntryPoint = "SetPhysLong")]
        private static extern bool SetPhysLong_x64(ref int PortAddress, ref uint Data);

        #endregion

        public short portAddress { get; private set; }

        public LPTAccess(short PortAddress)
        {
            portAddress = PortAddress;

            uint nResult = 0;
            try
            {
                nResult = IsInpOutDriverOpen_x64();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }

            if (nResult == 0)
            {
                throw new Exception("Unable to open InpOut32 driver");
            }
        }

        public void Write(short Data)
        {
            try
            {
                Out32_x64(portAddress, Data);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

        public byte Read()
        {
            try
            {
                return (byte)Inp32_x64(portAddress);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex);
            }
        }

    }
}