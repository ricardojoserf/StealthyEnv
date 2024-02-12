using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace StealthyEnv
{
    internal class Program
    {
        [DllImport("ntdll.dll", SetLastError = true)] static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref PROCESS_BASIC_INFORMATION pbi, uint processInformationLength, ref uint returnLength);
        [DllImport("ntdll.dll", SetLastError = true)] static extern bool NtReadVirtualMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, int dwSize, out IntPtr lpNumberOfBytesRead);


        private struct PROCESS_BASIC_INFORMATION { public uint ExitStatus; public IntPtr PebBaseAddress; public UIntPtr AffinityMask; public int BasePriority; public UIntPtr UniqueProcessId; public UIntPtr InheritedFromUniqueProcessId; }
        // Reference: _PEB and _RTL_USER_PROCESS_PARAMETERS structures
        // typedef struct _PEB { BYTE Reserved1[2]; BYTE BeingDebugged; BYTE Reserved2[1]; PVOID Reserved3[2]; PPEB_LDR_DATA Ldr; PRTL_USER_PROCESS_PARAMETERS  ProcessParameters; BYTE Reserved4[104]; PVOID Reserved5[52]; PPS_POST_PROCESS_INIT_ROUTINE PostProcessInitRoutine; BYTE Reserved6[128]; PVOID Reserved7[1]; ULONG SessionId; } PEB, *PPEB;
        // typedef struct _RTL_USER_PROCESS_PARAMETERS { ULONG MaximumLength; ULONG Length; ULONG Flags; ULONG DebugFlags; PVOID ConsoleHandle; ULONG ConsoleFlags; PVOID StandardInput; PVOID StandardOutput; PVOID StandardError; CURDIR CurrentDirectory; UNICODE_STRING DllPath; UNICODE_STRING ImagePathName; <--- UNICODE_STRING CommandLine; <--- PVOID Environment; ULONG StartingX; ULONG StartingY; ULONG CountX; ULONG CountY; ULONG CountCharsX; ULONG CountCharsY; ULONG FillAttribute; ULONG WindowFlags; ULONG ShowWindowFlags; UNICODE_STRING WindowTitle; UNICODE_STRING DesktopInfo; UNICODE_STRING ShellInfo; UNICODE_STRING RuntimeData; RTL_DRIVE_LETTER_CURDIR CurrentDirectores[32]; } RTL_USER_PROCESS_PARAMETERS, *PRTL_USER_PROCESS_PARAMETERS;         


        // Source: https://stackoverflow.com/questions/10702514/most-efficient-way-to-replace-one-sequence-of-the-bytes-with-some-other-sequence/10702934#10702934
        private static byte[] Replace(byte[] input, byte[] pattern, byte[] replacement)
        {
            if (pattern.Length == 0)
            {
                return input;
            }
            List<byte> result = new List<byte>();
            int i;
            for (i = 0; i <= input.Length - pattern.Length; i++)
            {
                bool foundMatch = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (input[i + j] != pattern[j])
                    {
                        foundMatch = false;
                        break;
                    }
                }
                if (foundMatch)
                {
                    result.AddRange(replacement);
                    i += pattern.Length - 1;
                }
                else
                {
                    result.Add(input[i]);
                }
            }
            for (; i < input.Length; i++)
            {
                result.Add(input[i]);
            }
            return result.ToArray();
        }


        unsafe static void GetEnv(int processparameters_offset, int environmentsize_offset, int environment_offset)
        {
            // Reference: Get environment variables using System.Environment
            // foreach (DictionaryEntry e in System.Environment.GetEnvironmentVariables()){ Console.WriteLine(e.Key + ":" + e.Value); }

            IntPtr hProcess = Process.GetCurrentProcess().Handle;
            PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
            uint temp = 0;
            NtQueryInformationProcess(hProcess, 0x0, ref pbi, (uint)(IntPtr.Size * 6), ref temp);
            IntPtr PebBaseAddress = (IntPtr)(pbi.PebBaseAddress);
            Console.WriteLine("[+] PEB base:                  \t0x{0}", PebBaseAddress.ToString("X"));

            IntPtr processparameters_pointer = (IntPtr)(pbi.PebBaseAddress + processparameters_offset);
            IntPtr processparameters = Marshal.ReadIntPtr(processparameters_pointer);
            // Console.WriteLine("[+] ProcessParameters Pointer: \t0x{0}", processparameters_pointer.ToString("X"));
            Console.WriteLine("[+] ProcessParameters Address: \t0x{0}", processparameters.ToString("X"));

            // Reference: https://www.geoffchappell.com/studies/windows/km/ntoskrnl/inc/api/pebteb/rtl_user_process_parameters.htm
            IntPtr environment_size_pointer = processparameters + environmentsize_offset;
            IntPtr environment_size = Marshal.ReadIntPtr(environment_size_pointer);
            // Console.WriteLine("[+] Environment Size Pointer:  \t0x{0}", environment_size_pointer.ToString("X"));
            Console.WriteLine("[+] Environment Size:          \t{0}", environment_size);

            IntPtr environment_pointer = processparameters + environment_offset;
            IntPtr environment_start = Marshal.ReadIntPtr(environment_pointer);
            // Console.WriteLine("[+] Environment Pointer:       \t0x{0}", environment_pointer.ToString("X"));
            Console.WriteLine("[+] Environment Address:       \t0x{0}", environment_start.ToString("X"));
            IntPtr environment_end = environment_start + (int)environment_size;

            // Console.WriteLine("\n[+] Reading {0} bytes from 0x{1} to 0x{2}...", environment_size, environment_start.ToString("X"), (environment_end).ToString("X"));
            Console.WriteLine("\n[+] Result:");
            byte[] data = new byte[(int)environment_size];
            NtReadVirtualMemory(hProcess, environment_start, data, data.Length, out _);
            byte[] result = Replace(data, new byte[] { 0, 0, 0 }, new byte[] { 0, 10, 0 });
            String environment_vars = Encoding.Unicode.GetString(result);
            Console.WriteLine(environment_vars);
        }


        static void Main(string[] args)
        {
            if (Environment.Is64BitProcess)
            {
                Console.WriteLine("[+] 64 bits process");
                GetEnv(0x20, 0x3F0, 0x80);
            }
            else
            {
                Console.WriteLine("[+] 32 bits process");
                GetEnv(0x10, 0x0290, 0x48);
            }
        }
    }
}
