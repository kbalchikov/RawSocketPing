This project demonstates realization of some common network command-line utilites written on C# using raw sockets.

1. Meleagre.Network. Basic library which includes realization of IP and ICMP protocols, and class for handling command-line arguments.
2. MPing - ping utility based on ICMP protocol. Sends echo-messages and waiting for replies. 
3. MTracert - traceroute utility, displays the path to specified host.
4. MTU discovery - utility that tries to determine MTU (maximum tranmission unit) to specifed host.

All utilites shows help, when they are typed to the command-line without any argument. 

NOTE! Working with raw sockets requires Administrator privileges, so all three utilites should be started 
from command-line, ran as Administrator.
