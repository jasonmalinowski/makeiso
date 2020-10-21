using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace MakeISO
{
    public static class Program
    {
        [DllImport("kernel32.dll", EntryPoint = "CreateFileW", CharSet = CharSet.Unicode, SetLastError = true )]
        private static extern SafeFileHandle CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string lpFileName,
            uint dwDesiredAccess,
            uint dwShareMode,
            IntPtr lpSecurityAttributes,
            uint dwCreationDisposition,
            uint dwFlagsAndAttributes,
            uint hTemplateFile);

        private const uint GENERIC_READ = 0x80000000;
        private const uint FILE_SHARE_READ = 0x00000001;
        private const uint OPEN_EXISTING = 0x00000003;
        private const uint FILE_FLAG_SEQUENTIAL_SCAN = 0x08000000;

        public static int Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine("usage: makeiso [drive] [file]");
                return 1;
            }

            var drive = new DriveInfo(args[0]);

            if (drive.DriveType != DriveType.CDRom)
            {
                Console.Error.WriteLine("Invalid drive letter.");
                return 2;
            }

            try
            {
                using (var inputFileHandle = CreateFile(@"\\.\" + drive.ToString().TrimEnd('\\'), GENERIC_READ, FILE_SHARE_READ, IntPtr.Zero, OPEN_EXISTING, FILE_FLAG_SEQUENTIAL_SCAN, 0))
                {
                    if (inputFileHandle.IsInvalid)
                    {
                        Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                    }

                    using (var inputStream = new FileStream(inputFileHandle, FileAccess.Read))
                    {
                        var outputFile = new FileInfo(args[1]);

                        if (!outputFile.Directory.Exists)
                        {
                            outputFile.Directory.Create();
                        }

                        using (var outputStream = outputFile.Open(FileMode.CreateNew))
                        {
                            inputStream.CopyTo(outputStream);
                        }
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.Message);
                return 1;
            }
        }
    }
}
