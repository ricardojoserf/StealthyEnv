# StealthyEnv
Stealthier alternative to whoami.exe or other well-known binaries. It is written in C# and gets the environment variables from PEB.

- Function [NtQueryInformationProcess](https://learn.microsoft.com/en-us/windows/win32/api/winternl/nf-winternl-ntqueryinformationprocess) returns a "_PROCESS_BASIC_INFORMATION" structure containing a pointer to the PEB base address.

- The PEB structure contains a pointer "ProcessParameters" which is a [RTL_USER_PROCESS_PARAMETERS](https://www.geoffchappell.com/studies/windows/km/ntoskrnl/inc/api/pebteb/rtl_user_process_parameters.htm) structure.

- From that structure you can get a pointer "Environment" to the environment variables and a pointer "EnvironmentSize" to the size of the environment variables.

- Reading the number of bytes indicated in "EnvironmentSize" from the address "Environment" as UNICODE text, it is possible to get the environment variables values.

![esquema](https://raw.githubusercontent.com/ricardojoserf/ricardojoserf.github.io/master/images/stealthyenv/Screenshot_0.png)


### Examples

64 bit process:

![64 bits](https://raw.githubusercontent.com/ricardojoserf/ricardojoserf.github.io/master/images/stealthyenv/Screenshot_1.png)


32 bit process:

![32 bits](https://raw.githubusercontent.com/ricardojoserf/ricardojoserf.github.io/master/images/stealthyenv/Screenshot_2.png)
