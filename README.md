# StealthyEnv
Alternative to whoami.exe or other well-known binaries to get the environment variables. It is written in C# and I guess it is stealthier because it gets the values parsing the PEB structure.

- Function [NtQueryInformationProcess](https://learn.microsoft.com/en-us/windows/win32/api/winternl/nf-winternl-ntqueryinformationprocess) returns a "PROCESS_BASIC_INFORMATION" structure containing a pointer to the PEB base address.

- The PEB structure contains a pointer "ProcessParameters" to a [RTL_USER_PROCESS_PARAMETERS](https://www.geoffchappell.com/studies/windows/km/ntoskrnl/inc/api/pebteb/rtl_user_process_parameters.htm) structure.

- From that structure you can get a pointer "Environment" to the environment variables and a pointer "EnvironmentSize" to the size of the environment variables.

- Reading the number of bytes indicated in "EnvironmentSize" from the address "Environment" as UNICODE text, you get the environment variables.

![esquema](https://raw.githubusercontent.com/ricardojoserf/ricardojoserf.github.io/master/images/stealthyenv/Screenshot_0.png)

### Examples

64 bit process:

![64 bits](https://raw.githubusercontent.com/ricardojoserf/ricardojoserf.github.io/master/images/stealthyenv/Screenshot_1.png)


32 bit process:

![32 bits](https://raw.githubusercontent.com/ricardojoserf/ricardojoserf.github.io/master/images/stealthyenv/Screenshot_2.png)
